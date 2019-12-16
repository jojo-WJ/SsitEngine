using System;
using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Curve
{
    /// <summary>Sin curve.</summary>
    public class SinCurve : ICurve
    {
        /// <summary>Coefficient of delta to lerp key.</summary>
        protected const float Coefficient = 0.05f;

        /// <summary>Args of sin curve.</summary>
        public SinArgs args;

        /// <summary>Constructor.</summary>
        public SinCurve()
        {
            args = new SinArgs();
        }

        /// <summary>Constructor.</summary>
        /// <param name="args">Args of sin curve.</param>
        public SinCurve( SinArgs args )
        {
            this.args = args;
        }

        /// <summary>Length of sin curve.</summary>
        public float Length
        {
            get
            {
                var num1 = 0.0f;
                var num2 = MaxKey * 0.05f;
                for (var x = 0.0f; (double) x < (double) MaxKey; x += num2)
                    num1 += Vector3.Distance(GetPointAt(x), GetPointAt(x + num2));
                return num1;
            }
        }

        /// <summary>Max key of sin curve.</summary>
        public virtual float MaxKey { set; get; }

        /// <summary>Get point on sin curve at x.</summary>
        /// <param name="x">Value of x axis.</param>
        /// <returns>The point on sin curve at x.</returns>
        public virtual Vector3 GetPointAt( float x )
        {
            return GetPointAt(args, x);
        }

        /// <summary>Evaluate the value of sin curve at x.</summary>
        /// <param name="args">Args of sin curve.</param>
        /// <param name="x">Value of x axis.</param>
        /// <returns>The value of sin curve at x.</returns>
        public static float Evaluate( SinArgs args, double x )
        {
            return args.amplitude * (float) Math.Sin(args.angular * x + args.phase) + args.setover;
        }

        /// <summary>Get point on sin curve at x.</summary>
        /// <param name="args">Args of sin curve.</param>
        /// <param name="x">Value of x axis.</param>
        /// <returns>The point on sin curve at x.</returns>
        public static Vector3 GetPointAt( SinArgs args, float x )
        {
            return new Vector3(x, Evaluate(args, x));
        }
    }
}