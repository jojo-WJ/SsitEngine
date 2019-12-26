using System;
using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Curve
{
    /// <summary>Anchor points of quadratic bezier curve.</summary>
    [Serializable]
    public struct QuadraticBezierAnchor
    {
        /// <summary>Start point of curve.</summary>
        public Vector3 start;

        /// <summary>End point of curve.</summary>
        public Vector3 end;

        /// <summary>Tangent point of curve.</summary>
        public Vector3 tangent;

        /// <summary>Constructor.</summary>
        /// <param name="start">Start point of curve.</param>
        /// <param name="end">End point of curve.</param>
        /// <param name="tangent">Tangent point of curve.</param>
        public QuadraticBezierAnchor( Vector3 start, Vector3 end, Vector3 tangent )
        {
            this.start = start;
            this.end = end;
            this.tangent = tangent;
        }
    }
}