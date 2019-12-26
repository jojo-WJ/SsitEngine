using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// %Constraints to rotation in world space
    /// </summary>
    [Serializable]
    public class ConstraintRotation : Constraint
    {
        public ConstraintRotation()
        {
        }

        public ConstraintRotation( Transform transform )
        {
            this.transform = transform;
        }

        #region Main Interface

        /// <summary>
        /// The target rotation.
        /// </summary>
        public Quaternion rotation;

        public override void UpdateConstraint()
        {
            if (weight <= 0) return;
            if (!isValid) return;

            // Slerping to target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, weight);
        }

        #endregion Main Interface
    }
}