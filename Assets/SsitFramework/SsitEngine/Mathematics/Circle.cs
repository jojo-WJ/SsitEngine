/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：圆                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 19:59:47                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace SsitEngine.Mathematics
{
    /// <summary>Circle in plane rectangular coordinate system.</summary>
    [Serializable]
    public struct Circle
    {
        /// <summary>Center.</summary>
        public Vector c;

        /// <summary>Radius.</summary>
        public double r;

        /// <summary>Unit circle.</summary>
        public static Circle Unit => new Circle(Vector.Zero, 1.0);

        /// <summary>Constructor.</summary>
        /// <param name="c">Center.</param>
        /// <param name="r">Radius.</param>
        public Circle( Vector c, double r )
        {
            this.c = c;
            this.r = r;
        }
    }
}