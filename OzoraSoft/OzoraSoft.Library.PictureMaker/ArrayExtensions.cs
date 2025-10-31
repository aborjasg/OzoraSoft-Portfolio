using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;
using OzoraSoft.Library.PictureMaker;
using OzoraSoft.Library.Enums.PictureMaker;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OzoraSoft.Library.PictureMaker
{
    /// <summary>
    /// C# array extentions.
    /// most methods are implementations of Numpy (Python)
    /// </summary>
    public static class ArrayExtensions
	{
		/// <summary>
		/// Gets value(s) from specified indices.
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="indices">Indices that map to elements in this.array</param>
		/// <returns>Returns the values from the specified indices</returns>
		public static Array GetValue(this Array array, int?[] indices)
		{
			int selectedIndex = -1;

			for (int k = 0; k < indices.Length; k++)
			{
				if (indices[k] == null)
				{
					selectedIndex = k;
					break;
				}
			}

			int lenD1 = array.GetLength(selectedIndex);
			double[] result = new double[lenD1];

			for (int d1 = 0; d1 < lenD1; d1++)
			{
				result[d1] = (double)array.GetValue(d1, (int)indices[1]!, (int)indices[2]!, (int)indices[3]!)!;
			}
			return result;
		}

		/// <summary>
		/// An extension for Array class.
		/// Set value for multidimensional array by using a singal index
		/// </summary>
		/// <example>
		/// A[2,3,4], dimen[2,3,4]<br/>
		/// index = 6<br/>	
		/// --> A[0,1,2] bMemorySequence = true<br/>
		///	--> A[2,0,0] bMemorySequence = false<br/>
		/// </example>
		/// <param name="array">this.array, input array</param>
		/// <param name="index">index in long</param>
		/// <param name="data">data to be set</param>
		/// <param name="bMemorySequence">false: the data sequence starts from dim[0], when we read the mat file, we must use this option. true : data sequence starts from dim[0]</param>
		/// <returns>void</returns>
		public static void SetValue(this Array array, long index, object data, bool bMemorySequence = true)
		{
			//make inexes for this index
			var anIndexes = array.GetIndices(index, bMemorySequence);
			//set this value
			array.SetValue(data, anIndexes);
		}

		/// <summary>
		/// Set values from source array to this.array. 
		/// </summary>
		/// <example>
		/// <code>
		/// data.SetValue(src, new int?[]{null, -1, null, null})	// Python equivalent: data[:,-1,:,:] = src
		/// // in this case, this.array must be a 4 dimensional array
		/// // and src must be a 3 dimension array.
		/// // All numbers of each dimension must be the same between these to arrays.
		/// data.SetValue(src, new int?[]{0, -1, 3, 2})	// Python equivalent Python data[0,-1,3,2] = src
		/// </code>
		/// </example>
		/// <param name="array">this.array, input array</param>
		/// <param name="src">Source array</param>
		/// <param name="indices">indices to set value in this.array from src. Null means for all members to be set. Ex. [null, -1, null, null], [0,2,1,3]</param>
		/// <returns>Returns true if values were successfully set. Returns false if dimensions between this array and src are not correct</returns>
		public static bool SetValue(this Array array, Array src, int?[] indices)
		{
			bool bRcd = false;
			if (array != null && src != null)
			{
				int[] srcDims = src.GetDimensions();
				int[] thisDims = array.GetDimensions();
				if (indices == null)
				{//copy all elements from src to this array
					if (srcDims.Length == thisDims.Length)
					{
						bRcd = true;
						for (int i = 0; i < srcDims.Length; i++)
						{
							if (srcDims[i] != thisDims[i])
							{
								bRcd = false;
								break;
							}
						}
						if (bRcd)
						{// same dimensions, so copy only
							Array.Copy(src, array, array.Length);
						}
					}
				}
				else
				{
					if (indices.Length == thisDims.Length)
					{//must be same
						int nSrc = 0;
						var thisIndexes = new int[thisDims.Length];
						var thisRangs = new SliceIndex[thisDims.Length];
						bRcd = true;
						for (int i = 0; i < indices.Length; i++)
						{
							thisRangs[i] = new SliceIndex()
							{
								OriginalLength = thisDims[i]    //must set this first
							};
							if (indices[i] == null)
							{//need element for this dimension from src
								if (nSrc < srcDims.Length &&  //has related src array
									srcDims[nSrc] == thisDims[i]) // the src array dimension number is same as this array's
								{
									thisIndexes[i] = 0;
									thisRangs[i].Start = 0;
									thisRangs[i].Stop = thisDims[i];
									nSrc++;
								}
								else
								{//src array numbers of this dimensions is not same as this array's
									bRcd = false;
									break;
								}
							}
							else
							{
								int ntemp = (int)indices[i]!;
								if (ntemp < 0)
								{// -1, -2.....
									ntemp = thisDims[i] + ntemp;
									if (ntemp < 0)
									{//recover, if has a big minus
										ntemp = 0;
									}
								}
								else if (ntemp >= thisDims[i])
								{
									ntemp = thisDims[i] - 1;
								}
								//now 0 <= ntemp < thisDims[i]
								thisIndexes[i] = ntemp;
								thisRangs[i].Start = ntemp;
								thisRangs[i].Stop = ntemp + 1;
							}
						}
						if (bRcd)
						{//set src data to array
							for (var i = 0; i < src.Length; i++)
							{
								dynamic tempData = src.GetAt(i);
								array.SetValue((object)tempData, thisIndexes);
								IncrementIndex(thisIndexes, thisRangs);
							}
						}
					}
				}
			}
			return bRcd;
		}

		/// <summary>
		/// Set value(s) to this array by specify a slice index
		/// </summary>
		/// <example>
		/// <code>
		/// var arr = new double[,,]{{{ 0  1  2  3  4},	//original array [3,2,5]
		///	                          { 5  6  7  8  9}},
		///	                         {{10 11 12 13 14},
		///	                          {15 16 17 18 19}},
		///	                         {{20 21 22 23 24},
		///	                          {25 26 27 28 29}}}
		///	var src3 = new double[]{1000, 1001, 1002};
		///	var src2 = new double[]{100, 101};
		/// var src5 = new double[]{1000, 1001, 1002, 1003, 1004};
		/// var src35 = new double[,]{{10000, 10001, 10002, 10003, 10004},{20000, 20001, 20002, 20003, 20004},{30000, 30001, 30002, 30003, 30004}};
		///	var src32 = new double[,]{{100,101},{202,203},{304,305}};
		///	var src25 = new double[,]{{200, 201, 202, 203, 204},{300, 301, 302, 303, 304}};
		///	short scalar = 12;	//a scalar
		///	bool bDone = false; //for checking the result
		///	// Case 1
		///	bDone = arr.SetValue2(src3,new int?[]{null,-1,-1});
		///	//          {null,-1,-1}          {null,null,null},{null null,-1},{null,-1,null},{non-null,any,any}
		///	//[[[   0    1    2    3    4]                   not working-->bDone == false
		/// //  [   5    6    7    8 1000]]
		/// // [[  10   11   12   13   14]
		/// //  [  15   16   17   18 1001]]
		/// // [[  20   21   22   23   24]
		/// //  [  25   26   27   28 1002]]]
		/// // Case 2
		///	bDone = arr.SetValue2(src2,new int?[]{-1,null,-1});
		///	//     {null,null,-1}                {-1,null,-1}           {null,null,null},{-1, null,null},{any,non-null,any}
		///	//[[[  0   1   2   3 100]      [[[  0   1   2   3   4]             not working-->bDone == false
		/// //  [  5   6   7   8 101]]	     [  5   6   7   8   9]]
		/// // [[ 10  11  12  13 100]	    [[ 10  11  12  13  14]
		/// //  [ 15  16  17  18 101]]	     [ 15  16  17  18  19]]
		/// // [[ 20  21  22  23 100]	    [[ 20  21  22  23 100]
		/// //  [ 25  26  27  28 101]]]	     [ 25  26  27  28 101]]]
		/// // Case 3
		///	bDone = arr.SetValue2(src5,new int?[]{-1,-1,null}); //first null is in 
		/// //       {null,null,null}               {null,-1,null}                    {-1,null,null}                    {-1,-1,null}               {any,any,non-null}
		/// //[[[1000 1001 1002 1003 1004]    [[[   0    1    2    3    4]    [[[   0    1    2    3    4]     [[[   0    1    2    3    4]     not working--> bDone=false
		/// //  [1000 1001 1002 1003 1004]]     [1000 1001 1002 1003 1004]]	    [   5    6    7    8    9]]	     [   5    6    7    8    9]]	  
		/// // [[1000 1001 1002 1003 1004]	    [[  10   11   12   13   14]	   [[  10   11   12   13   14]	    [[  10   11   12   13   14]	  
		/// //  [1000 1001 1002 1003 1004]]     [1000 1001 1002 1003 1004]]	    [  15   16   17   18   19]]	     [  15   16   17   18   19]]	  
		/// // [[1000 1001 1002 1003 1004]	    [[  20   21   22   23   24]	   [[1000 1001 1002 1003 1004]	    [[  20   21   22   23   24]	  
		/// //  [1000 1001 1002 1003 1004]]]    [1000 1001 1002 1003 1004]]]    [1000 1001 1002 1003 1004]]]     [1000 1001 1002 1003 1004]]]  
		///	// Case 4
		///	bDone = arr.SetValue2(src35,new int?[]{null,-1,null});
		///	//          {null,-1,null}			     {null, -1,-1},{-1,-1,null},{-1,-1,-1},{any,null,any}
		///	//[[[    0     1     2     3     4]         not working-->bDone == false
		/// //  [10000 10001 10002 10003 10004]]
		/// // [[   10    11    12    13    14]
		/// //  [20000 20001 20002 20003 20004]]
		/// // [[   20    21    22    23    24]
		/// //  [30000 30001 30002 30003 30004]]]
		///	// Case 5
		///	bDone = arr.SetValue2(src32,new int?[]{null,null,-1});
		///	//    {null,null,-1}	       {null,null,null},{null, -1,-1},{-1,-1,null},{-1, -1, -1},{any,non-null,any}
		///	//[[[  0   1   2   3 100]          not working-->bDone == false
		/// //  [  5   6   7   8 101]]
		/// // [[ 10  11  12  13 202]
		/// //  [ 15  16  17  18 203]]
		/// // [[ 20  21  22  23 304]
		/// //  [ 25  26  27  28 305]]]
		///	// Case 6
		///	bDone = arr.SetValue2(src25,new int?[]{-1,null,null});
		///	//     {null,null,null}          {-1,null,null}                 {any,any,non-null},{any,non-null,any}
		///	//[[[200 201 202 203 204]    [[[  0   1   2   3   4]               not working-->bDone == false
		/// //  [300 301 302 303 304]]	   [  5   6   7   8   9]]
		/// // [[200 201 202 203 204]	  [[ 10  11  12  13  14]
		/// //  [300 301 302 303 304]]	   [ 15  16  17  18  19]]
		/// // [[200 201 202 203 204]	  [[200 201 202 203 204]
		/// //  [300 301 302 303 304]]]	   [300 301 302 303 304]]]
		/// // Case 7
		/// bDone = arr.SetValue2(scalar);// set all elements to a scalar (12)
		/// bDone = arr.SetValue2(scalar, new int?[]{null,null,0});// set part of elements to a scalar(2)
		///	//     null				   {null,null,0}        
		///	//[[[12 12 12 12 12]    [[[ 12  1   2   3   4] 
		/// //  [12 12 12 12 12]]	   [12   6   7   8   9]]
		/// // [[12 12 12 12 12]	  [[12  11  12  13  14]
		/// //  [12 12 12 12 12]]	   [12  16  17  18  19]]
		/// // [[12 12 12 12 12]	  [[12  21  12  23  24]
		/// //  [12 12 12 12 12]]]	   [12  26  27  28  29]]]
		/// </code>
		/// <code>
		/// #test program in Python
		/// import numpy as np
		/// arr = np.arange(3 * 2 * 5).reshape(3, 2, 5)
		/// print(arr)
		/// src3 = np.array([1000, 1001, 1002])
		/// src2 = np.array([100, 101])
		/// src5 = np.array([1000, 1001, 1002, 1003, 1004])
		/// src35 = np.array([[10000, 10001, 10002, 10003, 10004],[20000, 20001, 20002, 20003, 20004],[30000, 30001, 30002, 30003, 30004]])
		/// src32 = np.array([[100, 101],[202, 203],[304, 305]])
		/// src25 = np.array([[200, 201, 202, 203, 204],[300, 301, 302, 303, 304]])
		/// scalar = 12
		/// print(src5)
		/// #Case 1
		/// # arr[:,-1,-1] = src3 # does not work arr[:,-1,:] = src3
		/// #Case 2
		/// # arr[:,:,-1] = src2
		/// #Case 3
		/// # arr[-1,-1,:] = src5
		/// #Case 4
		/// # arr[:,-1,:] = src35
		/// #Case 5
		/// #arr[:,:, -1] = src32
		/// #Case 6
		/// arr[-1,:,:] = src25
		/// # arr[:,:,:]=0
		/// #Case 7
		/// arr[:,:,:] = scalar
		/// print(arr)
		/// </code>
		/// </example>
		/// <param name="array">this array, it is original array, also it is ouput array</param>
		/// <param name="src">source data to set it to this array, it can a scalar (Ex. int double...) or an array. See example to find out how this array should be </param>
		/// <param name="sliceIndices">slice indices to be used for setting data, null:set all values of this array. not null: the length of this must same as array's dimensions</param>
		/// <returns></returns>
		public static bool SetValue2(this Array array, object src, int?[]? sliceIndices=null)
		{
			bool bRcd = false;
			if (array != null && src != null)
			{
				int[] thisDims = array.GetDimensions();
				Type thisElementType = array.GetType().GetElementType()!;
				//check the src type
				Type oTypeSrc = src.GetType();
				Array? aSrc = null;
				object? vSrc = null;
				int[]? srcDims = null;
				Type? srcElementType = null;
				if (oTypeSrc.IsArray)
				{
					aSrc = (src as Array)!;
					srcDims = aSrc.GetDimensions();
					srcElementType = aSrc.GetType().GetElementType()!;
				}
				else if (src.GetType().IsValueType)
				{
					//change the value type to this array's element type
					vSrc = Convert.ChangeType(src, thisElementType);
				}
				else
				{//do not accept any other
					return bRcd;
				}
				if (aSrc != null ^ vSrc != null)
				{//aSrc or vSrc not null --> only one of them is not null
					if (sliceIndices == null)//||	//caller set this to null
											   //(indices.All(o=>o==null)&&	//if all element of indices are null
											   // ((vSrc!=null)||	// if src is scalar
											   //  ((aSrc!=null)&&	// if src is an array (aSrc)
											   //   (srcDims.Length == thisDims.Length)&&	//aSrc & this array must have same Rank
											   //    srcDims.SequenceEqual(thisDims))))) //aSrc dism must equal thisDims
					{   //copy all elements from src to this array
						if (aSrc != null)
						{   //copy aSrc to this array
							if (srcDims!.Length == thisDims.Length &&
								srcDims.SequenceEqual(thisDims))  //aSrc dism must equal thisDims
							{//total same dimension, check it for indices == null,
								if (thisElementType.Equals(srcElementType))
								{//element type is same, use BlockCopy() for better performance
									Buffer.BlockCopy(aSrc, 0, array, 0, Buffer.ByteLength(array));
								}
								else
								{//element type is different
									int[] thisIndices = new int[thisDims.Length];
									Array.Clear(thisIndices, 0, thisIndices.Length);
									// copy value one by one
									for (var i = 0; i < array.Length; i++)
									{
										array.SetValue(Convert.ChangeType(aSrc.GetValue(thisIndices), thisElementType), thisIndices);
										thisIndices = thisIndices.Increament(thisDims);
									}
								}
								bRcd = true;
							}
						}
						else
						{   //set same value (vSrc) to this array
							//case 1 : for performance check
							//int[] thisIndices = new int[thisDims.Length];
							//Array.Clear(thisIndices, 0, thisIndices.Length);
							//// copy value one by one
							//for (var i = 0; i < array.Length; i++)
							//{
							//	array.SetValue2(vSrc, thisIndices);
							//	thisIndices = thisIndices.Increament(thisDims);
							//}
							//case 2 : for performance check
							//element size
							int nElementSize = Buffer.ByteLength(array) / array.Length;
							var oneElement = Array.CreateInstance(thisElementType, 1);
							oneElement.SetValue(vSrc, 0);
							for (var i = 0; i < array.Length; i++)
							{
								Buffer.BlockCopy(oneElement, 0, array, i * nElementSize, nElementSize);
							}
							bRcd = true;
						}
					}
					else
					{   //try to copy src to specific axis of this array, here 
						if (sliceIndices.Length == thisDims.Length)
						{   //same length with indices & this array dimensions
							if (sliceIndices.NormalizeIndices(thisDims))
							{   //all element value will be >=0&&<thisDims[i]
								if (aSrc != null)
								{   //copy src to sepecific position of this array
									if (srcDims!.Length <= thisDims.Length)
									{   //at least one null in indices, if not src must be scalar--> it means set one element only
										int nNoNull = sliceIndices.Count(o => o == null);
										if (srcDims.Length <= nNoNull)
										{//number of null in indices[] must >= srcDims.Length
											bool bGoodSrcIndices = true;
											// indices of aSrc that map to this array
											//int[] indicesOfSrcDimsMapToThisDims = new int[srcDims.Length];
											int nNullIndex = sliceIndices.Length - 1;
											int nIndexOfSrcDims = srcDims.Length - 1;
											//check srcDims
											for (int i = srcDims.Length - 1; i >= 0; i--)
											{
												nNullIndex = Array.LastIndexOf(sliceIndices, null, nNullIndex);
												if (nNullIndex < 0)
												{
													bGoodSrcIndices = false;
													break;
												}
												if (srcDims[nIndexOfSrcDims] != thisDims[nNullIndex])
												{
													bGoodSrcIndices = false;
													break;
												}
												nIndexOfSrcDims--;
												if (nIndexOfSrcDims < 0)
												{
													break;
												}
												nNullIndex--;
											}
											if (bGoodSrcIndices)
											{
												//make start index of this array
												int[] aIndices = new int[sliceIndices.Length];
												//for (int i = 0; i < indices.Length; i++)
												//{
												//	if (indices[i] == null)
												//	{//null means from 0 to thisDims[i]-1
												//		aIndices[i] = 0;
												//	}
												//	else
												//	{
												//		aIndices[i] = (int)indices[i];
												//	}
												//}
												aIndices.InitializeStartIndices(sliceIndices);
												int[] srcIndices = new int[srcDims.Length];
												Array.Clear(srcIndices);
												//set aSrc to this array
												do
												{
													array.SetValue(Convert.ChangeType(aSrc.GetValue(srcIndices), thisElementType), aIndices);
													srcIndices.Increament(srcDims);
												}
												while (aIndices.Increament(sliceIndices, thisDims));
												bRcd = true;
											}
										}
									}
								}
								else
								{   //set vSrc (scalar) to sepecific position of this array
									//make start index
									int[] aIndices = new int[sliceIndices.Length];
									//for(int i=0;i< indices.Length;i++)
									//{
									//	if (indices[i] == null)
									//	{//null means from 0 to thisDims[i]-1
									//		aIndices[i] = 0;
									//	}
									//	else
									//	{
									//		aIndices[i] = (int)indices[i];
									//	}
									//}
									aIndices.InitializeStartIndices(sliceIndices);
									//set vSrc to this array
									do
									{
										array.SetValue(vSrc, aIndices);
									}
									while (aIndices.Increament(sliceIndices, thisDims));
									bRcd = true;
								}
							}
						}
					}
				}
			}
			return bRcd;
		}

		/// <summary>
		/// Initialize a array indices with a slice indices
		/// </summary>
		/// <param name="array">this array</param>
		/// <param name="sliceIndices">slice indices, some member may be a null</param>
		/// <returns></returns>
		public static bool InitializeStartIndices(this int[] array, int?[] sliceIndices)
		{
			bool bRcd = false;
			if (array != null &&
				sliceIndices != null &&
				array.Length == sliceIndices.Length)
			{
				for (int i = 0; i < sliceIndices.Length; i++)
				{
					if (sliceIndices[i] == null)
					{//null means this index can be from 0 to thisDims[i]-1
						array[i] = 0;
					}
					else
					{
						array[i] = (int)sliceIndices[i]!;
					}
				}
			}
			return bRcd;
		}

		/// <summary>
		/// Converts a long index to an index table of this.array.
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="index">index in long, a serial number of all the elements</param>
		/// <param name="bMemorySequnce">false: data sequnce start from dim[0], when read mat file must use this option. true : data sequnce start from dim[0]</param>
		/// <param name="dims">dims for this array for speed up</param>
		/// <param name="Indices">indices array. for speed up</param>
		/// <returns>Returns an int[] array with an index table of this.array</returns>
		public static int[] GetIndices(this Array array, long index, bool bMemorySequnce = true, int[]? dims=null, int[]? Indices=null)
		{
			int CalcRemainder(int nSize)
			{
				long nIndex = index / nSize;
				long nTemp = index - nIndex * nSize;
				index = nIndex;
				return (int)nTemp;
			};

			int[]? oRcd = Indices;
			//make inexes for this index
			if (oRcd == null)
			{
				oRcd = new int[array.Rank];
			}
			if (dims == null)
			{
				dims = array.GetDimensions();
			}
			if (bMemorySequnce)
			{
				for (int i = oRcd.Length - 1; i >= 0; i--)
				{
					//int nSizes = array.GetLength(i);
					//int nSizes = dims[i];
					// this index range should be 0 --> nSizes -1.
					//long nIndex = index / nSizes;
					//long nRemainder = index - (nIndex * nSizes);
					//index = nIndex;
					//oRcd[i] = (int)nRemainder;
					oRcd[i] = CalcRemainder(dims[i]);
				}
			}
			else
			{//MAT file use this sequnce
				for (int i = 0; i < oRcd.Length; i++)
				{
					//int nSizes = array.GetLength(i);
					//int nSizes = dims[i];
					// this index range should be 0 -- nSizes -1.
					//long nIndex = index / nSizes;
					//long nRemainder = index - (nIndex * nSizes);
					//index = nIndex;
					//oRcd[i] = (int)nRemainder;
					//oRcd[i] = (int)(index % nSizes);
					//index = index / nSizes;
					oRcd[i] = CalcRemainder(dims[i]);
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Get an element at a specified index.
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="index">The idex to retrieve the element</param>
		/// <returns>Returns an object of the element in this.array</returns>
		public static object GetAt(this Array array, long index)
		{
			var anIndexes = array.GetIndices(index, true);
			return array.GetValue(anIndexes)!;
		}

		/// <summary>
		/// Copy the elements from an array to a different type of array, such as <c>int[,,,,...]</c> to <c>double[,,,,....]</c>.
		/// </summary>
		/// <example>
		/// <code>
		/// //Copy to double[,,..], has same dimension as orignal array has
		/// Array oArr = array.Copy&lt;double&gt;();
		/// //copy to double[], a singal dimesion array
		/// Array oArr = array.Copy&lt;double&gt;(true);
		/// </code>
		/// </example>
		/// <typeparam name="T">Element type.</typeparam>
		/// <param name="array">this.array, the array that is to be copied</param>
		/// <param name="copyToOneDim">true: copy it to a one dimensional array T[]</param>
		/// <returns>Return an array copy of type <typeparamref name="T"/> from the given object.</returns>
		public static Array Copy<T>(this Array array, bool copyToOneDim = false)
		{
			//for finding we are using MKL
			//string strMKL = MathNet.Numerics.Providers.LinearAlgebra.LinearAlgebraControl.Provider.ToString();

			Array? oRcd = null;
			if (array != null &&
				array.Length > 0)
			{
				int[] thisDims = array.GetDimensions();
				int[] targetDims;
				if (copyToOneDim)
				{
					oRcd = new T[array.Length];
					targetDims = new int[] { array.Length };
				}
				else
				{
					//source array dimensions
					oRcd = Array.CreateInstance(typeof(T), thisDims);
					targetDims = thisDims;
				}

				//check the item data type
				Type typeArray = array.GetType().GetElementType()!;
				Type typeNew = oRcd.GetType().GetElementType()!; //==typeof(T)
				if (typeArray == typeNew)
				{//same type, so just copy it.
					Buffer.BlockCopy(array, 0, oRcd, 0, Buffer.ByteLength(array));
				}
				else
				{
					int[] TargetIndices = new int[targetDims.Length];
					Array.Clear(TargetIndices, 0, TargetIndices.Length);
					int[] SourceIndices = new int[thisDims.Length];
					Array.Clear(SourceIndices, 0, SourceIndices.Length);
					// first methode (293.695msec), this is simple way and got better performance
					for (var i = 0; i < array.Length; i++)
					{
						//TargetIndices = oRcd.GetIndices(i, true, targetDims, TargetIndices);
						//SourceIndices = array.GetIndices(i, true, thisDims, SourceIndices);
						oRcd.SetValue((T)Convert.ChangeType(array.GetValue(SourceIndices), typeof(T))!, TargetIndices);
						SourceIndices = SourceIndices.Increament(thisDims);
						TargetIndices = TargetIndices.Increament(targetDims);
					}

					//other methode, (257.2, 306.2819msec)
					////int nSrcSize = Buffer.ByteLength(array) / array.Length;
					//int nDstSize = Buffer.ByteLength(oRcd) / oRcd.Length;

					////following code is slow, Array.GetValue(i) takes long time 
					////Array oTemp = Array.CreateInstance(array.GetType().GetElementType()!, array.Length);
					////Buffer.BlockCopy(array,0, oTemp, 0, Buffer.ByteLength(array));
					//////Array.ConvertAll< Array,T >(oTemp, (x) => (T)Convert.ChangeType(x, typeof(T)));
					////for (int i = 0; i < oRcd.Length; i++)
					////{
					////	object srcValue = oTemp.GetValue(i);	// this line take long time
					////	T[] dstValue = new T[] { (T)Convert.ChangeType(srcValue, typeof(T)) }; //this line take long time
					////	Buffer.BlockCopy(dstValue, 0, oRcd, i * nDstSize, nDstSize);
					////}

					//Array.GetValue(nIdex) has bad performance, we have to use arr[] to get the value
					//dynamic srcArray = null;
					//switch (typeArray)
					//{
					//	case Type t when t == typeof(bool): // be carefull, bool in a array only take 1 byte, but normal bool should take 4 bytes
					//		srcArray = new bool[array.Length];
					//		break;
					//	case Type t when t == typeof(char):
					//		srcArray = new char[array.Length];
					//		break;
					//	case Type t when t == typeof(double):
					//		srcArray = new double[array.Length];
					//		break;
					//	case Type t when t == typeof(float):
					//		srcArray = new float[array.Length];
					//		break;
					//	case Type t when t == typeof(Half):
					//		srcArray = new Half[array.Length];
					//		break;
					//	case Type t when t == typeof(long):
					//		srcArray = new long[array.Length];
					//		break;
					//	case Type t when t == typeof(ulong):
					//		srcArray = new ulong[array.Length];
					//		break;
					//	case Type t when t == typeof(int):
					//		srcArray = new int[array.Length];
					//		break;
					//	case Type t when t == typeof(uint):
					//		srcArray = new uint[array.Length];
					//		break;
					//	case Type t when t == typeof(short):
					//		srcArray = new short[array.Length];
					//		break;
					//	case Type t when t == typeof(ushort):
					//		srcArray = new ushort[array.Length];
					//		break;
					//	case Type t when t == typeof(byte):
					//		srcArray = new byte[array.Length];
					//		break;
					//	case Type t when t == typeof(sbyte):
					//		srcArray = new sbyte[array.Length];
					//		break;
					//	default:
					//		Debug.Assert(false, $"unsported type {typeArray.Name} for array copy");
					//		oRcd = null;
					//		break;
					//}
					//if (srcArray != null)
					//{
					//	Buffer.BlockCopy(array, 0, srcArray, 0, Buffer.ByteLength(array));
					//	//var temp = typeof(Array)?.GetMethod("ConvertAll", new Type[] { typeArray, typeNew })?.Invoke(srcArray, new object[] { (t => (object)t)) });

					//	//var srcArrayWithT = Array.ConvertAll(srcArray, new Converter(x=>(T)x));
					//	for (int i = 0; i < oRcd.Length; i++)
					//	{
					//		var srcValue = srcArray[i];
					//		//following line take long time, don't know why
					//		T[] dstValue = new T[] { (T)Convert.ChangeType(srcValue, typeof(T)) };

					//		//cast bool to double is not permitted. code going to be more complecated.
					//		//dynamic dstValue = null;
					//		//switch(typeNew)
					//		//{
					//		//	case Type t when t == typeof(bool): // be carefull, bool in a array only take 1 byte, but normal bool should take 4 bytes
					//		//		dstValue = new bool[] { srcValue != 0 ? true : false };
					//		//		break;
					//		//	case Type t when t == typeof(double):
					//		//		dstValue[0] = new double[] { (double)srcValue };
					//		//		break;
					//		//	default:
					//		//		Debug.Assert(false, $"unsported type {typeNew.Name} for array copy");
					//		//		oRcd = null;
					//		//		break;
					//		//}
					//		//if (dstValue==null)
					//		//{
					//		//	break;
					//		//}
					//		Buffer.BlockCopy(dstValue, 0, oRcd, i * nDstSize, nDstSize);
					//	}
					//}
				}


				//should be slow, need to check its performance, 472.522msec
				//for (var i = 0; i < array.Length; i++)
				//{
				//	oRcd.SetValue(i, (T)Convert.ChangeType(array.GetAt(i), typeof(T)));
				//}

				//types
				//Type typeArray = array.GetType().GetElementType()!;
				//Type typeNew = oRcd.GetType().GetElementType();
				//if (typeArray == typeNew)
				//{//same type, so just copy it.
				//	Buffer.BlockCopy(array, 0, oRcd, 0, array.Length);
				//}

				////try to use unsafe code,1502.655msec
				//GCHandle gchArray = GCHandle.Alloc(array, GCHandleType.Pinned);
				//GCHandle gchNew = GCHandle.Alloc(oRcd, GCHandleType.Pinned);
				//IntPtr ptrArray = gchArray.AddrOfPinnedObject();
				//IntPtr ptrNew = gchNew.AddrOfPinnedObject();
				////copy data block size in bytes
				//int sizeArray = Buffer.ByteLength(array) / array.Length;
				//int sizeNew = Buffer.ByteLength(oRcd) / oRcd.Length;
				////block copy loops
				//unsafe
				//{
				//	//fist data point for each array
				//	byte* pArray = (byte*)ptrArray.ToPointer();
				//	byte* pNew = (byte*)ptrNew.ToPointer();
				//	T value = default(T);
				//	byte* btNewValue = (byte*)&value;
				//	//unsafe T myFunc() => {
				//	//	int* pint = (int*)pArray; 
				//	//	return (T)Convert.ChangeType(pint[0], typeof(T));
				//	//}
				//	for (var i = 0; i < array.Length; i++)
				//	{
				//		switch (typeArray)
				//		{
				//			case Type t when t == typeof(int):
				//				int* pint = (int*)pArray;
				//				value = (T)Convert.ChangeType(pint[i], typeof(T));
				//				break;
				//			case Type t when t == typeof(uint):
				//				uint* puint = (uint*)pArray;
				//				value = (T)Convert.ChangeType(puint[i], typeof(T));
				//				break;
				//			case Type t when t == typeof(long):
				//				uint* plong = (uint*)pArray;
				//				value = (T)Convert.ChangeType(plong[i], typeof(T));
				//				break;
				//			case Type t when t == typeof(bool):
				//				bool* pbool = (bool*)pArray;
				//				value = (T)Convert.ChangeType(pbool[i], typeof(T));
				//				break;
				//			default:
				//				Debug.Assert(false, $"unsported type {typeArray.Name} for array copy");
				//				break;
				//		}
				//		var temp = (byte[]?)typeof(BitConverter)?.GetMethod("GetBytes", new Type[] { typeNew })?.Invoke(null, new object[] { value });
				//		fixed (byte* pbtTemp = temp)
				//		{
				//			Buffer.MemoryCopy(pbtTemp, pNew, sizeNew, sizeNew);
				//		}
				//		pNew += sizeNew;
				//	}
				//}
				////unlock those memroy from garbage collection
				//gchArray.Free();
				//gchNew.Free();
			}
			return oRcd!;
		}

		/// <summary>
		/// Reverse the order of elements in an array along the given axis.
		/// <para>See Numpy reference: <see href="https://numpy.org/doc/stable/reference/generated/numpy.flip.html">np.flip()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.Flip(1) // Python equivalent: np.fliplr(oArr) or arr[:,::-1,:]
		/// </code>
		/// </example>
		/// <param name="array">this.array, the array that is to flipped</param>
		/// <param name="nAxis">Axis number, Ex: 1 or -2</param>
		/// <returns>Returns a flipped array along a given axis.</returns>
		public static object Flip(this Array array, int nAxis = 1)
		{
			Array? oRcd = null;
			if (array != null &&
				array.Length > 1)
			{
				int[] dims = array.GetDimensions();
				if (nAxis < 0)
				{//from know use plus number only
					nAxis += dims.Length;
				}
				if (nAxis >= 0 && nAxis < dims.Length)
				{
					//create new onw
					Array oTemp = Array.CreateInstance(array.GetType().GetElementType()!, dims);
					oRcd = oTemp;
					int[] Indices = new int[dims.Length];
					Array.Clear(Indices, 0, Indices.Length);
					int[] TargetIndices = new int[dims.Length];
					for (var i = 0; i < array.Length; i++)
					{
						//give GetIndices() dims & nIndexies to make it speed up
						//Indices = array.GetIndices(i, true, dims, Indices);
						object data = array.GetValue(Indices)!;
						Buffer.BlockCopy(Indices, 0, TargetIndices, 0, Buffer.ByteLength(Indices));
						TargetIndices[nAxis] = dims[nAxis] - TargetIndices[nAxis] - 1;
						//if (nIndexes[nAxis] > (dims[nAxis] - 1))
						//{
						//	nIndexes[nAxis] -= dims[nAxis];
						//}
						oTemp.SetValue(data, TargetIndices);
						Indices = Indices.Increament(dims);
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Return the sum of array elements over a given axis treating Not a Numbers 
		/// (NaNs) as zero.
		/// <para>See Numpy reference: <see href="https://numpy.org/doc/stable/reference/generated/numpy.nansum.html">np.nansum()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.NanSum(1) // Python equivalent: np.nansum(1) 
		/// </code>
		/// </example>
		/// <param name="array">this.array containing numbers whose sum is desired</param>
		/// <param name="nAxis">An axis along which sum is computed, if the axis does not appear in the array, return sum of every element</param>
		/// <returns>Returns a double[,...], or a double</returns>
		public static object NanSum(this Array array, int nAxis)
		{
			return array.NanSum(new int[] { nAxis });
		}

		/// <summary>
		/// Return the sum of array elements over a given axis or axes treating Not a Numbers 
		/// (NaNs) as zero.
		/// <para>See Numpy reference: <see href="https://numpy.org/doc/stable/reference/generated/numpy.nansum.html">np.nansum()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// // sum by axis 1,3,10. if this array does not have axis 10, axis 10 will be ignored
		///	array.NanSum(new int[]{3,1,10})	 
		///	
		/// // sum of all elements
		///	array.NanSum()	  
		///	
		/// // same as array.NanSum(new int[] { 2, 3}) if array. 
		/// // For Redlen test data, arrays will usually be Rank==4 or Rank==5
		///	array.NanSum(new int[] { -2, -1}) 
		/// </code>
		/// </example>
		/// <param name="array">this.array containing numbers whose sum is desired</param>
		/// <param name="axises">a set of axes as an int[]</param>
		/// <returns>Returns a double[,...], or a double</returns>
		public static object NanSum(this Array array, int[]? axises = null)
		{
			return NanByAxis(array, axises!, (data) => data.Aggregate((total, next) => total + next), true);
		}

		/// <summary>
		/// Return the minimum of an array or mainimum along an axis, ignoring any NaNs.
		/// Equvalent to <seealso href="https://numpy.org/doc/stable/reference/generated/numpy.nanmin.html">Numpy.nanmin()</seealso>
		/// </summary>
		/// <param name="array">this array</param>
		/// <param name="nAxis">An axis along which sum is computed, if the axis does not appear in the array, return sum of every element</param>
		/// <returns>minimum value(s) (double or double[,...])</returns>
		public static object NanMin(this Array array, int nAxis)
		{
			return array.NanMin(new int[] { nAxis });
		}

		/// <summary>
		/// Return the minimum of an array or minimum along an axis, ignoring any NaNs.
		/// Equvalent to <seealso href="https://numpy.org/doc/stable/reference/generated/numpy.nanmin.html">Numpy.nanmin()</seealso>
		/// </summary>
		/// <param name="array">this array</param>
		/// <param name="axises">axises, null: return a minimum value of whole array</param>
		/// <returns>minimum value(s) (double or double[,...])</returns>
		public static object NanMin(this Array array, int[]? axises)
		{
			return NanByAxis(array, axises!, (data) => data.MinBy(x => x), true);
		}

		/// <summary>
		/// Return the maximum of an array or maximum along an axis, ignoring any NaNs.
		/// Equvalent to <seealso href="https://numpy.org/doc/stable/reference/generated/numpy.nanmax.html">Numpy.nanmax()</seealso>
		/// </summary>
		/// <param name="array">this array</param>
		/// <param name="nAxis">An axis along which sum is computed, if the axis does not appear in the array, return sum of every element</param>
		/// <returns>mxaimum value(s)(double or double[,...])</returns>
		public static object NanMax(this Array array, int nAxis)
		{
			return array.NanMax(new int[] { nAxis });
		}

		/// <summary>
		/// Return the maximum of an array or maximum along an axis, ignoring any NaNs.
		/// Equvalent to <seealso href="https://numpy.org/doc/stable/reference/generated/numpy.nanmax.html">Numpy.nanmax()</seealso>
		/// </summary>
		/// <param name="array">this array</param>
		/// <param name="axises">axises, null: return a maximum value of whole array</param>
		/// <returns>mxaimum value(s)(double or double[,...])</returns>
		public static object NanMax(this Array array, int[]? axises = null)
		{
			return NanByAxis(array, axises!, (data) => data.MaxBy(x => x), true);
		}

		/// <summary>
		/// Return the cumulative sum of array elements over a given axis treating Not a Number
		/// (NaNs) as zero. The cumulative sum does not change when NaNs are encountered and leading NaNs are replaced by zeros.
		/// <para>Zeros are returned for slices that are all-NaN or empty.</para>
		/// <para>See Numpy reference: <see href="https://numpy.org/doc/stable/reference/generated/numpy.nancumsum.html">np.nancumsum()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		///	array.NanCumSum(1) // Python equivalent: np.nancumsum(arr, axis=1) 
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="nAxis">Axis along which the cumulative sum is computed. Null will give the cumulative sum of all array elements </param>
		/// <returns></returns>
		public static object NanCumSum(this Array array, int? nAxis = null)
		{
			object? oRcd = null;
			if (array != null && array.Length > 0)
			{
				var dims = array.GetDimensions();
				if (nAxis == null || nAxis >= 0 && nAxis < dims.Length)
				{
					int[] thisIndex = new int[dims.Length];
					Array.Clear(thisIndex, 0, thisIndex.Length);
					var thisRangs = new SliceIndex[thisIndex.Length];
					for (var i = 0; i < thisRangs.Length; i++)
					{
						thisRangs[i] = new SliceIndex()
						{
							Start = 0,
							Stop = dims[i],
							OriginalLength = dims[i]   //must set this before use of it Nxxxx properties
						};
					}
					if (nAxis != null)
					{//cumulateive by axis
						int[] sumDims = new int[dims.Length];
						Array.Copy(dims, sumDims, dims.Length);
						sumDims[(int)nAxis] = 1;
						Array CumSum = Array.CreateInstance(typeof(double), sumDims);
						Array.Clear(CumSum, 0, CumSum.Length);
						Array temp = Array.CreateInstance(typeof(double), dims);
						oRcd = temp;
						for (var i = 0; i < temp.Length; i++)
						{
							int[] sumIndex = new int[dims.Length];
							Array.Copy(thisIndex, sumIndex, dims.Length);
							sumIndex[(int)nAxis] = 0;
							double dTemp = Convert.ToDouble(CumSum.GetValue(sumIndex));
							//int nCumSum = thisIndex[(int)nAxis];
							dTemp += Convert.ToDouble(array.GetValue(thisIndex));
							temp.SetValue(dTemp, thisIndex);
							CumSum.SetValue(dTemp, sumIndex);
							IncreamentIndex(thisIndex, thisRangs);
						}
					}
					else
					{//cumulative all elements
						Array temp = Array.CreateInstance(typeof(double), new int[] { array.Length });
						oRcd = temp;
						double dTemp = 0;
						for (var i = 0; i < temp.Length; i++)
						{
							dTemp += Convert.ToDouble(array.GetValue(thisIndex));
							temp.SetValue(i, dTemp);
							IncreamentIndex(thisIndex, thisRangs);
						}
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Test whether any array element along a given axis evaluates to True.
		/// <para>See Numpy reference: <see href="https://numpy.org/doc/stable/reference/generated/numpy.any.html">np.any()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		///	array.Any(1) // Python equivalent: np.any(arr, axis=1) 
		/// </code>
		/// </example>
		/// <param name="array">this.array A bool[], int[], or double[]</param>
		/// <param name="nAxis">Axis along which a logical OR reduction is performed. The default null is to perform
		/// a logical OR over all the dimensions of the input array. Axis may be negative, in which case it counts from the
		/// last to the first axis</param>
		/// <returns>Returns a single bool var if axis is null or a bool[]</returns>
		public static object Any(this Array array, int? nAxis = null)
		{
			object? oRcd = null;
			if (array != null && array.Length > 0)
			{
				var dims = array.GetDimensions();
				if (nAxis == null ||
					nAxis >= 0 && nAxis < dims.Length)
				{
					int[] thisIndex = new int[dims.Length];
					Array.Clear(thisIndex, 0, thisIndex.Length);
					var thisRangs = new SliceIndex[thisIndex.Length];
					for (var i = 0; i < thisRangs.Length; i++)
					{
						thisRangs[i] = new SliceIndex()
						{
							Start = 0,
							Stop = dims[i],
							OriginalLength = dims[i]   //must set this before use of it Nxxxx properties
						};
					}
					if (nAxis != null)
					{//checked by axis
						int[] newDims = new int[dims.Length];
						Array.Copy(dims, newDims, dims.Length);
						//delete a dimension
						newDims = newDims.Where((source, index) => index != (int)nAxis).ToArray();
						Array result = Array.CreateInstance(typeof(bool), newDims);
						Array.Clear(result, 0, result.Length);
						oRcd = result;
						for (var i = 0; i < array.Length; i++)
						{
							int[] newIndex = new int[dims.Length];
							Array.Copy(thisIndex, newIndex, dims.Length);
							newIndex = newIndex.Where((source, index) => index != (int)nAxis).ToArray();
							bool bTemp = Convert.ToBoolean(result.GetValue(newIndex));
							if (!bTemp)
							{
								if (Convert.ToBoolean(array.GetValue(thisIndex)))
								{
									result.SetValue(true, newIndex);
								}
							}
							IncreamentIndex(thisIndex, thisRangs);
						}
					}
					else
					{//check all elements
						oRcd = false;
						for (var i = 0; i < array.Length; i++)
						{
							if (Convert.ToBoolean(array.GetValue(thisIndex)))
							{
								oRcd = true;
								break;
							}
							IncreamentIndex(thisIndex, thisRangs);
						}
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Compute the arithmetic mean along the specified axis, ignoring NaNs.
		/// <para>See Numpy reference: <see href="https://numpy.org/doc/stable/reference/generated/numpy.nanmean.html">np.nanmean()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		///	array.NanMean(1)	// Python equivalent: np.nanmean(arr, axis=1) 
		///	array.NanMean()		// mean of all elements
		/// </code>
		/// </example>
		/// <param name="array">Array containing numbers whose mean is desired</param>
		/// <param name="nAxis">Array axis along which the mean is computed. If the axis does not appear in this array, return mean of all the elements</param>
		/// <returns>Returns a double[], or a double with the mean of the array elements</returns>
		public static object NanMean(this Array array, int nAxis)
		{
			return array.NanMean(new int[] { nAxis });
		}

		/// <summary>
		/// Compute the arithmetic mean along the specified axes, ignoring NaNs.
		/// <para>See Numpy reference: <see href="https://numpy.org/doc/stable/reference/generated/numpy.nanmean.html">np.nanmean()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		///	array.NanMean(new int[]{3,1,10})	// mean by axes 1,3,10. if this array does not have axis 10, axis 10 will be ignored
		///	array.NanMean()	// mean of all elements
		/// </code>
		/// </example>
		/// <param name="array">Array containing numbers whose mean is desired</param>
		/// <param name="axes">Array axis along which the mean is computed. If the axis does not appear in this array, return mean of all the elements</param>
		/// <returns>Returns a double[], or a double with the mean of the array elements</returns>
		public static object NanMean(this Array array, int[]? axes = null)
		{
			return NanByAxis(array, axes!, (data) => data.Mean(), true);
		}

		/// <summary>
		/// Compute the median along the specified axis, while ignoring NaNs.
		/// <para>See Numpy reference: <see href="https://numpy.org/doc/stable/reference/generated/numpy.nanmedian.html">np.nanmedian()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		///	array.NanMedian(1)	// python equivalent: np.nanmedian(arr, axis=1)
		///	array.NanMedian()	// median of all elements
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="nAxis">Array axis along which the median is to be computed. If the axis does not appear in this array, return median of whole elements</param>
		/// <returns>Returns a double[], or a double containing the median of the array elements</returns>
		public static object NanMedian(this Array array, int nAxis)
		{
			return array.NanMedian(new int[] { nAxis });
		}

		/// <summary>
		/// Compute the median along the specified axes, while ignoring NaNs.
		/// <para>See Numpy reference: <see href="https://numpy.org/doc/stable/reference/generated/numpy.nanmedian.html">np.nanmedian()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		///	array.NanMedian(new int[]{3,1,10})	// median by axes 1,3,10. if this array does not have axis 10, axis 10 will be ignored
		///	array.NanMedian()	// median of all elements
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="axes">Array axes, an int[], along which the medians is to be computed. If the axes do not appear in this array, return median of whole elements</param>
		/// <returns>Returns a double[], or a double containing the median of the array elements</returns>
		public static object NanMedian(this Array array, int[]? axes = null)
		{
			return NanByAxis(array, axes!, (data) => data.Median(), true);
		}

		/// <summary>
		/// Compute the standard deviation along the specified axis, while ignoring NaNs.
		/// <para>See Numpy reference: <see href="https://numpy.org/doc/stable/reference/generated/numpy.nanstd.html">np.nanstd()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		///	array.NanStd(1)	// Python equivalent: np.nanstd(arr, axis=1)
		///	array.NanStd()	// standard deviation of all elements
		/// </code>
		/// </example>
		/// <param name="array">this. array to calculate the standard deviation of the non-NaN values</param>
		/// <param name="nAxis">Array axis, and int, along which the standard deviation is computed. If the axis does not appear in this array, return standard deviation of whole elements</param>
		/// <param name="ddof">int, optional. Means Delta Degrees of Freedom. The divisor used in calculations is N - ddof, where N represents the number of non-NaN elements. By default ddof is zero. 0: use population standard deviation, 1: use normal standard diviation</param>
		/// <returns>Returns a double[], or a double containing the standard deviation of the array elements. If ddof is >= the number of non-NaN elements in a slice or the slice contains only NaNs, then the result for that slice is NaN.</returns>
		public static object NanStd(this Array array, int nAxis, int ddof = 0)
		{
			return array.NanStd(new int[] { nAxis });
		}

		/// <summary>
		/// Compute the standard deviation along the specified axes, while ignoring NaNs.
		/// <para>See Numpy reference: <see href="https://numpy.org/doc/stable/reference/generated/numpy.nanstd.html">np.nanstd()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		///	array.NanStd(new int[]{3,1,10})	// standard deviation by axes 1,3,10. If this array does not have axis 10, axis 10 will be ignored
		///	array.NanStd()	// standard deviation of all elements
		/// </code>
		/// </example>
		/// <param name="array">this. array to calculate the standard deviation of the non-NaN values</param>
		/// <param name="axes">Array axes, and int[], along which the standard deviations are computed. If the axes do not appear in this array, return standard deviation of whole elements</param>
		/// <param name="ddof">int, optional. Means Delta Degrees of Freedom. The divisor used in calculations is N - ddof, where N represents the number of non-NaN elements. By default ddof is zero. 0: use population standard deviation, 1: use normal standard diviation</param>
		/// <returns>Returns a double[], or a double containing the standard deviation of the array elements. If ddof is >= the number of non-NaN elements in a slice or the slice contains only NaNs, then the result for that slice is NaN.</returns>
		public static object NanStd(this Array array, int[]? axes = null, int ddof = 0)
		{
			Func<double[], double> func = (data) => data.PopulationStandardDeviation();
			if (ddof != 0)
			{
				func = (data) => data.StandardDeviation();
			}
			return NanByAxis(array, axes!, func, true);
		}

		/// <summary>
		/// Compute the q-th quantile of the data along the specified axis.
		/// Equvalent to <seealso href="https://numpy.org/doc/stable/reference/generated/numpy.quantile.html">Numpy.quantile()</seealso>
		/// </summary>
		/// <param name="array">this array</param>
		/// <param name="Quantile">
		/// Quantile or sequence of quantiles to compute,
		/// which must be between 0 and 1 inclusive.
		/// </param>
		/// <param name="nAxis">Axis along which the quantiles are computed.</param>
		/// <returns>a dobule value or an array with dobule elememts. </returns>
		public static object Quantile(this Array array, double[] Quantile, int nAxis)
		{
			return array.Quantile(Quantile, new int[] { nAxis });
		}

		/// <summary>
		/// Compute the q-th quantile of the data along the specified axis.
		/// Equvalent to <seealso href="https://numpy.org/doc/stable/reference/generated/numpy.quantile.html">Numpy.quantile()</seealso><br/>
		/// Use following linear methode only.<br/>
		/// <c> value = a[i] + (a[i+1]-a[i]) * (q*(N-1)-i) </c><br/>
		/// Notice : Whole ligic is same as NanByAxis(), except the part to make Quantile.
		/// </summary>
		/// <param name="array">this array</param>
		/// <param name="Quantile">
		/// Quantile or sequence of quantiles to compute,
		/// which must be between 0 and 1 inclusive.
		/// </param>
		/// <param name="axes">Axes along which the quantiles are computed. null : return quantile bases all elements.</param>
		/// <returns></returns>
		public static object Quantile(this Array array, double[] Quantile, int[]? axes = null)
		{
			object? oRcd = null;
			if (Quantile != null && Quantile.Length > 0)
			{
				//all Quantile must be in 0.0 to 1.0
				for (int i = 0; i < Quantile.Length; i++)
				{
					if (Quantile[i] < 0)
					{
						Quantile[i] = 0.0;
					}
					else if (Quantile[i] > 1.0)
					{
						Quantile[i] = 1.0;
					}
				}
				//orignal array dims
				var orignalDims = array.GetDimensions();
				//normalize axes
				var normalAxises = axes?.NormalizeAxes(orignalDims.Length);
				if (normalAxises != null)
				{
					if (normalAxises.Length >= orignalDims.Length)
					{
						normalAxises = null;
					}
					else
					{
						//sort it
						Array.Sort(normalAxises);
					}
				}
				if (normalAxises == null || normalAxises.Length <= 0)
				{
					Array oTemp = array.Copy<double>(true);
					Array.Sort(oTemp);
					Array oRcdTemp = Array.CreateInstance(typeof(double), new int[] { Quantile.Length });
					oRcd = oRcdTemp;
					for (int i = 0; i < Quantile.Length; i++)
					{
						double dIndex = Quantile[i] * (oTemp.Length - 1);
						int nIndex = (int)Math.Floor(dIndex);
						double dFraction = dIndex - nIndex;
						double dValue1 = (double)oTemp.GetValue(nIndex)!;
						if (dFraction > double.Epsilon && nIndex < oTemp.Length - 1)
						{//dFraction is not 0
							double dValue2 = (double)oTemp.GetValue(nIndex + 1)!;
							dValue1 += (dValue2 - dValue1) * dFraction;
						}
						oRcdTemp.SetValue(i, dValue1);
					}
					if (oRcdTemp.Length <= 1)
					{
						oRcd = oRcdTemp.GetValue(0);
					}
				}
				else
				{
					//get dimensions of this array
					var rangsForIncreament = new SliceIndex[normalAxises.Length];
					var axisesIndex = new int[normalAxises.Length];
					var axisesLength = 1;
					for (var i = 0; i < normalAxises.Length; i++)
					{
						axisesIndex[i] = 0;
						rangsForIncreament[i] = new SliceIndex()
						{
							Start = 0,
							Stop = orignalDims[normalAxises[i]],
							OriginalLength = orignalDims[normalAxises[i]]   //must set this before use of it Nxxxx properties
						};
						axisesLength *= rangsForIncreament[i].NStop;
					}

					//new dimesions
					int[] newDims = orignalDims.Where((source, index) => !normalAxises.Contains(index)).ToArray();
					//delete 1 length dimensions
					//newDims = newDims?.Where(e => e > 1).ToArray();
					//add last for Quantiles, work with Quantile.Length==1
					newDims = newDims.Concat(new int[] { Quantile.Length }).ToArray();
					//if ((newDims == null) || (newDims.Length <= 0))
					//{//all axises was selected
					//	newDims = new int[] { 1 };
					//}
					var newArray = Array.CreateInstance(typeof(double), newDims);
					int[] Indices = new int[newDims.Length];
					for (var newArrayIndex = 0; newArrayIndex < newArray.Length; newArrayIndex += Quantile.Length)
					{
						Indices = newArray.GetIndices(newArrayIndex, true, newDims, Indices);
						//origanl array indices
						int[] orignalIndices = new int[orignalDims.Length];
						int nNewIndex = 0;
						for (int j = 0; j < orignalDims.Length; j++)
						{
							if (normalAxises.Contains(j))
							{
								continue;
							}
							orignalIndices[j] = Indices[nNewIndex];
							nNewIndex++;
						}
						double[] dtemps = new double[axisesLength];
						for (int k = 0; k < axisesLength; k++)
						{//for axis
							for (int l = 0; l < normalAxises.Length; l++)
							{
								orignalIndices[normalAxises[l]] = axisesIndex[l];
							}
							dtemps[k] = Convert.ToDouble(array.GetValue(orignalIndices));
							IncreamentIndex(axisesIndex, rangsForIncreament);
						}
						//sort the data
						Array.Sort(dtemps);
						//calculate values and set them to oRcd
						for (int i = 0; i < Quantile.Length; i++)
						{
							double dIndex = Quantile[i] * (dtemps.Length - 1);
							int nIndex = (int)Math.Floor(dIndex);
							double dFraction = dIndex - nIndex;
							double dValue1 = (double)dtemps.GetValue(nIndex)!;
							if (dFraction > double.Epsilon && nIndex < dtemps.Length - 1)
							{//dFraction is not 0
								double dValue2 = (double)dtemps.GetValue(nIndex + 1)!;
								dValue1 += (dValue2 - dValue1) * dFraction;
							}
							Indices[^1] = i;
							newArray.SetValue(dValue1, Indices);
						}
					}

					if (newArray.Length == 1)
					{
						oRcd = newArray.GetAt(0);
					}
					else
					{//if Quantile.Length==1, last dimension will be removed
						newArray = newArray.MoveAxis(newDims.Length - 1, 0);
						oRcd = newArray.Squeeze();
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Normalize indices, main convert minus number to plus alsi its value should be &gt;=0 &amp; &lt;dims[this pos]
		/// </summary>
		/// <param name="array">an index array with null element, null means it can be 0 to &gt;dims[this pos]</param>
		/// <param name="dims">dimensions, the length must be same as this array</param>
		/// <returns>false if array==null, or dims==null or array.Length==dims.Length</returns>
		public static bool NormalizeIndices(this int?[] array, int[] dims)
		{
			bool bRcd = false;
			if (array != null &&
				dims != null &&
				array.Length == dims.Length &&
				dims.All(o => o > 0)) //all element of dims must > 0
			{
				for (int i = dims.Length - 1; i >= 0; i--)
				{
					if (array[i] != null)
					{
						//assume dims[i]=7,
						//if array[i]=-1 then array[i] will be 6,
						//if array[i]=10 then array[i] will be 3,
						//if array[i]=-7 then array[i] will be 0
						array[i] %= dims[i]; //make the value > -dims[i] && < dims[i]
						if (array[i] < 0)
						{
							array[i] += dims[i];
						}
					}
				}
				bRcd = true;
			}
			return bRcd;
		}

		/// <summary>
		/// Normalize given axes. all values must be &lt;=0 and &gt;= nRank
		/// </summary>
		/// <param name="array">this array, an <c>int[]</c> array</param>
		/// <param name="nRank">rank of target array in <c>int</c></param>
		/// <returns>Normalized Axes in <c>int[]</c></returns>
		public static int[] NormalizeAxes(this int[] array, int nRank)
		{
			int[]? oRcd = null;
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					//if (array[i] < 0)
					//{
					//	array[i] = (array[i] % nRank) + nRank;
					//}
					//else
					//{
					//	array[i] %= nRank;
					//}
					array[i] = array[i].NormalizeAxis(nRank);
				}
				// no same value in this array
				oRcd = array?.Distinct().ToArray()!;
			}
			return oRcd!;
		}

		/// <summary>
		/// Normalize an axis number (0 lesser and equal than Axis lesser than nRank)
		/// </summary>
		/// <param name="nAxis">this axis</param>
		/// <param name="nRank">rank or number of dimesions</param>
		/// <returns></returns>
		public static int NormalizeAxis(this int nAxis, int nRank)
		{
			nAxis %= nRank; //make the value > -dims[i] && < dims[i]
			if (nAxis < 0)
			{
				nAxis += nRank;
			}
			return nAxis;
		}

		/// <summary>
		/// Get part of array by using SliceIndex.
		/// </summary>
		/// <example>
		/// <code>
		/// intArray.PartOf(new SliceIndex[] {null, new SliceIndex(0,1) })	// npInt[:,:1,:]
		/// intArray.PartOf(new SliceIndex[] { null, null, new SliceIndex(2, 3) })	// npInt[:,:,2:3]
		/// intArray.PartOf(new SliceIndex[] { new SliceIndex(0, 2), null, new SliceIndex(2, 3) })	// npInt[:2,:,2:3]
		/// intArray.PartOf(new SliceIndex[] { null, new SliceIndex(null, null, -1)});	// npInt[":","::-1"] (=== np.fliplr(npInt) === np.flip(npInt, axis=1))
		/// intArray.PartOf(new SliceIndex[] { null, null, new SliceIndex(null, 4, 2) })	// npInt[:,:,:4:2]
		/// intArray.PartOf(new SliceIndex[] { null, null, new SliceIndex(-1, 2, -2) })	// npInt[:,:,-1:2:-2]
		/// intArray.PartOf(new object[] { null, null, new int[] { 0, 3, 4 } })	// npInt[:,:,0:3:4]
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="ranges">
		///		The range of each dimension to retrieve. for details see SliceIndex class. 
		///		And each element can be a SliceIndex object or a int[3], each int[] element is one of the element index in current dimension. by using int[].</param>
		/// <param name="bKeepOriginalDimesions">false:remove all dimesions those are equal and lesser than 1. true: keep all dimesions</param>
		/// <returns>Returns part of an array over a given range(s)</returns>
		public static object PartOf(this Array array, object[] ranges, bool bKeepOriginalDimesions = false)
		{
			object? oRcd = null;
            if (array != null && array.Length > 0)
            {
                var dims = array.GetDimensions();
                //object data = array.GetAt(0);
                if (ranges == null || ranges.Length <= 0)
                {//return a copy of current array
                    oRcd = Array.CreateInstance(array.GetType().GetElementType()!, dims);
                    Buffer.BlockCopy(array, 0, (Array)oRcd, 0, Buffer.ByteLength(array));
                }
                else
                {
                    //make real ranges
                    var realRangs = new object[array.Rank];
                    int[] newDims = new int[array.Rank];
                    int[] startIndexes = new int[array.Rank];
                    //init ranges, new array dims, indeces
                    for (int i = 0; i < realRangs.Length; i++)
                    {
                        SliceIndex oSlice = new SliceIndex()
                        {
                            Start = 0,
                            Stop = array.GetLength(i),
                            OriginalLength = array.GetLength(i) //must set up this first, then use SliceIndex.Nxxxx
                        };
                        realRangs[i] = oSlice;
                        newDims[i] = oSlice.Length;
                        startIndexes[i] = oSlice.NStart;
                        if (i < ranges.Length &&
                            ranges[i] != null)
                        {
                            if (ranges[i] is SliceIndex oSliceRange)
                            {
                                oSliceRange.OriginalLength = array.GetLength(i);//must set up this first, then use SliceIndex.Nxxxx
                                realRangs[i] = ranges[i];
                                newDims[i] = oSliceRange.Length > 0 ? oSliceRange.Length : 1;
                                startIndexes[i] = oSliceRange.NStart;
                            }
                            else if (ranges[i] is int[] oIdeces)
                            {
                                int[]? indecesTemp = null;
                                if (oIdeces.Length <= 0)
                                {// try do right things
                                    indecesTemp = new int[] { 0 };
                                }
                                else
                                {
                                    indecesTemp = oIdeces.Where((source, index) => source >= 0 && source < dims[i]).ToArray();
                                    indecesTemp = indecesTemp.Distinct().ToArray();
                                }
                                newDims[i] = indecesTemp.Length;
                                realRangs[i] = indecesTemp;
                            }
                            //else
                            //{// bat parameters, ranges must be SliceIndex or int[]
                            //	return null;
                            //}
                        }
                    }

                    //newDims = newDims.Where(x => x > 0).ToArray();
                    if (newDims.Length > 0)
                    {
                        var targetArray = Array.CreateInstance(array.GetType().GetElementType()!, newDims);
                        oRcd = targetArray;

                        //case 0: without Parallel.For()
                        //int[] targetIndice = new int[newDims.Length];
                        //Array.Clear(targetIndice);
                        //for (var targetArrayIndex = 0; targetArrayIndex < targetArray.Length; targetArrayIndex++)
                        //{
                        //	var tempData = array.GetValue(startIndexes);
                        //	//targetArray.SetValue((long)targetArrayIndex, (object)tempData);
                        //	targetArray.SetValue(tempData, targetIndice);
                        //	//array copy take more time then GetVelue&SetValue()
                        //	//Array.Copy(array, startIndexes.Aggregate(1, (a, b) => a * b), targetArray, targetArrayIndex,1);
                        //	IncreamentIndex(startIndexes, realRangs);   // since Step may be used in realRangs, so we can not change this to indices.Increament(dims)
                        //	targetIndice.Increament(newDims);
                        //}

                        //case 1: performance up??, using lock for shared variable, low performance????
                        //int[] targetIndice = new int[newDims.Length];
                        //Array.Clear(targetIndice);
                        //Parallel.For(0, targetArray.Length, targetArrayIndex =>
                        //{
                        //	dynamic tempData;
                        //	lock(targetIndice)
                        //	{
                        //		tempData = array.GetValue(startIndexes);
                        //		IncreamentIndex(startIndexes, realRangs);   // since Step may be used in realRangs, so we can not change this to indices.Increament(dims)
                        //		targetArray.SetValue(tempData, targetIndice);
                        //		targetIndice.Increament(newDims);
                        //	}
                        //});


                        //case 2: performance up, , whole local variables are inside of each thread
                        //Convert realRangs to simple data to make it fast
                        var oRanges = new object[realRangs.Length];
                        for (int k = 0; k < realRangs.Length; k++)
                        {
                            if (realRangs[k] is SliceIndex oSlice)
                            {
                                oRanges[k] = new long[] { oSlice.NStart, oSlice.NStep };
                                //sourcIndices[k] = oSlice.NStart + oSlice.NStep * targetIndices[k];
                            }
                            else if (realRangs[k] is int[] oIndices)
                            {
                                oRanges[k] = oIndices;
                            }
                        }
                        //case 2.1 using Parallel.For()
                        Parallel.For(0, targetArray.Length, (targetArrayIndex) =>
                        {
                            //get targetIndices from it long index
                            int[] targetIndices = targetArray.GetIndices(targetArrayIndex, true, newDims);
                            //map this target indices to source indices
                            int[] sourcIndices = new int[targetIndices.Length];
                            for (int k = 0; k < sourcIndices.Length; k++)
                            {
                                if (oRanges[k] is long[] oSlice)
                                {
                                    Debug.Assert(oSlice.Length == 2, "oSlice is a uint[], must contains 2 element");
                                    sourcIndices[k] = (int)oSlice[0] + (int)oSlice[1] * targetIndices[k];
                                }
                                else if (realRangs[k] is int[] oIndices)
                                {
                                    Debug.Assert(targetIndices[k] >= 0 && targetIndices[k] < oIndices.Length, "targetIndices[k] must be a index of oIndices");
                                    sourcIndices[k] = oIndices[targetIndices[k]];
                                }
                            }
                            var tempData = array.GetValue(sourcIndices);
                            targetArray.SetValue(tempData, targetIndices);
                        });

						//case 2.2 using Parallel.Foreach()
						//make all target indices
						//var AllTargetIndices = new object[targetArray.Length];
						//int[] targetIndices = new int[newDims.Length];
						//Array.Clear(targetIndices);
						//for (int i = 0; i < targetArray.Length; i++)
						//{
						//    AllTargetIndices[i] = targetIndices.Clone();
						//    targetIndices.Increament(newDims);
						//}
						//Parallel.ForEach(AllTargetIndices, TargetIndices =>
						//{
						//    int[] targetIndices = (int[])TargetIndices;
						//    int[] sourcIndices = new int[targetIndices.Length];
						//    for (int k = 0; k < sourcIndices.Length; k++)
						//    {
						//        if (oRanges[k] is long[] oSlice)
						//        {
						//            Debug.Assert(oSlice.Length == 2, "oSlice is a uint[], must contains 2 element");
						//            sourcIndices[k] = (int)oSlice[0] + (int)oSlice[1] * targetIndices[k];
						//        }
						//        else if (realRangs[k] is int[] oIndices)
						//        {
						//            Debug.Assert(targetIndices[k] >= 0 && targetIndices[k] < oIndices.Length, "targetIndices[k] must be a index of oIndices");
						//            sourcIndices[k] = oIndices[targetIndices[k]];
						//        }
						//    }
						//    var tempData = array.GetValue(sourcIndices);
						//    targetArray.SetValue(tempData, targetIndices);
						//});

						if (!bKeepOriginalDimesions &&
							targetArray.GetDimensions().Where(x => x == 0 || x == 1).ToList().Count > 0)
							oRcd = targetArray.Squeeze();
                    }
                    else
                        oRcd = (double)array.GetAt(0);
                }
            }
            return oRcd!;
        }

		/// <summary>
		/// Remove axis of length one from this array
		/// <para>See Numpy reference: <see href="https://numpy.org/doc/stable/reference/generated/numpy.squeeze.html">np.squeeze()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		///	array.Squeeze(1)	// Python equivalent: np.squeeze(arr, axis=1)
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="nAxis">Null or int, optional. Null means remove all axes whose lengths are one. Or specify an axis to be removed, if the length of this axis is not one, do nothing</param>
		/// <returns>Returns the input array, but with all or a subset of the dimensions of length 1 removed. If length of nAxis is not one, return null</returns>
		public static Array Squeeze(this Array array, int? nAxis = null)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0)
			{
				bool bCopy = false;
				var dims = array.GetDimensions();
				if (nAxis != null)
				{
					int nTemp = (int)nAxis;
					if (nTemp < dims.Length && dims[nTemp] == 1)
					{//remove this axis
						bCopy = true;
						dims = dims?.Where((source, index) => index != nTemp).ToArray();
					}
				}
				else
				{//remove all one length axis
				 //delete 1 length dimensions
					oRcd = array;
					var previousLen = dims.Length;
					int[] newdims = dims?.Where(e => e > 1).ToArray()!;
					if (newdims.Length > 0 && //for [1,1,1,..] may keep it
						newdims.Length < previousLen)
					{
						dims = newdims;
						bCopy = true;
					}
					else if (newdims.Length == 0)
					{
						oRcd = Array.CreateInstance(array.GetType().GetElementType()!, new int[] { 1 });
						oRcd.SetValue(array.GetAt(0), 0);
					}
				}
				if (bCopy)
				{
					//var data = array.GetAt(0);
					//if the array is bool array.GetAt(0) returen bool but its byte size is 2 not 1, so do not use array.Get(0) to get determin it element size
					oRcd = Array.CreateInstance(array.GetType().GetElementType()!, dims!);
					Buffer.BlockCopy(array, 0, oRcd, 0, Buffer.ByteLength(array));
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Join an array to this.array along an existing axis.
		/// <para>The data type of src and this.array must be same, if not, use Copy() to make the array the same data type.
		/// If the axis is not null, the length of each dimension except this axis must be the same, if not, return null</para>
		/// <para>See Numpy reference: <see href="https://numpy.org/doc/stable/reference/generated/numpy.concatenate.html">np.concatenate()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		///	array.Concatenate(arr1, 1)	// Python equivalent: np.concatenate((arr, arr1), axis=1)
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="src">An array that must have the same shape as this.array, except in the dimension corresponding to axis</param>
		/// <param name="nAxis">The axis along which the arrays will be joined. If axis is null, arrays are flattened before use</param>
		/// <returns>Returns the concatenated array</returns>
		public static Array Concatenate(this Array array, Array src, int? nAxis = null)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0 &&
				src != null && src.Length > 0)
			{
				//var srcData = src.GetAt(0);
				//var thisData = array.GetAt(0);
				if (array.GetType().GetElementType() == src.GetType().GetElementType())
				{
					int[] thisDims = array.GetDimensions();
					int[] srcDims = src.GetDimensions();
					if (nAxis == null || // to return a one dimensional array
						thisDims.Length == 1 && srcDims.Length == 1)  //current arry's are all one dimension.
					{
						oRcd = Array.CreateInstance(array.GetType().GetElementType()!, new int[] { array.Length + src.Length });
						int bytes = Buffer.ByteLength(array);
						Buffer.BlockCopy(array, 0, oRcd, 0, bytes);
						Buffer.BlockCopy(src, 0, oRcd, bytes, Buffer.ByteLength(src));
					}
					else
					{// >= 2 dimentions
						int nIndex = (int)nAxis;
						if (thisDims.Length == srcDims.Length &&
							nIndex >= 0 && nIndex < thisDims.Length)
						{//in this case must have same dimension, and nAxis is correct
							bool bCanBeDone = true;
							for (var i = 0; i < thisDims.Length; i++)
							{
								if (i == nIndex)
								{
									continue;
								}
								if (thisDims[i] != srcDims[i])
								{//can not concatenate
									bCanBeDone = false;
									break;
								}
							}
							if (bCanBeDone)
							{
								int[] newDims = new int[thisDims.Length];
								Array.Copy(thisDims, 0, newDims, 0, thisDims.Length);
								newDims[nIndex] += srcDims[nIndex];
								//create new array
								oRcd = Array.CreateInstance(array.GetType().GetElementType()!, newDims);
								//for (var i = 0; i < newDims.Length; i++)
								//{
								//	newDims[i] = thisDims[i];
								//	if (i == nIndex)
								//	{//different from this and src
								//		newDims[i] += srcDims[i];
								//	}
								//}
								if (nIndex == 0)
								{//do fast block copy
								 //oRcd = Array.CreateInstance(thisData.GetType(), newDims);
									int bytes = Buffer.ByteLength(array);
									Buffer.BlockCopy(array, 0, oRcd, 0, bytes);
									Buffer.BlockCopy(src, 0, oRcd, bytes, Buffer.ByteLength(src));
								}
								else
								{
#if UnuseUnsafe
									int[] newIndex = new int[thisDims.Length];
									Array.Clear(newIndex, 0, newIndex.Length);
									int[] thisIndex = new int[thisDims.Length];
									Array.Clear(thisIndex, 0, thisIndex.Length);
									int[] srcIndex = new int[srcDims.Length];
									Array.Clear(srcIndex, 0, srcIndex.Length);
									var newRangs = new SliceIndex[newIndex.Length];
									var thisRangs = new SliceIndex[thisIndex.Length];
									var srcRangs = new SliceIndex[thisIndex.Length];
									for (var i = 0; i < thisRangs.Length; i++)
									{
										//newDims[i] = thisDims[i];
										//if (i == nIndex)
										//{//different from this and src
										//	newDims[i] += srcDims[i];
										//}
										newRangs[i] = new SliceIndex()
										{
											Start = 0,
											Stop = newDims[i],
											OriginalLength = newDims[i]   //must set this before use of it Nxxxx properties
										};
										thisRangs[i] = new SliceIndex()
										{
											Start = 0,
											Stop = thisDims[i],
											OriginalLength = thisDims[i]   //must set this before use of it Nxxxx properties
										};
										srcRangs[i] = new SliceIndex()
										{
											Start = 0,
											Stop = srcDims[i],
											OriginalLength = srcDims[i]   //must set this before use of it Nxxxx properties
										};
									}
									for (var i=0; i < oRcd.Length; i++)
									{
										object oData = null;
										if (newIndex[nIndex] < thisDims[nIndex])
										{//read from this
											oData = array.GetValue(thisIndex);
											IncreamentIndex(thisIndex, thisRangs);
										}
										else
										{
											oData = src.GetValue(srcIndex);
											IncreamentIndex(srcIndex, srcRangs);
										}
										oRcd.SetValue(oData, newIndex);
										IncreamentIndex(newIndex,newRangs);
									}
#else
									//get memory handle of each array, and lock those memroy from garbage collection(prevents the garbage collector from moving the object and hence undermines the efficiency of the garbage collector)
									GCHandle gchArray = GCHandle.Alloc(array, GCHandleType.Pinned);
									GCHandle gchSrc = GCHandle.Alloc(src, GCHandleType.Pinned);
									GCHandle gchNew = GCHandle.Alloc(oRcd, GCHandleType.Pinned);
                                    nint ptrArray = gchArray.AddrOfPinnedObject();
                                    nint ptrSrc = gchSrc.AddrOfPinnedObject();
                                    nint ptrNew = gchNew.AddrOfPinnedObject();
									//copy data block size in bytes
									int blockSizeArray = thisDims[nIndex];
									int blockSizeSrc = srcDims[nIndex];
									for (var i = nIndex + 1; i < thisDims.Length; i++)
									{
										blockSizeArray *= thisDims[i];
										blockSizeSrc *= srcDims[i];
									}
									blockSizeArray *= Buffer.ByteLength(array) / array.Length;
									blockSizeSrc *= Buffer.ByteLength(src) / src.Length;
									//block copy loops
									int nLoop = thisDims[0];
									for (var i = 1; i < nIndex; i++)
									{
										nLoop *= thisDims[i];
									}
									unsafe
									{
										//fist data point for each array
										byte* pArray = (byte*)ptrArray.ToPointer();
										byte* pSrc = (byte*)ptrSrc.ToPointer();
										byte* pNew = (byte*)ptrNew.ToPointer();
										for (var i = 0; i < nLoop; i++)
										{
											Buffer.MemoryCopy(pArray, pNew, blockSizeArray, blockSizeArray);
											pArray += blockSizeArray;
											pNew += blockSizeArray;
											Buffer.MemoryCopy(pSrc, pNew, blockSizeSrc, blockSizeSrc);
											pSrc += blockSizeSrc;
											pNew += blockSizeSrc;
										}
									}
									//unlock those memroy from garbage collection
									gchArray.Free();
									gchSrc.Free();
									gchNew.Free();
#endif
								}
							}
						}
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Interchange two axes of an array.
		/// <para>See Numpy reference: <see href="https://numpy.org/doc/stable/reference/generated/numpy.swapaxes.html">np.swapaxes()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		///	array.SwapAxes(0, 1)	// Python equivalent: np.swapaxes(arr, 0, 1)
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="nAxis1">First axis number. Axis can be negative. -1 == last axis</param>
		/// <param name="nAxis2">Second axis number. Axis can be negative. -1 == last axis</param>
		/// <returns>Returns an array whose axes are swapped with the given axes</returns>
		public static Array SwapAxes(this Array array, int nAxis1, int nAxis2)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0)
			{
				int[] thisDims = array.GetDimensions();
				if (nAxis1 < 0)
				{//from know use plus number only
					nAxis1 += thisDims.Length;
				}
				if (nAxis2 < 0)
				{//from know use plus number only
					nAxis2 += thisDims.Length;
				}
				if (nAxis1 >= 0 && nAxis2 >= 0 && nAxis1 != nAxis2 &&
					nAxis1 < thisDims.Length && nAxis2 < thisDims.Length)
				{
					//create new dimesion
					int[] newDims = new int[thisDims.Length];
					Array.Copy(thisDims, 0, newDims, 0, thisDims.Length);
					newDims.SwapElement(nAxis1, nAxis2);
					//create an index array for array
					int[] thisIndex = new int[thisDims.Length];
					Array.Clear(thisIndex, 0, thisIndex.Length);
					var thisRangs = new SliceIndex[thisIndex.Length];
					for (var i = 0; i < thisRangs.Length; i++)
					{
						//rang for each dimenstion
						thisRangs[i] = new SliceIndex()
						{
							Start = 0,
							Stop = thisDims[i],
							OriginalLength = thisDims[i]   //must set this before use of it Nxxxx properties
						};
					}
					//create new array
					oRcd = Array.CreateInstance(array.GetType().GetElementType()!, newDims);
					for (var i = 0; i < oRcd.Length; i++)
					{
						object oData = array.GetValue(thisIndex)!;
						//create an index for new array
						int[] newIndex = new int[thisIndex.Length];
						Array.Copy(thisIndex, 0, newIndex, 0, thisIndex.Length);
						newIndex.SwapElement(nAxis1, nAxis2);
						oRcd.SetValue(oData, newIndex);
						IncreamentIndex(thisIndex, thisRangs);
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Moves axes of an array to new positions. Other axes remain in their original order.
		/// <para>See Numpy reference: <see href="https://numpy.org/doc/stable/reference/generated/numpy.moveaxis.html">np.moveaxis()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		///	array.MoveAxis(0, 1)	// Python equivalent: np.moveaxis(arr, 0, 1)
		/// </code>
		/// </example>
		/// <param name="array">this.array, the array whose axes should be reordered</param>
		/// <param name="nAxisSource">Original positions of the axes to move. These must be unique</param>
		/// <param name="nAxisDestination">Destination positions for each of the original axes. These must also be unique</param>
		/// <returns>Return an array with moved axes. This array is a view of the input array</returns>
		public static Array MoveAxis(this Array array, int nAxisSource, int nAxisDestination)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0)
			{
				int[] thisDims = array.GetDimensions();
				if (nAxisSource < 0)
				{//from know use plus number only
					nAxisSource += thisDims.Length;
				}
				if (nAxisDestination < 0)
				{//from know use plus number only
					nAxisDestination += thisDims.Length;
				}
				if (nAxisSource >= 0 && nAxisDestination >= 0 && nAxisSource != nAxisDestination &&
					nAxisSource < thisDims.Length && nAxisDestination < thisDims.Length)
				{
					//create new dimesion
					int[] newDims = new int[thisDims.Length];
					Array.Copy(thisDims, 0, newDims, 0, thisDims.Length);
					newDims.MoveElement(nAxisSource, nAxisDestination);
					//create an index array for array
					int[] thisIndex = new int[thisDims.Length];
					Array.Clear(thisIndex, 0, thisIndex.Length);
					var thisRangs = new SliceIndex[thisIndex.Length];
					for (var i = 0; i < thisRangs.Length; i++)
					{
						//rang for each dimenstion
						thisRangs[i] = new SliceIndex()
						{
							Start = 0,
							Stop = thisDims[i],
							OriginalLength = thisDims[i]   //must set this before use of it Nxxxx properties
						};
					}
					//create new array
					oRcd = Array.CreateInstance(array.GetType().GetElementType()!, newDims);
					for (var i = 0; i < oRcd.Length; i++)
					{
						object oData = array.GetValue(thisIndex)!;
						//create an index for new array
						int[] newIndex = new int[thisIndex.Length];
						Array.Copy(thisIndex, 0, newIndex, 0, thisIndex.Length);
						newIndex.MoveElement(nAxisSource, nAxisDestination);
						oRcd.SetValue(oData, newIndex);
						IncreamentIndex(thisIndex, thisRangs);
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Move an arrays element or dimension from one index to another.
		/// </summary>
		/// <example>
		/// <code>
		/// array.MoveElement(0, 2)	// [3,2,5] --> [2,5,3]
		/// </code>
		/// </example>
		/// <typeparam name="T">Should be of type int, uint, short...</typeparam>
		/// <param name="array">this.array T[]</param>
		/// <param name="nFrom">The index of the element to be moved</param>
		/// <param name="nTo">The index where the element will move to</param>
		/// <returns>Return an array with the same shape as this.array</returns>
		public static void MoveElement<T>(this T[] array, int nFrom, int nTo) where T : struct
		{
			if (array != null &&
				nFrom >= 0 && nFrom < array.Length &&
				nTo >= 0 && nTo < array.Length &&
				nFrom != nTo)
			{
				T tmp = array[nFrom];
				if (nTo < nFrom)
				{
					// Need to move part of the array "up" to make room
					Array.Copy(array, nTo, array, nTo + 1, nFrom - nTo);
				}
				else
				{
					// Need to move part of the array "down" to fill the gap
					Array.Copy(array, nFrom + 1, array, nFrom, nTo - nFrom);
				}
				array[nTo] = tmp;
			}
		}

		/// <summary>
		/// Swap two elements or dimensions of an array T[] with two given indices.
		/// </summary>
		/// <example>
		/// <code>
		/// array.SwapElement(0, 3) // swaps the elements or dimensions in the indices 0 and 3
		/// </code>
		/// </example>
		/// <typeparam name="T">Should be of type int, uint, short...</typeparam>
		/// <param name="array">this.array T[]</param>
		/// <param name="nIndex1">The index of the first element to be swapped</param>
		/// <param name="nIndex2">The index of the second element to be swapped</param>
		/// <return>Returns an array with elements in nIndex1 and nIndex2 swapped</return>
		public static void SwapElement<T>(this T[] array, int nIndex1, int nIndex2) where T : struct
		{
			if (array != null &&
				nIndex1 >= 0 && nIndex1 < array.Length &&
				nIndex2 >= 0 && nIndex2 < array.Length &&
				nIndex1 != nIndex2)
			{
				T tmp = array[nIndex1];
				array[nIndex1] = array[nIndex2];
				array[nIndex2] = tmp;
			}
		}

		/// <summary>
		/// Convert an array to a one dimensional array.
		/// </summary>
		/// <example>
		/// <code>
		/// array.To1D()
		/// </code>
		/// </example>
		/// <param name="array">this.array to be converted to a single dimension</param>
		/// <returns>Returns a copy of the input array, flattened to one dimension</returns>
		public static Array To1D(this Array array)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0)
			{
				//var thisData = array.GetAt(0);
				oRcd = Array.CreateInstance(array.GetType().GetElementType()!, new int[] { array.Length });
				int bytes = Buffer.ByteLength(array);
				Buffer.BlockCopy(array, 0, oRcd, 0, bytes);
			}
			return oRcd!;
		}

		/// <summary>
		/// Add a single dimension to this array.
		/// </summary>
		/// <example>
		/// <code>
		/// array.Add_1D()
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <returns>Returns a copy of the input array with a single dimension added to it</returns>
		public static List<Array> Add_1D(this Array array)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0)
			{
				int[] thisDims = array.GetDimensions();
				//var thisData = array.GetAt(0);
				int[] newDims = new int[thisDims.Length + 1];
				newDims[0] = 1;
				for (var i = 0; i < thisDims.Length; i++)
				{
					newDims[i + 1] = thisDims[i];
				}
				oRcd = Array.CreateInstance(array.GetType().GetElementType()!, newDims);
				int bytes = Buffer.ByteLength(array);
				Buffer.BlockCopy(array, 0, oRcd, 0, bytes);
			}
			return new List<Array>() { oRcd! };
		}

		/// <summary>
		/// Reverse or permute the axes of an array.
		/// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.ndarray.transpose.html#numpy.ndarray.transpose" >np.transpose()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.Transpose()	// python equivalent: np.transpose(arr)
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="b1Dto2D">true : if array is 1D, convert it to 2D. Ex. {0,1} --> {{0},{1}}</param>
		/// <returns>Returns a view of the array transposed</returns>
		public static object Transpose(this Array array, bool b1Dto2D = false)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0)
			{
				int[] thisDims = array.GetDimensions();
				//var thisData = array.GetAt(0);
				if (thisDims.Length <= 1)
				{//not transpose for 1D
					if (b1Dto2D)
					{//to 2D
						oRcd = Array.CreateInstance(array.GetType().GetElementType()!, new int[] { array.Length, 1 });
						int bytes = Buffer.ByteLength(array);
						Buffer.BlockCopy(array, 0, oRcd, 0, bytes);
					}
					else
					{//to 1D
						oRcd = array.To1D();
					}
				}
				else
				{//over 1D
					int[] newDims = new int[thisDims.Length];
					Array.Copy(thisDims, newDims, thisDims.Length);
					Array.Reverse(newDims);
					int[] thisIndex = new int[thisDims.Length];
					Array.Clear(thisIndex, 0, thisIndex.Length);
					//var thisRangs = new SliceIndex[thisIndex.Length];
					//for (var i = 0; i < thisRangs.Length; i++)
					//{
					//	thisRangs[i] = new SliceIndex()
					//	{
					//		Start = 0,
					//		Stop = thisDims[i],
					//		OriginalLength = thisDims[i]   //must set this before use of it Nxxxx properties
					//	};
					//}
					//create new array
					oRcd = Array.CreateInstance(array.GetType().GetElementType()!, newDims);
					int[] newIndex = new int[thisDims.Length];
					for (var i = 0; i < oRcd.Length; i++)
					{
						object oData = array.GetValue(thisIndex)!;
						//int[] newIndex = (int[])thisIndex.Copy<int>();
						Buffer.BlockCopy(thisIndex, 0, newIndex, 0, Buffer.ByteLength(thisIndex));
						Array.Reverse(newIndex);
						oRcd.SetValue(oData, newIndex);
						//IncreamentIndex(thisIndex, thisRangs);
						thisIndex = thisIndex.Increament(thisDims);
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Reverse or permute the axes of an array.
		/// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.ndarray.transpose.html#numpy.ndarray.transpose">np.transpose()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// int[] dimensions = new int[]{0, 2, 1};
		/// array.Transpose(dimensions);	// python equivalent: np.transpose(arr, (0,2,1))
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="dimensions">An array of integers</param>
		/// <returns>Returns a view of the array with axes transposed. null : dimensions is not correct.</returns>
		public static object Transpose(this Array array, int[] dimensions)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0)
			{
				if (dimensions == null)
				{// just reverse its dimensions
					return array.Transpose();
				}
				else
				{
					int[] thisDims = array.GetDimensions();
					//delete same value from dimensions
					dimensions = dimensions.Distinct().ToArray();
					//each element of dimensions[] must >=0 & <thisDims.Length
					dimensions = dimensions.Where(x => x >= 0 && x < thisDims.Length).ToArray();
					if (dimensions.Length == thisDims.Length)
					{//dimensions size must be same with this array
						int[] newDims = new int[thisDims.Length];
						for (int k = 0; k < dimensions.Length; k++)
							newDims[k] = thisDims[dimensions[k]];

						//Array.Copy(thisDims, newDims, thisDims.Length);
						//Array.Reverse(newDims);
						//int[] thisIndex = new int[thisDims.Length];
						//Array.Clear(thisIndex, 0, thisIndex.Length);
						//var thisRangs = new SliceIndex[thisDims.Length];
						//for (var i = 0; i < thisRangs.Length; i++)
						//{
						//	thisRangs[i] = new SliceIndex()
						//	{
						//		Start = 0,
						//		Stop = thisDims[i],
						//		OriginalLength = thisDims[i]   //must set this before use of it Nxxxx properties
						//	};
						//}
						//int[] targetIndices = new int[thisDims.Length];
						//create new array
						oRcd = Array.CreateInstance(array.GetType().GetElementType()!, newDims);
						int[] SourceIndices = new int[thisDims.Length];
						Array.Clear(SourceIndices, 0, SourceIndices.Length);
						int[] TargetIndices = new int[newDims.Length];
						Array.Clear(TargetIndices, 0, TargetIndices.Length);
						for (var i = 0; i < oRcd.Length; i++)
						{
							//SourceIndices = array.GetIndices(i, true, thisDims, SourceIndices);
							//TargetIndices = oRcd.GetIndices(i, true, newDims, TargetIndices);
							object oData = array.GetValue(SourceIndices)!;
							//int[] newIndex = (int[])thisIndex.Copy<int>();
							//for (int k = 0; k < dimensions.Length; k++)
							//	targetIndices[k] = sourceIndex[dimensions[k]];
							//set the value to new array
							oRcd.SetValue(oData, TargetIndices);
							//IncreamentIndex(thisIndex, thisRangs);
							SourceIndices = SourceIndices.Increament(thisDims);
							//TargetIndices = TargetIndices.Increament(newDims);
							for (int k = 0; k < dimensions.Length; k++)
								TargetIndices[k] = SourceIndices[dimensions[k]];
						}
					}
				}

			}
			return oRcd!;
		}

		/// <summary>
		/// Add the elements of each dimension of two arrays. The dimensions do not have to be the same size.
		/// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.sum.html">np.sum()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.Sum(arr2)	// python equivalent: np.sum(arr, arr2)
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="data">An array that may have different dimension sizes as this.array or a scalar</param>
		/// <returns>Returns an array whose elements have been added together</returns>
		public static Array Sum(this Array array, object data)
		{
			Func<double, double, double> func = (data1, data2) => data1 + data2;
			return ByElement(array, data, func);
		}

        /// <summary>
        /// Subtract the elements of each dimension of two arrays. The dimensions do not have to be the same size.
        /// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.subtract.html">np.subtract()</see></para>
        /// </summary>
        /// <example>
        /// <code>
        /// var result1 = arr.Sub(arr2)	// python equivalent: result1 = np.subtract(arr, arr2) or arr - arr2
		/// var result2 = arr.Sub(10)	// python equivalent: result1 = arr - 10
		/// var result3 = arr.Sub(10, true)	// python equivalent: result1 = 10 - arr
        /// </code>
        /// </example>
        /// <param name="array">this.array</param>
        /// <param name="data">An array that may have different dimension sizes as this.array or a scalar</param>
        /// <param name="bReverse"></param>
        /// <returns>Returns the different of array1 and array2, element-wise</returns>
        public static Array Sub(this Array array, object data, bool bReverse = false)
        {
            Func<double, double, double>? func = null;
            if (bReverse)
            {
                func = (data1, data2) => data2 - data1;
            }
            else
            {
                func = (data1, data2) => data1 - data2;
            }
            return ByElement(array, data, func);
        }

        /// <summary>
        /// Multiply the elements of each dimension of two arrays. The dimensions do not have to be the same size.
        /// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.multiply.html">np.multiply()</see></para>
        /// </summary>
        /// <example>
        /// <code>
        /// array.Multip(arr2)	// python equivalent: np.multiply(arr, arr2)
        /// </code>
        /// </example>
        /// <param name="array">this.array</param>
        /// <param name="data">An array that may have different dimension sizes as this.array or a scalar</param>
        /// <returns>The product of array1 and array2, element-wise</returns>
        public static Array Multip(this Array array, object data)
		{
			Func<double, double, double> func = (data1, data2) => data1 * data2;
			return ByElement(array, data, func);
		}

		/// <summary>
		/// Divide the elements of each dimension of two arrays. The dimensions do not have to be the same size.
		/// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.divide.html">np.divide()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.Div(arr2)	// python equivalent: np.divide(arr, arr2)
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="data">An array that may have different dimension sizes as this.array or a scalar</param>
		/// <returns>The quotient of array1 and array2, element-wise</returns>
		public static Array Div(this Array array, object data)
		{
			Func<double, double, double> func = (data1, data2) => data1 / data2;
			return ByElement(array, data, func);
		}

		///// <summary>
		///// do division
		///// </summary>
		///// <param name="data"></param>
		///// <param name="array"></param>
		///// <returns></returns>
		////public static Array Div(double data, Array array)
		////{
		////	Func<double, double, double> func = (data1, data2) => data1 / data2;
		////	return ByElement(data, array, func);
		////}

		/// <summary>
		/// First array elements raised to powers from second array. The dimensions do not have to be the same size.
		/// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.power.html">np.power()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.Pow(arr2)	// python equivalent: np.power(arr, arr2)
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="data">An array that may have different dimension sizes as this.array or a scalar</param>
		/// <returns>The base in array1 raised to the exponents in array2</returns>
		public static Array Pow(this Array array, object data)
		{
			Func<double, double, double> func = (data1, data2) => Math.Pow(data1, data2);
			return ByElement(array, data, func);
		}

		/// <summary>
		/// Calculate the exponential of all elements in the input array.
		/// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.exp.html">np.exp()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.Exp()	// python equivalent: np.exp(arr)
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <returns>Returns an array, element-wise exponential of this.array</returns>
		public static Array Exp(this Array array)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0)
			{
				int[] dims = array.GetDimensions();
				oRcd = Array.CreateInstance(typeof(double), dims);
				int[] index = new int[dims.Length];
				Array.Clear(index, 0, index.Length);
				for (var i = 0; i < oRcd.Length; i++)
				{
					//int[] index = array.GetIndices(i);
					double dData = (double)Convert.ChangeType(array.GetValue(index), typeof(double))!;
					oRcd.SetValue(Math.Exp(dData), index);
					index = index.Increament(dims);
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Calculate the absolute value element-wise.
		/// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.absolute.html">np.absolute()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.Abs()	// python equivalent: np.abs(arr)
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="amount">Amount is added to all elements in this.array</param>
		/// <param name="mask">An array with bool element, its dimensions must be same as array. if one element is true, the element in array does not participate in calculations. This is for masked array calculation to support what <c>Numpy.ma</c> is doing</param>
		/// <returns>Returns the absolute value</returns>
		public static Array Abs(this Array array, double amount, Array? mask = null)
		{
			Func<double, double, double> func = (data1, amount) => Math.Abs(data1 + amount);
			return ByElement(array, amount, func, mask);
		}

		/// <summary>
		/// Test element-wise for NaN and return result as a boolean array. Notice: all elements of this array must be double or float. for interger data, there is no NaN data. so if array is a int, this one retern all false.
		/// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.isnan.html">np.isnan()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// var boolArray = array.IsNaN()	// python equivalent: np.isnan(arr)
		/// </code>
		/// </example>
		/// <param name="array">this.array, it must be double or float array</param>
		/// <returns>a bool array</returns>
		public static Array IsNaN(this Array array)
		{
			Array? oRcd = null;
			if (array != null &&
				array.Length > 0)
			{
				int[] dims = array.GetDimensions();
				oRcd = Array.CreateInstance(typeof(bool), dims);
				for (var i = 0; i < array.Length; i++)
				{
					double data = (double)Convert.ChangeType(array.GetAt(i), typeof(double));
					oRcd.SetValue(i, double.IsNaN(data));
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// A 2-D array with 1's on the diagonal and zeros elsewhere.
		/// </summary>
		/// <example>
		/// <code>
		/// int[,] arr = Eye(2);
		/// </code>
		/// </example>
		/// <typeparam name="T">Should be of type int, uint, short...</typeparam>
		/// <param name="N">integer that represents how many elements are in the 2-D array</param>
		/// <returns>Returns a 2-D array with 1's on the diagonal</returns>
		public static T[,] Eye<T>(int N)
		{
			T[,]? oRcd = null;
			if (N > 0)
			{
				oRcd = new T[N, N];
				for (var i = 0; i < N; i++)
				{
					oRcd[i, i] = (T)(object)1;

					//for (var j = 0; j < N; j++)
					//{
					//	T data = (T)(object)0;
					//	if (i == j)
					//	{
					//		data = (T)(object)1;
					//	}
					//	oRcd[i, j] = data;
					//}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Get the length of each dimension and return each value in an int[].
		/// </summary>
		/// <example>
		/// <code>
		/// array.GetDimensions();
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <returns>Returns an int[] containing the length of each dimensions, or null</returns>
		public static int[] GetDimensions(this Array array)
		{
			int[]? oRcd = null;
			if (array != null)
			{
				oRcd = new int[array.Rank];
				for (int i = 0; i < oRcd.Length; i++)
				{
					oRcd[i] = array.GetLength(i);
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Upper triangle of an array. Return a copy of an array with the elements below the k-th diagonal zeroed. 
		/// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.triu.html">np.triu()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.Triu(1);	// python equivalent: np.triu(arr, 1)
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="k">An integer representing which diagonal to start the upper triangle</param>
		/// <returns></returns>
		public static Array Triu(this Array array, int k = 0)
		{
			Array? oRcd = null;
			if (array != null &&
				array.Length > 0)
			{
				int[] dims = array.GetDimensions();
				//object data = array.GetAt(0);
				//copy data
				oRcd = Array.CreateInstance(array.GetType().GetElementType()!, dims);
				Buffer.BlockCopy(array, 0, oRcd, 0, Buffer.ByteLength(array));
				//clear some element
				int[] index = new int[dims.Length];
				Array.Clear(index, 0, index.Length);
				var rangs = new SliceIndex[index.Length];
				for (var i = 0; i < rangs.Length; i++)
				{
					rangs[i] = new SliceIndex()
					{
						Start = 0,
						Stop = dims[i],
						OriginalLength = dims[i]   //must set this before use of it Nxxxx properties
					};
				}
				for (var i = 0; i < array.Length; i++)
				{
					if (index[^1] - (index.Length > 1 ? index[^2] : 0) < k)
					{//clear this element
						oRcd.SetValue(0, index);
					}
					IncreamentIndex(index, rangs);
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Gives a new shape to an array without changing its data.
		/// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.reshape.html">np.reshape()</see></para>
		/// <para>See <see href="https://eli.thegreenplace.net/2015/memory-layout-of-multi-dimensional-arrays">Memory layout of multi-dimensional arrays</see> for understanding FortuanMemorySequence.</para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.Reshape([2,3,1]);	// python equivalent: np.reshape(arr, (2,3,1))
		/// </code>
		/// </example>
		/// <param name="array">this.array that will be reshaped</param>
		/// <param name="newDims">The set of new dimensions. Dimension -1 means we have calculate it is calculated by using other dimension(s) and array.Length, only have one -1 here can be acceptable. 0 is not acceptable. Total item number must equals array.Length </param>
		/// <param name="FortranMemorySequence">true: change it to Fortran array memory sequence, false: keep current memory sequence</param>
		/// <returns>
		/// Returns a new array. in case of null :<br/>
		/// 1. newDims is not correct. <br/>
		/// 2. It cotains 0, <br/>
		/// 3. More than 1 elements are -1.<br/>
		/// 4. new item count not equals this array's count<br/>
		/// 5. This array is null
		/// </returns>
		public static Array Reshape(this Array array, int[] newDims, bool FortranMemorySequence = false)
		{
			Array oRcd = array;
			if (array != null && array.Length > 0 && newDims != null && newDims.Length > 0)
			{
				int nMinus = newDims.Where(x => x < 0).Count();
				int nZero = newDims.Where(x => x == 0).Count();
				if (nZero <= 0 && //no zero element
					nMinus <= 1) //only one -1 element or no -1 element
				{
					int[] dims = array.GetDimensions();
					int nCount = dims.Aggregate(1, (a, b) => a * b);
					// new Count may be a minus number, so make it to plus
					int nNewCount = Math.Abs(newDims.Aggregate(1, (a, b) => a * b));
					if (nMinus != 0)
					{//contains one -1 item
					 //find the -1 item, get it real number
						for (int i = 0; i < newDims.Length; i++)
						{
							if (newDims[i] < 0)
							{
								//calculate real number for this dimension
								newDims[i] = nCount / nNewCount;
								//recalculate the new count
								nNewCount *= newDims[i];
								break;  //only has one item whick is -1
							}
						}
					}
					if (nNewCount == nCount)
					{
						//object data = array.GetAt(0);
						oRcd = Array.CreateInstance(array.GetType().GetElementType()!, newDims);
						Buffer.BlockCopy(array, 0, oRcd, 0, Buffer.ByteLength(array));

						//memory sequence
						if (FortranMemorySequence &&
							newDims.Length > 1)
						{//change memory sequence to Fortran type
							Array Temp = Array.CreateInstance(array.GetType().GetElementType()!, newDims);
							//int nLast2DLength = newDims[^1] * newDims[^2];
							//for (int i=0;i<oRcd.Length;i++)
							//{
							//	int[] Index = oRcd.GetIndices(i);
							//	var Value = oRcd.GetValue(Index);
							//	//make a Fortran sequence index
							//	int nx = Index[^2];
							//	int ny = Index[^1];
							//	int nNumber = nx * newDims[^1] + ny; 
							//	Index[^2] = nNumber % newDims[^2];
							//	Index[^1] = nNumber / newDims[^2];
							//	Temp.SetValue(Value, Index);
							int[] thisIndices = new int[newDims.Length];
							Array.Clear(thisIndices, 0, thisIndices.Length);
							int[] newIndices = new int[newDims.Length];
							Array.Clear(newIndices, 0, newIndices.Length);
							for (int i = 0; i < oRcd.Length; i++)
							{
								var Value = oRcd.GetValue(thisIndices);
								//int[] newIndex = oRcd.GetIndices(i, false);
								Temp.SetValue(Value, newIndices);
								thisIndices.Increament(newDims);
								newIndices.Increament(newDims, false);
							}
							oRcd = Temp;
						}
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Dot product of two arrays.Specifically,<br/>
		/// If both arrayA and arrayB are 1-D arrays, it is inner product of vectors (without complex conjugation).<br/>
		/// If both arrayA and arrayB are 2-D arrays, it is matrix multiplication.
		/// <para>See Numpy reference: <seealso href = "https://numpy.org/doc/stable/reference/generated/numpy.dot.html">np.dot()</seealso></para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.Dot(arr2);	// python equivalent: np.dot(arr1, arr2)
		/// </code>
		/// </example>
		/// <param name="arrayA">this.arrayA, first argument</param>
		/// <param name="arrayB">Second argument</param>
		/// <returns>Returns the dot product of arrayA and arrayB. If arrays are both 1-D arrays then a scalar is returned; otherwise an array is returned.</returns>
		public static object Dot(this Array arrayA, Array arrayB)
		{
			object? oRcd = null;
			if (arrayA != null && arrayB != null)
			{
				if (arrayA.Rank == 1 && arrayB.Rank == 1)
				{//inner product of vectors
					MathNet.Numerics.LinearAlgebra.Vector<double> vA = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray((double[])arrayA);
					MathNet.Numerics.LinearAlgebra.Vector<double> vB = MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray((double[])arrayB);
					oRcd = vA * vB;
					//oRcd = arrayA.Multip(arrayB);
				}
				else if (arrayA.Rank == 2 && arrayB.Rank == 2)
				{//matrix multiplication
					Matrix<double> mA = DenseMatrix.OfArray((double[,])arrayA.Copy<double>());
					Matrix<double> mB = DenseMatrix.OfArray((double[,])arrayB.Copy<double>());
					oRcd = (mA * mB).ToArray();
					//oRcd = arrayA.Multip(arrayB);
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Stack arrays in sequence depth wise (along third axis).
		/// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.dstack.html">np.dstack()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.Dstack(arr2);	// python equivalent: np.dstack((arr1, arr2))
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="b">Second array to be used to stack onto this.array</param>
		/// <returns>Returns the array formed by stacking the given arrays, will be at least 3-D</returns>
		public static Array Dstack(this Array array, Array b)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0 &&
				b != null && b.Length > 0)
			{
				Array thisTemp = array;
				if (array.Rank < 2)
				{
					thisTemp = (Array)array.Transpose(true);
				}
				Array bTemp = b;
				if (b.Rank < 2)
				{
					bTemp = (Array)b.Transpose(true);
				}
				int[] thisDims = thisTemp.GetDimensions();
				int[] bDims = bTemp.GetDimensions();
				if (thisDims.Length >= bDims.Length)
				{//able to docking
					bool bContinue = true;
					if (thisDims.Length != bDims.Length)
					{//must be same
						int nTemp = thisDims.Length - bDims.Length;
						for (var i = 0; i < nTemp; i++)
						{
							if (thisDims[i] != 1)
							{
								bContinue = false;
								break;
							}
						}
						if (bContinue)
						{
							int[] tempDims = Enumerable.Repeat(1, thisDims.Length).ToArray();
							//Array.Clear(tempDims, 0, nTemp);
							Array.Copy(bDims, 0, tempDims, nTemp, bDims.Length);
						}
					}
					if (bContinue)
					{
						oRcd = thisTemp.Concatenate(bTemp, thisDims.Length - 1);
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Get the minimum value/values of the array along an axis.<br/>
		/// See Numpy reference: <seealso href="https://numpy.org/doc/stable/reference/generated/numpy.amin.html?highlight=amin">np.amin()</seealso>
		/// </summary>
		/// <example>
		/// <code>
		/// array.Amin(1);	// python equivalent: np.amin(arr, axis=1)
		/// </code>
		/// </example>
		/// <param name="array">this.array, input array</param>
		/// <param name="nAxis">Axis along which to operate. If null: return minimum of all elements</param>
		/// <returns>Returns the minimum value(s) in the array. if nAxis is null return an minimum value from whole data</returns>
		public static object Amin(this Array array, int? nAxis = null)
		{
			//for minimum
			Func<object, object, bool> func = (data1, data2) => ((IComparable)data1).CompareTo((IComparable)data2) > 0;
			return array.CheckedByAxis(nAxis, func);
		}

		/// <summary>
		/// Get the maximum value/values of the array along an axis.<br/>
		/// See Numpy reference: <seealso href="https://numpy.org/doc/stable/reference/generated/numpy.amax.html?highlight=amax">np.amax()</seealso>
		/// </summary>
		/// <example>
		/// <code>
		///		int[] dims = new int[] { 3, 2, 5 };
		///		var intArray = Array.CreateInstance(typeof(int), dims);
		///		//set data to intArray by using array class's extension
		///		for(var i=0; i&lt;intArray.Length; i++)
		///		{
		///			intArray.SetValue(i, (object)(int) i);
		///		}
		///		var maxIndex = intArray.Amax();
		///		//maxIndex : 29
		///		var maxIndices0 = intArray.Amax(0);
		///		//maxIndices0 :
		///		//[[20 21 22 23 24]
		///		// [25 26 27 28 29]]
		///		var maxIndices1 = intArray.Amax(1);
		///		//maxIndices1 :
		///		//[[ 5  6  7  8  9]
		///		// [15 16 17 18 19]
		///		// [25 26 27 28 29]]
		/// </code>
		/// </example>
		/// <param name="array">this.array, input array</param>
		/// <param name="nAxis">Axis along which to operate. If null: return maximum of all elements</param>
		/// <returns>Returns maximum value(s) in the array. if nAxis is null return an maximum value from whole data</returns>
		public static object Amax(this Array array, int? nAxis = null)
		{
			//for maximum
			Func<object, object, bool> func = (data1, data2) => ((IComparable)data1).CompareTo((IComparable)data2) < 0;
			return array.CheckedByAxis(nAxis, func);
		}

		/// <summary>
		/// Get the index / indices of the minimum values of the array along an axis. For example, see <see cref="ArgMax"/><br/>
		/// See Numpy reference: <seealso href="https://numpy.org/doc/stable/reference/generated/numpy.argmin.html">np.argmin()</seealso>
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="nAxis">An axis</param>
		/// <returns>Index / indices, if nAxis is null return an index from whole data</returns>
		public static object ArgMin(this Array array, int? nAxis = null)
		{
			Func<object, object, bool> func = (data1, data2) => ((IComparable)data1).CompareTo((IComparable)data2) > 0;
			return array.CheckedByAxis(nAxis, func, false);
		}

		/// <summary>
		/// Get the index / indices of the maximum values of the array along an axis.<br/>
		/// See Numpy reference: <seealso href="https://numpy.org/doc/stable/reference/generated/numpy.argmax.html">np.argmax()</seealso>
		/// </summary>
		/// <example>
		/// <code>
		///		int[] dims = new int[] { 3, 2, 5 };
		///		var intArray = Array.CreateInstance(typeof(int), dims);
		///		//set data to intArray by using array class's extension
		///		for(var i=0; i&lt;intArray.Length; i++)
		///		{
		///			intArray.SetValue(i, (object)(int) i);
		///		}
		///		var maxIndex = intArray.ArgMax();
		///		//maxIndex : 29
		///		var maxIndices0 = intArray.ArgMax(0);
		///		//maxIndices0 :
		///		//[[2 2 2 2 2]
		///		// [2 2 2 2 2]]
		///		var maxIndices1 = intArray.ArgMax(1);
		///		//maxIndices1 :
		///		//[[1 1 1 1 1]
		///		// [1 1 1 1 1]
		///		// [1 1 1 1 1]]
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="nAxis">An axis</param>
		/// <returns>Index / indices, if nAxis is null return an index from whole data</returns>
		public static object ArgMax(this Array array, int? nAxis = null)
		{
			Func<object, object, bool> func = (data1, data2) => ((IComparable)data1).CompareTo((IComparable)data2) < 0;
			return array.CheckedByAxis(nAxis, func, false);
		}

		/// <summary>
		/// Repeat elements of an array.
		/// <para>See Numpy reference: <seealso href = "https://numpy.org/doc/stable/reference/generated/numpy.repeat.html">np.repeat()</seealso></para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.Repeat(3, 1);	// python equivalent: np.repeat(arr, 3, axis=1)
		/// </code>
		/// </example>
		/// <param name="array">this.array, input array</param>
		/// <param name="nRepeat">The number of repetitions for each element. Repeats is broadcasted to fir the shape of the given axis</param>
		/// <param name="nAxis">The axis along which to repeat values</param>
		/// <returns>Returns the output array which has the same shape as this.array, except along the given axis</returns>
		public static Array Repeat(this Array array, int nRepeat, int? nAxis = null)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0 && nRepeat > 0)
			{
				int[] dims = array.GetDimensions();
				//object oData = array.GetAt(0);
				if (nAxis == null)
				{//repeat each element nRepeat times, and return 1D array
					oRcd = Array.CreateInstance(array.GetType().GetElementType()!, new int[] { array.Length * nRepeat });
					for (var i = 0; i < array.Length; i++)
					{
						object oData = array.GetAt(i);
						for (var j = 0; j < nRepeat; j++)
						{
							oRcd.SetValue(i * (long)nRepeat + j, oData);
						}
					}
				}
				else
				{//find max values along with nAxis
					int nTemp = (int)nAxis;
					if (nTemp < 0)
					{
						nTemp = 0;
					}
					else if (nTemp >= dims.Length)
					{
						nTemp = dims.Length - 1;
					}
					int[] thisIndex = new int[dims.Length];
					int[] newDims = new int[dims.Length];
					int[] newIndex = new int[dims.Length];
					var thisRangs = new SliceIndex[dims.Length];
					for (var i = 0; i < dims.Length; i++)
					{
						thisIndex[i] = 0;
						newDims[i] = dims[i];
						if (i == nTemp)
						{
							newDims[i] *= nRepeat;
						}
						newIndex[i] = 0;
						thisRangs[i] = new SliceIndex()
						{
							Start = 0,
							Stop = dims[i],
							OriginalLength = dims[i]   //must set this before use of it Nxxxx properties
						};
					}
					oRcd = Array.CreateInstance(array.GetType().GetElementType()!, newDims);
					for (var i = 0; i < oRcd.Length; i++)
					{
						object oData = array.GetValue(thisIndex)!;
						//copy current index
						Array.Copy(thisIndex, 0, newIndex, 0, thisIndex.Length);
						for (var j = 0; j < nRepeat; j++)
						{
							newIndex[nTemp] = thisIndex[nTemp] * nRepeat + j;
							oRcd.SetValue(oData, newIndex);
						}
						IncreamentIndex(thisIndex, thisRangs);
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Rotate an array by 90 degrees.
		/// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.rot90.html">np.rot90()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.Rot90(1);	// python equivalent: np.rot90(arr, 1)
		/// </code>
		/// </example>
		/// <param name="array">this.array, input array</param>
		/// <param name="k">The number of times the array is rotated by 90 degrees</param>
		/// <returns>A rotated view of this.array. If array is null or  its Rank less and equal than 1, return null</returns>
		//public static Array Rot90(this Array array, int k = 1)
		//{
		//	Array? oRcd = null;
		//	if ((array != null) && (array.Rank > 1))
		//	{
		//		//normalize k
		//		int nK = k % 4; //0,1,2,3 or 0,-3,-2 -1
		//		int[] thisDims = array.GetDimensions();
		//		//var thisData = array.GetAt(0);
		//		if (nK == 0)
		//		{//no change, return its copy
		//			oRcd = Array.CreateInstance(array.GetType().GetElementType()!, thisDims);
		//			int bytes = Buffer.ByteLength(oRcd);
		//			Buffer.BlockCopy(array, 0, oRcd, 0, bytes);
		//		}
		//		else
		//		{//nK!=0
		//			if (nK < 0)
		//			{//normalizt to 1(90),2(180),3(270)
		//				nK += 4;
		//			}
		//			int[] dstDims = new int[thisDims.Length];
		//			Array.Copy(thisDims, dstDims, thisDims.Length);
		//			if (nK != 2)
		//			{
		//				//swap dims[0] and dims[1]
		//				int nTemp = dstDims[0];
		//				dstDims[0] = dstDims[1];
		//				dstDims[1] = nTemp;
		//			}
		//			//new array
		//			oRcd = Array.CreateInstance(array.GetType().GetElementType()!, dstDims);
		//			//get memory handle of each array, and lock those memroy from garbage collection(prevents the garbage collector from moving the object and hence undermines the efficiency of the garbage collector)
		//			GCHandle gchArray = GCHandle.Alloc(array, GCHandleType.Pinned);
		//			GCHandle gchNew = GCHandle.Alloc(oRcd, GCHandleType.Pinned);
		//			IntPtr ptrArray = gchArray.AddrOfPinnedObject();
		//			IntPtr ptrNew = gchNew.AddrOfPinnedObject();
		//			//copy data block size in bytes
		//			int blockSize = 1;
		//			for (var i = 2; i < thisDims.Length; i++)
		//			{
		//				blockSize *= thisDims[i];
		//			}
		//			blockSize *= Buffer.ByteLength(array) / array.Length;
		//			//block copy
		//			unsafe
		//			{
		//				//fist data point for each array
		//				byte* pArray = (byte*)ptrArray.ToPointer();
		//				byte* pNew = (byte*)ptrNew.ToPointer();
		//				for (var i = 0; i < thisDims[0]; i++)
		//				{
		//					for (var j = 0; j < thisDims[1]; j++)
		//					{
		//						int thisOffset = (i * thisDims[1] + j) * blockSize;
		//						int newOffset = 0;
		//						if (nK == 2)
		//						{
		//							newOffset = ((thisDims[0] - 1 - i) * thisDims[1] + thisDims[1] - 1 - j) * blockSize;
		//						}
		//						else if (nK == 1)
		//						{
		//							newOffset = ((thisDims[1] - 1 - j) * thisDims[0] + i) * blockSize;
		//						}
		//						else if (nK == 3)
		//						{
		//							newOffset = (j * thisDims[0] + (thisDims[0] - 1 - i)) * blockSize;
		//						}
		//						Buffer.MemoryCopy(pArray + thisOffset, pNew + newOffset, blockSize, blockSize);
		//					}
		//				}
		//			}
		//			//unlock those memroy from garbage collection
		//			gchArray.Free();
		//			gchNew.Free();
		//		}
		//	}

		//	return oRcd!;
		//}
		/// <summary>
		/// Rotate an array by 90 degrees. Rotation direction is from the first towards the second axis. This means for a 2D array with the default k and axes, the rotation will be counterclockwise.
		/// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.rot90.html">np.rot90()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// var data = array.Rot90(1);	// python equivalent: np.rot90(arr, 1)
		/// var data = array.Rot90(1,(1,2)); //is equivalent to data = array.Rot90(-1,(2,1))
		/// </code>
		/// </example>
		/// <param name="array">this.array, input array</param>
		/// <param name="k">Number of times the array is rotated by 90 degrees ()</param>
		/// <param name="axes">The array is rotated in the plane defined by the axes. Default is null that means (0,1). this must be a 2 elements array. and all value must be &gt;=0 &amp; &lt; array.Rank</param>
		/// <returns>A rotated view of this.array. If array is null or  its Rank less and equal than 1, return null</returns>
		public static Array Rot90(this Array array, int k = 1, int[]? axes = null)
		{
			Array? oRcd = null;
			if (array != null && array.Rank > 1)
			{	//array must be at least 2 Rank
				if(axes == null)
				{	//for default axes
					axes = new int[2] {0,1 };
				}
				if (axes.Length == 2)
				{   //axes must have two elements
					//delete same value in axes
					axes = axes.Distinct().ToArray();
					if (axes.Length == 2)
					{   //do not have same value in axis
						axes = axes.NormalizeAxes(array.Rank);// nimus will be converted to puls, and now value out of 0 to rank-1
															  //normalize k
						int nK = k % 4; //0,1,2,3 or 0,-3,-2 -1
						int[] thisDims = array.GetDimensions();
						if (nK == 0)
						{   //no change, return its copy
							oRcd = Array.CreateInstance(array.GetType().GetElementType()!, thisDims);
							int bytes = Buffer.ByteLength(oRcd);
							Buffer.BlockCopy(array, 0, oRcd, 0, bytes);
						}
						else
						{   //nK!=0
							if (axes[0] > axes[1])
							{
								nK *= -1;
								//swap the 2 values
								int nTemp = axes[0];
								axes[0] = axes[1];
								axes[1] = nTemp;
							}
							if (nK < 0)
							{//normalizt to 1(90),2(180),3(270)
								nK += 4;
							}
							int[] dstDims = new int[thisDims.Length];
							Array.Copy(thisDims, dstDims, thisDims.Length);
							if (nK != 2)
							{
								//swap dims[0] and dims[1]
								int nTemp = dstDims[axes[0]];
								dstDims[axes[0]] = dstDims[axes[1]];
								dstDims[axes[1]] = nTemp;
							}
							//new array
							oRcd = Array.CreateInstance(array.GetType().GetElementType()!, dstDims);
							//get memory handle of each array, and lock those memroy from garbage collection(prevents the garbage collector from moving the object and hence undermines the efficiency of the garbage collector)
							GCHandle gchArray = GCHandle.Alloc(array, GCHandleType.Pinned);
							GCHandle gchNew = GCHandle.Alloc(oRcd, GCHandleType.Pinned);
                            nint ptrArray = gchArray.AddrOfPinnedObject();
                            nint ptrNew = gchNew.AddrOfPinnedObject();
							//one element size
							int elementSize = Buffer.ByteLength(array) / array.Length;
							//copy data block size in bytes
							int blockSize = 1;
							for (var i = axes[1] + 1; i < thisDims.Length; i++)
							{
								blockSize *= thisDims[i];
							}
							blockSize *= elementSize;

							//indices of this array
							int[] thisIndices = new int[thisDims.Length];
							int?[] nullIndices = new int?[thisDims.Length];
							for(int i=0; i < nullIndices.Length; i++)
							{
								if (i > axes[1])
								{	//corrosponding index will be fixed number.
									nullIndices[i] = 0;
								}
								else
								{	//those indices will be increaced one by one
									nullIndices[i] = null;
								}
							}
							thisIndices.InitializeStartIndices(nullIndices);
							unsafe
							{
								//fist data point for each array
								byte* pArray = (byte*)ptrArray.ToPointer();
								byte* pNew = (byte*)ptrNew.ToPointer();
								//case 1 normal logic
								int[] dstIndices = new int[thisIndices.Length];
								//do
								//{
								//	//current nIndices 
								//	long thisOffset = thisIndices.LongIndex(thisDims) * elementSize;
								//	//create an index array for target array
								//	Array.Copy(thisIndices, dstIndices, thisIndices.Length);
								//	if (nK == 1)
								//	{//90
								//		dstIndices[axes[0]] = thisDims[axes[1]] - 1 - thisIndices[axes[1]];
								//		dstIndices[axes[1]] = thisIndices[axes[0]];
								//		//newOffset = ((thisDims[axes[1]] - 1 - j) * thisDims[axes[0]] + i) * element0 * blockSize;
								//	}
								//	else if (nK == 2)
								//	{//180
								//		dstIndices[axes[0]] = thisDims[axes[0]] - 1 - thisIndices[axes[0]];
								//		dstIndices[axes[1]] = thisDims[axes[1]] - 1 - thisIndices[axes[1]];
								//		//newOffset = ((thisDims[axes[0]] - 1 - i) * thisDims[axes[1]] + thisDims[axes[1]] - 1 - j) * blockSize;
								//	}
								//	else if (nK == 3)
								//	{//270
								//		dstIndices[axes[0]] = thisIndices[axes[1]];
								//		dstIndices[axes[1]] = thisDims[axes[0]] - 1 - thisIndices[axes[0]];
								//		//newOffset = (j * thisDims[axes[0]] + (thisDims[axes[0]] - 1 - i)) * blockSize;
								//	}
								//	long newOffset = dstIndices.LongIndex(dstDims) * elementSize;
								//	Buffer.MemoryCopy(pArray + thisOffset, pNew + newOffset, blockSize, blockSize);
								//} while (thisIndices.Increament(nullIndices, thisDims));

								// case 2 using multiple thread, 10 times of case 1, do not use this. don't konw why???
								//List<Task> tasks = new List<Task>();
								//do
								//{
								//	//local function : copy specific indices data to to destination, make sure srcIndices must be copied (do not user reference data of thisIndices, because thisIndices will be increaced in main thread)
								//	void CopySrcToDst(int[] srcIndices)
								//	{
								//		//Debug.Assert(srcIndices.Length == thisDims.Length);
								//		// dist indices
								//		int[] dstIndices = new int[srcIndices.Length];
								//		//current nIndices 
								//		long thisOffset = srcIndices.LongIndex(thisDims) * elementSize;
								//		//create an index array for target array
								//		Array.Copy(srcIndices, dstIndices, srcIndices.Length);
								//		if (nK == 1)
								//		{//90
								//			dstIndices[axes[0]] = thisDims[axes[1]] - 1 - srcIndices[axes[1]];
								//			dstIndices[axes[1]] = srcIndices[axes[0]];
								//			//newOffset = ((thisDims[axes[1]] - 1 - j) * thisDims[axes[0]] + i) * element0 * blockSize;
								//		}
								//		else if (nK == 2)
								//		{//180
								//			dstIndices[axes[0]] = thisDims[axes[0]] - 1 - srcIndices[axes[0]];
								//			dstIndices[axes[1]] = thisDims[axes[1]] - 1 - srcIndices[axes[1]];
								//			//newOffset = ((thisDims[axes[0]] - 1 - i) * thisDims[axes[1]] + thisDims[axes[1]] - 1 - j) * blockSize;
								//		}
								//		else if (nK == 3)
								//		{//270
								//			dstIndices[axes[0]] = srcIndices[axes[1]];
								//			dstIndices[axes[1]] = thisDims[axes[0]] - 1 - srcIndices[axes[0]];
								//			//newOffset = (j * thisDims[axes[0]] + (thisDims[axes[0]] - 1 - i)) * blockSize;
								//		}
								//		long newOffset = dstIndices.LongIndex(dstDims) * elementSize;
								//		Buffer.MemoryCopy(pArray + thisOffset, pNew + newOffset, blockSize, blockSize);
								//	}
								//	Task oTask = Task.Run(() => CopySrcToDst((int[])thisIndices.Clone()));
								//	tasks.Add(oTask);
								//} while (thisIndices.Increament(nullIndices, thisDims));
								//Task.WaitAll(tasks.ToArray());
								//unlock those memroy from garbage collection

								//case 3 use Span<T>
								do
								{
									//current nIndices 
									long thisOffset = thisIndices.LongIndex(thisDims) * elementSize;
									//create an index array for target array
									Array.Copy(thisIndices, dstIndices, thisIndices.Length);
									if (nK == 1)
									{//90
										dstIndices[axes[0]] = thisDims[axes[1]] - 1 - thisIndices[axes[1]];
										dstIndices[axes[1]] = thisIndices[axes[0]];
										//newOffset = ((thisDims[axes[1]] - 1 - j) * thisDims[axes[0]] + i) * element0 * blockSize;
									}
									else if (nK == 2)
									{//180
										dstIndices[axes[0]] = thisDims[axes[0]] - 1 - thisIndices[axes[0]];
										dstIndices[axes[1]] = thisDims[axes[1]] - 1 - thisIndices[axes[1]];
										//newOffset = ((thisDims[axes[0]] - 1 - i) * thisDims[axes[1]] + thisDims[axes[1]] - 1 - j) * blockSize;
									}
									else if (nK == 3)
									{//270
										dstIndices[axes[0]] = thisIndices[axes[1]];
										dstIndices[axes[1]] = thisDims[axes[0]] - 1 - thisIndices[axes[0]];
										//newOffset = (j * thisDims[axes[0]] + (thisDims[axes[0]] - 1 - i)) * blockSize;
									}
									long newOffset = dstIndices.LongIndex(dstDims) * elementSize;
									Span<byte> spanArray = new(pArray + thisOffset, blockSize);
									Span<byte> spanNew = new(pNew + newOffset, blockSize);
									spanArray.CopyTo(spanNew);
									//Buffer.MemoryCopy(pArray + thisOffset, pNew + newOffset, blockSize, blockSize);
								} while (thisIndices.Increament(nullIndices, thisDims));
								gchArray.Free();
								gchNew.Free();
							}
						}
					}
				}
			}
			return oRcd!;
		}
		/// <summary>
		/// a 1D array index of a multi dimeensional array
		/// </summary>
		/// <param name="Indices">indices for multi dimesional array</param>
		/// <param name="dims">dimesion of the multidemensional array</param>
		/// <returns>the long index in 1D</returns>
		public static long LongIndex(this int[] Indices, int[] dims)
		{
			long nRcd = 0;
			if(Indices != null && 
				dims!=null&&
				dims.Length==Indices.Length)
			{
				nRcd = Indices[0];
				for(int i=1; i<Indices.Length; i++)
				{
					nRcd = nRcd * dims[i] + Indices[i];
				}
			}
			return nRcd;
		}

		/// <summary>
		/// Take elements from an array along an axis.
		/// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.take.html">np.take()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.take([0,1,4]);	// python equivalent: np.take(arr, [0,1,4])
		/// </code>
		/// </example>
		/// <param name="array">this.array, input array</param>
		/// <param name="indices">An interger or an array of the indices of the values to extract</param>
		/// <param name="nAxis">The axis over which to select values. If null: number in the indices are the serial number in array, that means if the array.Length is 100, the serial is 0,1,2,...99</param>
		/// <returns>Returns the an output array that had the same type as this.array</returns>
		public static object Take(this Array array, object indices, int? nAxis = null)
		{
			object? oRcd = null;
			if (array != null && indices != null)
			{
				//object oData = array.GetAt(0);
				Type oType = array.GetType().GetElementType()!;
				if (nAxis == null)
				{//indices are the serial number in array, that means if the array.Lenght is 100, the serial is 0,1,2,...99
					if (indices is int)
					{//return a element
						int nIndex = (int)indices;
						if (nIndex >= 0 && nIndex < array.Length)
						{
							oRcd = array.GetAt(nIndex);
						}
					}
					else if (indices is Array)
					{
						Array aIndices = (Array)indices;
						int[] newDims = aIndices.GetDimensions();
						Array aTemp = Array.CreateInstance(oType, newDims);
						oRcd = aTemp;
						for (var i = 0; i < aIndices.Length; i++)
						{
							int nTemp = (int)aIndices.GetAt(i);
							if (nTemp < 0 || nTemp >= array.Length)
							{// bad index
								oRcd = null;
								break;
							}
							aTemp.SetValue(i, array.GetAt(nTemp));
						}
					}
				}
				else
				{//indices are number in the axis
					int axis = (int)nAxis;
					int[] dims = array.GetDimensions();
					if (axis >= 0 && axis < dims.Length)
					{//have right axis number
						int[]? newDims = null;
						if (indices is int)
						{// int index
							newDims = new int[dims.Length - 1];
							//make new dimension
							Array.Copy(dims, 0, newDims, 0, axis);
							Array.Copy(dims, axis + 1, newDims, axis, dims.Length - 1 - axis);
							int nLoop = 1;  // number of data to be copied for one index
							int blockSizeArray = Buffer.ByteLength(array) / array.Length; //copy data block size in bytes
							for (int i = 0; i < dims.Length; i++)
							{
								if (i < axis)
								{
									nLoop *= dims[i];
								}
								else if (i > axis)
								{
									blockSizeArray *= dims[i];
								}
							}
							int nIndex = (int)indices;
							if (nIndex >= 0 && nIndex < dims[axis])
							{//right index
								int jumpSizeNew = blockSizeArray;  //jump to next size for new array
																   //new array
								Array oNew = Array.CreateInstance(oType, newDims);
								oRcd = oNew;
								GCHandle gchArray = GCHandle.Alloc(array, GCHandleType.Pinned);
								GCHandle gchNew = GCHandle.Alloc(oNew, GCHandleType.Pinned);
                                nint ptrArray = gchArray.AddrOfPinnedObject();
                                nint ptrNew = gchNew.AddrOfPinnedObject();

								int startPosArray = blockSizeArray * nIndex;  //start address to be copied of thes array
								int startPosNew = 0;   //start address to be copied of new array
								int jumpSizeArray = blockSizeArray * dims[axis];  //jump to next size for this array
								unsafe
								{
									//fist data point for each array
									byte* pArray = (byte*)ptrArray.ToPointer() + startPosArray;
									byte* pNew = (byte*)ptrNew.ToPointer() + startPosNew;
									for (var i = 0; i < nLoop; i++)
									{
										Buffer.MemoryCopy(pArray, pNew, blockSizeArray, blockSizeArray);
										pArray += jumpSizeArray;
										pNew += jumpSizeNew;
									}
								}
								//unlock those memroy from garbage collection
								gchArray.Free();
								gchNew.Free();
							}
						}
						else if (indices is Array)
						{
							Array aIndices = (Array)indices;
							int[] indexDims = aIndices.GetDimensions();
							newDims = new int[indexDims.Length + dims.Length - 1];
							//make new dimension
							Array.Copy(dims, 0, newDims, 0, axis);
							Array.Copy(indexDims, 0, newDims, axis, indexDims.Length);
							Array.Copy(dims, axis + 1, newDims, axis + indexDims.Length, dims.Length - 1 - axis);
							int blockSizeArray = Buffer.ByteLength(array) / array.Length; //copy data block size in bytes
							int nLoop = 1;// number of data to be copied for one index
							for (int i = 0; i < dims.Length; i++)
							{
								if (i < axis)
								{
									nLoop *= dims[i];
								}
								else if (i > axis)
								{
									blockSizeArray *= dims[i];
								}
							}
							int jumpSizeNew = blockSizeArray * indexDims.Aggregate(1, (a, b) => a * b);  //jump to next size for new array
																										 //new array
							Array oNew = Array.CreateInstance(oType, newDims);
							oRcd = oNew;
							GCHandle gchArray = GCHandle.Alloc(array, GCHandleType.Pinned);
							GCHandle gchNew = GCHandle.Alloc(oNew, GCHandleType.Pinned);
                            nint ptrArray = gchArray.AddrOfPinnedObject();
                            nint ptrNew = gchNew.AddrOfPinnedObject();
							for (var j = 0; j < aIndices.Length; j++)
							{
								int[] aIndex = aIndices.GetIndices(j);
								int nIndex = (int)aIndices.GetValue(aIndex)!;
								if (nIndex < 0 || nIndex >= dims[axis])
								{//bad index
									oRcd = null;
									break;
								}
								int startPosArray = blockSizeArray * nIndex;    //start address to be copied of thes array
								int startPosNew = j * blockSizeArray;   //start address to be copied of new array
								int jumpSizeArray = blockSizeArray * dims[axis];  //jump to next size for this array

								unsafe
								{
									//fist data point for each array
									byte* pArray = (byte*)ptrArray.ToPointer() + startPosArray;
									byte* pNew = (byte*)ptrNew.ToPointer() + startPosNew;
									for (var i = 0; i < nLoop; i++)
									{
										Buffer.MemoryCopy(pArray, pNew, blockSizeArray, blockSizeArray);
										pArray += jumpSizeArray;
										pNew += jumpSizeNew;
									}
								}
							}
							//unlock those memroy from garbage collection
							gchArray.Free();
							gchNew.Free();
						}
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Roll array elements along a given axis. Elements that roll beyond the last position are re-introduced at the first.
		/// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.roll.html">np.roll()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.Roll(1,0);		// python equivalent: np.roll(arr,1,0)
		/// array.Roll(-2,3);	// python equivalent: np.roll(arr,-2,3)
		/// </code>
		/// </example>
		/// <param name="array">this.array, input array</param>
		/// <param name="nShift">The number of places by which elements are shifted</param>
		/// <param name="nAxis">Axis which elements are shifted, can be minus</param>
		/// <returns>Returns an output array, which the same shape as this.array</returns>
		public static Array Roll(this Array array, int nShift, int nAxis)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0)
			{
				int[] thisDims = array.GetDimensions();
				if (nAxis < 0)
				{
					nAxis += thisDims.Length;
				}
				if (nAxis < thisDims.Length)
				{//correct Axis
				 // make output array, total same dimesions
					Type elementType = array.GetType().GetElementType()!;
					oRcd = Array.CreateInstance(elementType, thisDims);
					GCHandle gchArray = GCHandle.Alloc(array, GCHandleType.Pinned);
					GCHandle gchNew = GCHandle.Alloc(oRcd, GCHandleType.Pinned);
                    nint ptrArray = gchArray.AddrOfPinnedObject();
                    nint ptrNew = gchNew.AddrOfPinnedObject();
					//how many elements have to copy
					int nElements = 1;
					for (var i = nAxis + 1; i < thisDims.Length; i++)
					{
						nElements *= thisDims[i];
					}
					//those elements data size
					int nTotalSize = Buffer.ByteLength(array);
					int nElementSize = nTotalSize / array.Length;
					int blockSize = nElements * nElementSize;
					int shiftRangSize = blockSize * thisDims[nAxis];
					nShift = nShift % thisDims[nAxis];
					unsafe
					{
						//fist data point for each array
						byte* pArray = (byte*)ptrArray.ToPointer();
						byte* pNew = (byte*)ptrNew.ToPointer();
						if (nShift == 0)
						{//do have to do shift, just copy from source array
							Buffer.MemoryCopy(pArray, pNew, nTotalSize, nTotalSize);
						}
						else
						{
							byte* pArrayTemp = pArray;
							while (pArrayTemp < pArray + nTotalSize)
							{
								byte* pNewTemp = pNew + nShift * blockSize;
								for (int i = 0; i < thisDims[nAxis]; i++)
								{
									if (pNewTemp < pNew)
									{
										pNewTemp += shiftRangSize;
									}
									else if (pNewTemp >= pNew + shiftRangSize)
									{
										pNewTemp -= shiftRangSize;
									}
									Buffer.MemoryCopy(pArrayTemp, pNewTemp, blockSize, blockSize);
									pArrayTemp += blockSize;
									pNewTemp += blockSize;
								}
								pNew += shiftRangSize;
							}
						}
					}
					//unlock those memroy from garbage collection
					gchArray.Free();
					gchNew.Free();
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Roll array elements along a given axis.Elements that roll beyond the last position are re-introduced at the first.
		/// <para>See Numpy reference: <see href = "https://numpy.org/doc/stable/reference/generated/numpy.roll.html">np.roll()</see></para>
		/// </summary>
		/// <example>
		/// <code>
		/// array.Roll(1);		// python equivalent: np.roll(arr,1)
		/// array.Roll(-2,new int[]{1,0});	// python equivalent: np.roll(arr,-2,(1,0))
		/// </code>
		/// </example>
		/// <param name="array">this.array, input array</param>
		/// <param name="nShift">The number of places by which elements are shifted</param>
		/// <param name="Axis">Axes along which elements are shifted. Given 1 or more Axis, if this is a null, roll it as a 1D array</param>
		/// <returns>Returns an output array, which the same shape as this.array</returns>
		public static Array Roll(this Array array, int nShift, int[]? Axis = null)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0)
			{
				if (Axis == null)
				{
					int[] thisDims = array.GetDimensions();
					// make output array, total same dimesions
					Type elementType = array.GetType().GetElementType()!;
					oRcd = Array.CreateInstance(elementType, thisDims);
					GCHandle gchArray = GCHandle.Alloc(array, GCHandleType.Pinned);
					GCHandle gchNew = GCHandle.Alloc(oRcd, GCHandleType.Pinned);
                    nint ptrArray = gchArray.AddrOfPinnedObject();
                    nint ptrNew = gchNew.AddrOfPinnedObject();
					//how many elements have to copy
					//those elements data size
					int nTotalSize = Buffer.ByteLength(array);
					int nElementSize = nTotalSize / array.Length;
					nShift = nShift % array.Length;
					unsafe
					{
						//fist data point for each array
						byte* pArray = (byte*)ptrArray.ToPointer();
						byte* pNew = (byte*)ptrNew.ToPointer();
						if (nShift == 0)
						{//do have to do shift, just copy from source array
							Buffer.MemoryCopy(pArray, pNew, nTotalSize, nTotalSize);
						}
						else
						{
							byte* pArrayTemp = pArray;
							byte* pNewTemp = pNew + nShift * nElementSize;
							while (pArrayTemp < pArray + nTotalSize)
							{
								if (pNewTemp < pNew)
								{
									pNewTemp += nTotalSize;
								}
								else if (pNewTemp >= pNew + nTotalSize)
								{
									pNewTemp -= nTotalSize;
								}
								Buffer.MemoryCopy(pArrayTemp, pNewTemp, nElementSize, nElementSize);
								pArrayTemp += nElementSize;
								pNewTemp += nElementSize;
							}
						}
					}
					//unlock those memroy from garbage collection
					gchArray.Free();
					gchNew.Free();
				}
				else
				{
					oRcd = array;
					foreach (int nAxis in Axis)
					{
						oRcd = oRcd.Roll(nShift, nAxis);
						if (oRcd == null)
						{//axis error, 
							break;
						}
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Get Hex string from the array. Notice: for half, float, double, only return its memory bytes in Hex, for othe it Hex string will be in Big edian byte order. for example 0x1234 will be "12 34" in HEX not "34 12"
		/// </summary>
		/// <typeparam name="T">element type of the array</typeparam>
		/// <param name="array">this array, shoulbe be a value type</param>
		/// <param name="strSpliter">a spliter bettween each value, defult is now</param>
		/// <returns>a Hex string of the array</returns>
		public static string ToHexString<T>(this T[] array, string strSpliter = "") where T : struct
		{
			string strRcd = string.Empty;
			if (array != null && array.Length > 0)
			{
				Type elementType = array.GetType().GetElementType()!;
				if (elementType != typeof(decimal))
				{//do not support Decimal
				 //copy all byte to a byte array
					int nByteLength = Buffer.ByteLength(array);
					byte[] btTemp = new byte[nByteLength];
					Buffer.BlockCopy(array, 0, btTemp, 0, btTemp.Length);
					int nSizeElement = nByteLength / array.Length;
					bool bIsSingleByte = nSizeElement == 1; //for byte, sbyte and bool (inside of array bool data is 1 byte)
					bool bIsFloatValue = elementType == typeof(Half) || elementType == typeof(float) || elementType == typeof(double) || elementType == typeof(decimal);
					if (BitConverter.IsLittleEndian &&
						!bIsSingleByte &&
						!bIsFloatValue)
					{
						//element size must >1
						//Debug.Assert(nSizeElement > 1);
						for (int i = 0; i < btTemp.Length; i += nSizeElement)
						{
							Array.Reverse(btTemp, i, nSizeElement);
						}
					}
					//byte[] --> Hex string, ex. {0,1,2,3, 0xff} --> 00-01-02-03-FF
					strRcd = BitConverter.ToString(btTemp);
					if (bIsSingleByte)
					{//change spliter from "-" to our own spliter, strSpliter can by string.empty
						strRcd = strRcd.Replace("-", strSpliter);
					}
					else
					{
						//remove all "-"
						strRcd = strRcd.Replace("-", "");
						//and our own spliter
						if (!string.IsNullOrEmpty(strSpliter))
						{//need add a spliter
							for (int i = strRcd.Length - nSizeElement * 2; i > 0; i -= nSizeElement * 2)
							{
								strRcd = strRcd.Insert(i, strSpliter);
							}
						}
					}
				}
			}
			return strRcd;
		}

		#region FAT Capture data decoding
		/// <summary>
		/// Decode the caputure data from ODMB. For more details check out <see href="https://gitlab.com/redlen_dev/test-software/FAT-Cloud-Blazor/-/blob/fatcloud_v1.0.18/Documents/Capture%20Data%20Format%20for%20FAT%20Cloud.docx?ref_type=heads">Capture Data Format for FAT Cloud.docx</see> 
		/// See also parse_view() in dm_tp\pcct\buffer\odmb.py. For ODMB v2.10 and above see <see href="https://gitlab.com/redlen_dev/test-software/dm-tp/-/merge_requests/462#add-odmb-data-filtering-support-506">Add support for ODMB 2.1 features (#381, #472, #506)</see>
		/// </summary>
		/// <example>
		/// Data format:
		/// <code>
		///     Capture 0
		///         View 0
		///             Header (80bytes) + Data (uint or ushort). the byte order is little endian
		///                 Data is a 3 dimension aray [nCounter,nRow, nColomn]
		///         View 1
		///             Header (80bytes) + Data
		///         ......
		///         View N
		///             Header (80bytes) + Data
		///     Capture 1
		///         View 0
		///             Header (80bytes) + Data
		///         ......
		///         View N
		///             Header (80bytes) + Data
		///			......
		///     Capture M
		///         View 0
		///             Header (80bytes) + Data
		///         ......
		///         View N
		///             Header (80bytes) + Data
		/// </code>
		/// </example>
		/// <param name="array">A byte array that contains all the capture data</param>
		/// <param name="nViews">Number of views</param>
		/// <param name="bDropFirstFrame">The Capture data comes with many frames. bDropFirstFrame is to allow us to decode the captured data without the first frame. The first frame contains a lof of noice, so we do not use this</param>
		/// <param name="nCaptures">The number of captures</param>
		/// <param name="nCounters">The number of counters</param>
		/// <param name="bAccum">true: means each couter data is a 32-bit unsigned integer. false: means that each counter data is a 16-bit unsigned integer</param>
		/// <param name="nRowPerAsic">number of rows for an ASIC</param>
		/// <param name="nColumnPerAsic">number of columns for an ASIC</param>
		/// <param name="nMiniModules">number of mini moudles</param>
		/// <param name="nAsicsPerMM"></param>
		/// <returns>A multi dimensional array for ODMB capture data. Dimensions are [nCaptures(if have), nViews, nRow, nColumn, nCounters]</returns>
		public static Array ODMBDecode(this byte[] array, int nViews, bool bDropFirstFrame = false, int nCaptures = 1, int nCounters = 13, bool bAccum = false, int nRowPerAsic = 24, int nColumnPerAsic = 36, int nMiniModules = 8, int nAsicsPerMM = 2)
		{
            // view_dtype.itemsize: = 89856 ... data = data[view_dtype.itemsize:]
            Array? oRcd = null;
			if (nCaptures > 0 &&
				nViews > 0 &&
				nCounters > 0)
			{
				const int nHeaderSize = 80;
				int nNewViews = bDropFirstFrame ? nViews - 1 : nViews;
				//one ASIC size is 24 * 36, here we have 8*2 ASICs
				int nRow = nRowPerAsic * nMiniModules;  //==192
				int nColumn = nColumnPerAsic * nAsicsPerMM;   //==72
				int nOneDataSize = bAccum ? sizeof(uint) : sizeof(ushort);
				int nAViewDataSize = nCounters * nRow * nColumn * nOneDataSize;
				int nAviewSize = nHeaderSize + nAViewDataSize;
				int nSourceDataSize = nCaptures * nViews * nAviewSize; // 11502848 (dm-tp: (1438896))
                if (array != null &&
					array.Length == nSourceDataSize &&
					array.Rank == 1)
				{
					int[] dims = nCaptures > 1 ? new int[] { nCaptures, nNewViews, nCounters, nRow, nColumn } : new int[] { nNewViews, nCounters, nRow, nColumn }; //16, 0, 13, 192, 72 (dm-tp 16, 1, 13, 24, 72)
					Type oType = bAccum ? typeof(uint) : typeof(ushort); // uint32 GOOD
					oRcd = Array.CreateInstance(oType, dims);
					//get memory handle of each array, and lock those memroy from garbage collection(prevents the garbage collector from moving the object and hence undermines the efficiency of the garbage collector)
					GCHandle gchArray = GCHandle.Alloc(array, GCHandleType.Pinned);
					GCHandle gchNew = GCHandle.Alloc(oRcd, GCHandleType.Pinned);
                    nint ptrArray = gchArray.AddrOfPinnedObject();
                    nint ptrNew = gchNew.AddrOfPinnedObject();
					unsafe
					{
						//fist data point for each array
						byte* pArray = (byte*)ptrArray.ToPointer();
						byte* pNew = (byte*)ptrNew.ToPointer();
						for (int i = 0; i < nCaptures; i++)
						{//each capture
							for (int j = 0; j < nNewViews; j++)
							{//each vies
								if (j == 0 && bDropFirstFrame)
								{//skip first frame
									pArray += nAviewSize;
								}
								//skip header data
								pArray += nHeaderSize;
								//copy data block, each uint or ushort data is in little endian byte order,
								Buffer.MemoryCopy(pArray, pNew, nAViewDataSize, nAViewDataSize);
								pArray += nAViewDataSize;
								pNew += nAViewDataSize;
							}
						}
					}
					//unlock those memroy from garbage collection
					gchArray.Free();
					gchNew.Free();
					oRcd = oRcd.MoveAxis(-3, -1);//[nCaptures, nNewViews, nCounters, nRow, nColumn] --> [nCaptures, nNewViews, nRow, nColumn, nCounters]
					oRcd = (Array)oRcd.Flip(-1);
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Decode the caputure data from DM buffer. For more details check out <see href="https://gitlab.com/redlen_dev/test-software/FAT-Cloud-Blazor/-/blob/fatcloud_v1.0.18/Documents/Capture%20Data%20Format%20for%20FAT%20Cloud.docx?ref_type=heads">Capture Data Format for FAT Cloud.docx</see>.
		/// See also parse_views() in dm_tp\pcct\buffer\dm_buffer.py.
		/// </summary>
		/// <example>
		/// <code>
		///     Capture 0
		///         View 0
		///             Header (80bytes) + Data (uint or ushort). the byte order is little endian
		///             Data (Ex. for uint, notice: byte order is big endian, so the first byte is highst byte)
		///             [0x00],[0x04],[0x08],[0x0c] --> [ASIC=0, Row=0, Cloumn=0, Counter=0]	[0,0,0,0] = 00 00 01 CA	--> 0x01CA (458)
		///				[0x01],[0x05],[0x09],[0x0d] --> [ASIC=0, Row=12, Cloumn=0, Counter=0]	[0,23,0,0] = 00 00 4B 5A --> 0x4B5A(19290)
		///				[0x02],[0x06],[0x0a],[0x0e] --> [ASIC=1, Row=0, Cloumn=0, Counter=0]	[1,0,0,0] = 00 00 00 5E	--> 0x005E()
		///				[0x03],[0x07],[0x0b],[0x0f] --> [ASIC=1, Row=12, Cloumn=0, Counter=0]	[1,23,0,0]
		///             [0x10],[0x14],[0x18],[0x1c] --> [ASIC=0, Row=1, Cloumn=0, Counter=0]	[0,1,0,0] = 00 00 00 00 --> 0x0000(0)
		///             [0x11],[0x15],[0x19],[0x1d] --> [ASIC=0, Row=13, Cloumn=0, Counter=0]
		///				[0x12],[0x16],[0x1a],[0x1e] --> [ASIC=1, Row=1, Cloumn=0, Counter=0]
		///				[0x13],[0x17],[0x1b],[0x1f] --> [ASIC=1, Row=13, Cloumn=0, Counter=0]
		///             ....
		///             [0xb0],[0xb4],[0xb8],[0xbc] --> [ASIC=0, Row=11, Cloumn=0, Counter=0]	00 00 00 29 --> 0x0029(41)
		///             [0xb1],[0xb5],[0xb9],[0xbd] --> [ASIC=0, Row=23, Cloumn=0, Counter=0]	00 00 00 00 --> 0x0000(0)
		///				[0xb2],[0xb6],[0xba],[0xbe] --> [ASIC=1, Row=11, Cloumn=0, Counter=0]	00 00 00 0E --> 0x000E(14)
		///				[0xb3],[0xb7],[0xbb],[0xbf] --> [ASIC=1, Row=23, Cloumn=0, Counter=0]	00 00 00 02 --> 0x0002(2)
		///             [0xc0],[0xc4],[0xc8],[0xcc] --> [ASIC=0, Row=0, Cloumn=0, Counter=1]	00 00 00 00 --> 0x0000(0)
		///             [0xc1],[0xc5],[0xc9],[0xcd] --> [ASIC=0, Row=12, Cloumn=0, Counter=1]	00 00 00 00 --> 0x0000(0)
		///				[0xc2],[0xc6],[0xca],[0xce] --> [ASIC=1, Row=0, Cloumn=0, Counter=1]	00 00 00 00 --> 0x0000(0)
		///				[0xc3],[0xc7],[0xcb],[0xcf] --> [ASIC=1, Row=12, Cloumn=0, Counter=1]	00 00 00 00 --> 0x0000(0)
		///             ...
		///             [0x9b0],[0x9b4],[0x9b8],[0x9bc] --> [ASIC=0, Row=11, Cloumn=0, Counter=12]
		///             [0x9c0],[0x9c4],[0x9c8],[0x9cc] --> [ASIC=0, Row=0, Cloumn=1, Counter=0]
		///             ...
		///             [0x1380],[0x13844],[0x1388],[0x138c] --> [ASIC=0, Row=0, Cloumn=2, Counter=0]
		///             ...
		///             [0x1d40],[0x13844],[0x1388],[0x138c] --> [ASIC=0, Row=0, Cloumn=3, Counter=0]
		///             ...
		///             [0x15540],[0x15544],[0x15548],[0x1554c] --> [ASIC=0, Row=0, Cloumn=35, Counter=0]
		///             [0x15f10],[0x15f14],[0x15f18],[0x15f1c] --> [ASIC=0, Row=11, Cloumn=35, Counter=12]
		///             [0x15f11],[0x15f15],[0x15f19],[0x15f1d] --> [ASIC=0, Row=23, Cloumn=35, Counter=12]
		///				[0x15f12],[0x15f16],[0x15f1a],[0x15f1e] --> [ASIC=1, Row=11, Cloumn=35, Counter=12]
		///				[0x15f13],[0x15f17],[0x15f1b],[0x15f1f] --> [ASIC=1, Row=23, Cloumn=35, Counter=12]
		///         View 1
		///             Header (80bytes) + Data
		///         ......
		///         View N
		///             Header (80bytes) + Data
		///     Capture 1
		///         View 0
		///             Header (80bytes) + Data
		///         ......
		///         View N
		///             Header (80bytes) + Data
		///     ......
		///     Capture M
		///         View 0
		///             Header (80bytes) + Data
		///         ......
		///         View N
		///             Header (80bytes) + Data
		/// </code>
		/// </example>
		/// <param name="array">A byte array that contains all the capture data</param>
		/// <param name="nViews">Number of views</param>
		/// <param name="nCaptures">The number of captures</param>
		/// <param name="nCounters">The number of counters</param>
		/// <param name="bAccum">true: means each couter data is a 32-bit unsigned integer. false: means that each counter data is a 16-bit unsigned integer</param>
		/// <returns>A multi dimensional array for DM Buffer capture data. Dimensions are [nCaptures(if have), nViews, nRow, nColumn, nCounters]</returns>
		public static Array DMBufferDecode(this byte[] array, int nViews, int nCaptures = 1, int nCounters = 13, bool bAccum = false)
		{
			Array? oRcd = null;
			if (nCaptures > 0 &&
				nViews > 0 &&
				nCounters > 0)
			{
				//number of ASIC
				int nAsic = 2;
				//one ASIC size is 24 * 36
				int nRow = 24;
				int nColumn = 36;
				int nOneDataSize = bAccum ? sizeof(uint) : sizeof(ushort);
				int nHeaderSize = bAccum ? 16 + 16 : 4 + 8; //View_Number_len = 16 ro 4, preamble_len = 16 or 8, header size is View_Number_len + preamble_len
				int nAViewDataSize = nAsic * nRow * nColumn * nCounters * nOneDataSize;
				int nAviewSize = nHeaderSize + nAViewDataSize;
				int nSourceDataSize = nCaptures * nViews * nAviewSize;
				if (array != null &&
					array.Length == nSourceDataSize &&
					array.Rank == 1)
				{
					int[] dims = nCaptures > 1 ? new int[] { nCaptures, nViews, nAsic, nRow, nColumn, nCounters } : new int[] { nViews, nAsic, nRow, nColumn, nCounters };
					Type oType = bAccum ? typeof(uint) : typeof(ushort);
					oRcd = Array.CreateInstance(oType, dims);
					//get memory handle of each array, and lock those memroy from garbage collection(prevents the garbage collector from moving the object and hence undermines the efficiency of the garbage collector)
					GCHandle gchArray = GCHandle.Alloc(array, GCHandleType.Pinned);
					GCHandle gchNew = GCHandle.Alloc(oRcd, GCHandleType.Pinned);
                    nint ptrArray = gchArray.AddrOfPinnedObject();
                    nint ptrNew = gchNew.AddrOfPinnedObject();
					unsafe
					{
						//fist data point for each array
						byte* pArray = (byte*)ptrArray.ToPointer();
						byte* pNew = (byte*)ptrNew.ToPointer();
						for (int i = 0; i < nCaptures; i++)
						{//each capture
							for (int j = 0; j < nViews; j++)
							{//for each vies
							 //skip header data for this View
								pArray += nHeaderSize;
								for (int k = 0; k < nAsic * 2; k++)
								{//or each ASIC/2 (0,1 : for ASICS 1 (0:for nRow/2, 1 for rest of the rows), 2,3 : for ASIC2... )
									int nIndexCounter = 0;
									int nIndexColumn = 0;
									bool bSecondHalf = k % 2 != 0;  //2 half for 1 ASIC, firs half Row : 0->nRow/2-1, second half Row : nRow-1, nRow-2,..nRow/2+1,nRow/2
									int nIndexRow = bSecondHalf ? nRow - 1 : 0;
									int nMaxRow = 12;
									for (byte* pPosSrc = pArray + k; pPosSrc < pArray + nAViewDataSize; pPosSrc += 2 * nAsic * nOneDataSize)
									{//scan all view data for this ASIC
									 //this item's address in oRcd memory map
										byte* pTargetPos = pNew + (((k / 2 * nRow + nIndexRow) * nColumn + nIndexColumn) * nCounters + nIndexCounter) * nOneDataSize;
										//Copy bytes to oRcd. source data's byte oreder is big endian
										for (int l = nOneDataSize - 1; l >= 0; l--)
										{
											pTargetPos[0] = pPosSrc[l * 2 * nAsic];
											pTargetPos++;
										}
										//firs half Row : 0->nRow/2-1. (increase from 0 to nRow/2-1)
										//second half Row : nRow-1, nRow-2,..nRow/2+1,nRow/2. (decrease from nRow-1 to nRow/2)
										nIndexRow = bSecondHalf ? nIndexRow - 1 : nIndexRow + 1;
										if (bSecondHalf && nIndexRow < nMaxRow ||   //for second half
											!bSecondHalf && nIndexRow >= nMaxRow)   //for first half
										{
											//reset Row index
											nIndexRow = bSecondHalf ? nRow - 1 : 0;
											//increase counter
											nIndexCounter++;
											if (nIndexCounter >= nCounters)
											{
												//reset counter
												nIndexCounter = 0;
												//increase column
												nIndexColumn++;
												if (nIndexColumn >= nColumn)
												{//should not be here
												 //reset column
													nIndexColumn = 0;
												}
											}
										}
									}
								}
								//to next view data
								pArray += nAViewDataSize;
								pNew += nAViewDataSize;
							}
						}
					}
					//unlock those memroy from garbage collection
					gchArray.Free();
					gchNew.Free();
					//reverse all counter data
					oRcd = (Array)oRcd.Flip(-1);
				}
			}
			return oRcd!;
		}
		
		#endregion

		#region FAT Register data service
		/// <summary>
		/// split a ushort[8] to Die_ID contents. for deatail see TIAsic.RegisterName.Die_ID
		/// </summary>
		/// <param name="array">an ushort[8]</param>
		/// <returns>a DynamicDictionary contains all contents of Die_ID, null : when the array is not correct</returns>
		public static dynamic ParseDieID(this ushort[] array)
		{
			dynamic? oRcd = null;
			if (array != null && array.Length >= 8)
			{
				//make a Little endian order byte[16]
				byte[] bArray = new byte[16];
				int nIndex = 0;
				for (int i = 7; i >= 0; i--)
				{
					byte[] btData = BitConverter.GetBytes(array[i]);
					if (!BitConverter.IsLittleEndian)
					{
						Array.Reverse(btData);
					}
					Array.Copy(btData, 0, bArray, nIndex, 2);
					nIndex += 2;
				}
				oRcd = bArray.ParseDieID(false);
			}
			return oRcd!;
		}

        /// <summary>
        /// split a Guid (128 bit data) to Die_ID contents. for deatail see TIAsic.RegisterName.Die_ID
        /// </summary>
        /// <param name="guid">a Guid</param>
        /// <returns></returns>
        public static dynamic ParseDieID(this Guid guid)
		{
            return guid.ToByteArray().ParseDieID();
        }

		/// <summary>
		/// create DieID contest from 16 byte data (128 bit)
		/// </summary>
		/// <param name="data">16 bytes</param>
		/// <param name="isBigEndian">true : data is in big endian order</param>
		/// <returns></returns>
		/// <example>
		/// <code>
		/// //a 16 byte data, its byte order is Bigendian
		/// byte[] myDieID = new byte[]{0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x11,0x01,0x00,0x00,0x1f};
		/// var DieID = myDiID.ParseDiesID();
		/// //get the DieID in guid format
		/// Guid oGuid = DieID.Guid.ToByteArray();
		/// </code>
		/// </example>
		public static dynamic ParseDieID(this byte[] data, bool isBigEndian = true)
		{
            DynamicDictionary? oRcd = null;
			if (data != null && data.Length >= 16)
			{
                oRcd = new DynamicDictionary();
                //create a Biginteger from the byte[16]
                BigInteger DieID128bit = new BigInteger(data, true, isBigEndian);
                //whole data in 128 bits. use .ToString("X") to get its Hex string.
                oRcd.Set("DieID", (UInt128)DieID128bit);
				//DieID as Guid
				if (!isBigEndian)
				{
					Array.Reverse(data);
				}
				oRcd.Set("Guid", new Guid(data));
                //WaferX : B11:00
                oRcd.Set("WaferX", (ushort)(DieID128bit & 0x0fff));
                //WaferY : B23:12
                DieID128bit >>= 12;
                oRcd.Set("WaferY", (ushort)(DieID128bit & 0x0fff));
                //WaferNumber : B29:24
                DieID128bit >>= 12;
                oRcd.Set("WaferNumber", (byte)(DieID128bit & 0x03f));
                //Reserved1 : B31:30, according to the document, this 2 bits ware not used.???
                DieID128bit >>= 6;
                oRcd.Set("Reserved1", (byte)(DieID128bit & 0x03));
                //LotNumber : B55:32
                DieID128bit >>= 2;
                oRcd.Set("LotNumber", (uint)(DieID128bit & 0x00ffffff));
                //Reserved2 : B63:56
                DieID128bit >>= 24;
                oRcd.Set("Reserved2", (byte)(DieID128bit & 0x0ff));
                //TRIM2_QVDD_INT : B65:64
                DieID128bit >>= 8;
                oRcd.Set("TRIM2_QVDD_INT", (byte)(DieID128bit & 0x03));
                //Reserved3 : B127:66
                DieID128bit >>= 2;
                oRcd.Set("Reserved3", (ulong)DieID128bit);
            }
            return oRcd!;
		}

		/// <summary>
		/// Make Die ID
		/// </summary>
		/// <param name="data">Die ID in byte[], its length must be 16</param>
		/// <param name="nLotNumbe">Lot number</param>
		/// <param name="nWaferNumber">Wafer number</param>
		/// <param name="WaferX">Wafer X</param>
		/// <param name="WaferY">Wafer Y</param>
		/// <param name="TRIM2_QVDD_INT">TRIM2_QVDD_INT, default is 3</param>
		public static void CreateDieID(this byte[] data, uint nLotNumbe, byte nWaferNumber, ushort WaferX, ushort WaferY, byte TRIM2_QVDD_INT = 3)
		{
			if (data != null && data.Length == 16)
			{
				BigInteger nData = new BigInteger(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, true);
				//Reserved3 : B127:66
				nData <<= 62;
				//TRIM2_QVDD_INT : B65:64
				nData <<= 2;
				nData |= 0x03 & TRIM2_QVDD_INT;
				//Reserved2 : B63:56
				nData <<= 8;
				//LotNumber : B55:32
				nData <<= 24;
				nData |= 0x00ffffff & nLotNumbe;
				//Reserved1 : B31:30
				nData <<= 2;
				//WaferNumber : B29:24
				nData <<= 6;
				nData |= 0x3f & nWaferNumber;
				//WaferY : B23:12
				nData <<= 12;
				nData |= 0x0fff & WaferY;
				//WaferX : B11:00
				nData <<= 12;
				nData |= 0x0fff & WaferX;
				//byte array in big endian order
				byte[] btTemp = nData.ToByteArray(true, true);
				Array.Clear(data);
				Array.Copy(btTemp, 0, data, btTemp.Length > data.Length ? 0 : data.Length - btTemp.Length, btTemp.Length > data.Length ? data.Length : btTemp.Length);
			}
		}

		/// <summary>
		/// Swap byte[] data by 2 bytes. for example {0x12 0x34 0x56 0x78 0x9a} --> {0x34 0x12 0x78 0x56 0x9a}
		/// </summary>
		/// <param name="data"></param>
		public static void SwapBy2Bytes(this byte[] data)
		{
			if (data != null)
			{
				for (int i = 0; i < data.Length; i += 2)
				{
					if (i + 1 < data.Length)
					{
						byte btTemp = data[i];
						data[i] = data[i + 1];
						data[i + 1] = btTemp;
					}
				}
			}
		}
		#endregion

		#region used by those extensions

		/// <summary>
		/// Increament index of a multi dimensional array.
		/// </summary>
		/// <param name="indices">current indices to be increemented. it must be <c>int[]</c></param>
		/// <param name="ranges">
		///		Slice index for each dimension. Must set up OriginalLength for each Slice index in order to use its Nxxxx properties. And each element can be a SliceIndex object or a int[], each int[] element is one of the element index in current dimension. by using int[].
		/// </param>
		/// <returns>void</returns>
		public static void IncreamentIndex(int[] indices, object[] ranges)
		{
			if (indices != null &&
				indices.Length == ranges.Length)
			{
				for (int i = indices.Length - 1; i >= 0; i--)
				{
					if (ranges[i] is SliceIndex oSlice)
					{//SliceIndex class specified Start, Stop 
						indices[i] += oSlice.NStep;
						if (oSlice.NStep > 0)
						{
							if (indices[i] >= oSlice.NStop)
							{
								indices[i] = oSlice.NStart;
							}
							else
							{
								break;
							}
						}
						else
						{
							if (indices[i] <= oSlice.NStop)
							{
								indices[i] = oSlice.NStart;
							}
							else
							{
								break;
							}
						}
					}
					else if (ranges[i] is int[] oIndex)
					{
						int nNext = -1;
						for (var j = 0; j < oIndex.Length; j++)
						{
							if (oIndex[j] == indices[i])
							{//find next
								nNext = j + 1;
								break;
							}
						}
						if (nNext > 0)
						{
							if (nNext < oIndex.Length)
							{
								indices[i] = oIndex[nNext];
								break;
							}
							else
							{
								indices[i] = oIndex[0];
							}
						}
						else
						{//should not be here
						}
					}
					else
					{//unsupported index data type. only SliceIndex & int[3] are supported
						break;
					}
				}
			}
		}

		/// <summary>
		/// Increament Indice base one dims. see <seealso cref="Increament(int[], int?[], int[])"/> &amp; <seealso cref="IncreamentIndex(int[], object[])"/>
		/// </summary>
		/// <param name="Indices">current indices to be increemented. it must be <c>int[]</c></param>
		/// <param name="dims">related array dimensions as a <c>int[]</c></param>
		/// <param name="bMemorySequence">false: the data sequence starts from dim[0], when we read the mat file, we must use this option. true : data sequence starts from dim[0]</param>
		/// <returns>Indice after increament. null if wrong argument</returns>
		public static int[] Increament(this int[] Indices, int[] dims, bool bMemorySequence = true)
		{
			int[]? oRcd = null;
			if (dims != null &&
				Indices != null &&
				dims.Length > 0 &&
				dims.Length == Indices.Length)
			{
				int nStart = bMemorySequence ? -(Indices.Length - 1) : 0;
				int nEnd = bMemorySequence ? 0 : Indices.Length - 1;
				int nMinus = bMemorySequence ? -1 : 1;
				for (int nIndex = nStart; nIndex <= nEnd; nIndex++)
				{
					int nTemp = nIndex * nMinus;
					Indices[nTemp]++;
					if (Indices[nTemp] < dims[nTemp])
					{
						break;
					}
					Indices[nTemp] = 0;
				}
				oRcd = Indices;
			}
			return oRcd!;
		}

		/// <summary>
		/// Increament Indice base on nIndicesWithNull[] and dims[]. if nIndicesWithNull[i] is null See <seealso cref="Increament(int[], int[], bool)"/> &amp; <seealso cref="IncreamentIndex(int[], object[])"/>
		/// </summary>
		/// <param name="Indices">current indices to be increemented. it must be <c>int[]</c></param>
		/// <param name="nIndicesWithNull">a indices with same length of Indices. If a element is null, it means this index can be incrematend base on related dims[i]. null means all the elements of Indices can be incrementd /></param>
		/// <param name="dims">related array dimensions as a <c>int[]</c></param>
		/// <returns>fales:can not do incremental, that means current Indices is last indices. true: can use this Increament() again</returns>
		public static bool Increament(this int[] Indices, int?[] nIndicesWithNull, int[] dims)
		{
			bool bRcd = false;
			if (dims != null &&
				Indices != null &&
				nIndicesWithNull != null &&
				dims.Length > 0 &&
				dims.Length == Indices.Length)
			{
				for (int i = Indices.Length - 1; i >= 0; i--)
				{
					if (nIndicesWithNull[i] != null)
					{   //this index may be fixed index, does not have to be incremented
						continue;
					}
					Indices[i]++;
					if (Indices[i] < dims[i])
					{
						bRcd = true;
						break;
					}
					Indices[i] = 0;
				}
			}
			return bRcd;
		}

		/// <summary>
		/// Map indices to other indices with it dimensions.<br/>
		/// </summary>
		/// <example>
		/// <list type="table">
		///		<listheader>
		///			<term>Orignal Indices</term>
		///			<term>Target Dimensions</term>
		///			<term>Target Indices</term>
		///		</listheader>
		///		<item>
		///			<term>[7,0,2]</term>
		///			<term>[9,1,1]</term>
		///			<term>[7,0,0]</term>
		///		</item>
		///		<item>
		///		<item>
		///			<term>[7,0,2]</term>
		///			<term>[1,1,9]</term>
		///			<term>[0,0,2]</term>
		///		</item>
		///		</item>
		/// </list>
		/// </example>
		/// <param name="srcIndices">Orignal indices</param>
		/// <param name="dims">Dimensions for the result indices</param>
		/// <returns>Returns an output array of the mapping of indices</returns>
		public static int[] IndicesMapping(int[] srcIndices, int[] dims)
		{
			int[]? oRcd = null;
			if (srcIndices != null && dims != null)
			{
				oRcd = new int[dims.Length];
				Array.Clear(oRcd, 0, oRcd.Length);
				int nsrcDims = srcIndices.Length - 1;
				for (int i = oRcd.Length - 1; i >= 0; i--)
				{
					if (nsrcDims >= 0)
					{
						oRcd[i] = srcIndices[nsrcDims] % dims[i];
					}
					nsrcDims--;
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Common logic for getting data by using axes.
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="axises">int[]: Axes to gather data from</param>
		/// <param name="func">A function to compute double[] data</param>
		/// <param name="bIgnorNan">true: Ignor double.Nan value</param>
		/// <returns>Returns an object</returns>
		public static object NanByAxis(Array array, int[] axises, Func<double[], double> func, bool bIgnorNan = false)
		{
            object? oRcd = null;
            if (array != null &&
                array.Length > 1)
            {
                //orignal array dims
                var orignalDims = array.GetDimensions();
                //normalize Axises
                var normalAxises = axises?.NormalizeAxes(orignalDims.Length);
                if (normalAxises != null)
                {
                    if (normalAxises.Length >= orignalDims.Length)
                    {
                        normalAxises = null;
                    }
                    else
                    {
                        //sort it
                        Array.Sort(normalAxises);
                    }
                }
                if (normalAxises == null || normalAxises.Length <= 0)
                {//for all data
                    double[] data = (double[])array.Copy<double>(true);
                    if (bIgnorNan)
                    {//remove all NaN from the double array
                        data = data.Where(e => !double.IsNaN(e)).ToArray();
                    }
					return func(data);
					//if (double.IsNaN((double)oRcd) || double.IsInfinity((double)oRcd))
					//    oRcd = default(double);
				}
                else
                {//normalAsixes may have at least one element that should be in rang of 0--array.Rank
                 //get dimensions of this array
                    var rangsForIncreament = new SliceIndex[normalAxises.Length];
                    var axisesIndex = new int[normalAxises.Length];
                    var axisesLength = 1;
                    for (var i = 0; i < normalAxises.Length; i++)
                    {
                        axisesIndex[i] = 0;
                        rangsForIncreament[i] = new SliceIndex()
                        {
                            Start = 0,
                            Stop = orignalDims[normalAxises[i]],
                            OriginalLength = orignalDims[normalAxises[i]]   //must set this before use of it Nxxxx properties
                        };
                        axisesLength *= rangsForIncreament[i].NStop;
                    }

                    //new dimesions
                    int[] newDims = orignalDims.Where((source, index) => !normalAxises.Contains(index)).ToArray();
                    //delete 1 length dimensions
                    //newDims = newDims?.Where(e => e > 1).ToArray();
                    //if ((newDims == null) || (newDims.Length <= 0))
                    //{//all axises was selected
                    //	newDims = new int[] { 1 };
                    //}
                    var newArray = Array.CreateInstance(typeof(double), newDims);
                    int[] Indices = new int[newDims.Length];
                    Array.Clear(Indices, 0, Indices.Length);
                    for (var newArrayIndex = 0; newArrayIndex < newArray.Length; newArrayIndex++)
                    {
                        //Indices = newArray.GetIndices(newArrayIndex, true, newDims, Indices);
                        //origanl array indices
                        int[] orignalIndices = new int[orignalDims.Length];
                        int nNewIndex = 0;
                        for (int j = 0; j < orignalDims.Length; j++)
                        {
                            if (normalAxises.Contains(j))
                            {
                                continue;
                            }
                            orignalIndices[j] = Indices[nNewIndex];
                            nNewIndex++;
                        }
                        double[] dtemps = new double[axisesLength];
                        for (int k = 0; k < axisesLength; k++)
                        {//for axis
                            for (int l = 0; l < normalAxises.Length; l++)
                            {
                                orignalIndices[normalAxises[l]] = axisesIndex[l];
                            }
                            dtemps[k] = Convert.ToDouble(array.GetValue(orignalIndices));
                            IncrementIndex(axisesIndex, rangsForIncreament);
                        }
                        if (bIgnorNan)
                        {//remove all NaN from the double array
                            dtemps = dtemps.Where(e => !double.IsNaN(e)).ToArray();
                        }
                        //calculation
                        double dtemp = func(dtemps);
                        //set to array
                        newArray.SetValue(dtemp, Indices);
						Indices = Indices.Increament(newDims);
					}

                    if (newArray.Length == 1)
                    {
                        oRcd = newArray.GetAt(0);
                    }
                    else
                    {
                        oRcd = newArray.Squeeze();
                    }
                }
            }
            return oRcd!;
        }

		/// <summary>
		/// Do calculation by Element
		/// </summary>
		/// <example>
		/// <code>
		/// int[1,1,9] array1 = {{{0, 1, 2, 3, 4, 5, 6, 7, 8}}};
		/// int[9,1,1] array2 = {{{0}}, {{1}}, {{2}}, {{3}}, {{4}}, {{5}}, {{6}}, {{7}}, {{8}}}
		/// Func&lt;double,double,double&gt; func = (data1, data2) => data1 + data2;
		/// Array result = ByElement(array1, array2, func)		
		///     //result:  3D array - (9, 1, 9) =
		///		//	[[[ 0  1  2  3  4  5  6  7  8]]
		///		//	[[ 1  2  3  4  5  6  7  8  9]]
		///		//	[[ 2  3  4  5  6  7  8  9 10]]
		///		//	[[ 3  4  5  6  7  8  9 10 11]]
		///		//	[[ 4  5  6  7  8  9 10 11 12]]
		///		//	[[ 5  6  7  8  9 10 11 12 13]]
		///		//	[[ 6  7  8  9 10 11 12 13 14]]
		///		//	[[ 7  8  9 10 11 12 13 14 15]]
		///		//	[[ 8  9 10 11 12 13 14 15 16]]]
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="data">A second array or a scalar</param>
		/// <param name="func">A fuction to do calculation between two arrays</param>
		/// <param name="maskArray">An array with bool element, its dimensions must be same as array. if one element is true, the element in array does not participate in calculations. This is for masked array calculation to support what <c>Numpy.ma</c> is doing</param>
		/// <param name="maskData">An array with bool element, its dimensions must be same as data if data is an array. if one element is true, the element in data does not participate in calculations. This is for masked array calculation to support what <c>Numpy.ma</c> is doing</param>
		/// <returns>Returns output array with the calculations done by element</returns>
		public static Array ByElement(Array array, object data, Func<double, double, double> func, Array? maskArray = null, Array? maskData = null)
		{
			Array? oRcd = null;
			//an fuction to get a element mask
			bool GetMask(Array mask, int[] Index)
			{
				bool bRcd = false;
				if (mask != null)
				{
					try
					{
						bRcd = (bool)mask.GetValue(Index)!;
					}
					catch (Exception ex)
					{//do nothing, just return a false
						Debug.WriteLine(ex.Message);
                    }						
				}
				return bRcd;
			}
			if (array != null && array.Length > 0)
			{
				int[] dims = array.GetDimensions();
				if (maskArray == null ||
					maskArray != null && maskArray.GetDimensions().SequenceEqual(dims))
				{
					int[] index = new int[dims.Length];
					Array.Clear(index, 0, index.Length);
					if (data != null && data is Array src)
					{
						int[] srcDims = src.GetDimensions();
						if (maskData == null ||
							maskData != null && maskData.GetDimensions().SequenceEqual(srcDims))  //maskData's dimensions must be same as data's dimensions
						{
							//create target dimesion
							int[]? resultDims = null;
							if (srcDims.Length >= dims.Length)
							{
								//use src's dimension
								resultDims = new int[srcDims.Length];
								Buffer.BlockCopy(srcDims, 0, resultDims, 0, Buffer.ByteLength(srcDims));
								int indexDims = dims.Length - 1;
								for (int i = resultDims.Length - 1; i >= 0; i--)
								{
									if (indexDims >= 0 && dims[indexDims] > resultDims[i])
									{
										resultDims[i] = dims[indexDims];
									}
									indexDims--;
								}
							}
							else
							{
								//use array's dimension
								resultDims = new int[dims.Length];
								Buffer.BlockCopy(dims, 0, resultDims, 0, Buffer.ByteLength(dims));
								int indexSrcDims = srcDims.Length - 1;
								for (int i = resultDims.Length - 1; i >= 0; i--)
								{
									if (indexSrcDims >= 0 && srcDims[indexSrcDims] > resultDims[i])
									{
										resultDims[i] = srcDims[indexSrcDims];
									}
									indexSrcDims--;
								}
							}
							//create result's rang
							int[] resultIndex = new int[resultDims.Length];
							Array.Clear(resultIndex, 0, resultIndex.Length);
							//var resultRrangs = new SliceIndex[resultIndex.Length];
							//for (var i = 0; i < resultIndex.Length; i++)
							//{
							//	resultRrangs[i] = new SliceIndex()
							//	{
							//		Start = 0,
							//		Stop = resultDims[i],
							//		OriginalLength = resultDims[i]   //must set this before use of it Nxxxx properties
							//	};
							//}
							//create result array
							oRcd = Array.CreateInstance(typeof(double), resultDims);
							//do calculation
							for (var i = 0; i < oRcd.Length; i++)
							{
								var thisIndex = IndicesMapping(resultIndex, dims);
								var dataIndex = IndicesMapping(resultIndex, srcDims);
								double result = double.NaN; //if masked set result as NaN
								if (!GetMask(maskArray!, thisIndex) && !GetMask(maskData!, dataIndex))
								{
									double temp1 = Convert.ToDouble(array.GetValue(thisIndex));
									double temp2 = Convert.ToDouble(src.GetValue(dataIndex));
									result = func(temp1, temp2);
								}
								oRcd.SetValue(result, resultIndex);
								//IncreamentIndex(resultIndex, resultRrangs);
								resultIndex = resultIndex.Increament(resultDims);
							}
						}
					}
					else
					{//data is a constant
						oRcd = Array.CreateInstance(typeof(double), dims);
						double dData = Convert.ToDouble(data);
						for (var i = 0; i < array.Length; i++)
						{
							double result = double.NaN; //if masked set result as NaN
							if (!GetMask(maskArray!, index))
							{
								double temp1 = Convert.ToDouble(array.GetValue(index));
								result = func(temp1, dData);
							}
							oRcd.SetValue(result, index);
							//Increament indices;
							index = index.Increament(dims);
						}
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Enumerate by axis, and do comparison with the given fuction.
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="nAxis">Enumerate by nAxis. If null: enumrate entire element</param>
		/// <param name="func">Comparison function</param>
		/// <param name="bValue">true: return value(s), false: return index/indices</param>
		/// <returns>Value(Values) or Index(Indices)</returns>
		public static object CheckedByAxis(this Array array, int? nAxis, Func<object, object, bool> func, bool bValue = true)
		{
			object? oRcd = null;
			if (array != null && array.Length > 0)
			{
				int[] dims = array.GetDimensions();
				if (nAxis == null || dims.Length <= 1)
				{//find value/index from all element with given function
					object temp1 = array.GetAt(0);
					oRcd = bValue ? temp1 : 0;
					for (var i = 1; i < array.Length; i++)
					{
						object temp2 = array.GetAt(i);
						if (func(temp1, temp2))
						{
							temp1 = temp2;
							//return the value / index
							oRcd = bValue ? temp1 : i;
						}
					}
				}
				else
				{//find the values/indices along with nAxis with given function
					int nTemp = (int)nAxis;
					if (nTemp < 0)
					{
						nTemp = 0;
					}
					else if (nTemp >= dims.Length)
					{
						nTemp = dims.Length - 1;
					}
					int[] thisIndex = new int[dims.Length];
					int[] newDims = new int[dims.Length - 1];
					int[] newIndex = new int[newDims.Length];
					var newRangs = new SliceIndex[newDims.Length];
					int nIndex = 0;
					for (var i = 0; i < dims.Length; i++)
					{
						if (i == nTemp)
						{
							continue;
						}
						newDims[nIndex] = dims[i];
						newIndex[nIndex] = 0;
						newRangs[nIndex] = new SliceIndex()
						{
							Start = 0,
							Stop = newDims[nIndex],
							OriginalLength = newDims[nIndex]   //must set this before use of it Nxxxx properties
						};
						nIndex++;
					}
					Array oTemp = Array.CreateInstance(bValue ? array.GetType().GetElementType()! : typeof(int), newDims);
					Array.Clear(oTemp, 0, oTemp.Length);
					oRcd = oTemp;
					for (var i = 0; i < oTemp.Length; i++)
					{
						//copy current index
						nIndex = 0;
						for (var j = 0; j < thisIndex.Length; j++)
						{
							if (j == nTemp)
							{
								thisIndex[j] = 0;
								continue;
							}
							thisIndex[j] = newIndex[nIndex];
							nIndex++;
						}
						//first data
						object temp1 = array.GetValue(thisIndex)!;
						object oData = bValue ? temp1 : 0;
						//find max
						for (var j = 1; j < dims[nTemp]; j++)
						{
							thisIndex[nTemp] = j;
							object temp2 = array.GetValue(thisIndex)!;
							if (func(temp1, temp2))
							{
								temp1 = temp2;
								//return the value / index
								oData = bValue ? temp1 : j;
							}
						}
						oTemp.SetValue(oData, newIndex);
						IncreamentIndex(newIndex, newRangs);
					}
				}
			}
			return oRcd!;
		}


		#endregion used by those extensions

		#region More auxiliary functions

		/// <summary>
		/// Add value
		/// </summary>
		/// <param name="array"></param>
		/// <param name="indices"></param>
		/// <param name="newValue"></param>
		/// <param name="sum"></param>
		/// <returns></returns>
		public static Array AddValue(this Array array, List<int[]> indices, double newValue, bool sum = false)
		{
			double[,] doubleArray = (double[,])array.Clone();
			double[,] result = (double[,])array.Clone();

			if (array != null && indices != null)
			{
				int[] thisDims = array.GetDimensions();
				int idxDim = indices.Count;

				if (thisDims.Length == 2 && idxDim > 0)
				{
					var listIndices = new List<int[]>();

					for (int k = 0; k < thisDims.Length; k++)
					{
						if (indices[k] != null)
							listIndices.Add(indices[k]);
						else
						{
							var temp = new int[array.GetLength(k)];
							for (int i = 0; i < temp.Length; i++) temp[i] = i;
							listIndices.Add(temp);
						}
					}

					for (int j = 0; j < listIndices[0].Length && listIndices[0].Length <= array.GetLength(0); j++)
					{
						for (int i = 0; i < listIndices[1].Length && listIndices[1].Length <= array.GetLength(1); i++)
						{
							if (sum)
								result[listIndices[0][j], listIndices[1][i]] = doubleArray[listIndices[0][j], listIndices[1][i]] + newValue;
							else
								result[listIndices[0][j], listIndices[0][i]] = newValue;
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Fill an entire 1-D or 2-D array with the specified Value.
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="value">The value that will be added to each indice in this.array</param>
		/// <returns>Returns an array where each element == Value</returns>
		public static Array FillValues<T>(this Array array, T value) where T : struct
        {
			Array result = (Array)array.Clone();

			if (array.GetDimensions().Length == 1)
			{
                var temp = (T[])result;
                for (int i = 0; i < array.GetLength(0); i++)
				{
					temp[i] = value;
				}
			}
			else if (array.GetDimensions().Length == 2)
			{
				var temp = (T[,])result;
                for (int j = 0; j < array.GetLength(0); j++)
				{
					for (int i = 0; i < array.GetLength(1); i++)
					{
                        temp[j, i] = value;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Fill values in an array whose elements equal a specified ValueToCompare. If the element != ValueToCompare, then fill it in with a 0.
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="Value">The value that will be added to each indice in this.array</param>
		/// <param name="ValueToCompare">Specified value to compare with the current elements in this.array</param>
		/// <returns>Returns an array with either Value filling in elements who == ValueToCompare. If the element did not == Value to compare, 0 was filled in</returns>
		public static Array FillValues(this Array array, double Value, double ValueToCompare)
		{
			double[,] result = (double[,])array.Clone();

			if (array.GetDimensions().Length == 2)
			{
				for (int j = 0; j < array.GetLength(0); j++)
				{
					for (int i = 0; i < array.GetLength(1); i++)
					{
						if (result[j, i] == ValueToCompare)
							result[j, i] = Value;
						else
							result[j, i] = 0;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Repeat elements of an array.
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="repetitions">Amount of times to repeat the element of an array</param>
		/// <param name="axis">The axis along which to repeat values</param>
		/// <returns>Returns an output array which has the same shape as this.array, except along the given axis</returns>
		public static Array RepeatValues(this Array array, int repetitions, int axis)
		{
			int dim0 = array.GetLength(0), dim1 = array.GetLength(1), total = dim0 * dim1 * repetitions, J = axis + 1, I = total / J, stepsToJ = 0;
			double[,] doubleArray = (double[,])array.Clone();
			var result = new double[J, I];

			for (int j = 0; j < dim0; j++)
			{
				for (int i = 0; i < dim1; i++)
				{
					for (int r = 0; r < repetitions; r++)
					{
						result[stepsToJ, J * repetitions * j + i + r] = doubleArray[j, i];
					}
				}
				if (stepsToJ < J - 1) stepsToJ++;
			}

			return result;
		}

		/// <summary>
		/// Comparing the elements of an array to a specified condition.
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="condition">A double that is used to compare against the elements in this.array</param>
		/// <returns>Returns a bool array signifying if the elements in this.array are greather than the speficied condition</returns>
		public static Array IsTrueGreaterThan(this Array array, double condition)
		{
			var result = new bool[array.GetLength(0), array.GetLength(1)];
			var _array = (double[,])array.Clone();

			for (int j = 0; j < result.GetLength(0); j++)
			{
				for (int i = 0; i < result.GetLength(1); i++)
				{
					result[j, i] = _array[j, i] > condition;
				}
			}
			return result;
		}

		/// <summary>
		/// Comparing the elements of an array to a specified condition.
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="condition">A double that is used to compare against the elements in this.array</param>
		/// <returns>Returns a bool array signifying if the elements in this.array are less than the speficied condition</returns>
		public static Array IsTrueLesserThan(this Array array, double condition)
		{
			var result = new bool[array.GetLength(0), array.GetLength(1)];
			var _array = (double[,])array.Clone();

			for (int j = 0; j < result.GetLength(0); j++)
			{
				for (int i = 0; i < result.GetLength(1); i++)
				{
					result[j, i] = _array[j, i] < condition;
				}
			}
			return result;
		}

		/// <summary>
		/// Comparing the elements of an array to a specified condition. If the parameter "replace" is true and an element is
		/// greater than the given condition, then replace the element with 1, else 0. 
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="condition">A double that is used to compare against the elements in this.array</param>
		/// <param name="replace">The number 1 or 0 to replace the element in this.array</param>
		/// <returns>Returns a Double array signifying if the elements in this.array are greater than the speficied condition</returns>
		public static Array IsGreaterThan(this Array array, double condition, bool replace = true)
		{
			Func<double, double, double> func = (data1, condition) => data1 > condition ? replace ? data1 : 1 : 0;
			return ByElement(array, condition, func);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="array"></param>
		/// <param name="condition"></param>
		/// <param name="replace"></param>
		/// <returns></returns>
		public static Array IsGreaterThan(this Array array, double[] condition, bool replace = true)
		{
			var result = Array.CreateInstance(typeof(double), array.GetDimensions());
			if (array.GetLength(0) == condition.Length)
			{
				for (int k = 0; k < array.GetLength(0); k++)
				{
					var temp = (Array)array.PartOf(new SliceIndex?[] { new SliceIndex(k), null, null }!);
					result.SetValue(temp.IsGreaterThan(condition[k], false), new int?[] { k, null, null });
				}
			}
			return result;
		}

		/// <summary>
		/// Comparing the elements of an array to a specified condition. If the parameter "replace" is true and an element is
		/// less than the given condition, then replace the element with 1, else 0. 
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="condition">A double that is used to compare against the elements in this.array</param>
		/// <param name="replace">The number 1 or 0 to replace the element in this.array</param>
		/// <returns>Returns a Double array signifying if the elements in this.array are less than the speficied condition</returns>
		public static Array IsLesserThan(this Array array, double condition, bool replace = true)
		{
			Func<double, double, double> func = (data1, condition) => data1 < condition ? replace ? data1 : 1 : 0;
			return ByElement(array, condition, func);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="condition"></param>
		/// <returns></returns>
		public static Array Boolean_IsGreaterThan(this Array array, double condition)
		{
			Func<double, double, bool> func = (data1, condition) => data1 > condition;
			return ByElement(array, condition, func);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="condition"></param>
		/// <returns></returns>
		public static Array Boolean_IsGreaterThan(this Array array, double[] condition)
		{
			var result = Array.CreateInstance(typeof(bool), array.GetDimensions());

			if (array.GetLength(0) == condition.Length)
			{
				//for (int k = 0; k < array.GetLength(0); k++)
				Parallel.For(0, array.GetLength(0), k =>
				{
					var temp = (Array)array.PartOf(new SliceIndex?[] { new SliceIndex(k), null, null }!);
					result.SetValue(temp.Boolean_IsGreaterThan(condition[k]), new int?[] { k, null, null });
				});
			}
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="condition"></param>
		/// <returns></returns>
		public static Array Boolean_IsLesserThan(this Array array, double condition)
		{
			Func<double, double, bool> func = (data1, condition) => data1 < condition;
			return ByElement(array, condition, func);
		}

		/// <summary>
		/// Compare each indice in two bool arrays if either array's index is true, then add true to the 
		/// corresponding index in the returning array, else false.
		/// </summary>
		/// <param name="array1">First input array</param>
		/// <param name="array2">Second input array</param>
		/// <returns>Returns a bool array whose value is true if either the value in array1 or array2 are true, else false</returns>
		public static Array? ComparingBoolArrays(Array array1, Array array2)
		{
			if (array1.GetDimensions().Length == array2.GetDimensions().Length)
			{
				var result = (bool[,])Array.CreateInstance(typeof(bool), array1.GetLength(0), array1.GetLength(1));
				bool[,] boolArray1 = (bool[,])array1;
				bool[,] boolArray2 = (bool[,])array2;

				for (int j = 0; j < array1.GetLength(0); j++)
					for (int i = 0; i < array1.GetLength(1); i++)
					{
						result[j, i] = boolArray1[j, i] || boolArray2[j, i];
					}
				return result;
			}
			return null;
		}

		/// <summary>
		/// Determine if a value exists in each indice in this.array.
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="valueToFind">A function that returns a bool if a value matches the element in the indices</param>
		/// <returns>Returns a bool array that determines if a specified value exists in each indice</returns>
		public static Array? Any(this Array array, Func<double, bool> valueToFind)
		{
			switch (array.GetDimensions().Length)
			{
				case 1:
					{
						var result = (bool[])Array.CreateInstance(typeof(bool), array.GetLength(0));
						var doubleArray = (double[])array;
						for (int i = 0; i < doubleArray.Length; i++)
							if (valueToFind(doubleArray[i]))
								result[i] = true;
							else
								result[i] = false;

						return result;
					}
				case 2:
					{
						var result = (bool[,])Array.CreateInstance(typeof(bool), array.GetLength(0), array.GetLength(1));
						var doubleArray = (double[,])array;
						for (int j = 0; j < array.GetLength(0); j++)
							for (int i = 0; i < array.GetLength(1); i++)
								if (valueToFind(doubleArray[j, i]))
									result[j, i] = true;
								else
									result[j, i] = false;

						return result;
					}
			}
			return null;
		}

		/// <summary>
		/// Sum of array elements over a given axis.
		/// </summary>
		/// <param name="arr">this.array, input array</param>
		/// <param name="axes">Axes along which a sum is performed</param>
		/// <returns>Returns an array with elements along the specified axis summed</returns>
		public static Array SumOverAxes(this Array arr, int[] axes)
		{
			var len = axes.Length - 1;
			//var toReturn = arr.NanSum(new int[] { axes[len] }); // arr.Sum(axes[--len], type);
			var toReturn = arr;
			while (len >= 0)
			{
				toReturn = (Array)toReturn.NanSum(new int[] { axes[len] }); // toReturn.Sum(axes[--len], type);
				len--;
			}
			return toReturn;
		}

		///// <summary>
		///// Convert the elements in an array to the type Double.
		///// It may be better to use Copy&lt;double&gt;().
		///// </summary>
		///// <param name="array">this.array, input array</param>
		///// <returns>Returns an array with elements of type Double</returns>
		//public static Array ConvertToDouble(this Array array)
		//{
		//	Array? result = null;
		//	var dims = array.GetDimensions();

		//	if (dims.Length > 0)
		//	{
		//		switch (dims.Length)
		//		{
		//			case 4:
		//				{
		//					result = (double[,,,])Array.CreateInstance(typeof(double), dims);

		//					for (int a = 0; a < dims[0]; a++)
		//					{
		//						for (int b = 0; b < dims[1]; b++)
		//						{
		//							for (int c = 0; c < dims[2]; c++)
		//							{
		//								for (int d = 0; d < dims[3]; d++)
		//								{
		//									((double[,,,])result)[a, b, c, d] = ((bool?[,,,])array)[a, b, c, d] == true ? 1 : 0;
		//								}
		//							}
		//						}
		//					}
		//					break;
		//				}
		//			case 3:
		//				{
		//					result = (double[,,])Array.CreateInstance(typeof(double), dims);

		//					for (int a = 0; a < dims[0]; a++)
		//					{
		//						for (int b = 0; b < dims[1]; b++)
		//						{
		//							for (int c = 0; c < dims[2]; c++)
		//							{
		//								((double[,,])result)[a, b, c] = ((bool?[,,])array)[a, b, c] == true ? 1 : 0;
		//							}
		//						}
		//					}
		//					break;
		//				}
		//			case 2:
		//				{
		//					result = (double[,])Array.CreateInstance(typeof(double), dims);

		//					for (int a = 0; a < dims[0]; a++)
		//					{
		//						for (int b = 0; b < dims[1]; b++)
		//						{
		//							if (array.GetType() == typeof(float[,]))
		//								((double[,])result)[a, b] = ((float[,])array)[a, b];
		//							else if (array.GetType() == typeof(bool?[,]))
		//								((double[,])result)[a, b] = ((bool?[,])array)[a, b] == true ? 1 : 0;
		//                                  else if (array.GetType() == typeof(bool[,]))
		//                                      ((double[,])result)[a, b] = ((bool[,])array)[a, b] == true ? 1 : 0;
		//                              }
		//					}
		//					break;
		//				}
		//			case 1:
		//				{
		//					Array.ConvertAll<bool?, double>((bool?[])array, x => x == true ? 1 : 0);
		//					break;
		//				}
		//		}
		//	}

		//	return result;
		//}

		/// <summary>
		/// Counts how many elements for given value in this array.
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="Value">given value, can be <c>int, double,...</c></param>
		/// <returns>Returns an integer that represents how many elements for given value in the array.</returns>
		public static int CountByValue<T>(this Array array, T Value) where T : struct
		{
			int nRcd = 0;
			if (array != null)
			{
				T[] oTemp = (T[])array.Copy<T>(true);
				nRcd = oTemp.Count(p => p.Equals(Value));
			}
			return nRcd;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public static int CountNonZero(this Array array)
		{
			int nRcd = 0;
			if (array != null)
			{
				var oTemp = (double[])array.Copy<double>(true);
				nRcd = oTemp.Count(p => p != 0);
			}
			return nRcd;
		}

		/// <summary>
		/// Insert a new dimension into this.array that will expand the array shape.
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="toRight">A bool that determines whether to add the new dimension to the right. True to insert to the right, false to insert left</param>
		/// <returns>Returns an array with a dimension inserted into it</returns>
		public static Array AddDimension(this Array array, bool toRight = false)
		{
			Array? result = null;
			switch (array.GetDimensions().Length)
			{
				case 2:
					{
						var temp = new double[0, 0, 0];

						if (toRight)
							temp = new double[array.GetLength(0), array.GetLength(1), 1];
						else
							temp = new double[1, array.GetLength(0), array.GetLength(1)];

						var doubleArray = (double[,])array;
						for (int j = 0; j < array.GetLength(0); j++)
							for (int i = 0; i < array.GetLength(1); i++)
							{
								if (toRight)
									temp[j, i, 0] = doubleArray[j, i];
								else
									temp[0, j, i] = doubleArray[j, i];
							}
						result = temp;
						break;
					}
				case 3:
					{
						var temp = new double[0, 0, 0, 0];

						if (toRight)
							temp = new double[array.GetLength(0), array.GetLength(1), array.GetLength(2), 1];
						else
							temp = new double[1, array.GetLength(0), array.GetLength(1), array.GetLength(2)];

						var doubleArray = (double[,,])array;
						for (int j = 0; j < array.GetLength(0); j++)
							for (int i = 0; i < array.GetLength(1); i++)
								for (int k = 0; k < array.GetLength(2); k++)
								{
									if (toRight)
										temp[j, i, k, 0] = doubleArray[j, i, k];
									else
										temp[0, j, i, k] = doubleArray[j, i, k];
								}
						result = temp;
						break;
					}
			}
			return result!;
		}

		/// <summary>
		/// Insert a new dimension into this.array that will expand the array shape.
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="toRight">A bool that determines whether to add the new dimension to the right. True to insert to the right, false to insert left</param>
		/// <returns>Returns an array with a dimension inserted into it</returns>
		public static Array AddDimension<T>(this Array array, bool toRight = false)
		{
			Array? result = null;
			switch (array.GetDimensions().Length)
			{
				case 1:
					{
						var vector = (T[])array;
						var temp = new T[0, 0];

						if (toRight)
							temp = new T[array.Length, 1];
						else
							temp = new T[1, array.Length];

						for (int i = 0; i < array.Length; i++)
						{
							if (toRight)
								temp[i, 0] = vector[i];
							else
								temp[0, i] = vector[i];
						}
						result = temp;
						break;
					}
				case 2:
					{
						var temp = new T[0, 0, 0];

						if (toRight)
							temp = new T[array.GetLength(0), array.GetLength(1), 1];
						else
							temp = new T[1, array.GetLength(0), array.GetLength(1)];

						var tArray = (T[,])array;
						for (int j = 0; j < array.GetLength(0); j++)
							for (int i = 0; i < array.GetLength(1); i++)
							{
								if (toRight)
									temp[j, i, 0] = tArray[j, i];
								else
									temp[0, j, i] = tArray[j, i];
							}
						result = temp;
						break;
					}
				case 3:
					{
						var temp = new T[0, 0, 0, 0];

						if (toRight)
							temp = new T[array.GetLength(0), array.GetLength(1), array.GetLength(2), 1];
						else
							temp = new T[1, array.GetLength(0), array.GetLength(1), array.GetLength(2)];

						var tArray = (T[,,])array;
						for (int j = 0; j < array.GetLength(0); j++)
							for (int i = 0; i < array.GetLength(1); i++)
								for (int k = 0; k < array.GetLength(2); k++)
								{
									if (toRight)
										temp[j, i, k, 0] = tArray[j, i, k];
									else
										temp[0, j, i, k] = tArray[j, i, k];
								}
						result = temp;
						break;
					}
			}
			return result!;
		}

		/// <summary>
		/// Sanitize array by resetting the elements of the array to 0.
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <returns>Returns an array with element value replaces by 0's</returns>
		public static Array SanitizeArray(this Array array)
		{
			Func<double, double, double> func = (data1, condition) => data1 != double.NaN && data1 > double.MinValue && data1 < double.MaxValue ? data1 : 0;
			return ByElement(array, null, func);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public static Array SanitizeArrayPositiveInfinity(this Array array)
		{
			Func<double, double, double> func = (data1, condition) => data1 > int.MaxValue ? int.MaxValue : data1;
			return ByElement(array, null, func);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public static Array SanitizeArrayNegativeInfinity(this Array array)
		{
			Func<double, double, double> func = (data1, condition) => data1 < int.MinValue ? int.MinValue : data1;
			return ByElement(array, null, func);
		}

		/// <summary>
		/// Uses Math.Round for each element in the array.
		/// </summary>
		/// <param name="array">this.array, input array</param>
		/// <param name="numDecimal"></param>
		/// <returns>Returns a masked array</returns>
		public static Array MaskedArray(this Array array, int numDecimal)
		{
			Func<double, double, double> func = (data1, condition) => Math.Round(data1, numDecimal);
			return ByElement(array, null, func);
		}

		/// <summary>
		/// Uses Math.Round for the speficied number. 
		/// </summary>
		/// <param name="number">The number to be masked</param>
		/// <param name="numDecimals"></param>
		/// <returns>Returns a masked number</returns>
		public static double MaskedNumber(double number, int numDecimals)
		{
			return Math.Round(number, numDecimals);
		}

		/// <summary>
		/// Turn a rectangular pixel array into a linear array, accounting for the serpentine pixel indexing arrangement.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="rows"></param>
		/// <param name="cols"></param>
		/// <param name="numPixels"></param>
		/// <returns></returns>
		public static Array Snake(this Array array, int rows, int cols, int numPixels)
		{
			Array result = array;
			var dims = array.GetDimensions();
			var num_dims = dims.Length;

			if (dims[num_dims - 2] == rows && dims[num_dims - 1] == cols && num_dims == 4)
			{
				var temp = (Array)array.PartOf(new object?[] { null, null, new SliceIndex(1, null, 2), new SliceIndex(null, null, -1) }!);

				for (int k = 0; k < temp.GetLength(2); k++)
					result.SetValue2(temp, new int?[] { null, null, k * 2 + 1, null });

				result = result.Reshape(new int[] { array.GetLength(0), array.GetLength(1), numPixels });
			}

			return result;
		}

		/// <summary>
		/// Reverse the data of last dimentional data by index 1,3,5...
		/// </summary>
		/// <param name="array">this array, its rank (shape, dimention) must great than 1</param>
		/// <param name="bTo1D">true:return an 1D array</param>
		/// <returns>a array after sanke. null: when the orignal array is null or its rank is less than 2</returns>
		public static Array Snake(this Array array, bool bTo1D = false)
		{
			Array? oRcd = null;
			if (array != null && array.Rank >= 2)
			{
				//this array's dimesion
				int[] dims = array.GetDimensions();
				//create output array
				Type oType = array.GetType().GetElementType()!;
				oRcd = Array.CreateInstance(oType, dims);
				//init a slice
				int?[] sliceIndices = new int?[dims.Length];
				sliceIndices[^1] = dims[^1];
				//init a indices
				int[] aIndices = new int[dims.Length];
				Array.Clear(aIndices);
				for (int i = 0; i < array.Length; i += dims[^1])
				{
					if (aIndices[^2] % 2 == 0)
					{//for 0,2,4, just copy those to target array
						Array.Copy(array, i, oRcd, i, dims[^1]);
					}
					else
					{//for 1,3,5
						for (int k = 0; k < dims[^1]; k++)
						{
							aIndices[^1] = k;
							var oElement = array.GetValue(aIndices);
							aIndices[^1] = dims[^1] - k - 1;
							oRcd.SetValue(oElement, aIndices);
						}
					}
					aIndices.Increament(sliceIndices, dims);
				}
				if (bTo1D)
				{
					oRcd = oRcd.To1D();
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Turn a linear pixel array into a rectangular array, accounting for the serpentine pixel indexing arrangement.
		/// </summary>
		/// <param name="array">this.array, linear array with dimensions (..., N_PIXELS).</param>
		/// <param name="length"></param>
		/// <param name="rows"></param>
		/// <param name="cols"></param>
		/// <returns>Rectangular array with dimensions (..., N_ROWS, N_COLUMNS)</returns>
		public static Array Desnake(this Array array, int length, int rows, int cols)
		{
			Array? result = null;

			if (array.GetLength(1) == length)
			{
				var vector = array.Reshape(new int[] { rows, cols });
				var temp = (Array)vector.PartOf(new object[] { new SliceIndex(1, null, 2), new SliceIndex(null, null, -1) });
			}

			return result!;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="axis"></param>
		/// <param name="type"></param>
		/// <param name="bp"></param>
		/// <returns></returns>
		public static Array Detrend(this Array array, int axis = -1, string type = "linear", int bp = 0)
		{
			Array result = Array.CreateInstance(typeof(double), 0);

			if (type == "linear" || type == "constant")
			{
				var rnk = array.GetDimensions().Length;
				int n = 0;

				if (axis > -1 && axis < rnk)
					n = array.GetLength(axis);
				else
					n = rnk - 1;

				if (bp < n)
				{
					result = Array.CreateInstance(typeof(double), n--);

					if (axis < 0) axis = +rnk;

					var newDims = new int[] { axis, axis, axis + n };
					var newData = array.Transpose(new int[] { });
				}
			}

			return result;
		}

		/// <summary>
		/// Reshape an array to a 2-D array.
		/// </summary>
		/// <param name="array">this.array</param>
		/// <returns>Returns an array that has been reshaped to a 2D array</returns>
		public static Array? ReshapeTo2D(this Array array)
		{
			if (array.GetDimensions().Length == 1)
			{
				int n = array.GetLength(0);
				var result = new double[n][];
				var vector = (double[])array;

				for (int k = 0; k < vector.Length; k++)
				{
					result[k] = new double[] { vector[k] };
				}
				return result;
			}
			else
				return null;
		}

		/// <summary>
		/// Sum bins<br/>
		/// Equvalent to sum_bins(spectrum) (line 168 of spectral_class.py of dm-tp)
		/// </summary>
		/// <param name="array">this array</param>
		/// <param name="nAxis">axis, default is 1</param>
		/// <returns></returns>
		public static Array SumBins(this Array array, int nAxis = 1)
		{
			Array? result = null;

			if (array != null)
			{
				int[] thisDims = array.GetDimensions();
				if (thisDims.Length > 1 && thisDims.Length > nAxis)
				{
					int[] newDims = thisDims.Where((_, i) => i != nAxis).ToArray();
					result = Array.CreateInstance(typeof(double), newDims);
					SliceIndex[] oSlices = new SliceIndex[thisDims.Length];

					for (int i = 0; i < thisDims[nAxis]; i++)
					{
						oSlices[nAxis] = new SliceIndex(i, i);
						var temp1 = (Array)array.PartOf(oSlices);
						var temp2 = temp1.Roll(i, 0);
						result = result.Sum(temp2);
					}
				}
			}
			return result!;
		}

		/// <summary>
		/// Calculate a 1-D correlation along the given axis. Only "constant" mode can be used<br/>
		/// The lines of the array along the given axis are correlated with the given weights.<br/>
		/// Equvalent to <seealso href="https://docs.scipy.org/doc/scipy/reference/generated/scipy.ndimage.correlate1d.html">scipy.ndimage.correlate1d()</seealso>
		/// </summary>
		/// <param name="array"></param>
		/// <param name="weights"></param>
		/// <param name="nAxis">do culculation along with this axis </param>
		/// <param name="cval">for "constant" mode, use this value into most left/right side</param>
		/// <returns></returns>
		public static Array Correlate1D(this Array array, double[] weights, int nAxis = -1, double cval = 0)
		{
			Array? oRcd = null;
			if (array != null && weights != null)
			{
				//determine symetric of weitghts
				//int nSymetric = 0;
				//number of data to be used on its left side, if not data put 
				int nSize1 = weights.Length / 2;
				//int nSize2 = weights.Length - nSize1 -1;
				//if(weights.Length%2 != 0)
				//{//elements of weitght is odd number
				//	nSymetric = 1;
				//	for(int i = 1; i <= nSize1; i++)
				//	{
				//		if ((weights[i + nSize1] - weights[nSize1 - i] > double.Epsilon))
				//		{// the 2 vealues is not equal
				//			nSymetric = 0;
				//			break;
				//		}
				//	}
				//	if(nSymetric == 0)
				//	{
				//		nSymetric = -1;
				//		for (int i = 1; i <= nSize1; i++)
				//		{
				//			if ((weights[i + nSize1] + weights[nSize1 - i] > double.Epsilon))
				//			{// one vealue is plus, the other one is minus, but its abs is not equle
				//				nSymetric = 0;
				//				break;
				//			}
				//		}
				//	}
				//}

				//this array's dimesion
				int[] dims = array.GetDimensions();
				//normolize axis
				nAxis = nAxis.NormalizeAxis(dims.Length);
				//create output array
				oRcd = Array.CreateInstance(typeof(double), dims);
				int[] Indices = new int[dims.Length];
				Array.Clear(Indices, 0, Indices.Length);
				for (int i = 0; i < oRcd.Length; i++)
				{
					//sourec / target item indices
					//Indices = oRcd.GetIndices(i, true, dims, Indices);
					//indices for get data around this item
					int[] bTempIndices = new int[Indices.Length];
					Buffer.BlockCopy(Indices, 0, bTempIndices, 0, Buffer.ByteLength(Indices));
					double dTemp = 0;
					//double[] dtemps = new double[weights.Length];
					for (int j = 0; j < weights.Length; j++)
					{
						//for "constant" mode
						// kkkk/abcdefg/kkkk
						// abcdefg : orignal data item along with given axis
						// kkkk : data item before/after the original data item. number of these items should be (weights.Length/2) for left(before), (weights.Length -1 - weights.Length/2) for right(after) 
						bTempIndices[nAxis] = Indices[nAxis] - nSize1 + j;
						if (bTempIndices[nAxis] < 0 || bTempIndices[nAxis] >= dims[nAxis])
						{//put cval for those idices we don't have in original array
							dTemp += cval * weights[j];
						}
						else
						{//get the data from 
							dTemp += Convert.ToDouble(array.GetValue(bTempIndices)) * weights[j];
						}
					}
					oRcd.SetValue(dTemp, Indices);
					Indices = Indices.Increament(dims);
				}
			}
			return oRcd!;
		}

		#endregion More auxiliary functions

		/// <summary>
		/// Increament index array.
		/// </summary>
		/// <param name="indices">An array of indices containing integers</param>
		/// <param name="ranges">Slice index for each dimension. Must set up OriginalLength for each Slice index in order to use its Nxxxx properties</param>
		/// <returns>void</returns>
		public static void IncrementIndex(int[] indices, object[] ranges)
		{
			if (indices != null && indices.Length == ranges.Length)
			{
				for (int i = indices.Length - 1; i >= 0; i--)
				{
					if (ranges[i] is SliceIndex oSlice)
					{
						indices[i] += oSlice.NStep;
						if (oSlice.NStep > 0)
						{
							if (indices[i] >= oSlice.NStop)
							{
								indices[i] = oSlice.NStart;
							}
							else
							{
								break;
							}
						}
						else
						{
							if (indices[i] <= oSlice.NStop)
							{
								indices[i] = oSlice.NStart;
							}
							else
							{
								break;
							}
						}
					}
					else if (ranges[i] is int[] oIdex)
					{
						int nNext = -1;
						for (var j = 0; j < oIdex.Length; j++)
						{
							if (oIdex[j] == indices[i])
							{//find next
								nNext = j + 1;
								break;
							}
						}
						if (nNext > 0)
						{
							if (nNext < oIdex.Length)
							{
								indices[i] = oIdex[nNext];
								break;
							}
							else
							{
								indices[i] = oIdex[0];
							}
						}
						else
						{//should not be here
						}
					}
				}
			}
		}

		/// <summary>
		/// Increament Indice base one dims
		/// </summary>
		/// <param name="Indices">current Indices as a <c>int[]</c></param>
		/// <param name="dims">related array dimensions as a <c>int[]</c></param>
		/// <param name="bMemorySequence">false: the data sequence starts from dim[0], when we read the mat file, we must use this option. true : data sequence starts from dim[0]</param>
		/// <returns>Indice after increament. null if wrong argument</returns>
		public static int[] Increment(this int[] Indices, int[] dims, bool bMemorySequence = true)
		{
			int[]? oRcd = null;
			if (dims != null &&
				Indices != null &&
				dims.Length > 0 &&
				dims.Length == Indices.Length)
			{
				int nStart = bMemorySequence ? -(Indices.Length - 1) : 0;
				int nEnd = bMemorySequence ? 0 : Indices.Length - 1;
				int nMinus = bMemorySequence ? -1 : 1;

				for (int nIndex = nStart; nIndex <= nEnd; nIndex++)
				{
					int nTemp = nIndex * nMinus;
					Indices[nTemp]++;
					if (Indices[nTemp] < dims[nTemp])
					{
						break;
					}
					Indices[nTemp] = 0;
				}
				oRcd = Indices;
			}
			return oRcd!;
		}

		/// <summary>
		/// Do calculation by Element
		/// </summary>
		/// <example>
		/// <code>
		/// int[1,1,9] array1 = {{{0, 1, 2, 3, 4, 5, 6, 7, 8}}};
		/// int[9,1,1] array2 = {{{0}}, {{1}}, {{2}}, {{3}}, {{4}}, {{5}}, {{6}}, {{7}}, {{8}}}
		/// Func&lt;double,double,double&gt; func = (data1, data2) => data1 + data2;
		/// Array result = ByElement(array1, array2, func)		
		///     //result:  3D array - (9, 1, 9) =
		///		//	[[[ 0  1  2  3  4  5  6  7  8]]
		///		//	[[ 1  2  3  4  5  6  7  8  9]]
		///		//	[[ 2  3  4  5  6  7  8  9 10]]
		///		//	[[ 3  4  5  6  7  8  9 10 11]]
		///		//	[[ 4  5  6  7  8  9 10 11 12]]
		///		//	[[ 5  6  7  8  9 10 11 12 13]]
		///		//	[[ 6  7  8  9 10 11 12 13 14]]
		///		//	[[ 7  8  9 10 11 12 13 14 15]]
		///		//	[[ 8  9 10 11 12 13 14 15 16]]]
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="data">A second array or a scalar</param>
		/// <param name="func">A fuction to do calculation between two arrays</param>
		/// <returns>Returns output array with the calculations done by element</returns>
		public static Array ByElement(Array array, object? data, Func<double, double, double> func)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0)
			{
				int[] dims = array.GetDimensions();
				int[] index = new int[dims.Length];
				Array.Clear(index, 0, index.Length);
				if (data != null && data is Array src)
				{
					int[] srcDims = src.GetDimensions();
					//create target dimesion
					int[]? resultDims = null;
					if (srcDims.Length >= dims.Length)
					{
						//use src's dimension
						resultDims = new int[srcDims.Length];
						Buffer.BlockCopy(srcDims, 0, resultDims, 0, Buffer.ByteLength(srcDims));
						int indexDims = dims.Length - 1;
						for (int i = resultDims.Length - 1; i >= 0; i--)
						{
							if (indexDims >= 0 && dims[indexDims] > resultDims[i])
							{
								resultDims[i] = dims[indexDims];
							}
							indexDims--;
						}
					}
					else
					{
						//use array's dimension
						resultDims = new int[dims.Length];
						Buffer.BlockCopy(dims, 0, resultDims, 0, Buffer.ByteLength(dims));
						int indexSrcDims = srcDims.Length - 1;
						for (int i = resultDims.Length - 1; i >= 0; i--)
						{
							if (indexSrcDims >= 0 && srcDims[indexSrcDims] > resultDims[i])
							{
								resultDims[i] = srcDims[indexSrcDims];
							}
							indexSrcDims--;
						}
					}
					//create result's rang
					int[] resultIndex = new int[resultDims.Length];
					Array.Clear(resultIndex, 0, resultIndex.Length);
					var resultRrangs = new SliceIndex[resultIndex.Length];
					for (var i = 0; i < resultIndex.Length; i++)
					{
						resultRrangs[i] = new SliceIndex()
						{
							Start = 0,
							Stop = resultDims[i],
							OriginalLength = resultDims[i]   //must set this before use of it Nxxxx properties
						};
					}
					//create result array
					oRcd = Array.CreateInstance(typeof(double), resultDims);
					//do calculation
					for (var i = 0; i < oRcd.Length; i++)
					{
						double temp1 = Convert.ToDouble(array.GetValue(IndicesMapping(resultIndex, dims)));
						double temp2 = Convert.ToDouble(src.GetValue(IndicesMapping(resultIndex, srcDims)));
						double result = func(temp1, temp2);
						oRcd.SetValue(result, resultIndex);
						IncrementIndex(resultIndex, resultRrangs);
						//resultIndex = resultIndex.Increment(resultDims);
					}
				}
				else
				{//should be double
				 //create array's rang
					var rangs = new SliceIndex[index.Length];
					for (var i = 0; i < rangs.Length; i++)
					{
						rangs[i] = new SliceIndex()
						{
							Start = 0,
							Stop = dims[i],
							OriginalLength = dims[i]   //must set this before use of it Nxxxx properties
						};
					}
					oRcd = Array.CreateInstance(typeof(double), dims);
					var value = Convert.ToDouble(data);

					for (var i = 0; i < array.Length; i++)
					{
						double result = func((double)array.GetValue(index)!, value);
						oRcd.SetValue(result, index);
						IncrementIndex(index, rangs);
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Do calculation by Element. This method also handles Div, Sum.. in the same logic.
		/// </summary>
		/// <param name="data">An array or a scalar</param>
		/// <param name="array">An array</param>
		/// <param name="func">A fuction to do calculation between two arrays</param>
		/// <returns>Returns an output array with the calculations done by element</returns>
		public static Array ByElement(double data, Array array, Func<double, double, double> func)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0)
			{
				int[] dims = array.GetDimensions();
				oRcd = Array.CreateInstance(typeof(double), dims);
				int[] index = new int[dims.Length];
				Array.Clear(index, 0, index.Length);
				var rangs = new SliceIndex[index.Length];
				for (var i = 0; i < rangs.Length; i++)
				{
					rangs[i] = new SliceIndex()
					{
						Start = 0,
						Stop = dims[i],
						OriginalLength = dims[i]   //must set this before use of it Nxxxx properties
					};
				}
				if (data != 0)
				{
					for (var i = 0; i < array.Length; i++)
					{
						double result = func(data, (double)array.GetValue(index)!);
						oRcd.SetValue(result, index);
						IncrementIndex(index, rangs);
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Do calculation by Element
		/// </summary>
		/// <example>
		/// <code>
		/// int[1,1,9] array1 = {{{0, 1, 2, 3, 4, 5, 6, 7, 8}}};
		/// int[9,1,1] array2 = {{{0}}, {{1}}, {{2}}, {{3}}, {{4}}, {{5}}, {{6}}, {{7}}, {{8}}}
		/// Func&lt;double,double,double&gt; func = (data1, data2) => data1 + data2;
		/// Array result = ByElement(array1, array2, func)		
		///     //result:  3D array - (9, 1, 9) =
		///		//	[[[ 0  1  2  3  4  5  6  7  8]]
		///		//	[[ 1  2  3  4  5  6  7  8  9]]
		///		//	[[ 2  3  4  5  6  7  8  9 10]]
		///		//	[[ 3  4  5  6  7  8  9 10 11]]
		///		//	[[ 4  5  6  7  8  9 10 11 12]]
		///		//	[[ 5  6  7  8  9 10 11 12 13]]
		///		//	[[ 6  7  8  9 10 11 12 13 14]]
		///		//	[[ 7  8  9 10 11 12 13 14 15]]
		///		//	[[ 8  9 10 11 12 13 14 15 16]]]
		/// </code>
		/// </example>
		/// <param name="array">this.array</param>
		/// <param name="data">A second array or a scalar</param>
		/// <param name="func">A fuction to do calculation between two arrays</param>
		/// <returns>Returns output array with the calculations done by element</returns>
		public static Array ByElement(Array array, object data, Func<double, double, bool> func)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0)
			{
				int[] dims = array.GetDimensions();
				int[] index = new int[dims.Length];
				Array.Clear(index, 0, index.Length);
				if (data != null && data is Array src)
				{
					int[] srcDims = src.GetDimensions();
					//create target dimesion
					int[]? resultDims = null;
					if (srcDims.Length >= dims.Length)
					{
						//use src's dimension
						resultDims = new int[srcDims.Length];
						Buffer.BlockCopy(srcDims, 0, resultDims, 0, Buffer.ByteLength(srcDims));
						int indexDims = dims.Length - 1;
						for (int i = resultDims.Length - 1; i >= 0; i--)
						{
							if (indexDims >= 0 && dims[indexDims] > resultDims[i])
							{
								resultDims[i] = dims[indexDims];
							}
							indexDims--;
						}
					}
					else
					{
						//use array's dimension
						resultDims = new int[dims.Length];
						Buffer.BlockCopy(dims, 0, resultDims, 0, Buffer.ByteLength(dims));
						int indexSrcDims = srcDims.Length - 1;
						for (int i = resultDims.Length - 1; i >= 0; i--)
						{
							if (indexSrcDims >= 0 && srcDims[indexSrcDims] > resultDims[i])
							{
								resultDims[i] = srcDims[indexSrcDims];
							}
							indexSrcDims--;
						}
					}
					//create result's rang
					int[] resultIndex = new int[resultDims.Length];
					Array.Clear(resultIndex, 0, resultIndex.Length);
					var resultRrangs = new SliceIndex[resultIndex.Length];
					for (var i = 0; i < resultIndex.Length; i++)
					{
						resultRrangs[i] = new SliceIndex()
						{
							Start = 0,
							Stop = resultDims[i],
							OriginalLength = resultDims[i]   //must set this before use of it Nxxxx properties
						};
					}
					//create result array
					oRcd = Array.CreateInstance(typeof(bool), resultDims);
					//do calculation
					for (var i = 0; i < oRcd.Length; i++)
					{
						double temp1 = Convert.ToDouble(array.GetValue(IndicesMapping(resultIndex, dims)));
						double temp2 = Convert.ToDouble(src.GetValue(IndicesMapping(resultIndex, srcDims)));
						var result = func(temp1, temp2);
						oRcd.SetValue(result, resultIndex);
						IncrementIndex(resultIndex, resultRrangs);
					}
				}
				else
				{//should be double
				 //create array's rang
					var rangs = new SliceIndex[index.Length];
					for (var i = 0; i < rangs.Length; i++)
					{
						rangs[i] = new SliceIndex()
						{
							Start = 0,
							Stop = dims[i],
							OriginalLength = dims[i]   //must set this before use of it Nxxxx properties
						};
					}
					oRcd = Array.CreateInstance(typeof(bool), dims);
					var value = (double)data!;

					for (var i = 0; i < array.Length; i++)
					{
						var result = func((double)array.GetValue(index)!, value);
						oRcd.SetValue(result, index);
						IncrementIndex(index, rangs);
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="func"></param>
		/// <returns></returns>
		public static Array ByElement(Array array, Func<double, double> func)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0)
			{
				int[] dims = array.GetDimensions();
				int[] index = new int[dims.Length];
				Array.Clear(index, 0, index.Length);

				//should be double
				//create array's rang
				var rangs = new SliceIndex[index.Length];
				for (var i = 0; i < rangs.Length; i++)
				{
					rangs[i] = new SliceIndex()
					{
						Start = 0,
						Stop = dims[i],
						OriginalLength = dims[i]   //must set this before use of it Nxxxx properties
					};
				}
				oRcd = Array.CreateInstance(typeof(double), dims);
				for (var i = 0; i < array.Length; i++)
				{
					double result = func((double)array.GetValue(index)!);
					oRcd.SetValue(result, index);
					IncrementIndex(index, rangs);
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// do calculation by Element
		/// </summary>
		/// <param name="array">one array</param>
		/// <param name="data">an array or a scalar</param>
		/// <param name="func">a fuction to do calculation</param>
		/// <returns> result array </returns>
		public static Array ByElement(Array array, Array data, Func<double, double, bool> func)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0)
			{
				int[] dims = array.GetDimensions();
				int[] index = new int[dims.Length];
				Array.Clear(index, 0, index.Length);
				if (data != null && data is Array src)
				{
					int[] srcDims = src.GetDimensions();
					//create target dimesion
					int[]? resultDims = null;
					if (srcDims.Length >= dims.Length)
					{
						//use src's dimension
						resultDims = new int[srcDims.Length];
						Buffer.BlockCopy(srcDims, 0, resultDims, 0, Buffer.ByteLength(srcDims));
						int indexDims = dims.Length - 1;
						for (int i = resultDims.Length - 1; i >= 0; i--)
						{
							if (indexDims >= 0 && dims[indexDims] > resultDims[i])
							{
								resultDims[i] = dims[indexDims];
							}
							indexDims--;
						}
					}
					else
					{
						//use array's dimension
						resultDims = new int[dims.Length];
						Buffer.BlockCopy(dims, 0, resultDims, 0, Buffer.ByteLength(dims));
						int indexSrcDims = srcDims.Length - 1;
						for (int i = resultDims.Length - 1; i >= 0; i--)
						{
							if (indexSrcDims >= 0 && srcDims[indexSrcDims] > resultDims[i])
							{
								resultDims[i] = srcDims[indexSrcDims];
							}
							indexSrcDims--;
						}
					}
					//create result's rang
					int[] resultIndex = new int[resultDims.Length];
					Array.Clear(resultIndex, 0, resultIndex.Length);
					var resultRrangs = new SliceIndex[resultIndex.Length];
					for (var i = 0; i < resultIndex.Length; i++)
					{
						resultRrangs[i] = new SliceIndex()
						{
							Start = 0,
							Stop = resultDims[i],
							OriginalLength = resultDims[i]   //must set this before use of it Nxxxx properties
						};
					}
					//create result array
					oRcd = Array.CreateInstance(typeof(bool), resultDims);
					//do calculation
					for (var i = 0; i < oRcd.Length; i++)
					{
						double temp1 = Convert.ToDouble(array.GetValue(IndicesMapping(resultIndex, dims)));
						double temp2 = Convert.ToDouble(src.GetValue(IndicesMapping(resultIndex, srcDims)));
						bool result = func(temp1, temp2);
						oRcd.SetValue(result, resultIndex);
						IncrementIndex(resultIndex, resultRrangs);
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="data"></param>
		/// <param name="func"></param>
		/// <returns></returns>
		public static Array ByElement(Array array, Array data, Func<bool, bool, bool> func)
		{
			Array? oRcd = null;
			if (array != null && array.Length > 0)
			{
				int[] dims = array.GetDimensions();
				int[] index = new int[dims.Length];
				Array.Clear(index, 0, index.Length);
				if (data != null && data is Array src)
				{
					int[] srcDims = src.GetDimensions();
					//create target dimesion
					int[]? resultDims = null;
					if (srcDims.Length >= dims.Length)
					{
						//use src's dimension
						resultDims = new int[srcDims.Length];
						Buffer.BlockCopy(srcDims, 0, resultDims, 0, Buffer.ByteLength(srcDims));
						int indexDims = dims.Length - 1;
						for (int i = resultDims.Length - 1; i >= 0; i--)
						{
							if (indexDims >= 0 && dims[indexDims] > resultDims[i])
							{
								resultDims[i] = dims[indexDims];
							}
							indexDims--;
						}
					}
					else
					{
						//use array's dimension
						resultDims = new int[dims.Length];
						Buffer.BlockCopy(dims, 0, resultDims, 0, Buffer.ByteLength(dims));
						int indexSrcDims = srcDims.Length - 1;
						for (int i = resultDims.Length - 1; i >= 0; i--)
						{
							if (indexSrcDims >= 0 && srcDims[indexSrcDims] > resultDims[i])
							{
								resultDims[i] = srcDims[indexSrcDims];
							}
							indexSrcDims--;
						}
					}
					//create result's rang
					int[] resultIndex = new int[resultDims.Length];
					Array.Clear(resultIndex, 0, resultIndex.Length);
					var resultRrangs = new SliceIndex[resultIndex.Length];
					for (var i = 0; i < resultIndex.Length; i++)
					{
						resultRrangs[i] = new SliceIndex()
						{
							Start = 0,
							Stop = resultDims[i],
							OriginalLength = resultDims[i]   //must set this before use of it Nxxxx properties
						};
					}
					//create result array
					oRcd = Array.CreateInstance(typeof(bool), resultDims);
					//do calculation
					for (var i = 0; i < oRcd.Length; i++)
					{
						var temp1 = Convert.ToBoolean(array.GetValue(IndicesMapping(resultIndex, dims)));
						var temp2 = Convert.ToBoolean(src.GetValue(IndicesMapping(resultIndex, srcDims)));
						bool result = func(temp1, temp2);
						oRcd.SetValue(result, resultIndex);
						IncrementIndex(resultIndex, resultRrangs);
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Print out multi demension array
		/// </summary>
		/// <param name="array"></param>
		/// <returns>Array data in sting. Here has an output example. this an int[2,3,4] array
		/// <code>
		/// [[[0,1,2,3],
		///   [4,5,6,7],
		///   [8,9,10,11]],
		///  [[12,13,14,15],
		///   [16,17,18,19],
		///   [20,21,22,23]]]
		/// </code>
		/// </returns>
		public static string ToString2(this Array array)
		{
			string strRcd = string.Empty;
			if (array != null)
			{
				int[] dims = array.GetDimensions();
				StringBuilder strTemp = new StringBuilder();
				int[] Indices = new int[dims.Length];
				Array.Clear(Indices);
				for (int i = 0; i < array.Length; i += dims[^1])
				{
					//find non zero from Indices
					int nNonZeroIndex = Array.FindLastIndex(Indices, value => value != 0);
					//put "[" for each dimemsion to last third dimemsion
					for (int j = nNonZeroIndex; j < Indices.Length - 1; j++)
					{
						strTemp.Append('[');
					}
					//for all last dimension's values
					for (int j = 0; j < dims[^1]; j++)
					{
						Indices[^1] = j;
						strTemp.Append(array.GetValue(Indices)!.ToString());
						if (j < dims[^1] - 1)
						{
							strTemp.Append(',');
						}
					}
					//clear last dimension's index
					Indices[^1] = 0;
					//put "}" for last dimesion
					strTemp.Append(']');
					// increment Indices from last second to first
					for (int nIndex = Indices.Length - 2; nIndex >= 0; nIndex--)
					{
						Indices[nIndex]++;
						if (Indices[nIndex] < dims[nIndex])
						{
							break;
						}
						//put "}" for this dimesion
						strTemp.Append(']');
						Indices[nIndex] = 0;
					}
					//put ","
					if (i + dims[^1] < array.Length - 1)
					{
						strTemp.Append(',');
					}
				}
				strRcd = strTemp.ToString();
			}
			return strRcd;
		}

		/// <summary>
		/// Return evenly spaced numbers over a specified interval.
		/// Returns num evenly spaced samples, calculated over the interval[start, stop].
		/// The endpoint of the interval can optionally be excluded.
		/// Example in C#: Array arr = Linspace(-4, 4, 5)  // output: {-4, -2, 0, 2, 4}
		/// Equvalent to <seealso href="https://numpy.org/doc/stable/reference/generated/numpy.linspace.html">numpy.linspace()</seealso>
		/// </summary>
		/// <param name="start">The starting value of the sequence.</param>
		/// <param name="stop">The end value of the sequence, unless endpoint is set to False. 
		/// In that case, the sequence consists of all but the last of num + 1 evenly spaced samples, 
		/// so that stop is excluded. Note that the step size changes when endpoint is False.</param>
		/// <param name="num">Number of samples to generate. Default is 50. Must be non-negative.</param>
		/// <param name="endpoint">If True, stop is the last sample. Otherwise, it is not included. Default is True.</param>
		/// <returns></returns>
		public static Array Linspace(double start, double stop, int num = 50, bool endpoint = true)
		{
			double interval = stop / Math.Abs(stop) * Math.Abs(stop - start) / (num - 1);
			return (from value in Enumerable.Range(0, num) select start + value * interval).ToArray();
		}

		/// <summary>
		/// Serialize an Array with any dimesions. see <seealso cref="Deserialize(byte[])"/>
		/// </summary>
		/// <param name="array">this array to be serialized</param>
		/// <returns>
		/// <prar>
		///		Serialized data with a byte array. The data format is as follow. All byte orders are Lttle endian (Intel x86)
		/// </prar>
		/// <para>
		///		| length of type name in byte (1 byte)| type name in UTF8 | dimention count (1 byte) | dimensions (Ex. {3,2,5,101}, all are int) | all elements |
		/// </para>
		/// </returns>
		public static byte[] Serialize(this Array array)
		{
			Debug.Assert(BitConverter.IsLittleEndian, "Your system is big endian system, Currently Array Deserialize(this byte[] array) only supports Little Endian system,");
			byte[]? btRcd = null;
			if (array != null)
			{
				Type oType = array.GetType().GetElementType()!;
				if (oType.IsValueType)
				{
					try
					{
						using (MemoryStream ms = new MemoryStream(409600000))
						{
							//convert type name to UTF8
							byte[] typeName = Encoding.UTF8.GetBytes(oType.FullName!);
							// length of type name in byte (1 byte)
							ms.WriteByte((byte)typeName.Length);
							//type name in UTF8
							ms.Write(typeName, 0, typeName.Length);
							int[] dims = array.GetDimensions();
							//dimention count
							ms.WriteByte((byte)dims.Length);
							//dimensions
							ms.Write(MemoryMarshal.AsBytes(dims.AsSpan()));
							//array data
							int nArrayCount = Buffer.ByteLength(array);
							GCHandle gchArray = GCHandle.Alloc(array, GCHandleType.Pinned);
                            nint ptrArray = gchArray.AddrOfPinnedObject();
							byte[] btTemp = new byte[nArrayCount];
							Marshal.Copy(ptrArray, btTemp, 0, nArrayCount);
							ms.Write(btTemp, 0, nArrayCount);
							btRcd = ms.ToArray();
							gchArray.Free();
						}
					}
					catch (Exception /*ex*/)
					{//
						btRcd = null;
					}
				}
			}
			return btRcd!;
		}

		/// <summary>
		/// Deserialize data to a Array. see <seealso cref="Serialize(Array)"/>
		/// </summary>
		/// <param name="array">an </param>
		/// <returns></returns>
		public static Array Deserialize(this byte[] array)
		{
			Debug.Assert(BitConverter.IsLittleEndian, "Your system is big endian system, Currently Array Deserialize(this byte[] array) only supports Little Endian system,");
			Array? oRcd = null;
			if (array != null)
			{
				try
				{
					using (MemoryStream ms = new MemoryStream(array))
					{
						ms.Seek(0, SeekOrigin.Begin);
						//length of type name in byte (1 byte)
						byte nTypeName = (byte)ms.ReadByte();
						//type name in UTF8
						byte[] typeName = new byte[nTypeName];
						ms.Read(typeName, 0, typeName.Length);
						string strTypeName = Encoding.UTF8.GetString(typeName);
						Type oType = Type.GetType(strTypeName)!;
						//dimention count
						byte dimsLength = (byte)ms.ReadByte();
						//dimensions
						int[] dims = new int[dimsLength];
						byte[] btTemp = new byte[sizeof(int)];
						bool bReadError = false;
						for (int i = 0; i < dims.Length; i++)
						{
							if (ms.Read(btTemp, 0, btTemp.Length) < btTemp.Length)
							{
								bReadError = true;
								break;
							}
							dims[i] = BitConverter.ToInt32(btTemp);
						}
						if (!bReadError)
						{
							oRcd = Array.CreateInstance(oType, dims);
							//Array.Clear(indices, 0, dims.Length);
							int nCount = Buffer.ByteLength(oRcd);
							GCHandle gchArray = GCHandle.Alloc(oRcd, GCHandleType.Pinned);
                            nint ptrArray = gchArray.AddrOfPinnedObject();
							btTemp = new byte[nCount];
							if (ms.Read(btTemp, 0, nCount) == nCount)
							{
								Marshal.Copy(btTemp, 0, ptrArray, nCount);
							}
							else
							{
								oRcd = null;
							}
							gchArray.Free();
						}
					}
				}
				catch (Exception /*e*/)
				{
					oRcd = null;
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Compress a byte array data. the result can decompressed by <see cref="Decompress(byte[])"/>
		/// </summary>
		/// <param name="data">Compressed data. null: orignal data is null, or IO exception</param>
		/// <returns></returns>
		public static byte[] CompressBytes(this byte[] data)
		{
			byte[]? oRcd = null;
			try
			{
				using (var inputStream = new MemoryStream(data))
				{
					using (var msCompressed = new MemoryStream(409600000)) //400M buffer will be enough for our capture data
					{
						//Use CompressionLevel.SmallestSize will be very slow (10X), and its output size is not changed much more
						//and GZipStream is little bit fast, and its compression ratio is little bit high.
						//using (DeflateStream compressor = new DeflateStream(msCompressed, CompressionMode.Compress))
						//using (DeflateStream compressor = new DeflateStream(msCompressed, CompressionLevel.SmallestSize))
						//using (GZipStream compressor = new GZipStream(msCompressed, CompressionMode.Compress))
						//{
						//	outputStream.CopyTo(compressor);
						//}
						using (var compressor = new BrotliStream(msCompressed, CompressionLevel.Optimal))
						{
							inputStream.CopyTo(compressor);
						}
						oRcd = msCompressed.ToArray();
					}
				}
			}
			catch (Exception /*e*/)
			{
			}
			return oRcd!;
		}

		/// <summary>
		/// Decompress a byte array data tha was compressed by <see cref="Compress(byte[])"/>
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] DecompressBytes(this byte[] data)
		{
			byte[]? oRcd = null;
			try
			{
				using (MemoryStream inputStream = new MemoryStream(data))
				{
					//and GZipStream is little bit fast
					//using (DeflateStream decompressor = new DeflateStream(ms, CompressionMode.Decompress))
					//using (GZipStream decompressor = new GZipStream(inputStream, CompressionMode.Decompress))
					//{
					//	using (MemoryStream msDecompressed = new MemoryStream(409600000))
					//	{
					//		decompressor.CopyTo(msDecompressed);
					//		oRcd = msDecompressed.ToArray();
					//	}
					//}
					using (var decompressor = new BrotliStream(inputStream, CompressionMode.Decompress))
					{
						using (var outputStream = new MemoryStream(409600000))
						{
							decompressor.CopyTo(outputStream);
							oRcd = outputStream.ToArray();
						}
					}
				}
			}
			catch (Exception /*e*/)
			{
			}
			return oRcd!;
		}

		/// <summary>
		/// Make bool array as a mask array
		/// </summary>
		/// <example>
		/// <code>
		/// // C# sample code
		/// double dMax = 15.2;
		/// double dMin = 3.5;
		/// Func&lt;double, bool&gt; func = (data) =&gt; {return (data&gt;dMax) || (data&lt;dMin);};
		/// Array oMask = myArray.MakeMask(func);
		/// //   myArray            oMask(0:false, 1:true)
		/// //[[[ 0  1  2  3  4]      [[[1 1 1 1 0]
		/// //  [ 5  6  7  8  9]]       [0 0 0 0 0]]
		/// // [[10 11 12 13 14]       [[0 0 0 0 0]
		/// //  [15 16 17 18 19]]       [0 1 1 1 1]]
		/// // [[20 21 22 23 24]       [[1 1 1 1 1]
		/// //  [25 26 27 28 29]]]      [1 1 1 1 1]]]
		/// </code>
		/// <code>
		/// #a test code in Python
		/// import numpy as np
		/// import numpy.ma as ma
		/// arr = np.arange(3 * 2 * 5).reshape(3, 2, 5)
		/// mask = np.logical_or(arr&lt;4,arr&gt;15)
		/// print(arr)
		/// print(mask)
		/// </code>
		/// </example>
		/// <param name="array">this array</param>
		/// <param name="conditions">
		/// <para>a condition function.</para>
		/// <para>The return value must be bool. And it will be set the return array as an element. False means the correspinding element is not masked (= the correspinding element can be used any calculation) </para>
		/// <para>If this is null, all the element of return array will be false.</para>
		/// </param>
		/// <returns>a bool array which dimesion is same as this arry, and its elements are all bool.</returns>
		public static Array MakeMask(this Array array, Func<double, bool> conditions)
		{
			Array? oRcd = null;
			if (array != null)
			{
				int[] dims = array.GetDimensions();
				oRcd = Array.CreateInstance(typeof(bool), dims);
				if (conditions != null)
				{
					int[] indices = new int[dims.Length];
					Array.Clear(indices, 0, dims.Length);
					for (int i = 0; i < array.Length; i++)
					{
						double dTemp = (double)array.GetValue(indices)!;
						bool bTemp = conditions(dTemp);
						oRcd.SetValue(bTemp, indices);
						indices = indices.Increament(dims);
					}
				}
				else
				{
					//set all element as false
					oRcd.SetValue2(false);
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Return elements chosen from x or y depending on condition.
		/// see <see href="https://numpy.org/doc/stable/reference/generated/numpy.where.html">numpy.where</see>
		/// </summary>
		/// <example>
		/// <code>
		/// //C# code
		/// int[] arr = new int[]{2,3,4,5,6,7}
		/// Func&lt;double, bool&gt; fconditions = (data) =&gt; {return (data&lt;5);};
		/// Func&lt;double, double&gt; fx = (data) =&gt; {return (data + 10);};
		/// Func&lt;double, double&gt; fy = (data) =&gt; {return (data * 10);};
		/// var arr1 = arr.Where(fconditions,fx,fy); // [12,12,14,50,60,70]
		/// </code>
		/// <code>
		/// #Python code
		/// import numpy as np
		/// arr = np.array([1, 2, 3, 4, 5, 6, 7])
		/// wherepArr = np.where(arr &lt; 5, arr + 10, arr * 10)
		/// print(wherepArr)
		/// </code>
		/// </example>
		/// <param name="array">this array</param>
		/// <param name="conditions">conditions</param>
		/// <param name="x">use this return value when conditions is true</param>
		/// <param name="y">use this return value when conditions is false</param>
		/// <returns></returns>
		public static Array Where(this Array array, Func<double, bool> conditions, Func<double, double> x, Func<double, double> y)
		{
			Array? oRcd = null;
			if (array != null)
			{
				oRcd = array.Copy<double>();
				int[] dims = oRcd.GetDimensions();
				if (conditions != null && x != null && y != null)
				{
					int[] indices = new int[dims.Length];
					Array.Clear(indices, 0, dims.Length);
					for (int i = 0; i < array.Length; i++)
					{
						double dTemp = (double)oRcd.GetValue(indices)!;
						bool bTemp = conditions(dTemp);
						if (bTemp)
						{
							dTemp = x(dTemp);
						}
						else
						{
							dTemp = y(dTemp);
						}
						oRcd.SetValue(dTemp, indices);
						indices = indices.Increament(dims);
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Make a masked dobule array. the masked element will be NaN
		/// </summary>
		/// <example>
		/// <code>
		/// // C# sample code
		/// double dMax = 15.2;
		/// double dMin = 3.5;
		/// Func&lt;double, bool&gt; func = (data) =&gt; {return (data&gt;dMax) || (data&lt;dMin);};
		/// Array maskedDoubleArr = myArray.MakeMaskedDoubleArrayWithNaN(func);
		/// //      myArray             maskedDoubleArr (-- : double.NaN)
		/// //[[[ 0  1  2  3  4]      [[[-- -- -- --  4]
		/// //  [ 5  6  7  8  9]]       [ 5  6  7  8  9]]
		/// // [[10 11 12 13 14]       [[10 11 12 13 14]
		/// //  [15 16 17 18 19]]       [15 -- -- -- --]]
		/// // [[20 21 22 23 24]       [[-- -- -- -- --]
		/// //  [25 26 27 28 29]]]      [-- -- -- -- --]]]
		/// var mean1 = myArray.NanMean(0);
		/// var maskedMean = maskedDoubleArr.NanMean(0);
		/// //       mean1                    maskedMean
		/// //[[10. 11. 12. 13. 14.]      [[10.0 11.0 12.0 13.0 9.0]
		/// // [15. 16. 17. 18. 19.]]      [10.0  6.0  7.0  8.0 9.0]]
		/// </code>
		/// <code>
		/// #a test code in Python
		/// import numpy as np
		/// import numpy.ma as ma
		/// arr = np.arange(3 * 2 * 5).reshape(3, 2, 5)
		/// maskedArr = ma.masked_array(arr, mask=np.logical_or(arr&lt;4,arr&gt;15))
		/// print(arr)
		/// print(maskedArr)
		/// mean1 = arr.mean(axis=0);
		/// meanwithMask = maskedArr.mean(axis=0);
		/// print(mean1)
		/// print(meanwithMask)
		/// </code>
		/// </example>
		/// <param name="array">this array</param>
		/// <param name="conditions">
		/// <para>a condition function.</para>
		/// <para>The return value must be bool. And it will be set the return array as an element. False means the correspinding element is not masked (= the correspinding element can be used any calculation) </para>
		/// <para>If this is null, all the element of return array will be the value of original array.</para>
		/// </param>
		/// <returns>A double array, if a correspinding element is masked (= conditions function return true), the element will be set as NaN</returns>
		public static Array MakeMaskedDoubleArrayWithNaN(this Array array, Func<double, bool> conditions)
		{
			Array? oRcd = null;
			if (array != null)
			{
				//copy it to an array with double elements
				oRcd = array.Copy<double>();
				if (conditions != null)
				{
					int[] dims = oRcd.GetDimensions();
					int[] indices = new int[dims.Length];
					Array.Clear(indices, 0, dims.Length);
					for (int i = 0; i < array.Length; i++)
					{
						double dTemp = (double)oRcd.GetValue(indices)!;
						bool bTemp = conditions(dTemp);
						if (bTemp)
						{//if it is true, set NaN for this element
							oRcd.SetValue(double.NaN, indices);
						}
						indices = indices.Increament(dims);
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Make a masked dobule array after compaired with the target array. the masked element will be NaN
		/// </summary>
		/// <example>
		/// <code>
		/// // C# sample code
		/// Func&lt;double, doulbe bool&gt; func = (data1, data2) =&gt; {return (data1&gt;data2+1) || (data1&lt;data2+1);};
		/// Array maskedDoubleArr = myArray.MakeMaskedDoubleArrayWithNaN(target, func);
		/// //      myArray               target              maskedDoubleArr (-- : double.NaN)
		/// //[[[ 0  1  2  3  4]      [[[ 3  3  3  3  3]         [[[-- -- -- --  4]
		/// //  [ 5  6  7  8  9]]       [ 4  5  6  7  8]]          [ 5  6  7  8  9]]
		/// // [[10 11 12 13 14]       [[ 9 10 11 12 13]          [[10 11 12 13 14]
		/// //  [15 16 17 18 19]]       [14 13 13 13 13]]          [15 -- -- -- --]]
		/// // [[20 21 22 23 24]       [[29 29 29 29 29]          [[-- -- -- -- --]
		/// //  [25 26 27 28 29]]]      [29 29 29 29 29]]]         [-- -- -- -- --]]]
		/// var mean1 = myArray.NanMean(0);
		/// var maskedMean = maskedDoubleArr.NanMean(0);
		/// //       mean1                    maskedMean
		/// //[[10. 11. 12. 13. 14.]      [[10.0 11.0 12.0 13.0 9.0]
		/// // [15. 16. 17. 18. 19.]]      [10.0  6.0  7.0  8.0 9.0]]
		/// </code>
		/// <code>
		/// #a test code in Python
		/// import numpy as np
		/// import numpy.ma as ma
		/// arr = np.arange(3 * 2 * 5).reshape(3, 2, 5)
		/// arr2 = np.array([[[3, 3, 3, 3, 3],[4, 5, 6, 7, 8]],[[9,10,11,12,13],[14,13,13,13,13]],[[29,29,29,29,29],[29,29,29,29,29]]]);
		/// maskedArr = ma.masked_array(arr, mask=np.logical_or(arr&lt;arr2+1, arr&gt;arr2+1))
		/// print(arr)
		/// print(arr2)
		/// print(maskedArr)
		/// mean1 = arr.mean(axis=0);
		/// meanwithMask = maskedArr.mean(axis=0);
		/// print(mean1)
		/// print(meanwithMask)
		/// </code>
		/// </example>
		/// <param name="array">this array</param>
		/// <param name="target">target array to be compaired</param>
		/// <param name="conditions">
		/// <para>a condition function.</para>
		/// <para>The return value must be bool. And it will be set the return array as an element. False means the correspinding element is not masked (= the correspinding element can be used any calculation) </para>
		/// <para>If this is null, all the element of return array will be the value of original array</para>
		/// </param>
		/// <returns>A double array, if a correspinding element is masked (= conditions function return true), the element will be set as NaN</returns>
		public static Array MakeMaskedDoubleArrayWithNaN(this Array array, Array target, Func<double, double, bool> conditions)
		{
			Array? oRcd = null;
			if (array != null)
			{
				//copy it to an array with double elements
				oRcd = array.Copy<double>();
				if (conditions != null && target != null)
				{
					int[] dims = oRcd.GetDimensions();
					int[] dimsTarget = target.GetDimensions();
					if (dims.Length == dimsTarget.Length &&
						dims.SequenceEqual(dimsTarget))
					{
						int[] indices = new int[dims.Length];
						Array.Clear(indices, 0, dims.Length);
						for (int i = 0; i < array.Length; i++)
						{
							double dTemp1 = (double)oRcd.GetValue(indices)!;
							double dTemp2 = (double)target.GetValue(indices)!;
							bool bTemp = conditions(dTemp1, dTemp2);
							if (bTemp)
							{//if it is true, set NaN for this element
								oRcd.SetValue(double.NaN, indices);
							}
							indices = indices.Increament(dims);
						}
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Clip the element value by using given minimum and maximum.<br/>
		/// see <see href="https://numpy.org/doc/stable/reference/generated/numpy.clip.html">numpy.clip</see>,
		/// see also <seealso href="https://numpy.org/doc/stable/reference/generated/numpy.minimum.html">numpy.minimum</seealso>,
		/// see also <seealso href="https://numpy.org/doc/stable/reference/generated/numpy.maximum.html">umpy.maximum</seealso>.
		/// </summary>
		/// <example>
		/// <code>
		/// //C# code
		/// int[] arr = new int[] { 1, 2, 3, 4, 5, 6, 7 };
		/// var cliparr1 = arr.Clip(3, 5);  //{3,3,3,4,5,5,5}, equivalent with np.clip(arr,3,5);
		/// var clipmaximums = arr.Clip(4, null); //{4,4,4,4,5,6,7}, equivalent with np.maximum(arr,4);
		/// var clipminimums = arr.Clip(null, 3); //{1,2,3,3,3,3,3}, equivalent with np.minimum(arr,3);
		/// int[] clippermin = new int[] { 3, 1, 2, 6, 5, 4, 7 };
		/// int[] clippermax = new int[] { 2, 3, 5, 4, 6, 7, 1 };
		/// var clipmaximum = arr.Clip(clippermin, null);   //[3 2 3 6 5 6 7], equivalent with np.maximum(arr,clippermin);
		/// var clipmiimum = arr.Clip(null, clippermax);   //[1 2 3 4 5 6 1], equivalent with np.minimum(arr,clippermax);
		/// var cliparr4 = arr.Clip(clippermin, clippermax);  //{2,2,3,4,5,6,1}, equivalent with np.clip(arr,clippermin,clippermax);
		/// </code>
		/// <code>
		/// #Python code
		/// import numpy as np
		/// arr = np.array([1, 2, 3, 4, 5, 6, 7])
		/// cliparr1 = np.clip(arr, 3, 5)
		/// print(cliparr1)
		/// clipmaximums = np.maximum(arr, 4)
		/// clipminimums = np.minimum(arr, 3)
		/// print(clipmaximums)
		/// print(clipminimums)
		/// clippermin = np.array([3, 1, 2, 6, 5, 4, 7])
		/// clippermax = np.array([2, 3, 5, 4, 6, 7, 1])
		/// clipmaximum = np.maximum(arr, clippermin)
		/// clipminimum = np.minimum(arr, clippermax)
		/// cliparr4 = np.clip(arr, clippermin, clippermax)
		/// print(clipmaximum)
		/// print(clipminimum)
		/// print(cliparr4)
		/// </code>
		/// </example>
		/// <param name="array">this array</param>
		/// <param name="minimum">minimum, can be a scaler or an array, if it is a array, it dimension must be total same as this array, if it is null, it does not compaire with this </param>
		/// <param name="maximum">maximum, can be a scaler or an array, if it is a array, it dimension must be total same as this array</param>
		/// <returns>a clipped double array</returns>
		public static Array Clip(this Array array, object minimum, object maximum)
		{
			Array? oRcd = null;
			if (array != null)
			{
				int[] dims = array.GetDimensions();
				Array? aMinimum = null;
				double dMinimum = 0;
				bool bSame = true;
				if (minimum != null)
				{
					if (minimum is Array)
					{
						aMinimum = minimum as Array;
						int[] minmumDims = aMinimum!.GetDimensions();
						if (minmumDims.Length != dims.Length ||
							!dims.SequenceEqual(minmumDims))
						{
							bSame = false;
						}
					}
					else
					{
						dMinimum = Convert.ToDouble(minimum);
					}
				}
				if (bSame)
				{
					Array? aMaximum = null;
					double dMaximum = 0;
					if (maximum != null)
					{
						if (maximum is Array)
						{
							aMaximum = maximum as Array;
							int[] maxmumDims = aMaximum!.GetDimensions();
							if (maxmumDims.Length != dims.Length ||
								!dims.SequenceEqual(maxmumDims))
							{
								bSame = false;
							}
						}
						else
						{
							dMaximum = Convert.ToDouble(maximum);
						}
					}
					if (bSame)
					{
						oRcd = array.Copy<double>();
						if (maximum != null ||
							minimum != null)
						{//if maximum&minimum all are null, just return the copy of original array
							int[] indices = new int[dims.Length];
							Array.Clear(indices, 0, dims.Length);
							for (int i = 0; i < array.Length; i++)
							{
								double dValue = (double)oRcd.GetValue(indices)!;
								if (minimum != null)
								{
									double dMin = aMinimum == null ? dMinimum : (double)Convert.ChangeType(aMinimum.GetValue(indices)!, typeof(double));
									if (dValue < dMin)
									{
										dValue = dMin;
									}
								}
								if (maximum != null)
								{
									double dMax = aMaximum == null ? dMaximum : (double)Convert.ChangeType(aMaximum.GetValue(indices)!, typeof(double));
									if (dValue > dMax)
									{
										dValue = dMax;
									}
								}
								oRcd.SetValue(dValue, indices);
								indices = indices.Increament(dims);
							}
						}
					}

				}
			}
			return oRcd!;
		}

		/// <summary>
		/// Interpolation<br/>
		/// see <see href="https://numpy.org/doc/stable/reference/generated/numpy.interp.html">numpy.interp</see>
		/// </summary>
		/// <example>
		/// <code>
		/// //C# code
		/// double[] dArr = new double[] { 1, 1.5, 2, 2.1, 3.0, 4.0 };
		/// double[] xp = new double[] { 1, 2, 3, 4 };
		/// double[] fp = new double[] { 3, 2, 4, -1 };
		/// var interpArr = dArr.Interp(xp, fp);  //[ 3.   2.5  2.   2.2  4.  -1. ]
		/// </code>
		/// <code>
		/// #Python code
		/// import numpy as np
		/// arr = np.array([[0, 1, 1.5, 2.1, 2.9, 3.0, 4.0]])
		/// xp = np.array([1, 2, 3])
		/// fp = np.array([3, 2, 0])
		/// x = np.interp(arr, xp, fp)
		/// print(x)
		/// </code>
		/// </example>
		/// <typeparam name="T">original array type, a one dimesional array with this type</typeparam>
		/// <param name="array">this array,The x-coordinates at which to evaluate the interpolated values.</param>
		/// <param name="xp">The x-coordinates of the data points, must be increasing</param>
		/// <param name="fp">The y-coordinates of the data points, same length as xp</param>
		/// <returns></returns>
		public static double[] Interp<T>(this T[] array, double[] xp, double[] fp) where T : struct
		{
			double[]? oRcd = null;
			if (array != null &&
				xp != null &&   //must be increasing, but do not check it here
				fp != null &&
				xp.Length == fp.Length)   //must be same
			{
				oRcd = new double[array.Length];
				double[] doubleArray = (double[])array.Copy<double>();
				for (int i = 0; i < oRcd.Length; i++)
				{
					//find indice that array[i] is between those 2 values
					for (int j = 0; j < xp.Length; j++)
					{
						double dTempDistance = doubleArray[i] - xp[j];
						if (Math.Abs(dTempDistance) <= double.Epsilon)
						{   //array[i] == xp[j], np.interp() take the last value of fp if next xp value is same as xp[j]
							int n = 1;
							while (j + n < xp.Length && xp[j + n] == xp[j])
							{
								n++;
							}
							oRcd[i] = fp[j + n - 1];
							break;
						}
						else if (dTempDistance < 0)
						{   //array[i] < xp[j]
							if (j == 0)
							{   // is first xp
								oRcd[i] = fp[j];
							}
							else
							{   // between j-1 and j, 
								double dx = xp[j] - xp[j - 1];
								double dy = fp[j] - fp[j - 1];
								if (Math.Abs(dx) <= double.Epsilon)
								{   // the line is a vertical line
									oRcd[i] = fp[j];
								}
								else if (Math.Abs(dy) <= double.Epsilon)
								{   // this line is a horizongtal line
									oRcd[i] = fp[j];
								}
								else
								{   //build up a line quation by using (xp[j-1], fp[j-1]) and (xp[j], fp[j])
									oRcd[i] = dy / dx * (doubleArray[i] - xp[j]) + fp[j];
								}
							}
							break;
						}
						else
						{   //array[i] > xp[j]
							if (j >= xp.Length - 1)
							{   // is last xp
								oRcd[i] = fp[j];
								break;
							}
							continue;
						}
					}
				}
			}
			return oRcd!;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="step"></param>
		/// <returns></returns>
		public static int[] GetThreshSweepList(int start, int end, int step)
		{
			var toReturn = new List<int>();
			for (int i = start; i < end + 1; i += step)
			{
				toReturn.Add(i);
			}
			return toReturn.ToArray();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="numBins"></param>
		/// <param name="threshStart"></param>
		/// <param name="threshEnd"></param>
		/// <param name="threshStep"></param>
		/// <param name="numThresh"></param>
		/// <returns></returns>
		public static Array? GetBinThreshList(int numBins, int threshStart, int threshEnd, int threshStep, int numThresh)
		{
			if (numBins > 0)
			{
				int[] ThreshSweepList = GetThreshSweepList(threshStart, threshEnd, threshStep);
				if (numThresh > ThreshSweepList.Length) numThresh = ThreshSweepList.Length;
				var binThreshList = new int[numBins, numThresh];

				for (int i = 0; i < binThreshList.GetLength(0); i++)
				{
					for (int y = 0; y < binThreshList.GetLength(1); y++)
					{
						binThreshList[i, y] = ThreshSweepList[y] + i;
					}
				}
				return binThreshList;
			}
			else
				return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		/// 
		public static double[] Revert1D(this double[] array)
		{
			var result = new double[array.Length];

			for (int k = 0; k < array.Length; k++)
				result[array.Length - 1 - k] = array[k];

			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="decimals"></param>
		/// <returns></returns>
		public static Array Round(this Array array, int decimals = 0)
		{
			Func<double, double, double> func = (data1, data2) => Math.Round(data1, (int)data2);
			return ByElement(array, decimals, func);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public static Array Floor(this Array array)
		{
			Func<double, double> func = (data) => Math.Floor(data);
			return ByElement(array, func);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <returns></returns>
		public static Array Sqrt(this Array array)
		{
			Func<double, double, double> func = (data1, data2) => Math.Sqrt(data1);
			return ByElement(array, null, func);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="exponents"></param>
		/// <returns></returns>
		public static double[,] Pow(this double[] vector, double[] exponents)
		{
			var result = new double[exponents.Length, vector.Length];

			for (int j = 0; j < exponents.Length; j++)
			{
				for (int i = 0; i < vector.Length; i++)
				{
					result[j, i] = Math.Pow(vector[i], exponents[j]);
				}
			}

			return result;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="matrix"></param>
		/// <returns></returns>
		public static double[,] RemoveValuesLessThanZero(this double[,] matrix)
		{
			for (int j = 0; j < matrix.GetLength(0); j++)
			{
				for (int i = 0; i < matrix.GetLength(1); i++)
				{
					if (matrix[j, i] < 0) matrix[j, i] = 0;
				}
			}
			return matrix;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayToCompare"></param>
		/// <returns></returns>
		public static Array LesserThan(this Array array, Array arrayToCompare)
		{
			Func<double, double, bool> func = (data1, data2) => data1 < data2;
			return ByElement(array, arrayToCompare, func);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayToCompare"></param>
		/// <returns></returns>
		public static Array GreaterThan(this Array array, Array arrayToCompare)
		{
			Func<double, double, bool> func = (data1, data2) => data1 > data2;
			return ByElement(array, arrayToCompare, func);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayToCompare"></param>
		/// <returns></returns>
		public static Array Logical_Or(Array array, Array arrayToCompare)
		{
			Func<bool, bool, bool> func = (data1, data2) => data1 || data2;
			return ByElement(array, arrayToCompare, func);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayToCompare"></param>
		/// <returns></returns>
		public static Array Logical_And(Array array, Array arrayToCompare)
		{
			Func<bool, bool, bool> func = (data1, data2) => data1 && data2;
			return ByElement(array, arrayToCompare, func);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="c"></param>
		/// <param name="x"></param>
		/// <returns></returns>
		public static double[] Cubic(double a, double b, double c, double[] x)
		{
			var toReturn = new double[x.Length];
			for (int i = 0; i < x.Length; i++)
			{
				toReturn[i] = a * x[i] + b * Math.Pow(x[i], 2) + c * Math.Pow(x[i], 3);
			}
			return toReturn;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="c"></param>
		/// <param name="x"></param>
		/// <returns></returns>
		public static Array Cubic(Array a, Array b, Array c, double x)
		{
			var t1 = a.Multip(x);
			var t2 = b.Multip(x).Multip(x);
			var t3 = c.Multip(x).Multip(x).Multip(x);
			return t1.Sum(t2).Sum(t3); // return a * x + (b * (x * x)) + (c * (x * x * x));
		}

		private static void Cubic(double[] c, double[] x, ref double func, object obj)
		{
			func = c[0] * x[0] + c[1] * Math.Pow(x[0], 2) + c[2] * Math.Pow(x[0], 3);
		}

		private static void Gauss(double[] c, double[] x, ref double func, object obj)
		{
			func = c[0] * Math.Exp(-Math.Pow(x[0] - c[1], 2) / (2 * Math.Pow(c[2], 2)));
		}

		//private static double[] LevenbergMarquardtOptimize(enmMathFunction funtionName, double[] xdata, double[] ydata, double[] p0)
		//{
		//	var result = new double[p0.Length];
		//	var x = (double[,])((Array)xdata).AddDimension<double>(true);
		//	var y = new double[xdata.Length];
		//	// int info;
		//	alglib.lsfitstate state;
		//	alglib.lsfitreport rep;

		//	alglib.lsfitcreatef(x, ydata, p0, 1.49012e-08, out state);  //alglib.lsfitcreatef(x, ydata, p0, 1.49012e-08, out state);
		//	alglib.lsfitsetcond(state, double.Epsilon, 0);

		//	switch (funtionName)
		//	{
		//		case enmMathFunction.Gauss:
		//			{
		//				alglib.lsfitfit(state, Gauss, null, null);
		//				break;
		//			}
		//		case enmMathFunction.Cubic:
		//			{
		//				alglib.lsfitfit(state, Cubic, null, null);
		//				break;
		//			}
		//	}

		//	alglib.lsfitresults(state, out result, out rep);
		//	return result;
		//}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="functionName"></param>
		/// <param name="xdata"></param>
		/// <param name="ydata"></param>
		/// <param name="p0"></param>
		public static double[] CurveFit(enmMathFunction functionName, double[] xdata, double[] ydata, double[] p0)
		{
			double[]? result = null;
			bool bounded_problem = false;

			foreach (var elem in p0)
			{
				bounded_problem = elem == double.NegativeInfinity || elem == double.PositiveInfinity;
				if (bounded_problem) { break; }
			}

			if (!bounded_problem && xdata.Length > 0 && ydata.Length > 0 && p0.Length == 3)
			{
				//var method = "lm";
				var sigma = p0[2];

				ydata = (double[])ydata.SanitizeArray();
				xdata = (double[])xdata.SanitizeArray();

				if (ydata.Length != 1 && p0.Length > ydata.Length)
					throw new Exception($"The number of func parameters={p0.Length} must not exceed the number of data points={ydata.Length}");

				// Least Squares:
				result = []; // LevenbergMarquardtOptimize(functionName, xdata, ydata, p0);
			}
			else
			{
				throw new Exception("Method 'lm' only works for unconstrained problems. Use 'trf' or 'dogbox' instead.");
			}
			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arrs"></param>
		/// <returns></returns>
		public static List<int[]> DotNetIx(params int[][] arrs)
		{
			var toReturn = new List<int[]>();

			for (int i = 0; i < arrs.Length; i++)
			{
				int[] ndarr = arrs[i];
				toReturn.Add(ndarr);
			}
			return toReturn;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="bolArray"></param>
		/// <returns></returns>
		public static Array Visible(this Array array, Array bolArray)
		{
			Array? result = null;

			if (array.GetLength(0) == bolArray.GetLength(0) && array.GetLength(1) == bolArray.GetLength(1) && array.GetDimensions().Length == 3)
			{
				result = Array.CreateInstance(array.GetType().GetElementType()!, new int[] { bolArray.CountByValue(true), array.GetLength(2) });
				var bolMatrix = (bool[,])bolArray;
				int index = 0;

				for (int j = 0; j < array.GetLength(0); j++)
				{
					for (int i = 0; i < array.GetLength(1); i++)
					{
						if (bolMatrix[j, i])
						{
							var temp = (Array)array.PartOf(new SliceIndex?[] { new SliceIndex(j), new SliceIndex(i), null }!);
							result.SetValue(temp, new int?[] { index, null });
							index++;
						}
					}
				}
			}

			return result!;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="numBins"></param>
		/// <returns></returns>
		public static Array MakeOpenBins(Array array, int numBins)
		{
			var result = Array.CreateInstance(typeof(double), array.GetDimensions());
			Array acum = Array.CreateInstance(typeof(double), new int[] { array.GetLength(1), array.GetLength(2) });

			for (int k = numBins - 1; k >= 0; k--)
			{
				var temp = (Array)array.PartOf(new SliceIndex?[] { new SliceIndex(k), null, null }!);
				acum = acum.Sum(temp);
				result.SetValue(acum, new int?[] { k, null, null });
			}

			return result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="numBins"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static Array MakeShutterWindow(Array data, int numBins)
		{
			var cc_window_all_means = Array.CreateInstance(typeof(double), numBins, data.GetLength(1));
			var cc_window_max_mean = new double[numBins];
			var cc_window_threshold = new double[numBins];
			double leading_edge_thresh = 0.5, view_period = 0.003, shutter_period = 1.2; // leading_edge_advance = 0.006, 
			int views_per_shutter = (int)(shutter_period / view_period);

			Parallel.For(0, numBins, (k, state) =>
			{
				var temp = (Array)((Array)data.PartOf(new SliceIndex?[] { new SliceIndex(k), null, null }!)).NanMean(1);
				temp = temp.Floor().Squeeze();
				cc_window_all_means.SetValue(temp, new int?[] { k, null });
				//var temp = (Array)cc_window_all_means.PartOf(new SliceIndex[] { new SliceIndex(k), null });
				cc_window_max_mean[k] = (double)temp.NanMax();
				cc_window_threshold[k] = cc_window_max_mean[k] * leading_edge_thresh;
			});

			var vector = (double[])(Array)cc_window_all_means.PartOf(new SliceIndex?[] { new SliceIndex(0), null }!);
			var cc_window_start = vector.ToList().FindIndex(x => x > cc_window_threshold[0]);

			vector = (double[])(Array)cc_window_all_means.PartOf(new SliceIndex[] { new SliceIndex(0), new SliceIndex(cc_window_start, null) });
			var cc_window_end = vector.ToList().FindIndex(x => x < cc_window_threshold[0]);

			if (cc_window_end < views_per_shutter)
				throw new Exception($"Shutter window less than nominal: {cc_window_end} views");
			else
				cc_window_end = views_per_shutter;

			return (Array)data.PartOf(new SliceIndex?[] { null, new SliceIndex(cc_window_start, cc_window_start + cc_window_end, null) }!);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="threshold"></param>
		/// <param name="viewsFirst"></param>
		/// <param name="viewsLast"></param>
		/// <returns></returns>
		public static (double, double, double) DynamicMetrics(double[] vector, int threshold, int viewsFirst, int viewsLast)
		{
			double view_period = 0.003;
			int leading_edge_advance = 2, lag_period = 1000;
			int lag_views = (int)(lag_period / 1000 / view_period);
			int idx_start = vector.ToList().FindIndex(x => x > threshold) + leading_edge_advance;
			int idx_end = idx_start + lag_views;

			var dyna_vec = (Array)vector.PartOf(new SliceIndex[] { new SliceIndex(idx_start, idx_end) });
			var mean_first_50ms = (double)((Array)dyna_vec.PartOf(new SliceIndex[] { new SliceIndex(null, viewsFirst) })).NanMean();
			var mean_last_300ms = (double)((Array)dyna_vec.PartOf(new SliceIndex[] { new SliceIndex(dyna_vec.Length - viewsLast, null) })).NanMean();
			var dyna_per_pixel = 100 * (1 - mean_first_50ms / mean_last_300ms);

			return (dyna_per_pixel, mean_first_50ms, mean_last_300ms);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="array"></param>
		/// <param name="toCompare"></param>
		/// <param name="condition"></param>
		/// <param name="isTrue"></param>
		/// <returns></returns>
		public static Array ReplaceIf(this Array array, Array toCompare, bool condition, object? isTrue = null)
		{
			Array? result = null;

			if (array != null && toCompare != null)
			{
				result = (Array)array.Clone();
				if (array.GetDimensions().Length == toCompare.GetDimensions().Length && array.GetDimensions().Length == 4)
				{
					var arrCompare = (bool[,,,])toCompare;

					for (int a = 0; a < toCompare.GetLength(0); a++)
					{
						for (int b = 0; b < toCompare.GetLength(1); b++)
						{
							for (int c = 0; c < toCompare.GetLength(2); c++)
							{
								for (int d = 0; d < toCompare.GetLength(3); d++)
								{
									if (arrCompare[a, b, c, d] == condition)
									{
										result.SetValue(isTrue, new int[] { a, b, c, d });
									}
								}
							}
						}
					}
				}
			}
			return result!;
		}

	}
}