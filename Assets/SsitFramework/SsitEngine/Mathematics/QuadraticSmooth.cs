/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：二次光滑                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 20:09:02                             
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Mathematics
{
    /// <summary>Quadratic Smooth.</summary>
    public static class QuadraticSmooth
    {
        /// <summary>Five point quadratic smooth.</summary>
        /// <param name="source">Data source.</param>
        /// <returns>Smooth result.</returns>
        public static double[] FivePointSmooth( double[] source )
        {
            var length = source.Length;
            var numArray = new double[length];
            if (length < 5)
            {
                for (var index = 0; index < length; ++index)
                {
                    numArray[index] = source[index];
                }
            }
            else
            {
                numArray[0] =
                    (31.0 * source[0] + 9.0 * source[1] - 3.0 * source[2] - 5.0 * source[3] + 3.0 * source[4]) / 35.0;
                numArray[1] =
                    (9.0 * source[0] + 13.0 * source[1] + 12.0 * source[2] + 6.0 * source[3] - 5.0 * source[4]) / 35.0;
                for (var index = 2; index < length - 2; ++index)
                {
                    numArray[index] = (-3.0 * (source[index - 2] + source[index + 2]) +
                                       12.0 * (source[index - 1] + source[index + 1]) + 17.0 * source[index]) / 35.0;
                }
                numArray[length - 2] = (9.0 * source[length - 1] + 13.0 * source[length - 2] +
                                        12.0 * source[length - 3] + 6.0 * source[length - 4] -
                                        5.0 * source[length - 5]) / 35.0;
                numArray[length - 1] = (31.0 * source[length - 1] + 9.0 * source[length - 2] -
                                        3.0 * source[length - 3] - 5.0 * source[length - 4] +
                                        3.0 * source[length - 5]) / 35.0;
            }
            return numArray;
        }

        /// <summary>Seven point quadratic smooth.</summary>
        /// <param name="source">Data source.</param>
        /// <returns>Smooth result.</returns>
        public static double[] SevenPointSmooth( double[] source )
        {
            var length = source.Length;
            var numArray = new double[length];
            if (length < 7)
            {
                for (var index = 0; index < length; ++index)
                {
                    numArray[index] = source[index];
                }
            }
            else
            {
                numArray[0] = (32.0 * source[0] + 15.0 * source[1] + 3.0 * source[2] - 4.0 * source[3] -
                               6.0 * source[4] - 3.0 * source[5] + 5.0 * source[6]) / 42.0;
                numArray[1] = (5.0 * source[0] + 4.0 * source[1] + 3.0 * source[2] + 2.0 * source[3] + source[4] -
                               source[6]) / 14.0;
                numArray[2] = (source[0] + 3.0 * source[1] + 4.0 * source[2] + 4.0 * source[3] + 3.0 * source[4] +
                               source[5] - 2.0 * source[6]) / 14.0;
                for (var index = 3; index < length - 3; ++index)
                {
                    numArray[index] = (-2.0 * (source[index - 3] + source[index + 3]) +
                                       3.0 * (source[index - 2] + source[index + 2]) +
                                       6.0 * (source[index - 1] + source[index + 1]) + 7.0 * source[index]) / 21.0;
                }
                numArray[length - 3] = (source[length - 1] + 3.0 * source[length - 2] + 4.0 * source[length - 3] +
                                        4.0 * source[length - 4] + 3.0 * source[length - 5] + source[length - 6] -
                                        2.0 * source[length - 7]) / 14.0;
                numArray[length - 2] = (5.0 * source[length - 1] + 4.0 * source[length - 2] + 3.0 * source[length - 3] +
                                        2.0 * source[length - 4] + source[length - 5] - source[length - 7]) / 14.0;
                numArray[length - 1] = (32.0 * source[length - 1] + 15.0 * source[length - 2] +
                                        3.0 * source[length - 3] - 4.0 * source[length - 4] - 6.0 * source[length - 5] -
                                        3.0 * source[length - 6] + 5.0 * source[length - 7]) / 42.0;
            }
            return numArray;
        }
    }
}