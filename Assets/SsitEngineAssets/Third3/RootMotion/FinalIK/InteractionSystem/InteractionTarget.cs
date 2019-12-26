using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// The target of an effector in the InteractionSystem.
    /// </summary>
    [HelpURL("https://www.youtube.com/watch?v=r5jiZnsDH3M")]
    [AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction Target")]
    public class InteractionTarget : MonoBehaviour
    {
        private Quaternion defaultLocalRotation;

        /// <summary>
        /// The type of the FBBIK effector.
        /// </summary>
        [Tooltip("The type of the FBBIK effector.")]
        public FullBodyBipedEffector effectorType;

        /// <summary>
        /// The interaction speed multiplier for this effector. This can be used to make interactions faster/slower for specific effectors.
        /// </summary>
        [Tooltip(
            "The interaction speed multiplier for this effector. This can be used to make interactions faster/slower for specific effectors.")]
        public float interactionSpeedMlp = 1f;

        private Transform lastPivot;

        /// <summary>
        /// InteractionObject weight curve multipliers for this effector target.
        /// </summary>
        [Tooltip("InteractionObject weight curve multipliers for this effector target.")]
        public Multiplier[] multipliers;

        /// <summary>
        /// The pivot to twist/swing this interaction target about. For symmetric objects that can be interacted with from a certain angular range.
        /// </summary>
        [Tooltip(
            "The pivot to twist/swing this interaction target about. For symmetric objects that can be interacted with from a certain angular range.")]
        public Transform pivot;

        /// <summary>
        /// If true, will twist/swing around the pivot only once at the start of the interaction. If false, will continue rotating throuout the whole interaction.
        /// </summary>
        [Tooltip(
            "If true, will twist/swing around the pivot only once at the start of the interaction. If false, will continue rotating throuout the whole interaction.")]
        public bool rotateOnce = true;

        /// <summary>
        /// The weight of swinging the interaction target towards the effector bone in the start of the interaction. Swing is defined as a 3-DOF rotation around any axis, while twist is only around the twist axis.
        /// </summary>
        [Tooltip(
            "The weight of swinging the interaction target towards the effector bone in the start of the interaction. Swing is defined as a 3-DOF rotation around any axis, while twist is only around the twist axis.")]
        public float swingWeight;

        /// <summary>
        /// The axis of twisting the interaction target.
        /// </summary>
        [Tooltip("The axis of twisting the interaction target (blue line).")]
        public Vector3 twistAxis = Vector3.up;

        /// <summary>
        /// The weight of twisting the interaction target towards the effector bone in the start of the interaction.
        /// </summary>
        [Tooltip(
            "The weight of twisting the interaction target towards the effector bone in the start of the interaction.")]
        public float twistWeight = 1f;

        // Open a video tutorial video
        [ContextMenu("TUTORIAL VIDEO (PART 1: BASICS)")]
        private void OpenTutorial1()
        {
            Application.OpenURL("https://www.youtube.com/watch?v=r5jiZnsDH3M");
        }

        // Open a video tutorial video
        [ContextMenu("TUTORIAL VIDEO (PART 2: PICKING UP...)")]
        private void OpenTutorial2()
        {
            Application.OpenURL("https://www.youtube.com/watch?v=eP9-zycoHLk");
        }

        // Open a video tutorial video
        [ContextMenu("TUTORIAL VIDEO (PART 3: ANIMATION)")]
        private void OpenTutorial3()
        {
            Application.OpenURL(
                "https://www.youtube.com/watch?v=sQfB2RcT1T4&index=14&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
        }

        // Open a video tutorial video
        [ContextMenu("TUTORIAL VIDEO (PART 4: TRIGGERS)")]
        private void OpenTutorial4()
        {
            Application.OpenURL(
                "https://www.youtube.com/watch?v=-TDZpNjt2mk&index=15&list=PLVxSIA1OaTOu8Nos3CalXbJ2DrKnntMv6");
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

        // Should a curve of the Type be ignored for this effector?
        public float GetValue( InteractionObject.WeightCurve.Type curveType )
        {
            for (var i = 0; i < multipliers.Length; i++)
                if (multipliers[i].curve == curveType)
                    return multipliers[i].multiplier;
            return 1f;
        }

        // Reset the twist and swing rotation of the target
        public void ResetRotation()
        {
            if (pivot != null) pivot.localRotation = defaultLocalRotation;
        }

        // Rotate this target towards a position
        public void RotateTo( Vector3 position )
        {
            if (pivot == null) return;

            if (pivot != lastPivot)
            {
                defaultLocalRotation = pivot.localRotation;
                lastPivot = pivot;
            }

            // Rotate to the default local rotation
            pivot.localRotation = defaultLocalRotation;

            // Twisting around the twist axis
            if (twistWeight > 0f)
            {
                var targetTangent = transform.position - pivot.position;
                var n = pivot.rotation * twistAxis;
                var normal = n;
                Vector3.OrthoNormalize(ref normal, ref targetTangent);

                normal = n;
                var direction = position - pivot.position;
                Vector3.OrthoNormalize(ref normal, ref direction);

                var q = QuaTools.FromToAroundAxis(targetTangent, direction, n);
                pivot.rotation = Quaternion.Lerp(Quaternion.identity, q, twistWeight) * pivot.rotation;
            }

            // Swinging freely
            if (swingWeight > 0f)
            {
                var s = Quaternion.FromToRotation(transform.position - pivot.position, position - pivot.position);
                pivot.rotation = Quaternion.Lerp(Quaternion.identity, s, swingWeight) * pivot.rotation;
            }
        }

        // Open the User Manual URL
        [ContextMenu("User Manual")]
        private void OpenUserManual()
        {
            Application.OpenURL("http://www.root-motion.com/finalikdox/html/page10.html");
        }

        // Open the Script Reference URL
        [ContextMenu("Scrpt Reference")]
        private void OpenScriptReference()
        {
            Application.OpenURL(
                "http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_target.html");
        }

        /// <summary>
        /// Multiplies the value of a weight curve for this effector target.
        /// </summary>
        [Serializable]
        public class Multiplier
        {
            /// <summary>
            /// The curve type (InteractionObject.WeightCurve.Type).
            /// </summary>
            [Tooltip("The curve type (InteractionObject.WeightCurve.Type).")]
            public InteractionObject.WeightCurve.Type curve;

            /// <summary>
            /// Multiplier of the curve's value.
            /// </summary>
            [Tooltip("Multiplier of the curve's value.")]
            public float multiplier;
        }
    }
}