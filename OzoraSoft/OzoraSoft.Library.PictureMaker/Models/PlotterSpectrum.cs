using OzoraSoft.Library.Enums.PictureMaker;
using OzoraSoft.Library.PictureMaker.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzoraSoft.Library.PictureMaker
{
    public class PlotterSpectrum : PlotBase, IPlotEngine
    {
        public PlotterSpectrum()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plotTemplate"></param>
        public  new void SetUpLayout(PlotTemplate plotTemplate, PlotItem plotItem)
        {
            switch (plotTemplate.PlotType)
            {
                case enmPlotType.linechart:
                    {
                        break;
                    }
                case enmPlotType.histogram1:
                    {
                        var arrX = (double[])plotItem.ArrayData!.PartOf(new SliceIndex?[] { new SliceIndex(0), null }!);
                        var arrY = (double[])plotItem.ArrayData!.PartOf(new SliceIndex?[] { new SliceIndex(1), null }!);

                        // Axis X  (auto-scaled if it's empty float[]):
                        int jumps = 5;
                        var (minX, maxX, baseX) = DataTransformation.AdjustLimits(arrX.Min(), arrX.Max(), jumps, true);
                        AxisValues[0] = new double[] { minX, maxX, baseX };

                        // Axis Y (auto-scaled if it's empty float[]):
                        var (minY, maxY, baseY) = DataTransformation.AdjustLimits(arrY.Min(), arrY.Max(), jumps, true);
                        AxisValues[1] = new double[] { minY, maxY, baseY };
                        break;
                    }
            }
            base.SetUpLayout(plotTemplate, plotItem);
        }
    }
}