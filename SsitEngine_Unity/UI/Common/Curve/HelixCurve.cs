using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Curve
{
    /// <summary>Helix curve螺线线.</summary>
    public class HelixCurve : ICurve
    {
        /// <summary>Coefficient of delta to lerp key.</summary>
        protected const float Coefficient = 0.05f;

        /// <summary>Bottom ellipse args of curve.</summary>
        public EllipseArgs bottomEllipse;

        /// <summary>Top ellipse args of curve.</summary>
        public EllipseArgs topEllipse;

        /// <summary>Constructor.</summary>
        public HelixCurve()
        {
            topEllipse = new EllipseArgs();
            bottomEllipse = new EllipseArgs();
        }

        /// <summary>Constructor.</summary>
        /// <param name="topEllipse">Top ellipse args of curve.</param>
        /// <param name="bottomEllipse">Bottom ellipse args of curve.</param>
        public HelixCurve( EllipseArgs topEllipse, EllipseArgs bottomEllipse )
        {
            this.topEllipse = topEllipse;
            this.bottomEllipse = bottomEllipse;
        }

        /// <summary>Length of curve.</summary>
        public float Length
        {
            get
            {
                var num1 = 0.0f;
                var num2 = MaxKey * 0.05f;
                for (var radian = 0.0f; (double) radian < (double) MaxKey; radian += num2)
                    num1 += Vector3.Distance(GetPointAt(radian), GetPointAt(radian + num2));
                return num1;
            }
        }

        /// <summary>Max around radian of helix.</summary>
        public float MaxKey { set; get; }

        /// <summary>Get point on helix at around radian.</summary>
        /// <param name="radian">Around radian of helix.</param>
        /// <returns>The point on helix at around radian.</returns>
        public Vector3 GetPointAt( float radian )
        {
            return GetPointAt(topEllipse, bottomEllipse, MaxKey, radian);
        }

        /// <summary>Get point on helix at around radian.</summary>
        /// <param name="topEllipse">Top ellipse args of curve.</param>
        /// <param name="bottomEllipse">Bottom ellipse args of curve.</param>
        /// <param name="maxRadian">Max around radian of helix.</param>
        /// <param name="radian">Around radian of helix.</param>
        /// <returns>The point on helix at around radian.</returns>
        public static Vector3 GetPointAt(
            EllipseArgs topEllipse,
            EllipseArgs bottomEllipse,
            float maxRadian,
            float radian )
        {
            if (maxRadian == 0.0)
                maxRadian = Mathf.Epsilon;
            return Vector3.Lerp(EllipseCurve.GetPointAt(bottomEllipse, radian),
                EllipseCurve.GetPointAt(topEllipse, radian), radian / maxRadian);
        }
    }
}