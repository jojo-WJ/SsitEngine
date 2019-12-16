using System;
using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Curve
{
    /// <summary>Vector keyframe.</summary>
    [Serializable]
    public struct VectorKeyFrame
    {
        /// <summary>Key of keyframe.</summary>
        public float key;

        /// <summary>Value of keyframe.</summary>
        public Vector3 value;

        /// <summary>Constructor.</summary>
        /// <param name="key">Key of keyframe.</param>
        /// <param name="value">Value of keyframe.</param>
        public VectorKeyFrame( float key, Vector3 value )
        {
            this.key = key;
            this.value = value;
        }
    }
}