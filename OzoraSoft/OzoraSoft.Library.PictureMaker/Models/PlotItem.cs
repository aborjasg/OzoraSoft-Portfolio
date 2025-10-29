using OzoraSoft.Library.Enums.PictureMaker;

namespace OzoraSoft.Library.PictureMaker.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class PlotItem
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public enmPlotType PlotType { get; set; }
        public double[,]? ArrayData { get; set; }
        public int[] PointRef { get; set; } = new int[2];
        public int[] IndexRef { get; set; } = new int[2];
    }
}
