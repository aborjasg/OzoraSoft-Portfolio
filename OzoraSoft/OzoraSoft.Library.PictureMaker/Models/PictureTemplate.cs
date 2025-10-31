using OzoraSoft.Library.Enums.PictureMaker;

namespace OzoraSoft.Library.PictureMaker.Models
{
    public class PictureTemplate
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        public enmTestType TestType;
        /// <summary>
        /// 
        /// </summary>
        public int[] PictureDimensions { get; set; } = new int[2]; // 0:X, 1:Y
        /// <summary>
        /// 
        /// </summary>
        public int[] PictureLayout { get; set; } = new int[2]; // 0:X, 1:Y
        /// <summary>
        /// 
        /// </summary>
        public bool PicturePreviewFlag { get; set; } = false;
        /// <summary>
        /// 
        /// </summary>
        public string PicturePreviewPath { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public int[] StartPoint { get; set; } = new int[2]; // 0:X, 1:Y
        /// <summary>
        /// 
        /// </summary>
        public int[] PlotSpacing { get; set; } = new int[2]; // 0:X, 1:Y  
        /// <summary>
        /// 
        /// </summary>        
        public bool Active { get; set; } = true;
        /// <summary>
        /// 
        /// </summary>
        public PlotTemplate[] PlotTemplates { get; set; } = Array.Empty<PlotTemplate>();

        /// <summary>
        /// 
        /// </summary>
        public PictureTemplate()
        {

        }

    }
}
