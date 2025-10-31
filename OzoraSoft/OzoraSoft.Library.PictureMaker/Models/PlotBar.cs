using OzoraSoft.Library.Enums.PictureMaker;
using SkiaSharp;

namespace OzoraSoft.Library.PictureMaker.Models
{
    public class PlotBar
    {
        public enmTextOrientation Orientation { get; set;  }
        /// <summary>
        /// 
        /// </summary>
        public int[] Spacing { get; set; } = [];
        /// <summary>
        /// 
        /// </summary>
        public int[] Size { get; set; } = [];
        /// <summary>
        /// 
        /// </summary>
        public float[] ColorPositions { get; set; } = [];
        /// <summary>
        /// 
        /// </summary>
        public SKColor[] Colors { get; set; } = [];
        /// <summary>
        /// 
        /// </summary>
        public string ColorMap { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public double[] Labels { get; set; } = [];
        /// <summary>
        /// 
        /// </summary>
        public bool IsLabelVisible { get; set; } = true;
        /// <summary>
        /// 
        /// </summary>
        public float[] Offset { get; set; } = [];
        /// <summary>
        /// 
        /// </summary>
        public bool[] Edges { get; set; } = [true, true];
    }
}
