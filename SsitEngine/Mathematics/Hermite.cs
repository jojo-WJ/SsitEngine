/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：埃尔米特插值公式                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 20:12:20                             
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Mathematics
{
    /// <summary>Hermite interpolation.</summary>
    /// <remarks>
    ///     埃尔米特插值公式人从甲地开车去乙地，每隔一段时间对行车距离和速率进行一次采样，得到在 n+1 个采样时刻点 ti的里程 si和速率 vi（i=0,1,...,n）。
    ///     构造埃尔米特插值多项式 H2n+1(t)
    /// </remarks>
    /// >
    public static class Hermite
    {
        /// <summary>Evaluate the value of hermite curve at time.</summary>
        /// <param name="t0">Time of start key frame.</param>
        /// <param name="t1">Time of end key frame.</param>
        /// <param name="v0">Value of start key frame.</param>
        /// <param name="v1">Value of end key frame.</param>
        /// <param name="m0">Micro quotient value of start key frame.</param>
        /// <param name="m1">Micro quotient value of end key frame.</param>
        /// <param name="t">Time of curve to evaluate value.</param>
        /// <returns>The value of hermite curve at time.</returns>
        public static double Evaluate(
            double t0,
            double t1,
            double v0,
            double v1,
            double m0,
            double m1,
            double t )
        {
            var num1 = t - t0;
            var num2 = t - t1;
            var num3 = t0 - t1;
            var num4 = num1 / num3;
            var num5 = num2 / num3;
            var num6 = num5 * num5;
            var num7 = num4 * num4;
            return (1.0 - 2.0 * num4) * v0 * num6 + (1.0 + 2.0 * num5) * v1 * num7 + m0 * num1 * num6 +
                   m1 * num2 * num7;
        }
    }
}