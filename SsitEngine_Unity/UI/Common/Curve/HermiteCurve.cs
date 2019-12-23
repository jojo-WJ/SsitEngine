using SsitEngine.DebugLog;
using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Curve
{
    /// <summary>Hermite curve in three dimensional space赫米特曲线.</summary>
    public class HermiteCurve : ICurve
    {
        /// <summary>Coefficient of delta to lerp key.</summary>
        protected const float Coefficient = 0.05f;

        /// <summary>Curve for x.</summary>
        protected AnimationCurve xCurve;

        /// <summary>Curve for y.</summary>
        protected AnimationCurve yCurve;

        /// <summary>Curve for z.</summary>
        protected AnimationCurve zCurve;

        /// <summary>Constructor.</summary>
        public HermiteCurve()
        {
            xCurve = new AnimationCurve();
            yCurve = new AnimationCurve();
            zCurve = new AnimationCurve();
        }

        /// <summary>Get the index key frame.</summary>
        /// <param name="index">Index of key frame.</param>
        /// <returns>Key frame at index.</returns>
        public VectorKeyFrame this[ int index ]
        {
            get
            {
                var keyframe = xCurve[index];
                double time = keyframe.time;
                keyframe = xCurve[index];
                double num1 = keyframe.value;
                keyframe = yCurve[index];
                double num2 = keyframe.value;
                keyframe = zCurve[index];
                double num3 = keyframe.value;
                var vector3 = new Vector3((float) num1, (float) num2, (float) num3);
                return new VectorKeyFrame((float) time, vector3);
            }
        }

        /// <summary>Count of Keyframes.</summary>
        public int KeyframeCount => xCurve.length;

        /// <summary>The behaviour of the animation after the last keyframe.</summary>
        public WrapMode PostWrapMode
        {
            set => xCurve.postWrapMode = yCurve.postWrapMode = zCurve.postWrapMode = value;
            get => xCurve.postWrapMode;
        }

        /// <summary>
        ///     The behaviour of the animation before the first keyframe.
        /// </summary>
        public WrapMode PreWrapMode
        {
            set => xCurve.preWrapMode = yCurve.preWrapMode = zCurve.preWrapMode = value;
            get => xCurve.preWrapMode;
        }

        /// <summary>Length of curve.</summary>
        public float Length
        {
            get
            {
                var num1 = 0.0f;
                var num2 = MaxKey * 0.05f;
                for (var key = 0.0f; key < (double) MaxKey; key += num2)
                {
                    num1 += Vector3.Distance(GetPointAt(key), GetPointAt(key + num2));
                }
                return num1;
            }
        }

        /// <summary>Max key of curve.</summary>
        public float MaxKey
        {
            get
            {
                if (KeyframeCount == 0)
                {
                    return 0.0f;
                }
                return xCurve[KeyframeCount - 1].time;
            }
        }

        /// <summary>Get point by evaluate the curve at key.</summary>
        /// <param name="key">The key within the curve you want to evaluate.</param>
        /// <returns>The point on the curve at the key.</returns>
        public Vector3 GetPointAt( float key )
        {
            return new Vector3(xCurve.Evaluate(key), yCurve.Evaluate(key), zCurve.Evaluate(key));
        }

        /// <summary>Add a new keyframe to the curve.</summary>
        /// <param name="keyframe">The keyframe to add to the curve.</param>
        /// <returns>The index of the added keyframe, or -1 if the keyframe could not be added.</returns>
        public int AddKeyframe( VectorKeyFrame keyframe )
        {
            xCurve.AddKey(keyframe.key, keyframe.value.x);
            yCurve.AddKey(keyframe.key, keyframe.value.y);
            return zCurve.AddKey(keyframe.key, keyframe.value.z);
        }

        /// <summary>Add a new keyframe to the curve.</summary>
        /// <param name="key">The key of the keyframe.</param>
        /// <param name="value">The value of the keyframe.</param>
        /// <returns>The index of the added keyframe, or -1 if the keyframe could not be added.</returns>
        public int AddKeyframe( float key, Vector3 value )
        {
            xCurve.AddKey(key, value.x);
            yCurve.AddKey(key, value.y);
            return zCurve.AddKey(key, value.z);
        }

        /// <summary>Removes a keyframe.</summary>
        /// <param name="index">The index of the keyframe to remove.</param>
        public void RemoveKeyframe( int index )
        {
            xCurve.RemoveKey(index);
            yCurve.RemoveKey(index);
            zCurve.RemoveKey(index);
        }

        /// <summary>
        ///     Smooth the in and out tangents of the keyframe at index.
        /// </summary>
        /// <param name="index">The index of the keyframe.</param>
        /// <param name="weight">The smoothing weight to apply to the keyframe's tangents.</param>
        public void SmoothTangents( int index, float weight )
        {
            xCurve.SmoothTangents(index, weight);
            yCurve.SmoothTangents(index, weight);
            zCurve.SmoothTangents(index, weight);
        }

        /// <summary>Smooth the in and out tangents of keyframes.</summary>
        /// <param name="weight">The smoothing weight to apply to the keyframe's tangents.</param>
        public void SmoothTangents( float weight )
        {
            for (var index = 0; index < KeyframeCount; ++index)
            {
                SmoothTangents(index, weight);
            }
        }

        /// <summary>Create a curve base on anchors.</summary>
        /// <param name="anchors">Anchor points of curve.</param>
        /// <param name="close">Curve is close?</param>
        /// <returns>New curve.</returns>
        public static HermiteCurve FromAnchors( Vector3[] anchors, bool close = false )
        {
            var hermiteCurve = new HermiteCurve();
            if (anchors == null || anchors.Length == 0)
            {
                SsitDebug.Warning("Created a curve with no key frame: The anchors is null or empty.");
            }
            else
            {
                var key1 = 0.0f;
                for (var index = 0; index < anchors.Length - 1; ++index)
                {
                    hermiteCurve.AddKeyframe(key1, anchors[index]);
                    key1 += Vector3.Distance(anchors[index], anchors[index + 1]);
                }
                hermiteCurve.AddKeyframe(key1, anchors[anchors.Length - 1]);
                if (close)
                {
                    var key2 = key1 + Vector3.Distance(anchors[anchors.Length - 1], anchors[0]);
                    hermiteCurve.AddKeyframe(key2, anchors[0]);
                }
                hermiteCurve.SmoothTangents(0.0f);
            }
            return hermiteCurve;
        }
    }
}