using System;
using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Generic
{
    /// <summary>Rectangle area on plane.</summary>
    [Serializable]
    public struct PlaneArea
    {
        /// <summary>Center of area.</summary>
        public Transform center;

        /// <summary>Width of area.</summary>
        public float width;

        /// <summary>Length of area.</summary>
        public float length;

        /// <summary>Constructor.</summary>
        /// <param name="center">Center of area.</param>
        /// <param name="width">Width of area.</param>
        /// <param name="length">Length of area.</param>
        public PlaneArea( Transform center, float width, float length )
        {
            this.center = center;
            this.width = width;
            this.length = length;
        }
    }
}