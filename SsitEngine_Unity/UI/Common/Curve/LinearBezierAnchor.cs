using System;
using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Curve
{
    /// <summary>Anchor points of linear bezier curve.</summary>
    [Serializable]
    public struct LinearBezierAnchor
    {
        /// <summary>Start point of curve.</summary>
        public Vector3 start;

        /// <summary>End point of curve.</summary>
        public Vector3 end;

        /// <summary>Constructor.</summary>
        /// <param name="start">Start point of curve.</param>
        /// <param name="end">End point of curve.</param>
        public LinearBezierAnchor( Vector3 start, Vector3 end )
        {
            this.start = start;
            this.end = end;
        }
    }
}