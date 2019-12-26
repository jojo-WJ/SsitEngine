using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Curve
{
    /// <summary>Interface of curve.</summary>
    public interface ICurve
    {
        /// <summary>Length of curve.</summary>
        float Length { get; }

        /// <summary>Max key of curve.</summary>
        float MaxKey { get; }

        /// <summary>Get point on curve at key.</summary>
        /// <param name="key">Key of curve.</param>
        /// <returns>The point on curve at key.</returns>
        Vector3 GetPointAt( float key );
    }
}