using OzoraSoft.Library;
using OzoraSoft.Library.PictureMaker.Models;
using SkiaSharp;
using System;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OzoraSoft.Library.PictureMaker
{
    public class PictureEngine : IPictureEngine
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        protected Guid Guid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        protected PictureTemplate pictureTemplate;
        /// <summary>
        /// 
        /// </summary>
        protected DerivedData derivedData;
        /// <summary>
        /// 
        /// </summary>
        protected IPlotEngine plotEngine;
        /// <summary>
        /// 
        /// </summary>
        protected string[]? pictureLegend;

        #endregion

        #region Plot Objects

        /// <summary>
        /// 
        /// </summary>
        protected SKImageInfo ImageInfo;
        /// <summary>
        /// 
        /// </summary>
        protected SKSize PictureSize;
        /// <summary>
        /// 
        /// </summary>
        public SKSurface Surface;

        #endregion

        public PictureEngine(PictureTemplate pictureTemplate, DerivedData derivedData, IPlotEngine plotEngine)
        {
            Guid = Guid.NewGuid();
            if (pictureTemplate != null)
            {
                this.pictureTemplate = pictureTemplate;
                ImageInfo = new SKImageInfo(pictureTemplate.PictureDimensions[0], pictureTemplate.PictureDimensions[1]);
                Surface = SKSurface.Create(ImageInfo);
                Surface.Canvas.Clear(SKColors.White);
                this.derivedData = derivedData;
                this.plotEngine = plotEngine;
            }
            else
                throw new Exception("PictureTemplate invalid,");
        }

        #region Protected/Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plotName"></param>
        /// <param name="plotType"></param>
        /// <param name="data"></param>
        /// <param name="point"></param>
        protected void DrawPlot(PlotItem plotItem)
        {
            PlotTemplate? plotTemplate = null;
            var pointRef = new SKPoint(plotItem.PointRef[0], ImageInfo.Height - plotItem.PointRef[1]);

            plotTemplate = pictureTemplate.PlotTemplates.Where(x => x.PlotType == plotItem.PlotType && x.Active==true)!.FirstOrDefault();
            if (plotTemplate != null)
            {
                // Calculate Point Ref:
                if (plotItem.IndexRef != null && pictureTemplate.PictureLayout[0] > 0 && pictureTemplate.PictureLayout[1] > 0)
                {
                    plotItem.PointRef = new int[] { pictureTemplate.StartPoint[0] + (pictureTemplate.PlotSpacing[0] + pictureTemplate.PictureDimensions[0] / pictureTemplate.PictureLayout[0]) * plotItem.IndexRef[0], pictureTemplate.StartPoint[1] + (pictureTemplate.PlotSpacing[1] + pictureTemplate.PictureDimensions[1] / pictureTemplate.PictureLayout[1]) * plotItem.IndexRef[1] };
                    pointRef = new SKPoint(plotItem.PointRef[0], ImageInfo.Height - plotItem.PointRef[1]);
                }
                // Preparing plot layout:
                plotEngine.SetUpLayout(plotTemplate, plotItem);
                // Draw array data:
                plotEngine.DrawData(plotTemplate, pointRef, Surface, plotItem);
                // Set plot title:
                plotEngine.DrawPlotTitle(plotTemplate, pointRef, Surface, plotItem, plotItem.IndexRef!.Length > 0 ? $" [{plotItem.IndexRef[0]}/{plotItem.IndexRef[1]}]" : string.Empty);

            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected void DrawPlots()
        {
            foreach (var item in derivedData.PlotItems!)
                DrawPlot(item);
            //Parallel.ForEach(derivedData.PlotItems!, item =>
            //    DrawPlot(item));
        }
        /// <summary>
        /// 
        /// </summary>
        protected byte[] GeneratePicture()
        {
            DrawPlots();
            //get png from SKSurface
            SKImage img = Surface.Snapshot();
            SKData imgData = img.Encode(SKEncodedImageFormat.Png, 100);
            var image = imgData.ToArray();

            //get binary data
            return image;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        public void DrawPictureTitle()
        {
            var textFont = new SKFont {Size = 11.0f};
            Surface.Canvas.DrawText(pictureTemplate.Name, pictureTemplate.PictureDimensions[0] / 2f, pictureTemplate.StartPoint[1] - 20, SKTextAlign.Center, textFont, Constants.PaintTitle);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string MakePicture()
        {
            //get png from SKSurface            
            var image = GeneratePicture();
            // Download image:            
            if (pictureTemplate.PicturePreviewFlag)
            {
                string filePath = $"{pictureTemplate.PicturePreviewPath}{pictureTemplate.TestType}_{Guid}.png";
                File.WriteAllBytes(filePath, image);
            }
            return "data:image/png;base64, " + Convert.ToBase64String(image);
        }

        #endregion
    }
}
