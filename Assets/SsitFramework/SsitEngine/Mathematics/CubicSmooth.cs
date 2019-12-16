/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：立方平滑                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 20:09:49                             
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Mathematics
{
    /// <summary>Cubic Smooth.</summary>
    public static class CubicSmooth
    {
        /// <summary>Five point cubic smooth.</summary>
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
                numArray[0] = (69.0 * source[0] + 4.0 * source[1] - 6.0 * source[2] + 4.0 * source[3] - source[4]) /
                              70.0;
                numArray[1] =
                    (2.0 * source[0] + 27.0 * source[1] + 12.0 * source[2] - 8.0 * source[3] + 2.0 * source[4]) / 35.0;
                for (var index = 2; index < length - 2; ++index)
                    numArray[index] = (-3.0 * (source[index - 2] + source[index + 2]) +
                                       12.0 * (source[index - 1] + source[index + 1]) + 17.0 * source[index]) / 35.0;
                numArray[length - 2] = (2.0 * source[length - 5] - 8.0 * source[length - 4] +
                                        12.0 * source[length - 3] + 27.0 * source[length - 2] +
                                        2.0 * source[length - 1]) / 35.0;
                numArray[length - 1] = (-source[length - 5] + 4.0 * source[length - 4] - 6.0 * source[length - 3] +
                                        4.0 * source[length - 2] + 69.0 * source[length - 1]) / 70.0;
            }
            return numArray;
        }

        /// <summary>Seven point cubic smooth.</summary>
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
                numArray[0] = (39.0 * source[0] + 8.0 * source[1] - 4.0 * source[2] - 4.0 * source[3] + source[4] +
                               4.0 * source[5] - 2.0 * source[6]) / 42.0;
                numArray[1] = (8.0 * source[0] + 19.0 * source[1] + 16.0 * source[2] + 6.0 * source[3] -
                               4.0 * source[4] - 7.0 * source[5] + 4.0 * source[6]) / 42.0;
                numArray[2] = (-4.0 * source[0] + 16.0 * source[1] + 19.0 * source[2] + 12.0 * source[3] +
                               2.0 * source[4] - 4.0 * source[5] + source[6]) / 42.0;
                for (var index = 3; index <= length - 4; ++index)
                    numArray[index] = (-2.0 * (source[index - 3] + source[index + 3]) +
                                       3.0 * (source[index - 2] + source[index + 2]) +
                                       6.0 * (source[index - 1] + source[index + 1]) + 7.0 * source[index]) / 21.0;
                numArray[length - 3] = (-4.0 * source[length - 1] + 16.0 * source[length - 2] +
                                        19.0 * source[length - 3] + 12.0 * source[length - 4] +
                                        2.0 * source[length - 5] - 4.0 * source[length - 6] + source[length - 7]) /
                                       42.0;
                numArray[length - 2] = (8.0 * source[length - 1] + 19.0 * source[length - 2] +
                                        16.0 * source[length - 3] + 6.0 * source[length - 4] -
                                        4.0 * source[length - 5] - 7.0 * source[length - 6] +
                                        4.0 * source[length - 7]) / 42.0;
                numArray[length - 1] = (39.0 * source[length - 1] + 8.0 * source[length - 2] -
                                        4.0 * source[length - 3] - 4.0 * source[length - 4] + source[length - 5] +
                                        4.0 * source[length - 6] - 2.0 * source[length - 7]) / 42.0;
            }
            return numArray;
        }
    }
}