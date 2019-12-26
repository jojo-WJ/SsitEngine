using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// Simple angular rotation limit.
    /// </summary>
    [HelpURL("http://www.root-motion.com/finalikdox/html/page12.html")]
    [AddComponentMenu("Scripts/RootMotion.FinalIK/Rotation Limits/Rotation Limit Angle")]
    public class RotationLimitAngle : RotationLimit
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
                "http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_rotation_limit_angle.html");
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
         * Apply swing limits
         * */
        private Quaternion LimitSwing( Quaternion rotation )
        {
            if (axis == Vector3.zero) return rotation; // Ignore with zero axes
            if (rotation == Quaternion.identity) return rotation; // Assuming initial rotation is in the reachable area
            if (limit >= 180) return rotation;

            var swingAxis = rotation * axis;

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
        /// The swing limit.
        /// </summary>
        [Range(0f, 180f)] public float limit = 45;

        /// <summary>
        /// Limit of twist rotation around the main axis.
        /// </summary>
        [Range(0f, 180f)] public float twistLimit = 180;

        #endregion Main Interface
    }
}