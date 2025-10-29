using OzoraSoft.Library.Enums;
using OzoraSoft.Library.PictureMaker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace OzoraSoft.Library.PictureMaker
{
    public class Orchestration
    {
        public Orchestration()
        {
        }

        public ActionResponse getSourceData(DerivedDataFilter filter)
        {
            var result = new ActionResponse();
            try
            {
                var engine = new DataSourceEngine(filter.Name);
                var derivedData = engine.GetDerivedData();
                result.Message = "OK";
                result.Content = UtilsForMessages.Compress(UtilsForMessages.SerializeObject(derivedData));
            }
            catch (Exception ex)
            {
                result.Type = "Error";
                result.Message = ex.Message;
            }
            result.EndDate = DateTime.Now;
            return result;
        }

        public ActionResponse processData(DerivedDataFilter filter)
        {
            var result = new ActionResponse();
            var record = new RunImage();
            try
            {
                if (!string.IsNullOrEmpty(filter.CompressedData))
                {
                    var derivedData = UtilsForMessages.DeserializeObject<DerivedData>(UtilsForMessages.Decompress(filter.CompressedData))!;
                    var dataEngine = new DataSourceEngine(derivedData.Name);
                    var pictureTemplate = dataEngine.GetPictureTemplate();
                    IPlotEngine? plotEngine = pictureTemplate.TestType switch
                    {
                        enmTestType.heatmapDM => new PlotterNCP(),
                        enmTestType.ncps => new PlotterNCP(),
                        enmTestType.spectrum => new PlotterSpectrum(),
                        enmTestType.energy => new PlotterEnergy(),
                        enmTestType.energy_cal => new PlotterEnergyCal(),
                        enmTestType.uniformity => new PlotterUniformity(),
                        enmTestType.stability => new PlotterStability(),
                        _ => null
                    };
                    if (plotEngine != null)
                    {
                        var pictureEngine = new PictureEngine(pictureTemplate, derivedData, plotEngine!);
                        var plotImage = pictureEngine.MakePicture()!;
                        var metadata = new RunMetadata(derivedData.Name, pictureTemplate);
                        //record.LoadData(derivedData.Name, metadata, derivedData, plotImage);
                        //record.EndProcess = DateTime.Now;
                        result.Message = "OK";
                        result.Content = plotImage; // UtilsForMessages.Compress(UtilsForMessages.SerializeObject(record));
                    }
                    else
                        throw new Exception("PlotEngine invalid");
                }
                else
                    throw new Exception("DataSource invalid");
            }
            catch (Exception ex)
            {
                result.Type = "Error";
                result.Message = ex.Message;
            }
            result.EndDate = DateTime.Now;
            return result;
        }

        public string getImageString(string content)
        {
            return $"data:image/png;base64, {content}";
        }
    }
}
