using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Curve
{
    /// <summary>Quadratic bezier curve.</summary>
    public class QuadraticBezierCurve : BezierCurve
    {
        /// <summary>Anchor points of curve.</summary>
        public QuadraticBezierAnchor anchor;

        /// <summary>Constructor.</summary>
        public QuadraticBezierCurve()
        {
            anchor = new QuadraticBezierAnchor();
        }

        /// <summary>Constructor.</summary>
        /// <param name="anchor">Anchor points of curve.</param>
        public QuadraticBezierCurve( QuadraticBezierAnchor anchor )
        {
            this.anchor = anchor;
        }

        /// <summary>Get point on curve at key.</summary>
        /// <param name="key">Key is in the range(0~1).</param>
        /// <returns>The point on curve at key.</returns>
        public override Vector3 GetPointAt( float key )
        {
            return GetPointAt(anchor, key);
        }

        /// <summary>Get curve point base on anchor points and t.</summary>
        /// <param name="anchor">Anchor points of curve.</param>
        /// <param name="t">t is in the range(0~1).</param>
        /// <returns>Point on curve.</returns>
        public static Vector3 GetPointAt( QuadraticBezierAnchor anchor, float t )
        {
            return Mathf.Pow(1f - t, 2f) * anchor.start + (float) (2.0 * t * (1.0 - t)) * anchor.tangent +
                   Mathf.Pow(t, 2f) * anchor.end;
        }
    }
}