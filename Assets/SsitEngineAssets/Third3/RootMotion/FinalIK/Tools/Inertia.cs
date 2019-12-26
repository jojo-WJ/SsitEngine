using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// Demo script that adds the illusion of mass to your character using FullBodyBipedIK.
    /// </summary>
    public class Inertia : OffsetModifier
    {
        [Tooltip("The array of Bodies")] public Body[] bodies;

        [Tooltip("The array of OffsetLimits")] public OffsetLimits[] limits;

        // Reset all Bodies
        public void ResetBodies()
        {
            lastTime = Time.time;
            foreach (var body in bodies) body.Reset();
        }

        // Called by IKSolverFullBody before updating
        protected override void OnModifyOffset()
        {
            // Update the Bodies
            foreach (var body in bodies) body.Update(ik.solver, weight, deltaTime);

            // Apply the offset limits
            ApplyLimits(limits);
        }

        /// <summary>
        /// Body is just following it's transform in a lazy and bouncy way.
        /// </summary>
        [Serializable]
        public class Body
        {
            [Tooltip("The acceleration, smaller values means lazyer following")]
            public float acceleration = 3f;

            private Vector3 delta;
            private Vector3 direction;

            [Tooltip("Linking the body to effectors. One Body can be used to offset more than one effector")]
            public EffectorLink[] effectorLinks;

            private bool firstUpdate = true;

            [Tooltip("gravity applied to the Body")]
            public float gravity;

            private Vector3 lastPosition;
            private Vector3 lazyPoint;

            [Tooltip("Matching target velocity")] [Range(0f, 1f)]
            public float matchVelocity;

            [Tooltip("The speed to follow the Transform")]
            public float speed = 10f;

            [Tooltip("The Transform to follow, can be any bone of the character")]
            public Transform transform;

            // Reset to Transform
            public void Reset()
            {
                if (transform == null) return;
                lazyPoint = transform.position;
                lastPosition = transform.position;
                direction = Vector3.zero;
            }

            // Update this body, apply the offset to the effector
            public void Update( IKSolverFullBodyBiped solver, float weight, float deltaTime )
            {
                if (transform == null) return;

                // If first update, set this body to Transform
                if (firstUpdate)
                {
                    Reset();
                    firstUpdate = false;
                }

                // Acceleration
                direction = Vector3.Lerp(direction, (transform.position - lazyPoint) / deltaTime * 0.01f,
                    deltaTime * acceleration);

                // Lazy follow
                lazyPoint += direction * deltaTime * speed;

                // Match velocity
                delta = transform.position - lastPosition;
                lazyPoint += delta * matchVelocity;

                // Gravity
                lazyPoint.y += gravity * deltaTime;

                // Apply position offset to the effector
                foreach (var effectorLink in effectorLinks)
                    solver.GetEffector(effectorLink.effector).positionOffset +=
                        (lazyPoint - transform.position) * effectorLink.weight * weight;

                lastPosition = transform.position;
            }

            /// <summary>
            /// Linking this to an effector
            /// </summary>
            [Serializable]
            public class EffectorLink
            {
                [Tooltip("Type of the FBBIK effector to use")]
                public FullBodyBipedEffector effector;

                [Tooltip("Weight of using this effector")]
                public float weight;
            }
        }
    }
}