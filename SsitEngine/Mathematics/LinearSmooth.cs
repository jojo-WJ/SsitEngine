/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：Linear Smooth.                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 20:03:38                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Mathematics
{
    /// <summary>Linear Smooth.</summary>
    public static class LinearSmooth
    {
        /// <summary>Three point linear smooth.</summary>
        /// <param name="source">Data source.</param>
        /// <returns>Smooth result.</returns>
        public static double[] ThreePointSmooth( double[] source )
        {
            var length = source.Length;
            var numArray = new double[length];
            if (length < 3)
            {
                for (var index = 0; index < length; ++index)
                    numArray[index] = source[index];
            }
            else
            {
                numArray[0] = (5.0 * source[0] + 2.0 * source[1] - source[2]) / 6.0;
                for (var index = 1; index < length - 1; ++index)
                    numArray[index] = (source[index - 1] + source[index] + source[index + 1]) / 3.0;
                numArray[length - 1] = (5.0 * source[length - 1] + 2.0 * source[length - 2] - source[length - 3]) / 6.0;
            }
            return numArray;
        }

        /// <summary>Five point linear smooth.</summary>
        /// <param name="source">Data source.</param>
        /// <returns>Smooth result.</returns>
        public static double[] FivePointSmooth( double[] source )
        {
            var length = source.Length;
            var numArray = new double[length];
            if (length < 5)
            {
                for (var index = 0; index < length; ++index)
                    numArray[index] = source[index];
            }
            else
            {
                numArray[0] = (3.0 * source[0] + 2.0 * source[1] + source[2] - source[4]) / 5.0;
                numArray[1] = (4.0 * source[0] + 3.0 * source[1] + 2.0 * source[2] + source[3]) / 10.0;
                for (var index = 2; index < length - 2; ++index)
                    numArray[index] = (source[index - 2] + source[index - 1] + source[index] + source[index + 1] +
                                       source[index + 2]) / 5.0;
                numArray[length - 2] = (4.0 * source[length - 1] + 3.0 * source[length - 2] + 2.0 * source[length - 3] +
                                        source[length - 4]) / 10.0;
                numArray[length - 1] = (3.0 * source[length - 1] + 2.0 * source[length - 2] + source[length - 3] -
                                        source[length - 5]) / 5.0;
            }
            return numArray;
        }

        /// <summary>Seven point linear smooth.</summary>
        /// <param name="source">Data source.</param>
        /// <returns>Smooth result.</returns>
        public static double[] SevenPointSmooth( double[] source )
        {
            var length = source.Length;
            var numArray = new double[length];
            if (length < 7)
            {
                for (var index = 0; index < length; ++index)
                    numArray[index] = source[index];
            }
            else
            {
                numArray[0] = (13.0 * source[0] + 10.0 * source[1] + 7.0 * source[2] + 4.0 * source[3] + source[4] -
                               2.0 * source[5] - 5.0 * source[6]) / 28.0;
                numArray[1] = (5.0 * source[0] + 4.0 * source[1] + 3.0 * source[2] + 2.0 * source[3] + source[4] -
                               source[6]) / 14.0;
                numArray[2] = (7.0 * source[0] + 6.0 * source[1] + 5.0 * source[2] + 4.0 * source[3] + 3.0 * source[4] +
                               2.0 * source[5] + source[6]) / 28.0;
                for (var index = 3; index < length - 3; ++index)
                    numArray[index] = (source[index - 3] + source[index - 2] + source[index - 1] + source[index] +
                                       source[index + 1] + source[index + 2] + source[index + 3]) / 7.0;
                numArray[length - 3] = (7.0 * source[length - 1] + 6.0 * source[length - 2] + 5.0 * source[length - 3] +
                                        4.0 * source[length - 4] + 3.0 * source[length - 5] + 2.0 * source[length - 6] +
                                        source[length - 7]) / 28.0;
                numArray[length - 2] = (5.0 * source[length - 1] + 4.0 * source[length - 2] + 3.0 * source[length - 3] +
                                        2.0 * source[length - 4] + source[length - 5] - source[length - 7]) / 14.0;
                numArray[length - 1] = (13.0 * source[length - 1] + 10.0 * source[length - 2] +
                                        7.0 * source[length - 3] + 4.0 * source[length - 4] + source[length - 5] -
                                        2.0 * source[length - 6] - 5.0 * source[length - 7]) / 28.0;
            }
            return numArray;
        }
    }
}