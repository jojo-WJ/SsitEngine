using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// Definition of FBBIK Offset pose.
    /// </summary>
    public class OffsetPose : MonoBehaviour
    {
        public EffectorLink[] effectorLinks = new EffectorLink[0];

        // Apply positionOffsets of all the EffectorLinks
        public void Apply( IKSolverFullBodyBiped solver, float weight )
        {
            for (var i = 0; i < effectorLinks.Length; i++)
                effectorLinks[i].Apply(solver, weight, solver.GetRoot().rotation);
        }

        public void Apply( IKSolverFullBodyBiped solver, float weight, Quaternion rotation )
        {
            for (var i = 0; i < effectorLinks.Length; i++) effectorLinks[i].Apply(solver, weight, rotation);
        }

        /// <summary>
        /// State of an effector in this pose
        /// </summary>
        [Serializable]
        public class EffectorLink
        {
            public FullBodyBipedEffector effector; // The effector type (this is just an enum)
            public Vector3 offset; // Offset of the effector in this pose
            public Vector3 pin; // Pin position relative to the solver root Transform
            public Vector3 pinWeight; // Pin weight vector

            // Apply positionOffset to the effector
            public void Apply( IKSolverFullBodyBiped solver, float weight, Quaternion rotation )
            {
                // Offset
                solver.GetEffector(effector).positionOffset += rotation * offset * weight;

                // Calculating pinned position
                var pinPosition = solver.GetRoot().position + rotation * pin;
                var pinPositionOffset = pinPosition - solver.GetEffector(effector).bone.position;

                var pinWeightVector = pinWeight * Mathf.Abs(weight);

                // Lerping to pinned position
                solver.GetEffector(effector).positionOffset = new Vector3(
                    Mathf.Lerp(solver.GetEffector(effector).positionOffset.x, pinPositionOffset.x, pinWeightVector.x),
                    Mathf.Lerp(solver.GetEffector(effector).positionOffset.y, pinPositionOffset.y, pinWeightVector.y),
                    Mathf.Lerp(solver.GetEffector(effector).positionOffset.z, pinPositionOffset.z, pinWeightVector.z)
                );
            }
        }
    }
}