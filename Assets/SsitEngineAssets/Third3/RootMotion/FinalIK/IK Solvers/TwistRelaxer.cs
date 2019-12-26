using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// Relaxes the twist rotation of this Transform relative to a parent and a child Transform, using their initial rotations as the most relaxed pose.
    /// </summary>
    public class TwistRelaxer : MonoBehaviour
    {
        private Vector3 axis = Vector3.forward;
        private Vector3 axisRelativeToParentDefault, axisRelativeToChildDefault;
        private Transform child;

        public IK ik;
        private Transform parent;

        [Tooltip(
            "If 0.5, this Transform will be twisted half way from parent to child. If 1, the twist angle will be locked to the child and will rotate with along with it.")]
        [Range(0f, 1f)]
        public float parentChildCrossfade = 0.5f;

        [Tooltip("Rotation offset around the twist axis.")] [Range(-180f, 180f)]
        public float twistAngleOffset;

        private Vector3 twistAxis = Vector3.right;

        [Tooltip("The weight of relaxing the twist of this Transform")] [Range(0f, 1f)]
        public float weight = 1f;

        /// <summary>
        /// Rotate this Transform to relax it's twist angle relative to the "parent" and "child" Transforms.
        /// </summary>
        public void Relax()
        {
            if (weight <= 0f) return; // Nothing to do here

            var rotation = transform.rotation;
            var twistOffset = Quaternion.AngleAxis(twistAngleOffset, rotation * twistAxis);
            rotation = twistOffset * rotation;

            // Find the world space relaxed axes of the parent and child
            var relaxedAxisParent = twistOffset * parent.rotation * axisRelativeToParentDefault;
            var relaxedAxisChild = twistOffset * child.rotation * axisRelativeToChildDefault;

            // Cross-fade between the parent and child
            var relaxedAxis = Vector3.Slerp(relaxedAxisParent, relaxedAxisChild, parentChildCrossfade);

            // Convert relaxedAxis to (axis, twistAxis) space so we could calculate the twist angle
            var r = Quaternion.LookRotation(rotation * axis, rotation * twistAxis);
            relaxedAxis = Quaternion.Inverse(r) * relaxedAxis;

            // Calculate the angle by which we need to rotate this Transform around the twist axis.
            var angle = Mathf.Atan2(relaxedAxis.x, relaxedAxis.z) * Mathf.Rad2Deg;

            // Store the rotation of the child so it would not change with twisting this Transform
            var childRotation = child.rotation;

            // Twist the bone
            transform.rotation = Quaternion.AngleAxis(angle * weight, rotation * twistAxis) * rotation;

            // Revert the rotation of the child
            child.rotation = childRotation;
        }

        private void Start()
        {
            parent = transform.parent;

            if (transform.childCount == 0)
            {
                var children = parent.GetComponentsInChildren<Transform>();
                for (var i = 1; i < children.Length; i++)
                    if (children[i] != transform)
                    {
                        child = children[i];
                        break;
                    }
            }
            else
            {
                child = transform.GetChild(0);
            }

            twistAxis = transform.InverseTransformDirection(child.position - transform.position);
            axis = new Vector3(twistAxis.y, twistAxis.z, twistAxis.x);

            // Axis in world space
            var axisWorld = transform.rotation * axis;

            // Store the axis in worldspace relative to the rotations of the parent and child
            axisRelativeToParentDefault = Quaternion.Inverse(parent.rotation) * axisWorld;
            axisRelativeToChildDefault = Quaternion.Inverse(child.rotation) * axisWorld;

            if (ik != null) ik.GetIKSolver().OnPostUpdate += OnPostUpdate;
        }

        private void OnPostUpdate()
        {
            if (ik != null) Relax();
        }

        private void LateUpdate()
        {
            if (ik == null) Relax();
        }

        private void OnDestroy()
        {
            if (ik != null) ik.GetIKSolver().OnPostUpdate -= OnPostUpdate;
        }
    }
}