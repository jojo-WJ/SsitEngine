using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// Contains methods common for all heuristic solvers.
    /// </summary>
    [Serializable]
    public class IKSolverHeuristic : IKSolver
    {
        protected float chainLength;
        protected Vector3 lastLocalDirection;

        protected virtual int minBones => 2;
        protected virtual bool boneLengthCanBeZero => true;
        protected virtual bool allowCommonParent => false;

        protected override void OnInitiate()
        {
        }

        protected override void OnUpdate()
        {
        }

        /*
         * Initiates all bones to match their current state
         * */
        protected void InitiateBones()
        {
            chainLength = 0;

            for (var i = 0; i < bones.Length; i++) // Find out which local axis is directed at child/target position
                if (i < bones.Length - 1)
                {
                    bones[i].length = (bones[i].transform.position - bones[i + 1].transform.position).magnitude;
                    chainLength += bones[i].length;

                    var nextPosition = bones[i + 1].transform.position;
                    bones[i].axis = Quaternion.Inverse(bones[i].transform.rotation) *
                                    (nextPosition - bones[i].transform.position);

                    // Disable Rotation Limits from updating to take control of their execution order
                    if (bones[i].rotationLimit != null)
                    {
                        if (XY)
                        {
                            if (bones[i].rotationLimit is RotationLimitHinge)
                            {
                            }
                            else
                            {
                                Warning.Log("Only Hinge Rotation Limits should be used on 2D IK solvers.",
                                    bones[i].transform);
                            }
                        }
                        bones[i].rotationLimit.Disable();
                    }
                }
                else
                {
                    bones[i].axis = Quaternion.Inverse(bones[i].transform.rotation) *
                                    (bones[bones.Length - 1].transform.position - bones[0].transform.position);
                }
        }

        /*
         * Get target offset to break out of the linear singularity issue
         * */
        protected Vector3 GetSingularityOffset()
        {
            if (!SingularityDetected()) return Vector3.zero;

            var IKDirection = (IKPosition - bones[0].transform.position).normalized;

            var secondaryDirection = new Vector3(IKDirection.y, IKDirection.z, IKDirection.x);

            // Avoiding getting locked by the Hinge Rotation Limit
            if (useRotationLimits && bones[bones.Length - 2].rotationLimit != null &&
                bones[bones.Length - 2].rotationLimit is RotationLimitHinge)
                secondaryDirection = bones[bones.Length - 2].transform.rotation *
                                     bones[bones.Length - 2].rotationLimit.axis;

            return Vector3.Cross(IKDirection, secondaryDirection) * bones[bones.Length - 2].length * 0.5f;
        }

        /*
         * Detects linear singularity issue when the direction from first bone to IKPosition matches the direction from first bone to the last bone.
         * */
        private bool SingularityDetected()
        {
            if (!initiated) return false;

            var toLastBone = bones[bones.Length - 1].transform.position - bones[0].transform.position;
            var toIKPosition = IKPosition - bones[0].transform.position;

            var toLastBoneDistance = toLastBone.magnitude;
            var toIKPositionDistance = toIKPosition.magnitude;

            if (toLastBoneDistance < toIKPositionDistance) return false;
            if (toLastBoneDistance < chainLength - bones[bones.Length - 2].length * 0.1f) return false;
            if (toLastBoneDistance == 0) return false;
            if (toIKPositionDistance == 0) return false;
            if (toIKPositionDistance > toLastBoneDistance) return false;

            var dot = Vector3.Dot(toLastBone / toLastBoneDistance, toIKPosition / toIKPositionDistance);
            if (dot < 0.999f) return false;

            return true;
        }

        #region Main Interface

        /// <summary>
        /// The target Transform. Solver IKPosition will be automatically set to the position of the target.
        /// </summary>
        public Transform target;

        /// <summary>
        /// Minimum distance from last reached position. Will stop solving if difference from previous reached position is less than tolerance. If tolerance is zero, will iterate until maxIterations.
        /// </summary>
        public float tolerance;

        /// <summary>
        /// Max iterations per frame
        /// </summary>
        public int maxIterations = 4;

        /// <summary>
        /// If true, rotation limits (if excisting) will be applied on each iteration.
        /// </summary>
        public bool useRotationLimits = true;

        /// <summary>
        /// Solve in 2D?
        /// </summary>
        public bool XY;

        /// <summary>
        /// The hierarchy of bones.
        /// </summary>
        public Bone[] bones = new Bone[0];

        /// <summary>
        /// Rebuild the bone hierarcy and reinitiate the solver.
        /// </summary>
        /// <returns>
        /// Returns true if the new chain is valid.
        /// </returns>
        public bool SetChain( Transform[] hierarchy, Transform root )
        {
            if (bones == null || bones.Length != hierarchy.Length) bones = new Bone[hierarchy.Length];
            for (var i = 0; i < hierarchy.Length; i++)
            {
                if (bones[i] == null) bones[i] = new Bone();
                bones[i].transform = hierarchy[i];
            }

            Initiate(root);
            return initiated;
        }

        /// <summary>
        /// Adds a bone to the chain.
        /// </summary>
        public void AddBone( Transform bone )
        {
            var newBones = new Transform[bones.Length + 1];

            for (var i = 0; i < bones.Length; i++) newBones[i] = bones[i].transform;

            newBones[newBones.Length - 1] = bone;

            SetChain(newBones, root);
        }

        public override void StoreDefaultLocalState()
        {
            for (var i = 0; i < bones.Length; i++) bones[i].StoreDefaultLocalState();
        }

        public override void FixTransforms()
        {
            if (!initiated) return;
            if (IKPositionWeight <= 0f) return;

            for (var i = 0; i < bones.Length; i++) bones[i].FixTransform();
        }

        public override bool IsValid( ref string message )
        {
            if (bones.Length == 0)
            {
                message = "IK chain has no Bones.";
                return false;
            }
            if (bones.Length < minBones)
            {
                message = "IK chain has less than " + minBones + " Bones.";
                return false;
            }
            foreach (var bone in bones)
                if (bone.transform == null)
                {
                    message = "One of the Bones is null.";
                    return false;
                }

            var duplicate = ContainsDuplicateBone(bones);
            if (duplicate != null)
            {
                message = duplicate.name + " is represented multiple times in the Bones.";
                return false;
            }

            if (!allowCommonParent && !HierarchyIsValid(bones))
            {
                message =
                    "Invalid bone hierarchy detected. IK requires for it's bones to be parented to each other in descending order.";
                return false;
            }

            if (!boneLengthCanBeZero)
                for (var i = 0; i < bones.Length - 1; i++)
                {
                    var l = (bones[i].transform.position - bones[i + 1].transform.position).magnitude;
                    if (l == 0)
                    {
                        message = "Bone " + i + " length is zero.";
                        return false;
                    }
                }
            return true;
        }

        public override Point[] GetPoints()
        {
            return bones;
        }

        public override Point GetPoint( Transform transform )
        {
            for (var i = 0; i < bones.Length; i++)
                if (bones[i].transform == transform)
                    return bones[i];
            return null;
        }

        #endregion Main Interface

        #region Optimizations

        /*
         * Gets the direction from last bone to first bone in first bone's local space.
         * */
        protected virtual Vector3 localDirection => bones[0].transform
            .InverseTransformDirection(bones[bones.Length - 1].transform.position - bones[0].transform.position);

        /*
         * Gets the offset from last position of the last bone to its current position.
         * */
        protected float positionOffset => Vector3.SqrMagnitude(localDirection - lastLocalDirection);

        #endregion Optimizations
    }
}