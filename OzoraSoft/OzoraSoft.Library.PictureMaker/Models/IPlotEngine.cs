using OzoraSoft.Library.PictureMaker.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzoraSoft.Library.PictureMaker
{
    public interface IPlotEngine
    {
        void SetUpLayout(PlotTemplate plotTemplate, PlotItem plotItem);
        void DrawPlotTitle(PlotTemplate plotTemplate, SKPoint point, SKSurface surface, PlotItem plotItem, string addToTitle = "");        
        void DrawData(PlotTemplate plotTemplate, SKPoint point, SKSurface surface, PlotItem plotItem);
    }
}
