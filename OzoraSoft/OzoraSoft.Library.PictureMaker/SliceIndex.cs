using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzoraSoft.Library.PictureMaker
{
    /// <summary>
	/// for Python Slice notation [start:end:step]<br/>
	/// Ex.<br/>
	///		[:,1:2,:] --> new Slice[]{null,new Slice{Start=1, Stop=2}, null} or new Slice[]{new SliceIndex(),new Slice(1, 2)}<br/>
	///		[:,:,-1:2:-2] --> new SliceIndex[] { null, null, new SliceIndex(-1, 2, -2) }<br/>
	///		[:,:,:4:2] --> new SliceIndex[] { null, null, new SliceIndex(null, 4, 2) }<br/>
	///		[":","::-1"] --> new SliceIndex[] {null, new SliceIndex(null, null, -1)}   : it is np.fliplr(npInt), also it is np.flip(npInt, axis=1).<br/>
	/// </summary>
    public class SliceIndex
    {
        #region Constructors

        /// <summary>
        /// Constructor that creates a SliceIndex object to represent the set of indices that range
        /// </summary>
        public SliceIndex()
        {
        }

        /// <summary>
        /// Constructor that creates a SliceIndex object to represent the set of indices that range(start) specifies.
        /// </summary>
        /// <param name="nStart">Where the slicing begins</param>
        public SliceIndex(int? nStart)
        {
            Start = nStart;
            Stop = nStart;
        }

        /// <summary>
        /// Constructor that creates a SliceIndex object to represent the set of indices that range(start, stop) specifies.
        /// </summary>
        /// <param name="nStart">Where the slicing begins</param>
        /// <param name="nStop">Where to stop slicing+1</param>
        public SliceIndex(int? nStart, int? nStop)
        {
            Start = nStart;
            Stop = nStop;
        }

        /// <summary>
        /// Constructor that creates a SliceIndex object to represent the set of indices that range(start, stop, step) specifies.
        /// </summary>
        /// <param name="nStart">Where the slicing begins</param>
        /// <param name="nStop">Where to stop slicing+1</param>
        /// <param name="nStep">How much to increment between each index</param>
        public SliceIndex(int? nStart, int? nStop, int? nStep)
        {
            Start = nStart;
            Stop = nStop;
            Step = nStep;
        }

        #endregion Constructors

        #region properties

        /// <summary>
        /// Gets and returns the index to where the slicing begins.
        /// </summary>
        public int NStart
        {
            get
            {
                int nRcd = NStep >= 0 ? 0 : OriginalLength - 1;
                if (Start != null)
                {
                    nRcd = (int)Start;
                    if (nRcd < 0)
                    {
                        nRcd += OriginalLength;
                        if (nRcd < 0)
                        {
                            nRcd = 0;
                        }
                    }
                    else if (nRcd >= OriginalLength)
                    {
                        nRcd = OriginalLength - 1;
                    }
                }
                return nRcd;
            }
        }

        /// <summary>
        /// Gets and returns the index to where the slicing stops.
        /// </summary>
        public int NStop
        {
            get
            {
                int nRcd = NStep >= 0 ? OriginalLength : -1;
                if (Stop != null)
                {
                    nRcd = (int)Stop;
                    if (nRcd < 0)
                    {
                        nRcd += OriginalLength;
                        if (nRcd < 0)
                        {
                            nRcd = -1;
                        }
                    }
                    else if (nRcd > OriginalLength)
                    {
                        nRcd = OriginalLength;
                    }
                    if (NStep >= 0)
                    {// must be start < stop
                        if (nRcd <= NStart)
                        {
                            nRcd = NStart + 1;
                        }
                    }
                    else
                    {// must be stop < start
                        if (nRcd >= NStart)
                        {
                            nRcd = NStart - 1; //minimum value will be -1;
                        }
                    }
                }
                return nRcd;
            }
        }

        /// <summary>
        /// Gets and returns how much the slicing will be incrementing by.
        /// </summary>
        public int NStep
        {
            get
            {
                int nRcd = 1;
                if (Step != null && (int)Step != 0)
                {
                    nRcd = (int)Step;
                }
                return nRcd;
            }
        }

        /// <summary>
        /// Gets and sets the original array Length for a specific dimension (max index)
        /// </summary>
        public int OriginalLength
        {
            get; set;
        } = 0;

        /// <summary>
        /// Gets the new array length for a specific dimension from the effect of slicing.
        /// </summary>
        public int Length
        {
            get
            {
                int nRcd = NStop - NStart;
                if (nRcd * NStep >= 0)
                {
                    int nTemp = nRcd / NStep;
                    if (nRcd % NStep != 0)
                    {
                        nRcd = nTemp + 1;
                    }
                    else
                    {
                        nRcd = nTemp;
                    }
                }
                else
                {//length must be at least one
                    nRcd = 1;
                }
                return nRcd;
            }
        }

        /// <summary>
        /// Start index, slicing will start from this index (includes this index).<br/>
        /// <para>if Step is negative, Start &gt; Stop, otherwise Start is &lt; Stop.</para>
        ///	<para>null: start from 0 if Step is a positive number, or start from last index if Step is negative.</para>
        ///	<para>negative: start from (Array length + Start)</para>
        /// </summary>
        public int? Start { get; set; } = null;

        /// <summary>
        /// Stop index, Slicing will stop up to but not including the Stop index.<br/> 
        /// <para>If Step is a negative number Start &gt; Stop, otherwise Start &lt; Stop.</para>
        ///	<para>null: end at last index - 1 if Step is positive, or end at one index before 0 if Step is negative</para>
        ///	<para>negative: stop at (Array length + this value)</para>
        /// </summary>
        /// <example>
        /// Stop = 5:<br/>
        /// If Step is a positive number, slicing will stop at index 4.<br/>
        /// If Step is a negative number, slicing will stop at index 6.
        /// </example>
        public int? Stop { get; set; } = null;

        /// <summary>
        /// <para>Step is the increment from Start toward to Stop.</para>
        /// <para>This value cannot be 0.</para>
        /// </summary>
        public int? Step { get; set; } = null;

        #endregion properties
    }
}
