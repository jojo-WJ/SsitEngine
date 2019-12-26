using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// Using a spline to limit the range of rotation on universal and ball-and-socket joints. 
    /// Reachable area is defined by an AnimationCurve orthogonally mapped onto a sphere.
    /// </summary>
    [HelpURL("http://www.root-motion.com/finalikdox/html/page12.html")]
    [AddComponentMenu("Scripts/RootMotion.FinalIK/Rotation Limits/Rotation Limit Spline")]
    public class RotationLimitSpline : RotationLimit
    {
        // Open the User Manual URL
        [ContextMenu("User Manual")]
        private void OpenUserManual()
        {
            Application.OpenURL("http://www.root-motion.com/finalikdox/html/page12.html");
        }

        // Open the Script Reference URL
        [ContextMenu("Scrpt Reference")]
        private void OpenScriptReference()
        {
            Application.OpenURL(
                "http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_rotation_limit_spline.html");
        }

        // Link to the Final IK Google Group
        [ContextMenu("Support Group")]
        private void SupportGroup()
        {
            Application.OpenURL("https://groups.google.com/forum/#!forum/final-ik");
        }

        // Link to the Final IK Asset Store thread in the Unity Community
        [ContextMenu("Asset Store Thread")]
        private void ASThread()
        {
            Application.OpenURL(
                "http://forum.unity3d.com/threads/final-ik-full-body-ik-aim-look-at-fabrik-ccd-ik-1-0-released.222685/");
        }

        /*
         * Limits the rotation in the local space of this instance's Transform.
         * */
        protected override Quaternion LimitRotation( Quaternion rotation )
        {
            // Subtracting off-limits swing
            var swing = LimitSwing(rotation);

            // Apply twist limits
            return LimitTwist(swing, axis, secondaryAxis, twistLimit);
        }

        /*
         * Apply the swing rotation limits
         * */
        public Quaternion LimitSwing( Quaternion rotation )
        {
            if (axis == Vector3.zero) return rotation; // Ignore with zero axes
            if (rotation == Quaternion.identity) return rotation; // Assuming initial rotation is in the reachable area

            // Get the rotation angle orthogonal to Axis
            var swingAxis = rotation * axis;
            var angle = GetOrthogonalAngle(swingAxis, secondaryAxis, axis);

            // Convert angle from 180 to 360 degrees representation
            var dot = Vector3.Dot(swingAxis, crossAxis);
            if (dot < 0) angle = 180 + (180 - angle);

            // Evaluate the limit for this angle
            var limit = spline.Evaluate(angle);

            // Get the limited swing axis
            var swingRotation = Quaternion.FromToRotation(axis, swingAxis);
            var limitedSwingRotation = Quaternion.RotateTowards(Quaternion.identity, swingRotation, limit);

            // Rotation from current(illegal) swing rotation to the limited(legal) swing rotation
            var toLimits = Quaternion.FromToRotation(swingAxis, limitedSwingRotation * axis);

            // Subtract the illegal rotation
            return toLimits * rotation;
        }

        #region Main Interface

        /// <summary>
        /// Limit of twist rotation around the main axis.
        /// </summary>
        [Range(0f, 180f)] public float twistLimit = 180;

        /// <summary>
        /// Set the spline keyframes.
        /// </summary>
        /// <param name='keyframes'>
        /// Keyframes.
        /// </param>
        public void SetSpline( Keyframe[] keyframes )
        {
            spline.keys = keyframes;
        }

        /*
         * The AnimationCurve orthogonally mapped onto a sphere that defines the swing limits
         * */
        [SerializeField] [HideInInspector] public AnimationCurve spline;

        #endregion Main Interface
    }
}