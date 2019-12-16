/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：基于时间和价值的关键帧                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 20:05:11                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace SsitEngine.Mathematics
{
    /// <summary>Key frame base on time and value.</summary>
    [Serializable]
    public struct KeyFrame
    {
        /// <summary>Time of key frame.</summary>
        public double time;

        /// <summary>Value of key frame.</summary>
        public double value;

        /// <summary>In tangent of key frame.</summary>
        public double inTangent;

        /// <summary>Out tangent of key frame.</summary>
        public double outTangent;

        /// <summary>Constructor.</summary>
        /// <param name="time">Time of key frame.</param>
        /// <param name="value">Value of key frame.</param>
        public KeyFrame( double time, double value )
        {
            this.time = time;
            this.value = value;
            inTangent = 0.0;
            outTangent = 0.0;
        }

        /// <summary>Constructor.</summary>
        /// <param name="time">Time of key frame.</param>
        /// <param name="value">Value of key frame.</param>
        /// <param name="inTangent">In tangent of key frame.</param>
        /// <param name="outTangent">Out tangent of key frame.</param>
        public KeyFrame( double time, double value, double inTangent, double outTangent )
        {
            this.time = time;
            this.value = value;
            this.inTangent = inTangent;
            this.outTangent = outTangent;
        }
    }
}