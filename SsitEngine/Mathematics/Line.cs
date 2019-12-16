/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：线
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 20:19:40                             
*└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace SsitEngine.Mathematics
{
    /// <summary>Line in plane rectangular coordinate system.</summary>
    [Serializable]
    public struct Line
    {
        /// <summary>Slope of line.</summary>
        public double k;

        /// <summary>Intercept of line.</summary>
        public double b;

        /// <summary>Horizontal line (x axis).</summary>
        public static Line Horizontal => new Line(0.0, 0.0);

        /// <summary>Vertical line (y axis).</summary>
        public static Line Vertical => new Line(double.PositiveInfinity, 0.0);

        /// <summary>Constructor.</summary>
        /// <param name="k">Slope of line.</param>
        /// <param name="b">Intercept of line.</param>
        public Line( double k, double b )
        {
            this.k = k;
            this.b = b;
        }

        /// <summary>Get the line that pass vector v1 and v2.</summary>
        /// <param name="v1">Vector p1.</param>
        /// <param name="v2">Vector p2.</param>
        /// <returns>The line that pass vector v1 and v2.</returns>
        public static Line FromPoints( Vector v1, Vector v2 )
        {
            var num1 = v2.x - v1.x;
            var num2 = v2.y - v1.y;
            double k;
            double b;
            if (num1 == 0.0)
            {
                k = double.PositiveInfinity;
                b = v1.x;
            }
            else
            {
                k = num2 / num1;
                b = v1.y - k * v1.x;
            }

            return new Line(k, b);
        }
    }
}