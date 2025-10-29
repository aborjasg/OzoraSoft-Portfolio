using OzoraSoft.Library.Enums.PictureMaker;

namespace OzoraSoft.Library.PictureMaker.Models
{
    public class PlotAxis
    {
        /// <summary>
        /// 
        /// </summary>
        public string Label { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public double[] Range { get; set; } = []; // 0: Start, 1:End, 2: Jump
        /// <summary>
        /// 
        /// </summary>
        public float[] Offset { get; set; } = [];
        /// <summary>
        /// 
        /// </summary>
        public double Scale { get; set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        public enmTextOrientation? Orientation { get; set; }
    }
}
