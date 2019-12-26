using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// The base abstract class for all Transform constraints.
    /// </summary>
    [Serializable]
    public abstract class Constraint
    {
        #region Main Interface

        /// <summary>
        /// The transform to constrain.
        /// </summary>
        public Transform transform;

        /// <summary>
        /// %Constraint weight.
        /// </summary>
        public float weight;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Constraint"/> is valid.
        /// </summary>
        /// <value>
        /// <c>true</c> if is valid; otherwise, <c>false</c>.
        /// </value>
        public bool isValid => transform != null;

        /// <summary>
        /// Updates the constraint.
        /// </summary>
        public abstract void UpdateConstraint();

        #endregion Main Interface
    }
}