using Newtonsoft.Json;
using OzoraSoft.Library.PictureMaker;
using OzoraSoft.Library.PictureMaker.Models;
using OzoraSoft.Library.Enums.PictureMaker;
using System;
using System.ComponentModel;

namespace OzoraSoft.Library.PictureMaker
{
    /// <summary>
    /// 
    /// </summary>
    public class DataSourceEngine
    {
        protected PictureTemplate pictureTemplate = new();
        protected DerivedData derivedData;
        protected const string templatesPath = "PictureTemplates.json";

        public DataSourceEngine(string name)
        {
            derivedData = new DerivedData() { Name = name };
            using (StreamReader r = new StreamReader(templatesPath))
            {
                string json = r.ReadToEnd();
                var templates = JsonConvert.DeserializeObject<PictureTemplate[]>(json);
                if (templates != null)
                {
                    pictureTemplate = templates!.Where(x => x.Name == derivedData.Name)!.FirstOrDefault()!;
                 }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testType"></param>
        /// <param name="pictureTemplate"></param>
        /// <returns></returns>
        private List<PlotItem> GetFakeDataSource(PictureTemplate pictureTemplate)
        {
            var result = new List<PlotItem>();
            switch (pictureTemplate.TestType)
            {
                case enmTestType.ncps:
                    {
                        var random = new Random();
                        // Preparing data sample:
                        for (int j = 0; j < pictureTemplate.PictureLayout[1]; j++)
                            for (int i = 0; i < pictureTemplate.PictureLayout[0]; i++)
                                result.Add(new PlotItem() { Name = "Combined NCPs", PlotType = enmPlotType.ncp, ArrayData = FakeData.GetNcpData(), IndexRef = [i, j] });

                        break;
                    }
                case enmTestType.heatmapDM:
                    {
                        var random = new Random();
                        // Preparing data sample:
                        for (int j = 0; j < pictureTemplate.PictureLayout[1]; j++)
                            for (int i = 0; i < pictureTemplate.PictureLayout[0]; i++)
                                result.Add(new PlotItem() { Name = "Heatmap DM", PlotType = enmPlotType.heatmap, ArrayData = FakeData.GetHeatMapData(), IndexRef = [i, j] });

                        break;
                    }
                case enmTestType.spectrum:
                    {
                        result.Add(new PlotItem() { Name = "Spectrum [Linechart]", PlotType = enmPlotType.linechart, ArrayData = FakeData.GetLineChartData(), IndexRef = [0, 0] });
                        result.Add(new PlotItem() { Name = "Spectrum [Histogram]", PlotType = enmPlotType.histogram1, ArrayData = FakeData.GetHistogramData(), IndexRef = [1, 0] });
                        break;
                    }
                case enmTestType.energy:
                case enmTestType.electrical:
                    {
                        var arrData = FakeData.GetHeatMapData();
                        result.Add(new PlotItem() { Name = "Electrical [Heatmap]", PlotType = enmPlotType.heatmap, ArrayData = arrData, IndexRef = [0, 0] });
                        result.Add(new PlotItem() { Name = "Electrical [Histogram]", PlotType = enmPlotType.histogram2, ArrayData = arrData, IndexRef = [1, 0] });
                        break;
                    }
                case enmTestType.energy_cal:
                    {
                        result.Add(new PlotItem() { Name = "XRAY-RAW-K-EDGE_1000_ENERGY_CALIBRATION [spec_mean]", PlotType = enmPlotType.curvechart, ArrayData = FakeData.GetEnergyCal_XrayRaw(), IndexRef = [0, 0] });
                        result.Add(new PlotItem() { Name = "XRAY-PB-K-EDGE_1000_ENERGY_CALIBRATION [spec_mean]", PlotType = enmPlotType.curvechart, ArrayData = FakeData.GetEnergyCal_XrayPB(), IndexRef = [1, 0] });
                        result.Add(new PlotItem() { Name = "XRAY-CEO2-K-EDGE_1000_ENERGY_CALIBRATION [spec_mean]", PlotType = enmPlotType.curvechart, ArrayData = FakeData.GetEnergyCal_XrayCEO2(), IndexRef = [2, 0] });
                        break;
                    }
                case enmTestType.stability:
                    {
                        var arrData1 = (Array)FakeData.GetStability_DNumber();
                        var arrData2 = (Array)FakeData.GetStability_DNumber_Ncp();
                        var data1 = (double[,])arrData1.PartOf(new SliceIndex?[] { new SliceIndex(0), null, null }!);
                        var data2 = (double[,])arrData2.PartOf(new SliceIndex?[] { new SliceIndex(0), null, null }!);

                        result.Add(new PlotItem() { Name = "D-Number [Heatmap]", PlotType = enmPlotType.heatmap_stability, ArrayData = data1, IndexRef = [0, 2] });
                        result.Add(new PlotItem() { Name = "D-Number [Histogram]", PlotType = enmPlotType.histogram_stability, ArrayData = data1, IndexRef = [0, 1] });
                        result.Add(new PlotItem() { Name = "D-Number [NCP]", PlotType = enmPlotType.ncp, ArrayData = data2, IndexRef = [0, 0] });
                        break;
                    }
            }
            return result;
        }

        public DerivedData GetDerivedData()
        {
            if (pictureTemplate != null)
                derivedData.PlotItems = GetFakeDataSource(pictureTemplate);
            return derivedData; 
        }

        public PictureTemplate GetPictureTemplate() { return pictureTemplate; }

        public  Dictionary<enmDerivedData, Array> GetDerivedDataFromFile(enmTestType testType, Array arrData)
        {
            var result = new Dictionary<enmDerivedData, Array>();
            try
            {
                if (arrData != null)
                {
                    switch (testType)
                    {
                        case enmTestType.stability:
                            {
                                Console.WriteLine($"StabilityTest -> Starting ProcessSource()...");
                                if (arrData.GetLength(0) == 13)
                                    arrData = (Array)arrData.PartOf(new SliceIndex?[] { new SliceIndex(6, 12), null, null, null }!);

                                var numBins = arrData.GetLength(0);
                                var numViews = arrData.GetLength(1);
                                //var areaCorrection = Container.ModuleConfig.AreaCorrect();
                                
                                var sample_period = (int)(Constants.SAMPLE_PERIOD * 1000);
                                int num_repeats = 1, window_offset = 20;

                                if (Constants.REQUIRED_SAMPLE_PERIOD % sample_period != 0)
                                    throw new Exception($"Sampling period is not an integer factor of {Constants.REQUIRED_SAMPLE_PERIOD} ms");

                                Console.WriteLine($"StabilityTest -> ProcessSource() -> Getting 'cc_data_resampled' ...");
                                var resampling_factor = Constants.REQUIRED_SAMPLE_PERIOD / sample_period;
                                var temp = arrData.MoveAxis(0, 1);
                                var cc_data_resampled = DataTransformation.AccumulateFrames(temp, resampling_factor);
                                cc_data_resampled = cc_data_resampled.MoveAxis(0, 1);

                                var sumcc_resampled = (Array)((Array)cc_data_resampled.PartOf(new SliceIndex?[] { new SliceIndex(1, null), null, null, null }!)).NanSum(0);
                                var views_per_repeat = (int)(numViews / num_repeats / resampling_factor);
                                var ncp_threshold = Constants.D_NUMBER_NCP_THRESHOLDS;

                                Console.WriteLine($"StabilityTest -> ProcessSource() -> Getting 'dnumber' values ...");
                                var (d_number, d_number_ncps) = DataTransformation.CalculateDNumberValues(sumcc_resampled, ncp_threshold, num_repeats, views_per_repeat, window_offset);
                                result.Add(enmDerivedData.d_number, d_number);
                                result.Add(enmDerivedData.d_number_ncps, d_number_ncps);

                                Console.WriteLine($"StabilityTest -> ProcessSource() -> Getting 'raw_sum' values ...");
                                var raw_sum = (Array)arrData.NanSum(new int[] { 2, 3 });
                                var raw_sum_resampled = (Array)cc_data_resampled.NanSum(new int[] { 2, 3 });
                                result.Add(enmDerivedData.raw_sum, raw_sum);
                                result.Add(enmDerivedData.raw_sum_resampled, raw_sum_resampled);

                                Console.WriteLine($"StabilityTest -> ProcessSource() -> Getting 'norm' values ...");
                                var (norm, norm_mask, cc_norm, norm_sum) = DataTransformation.CalculateNormalizationValues(arrData);
                                result.Add(enmDerivedData.norm, norm);
                                result.Add(enmDerivedData.norm_mask, norm_mask);
                                result.Add(enmDerivedData.norm_sum, norm_sum);

                                Console.WriteLine($"StabilityTest -> ProcessSource() -> Getting 'md1x' values ...");
                                var (md1f, md1s, md1f_ncps, md1s_ncps) = DataTransformation.CalculateMD1XValues(cc_norm);
                                result.Add(enmDerivedData.md1f, md1f);
                                result.Add(enmDerivedData.md1s, md1s);
                                result.Add(enmDerivedData.md1f_ncps, md1f_ncps);
                                result.Add(enmDerivedData.md1s_ncps, md1s_ncps);

                                Console.WriteLine($"StabilityTest -> Plot Dictionary ready. End of Process.");
                                break;
                            }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Message: {ex.Message}");
            }
            return result;
        }
    }
}
