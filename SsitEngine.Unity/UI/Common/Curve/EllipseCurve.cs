using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Curve
{
    /// <summary>Ellipse curve椭圆曲线.</summary>
    public class EllipseCurve : ICurve
    {
        /// <summary>Args of ellipse curve.</summary>
        public EllipseArgs args;

        /// <summary>Constructor.</summary>
        public EllipseCurve()
        {
            args = new EllipseArgs();
        }

        /// <summary>Constructor.</summary>
        /// <param name="args">Args of ellipse curve.</param>
        public EllipseCurve( EllipseArgs args )
        {
            this.args = args;
        }

        /// <summary>Length of curve.</summary>
        public float Length
        {
            get
            {
                var num1 = Mathf.Min(args.semiMinorAxis, args.semiMajorAxis);
                var num2 = Mathf.Max(args.semiMinorAxis, args.semiMajorAxis);
                return (float) (6.28318548202515 * num1 + 4.0 * (num2 - (double) num1));
            }
        }

        /// <summary>Max around radian of ellipse.</summary>
        public virtual float MaxKey => 6.283185f;

        /// <summary>Get point on ellipse at around radian.</summary>
        /// <param name="radian">Around radian of ellipse.</param>
        /// <returns>The point on ellipse at around radian.</returns>
        public virtual Vector3 GetPointAt( float radian )
        {
            return GetPointAt(args, radian);
        }

        /// <summary>Get point on ellipse at around radian.</summary>
        /// <param name="args">Args of ellipse curve.</param>
        /// <param name="radian">Around radian of ellipse.</param>
        /// <returns>The point on ellipse at around radian.</returns>
        public static Vector3 GetPointAt( EllipseArgs args, float radian )
        {
            return args.center + new Vector3(args.semiMinorAxis * Mathf.Cos(radian), 0.0f,
                       args.semiMajorAxis * Mathf.Sin(radian));
        }
    }
}