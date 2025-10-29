using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Newtonsoft.Json;
using OzoraSoft.Library.Enums.PictureMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;


namespace OzoraSoft.Library.PictureMaker
{
    /// <summary>
    /// 
    /// </summary>
    public class DataTransformation
    {
		#region "Auxiliary Functions"

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		private static string ArrayToString(double[,] array)
		{
			string result = "";
			for (int i = 0; i < array.GetLength(0); i++)
			{
				result += "[";
				for (int j = 0; j < array.GetLength(1); j++)
				{
					result += array[i, j].ToString();
					if (j < array.GetLength(1) - 1)
						result += ", ";
				}
				result += "]";
				if (i < array.GetLength(0) - 1)
					result += ", ";
			}

			return "[[[" + result + "]]]";
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="indexesInterp"></param>
		/// <param name="polyCoeffs"></param>
		/// <returns></returns>
		private static double[] PolynomialValues(int[] indexesInterp, double[] polyCoeffs)
		{
			var result = new double[indexesInterp.Length];

			for (int j = 0; j < indexesInterp.Length; j++)
				for (int i = 0; i < polyCoeffs.Length; i++)
				{
					result[j] += polyCoeffs[i] * Math.Pow(indexesInterp[j], i);
				}
			return result;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="indexesInterp"></param>
		/// <param name="polyCoeffs"></param>
		/// <returns></returns>
		private static Array PolynomialValues(int[] indexesInterp, Array polyCoeffs)
		{
			var result = Array.CreateInstance(typeof(double), new int[] { indexesInterp.GetLength(0), polyCoeffs.GetLength(1) });

			for (int k = 0; k < polyCoeffs.GetLength(0); k++)
			{
				var coeffs = (Array)polyCoeffs.PartOf(new SliceIndex?[] { new SliceIndex(k), null }!);
				for (int p = 0; p < indexesInterp.Length; p++)
					result.SetValue(((Array)result.PartOf(new SliceIndex?[] { new SliceIndex(p), null }!)).Multip(indexesInterp[p]).Sum(coeffs), new int?[] { p, null });
			}
			return result;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>        
		private static double Factorial(int n)
		{
			if (n == 1 || n == 0)
				return 1;
			else
				return n * Factorial(n - 1);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="a"></param>
		/// <param name="x0"></param>
		/// <param name="mean"></param>
		/// <param name="sigma"></param>
		/// <returns></returns>
		private static double Gauss(double x, double a, double x0, double mean = 59.54, double sigma = 1)
		{
			return a * Math.Exp(-Math.Pow(x - x0, 2) / Math.Pow(2 * sigma, 2));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static double[] MultiplyAccumulate(double[] input)
		{
			var result = new double[input.Length];
			double accumulate = 1;

			for (int k = 0; k < input.Length; k++)
			{
				if (k > 0) accumulate = result[k - 1];
				result[k] = accumulate * input[k];
			}
			return result;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="spectraList"></param>
		/// <param name="thresholdList"></param>
		/// <param name="bin"></param>
		/// <returns></returns>
		public static double[] DNLScrub(Array spectraList, int[] thresholdList, int bin)
        {
			double[] result = (double[])spectraList;
			var dnlList = new int[] { 30 + bin, 62 + bin, 94 + bin, 126 + bin, 158 + bin };

			//for (int i = 1; i < result.Length - 1; i++)
			Parallel.For(1, result.Length - 1, i =>
			{
				if (dnlList.Contains(thresholdList[i]))
				{
					ref double curr = ref result[i];
					ref double prev = ref result[i - 1];
					ref double next = ref result[i + 1];

					if (curr > 50 && prev > 0 && next > 0)
					{
						if (curr / prev > 1 || curr / next > 1)
						{
							curr = (prev + next) / 2;
						}
					}
				}
			});
			return result;
		}
		/// <summary>
		/// Savosky-Goland Coefficients 
		/// </summary>
		/// <param name="window_length"></param>
		/// <param name="polyorder"></param>
		/// <param name="deriv"></param>
		/// <param name="delta"></param>
		/// <param name="use"></param>
		/// <param name="pos"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static double[] SavGol_Coeffs(int window_length, int polyorder, int deriv = 0, double delta = 1, string use = "conv", int? pos = null)
		{
			var result = new double[window_length];

			if (polyorder >= window_length)
				throw new Exception("Polyorder must be less than window_length");
			else
			{
				var halflen = window_length / 2;
				var rem = window_length % 2;

				if (pos == null)
				{
					if (rem == 0)
						pos = halflen / 2;
					else
						pos = halflen;
				}

				if (pos < 0)
					throw new Exception("Pos must be nonnegative and less than Window length");

				if (deriv > polyorder) return (double[])result.FillValues<double>(0);

				var factors = CreateSequence((double)(-1 * pos), window_length - (int)pos - 1);

				if (use == "conv") factors = factors.Revert1D();

				var order = CreateSequence(0d, polyorder); //.Reshape(new int[] { -1, 1 });

				var A = factors.Pow(order);
				var y = (double[])Array.CreateInstance(typeof(double), order.Length);
				y[deriv] = Factorial(deriv) / Math.Pow(delta, deriv);

				result = LeastSquares(A, y);
			}

			return result;
		}
		/// <summary>
		/// Savosky-Goland Filter: Savitzky-Golay Smoothing (Provider: CenterSpace)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="window_length"></param>
		/// <param name="polyorder"></param>
		/// <returns>double[]: Refine array values ready to plot</returns>
		public static double[] SavGol_Filter(double[] data, int window_length, int polyorder)
		{
			// Generate the coefficient:
			int nSide = (window_length - 1) / 2;
			//var filter = new SavitzkyGolayFilter(nSide, nSide, polyorder, 0);

			if (window_length > data.Length)
				throw new Exception("Window length must be less than or equal to the size of  data array");

			if (data.GetDimensions().Length != 1)
				throw new Exception("The size of  data array must be 1 only");

            // Get refined array values:
            //var filter = new SavitzkyGolayFilter(nSide, polyorder);

            return null!; // filter.Process(data);
		}
		/// <summary>
		/// Savosky-Goland Filter
		/// </summary>
		/// <param name="input"></param>
		/// <param name="window_length"></param>
		/// <param name="polyorder"></param>
		/// <param name="mode"></param>
		/// <param name="axis"></param>
		/// <returns>double[] </returns>
		public static double[] SavGol_Filter(double[] input, int window_length, int polyorder, int axis, string mode = "interp")
		{
			var result = new double[0];
			var coeffs = SavGol_Coeffs(window_length, polyorder);

			if (mode == "interp")
			{
				if (window_length > input.Length)
					throw new Exception("Window length must be less than or equal to the size of  data array");

				var output = (double[])input.Correlate1D(coeffs);
				FitEdges_PolynomialInterpolation(input, output, window_length, polyorder, axis);
			}
			else
				result = (double[])input.Correlate1D(coeffs, axis);

			return result;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="input"></param>
		/// <param name="window_length"></param>
		/// <param name="polyorder"></param>
		/// <param name="axis"></param>
		/// <param name="mode"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static Array SavGol_Filter(Array input, int window_length, int polyorder, int axis, string mode = "interp")
		{
			Array? result = Array.CreateInstance(typeof(double), input.GetDimensions());
			var coeffs = SavGol_Coeffs(window_length, polyorder);

			if (mode == "interp")
			{
				if (window_length > input.Length)
					throw new Exception("Window length must be less than or equal to the size of  data array");

				var output = input.Correlate1D(coeffs, axis);
				result = FitEdges_PolynomialInterpolation(input, output, window_length, polyorder, axis);
			}
			else
				result = input.Correlate1D(coeffs);

			return result;
		}
		/// <summary>
		/// FitEdgesPolynomialInterpolation
		/// </summary>
		/// <returns></returns>
		public static Array FitEdges_PolynomialInterpolation(Array x, Array y, int window_length, int polyorder, int axis = -1, int deriv = 0, double delta = 1)
		{
			int halflen = window_length / 2;
			FitEdge(x, ref y, 0, window_length, 0, halflen, polyorder, axis, deriv, delta);

			var n = x.GetLength(0);
			FitEdge(x, ref y, n - window_length, n, n - halflen, n, polyorder, axis, deriv, delta);

			return y;
		}
		/// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="windowStart"></param>
        /// <param name="windowStop"></param>
        /// <param name="interpStart"></param>
        /// <param name="interpStop"></param>
        /// <param name="polyorder"></param>
        /// <param name="axis"></param>
        /// <param name="deriv"></param>
        /// <param name="delta"></param>
		public static void FitEdge(Array x, ref Array y, int windowStart, int windowStop, int interpStart, int interpStop, int polyorder, int axis, int deriv, double delta)
		{
			var result = new double[0];
			var x_edge = (Array)x.PartOf(new SliceIndex?[] { new SliceIndex(windowStart, windowStop), null, null }!);
			bool swapped = false;

			// Axis
			if (axis > 0 && axis < x.GetDimensions().Length)
			{
				x_edge = (double[])x_edge.SwapAxes(axis, 0);
				swapped = true;
			}

			var xx_edge = x_edge.Reshape(new int[] { x_edge.GetLength(0), -1 });
			var indexesCoeffs = CreateSequence(0, (double)xx_edge.GetLength(0) - 1);
			var polyCoeffs = PolynomialFit(indexesCoeffs, (double[,])xx_edge, polyorder);

			//if (deriv > 0) polyCoeffs = PolyDer(polyCoeffs, deriv);

			var interpolated = CreateSequence(interpStart - windowStart, interpStop - windowStart - 1);
			var values = PolynomialValues(interpolated, polyCoeffs);
			values = values.Div(Math.Pow(delta, deriv));

			var shp = y.GetDimensions();
			if (shp.Length > 1)
			{
				var tmp = shp[axis];
				shp[0] = tmp;
				shp[axis] = shp[0];
				values = values.Reshape(new int[] { interpStop - interpStart, shp[1], shp[2] });
			}

			if (swapped && axis > 0) values = values.SwapAxes(0, axis);

			for (int k = 0; k < interpStop - interpStart; k++)
				y.SetValue((Array)values.PartOf(new SliceIndex?[] { new SliceIndex(k), null, null }!), new int?[] { interpStart + k, null, null });

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="degree"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static Array PolynomialFit(double[] x, double[,] y, int degree)
		{
			var order = degree + 1;

			if (degree < 0)
				throw new ArgumentException("Degree expected > 0");
			if (x.GetDimensions().Length != 1)
				throw new ArgumentException("Expected 1D array for X");
			if (x.Length == 0)
				throw new ArgumentException("Expected non-empty array for X");
			if (y.GetDimensions().Length != 2)
				throw new ArgumentException("Expected 2D array for Y");
			if (x.Length != y.GetLength(0))
				throw new ArgumentException("Expected X and Y to haver same length");

			var recond = x.Length * double.Epsilon;
			var lhs = (double[,])Array.CreateInstance(typeof(double), new int[] { x.Length, order }).FillValues<double>(1);

			// vander:
			for (int j = 0; j < x.Length; j++)
			{
				for (int i = 0; i < order - 1; i++)
				{
					lhs[j, i] = x[j];
				}
			}

			for (int k = 0; k < lhs.GetLength(0); k++)
			{
				var temp = ((double[])lhs.PartOf(new SliceIndex?[] { new SliceIndex(k), null }!)).Revert1D();
				lhs.SetValue(MultiplyAccumulate(temp).Revert1D(), new int?[] { k, null });
			}

			var rhs = y;
			var scale = ((Array)lhs.Multip(lhs).NanSum(0)).Sqrt();
			var lhsDiv = lhs.Div(scale);
			var coeffs = new double[lhsDiv.GetLength(1), rhs.GetLength(1)];

			//ArrayExtensions.LeastSquares(ref coeffs, (double[,])lhsDiv, rhs);

			for (int k = 0; k < coeffs.GetLength(1); k++)
			{
				var c = LeastSquares((double[,])lhsDiv, (double[])rhs.PartOf(new SliceIndex?[] { null, new SliceIndex(k) }!));
				coeffs.SetValue(c, new int?[] { null, k });
			}

			return (Array)((Array)coeffs.Transpose()).Div(scale).Transpose(); ;
		}
		/// <summary>
		/// PreparedArray: Take just 6 multi-arrays in the 2nd dimension (Source: spectral_class.py/def __init__)
		/// </summary>
		/// <param name="data"></param>
		/// <param name="D1"></param>
		/// <param name="D2"></param>
		/// <param name="D3"></param>
		/// <param name="D4"></param>
		/// <returns>float[][][][]</returns>
		public static object TrimArray(dynamic data, int D1, int D2, int D3, int D4)
		{
			var result = new double[D1, D2, D3, D4];

			for (int d1 = 0; d1 < D1; d1++)
			{
				for (int d2 = 0; d2 < D2; d2++)
				{
					for (int d3 = 0; d3 < D3; d3++)
					{
						for (int d4 = 0; d4 < D4; d4++)
						{
							result[d1, d2, d3, d4] = data[d1, d2, d3, d4];
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Linear Least Squares
		/// </summary>
		/// <param name="a">double[,]</param>
		/// <param name="b">double[]</param>
		/// <returns>double[]</returns>
		public static double[] LeastSquares(double[,] a, double[] b)
		{
			var result = new double[0];
			//int info;
			//alglib.lsfitreport rep;
			//alglib.lsfitlinear(b, a, out result, out rep);
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		public static void LeastSquares(ref double[,] data, double[,] a, double[,] b)
		{
			for (int k = 0; k < data.GetLength(1); k++)
			{
				var c = LeastSquares(a, (double[])b.PartOf(new SliceIndex?[] { null, new SliceIndex(k) }!));
				data.SetValue(c, new int?[] { null, k });
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="num_bins"></param>
		/// <param name="thresh_start"></param>
		/// <param name="thresh_end"></param>
		/// <param name="thresh_step"></param>
		/// <returns></returns>
		public static int[][] CreateSequence(int num_bins, int thresh_start, int thresh_end, int thresh_step)
        {
            var result = new int[num_bins][];

            for (int k = 0; k < num_bins; k++)
            {
                var temp = new int[thresh_end - thresh_start + 1];
                for (int i = 0; i <= thresh_end - thresh_start; i += thresh_step)
                {
                    temp[i] = k + thresh_start + i;
                }
                result[k] = temp;
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static double[] CreateSequence(double start, double end, double step=1)
        {			
			var result = new double[(int)((end - start) / step) + 1];

			for (int k = 0; k < result.Length; k++)
			{
				result[k] = start + step * k;
			}
			return result;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static int[] CreateSequence(int start, int end, int step = 1)
        {
            var result = new int[(end - start) / step + 1];

            for (int k = 0; k < result.Length; k++)
            {
                result[k] = start + step * k;
            }
            return result;
        }        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="a"></param>
        /// <param name="x0"></param>
        /// <param name="binOfInterest"></param>
        /// <param name="mean"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        private static double Gauss(double x, double a, double x0, int binOfInterest=1, double mean=59.54, double sigma=1)
        {
            return a * Math.Exp(-Math.Pow(x - x0, 2) / Math.Pow(2 * sigma, 2));
        }
          
        /// <summary>
        /// CalculateCRC
        /// </summary>
        /// <param name="data"></param>
        /// <returns>ushort</returns>
        public static ushort CalculateCRC(byte[] data)
        {
            ushort wCRC = 0;
            foreach (byte bt in data)
            {
                wCRC ^= (ushort)(bt << 8);
                for (int j = 0; j < 8; j++)
                {
                    //wCRC = wCRC & 0x8000 != 0 ? (wCRC << 1) ^ 0x8005 : wCRC << 1;
                }
            }
            return wCRC;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static string FixJsonValueforList(string json, string field)
        {
            return json.Replace('"' + field + '"' + ": " + '"' + "N/A" + '"', '"' + field + '"' + ": " + "[" + $"{Constants.UNITILIALIZED_VALUE}" + "]"); ;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string SanitizeString(string data)
        {
            var sanitizedString = data.Replace("N/A", $"{Constants.UNITILIALIZED_VALUE}");
            return sanitizedString;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static IEnumerable<string?> ReplaceNewLine(string str)
        {
            if (string.IsNullOrEmpty(str)) yield return null;

            string[] temp = str.Split('\n');

            foreach (var item in temp)
            {
                yield return item;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static (double, double) GetLimits(double[] data)
        {
            if (data.Length > 0)
            {
                var list = new List<double>(data);
                double min = list.Min(), max = list.Max();
                return (min, max);
            }
            else
            {
                return (0, 0);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static (double, double) GetLimits(int[] data)
        {
            var result = (0d, 0d);
            if (data != null)
            {
                if (data.Length > 0)
                {
                    var list = new List<double>();
                    foreach (var item in data)
                        list.Add(item);

                    double min = list.Min(), max = list.Max();
                    result = (min, max);
                }
            }
            return result;
        }
       /// <summary>
       /// 
       /// </summary>
       /// <param name="min"></param>
       /// <param name="max"></param>
       /// <param name="jumps"></param>
       /// <param name="autoSplit"></param>
       /// <returns></returns>
        public static (double, double, double) AdjustLimits(double min, double max, int jumps, bool autoSplit = true)
        {
            double range = max - min != 0 ? max - min : max, range0 = range, power = 0, resultMin = 0, resultMax = 0, base10 = 1;

            // Assessing 'base10':
            if (range != 0 && jumps > 1)
            {
                if (range / 10 >= 1)
                {
                    while (range / 10 >= 1)
                    {
                        power++;
                        range /= 10;
                    }
                }
                else if (range * 10 < 1)
                {
                    while (range < 1)
                    {
                        power--;
                        range *= 10;
                    }
                }
                else if (max < 10)
                {
                    power = 0;
                }
                else
                {
                    power = 1;
                }

                base10 = Math.Pow(10, power);
                range = range0;

                if (range % jumps != 0 || range % base10 != 0)
                {
                    var searchFactor = base10;
                    while (range / searchFactor > jumps)
                    {
                        searchFactor += base10;
                    }
                    base10 = searchFactor;
                }

                // Re-adjusting the base10:
                while (autoSplit && (max - min) / base10 > 1 && (max - min) / base10 < jumps / 2)
                {
                    base10 /= 2;
                }

                // Estimating minimum limit:
                if (min % base10 != 0)
                {
                    var diff = min % base10;
                    resultMin = min - diff;

                    if (Math.Abs(diff / base10) >= 0.2 && Math.Abs(diff / base10) < 1) // Removing: min < 0
                    {
                        if (autoSplit && base10 == Math.Pow(10, power) && Math.Abs(diff / base10) > 0.5)
                        {
                            base10 /= 2;
                            resultMin += base10;
                        }
                        else if (Math.Abs(diff / base10) >= 0.75)
                            resultMin += base10;
                    }
                }
                else
                    resultMin = min;

                // Estimating maximum limit:
                if (max % base10 != 0)
                {
                    var diff = max % base10;
                    resultMax = max - diff != 0 ? max - diff : max;

                    if (max > 0 && Math.Abs(diff / base10) >= 0.2 && Math.Abs(diff / base10) < 1)
                    {
                        if (autoSplit && base10 == Math.Pow(10, power) && Math.Abs(diff / base10) < 0.5)
                        {
                            base10 /= 2;
                            resultMax += base10;
                        }
                        else if (Math.Abs(diff / base10) >= 0.5)
                            resultMax += base10;
                    }
                }
                else
                    resultMax = max;

                return (resultMin, resultMax, base10);
            }
            else
                return (min, max, base10);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="percentile"></param>
        /// <returns></returns>
        public static double GetPercentil(List<double> sample, float[] percentile)
        {
            var sampleOrdered = new List<double>(sample);
            sampleOrdered.Sort();

            int perHighIndex = (int)Math.Round(percentile[0] * (sampleOrdered.Count - 1) + 1);
            int perLowIndex = (int)Math.Round(percentile[1] * (sampleOrdered.Count - 1) + 1);
            var perHigh = sampleOrdered[perHighIndex - 1];
            var perLow = sampleOrdered[perLowIndex - 1];

            double n = (sampleOrdered.Count + 1) * percentile[0];
            double n1 = (sampleOrdered.Count - 1) * (percentile[0] - percentile[1]) + 1;
            //sampleOrdered = sampleOrdered.Where(x => x >=perMin && x <= perMax).ToList();
            double iqr;

            if (n == 1d) iqr = sampleOrdered[0];
            else if (n == sampleOrdered.Count) iqr = sampleOrdered[sampleOrdered.Count - 1];
            else
                iqr = perHigh - perLow;

            var result = (double)(2.0 * iqr * Math.Pow(sample.Count, -1.0 / 3.0));
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="percentile"></param>
        /// <returns></returns>
        public static int GetBinSize(List<double> sample, float[] percentile)
        {
            var width = GetPercentil(sample, percentile);
            return (int)Math.Ceiling(Math.Abs(sample.Max() - sample.Min()) / width);
        }        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static double[] ConvertTo1DArray(float[] list)
        {
            double[] result = new double[list.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = list[i];
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static double[,] ConvertTo2DArray(float[][] list)
        {
            double[,] result = new double[list.GetLength(0), list[0].GetLength(0)];
            for (int j = 0; j < result.GetLength(0); j++)
            {
                for (int i = 0; i < result.GetLength(1); i++)
                {
                    result[j, i] = list[j][i];
                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static double[,] ConvertTo2DArray(bool[,] list)
        {
            double[,] result = new double[list.GetLength(0), list.GetLength(1)];
            for (int j = 0; j < result.GetLength(0); j++)
            {
                for (int i = 0; i < result.GetLength(1); i++)
                {
                    result[j, i] = list[j, i] == true ? 1 : 0;
                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static double[,] ConvertTo2DArray(double[,,,] list, int row, int col)
        {
            double[,] result = new double[list.GetLength(0), list.GetLength(1)];
            for (int j = 0; j < result.GetLength(0); j++)
            {
                for (int i = 0; i < result.GetLength(1); i++)
                {
                    result[j, i] = list[j, i, row, col];
                }
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T[,] Convert<T>(T[][] source)
        {
            T[,]? oRcd = null;
            if (source != null && source?.Length > 0 && source?[0].Length > 0)
            {
                oRcd = new T[source.Length, source[0].Length];
                for (int i = 0; i < source.Length; i++)
                {
                    for (int j = 0; j < source[0].Length; j++)
                    {
                        oRcd[i, j] = source[i][j];
                    }
                }
            }
            return oRcd!;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">format: "0.00e+00"</param>
        /// <returns></returns>
        public static string ConvertToExponentialFormat(string key)
        {
            string[] sections = key.Split(", ");

            if (sections.Length == 2)
            {
                string[] section1 = sections[0].Trim().Split(':');
                string[] section2 = sections[1].Trim().Split(':');

                string value1 = section1[1].Trim();
                if (value1 != "NaN")
                {
                    var tmp = System.Convert.ToDouble(section1[1]);
                    if (tmp <= 1)
                        value1 = tmp.ToString("0.00");
                    else
                        value1 = tmp.ToString("0.00e+00");
                }
                else
                    value1 = "0.00";

                string value2 = section2[1].Trim();
                if (value2 != "NaN")
                {
                    var tmp = System.Convert.ToDouble(section2[1]);
                    value2 = tmp.ToString("0.00e+00");
                }
                else
                    value2 = "0.00";

                return $"{section1[0]}: {value1}, {section2[0]}: {value2}";
            }
            else
                return key;
        }
        /// <summary>
        /// 
        /// </summary>
        public static (double, string) ConvertToExponentialFormat(double value)
        {
            double doubleResult = value;
            int exp = 0;

            while (doubleResult / 10 >= 1)
            {
                doubleResult /= 10;
                exp++;
            }
            string stringResult = "1e" + exp.ToString();

            return (doubleResult, stringResult);
        }

        private static List<int> TUBE_SPECTRA_VOLTAGES = new List<int> { 80, 100, 120, 140 };
        private static double[,] TUBE_SPECTRA = new double[,] {
            { 0, 0, 0, 0, 0, 5.51389E-31, 2.75759E-17, 1.07107E-09, 0.000149618, 0.190552163, 1.865341688, 84.19044375,
             101.4284813, 491.2833375, 1584.0585, 3823.081313, 7474.291875, 12502.38375, 18599.43938, 25306.11, 32141.4075,
             38693.30063, 44660.44125, 49856.95688, 54196.56563, 57668.11875, 60311.8125, 62198.775, 63415.74375, 64054.18125,
             64203.1875, 63945.675, 63355.78125, 62498.53125, 61429.725, 60196.44375, 58837.89375, 57386.5875, 55868.895,
             54306.19125, 52715.50313, 51110.30813, 49501.15875, 47896.07063, 46301.19188, 44720.9775, 43158.61125, 41616.24188,
             40095.19688, 38596.1625, 37119.33563, 35664.525, 34231.28625, 32818.93313, 31426.68938, 30053.64938, 28698.89063,
             37447.29563, 26040.45375, 41795.865, 23444.04375, 22167, 20903.0625, 19651.56188, 18411.91313, 17183.61,
             15966.19688, 19706.72063, 13562.7525, 13639.4325, 11026.90125, 9899.420625, 8775.765, 7656.519375, 6542.29125,
             5433.723, 4331.474438, 3236.24475, 2148.75225, 1069.74675, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
             0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
             0, 0, 0 },
            { 0, 0, 0, 0, 0, 4.67184E-31, 2.32589E-17, 9.19676E-10, 0.000147815, 0.199745438, 1.861071188, 94.18365, 90.8886375,
             451.0888313, 1499.203125, 3741.694313, 7571.1825, 13096.78875, 20111.45625, 28177.71188, 36759.11063, 45336.13875,
             53479.15875, 60878.475, 67343.68125, 72786.09375, 77194.6875, 80612.83125, 83117.64375, 84805.03125, 85777.425,
             86136.8625, 85979.7, 85394.1375, 84458.8125, 83242.74375, 81805.21875, 80196.8625, 78460.36875, 76631.23125,
             74738.86875, 72807.46875, 70856.55, 68901.80625, 66955.78125, 65028.2625, 63126.675, 61256.75625, 59422.66875,
             57627.28125, 55872.35438, 54158.97938, 52487.51625, 50857.7625, 49269.16125, 47720.80125, 46211.535, 99403.70625,
             43304.86125, 134520.4125, 40537.28813, 39201.6825, 37896.06375, 36618.83438, 35368.45313, 34143.39, 32942.19938,
             58749.46875, 30605.8725, 36364.59563, 26850.68438, 25895.57063, 24945.2325, 23999.50125, 23058.18563, 22121.11688,
             21188.14875, 20259.14625, 19334.00813, 18412.63313, 17494.95375, 16580.91938, 15670.5075, 14763.71813, 13860.5625,
             12961.08563, 12065.34375, 11173.42688, 10285.43625, 9401.495625, 8521.75125, 7646.360625, 6775.515, 5909.41125,
             5048.271, 4192.327125, 3341.836125, 2497.065188, 1658.2995, 825.84, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
             0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 4.10399E-31, 2.02922E-17, 8.01124E-10, 0.000143153, 0.199228331, 1.751829188, 97.73454375,
             81.50416875, 407.7385313, 1373.241938, 3490.070063, 7216.92, 12783.49875, 20114.7075, 28867.93313, 38537.51063,
             48570.98625, 58459.8375, 67792.6125, 76274.775, 83724.13125, 90054.3375, 95253.4125, 99362.3625, 102456.9,
             104632.3688, 105993.225, 106645.05, 106689.4875, 106221.2625, 105326.325, 104081.4563, 102553.875, 100802.0813,
             98876.30625, 96819.3, 94667.11875, 92450.025, 90193.10625, 87917.175, 85639.1625, 83372.79375, 81129.15,
             78916.89375, 76742.83125, 74612.025, 72528.35625, 70494.46875, 68511.99375, 66582, 64704.825, 62880.1875,
             182151.5625, 59386.275, 263159.4375, 56092.10625, 54516.49875, 52986.4425, 51500.28375, 50056.27313, 48652.68938,
             47287.7775, 106138.2938, 44667.0675, 58802.175, 38348.3475, 37429.30688, 36520.62188, 35621.98875, 34733.10938,
             33853.64063, 32983.26188, 32121.60188, 31268.34, 30423.105, 29585.55375, 28755.35438, 27932.175, 27115.70063,
             26305.605, 25501.60125, 24703.39688, 23910.73875, 23123.36813, 22341.0375, 21563.53875, 20790.6525, 20022.19875,
             19257.9975, 18497.89125, 17741.73938, 16989.41813, 16240.80938, 15495.8175, 14754.375, 14016.40313, 13281.85125,
             12550.68563, 11822.87813, 11098.42875, 10377.32063, 9659.5875, 8945.24625, 8234.330625, 7526.908125, 6823.02375,
             6122.761875, 5426.196188, 4733.425125, 4044.54825, 3359.679188, 2678.939438, 2002.458375, 1330.3755, 662.8381875,
             0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 3.6923E-31, 1.81672E-17, 7.14082E-10, 0.000139696, 0.196480631, 1.612499625, 97.69291875,
             74.09300625, 371.28645, 1255.96575, 3217.355438, 6730.0875, 12096.0675, 19353.465, 28276.47, 38443.29188,
             49333.51688, 60419.08125, 71230.3875, 81393.13125, 90639.45, 98802.5625, 105802.0875, 111625.2563, 116308.9125,
             119923.2, 122558.5125, 124315.3688, 125297.4375, 125605.6875, 125335.9688, 124576.875, 123408.3938, 121901.85,
             120120.4688, 118119.2063, 115945.7063, 113640.8063, 111239.4938, 108771.3563, 106261.2563, 103730.0625, 101195.1,
             98670.54375, 96168.15, 93697.2, 91265.2875, 88878.2625, 86540.68125, 84255.8625, 82026.225, 79853.34375,
             281104.2563, 75681, 419499.3938, 71739.95625, 69854.79375, 68025.31875, 66250.18125, 64528.03125, 62857.4625,
             61236.84375, 161534.925, 58138.7625, 82745.2125, 48225.9825, 47266.11, 46319.42813, 45386.00438, 44465.7375,
             43558.52625, 42664.23, 41782.59563, 40913.40375, 40056.38438, 39211.2, 38377.575, 37555.14375, 36743.55188,
             35942.47875, 35151.54188, 34370.3925, 33598.64813, 32835.97688, 32082.01875, 31336.41375, 30598.83563, 29868.9525,
             29146.41563, 28430.94375, 27722.21063, 27019.935, 26323.83, 25633.62, 24949.04625, 24269.8725, 23595.84,
             22926.75188, 22262.37188, 21602.50875, 20946.97688, 20295.585, 19648.18125, 19004.59688, 18364.69688, 17728.34625,
             17095.41563, 16465.7925, 15839.38125, 15216.08063, 14595.81188, 13978.50188, 13364.08313, 12752.49938, 12143.70563,
             11537.65688, 10934.33625, 10333.70438, 9735.755625, 9140.484375, 8547.890625, 7957.96875, 7370.7525, 6786.253125,
             6204.493125, 5625.511875, 5049.34875, 4476.04875, 3905.6625, 3338.2485, 2773.8675, 2212.588125, 1654.48125,
             1099.625625, 548.1029813 }
        };
        private static double[] CU_1MM_ATTEN_SPECTRUM = new double[] {
            1000, 1000, 1000, 193.4464, 150.4249961, 119.561434, 96.79422044, 79.59941749, 66.3488, 55.64065729, 47.16144877,
            40.35408234, 34.8216294, 30.27584, 26.42809052, 23.21571081, 20.51164683, 18.21832117, 16.25992341, 14.57692774,
            13.12213797, 11.85779633, 10.75344398, 9.78432, 8.922312288, 8.160108622, 7.483554528, 6.880850381, 6.342105058,
            5.858983984, 5.424429634, 5.032438106, 4.677879404, 4.356352, 4.067065957, 3.803282769, 3.562224757, 3.341471864,
            3.138908827, 2.952681114, 2.781158016, 2.622901614, 2.476640584, 2.341248, 2.218726438, 2.104812436, 1.998752763,
            1.899872625, 1.807566235, 1.721288674, 1.640548825, 1.564903238, 1.493950787, 1.427328, 1.368218121, 1.312458494,
            1.259809696, 1.210052843, 1.162987548, 1.118430098, 1.076211845, 1.036177768, 0.998185194, 0.962102654, 0.927808856,
            0.895191771, 0.864147812, 0.834581089, 0.806402743, 0.779530352, 0.753887379, 0.72940269, 0.706010106, 0.683648,
            0.664528752, 0.646169061, 0.628530031, 0.611575196, 0.595270339, 0.579583329, 0.564483971, 0.549943872, 0.535936311,
            0.522436123, 0.509419595, 0.496864366, 0.484749338, 0.473054588, 0.461761296, 0.450851668, 0.440308876, 0.430116992,
            0.42026093, 0.4107264, 0.403469395, 0.396410229, 0.389541648, 0.38285673, 0.376348868, 0.370011753, 0.363839356,
            0.357825916, 0.351965924, 0.34625411, 0.340685431, 0.335255057, 0.329958365, 0.324790924, 0.319748488, 0.314826984,
            0.310022507, 0.305331308, 0.300749792, 0.296274503, 0.291902125, 0.287629468, 0.28345347, 0.279371184, 0.275379776,
            0.271476521, 0.267658794, 0.263924071, 0.260269919, 0.256693996, 0.253194043, 0.249767886, 0.246413427, 0.243128643,
            0.239911583, 0.236760365, 0.233673171, 0.230648248, 0.227683902, 0.224778496, 0.221930451, 0.21913824, 0.216400385,
            0.21371546, 0.211082084, 0.208498922
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="voltage"></param>
        /// <param name="current"></param>
        /// <param name="thickness"></param>
        /// <param name="distance"></param>
        /// <param name="time"></param>
        /// <param name="pixelArea"></param>
        /// <returns></returns>
        public static int EstimateFlux(int voltage, int current, double thickness, double distance, int time = 1, int pixelArea = 1)
        {
            var voltageIndex = TUBE_SPECTRA_VOLTAGES.FindIndex(x => x == voltage);
            if (voltageIndex > 0)
            {
				var temp = (Array)TUBE_SPECTRA.PartOf(new SliceIndex?[] { new SliceIndex(voltageIndex), null }!);
				var spectrum = CU_1MM_ATTEN_SPECTRUM.Multip(-1 * thickness).Exp();
				spectrum = temp.Multip(spectrum);
				var result = (double)spectrum.NanSum() * Math.Pow(1 / (distance / 100), 2) * time * current * pixelArea;
				return (int)result;
			}
            else
                return -1;
        }
/// <summary>
        /// 
        /// </summary>
        /// <param name="ticks"></param>
        /// <param name="ratios"></param>
        /// <param name="valueRef"></param>
        /// <returns></returns>
        public static double GetValueFromOddRange(int[] ticks, double[] ratios, double valueRef)
        {
            double result = 0, value = valueRef, temp = 0;

            if (ticks != null && valueRef != 0)
            {
                if (value > ticks.Max()) return ticks.Max() + 1;

                for (int k = 1; k < ticks.Length; k++)
                {
                    var diff = ticks[k] - ticks[k - 1];
                    if (value > diff && value > 0)
                    {
                        temp = diff;
                    }
                    else
                    {
                        temp = value;
                    }
                    value -= temp;

                    result += temp * ratios[k - 1];
                }
            }

            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static (Dictionary<int, List<dynamic>>, Array) CalculateCounting ()
        {
            var ncp_info = new Dictionary<int, List<dynamic>>();
            Array ncps = Array.CreateInstance(typeof(bool), Constants.NUM_ROWS, Constants.NUM_COLS);



            return (ncp_info, ncps);
        }
		
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static object ConvertTo<T>(dynamic input)
        {
            try
            {
                object? result = default(T);
                bool isValid = false;
                var strTypeName = input.GetType().Name;
                switch (strTypeName)
                {
                    case "Int16":
                    case "Int32":
                    case "Int64":
                        {
                            int temp;
                            isValid = int.TryParse(input.Value.ToString(), out temp);
                            if(isValid)
                                result = strTypeName == typeof(T).Name ? temp : System.Convert.ToInt32(temp);
                            break;
                        }
                    case "Double":
                        {
                            double temp;
                            isValid = double.TryParse(input.Value.ToString(), out temp);
                            if (isValid)
                                result = strTypeName == typeof(T).Name ? temp : System.Convert.ToDouble(temp);
                            break;
                        }
                    case "String":
                        {
                            if (strTypeName == typeof(T).Name)
                                result = input.Value;
                            break;
                        }
                    case "JValue":
                        {
                            double temp;
                            isValid = double.TryParse(input.Value.ToString(), out temp);
                            if (isValid)
                                result = new double[] { (double)input.Value };
                            else
                                result = new double[0];
                            break;
                        }
                    case "JArray":
                        {
                            result = input.ToObject<T[]>();
                            break;
                        }
                }
                return result!;
            }
            catch (Exception)
            {
                return default(T)!;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetJsonValuebyKey(string content, string path)
        {
            string result = "";
            Dictionary<string, dynamic> objNode = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(content)!;
            var nodes = path.Split('.').ToList();

            if (objNode != null && nodes.Count > 0)
            {
                if (objNode.Keys.Contains(nodes[0]))
                {
                    string newContent = objNode[nodes[0]].ToString();
                    nodes.RemoveAt(0);

                    if (nodes.Count > 0)
                    {
                        result = GetJsonValuebyKey(newContent, string.Join(".", nodes.ToArray()));
                    }
                    else
                        result = newContent;
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string GetTestTypeName(List<int> list)
        {
            var result = new List<string>();

            foreach (var item in list)
            {
                result.Add(((enmTestType)item).ToString().ToUpper());
            }
            return string.Join(",", result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetServerDateFormat(string date)
        {
            string result = "";
            var elements = date.Split('-');

            if (elements.Any<string>())
            {
                result = $"{System.Convert.ToInt16(elements[1])},{System.Convert.ToInt16(elements[2])},{System.Convert.ToInt16(elements[0])}";
            }
            return result;
        }

		#endregion

		#region "Functions for arrays"

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arrays"></param>
        /// <returns></returns>
		public static int[] GetMaxDimensions(Array[] arrays)
		{
			var result = new int[arrays[0].GetDimensions().Length];
			for (int k = 0; k < arrays.Length; k++)
			{
				for (int i = 0; i < result.Length; i++)
				{
					if (result[i] < arrays[k].GetLength(i))
						result[i] = arrays[k].GetLength(i);
				}
			}
			return result;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arrays"></param>
        /// <returns></returns>
		public static Array ConvertToArray(Array[] arrays)
		{
			var numElems = arrays.Length;
			var dims = GetMaxDimensions(arrays).ToList();
			dims.Insert(0, numElems);
			var result = Array.CreateInstance(arrays[0].GetType().GetElementType()!, dims.ToArray());
			var indexes = new int?[dims.Count];

			for (int k = 0; k < arrays.Length; k++)
			{
				indexes[0] = k;
				result.SetValue(arrays[k], indexes);
			}
			return result;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static Array AccumulateFrames(Array array, int factor)
        {
            Array? result = null;
            var maxlen = System.Convert.ToInt16(array.GetLength(0) / factor) * factor;

            var old_shape = array.GetDimensions().ToList();
            old_shape.RemoveAt(0);
            var new_shape = new List<int>() { factor, maxlen / factor };
            new_shape.AddRange(old_shape);

            if (maxlen != array.GetLength(0))
                result = (Array)array.PartOf(new SliceIndex?[] { new SliceIndex(0, maxlen), null, null, null }!);
            else
                result = array;

            result = result.Reshape(new_shape.ToArray(), true);
            result = (Array)result.NanSum(0);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="ncp_threshold"></param>
        /// <param name="num_repeats"></param>
        /// <param name="views_per_repeat"></param>
        /// <param name="window_offset"></param>
        /// <param name="window"></param>
        /// <param name="d_window"></param>
        /// <param name="window_threshold"></param>
        /// <returns></returns>
        public static (Array, Array) CalculateDNumberValues(Array array, double[] ncp_threshold, int num_repeats, int views_per_repeat, int window_offset = 0, int window = 1200, int d_window = 200, double window_threshold = 0.9)
        {
            var indexes = array.GetDimensions().ToList();
            indexes.Insert(0, num_repeats);

            Array windows = Array.CreateInstance(typeof(double), indexes.ToArray());
            var d_number = Array.CreateInstance(typeof(double), new int[] { 3, Constants.NUM_ROWS, Constants.NUM_COLS });
            var d_number_ncps = Array.CreateInstance(typeof(bool), new int[] { 3, Constants.NUM_ROWS, Constants.NUM_COLS });

            if (array.GetDimensions().Length == 3)
            {
                for (int k = 0; k < num_repeats; k++)
                {
                    var temp = (Array)array.PartOf(new SliceIndex?[] { new SliceIndex(k * views_per_repeat, (k + 1) * views_per_repeat), null, null }!);
                    windows.SetValue(temp, new int?[] { k, null, null, null });
                }
                windows = (Array)windows.NanMean(0);

                var median = (Array)windows.NanMedian(new int[] { 1, 2 });
                var start = (int)median.IsGreaterThan(window_threshold * (double)median.NanMax(), false).ArgMax() + window_offset;

                windows = (Array)windows.PartOf(new SliceIndex?[] { new SliceIndex(start, start + window), null, null }!);
                median = (Array)median.PartOf(new SliceIndex?[] { new SliceIndex(start, start + window) }!);
                var mean = median.NanMean(0);
                var normalized = windows.Multip(mean).Div(median.AddDimension<double>(true).AddDimension<double>(true));

                mean = normalized.NanMean(0);
                var mean_w1 = (Array)((Array)normalized.PartOf(new SliceIndex?[] { new SliceIndex(null, d_window), null, null }!)).NanMean(0);
                var mean_w2 = (Array)((Array)normalized.PartOf(new SliceIndex?[] { new SliceIndex(window / 2 - d_window / 2, window / 2 + d_window / 2), null, null }!)).NanMean(0);
                var mean_w3 = (Array)((Array)normalized.PartOf(new SliceIndex?[] { new SliceIndex(normalized.GetLength(0) - d_window, normalized.GetLength(0)), null, null }!)).NanMean(0);


                d_number.SetValue(mean_w1.Sub(mean_w2), new int?[] { 0, null, null });
                d_number.SetValue(mean_w1.Sub(mean_w3), new int?[] { 1, null, null });
                d_number.SetValue(mean_w2.Sub(mean_w3), new int?[] { 2, null, null });
                d_number = d_number.Div(mean);

                d_number_ncps = d_number.Abs(0).Boolean_IsGreaterThan(ncp_threshold);

            }
            return (d_number, d_number_ncps);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static Array MaxDeviation(Array array, int axis = -1)
        {
            var arrMean = (Array)array.NanMean(axis);
            var result = Array.CreateInstance(array.GetType().GetElementType()!, array.GetDimensions());

            //result = array.Sub(arrMean);
            //for (int k=0; k<array.GetLength(1); k++)
            Parallel.For(0, array.GetLength(1), k =>
            {
                var temp = (Array)array.PartOf(new SliceIndex?[] { null, new SliceIndex(k), null, null }!);
                temp = temp.Sub(arrMean);
                result.SetValue(temp, new int?[] { null, k, null, null });
            });

            result = result.Abs(0);
            var tempMax = (Array)result.NanMax(axis);
            result = tempMax.Div(arrMean);

            return result;    // Not needed: Squeeze(axis);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="n"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static Array MovingMean(Array array, int n, int axis = -1)
        {
            var newDims = array.GetDimensions().ToArray();
            newDims[axis]++;

            var newArray = Array.CreateInstance(array.GetType().GetElementType()!, newDims);
            var cumsum = (Array)array.NanCumSum(axis);
            cumsum = cumsum.MoveAxis(axis, 0);

            var temp1 = (Array)cumsum.PartOf(new SliceIndex?[] { new SliceIndex(n, null), null, null, null }!);
            var temp2 = (Array)cumsum.PartOf(new SliceIndex?[] { new SliceIndex(null, cumsum.GetLength(0) - n), null, null, null }!);
            var mean = temp1.Sub(temp2).Div(n);
            var result = mean.MoveAxis(0, axis);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="norm_mask_range"></param>
        /// <returns></returns>
        public static (Array, Array) NormalizeArray(Array array, double norm_mask_range = 0.2)
        {
            var numBins = array.GetLength(0);
            var norm_mask_mean = (Array)array.NanMean(1);
            var norm_mask_median = (double[])norm_mask_mean.NanMedian(new int[] { 1, 2 });
            var norm_mask = Array.CreateInstance(typeof(bool), norm_mask_mean.GetDimensions());
            var norm = Array.CreateInstance(typeof(double), new int[] { array.GetLength(0), array.GetLength(1) });

            //for (int k = 0; k < numBins; k++)
            Parallel.For(0, numBins, k =>
            {
                var temp = (Array)norm_mask_mean.PartOf(new SliceIndex?[] { new SliceIndex(k), null, null }!);
                var temp1 = temp.Boolean_IsGreaterThan(norm_mask_median[k] * (1 - norm_mask_range));
                var temp2 = temp.Boolean_IsLesserThan(norm_mask_median[k] * (1 + norm_mask_range));
                var temp3 = ArrayExtensions.Logical_And(temp1, temp2);
                norm_mask.SetValue(temp3, new int?[] { k, null, null });

                var temp4 = (Array)array.PartOf(new SliceIndex?[] { new SliceIndex(k), null, null, null }!);
                temp4 = (Array)temp4.Transpose(new int[] { 1, 2, 0 });
                temp4 = temp4.Visible(temp3);
                temp4 = (Array)temp4.NanMean(0);
                norm.SetValue(temp4, new int?[] { k, null });
            });

            // norm /= norm.mean(axis=1, keepdims=True)
            var arrMean = (double[])norm.NanMean(1);
            //for (int k = 0; k < numBins; k++)
            Parallel.For(0, numBins, k =>
            {
                var temp5 = ((Array)norm.PartOf(new SliceIndex?[] { new SliceIndex(k), null }!)).Div(arrMean[k]);
                norm.SetValue(temp5, new int?[] { k, null });
            });

            return (norm, norm_mask);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static (Array, Array, Array, Array) CalculateNormalizationValues(Array array)
        {
            // integrate_bins:
            int axis = 0;
            var cc_open = (Array)array.Flip(axis);
            cc_open = (Array)cc_open.NanCumSum(axis);
            cc_open = (Array)cc_open.Flip(axis);

            var numBins = array.GetLength(0);
            var numViews = array.GetLength(1);
            double sample_period = 0.025400000000000002, STABILITY_NORMALIZATION_RANGE = 0.2;

            var sumcc = (Array)((Array)array.PartOf(new SliceIndex?[] { new SliceIndex(1, null), null, null }!)).NanSum(axis);
            var sumcc_median = (Array)sumcc.NanMedian(new int[] { 1, 2 });
            var start = (int)sumcc_median.IsGreaterThan(0.9 * (double)sumcc_median.NanMax(), false).ArgMax() + (int)(10 / sample_period);
            var end = start + (int)(60 / sample_period);
            var temp = (Array)cc_open.PartOf(new SliceIndex?[] { null, new SliceIndex(start, end), null, null }!);
            var (norm, norm_mask) = NormalizeArray(temp, STABILITY_NORMALIZATION_RANGE);

            var cc_norm = Array.CreateInstance(typeof(double), temp.GetDimensions());
            var mNorm = (double[,])norm;

            //for (int j=0; j<norm.GetLength(0); j++)
            Parallel.For(0, norm.GetLength(0), j =>
            {
                for (int i = 0; i < norm.GetLength(1); i++)
                {
                    var temp1 = (Array)temp.PartOf(new SliceIndex?[] { new SliceIndex(j), new SliceIndex(i), null, null }!);
                    cc_norm.SetValue(temp1.Div(mNorm[j, i]), new int?[] { j, i, null, null });
                }
            });
            var norm_sum = (Array)cc_norm.NanSum(new int[] { 2, 3 });

            return (norm, norm_mask, cc_norm, norm_sum);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static (Array, Array, Array, Array) CalculateMD1XValues(Array array)
        {
            var numBins = array.GetLength(0);
            var numViews = array.GetLength(1);
            var STABILITY_MD1F_THRESHOLD = new double[] { double.NaN, 2, 2, 2, 2, double.NaN };
            var STABILITY_MD1S_THRESHOLD = new double[] { double.NaN, 0.5, 0.5, 0.6, 0.8, double.NaN };
            double view_period = 0.025;

            var md1f = MaxDeviation(array, 1).Multip(100);
            var md1s = MaxDeviation(MovingMean(array, (int)(1 / view_period), 1), 1).Multip(100);
            var md1f_ncps = Array.CreateInstance(typeof(bool), md1f.GetDimensions());
            var md1s_ncps = Array.CreateInstance(typeof(bool), md1s.GetDimensions());

            //for (int k = 0; k < numBins; k++)
            Parallel.For(0, numBins, k =>
            {
                var temp1 = (Array)md1f.PartOf(new SliceIndex?[] { new SliceIndex(k), null, null }!);
                md1f_ncps.SetValue(temp1.Boolean_IsGreaterThan(STABILITY_MD1F_THRESHOLD[k]), new int?[] { k, null, null });

                var temp2 = (Array)md1s.PartOf(new SliceIndex?[] { new SliceIndex(k), null, null }!);
                md1s_ncps.SetValue(temp2.Boolean_IsGreaterThan(STABILITY_MD1S_THRESHOLD[k]), new int?[] { k, null, null });
            });
            return (md1f, md1s, md1f_ncps, md1s_ncps);
        }


        #endregion

    }
}