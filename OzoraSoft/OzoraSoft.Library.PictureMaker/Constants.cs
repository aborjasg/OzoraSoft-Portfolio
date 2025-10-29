using SkiaSharp;
using System.Drawing;

namespace OzoraSoft.Library.PictureMaker
{
    public static class Constants
    {
        public const int NUM_COLS = 36;
        public const int NUM_ROWS = 24;
        public const int NUM_BINS = 6;
        public static long UNITILIALIZED_VALUE = -99999;
        public static double[] D_NUMBER_NCP_THRESHOLDS { get; set; } = new double[] { 0.001, 0.001, 0.001 };
        public static int[] Ticks = new int[] { 0, 10, 100, 864 };
        public const double SAMPLE_PERIOD = 0.025400000000000002;
        public const int REQUIRED_SAMPLE_PERIOD = 50;

        public static SKColor[] Brushes = { SKColors.Blue, SKColors.Orange, SKColors.Green, SKColors.Red, SKColors.MediumPurple, SKColors.SaddleBrown };
        /// <summary>
        /// 
        /// </summary>
        public static SKPaint PaintTitle = new SKPaint()
        {
            Color = SKColors.Black,
            IsAntialias = false,
            IsStroke = false,
            StrokeCap = SKStrokeCap.Butt,
            StrokeJoin = SKStrokeJoin.Miter,
            StrokeWidth = 1f,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Center,
            Typeface = SKTypeface.FromFamilyName("Arial"),
            TextSize = 11f,
            FilterQuality = SKFilterQuality.High
        };
        /// <summary>
        /// 
        /// </summary>
        public static SKPaint PaintText = new SKPaint()
        {
            Color = SKColors.Black,
            IsAntialias = false,
            IsStroke = false,
            StrokeCap = SKStrokeCap.Butt,
            StrokeJoin = SKStrokeJoin.Miter,
            StrokeWidth = 1f,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Left,
            TextSize = 14f,
            FilterQuality = SKFilterQuality.High
        };
        /// <summary>
        /// 
        /// </summary>
        public static SKPaint PaintTextSmall  = new SKPaint()
        {
            Color = SKColors.Black,
                IsAntialias = false,
                IsStroke = false,
                StrokeCap = SKStrokeCap.Butt,
                StrokeJoin = SKStrokeJoin.Miter,
                StrokeWidth = 1f,
                Style = SKPaintStyle.Fill,
                TextAlign = SKTextAlign.Left,
                TextSize = 10f,
                FilterQuality = SKFilterQuality.High
            };
        /// <summary>
        /// 
        /// </summary>
        public static SKPaint PaintTextSmallCenter = new SKPaint()
        {
            Color = SKColors.Black,
            IsAntialias = false,
            IsStroke = true,
            StrokeCap = SKStrokeCap.Butt,
            StrokeJoin = SKStrokeJoin.Miter,
            StrokeWidth = 1f,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Center,
            TextSize = 9f,
            FilterQuality = SKFilterQuality.High
        };
        /// <summary>
        /// 
        /// </summary>
        public static SKPaint PaintAxis = new SKPaint()
        {
            Color = SKColors.Black,
            IsAntialias = false,
            IsStroke = true,
            StrokeWidth = 1f,
            StrokeCap = SKStrokeCap.Butt,
            StrokeJoin = SKStrokeJoin.Miter,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Left,
            Typeface = SKTypeface.FromFamilyName("Arial"),
            TextSize = 10f,
            FilterQuality = SKFilterQuality.High
        };
        /// <summary>
        /// 
        /// </summary>
        public static SKPaint PaintLegend = new SKPaint()
        {
            Color = SKColors.Black,
            IsAntialias = true,
            IsStroke = true,
            StrokeWidth = 1f,
            StrokeCap = SKStrokeCap.Butt,
            StrokeJoin = SKStrokeJoin.Miter,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Left,
            Typeface = SKTypeface.FromFamilyName("Arial"),
            TextSize = 9f,
            FilterQuality = SKFilterQuality.High
        };
        /// <summary>
        /// 
        /// </summary>
        public static SKPaint PaintBack = new SKPaint()
        {
            Color = SKColors.Black,
            IsAntialias = true,
            IsStroke = true,
            Style = SKPaintStyle.Fill,
            StrokeWidth = 2,
            FilterQuality = SKFilterQuality.High
        };
        /// <summary>
        /// 
        /// </summary>
        public static SKPaint PaintFrame = new SKPaint()
        {
            Color = SKColors.WhiteSmoke,
            IsAntialias = true,
            IsStroke = true,
            StrokeCap = SKStrokeCap.Butt,
            StrokeJoin = SKStrokeJoin.Miter,
            StrokeWidth = 1f,
            Style = SKPaintStyle.Fill,
            Typeface = SKTypeface.FromFamilyName("Arial"),
            TextAlign = SKTextAlign.Left,
            FilterQuality = SKFilterQuality.High
        };
        /// <summary>
        /// 
        /// </summary>
        public static SKPaint PaintFrameSmall = new SKPaint()
        {
            Color = SKColors.Black,
            IsAntialias = true,
            IsStroke = true,
            StrokeCap = SKStrokeCap.Butt,
            StrokeJoin = SKStrokeJoin.Miter,
            StrokeWidth = 1f,
            Style = SKPaintStyle.Stroke,
            Typeface = SKTypeface.FromFamilyName("Arial"),
            TextAlign = SKTextAlign.Left,
            FilterQuality = SKFilterQuality.High
        };
        /// <summary>
        /// 
        /// </summary>
        public static SKPaint PaintBorder = new SKPaint()
        {
            TextSize = 11f,
            IsAntialias = false,
            IsStroke = true,
            Color = SKColors.Black,
            StrokeWidth = 1f,
            Style = SKPaintStyle.Stroke,
            TextAlign = SKTextAlign.Right,
            FilterQuality = SKFilterQuality.High
        };
        /// <summary>
        /// 
        /// </summary>
        public static SKPaint PaintPoint = new SKPaint()
        {
            TextSize = 11f,
            IsAntialias = false,
            IsStroke = false,
            Color = SKColors.Black,
            StrokeWidth = 1f,
            Style = SKPaintStyle.Stroke,
            TextAlign = SKTextAlign.Right,
            FilterQuality = SKFilterQuality.High,            
        };
        /// <summary>
        /// 
        /// </summary>
        public static SKPaint PaintSquare = new SKPaint()
        {
            Color = SKColors.Gray,
            IsAntialias = false,
            IsStroke = true,
            StrokeCap = SKStrokeCap.Butt,
            StrokeJoin = SKStrokeJoin.Miter,
            Style = SKPaintStyle.Fill,
            StrokeWidth = 1f,
            PathEffect = SKPathEffect.CreateDash(new float[] { 1, 1 }, 2)
        };
    }
}

