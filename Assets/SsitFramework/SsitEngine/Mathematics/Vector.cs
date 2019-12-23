/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：数学类                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 20:01:19                             
*└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace SsitEngine.Mathematics
{
    /// <summary>Vector in plane rectangular coordinate system.</summary>
    [Serializable]
    public struct Vector
    {
        /// <summary>X of vector.</summary>
        public double x;

        /// <summary>Y of vector.</summary>
        public double y;

        /// <summary>Origin(0,0) of plane rectangular coordinate system.</summary>
        public static Vector Zero => new Vector();

        /// <summary>Vector(1,1) in plane rectangular coordinate system.</summary>
        public static Vector One => new Vector(1.0, 1.0);

        /// <summary>Constructor.</summary>
        /// <param name="x">X of vector.</param>
        /// <param name="y">Y of vector.</param>
        public Vector( double x, double y )
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>Equals?</summary>
        /// <param name="obj">Target obj.</param>
        /// <returns>Equals?</returns>
        public override bool Equals( object obj )
        {
            return base.Equals(obj);
        }

        /// <summary>Get hash code.</summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>To string.</summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            return string.Format("({0}, {1})", x, y);
        }

        /// <summary>Center of vector v1 and v2.</summary>
        /// <param name="v1">Vector v1.</param>
        /// <param name="v2">Vector v2.</param>
        /// <returns>The center of vector v1 and v2.</returns>
        public static Vector Center( Vector v1, Vector v2 )
        {
            return (v1 + v2) * 0.5;
        }

        /// <summary>Distance from vector v1 to v2.</summary>
        /// <param name="v1">Vector v1.</param>
        /// <param name="v2">Vector v2.</param>
        /// <returns>Distance from vector v1 to v2.</returns>
        public static double Distance( Vector v1, Vector v2 )
        {
            return Math.Sqrt(Math.Pow(v2.x - v1.x, 2.0) + Math.Pow(v2.y - v1.y, 2.0));
        }

        /// <summary>Operator +</summary>
        /// <param name="lhs">Vector1.</param>
        /// <param name="rhs">Vector2.</param>
        /// <returns>lhs+rhs</returns>
        public static Vector operator +( Vector lhs, Vector rhs )
        {
            return new Vector(lhs.x + rhs.x, lhs.y + rhs.y);
        }

        /// <summary>Operator -</summary>
        /// <param name="lhs">Vector1.</param>
        /// <param name="rhs">Vector2.</param>
        /// <returns>lhs-rhs</returns>
        public static Vector operator -( Vector lhs, Vector rhs )
        {
            return new Vector(lhs.x - rhs.x, lhs.y - rhs.y);
        }

        /// <summary>Operator -</summary>
        /// <param name="p">Vector</param>
        /// <returns>-Vector</returns>
        public static Vector operator -( Vector p )
        {
            return new Vector(-p.x, -p.y);
        }

        /// <summary>Operator *</summary>
        /// <param name="lhs">Vector.</param>
        /// <param name="rhs">double.</param>
        /// <returns>lhs*rhs</returns>
        public static Vector operator *( Vector lhs, double rhs )
        {
            return new Vector(lhs.x * rhs, lhs.y * rhs);
        }

        /// <summary>Operator *</summary>
        /// <param name="lhs">double.</param>
        /// <param name="rhs">Vector.</param>
        /// <returns>lhs*rhs</returns>
        public static Vector operator *( double lhs, Vector rhs )
        {
            return rhs * lhs;
        }

        /// <summary>Operator ==</summary>
        /// <param name="lhs">Vector1.</param>
        /// <param name="rhs">Vector2.</param>
        /// <returns>lhs==rhs?</returns>
        public static bool operator ==( Vector lhs, Vector rhs )
        {
            if (lhs.x == rhs.x)
            {
                return lhs.y == rhs.y;
            }
            return false;
        }

        /// <summary>Operator !=</summary>
        /// <param name="lhs">Vector1.</param>
        /// <param name="rhs">Vector2.</param>
        /// <returns>lhs!=rhs?</returns>
        public static bool operator !=( Vector lhs, Vector rhs )
        {
            return !(lhs == rhs);
        }
    }
}