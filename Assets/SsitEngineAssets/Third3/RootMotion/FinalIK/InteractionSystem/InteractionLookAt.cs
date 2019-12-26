using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// Controls LookAtIK for the InteractionSystem
    /// </summary>
    [Serializable]
    public class InteractionLookAt
    {
        private bool
            firstFBBIKSolve; // Has the FBBIK already solved for this frame? In case it is solved more than once, for example when using the ShoulderRotator

        // (Optional) reference to the LookAtIK component that will be used to make the character look at the objects that it is interacting with.
        [Tooltip(
            "(Optional) reference to the LookAtIK component that will be used to make the character look at the objects that it is interacting with.")]
        public LookAtIK ik;

        [HideInInspector] public bool isPaused;

        /// <summary>
        /// Interpolation speed of the LookAtIK target.
        /// </summary>
        [Tooltip("Interpolation speed of the LookAtIK target.")]
        public float lerpSpeed = 5f;

        private Transform lookAtTarget; // The target Transform to look at
        private float stopLookTime; // Time to start fading out the LookAtIK
        private float weight; // Current weight

        /// <summary>
        /// Interpolation speed of the LookAtIK weight.
        /// </summary>
        [Tooltip("Interpolation speed of the LookAtIK weight.")]
        public float weightSpeed = 1f;

        /// <summary>
        /// Look the specified target for the specified time.
        /// </summary>
        public void Look( Transform target, float time )
        {
            if (ik == null) return;

            if (ik.solver.IKPositionWeight <= 0f)
                ik.solver.IKPosition = ik.solver.GetRoot().position + ik.solver.GetRoot().forward * 3f;
            lookAtTarget = target;
            stopLookTime = time;
        }

        public void OnFixTransforms()
        {
            if (ik == null) return;
            if (ik.fixTransforms) ik.solver.FixTransforms();
        }

        public void Update()
        {
            if (ik == null) return;
            if (ik.enabled) ik.enabled = false;

            if (lookAtTarget == null) return;

            if (isPaused) stopLookTime += Time.deltaTime;

            // Interpolate the weight
            var add = Time.time < stopLookTime ? weightSpeed : -weightSpeed;
            weight = Mathf.Clamp(weight + add * Time.deltaTime, 0f, 1f);

            // Set LookAtIK weight
            ik.solver.IKPositionWeight = Interp.Float(weight, InterpolationMode.InOutQuintic);

            // Set LookAtIK position
            ik.solver.IKPosition =
                Vector3.Lerp(ik.solver.IKPosition, lookAtTarget.position, lerpSpeed * Time.deltaTime);

            // Release the LookAtIK for other tasks once we're weighed out
            if (weight <= 0f) lookAtTarget = null;

            firstFBBIKSolve = true;
        }

        public void SolveSpine()
        {
            if (ik == null) return;
            if (!firstFBBIKSolve) return;

            var headWeight = ik.solver.headWeight;
            var eyesWeight = ik.solver.eyesWeight;

            ik.solver.headWeight = 0f;
            ik.solver.eyesWeight = 0f;
            ik.solver.Update();
            ik.solver.headWeight = headWeight;
            ik.solver.eyesWeight = eyesWeight;
        }

        public void SolveHead()
        {
            if (ik == null) return;
            if (!firstFBBIKSolve) return;

            var bodyWeight = ik.solver.bodyWeight;

            ik.solver.bodyWeight = 0f;
            ik.solver.Update();
            ik.solver.bodyWeight = bodyWeight;

            firstFBBIKSolve = false;
        }
    }
}