/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：平面测量                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 20:22:57                             
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace SsitEngine.Mathematics
{
    /// <summary>Planimetry.</summary>
    public static class Planimetry
    {
        /// <summary>Gets the distance from line L1 to line L2.</summary>
        /// <param name="L1">line L1.</param>
        /// <param name="L2">line L2.</param>
        /// <returns>The distance from line L1 to line L2.</returns>
        public static double GetDistance( Line L1, Line L2 )
        {
            var num = 0.0;
            if (L1.k == L2.k)
            {
                num = Math.Abs(L2.b - L1.b);
                if (L1.k != 0.0 && L1.k != double.PositiveInfinity)
                {
                    num /= Math.Sqrt(1.0 + Math.Pow(L1.k, 2.0));
                }
            }
            return num;
        }

        /// <summary>Gets the distance from vector to line.</summary>
        /// <param name="v">Vector.</param>
        /// <param name="L">Line.</param>
        /// <returns>The distance from vector to line.</returns>
        public static double GetDistance( Vector v, Line L )
        {
            return L.k != 0.0
                ? L.k != double.PositiveInfinity
                    ? Math.Abs(L.k * v.x - v.y + L.b) / Math.Sqrt(1.0 + Math.Pow(L.k, 2.0))
                    : Math.Abs(v.x - L.b)
                : Math.Abs(v.y - L.b);
        }

        /// <summary>Get relation of two circles.</summary>
        /// <param name="c1">Circle c1.</param>
        /// <param name="c2">Circle c2.</param>
        /// <returns>Relation of two circles.</returns>
        public static Relation GetRelation( Circle c1, Circle c2 )
        {
            var num1 = Vector.Distance(c1.c, c2.c);
            var num2 = c1.r + c2.r;
            var num3 = Math.Abs(c1.r - c2.r);
            return num1 <= num2
                ? num1 != num2 ? num1 <= num3 ? num1 != num3 ? Relation.Internal :
                num3 != 0.0 ? Relation.InsideTangent : Relation.Coincidence : Relation.Intersect :
                Relation.OutsideTangent
                : Relation.External;
        }

        /// <summary>Get relation of circle and line.</summary>
        /// <param name="c">Circle.</param>
        /// <param name="L">Line.</param>
        /// <returns>Relation of circle and line.</returns>
        public static Relation GetRelation( Circle c, Line L )
        {
            var distance = GetDistance(c.c, L);
            return distance <= c.r ? distance != c.r ? Relation.Intersect : Relation.OutsideTangent : Relation.External;
        }

        /// <summary>Get relation of circle and vector.</summary>
        /// <param name="c">Circle.</param>
        /// <param name="v">Vector.</param>
        /// <returns>Relation of circle and vector.</returns>
        public static Relation GetRelation( Circle c, Vector v )
        {
            var num = Vector.Distance(c.c, v);
            return num <= c.r ? num != c.r ? Relation.Internal : Relation.Coincidence : Relation.External;
        }

        /// <summary>Get relation of two lines.</summary>
        /// <param name="L1">Line L1.</param>
        /// <param name="L2">Line L2.</param>
        /// <returns>Relation of two lines.</returns>
        public static Relation GetRelation( Line L1, Line L2 )
        {
            return L1.k != L2.k ? Relation.Intersect : L1.b != L2.b ? Relation.Parallel : Relation.Coincidence;
        }

        /// <summary>Get relation of line and vector.</summary>
        /// <param name="L">Line.</param>
        /// <param name="v">Vector.</param>
        /// <returns>Relation of line and vector.</returns>
        public static Relation GetRelation( Line L, Vector v )
        {
            return L.k != double.PositiveInfinity ? v.y != L.k * v.x + L.b ? Relation.External :
                Relation.Coincidence :
                v.x != L.b ? Relation.External : Relation.Coincidence;
        }

        /// <summary>Get intersections of two circles.</summary>
        /// <param name="c1">Circle c1.</param>
        /// <param name="c2">Circle c2.</param>
        /// <returns>Intersections of two circles.</returns>
        public static List<Vector> GetIntersections( Circle c1, Circle c2 )
        {
            switch (GetRelation(c1, c2))
            {
                case Relation.Intersect:
                case Relation.OutsideTangent:
                case Relation.InsideTangent:
                    var num1 = c2.c.x - c1.c.x;
                    var num2 = c2.c.y - c1.c.y;
                    var num3 = Math.Pow(c2.c.x, 2.0) + Math.Pow(c2.c.y, 2.0) + Math.Pow(c1.r, 2.0) -
                               Math.Pow(c1.c.x, 2.0) - Math.Pow(c1.c.y, 2.0) - Math.Pow(c2.r, 2.0);
                    double k;
                    double b;
                    if (num2 == 0.0)
                    {
                        k = double.PositiveInfinity;
                        b = num3 / (2.0 * num1);
                    }
                    else
                    {
                        k = -num1 / num2;
                        b = num3 / (2.0 * num2);
                    }
                    return GetIntersections(c1, new Line(k, b));
                default:
                    return null;
            }
        }

        /// <summary>Get intersections of circle and line.</summary>
        /// <param name="C">Circle.</param>
        /// <param name="L">Line.</param>
        /// <returns>Intersections of circle and line.</returns>
        public static List<Vector> GetIntersections( Circle C, Line L )
        {
            var relation = GetRelation(C, L);
            switch (relation)
            {
                case Relation.Intersect:
                case Relation.OutsideTangent:
                    var vectorList = new List<Vector>();
                    if (L.k == double.PositiveInfinity)
                    {
                        var b = L.b;
                        var num = Math.Sqrt(Math.Pow(C.r, 2.0) - Math.Pow(b - C.c.x, 2.0));
                        var y1 = num + C.c.y;
                        vectorList.Add(new Vector(b, y1));
                        if (relation == Relation.Intersect)
                        {
                            var x = b;
                            var y2 = -num + C.c.y;
                            vectorList.Add(new Vector(x, y2));
                        }
                    }
                    else
                    {
                        var num1 = 1.0 + Math.Pow(L.k, 2.0);
                        var x1 = 2.0 * (L.k * (L.b - C.c.y) - C.c.x);
                        var num2 = Math.Pow(C.c.x, 2.0) + Math.Pow(L.b - C.c.y, 2.0) - Math.Pow(C.r, 2.0);
                        var d = Math.Pow(x1, 2.0) - 4.0 * num1 * num2;
                        var x2 = (-x1 + Math.Sqrt(d)) / (2.0 * num1);
                        var y1 = L.k * x2 + L.b;
                        vectorList.Add(new Vector(x2, y1));
                        if (relation == Relation.Intersect)
                        {
                            var x3 = (-x1 - Math.Sqrt(d)) / (2.0 * num1);
                            var y2 = L.k * x3 + L.b;
                            vectorList.Add(new Vector(x3, y2));
                        }
                    }
                    return vectorList;
                default:
                    return null;
            }
        }

        /// <summary>Get intersection of two lines.</summary>
        /// <param name="L1">Line L1.</param>
        /// <param name="L2">Line L2.</param>
        /// <returns>Intersection of two lines.</returns>
        public static List<Vector> GetIntersections( Line L1, Line L2 )
        {
            if (L1.k == L2.k)
            {
                return null;
            }
            double x;
            double y;
            if (L1.k == double.PositiveInfinity)
            {
                x = L1.b;
                y = L2.k * x + L2.b;
            }
            else if (L2.k == double.PositiveInfinity)
            {
                x = L2.b;
                y = L1.k * x + L1.b;
            }
            else
            {
                x = (L1.b - L2.b) / (L2.k - L1.k);
                y = L1.k * x + L1.b;
            }
            return new List<Vector> {new Vector(x, y)};
        }
    }
}