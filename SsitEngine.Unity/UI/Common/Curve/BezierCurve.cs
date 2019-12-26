/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 20:30:58                     
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Curve
{
    /// <summary>Bezier curve.</summary>
    public abstract class BezierCurve : ICurve
    {
        /// <summary>Delta to lerp key.</summary>
        protected const float Delta = 0.05f;

        /// <summary>Length of curve.</summary>
        public float Length
        {
            get
            {
                var num = 0.0f;
                for (var key = 0.0f; key < (double) MaxKey; key += 0.05f)
                {
                    num += Vector3.Distance(GetPointAt(key), GetPointAt(key + 0.05f));
                }
                return num;
            }
        }

        /// <summary>Max key of curve.</summary>
        public float MaxKey => 1f;

        /// <summary>Get point on curve at key.</summary>
        /// <param name="key">Key is in the range(0~1).</param>
        /// <returns>The point on curve at key.</returns>
        public abstract Vector3 GetPointAt( float key );
    }
}