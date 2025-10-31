using MathNet.Numerics.Statistics;
using OzoraSoft.Library.PictureMaker.Models;
using OzoraSoft.Library.Enums.PictureMaker;
using SkiaSharp;

namespace OzoraSoft.Library.PictureMaker
{
    public class PlotBase : IPlotEngine
    {
        /// <summary>
        /// 
        /// </summary>
        public SKFont TextFont;        

        /// <summary>
        /// 
        /// </summary>
        protected double[][] AxisValues { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public PlotBase()
        {
            AxisValues = new double[][] { Array.Empty<double>(), Array.Empty<double>() };
            TextFont = new SKFont {Size = 11.0f};
        }

        #region Protected/Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plotTemplate"></param>
        /// <returns></returns>
        protected (float, float) GetPoint0(PlotTemplate plotTemplate)
        {
            float x = (float)(Math.Abs(plotTemplate.Axis[0].Range[0]) * plotTemplate.Axis[0].Scale);
            float y = plotTemplate.FrameSize[1] - (float)(Math.Abs(plotTemplate.Axis[1].Range[0]) * plotTemplate.Axis[1].Scale);

            return (x, y);
        }
        /// <summary>
        /// 
        /// </summary>
        protected void SetNoData(PlotTemplate plotTemplate, SKPoint point, SKSurface surface)
        {            
            surface.Canvas.DrawText("N/A", new SKPoint(point.X + plotTemplate.FrameSize[0] / 2 - 20, point.Y - plotTemplate.FrameSize[1] / 2), this.TextFont, Constants.PaintText);
        }        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plotTemplate"></param>
        /// <param name="plotItem"></param>
        protected void SetScaleLayout(PlotTemplate plotTemplate, PlotItem plotItem)
        { 
            // Set Frame Size:
            if (plotTemplate.FrameSize.Length > 0)
            {
                double rangeX = 0, rangeY = 0, width = 0, height = 0;

                if (AxisValues![0].Length > 0)
                {
                    rangeX = AxisValues[0][1] - AxisValues[0][0];
                    width = plotTemplate.FrameSize[0] - plotTemplate.Axis[0].Offset[0] - plotTemplate.Axis[0].Offset[1];

                    if (rangeX != 0)
                        plotTemplate.Axis[0].Scale = width / rangeX;
                    else
                        plotTemplate.Axis[0].Scale = 1;
                }

                if (AxisValues![1].Length > 0)
                {
                    rangeY = AxisValues[1][1] - AxisValues[1][0];
                    height = plotTemplate.FrameSize[1] - plotTemplate.Axis[1].Offset[0] - plotTemplate.Axis[1].Offset[1];

                    if (rangeY != 0)
                        plotTemplate.Axis[1].Scale = height / rangeY;
                    else
                        plotTemplate.Axis[1].Scale = 1;
                }

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plotTemplate"></param>
        /// <param name="point"></param>
        /// <param name="surface"></param>
        protected void DrawLayout(PlotTemplate plotTemplate, SKPoint point, SKSurface surface)
        {
            var paintSquare = Constants.PaintSquare;

            switch (plotTemplate.AreaLayout)
            {
                case enmAreaLayout.Plain:
                    {
                        float x0 = (float)point.X, x1 = point.X + plotTemplate.FrameSize[0], y0 = point.Y, y1 = point.Y - plotTemplate.FrameSize[1];

                        surface.Canvas.DrawLine(new SKPoint(x0, y0), new SKPoint(x0, y1), paintSquare);
                        surface.Canvas.DrawLine(new SKPoint(x0, y0), new SKPoint(x1, y0), paintSquare);
                        surface.Canvas.DrawLine(new SKPoint(x0, y1), new SKPoint(x1, y1), paintSquare);
                        surface.Canvas.DrawLine(new SKPoint(x1, y0), new SKPoint(x1, y1), paintSquare);

                        break;
                    }
                case enmAreaLayout.Squad:
                    {
                        float x0 = (float)point.X, x1 = point.X + plotTemplate.FrameSize[0], y0 = point.Y - plotTemplate.FrameSize[1], y1 = point.Y;
                        float space = plotTemplate.StrokeWidth;

                        // Dotted-line matrix:
                        for (float k = x0; k <= x1; k += space)
                        {
                            surface.Canvas.DrawLine(new SKPoint(k, y0), new SKPoint(k, y1), paintSquare);
                        }
                        for (float k = y0; k <= y1; k += space)
                        {
                            surface.Canvas.DrawLine(new SKPoint(x0, k), new SKPoint(x1, k), paintSquare);
                        }

                        paintSquare.PathEffect = null;

                        // bold-line matrix:
                        for (float k = x0; k <= x1; k += space * 3)
                        {
                            surface.Canvas.DrawLine(new SKPoint(k, y0), new SKPoint(k, y1), paintSquare);
                        }
                        for (float k = y0; k <= y1; k += space * 3)
                        {
                            surface.Canvas.DrawLine(new SKPoint(x0, k), new SKPoint(x1, k), paintSquare);
                        }
                        break;
                    }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plotTemplate"></param>
        /// <param name="point"></param>
        /// <param name="surface"></param>
        protected void DrawAxis(PlotTemplate plotTemplate, SKPoint point, SKSurface surface)
        {
            SKPaint paint = Constants.PaintTextSmall.Clone();

            if (AxisValues != null && plotTemplate.IsAxisVisible)
            {
                switch (plotTemplate.PlotType)
                {
                    case enmPlotType.histogram_stability:
                        {                            
                            string numDecimals = "0";
                            if (AxisValues[0][2] < 1) numDecimals = AxisValues[0][2] <= 0.25 ? "0.00" : "0.0";

                            // X Axis:

                            for (double k = AxisValues[0][0]; k <= AxisValues[0][1]; k += AxisValues[0][2])
                            {
                                var (x, y) = ((float)(point.X + plotTemplate.Axis[0].Offset[0] + (k - AxisValues[0][0]) * plotTemplate.Axis[0].Scale), (float)(point.Y));
                                var pointRef = new SKPoint(x, y);

                                surface.Canvas.DrawLine(pointRef, new SKPoint(pointRef.X, pointRef.Y + 5), Constants.PaintBorder);
                                surface.Canvas.DrawText(k.ToString(numDecimals), new SKPoint(pointRef.X, pointRef.Y + 17), SKTextAlign.Center, TextFont, paint);
                            }

                            // Y Axis:                            

                            for (int k = 0; k < Constants.Ticks.Length; k++)
                            {
                                var (x, y) = (point.X, (float)(point.Y - plotTemplate.Axis[1].Offset[0] - (k - AxisValues[1][0]) * plotTemplate.Axis[1].Scale));
                                var pointRef = new SKPoint(x, y);

                                surface.Canvas.DrawLine(pointRef, new SKPoint(pointRef.X - 5, pointRef.Y), Constants.PaintBorder);
                                surface.Canvas.DrawText(Constants.Ticks[k].ToString(), new SKPoint(pointRef.X - 7, pointRef.Y + 3), SKTextAlign.Right, TextFont, paint);
                            }

                            break;
                        }
                    default:
                        {
                            string numDecimals = "0";
                            if (AxisValues[0][2] < 1) numDecimals = AxisValues[0][2] <= 0.25 ? "0.00" : "0.0";

                            // X Axis:

                            for (double k = AxisValues[0][0]; k <= AxisValues[0][1]; k += AxisValues[0][2])
                            {
                                var (x, y) = ((float)(point.X + plotTemplate.Axis[0].Offset[0] + (k - AxisValues[0][0]) * plotTemplate.Axis[0].Scale), point.Y);
                                var pointRef = new SKPoint(x, y);

                                surface.Canvas.DrawLine(pointRef, new SKPoint(pointRef.X, pointRef.Y + 5), Constants.PaintBorder);
                                surface.Canvas.DrawText(k.ToString(numDecimals), new SKPoint(pointRef.X, pointRef.Y + 17), SKTextAlign.Center, TextFont, paint);
                            }

                            // Y Axis:                            

                            for (double k = AxisValues[1][0]; k <= AxisValues[1][1]; k += AxisValues[1][2])
                            {
                                var (x, y) = (point.X, (float)(point.Y - plotTemplate.Axis[1].Offset[0] - (k - AxisValues[1][0]) * plotTemplate.Axis[1].Scale));
                                var pointRef = new SKPoint(x, y);

                                surface.Canvas.DrawLine(pointRef, new SKPoint(pointRef.X - 5, pointRef.Y), Constants.PaintBorder);
                                surface.Canvas.DrawText(k.ToString(), new SKPoint(pointRef.X - 7, pointRef.Y + 3), SKTextAlign.Right, TextFont, paint);
                            }
                            break;
                        }
                }                
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plotTemplate"></param>
        /// <param name="surface"></param>
        /// <param name="edges"></param>
        protected void DrawBar(PlotTemplate plotTemplate, SKPoint point, SKSurface surface)
        {
            if (plotTemplate.Bar != null && AxisValues != null && plotTemplate.IsBarVisible)
            {
                int barX = plotTemplate.Bar.Orientation switch
                {
                    enmTextOrientation.Vertical => (int)(point.X + plotTemplate.FrameSize[0] + plotTemplate.Bar.Spacing[0]),
                    enmTextOrientation.Horizontal => (int)(point.X),
                    _ => 0
                };
                int barY = plotTemplate.Bar.Orientation switch
                {
                    enmTextOrientation.Vertical => (int)(point.Y - plotTemplate.FrameSize[1]),
                    enmTextOrientation.Horizontal => (int)(point.Y + plotTemplate.FrameSize[1] + plotTemplate.Bar.Spacing[1]),
                    _ => 0
                };

                var pointRef = new SKPoint(barX, barY);
                var data = new uint[plotTemplate.Bar.Size[0], plotTemplate.Bar.Size[1]];

                // right side gradient bar
                var rcBar = new SKRect
                {
                    Left = pointRef.X,
                    Top = pointRef.Y,
                    Right = pointRef.X + data.GetLength(0),
                    Bottom = pointRef.Y + data.GetLength(1)
                };

                //Canvas.DrawBitmap(bmp, point);
                var paintBar = new SKPaint
                {
                    Color = SKColors.Black,
                    IsAntialias = false,
                    StrokeCap = SKStrokeCap.Butt,
                    StrokeJoin = SKStrokeJoin.Miter,
                    StrokeWidth = 1,
                    Shader = SKShader.CreateLinearGradient(
                            new SKPoint(rcBar.Left, rcBar.Bottom),
                            new SKPoint(rcBar.Left, rcBar.Top),
                            plotTemplate.Bar.Colors,
                            plotTemplate.Bar.ColorPositions,
                            SKShaderTileMode.Repeat),
                    Style = SKPaintStyle.Fill
                };

                var paintBorder = new SKPaint
                {
                    //TextSize = 11f,
                    IsAntialias = false,
                    Color = SKColors.Black,
                    IsStroke = false,
                    StrokeWidth = 1f,
                    Style = SKPaintStyle.Stroke,
                    //TextAlign = SKTextAlign.Right
                };

                surface.Canvas.DrawRect(rcBar, paintBar);
                surface.Canvas.DrawRect(rcBar, paintBorder);

                // Addind 2 triangles:
                if (plotTemplate.Bar.Edges.Length > 0)
                {
                    var paintTriangle = new SKPaint
                    {
                        StrokeCap = SKStrokeCap.Butt,
                        StrokeJoin = SKStrokeJoin.Miter,
                        Style = SKPaintStyle.Fill
                    };

                    if (plotTemplate.Bar.Edges[0])
                    {
                        var path1 = new SKPath();
                        path1.MoveTo(rcBar.Left, rcBar.Top);
                        path1.LineTo((rcBar.Right + rcBar.Left) / 2, rcBar.Top - rcBar.Width);
                        path1.LineTo(rcBar.Right, rcBar.Top);
                        path1.Close();

                        paintTriangle.Color = SKColors.Red;
                        surface.Canvas.DrawPath(path1, paintTriangle);
                        surface.Canvas.DrawPath(path1, paintBorder);
                    }

                    if (plotTemplate.Bar.Edges[1])
                    {
                        var path2 = new SKPath();
                        path2.MoveTo(rcBar.Left, rcBar.Bottom);
                        path2.LineTo((rcBar.Right + rcBar.Left) / 2, rcBar.Bottom + rcBar.Width);
                        path2.LineTo(rcBar.Right, rcBar.Bottom);
                        path2.Close();

                        paintTriangle.Color = SKColors.Blue;
                        surface.Canvas.DrawPath(path2, paintTriangle);
                        surface.Canvas.DrawPath(path2, paintBorder);
                    }

                    if (plotTemplate.Bar.IsLabelVisible)
                    {
                        //label possition and label
                        string numDecimals = "0";
                        if (plotTemplate.Bar.Labels[2] < 1) numDecimals = plotTemplate.Bar.Labels[2] <= 0.25 ? "0.00" : "0.0";

                        paintBorder.Style = SKPaintStyle.StrokeAndFill;
                        var labels = new Dictionary<string, SKPoint>();
                        double jump = (plotTemplate.Bar.Size[1] - plotTemplate.Bar.Offset[0] - plotTemplate.Bar.Offset[1]) / (plotTemplate.Bar.Labels[1] - plotTemplate.Bar.Labels[0]);

                        for (double k = plotTemplate.Bar.Labels[0]; Math.Round(k, 4) <= plotTemplate.Bar.Labels[1]; k += Math.Round(plotTemplate.Bar.Labels[2], 4))
                        {
                            labels.Add(k.ToString(numDecimals), new SKPoint(rcBar.Right, (float)(rcBar.Bottom - plotTemplate.Bar.Offset[0] - (k - plotTemplate.Bar.Labels[0]) * jump)));
                        }

                        var rcMaxTextBound = new SKRect();
                        var font = TextFont;
                        font.MeasureText("-5",  paintBorder); // paintBorder.MeasureText("-5", ref rcMaxTextBound);
                        foreach (var item in labels)
                        {
                            surface.Canvas.DrawLine(item.Value, new SKPoint(item.Value.X + 5, item.Value.Y), paintBorder);
                            surface.Canvas.DrawText(item.Key, new SKPoint(item.Value.X + rcMaxTextBound.Width, item.Value.Y + rcMaxTextBound.Height / 2f - 2), TextFont, Constants.PaintAxis);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plotTemplate"></param>
        /// <param name="point"></param>
        /// <param name="surface"></param>
        /// <param name="plotItem"></param>
        /// <exception cref="NotImplementedException"></exception>
        protected void DrawFigures(PlotTemplate plotTemplate, SKPoint point, SKSurface surface, PlotItem plotItem)
        {
            if (plotItem.ArrayData != null)
            {
                if (plotTemplate.FrameSize.Length == 2)
                {
                    // Preparing plot:
                    SKBitmap bitmap = new SKBitmap((int)plotTemplate.FrameSize[0], (int)plotTemplate.FrameSize[1]);
                    using var canvas = new SKCanvas(bitmap);
                    const int seccionsForY = 6;

                    switch (plotTemplate.PlotType)
                    {
                        case enmPlotType.ncp:
                            {
                                var (x, y) = GetPoint0(plotTemplate);
                                SKPoint pointRef = new SKPoint(x, y);
                                var color = SKColors.Black;
                                var arrayData = (double[,])plotItem.ArrayData!;
                                var paintPoint = Constants.PaintPoint;
                                paintPoint.StrokeWidth = plotTemplate.StrokeWidth;
                                plotItem.Title = $"NCP count={arrayData.NanSum()}";

                                for (int row = 0; row < arrayData.GetLength(0); row++)
                                {
                                    for (int col = 0; col < arrayData.GetLength(1); col++)
                                    {
                                        if (arrayData[row, col] > 0)
                                        {
                                            var (px, py) = (col * plotTemplate.StrokeWidth + plotTemplate.StrokeWidth / 2, row * plotTemplate.StrokeWidth + plotTemplate.StrokeWidth / 2);
                                            paintPoint.Color = color;
                                            canvas.DrawPoint(new SKPoint(pointRef.X + px, pointRef.Y - py), paintPoint);
                                        }
                                    }
                                }
                                break;
                            }
                        case enmPlotType.heatmap:
                            {
                                var (x, y) = GetPoint0(plotTemplate);
                                SKPoint pointRef = new SKPoint(x, y);
                                var color = SKColors.Black;
                                var arrayData = (double[,])plotItem.ArrayData!;
                                var paintPoint = Constants.PaintPoint;
                                paintPoint.StrokeWidth = plotTemplate.StrokeWidth;

                                if (plotTemplate.Bar != null)
                                {
                                    switch (plotTemplate.Bar.ColorMap)
                                    {
                                        case "viridis":
                                            {
                                                (plotTemplate.Bar.Colors, plotTemplate.Bar.ColorPositions) = ColorMap.Viridis;
                                                break;
                                            }
                                    }

                                    for (int row = 0; row < arrayData.GetLength(0); row++)
                                    {
                                        for (int col = 0; col < arrayData.GetLength(1); col++)
                                        {
                                            double factor = 1 - (plotTemplate.Bar.Labels[1] - arrayData[row, col]) / (plotTemplate.Bar.Labels[1] - plotTemplate.Bar.Labels[0]);

                                            if (factor <= 0)
                                                color = SKColors.Blue;
                                            else if (factor >= 1)
                                                color = SKColors.Red;
                                            else
                                                color = ColorMap.GetColor((plotTemplate.Bar.Colors, plotTemplate.Bar.ColorPositions), (float)factor);

                                            var (px, py) = (col * plotTemplate.StrokeWidth + plotTemplate.StrokeWidth / 2, plotTemplate.FrameSize[1] - (row * plotTemplate.StrokeWidth + plotTemplate.StrokeWidth / 2));
                                            canvas.DrawPoint(new SKPoint(pointRef.X + px, pointRef.Y - py), new SKPaint { Color = color, Style = SKPaintStyle.Fill, StrokeWidth = plotTemplate.StrokeWidth });
                                        }
                                    }
                                }
                                break;
                            }
                        case enmPlotType.heatmap_stability:
                            {
                                var (x, y) = GetPoint0(plotTemplate);
                                SKPoint pointRef = new SKPoint(x, y);
                                var color = SKColors.Black;
                                var arrayData = (double[,])plotItem.ArrayData!;
                                var paintPoint = new SKPaint { IsAntialias = false, Style = SKPaintStyle.Fill, StrokeWidth = plotTemplate.StrokeWidth };
                                plotItem.Title = $"D{plotItem.IndexRef[0] + 1}";

                                if (plotTemplate.Bar != null)
                                {
                                    switch (plotTemplate.Bar.ColorMap)
                                    {
                                        case "viridis":
                                            {
                                                (plotTemplate.Bar.Colors, plotTemplate.Bar.ColorPositions) = ColorMap.Viridis;
                                                break;
                                            }
                                    }

                                    // Get color range:
                                    var ncpCriteria = Constants.D_NUMBER_NCP_THRESHOLDS[0] * 100;
                                    var ncpFactor = Math.Floor(ncpCriteria / 2) + 1;
                                    var multiplier = ncpFactor != 2 ? 2 : 1; // ok sure, but why?                
                                    //var colorScale = new float[] { (float)(multiplier * (ncpCriteria * -1)), (float)(multiplier * ncpCriteria) };
                                    var colorScale = plotTemplate.Bar.Labels;

                                    for (int row = 0; row < arrayData.GetLength(0); row++)
                                    {
                                        for (int col = 0; col < arrayData.GetLength(1); col++)
                                        {
                                            float factor =(float)((arrayData[row, col] - colorScale[0]) / (colorScale[1] - colorScale[0]));
                                            if (factor <= 0)
                                                color = SKColors.Blue;
                                            else if (factor >= 1)
                                                color = SKColors.Red;
                                            else
                                                color = ColorMap.GetColor(ColorMap.Viridis, factor);

                                            var (px, py) = (col * plotTemplate.StrokeWidth + plotTemplate.StrokeWidth / 2, plotTemplate.FrameSize[1] - (row * plotTemplate.StrokeWidth + plotTemplate.StrokeWidth / 2));
                                            paintPoint.Color = color;
                                            canvas.DrawPoint(new SKPoint(pointRef.X + px, pointRef.Y - py), paintPoint);
                                        }
                                    }
                                }
                                break;
                            }
                        case enmPlotType.histogram1:
                            {
                                SetUpLayout(plotTemplate, plotItem);

                                // Preparing plot:
                                float px, py0, py1;
                                SKColor color = new SKColor(31, 119, 180);
                                var paintPoint = new SKPaint { Color = color, StrokeWidth = plotTemplate.StrokeWidth };
                                var arrX = (double[])plotItem.ArrayData!.PartOf(new SliceIndex?[] { new SliceIndex(0), null }!);
                                var arrY = (double[])plotItem.ArrayData!.PartOf(new SliceIndex?[] { new SliceIndex(1), null }!);

                                var (minY, maxY, baseY) = DataTransformation.AdjustLimits(0, arrY.Max(), seccionsForY, true);
                                AxisValues[1] = new double[] { 0, maxY, baseY };
                                SetScaleLayout(plotTemplate, plotItem);

                                for (int k = 0; k < arrX.Length; k++)
                                {
                                    var refX = (arrX[k] - AxisValues![0][0]) * plotTemplate.Axis[0].Scale;
                                    var refY = (arrY[k] - AxisValues[1][0]) * plotTemplate.Axis[1].Scale;
                                    px = (float)(refX + plotTemplate.Axis[0].Offset[0]);
                                    py0 = (float)(plotTemplate.FrameSize[1] - plotTemplate.Axis[1].Offset[0]);
                                    py1 = (float)(py0 - refY);

                                    canvas.DrawLine(new SKPoint(px, py0), new SKPoint(px, py1), paintPoint);
                                }
                                break;
                            }
                        case enmPlotType.histogram2:
                            {
                                var list = new List<double>();
                                var array1D = (double[])plotItem.ArrayData.To1D();
                                var tempMax = array1D.Max();
                                var tempMin = array1D.Min();
                                var tempMed = array1D.Median();
                                var tempDiff = Math.Min(Math.Abs(tempMax - tempMed), Math.Abs(tempMin - tempMed));

                                foreach (var elem in array1D)
                                {
                                    if (tempDiff != 0)
                                    {
                                        if (Math.Abs(tempMed - elem) < tempDiff * 10)
                                            list.Add((float)elem);
                                    }
                                    else
                                        list.Add((float)elem);
                                }

                                var binSize = 10;
                                var bins = new double[] { 0, 1 };
                                double scope = (bins[1]) / binSize;
                                var dict = new Dictionary<double, int>();

                                for (double k = bins[0]; Math.Round(k, 4) <= bins[1]; k += Math.Round(scope, 4))
                                {
                                    var count = list.Count(x => x >= k && x < k + scope);
                                    dict.Add(Math.Round(k, 4), count);
                                }

                                // Axis X:
                                AxisValues[0] = new double[] { bins[0], bins[1], scope };
                                // Axis Y (auto-scaled if it's empty float[]):                            
                                var (minY, maxY, baseY) = DataTransformation.AdjustLimits(dict.Values.Min(), dict.Values.Max(), seccionsForY, true);
                                AxisValues[1] = new double[] { 0, maxY, baseY };
                                SetScaleLayout(plotTemplate, plotItem);

                                // Preparing plot:
                                float px, py0, py1;
                                SKColor color = SKColors.Green;
                                var paintPoint = new SKPaint { Color = color, StrokeWidth = plotTemplate.StrokeWidth };

                                foreach (var elem in dict.OrderBy(x => x.Key))
                                {
                                    var refX = (elem.Key - AxisValues![0][0]) * plotTemplate.Axis[0].Scale;
                                    var refY = (elem.Value - AxisValues[1][0]) * plotTemplate.Axis[1].Scale;
                                    px = (float)(refX + plotTemplate.Axis[0].Offset[0]);
                                    py0 = (float)(plotTemplate.FrameSize[1] - plotTemplate.Axis[1].Offset[0]);
                                    py1 = (float)(py0 - refY);

                                    canvas.DrawLine(new SKPoint(px, py0), new SKPoint(px, py1), paintPoint);
                                }                                
                                break;
                            }
                        case enmPlotType.histogram_stability:
                            {                          
                                var ncpCriteria = Constants.D_NUMBER_NCP_THRESHOLDS[0] * 100;
                                var ncpFactor = Math.Floor(ncpCriteria / 2) + 1;
                                var multiplier = 0 != 2 ? 2 : 1; 
                                var colorScale = new double[] { multiplier * (ncpFactor * -1), multiplier * ncpFactor };
                                plotItem.Title = $"µ={plotItem.ArrayData.NanMean():N4}% σ={plotItem.ArrayData.NanStd():N2}% M={plotItem.ArrayData.NanMedian():N5}%";

                                // X Axis:
                                AxisValues[0] = new double[] { colorScale[0], colorScale[1], Math.Abs(colorScale[1]) * 0.25 };
                                var list = new List<double>();

                                foreach (var elem in (double[,])plotItem.ArrayData)
                                {
                                    list.Add((float)elem);
                                }

                                var listMin = AxisValues[0][0];
                                var listMax = AxisValues[0][1];
                                int binSize = 50; // DataTransformation.GetBinSize(list, new float[] { 0.75f, 0.25f });
                                double scope = (listMax - listMin) / binSize;
                                var dict = new Dictionary<double, int>();

                                for (double k = listMin; k < listMax; k += scope)
                                {
                                    var count = list.OrderBy(x=>x).Count(x => x >= k && x < k + scope);
                                    dict.Add(k, count);
                                }

                                var (minY, maxY, baseY) = DataTransformation.AdjustLimits(0, dict.Values.Max(), seccionsForY, true); // (0, 864, 4);
                                AxisValues[1] = new double[] { minY, maxY, baseY };
                                SetScaleLayout(plotTemplate, plotItem);

                                // Y Axis:
                                SKColor color = new SKColor(31, 119, 180);
                                var paintPoint = new SKPaint { Color = color, StrokeWidth = plotTemplate.StrokeWidth };
                                var pointRef = new SKPoint(point.X, point.Y + plotTemplate.FrameSize[1]);
                                double x, y;
                                float px, py0, py1;

                                // Drawing figures:
                                foreach (var elem in dict.OrderBy(x => x.Key))
                                {
                                    x = (elem.Key - AxisValues[0][0]) * plotTemplate.Axis[0].Scale;
                                    y = (elem.Value - AxisValues[1][0]) * plotTemplate.Axis[1].Scale;

                                    px = (float)(plotTemplate.Axis[0].Offset[0] + x);
                                    py0 = (float)(plotTemplate.FrameSize[1] - plotTemplate.Axis[1].Offset[0]);
                                    py1 = (float)(py0 - y);

                                    canvas.DrawLine(new SKPoint(px, py0), new SKPoint(px, py1), paintPoint);
                                }
                               
                                break;
                            }
                        case enmPlotType.linechart:
                            {
                                SetUpLayout(plotTemplate, plotItem);

                                // Preparing plot:
                                var (x, y) = GetPoint0(plotTemplate);
                                SKPoint pointRef = new SKPoint(x, y);
                                var arrX = (double[])plotItem.ArrayData!.PartOf(new SliceIndex?[] { new SliceIndex(0), null }!);
                                float px, py;
                                SKPath path;

                                for (int j = 1; j < plotItem.ArrayData.GetLength(0); j++) // Start from index 1 the n curves for Y axis
                                {
                                    var arrY = (double[])plotItem.ArrayData!.PartOf(new SliceIndex?[] { new SliceIndex(j), null }!);
                                    path = new SKPath();
                                    path.MoveTo(pointRef);

                                    for (int i = 0; i < arrX.Length; i++)
                                    {
                                        px = (float)(pointRef.X + arrX[i] * plotTemplate.Axis[0].Scale);
                                        py = (float)(pointRef.Y - arrY[i] * plotTemplate.Axis[1].Scale);
                                        if (i == 0)
                                            path.MoveTo(px, py);
                                        else
                                            path.LineTo(px, py);
                                    }

                                    // Draw into canvas:
                                    var paint = new SKPaint()
                                    {
                                        Color = Constants.Brushes[j - 1],
                                        IsAntialias = false,
                                        IsStroke = false,
                                        StrokeWidth = 1f,
                                        Style = SKPaintStyle.Stroke
                                    };

                                    canvas.DrawPath(path, paint);
                                    canvas.DrawPoint(pointRef, Constants.PaintBack);
                                    path.Close();
                                }
                                break;
                            }
                        case enmPlotType.curvechart:
                            {
                                AxisValues = new double[][] { Array.Empty<double>(), Array.Empty<double>() };
                                List<double> listMinX = new List<double>(), listMaxX = new List<double>(), listMinY = new List<double>(), listMaxY = new List<double>();
                                int jumps = 6, numBins = 6, numThresh = plotItem.ArrayData.GetLength(1), threshStart = 30, threshEnd = numThresh + threshStart - 1, threshStep = 1; // windowLength = 17, polyOrder = 3;
                                var binThreshList = ArrayExtensions.GetBinThreshList(numBins, threshStart, threshEnd, threshStep, numThresh);
                                
                                for (int k=0; k< numThresh; k++)
                                {
                                    var arrX = (int[])binThreshList!.PartOf(new SliceIndex?[] { new SliceIndex(k), null }!);
                                    if (arrX != null)
                                    {
                                        var (minValue, maxValue) = DataTransformation.GetLimits(arrX);
                                        listMinX.Add(minValue);
                                        listMaxX.Add(maxValue);
                                    }
                                    else
                                    {
                                        throw new Exception("arrX empty");
                                    }

                                    var arrY = (double[])plotItem.ArrayData.PartOf(new SliceIndex?[] { new SliceIndex(k), null }!);

                                    if (arrY != null)
                                    {
                                        var (minValue, maxValue) = DataTransformation.GetLimits(arrY);
                                        listMinY.Add(minValue);
                                        listMaxY.Add(maxValue);
                                    }
                                    else
                                    {
                                        throw new Exception("arrY empty");
                                    }
                                }

                                // Axis X  (auto-scaled if it's empty float[]):                                    
                                var (minX, maxX, baseX) = DataTransformation.AdjustLimits(listMinX.Min(), listMaxX.Max(), jumps, true);
                                AxisValues[0] = new double[] { minX, maxX, baseX };
                                // Axis Y (auto-scaled if it's empty float[]):
                                var (minY, maxY, baseY) = DataTransformation.AdjustLimits(listMinY.Min(), listMaxY.Max(), jumps, true);
                                AxisValues[1] = new double[] { minY, maxY, baseY };
                                SetScaleLayout(plotTemplate, plotItem);

                                // Preparing plot:j
                                var paint = new SKPaint()
                                {
                                    IsAntialias = true,
                                    IsStroke = false,
                                    StrokeCap = SKStrokeCap.Butt,
                                    StrokeJoin = SKStrokeJoin.Miter,
                                    StrokeWidth = plotTemplate.StrokeWidth,
                                    Style = SKPaintStyle.Stroke,
                                    //TextAlign = SKTextAlign.Center,
                                    //TextSize = 20f,
                                };

                                for (int j=0; j< plotItem.ArrayData.GetLength(0); j++)
                                {
                                    var arrY = (double[])plotItem.ArrayData.PartOf(new SliceIndex?[] { new SliceIndex(j), null }!);
                                    List<SKPoint> points = new List<SKPoint>();

                                    // Paint:
                                    paint.Color = Constants.Brushes[j];

                                    // Path:
                                    var path = new SKPath();
                                    var pointRef = new SKPoint((float)plotTemplate.Axis[0].Offset[0], plotTemplate.FrameSize[1] - (float)plotTemplate.Axis[1].Offset[0]);
                                    float px, py;

                                    path.MoveTo(pointRef);

                                    for (int i = 0; i < arrY.Length; i++)
                                    {
                                        px = (float)(pointRef.X + (threshStart + i - AxisValues[0][0]) * plotTemplate.Axis[0].Scale);
                                        py = (float)(pointRef.Y - (arrY[i] - AxisValues[1][0]) * plotTemplate.Axis[1].Scale);

                                        if (i == 0)
                                            path.MoveTo(px, py);
                                        else
                                            path.LineTo(px, py);
                                    }

                                    // Draw into canvas:
                                    canvas.DrawPath(path, paint);

                                    path.Close();
                                }
                                break;
                            }
                    }

                    surface.Canvas.DrawBitmap(bitmap, new SKPoint(point.X, point.Y - (int)plotTemplate.FrameSize[1]));
                }
                else
                    throw new Exception($"FrameSize invalid: {plotTemplate.FrameSize.Length}");
            }
            else
                SetNoData(plotTemplate, point, surface);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plotTemplate"></param>
        /// <param name="plotItem"></param>
        public void SetUpLayout(PlotTemplate plotTemplate, PlotItem plotItem)
        {
            // Get ranges:
            if (plotTemplate.Axis[0].Range == null || plotTemplate.Axis[0].Range.Length == 0)
            {
                var range = (double[])plotItem.ArrayData!.PartOf(new SliceIndex?[] { new SliceIndex(0), null }!);
                plotTemplate.Axis[0].Range = new double[] { range[0], range[range.Length - 1], 1 };
            }
            if (plotTemplate.Axis[1].Range == null || plotTemplate.Axis[1].Range.Length == 0)
            {
                var range = (double[])plotItem.ArrayData!.PartOf(new SliceIndex?[] { new SliceIndex(1), null }!);
                plotTemplate.Axis[1].Range = new double[] { range[0], range[range.Length - 1], 1 };
            }

            // Set Axises:
            if (plotTemplate.Axis[0].Range != null && plotTemplate.Axis[0].Range.Length == 3)
            {
                AxisValues[0] = plotTemplate.Axis[0].Range;
            }
            if (plotTemplate.Axis[1].Range != null && plotTemplate.Axis[1].Range.Length == 3)
            {
                AxisValues[1] = plotTemplate.Axis[1].Range;
            }

            SetScaleLayout(plotTemplate, plotItem);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plotTemplate"></param>
        /// <param name="point"></param>
        /// <param name="surface"></param>
        /// <param name="addToTitle"></param>
        public void DrawPlotTitle(PlotTemplate plotTemplate, SKPoint point, SKSurface surface, PlotItem plotItem, string addToTitle = "")
        {            
            if(plotTemplate.IsTitleVisible)
                surface.Canvas.DrawText(plotItem.Title + addToTitle, point.X + plotTemplate.FrameSize[0] / 2f, point.Y - plotTemplate.FrameSize[1] - 5, TextFont, Constants.PaintTitle);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plotTemplate"></param>
        /// <param name="point"></param>
        /// <param name="surface"></param>
        /// <param name="plotItem"></param>
        public void DrawData(PlotTemplate plotTemplate, SKPoint point, SKSurface surface, PlotItem plotItem)
        {
            DrawFigures(plotTemplate, point, surface, plotItem);
            DrawLayout(plotTemplate, point, surface);
            DrawAxis(plotTemplate, point, surface);
            DrawBar(plotTemplate, point, surface);
        }

        #endregion
    }
}
