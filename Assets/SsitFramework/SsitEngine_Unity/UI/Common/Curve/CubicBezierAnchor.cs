using System;
using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Curve
{
    /// <summary>Anchor points of cubic bezier curve.</summary>
    [Serializable]
    public struct CubicBezierAnchor
    {
        /// <summary>Start point of curve.</summary>
        public Vector3 start;

        /// <summary>End point of curve.</summary>
        public Vector3 end;

        /// <summary>Start tangent point of curve.</summary>
        public Vector3 startTangent;

        /// <summary>End tangent point of curve.</summary>
        public Vector3 endTangent;

        /// <summary>Constructor.</summary>
        /// <param name="start">Start point of curve.</param>
        /// <param name="end">End point of curve.</param>
        /// <param name="startTangent">Start tangent point of curve.</param>
        /// <param name="endTangent">End tangent point of curve.</param>
        public CubicBezierAnchor( Vector3 start, Vector3 end, Vector3 startTangent, Vector3 endTangent )
        {
            this.start = start;
            this.end = end;
            this.startTangent = startTangent;
            this.endTangent = endTangent;
        }
    }
}