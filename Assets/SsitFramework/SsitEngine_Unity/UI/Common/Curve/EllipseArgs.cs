/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：椭圆参数                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 20:33:44                             
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Curve
{
    /// <summary>Args of ellipse.</summary>
    [Serializable]
    public struct EllipseArgs
    {
        /// <summary>Center of ellipse.</summary>
        public Vector3 center;

        /// <summary>Semi minor axis of ellipse.</summary>
        public float semiMinorAxis;

        /// <summary>Semi major axis of ellipse.</summary>
        public float semiMajorAxis;

        /// <summary>Constructor.</summary>
        /// <param name="center">Center of ellipse.</param>
        /// <param name="semiMinorAxis">Semi minor axis of ellipse.</param>
        /// <param name="semiMajorAxis">Semi major axis of ellipse.</param>
        public EllipseArgs( Vector3 center, float semiMinorAxis, float semiMajorAxis )
        {
            this.center = center;
            this.semiMinorAxis = semiMinorAxis;
            this.semiMajorAxis = semiMajorAxis;
        }
    }
}