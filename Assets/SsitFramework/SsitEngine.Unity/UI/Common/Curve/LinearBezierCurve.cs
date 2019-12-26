using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Curve
{
    /// <summary>Linear bezier curve.</summary>
    public class LinearBezierCurve : BezierCurve
    {
        /// <summary>Anchor points of curve.</summary>
        public LinearBezierAnchor anchor;

        /// <summary>Constructor.</summary>
        public LinearBezierCurve()
        {
            anchor = new LinearBezierAnchor();
        }

        /// <summary>Constructor.</summary>
        /// <param name="anchor">Anchor points of curve.</param>
        public LinearBezierCurve( LinearBezierAnchor anchor )
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
        public static Vector3 GetPointAt( LinearBezierAnchor anchor, float key )
        {
            return (1f - key) * anchor.start + key * anchor.end;
        }
    }
}