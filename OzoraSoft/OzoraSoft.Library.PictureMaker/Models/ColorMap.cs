using SkiaSharp;
using System;

namespace OzoraSoft.Library.PictureMaker
{
    /// <summary>
    /// all the color map use in FAT Cloud
    /// https://matplotlib.org/stable/tutorials/colors/colormaps.html
    /// </summary>
    public sealed class ColorMap
    {
        #region properties

        /// <summary>
        /// Viridis
        /// https://waldyrious.net/viridis-palette-generator/
        /// </summary>
        public static (SKColor[] ColorTable, float[] ColorPos) Viridis
        {
            get
            {
                return new(new SKColor[] {
                        new SKColor(68, 1, 84),
                        new SKColor(65, 68, 135),
                        new SKColor(42, 120, 142),
                        new SKColor(34, 168, 132),
                        new SKColor(122, 209, 81),
                        new SKColor(253, 231, 37)
                },
                    new float[] { });
            }
        }

        /// <summary>
        /// JET
        /// </summary>
        public static (SKColor[] ColorTable, float[] ColorPos) Jet
        {
            get
            {
                return new(
                    new SKColor[] {
                        new SKColor(0, 0, 131),
                        new SKColor(0, 0, 255),
                        new SKColor(0, 255, 255),
                        new SKColor(255, 255, 0),
                        new SKColor(255, 0, 0),
                        new SKColor(128, 0, 0)
                    },
                    new float[] {
                        0f,
                        0.125f,
                        0.375f,
                        0.625f,
                        0.875f,
                        1f
                    });
            }
        }

        #endregion properties

        #region method

        /// <summary>
        /// get color from color map and positon
        /// </summary>
        /// <param name="colorMap">a color map</param>
        /// <param name="fPos">position (0.0--1.0)</param>
        /// <returns></returns>
        public static SKColor GetColor((SKColor[] ColorTable, float[] ColorPos) colorMap, float fPos)
        {
            if (colorMap.ColorTable != null)
            {
                if (fPos <= 0f)
                {
                    return colorMap.ColorTable[0];
                }
                else if (fPos >= 1f)
                {
                    return colorMap.ColorTable[^1];
                }
                else
                {
                    if (colorMap.ColorTable.Length <= 1)
                    {// only one color
                        return colorMap.ColorTable[0];
                    }
                    else
                    {
                        float[] fPositions = colorMap.ColorPos;
                        if (fPositions == null)
                        {//generate positions
                            fPositions = new float[colorMap.ColorTable.Length];
                            for (int i = 0; i < fPositions.Length; i++)
                            {
                                fPositions[i] = i / (fPositions.Length - 1f);
                            }
                        }
                        //find its segment
                        int nStart = 0;
                        int nEnd = fPositions.Length - 1;
                        for (int i = 0; i < fPositions.Length - 1; i++)
                        {
                            if (fPos >= fPositions[i] && fPos < fPositions[i + 1])
                            {
                                nStart = i;
                                nEnd = i + 1;
                                break;
                            }
                        }
                        float offset = (float)Math.Round((fPos - fPositions[nStart]) / (fPositions[nEnd] - fPositions[nStart]), 2);
                        SKColor left = colorMap.ColorTable[nStart];
                        SKColor right = colorMap.ColorTable[nEnd];
                        return new SKColor(
                            (byte)((right.Red - left.Red) * offset + left.Red),
                            (byte)((right.Green - left.Green) * offset + left.Green),
                            (byte)((right.Blue - left.Blue) * offset + left.Blue),
                            (byte)((right.Alpha - left.Alpha) * offset + left.Alpha)
                            );
                    }
                }
            }
            else
            {//something wrong
                return SKColors.Red;
            }
        }

        #endregion method
    }
}