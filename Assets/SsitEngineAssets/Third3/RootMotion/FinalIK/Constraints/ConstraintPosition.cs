using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// %Constraints to position in world space.
    /// </summary>
    [Serializable]
    public class ConstraintPosition : Constraint
    {
        public ConstraintPosition()
        {
        }

        public ConstraintPosition( Transform transform )
        {
            this.transform = transform;
        }

        #region Main Interface

        /// <summary>
        /// The target position.
        /// </summary>
        public Vector3 position;

        public override void UpdateConstraint()
        {
            if (weight <= 0) return;
            if (!isValid) return;

            // Lerping to position
            transform.position = Vector3.Lerp(transform.position, position, weight);
        }

        #endregion Main Interface
    }
}