using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Curve
{
    /// <summary>Cubic bezier curve.</summary>
    public class CubicBezierCurve : BezierCurve
    {
        /// <summary>Anchor points of curve.</summary>
        public CubicBezierAnchor anchor;

        /// <summary>Constructor.</summary>
        public CubicBezierCurve()
        {
            anchor = new CubicBezierAnchor();
        }

        /// <summary>Constructor.</summary>
        /// <param name="anchor">Anchor points of curve.</param>
        public CubicBezierCurve( CubicBezierAnchor anchor )
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

        /// <summary>Get curve point base on anchor points and key.</summary>
        /// <param name="anchor">Anchor points of curve.</param>
        /// <param name="key">Key is in the range(0~1).</param>
        /// <returns>Point on curve.</returns>
        public static Vector3 GetPointAt( CubicBezierAnchor anchor, float key )
        {
            return Mathf.Pow(1f - key, 3f) * anchor.start + 3f * key * Mathf.Pow(1f - key, 2f) * anchor.startTangent +
                   (float) (3.0 * (1.0 - key)) * Mathf.Pow(key, 2f) * anchor.endTangent +
                   Mathf.Pow(key, 3f) * anchor.end;
        }
    }
}