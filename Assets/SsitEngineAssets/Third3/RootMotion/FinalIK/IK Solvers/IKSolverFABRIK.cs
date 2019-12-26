using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// Forward and Backward Reaching Inverse Kinematics solver.
    /// 
    /// This class is based on the "FABRIK: A fast, iterative solver for the inverse kinematics problem." paper by Aristidou, A., Lasenby, J.
    /// </summary>
    [Serializable]
    public class IKSolverFABRIK : IKSolverHeuristic
    {
        private bool[] limitedBones = new bool[0];
        private Vector3[] solverLocalPositions = new Vector3[0];

        /*
         * If true, the solver will work with 0 length bones
         * */
        protected override bool
            boneLengthCanBeZero // Returning false here also ensures that the bone lengths will be calculated
            =>
                false;

        protected override void OnInitiate()
        {
            if (firstInitiation || !Application.isPlaying) IKPosition = bones[bones.Length - 1].transform.position;

            for (var i = 0; i < bones.Length; i++)
            {
                bones[i].solverPosition = bones[i].transform.position;
                bones[i].solverRotation = bones[i].transform.rotation;
            }

            limitedBones = new bool[bones.Length];
            solverLocalPositions = new Vector3[bones.Length];

            InitiateBones();

            for (var i = 0; i < bones.Length; i++)
                solverLocalPositions[i] = Quaternion.Inverse(GetParentSolverRotation(i)) *
                                          (bones[i].transform.position - GetParentSolverPosition(i));
        }

        protected override void OnUpdate()
        {
            if (IKPositionWeight <= 0) return;
            IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);

            OnPreSolve();

            if (target != null) IKPosition = target.position;
            if (XY) IKPosition.z = bones[0].transform.position.z;

            var singularityOffset = maxIterations > 1 ? GetSingularityOffset() : Vector3.zero;

            // Iterating the solver
            for (var i = 0; i < maxIterations; i++)
            {
                // Optimizations
                if (singularityOffset == Vector3.zero && i >= 1 && tolerance > 0 &&
                    positionOffset < tolerance * tolerance) break;
                lastLocalDirection = localDirection;

                if (OnPreIteration != null) OnPreIteration(i);

                Solve(IKPosition + (i == 0 ? singularityOffset : Vector3.zero));
            }

            OnPostSolve();
        }

        /*
         * Interpolates the joint position to match the bone's length
        */
        private Vector3 SolveJoint( Vector3 pos1, Vector3 pos2, float length )
        {
            if (XY) pos1.z = pos2.z;

            return pos2 + (pos1 - pos2).normalized * length;
        }

        /*
         * Check if bones have moved from last solved positions
         * */
        private void OnPreSolve()
        {
            chainLength = 0;

            for (var i = 0; i < bones.Length; i++)
            {
                bones[i].solverPosition = bones[i].transform.position;
                bones[i].solverRotation = bones[i].transform.rotation;

                if (i < bones.Length - 1)
                {
                    bones[i].length = (bones[i].transform.position - bones[i + 1].transform.position).magnitude;
                    bones[i].axis = Quaternion.Inverse(bones[i].transform.rotation) *
                                    (bones[i + 1].transform.position - bones[i].transform.position);

                    chainLength += bones[i].length;
                }

                if (useRotationLimits)
                    solverLocalPositions[i] = Quaternion.Inverse(GetParentSolverRotation(i)) *
                                              (bones[i].transform.position - GetParentSolverPosition(i));
            }
        }

        /*
         * After solving the chain
         * */
        private void OnPostSolve()
        {
            // Rotating bones to match the solver positions
            if (!useRotationLimits) MapToSolverPositions();
            else MapToSolverPositionsLimited();

            lastLocalDirection = localDirection;
        }

        private void Solve( Vector3 targetPosition )
        {
            // Forward reaching
            ForwardReach(targetPosition);

            // Backward reaching
            BackwardReach(bones[0].transform.position);
        }

        /*
         * Stage 1 of FABRIK algorithm
         * */
        private void ForwardReach( Vector3 position )
        {
            // Lerp last bone's solverPosition to position
            bones[bones.Length - 1].solverPosition =
                Vector3.Lerp(bones[bones.Length - 1].solverPosition, position, IKPositionWeight);

            for (var i = 0; i < limitedBones.Length; i++) limitedBones[i] = false;

            for (var i = bones.Length - 2; i > -1; i--)
            {
                // Finding joint positions
                bones[i].solverPosition =
                    SolveJoint(bones[i].solverPosition, bones[i + 1].solverPosition, bones[i].length);

                // Limiting bone rotation forward
                LimitForward(i, i + 1);
            }

            // Limiting the first bone's rotation
            LimitForward(0, 0);
        }

        private void SolverMove( int index, Vector3 offset )
        {
            for (var i = index; i < bones.Length; i++) bones[i].solverPosition += offset;
        }

        private void SolverRotate( int index, Quaternion rotation, bool recursive )
        {
            for (var i = index; i < bones.Length; i++)
            {
                bones[i].solverRotation = rotation * bones[i].solverRotation;

                if (!recursive) return;
            }
        }

        private void SolverRotateChildren( int index, Quaternion rotation )
        {
            for (var i = index + 1; i < bones.Length; i++) bones[i].solverRotation = rotation * bones[i].solverRotation;
        }

        private void SolverMoveChildrenAroundPoint( int index, Quaternion rotation )
        {
            for (var i = index + 1; i < bones.Length; i++)
            {
                var dir = bones[i].solverPosition - bones[index].solverPosition;
                bones[i].solverPosition = bones[index].solverPosition + rotation * dir;
            }
        }

        private Quaternion GetParentSolverRotation( int index )
        {
            if (index > 0) return bones[index - 1].solverRotation;
            if (bones[0].transform.parent == null) return Quaternion.identity;
            return bones[0].transform.parent.rotation;
        }

        private Vector3 GetParentSolverPosition( int index )
        {
            if (index > 0) return bones[index - 1].solverPosition;
            if (bones[0].transform.parent == null) return Vector3.zero;
            return bones[0].transform.parent.position;
        }

        private Quaternion GetLimitedRotation( int index, Quaternion q, out bool changed )
        {
            changed = false;

            var parentRotation = GetParentSolverRotation(index);
            var localRotation = Quaternion.Inverse(parentRotation) * q;

            var limitedLocalRotation = bones[index].rotationLimit.GetLimitedLocalRotation(localRotation, out changed);

            if (!changed) return q;

            return parentRotation * limitedLocalRotation;
        }

        /*
         * Applying rotation limit to a bone in stage 1 in a more stable way
         * */
        private void LimitForward( int rotateBone, int limitBone )
        {
            if (!useRotationLimits) return;
            if (bones[limitBone].rotationLimit == null) return;

            // Storing last bone's position before applying the limit
            var lastBoneBeforeLimit = bones[bones.Length - 1].solverPosition;

            // Moving and rotating this bone and all its children to their solver positions
            for (var i = rotateBone; i < bones.Length - 1; i++)
            {
                if (limitedBones[i]) break;

                var fromTo = Quaternion.FromToRotation(bones[i].solverRotation * bones[i].axis,
                    bones[i + 1].solverPosition - bones[i].solverPosition);
                SolverRotate(i, fromTo, false);
            }

            // Limit the bone's rotation
            var changed = false;
            var afterLimit = GetLimitedRotation(limitBone, bones[limitBone].solverRotation, out changed);

            if (changed)
            {
                // Rotating and positioning the hierarchy so that the last bone's position is maintained
                if (limitBone < bones.Length - 1)
                {
                    var change = QuaTools.FromToRotation(bones[limitBone].solverRotation, afterLimit);
                    bones[limitBone].solverRotation = afterLimit;
                    SolverRotateChildren(limitBone, change);
                    SolverMoveChildrenAroundPoint(limitBone, change);

                    // Rotating to compensate for the limit
                    var fromTo = Quaternion.FromToRotation(
                        bones[bones.Length - 1].solverPosition - bones[rotateBone].solverPosition,
                        lastBoneBeforeLimit - bones[rotateBone].solverPosition);

                    SolverRotate(rotateBone, fromTo, true);
                    SolverMoveChildrenAroundPoint(rotateBone, fromTo);

                    // Moving the bone so that last bone maintains it's initial position
                    SolverMove(rotateBone, lastBoneBeforeLimit - bones[bones.Length - 1].solverPosition);
                }
                else
                {
                    // last bone
                    bones[limitBone].solverRotation = afterLimit;
                }
            }

            limitedBones[limitBone] = true;
        }

        /*
         * Stage 2 of FABRIK algorithm
         * */
        private void BackwardReach( Vector3 position )
        {
            if (useRotationLimits) BackwardReachLimited(position);
            else BackwardReachUnlimited(position);
        }

        /*
         * Stage 2 of FABRIK algorithm without rotation limits
         * */
        private void BackwardReachUnlimited( Vector3 position )
        {
            // Move first bone to position
            bones[0].solverPosition = position;

            // Finding joint positions
            for (var i = 1; i < bones.Length; i++)
                bones[i].solverPosition = SolveJoint(bones[i].solverPosition, bones[i - 1].solverPosition,
                    bones[i - 1].length);
        }

        /*
         * Stage 2 of FABRIK algorithm with limited rotations
         * */
        private void BackwardReachLimited( Vector3 position )
        {
            // Move first bone to position
            bones[0].solverPosition = position;

            // Applying rotation limits bone by bone
            for (var i = 0; i < bones.Length - 1; i++)
            {
                // Rotating bone to look at the solved joint position
                var nextPosition = SolveJoint(bones[i + 1].solverPosition, bones[i].solverPosition, bones[i].length);

                var swing = Quaternion.FromToRotation(bones[i].solverRotation * bones[i].axis,
                    nextPosition - bones[i].solverPosition);
                var targetRotation = swing * bones[i].solverRotation;

                // Rotation Constraints
                if (bones[i].rotationLimit != null)
                {
                    var changed = false;
                    targetRotation = GetLimitedRotation(i, targetRotation, out changed);
                }

                var fromTo = QuaTools.FromToRotation(bones[i].solverRotation, targetRotation);
                bones[i].solverRotation = targetRotation;
                SolverRotateChildren(i, fromTo);

                // Positioning the next bone to its default local position
                bones[i + 1].solverPosition =
                    bones[i].solverPosition + bones[i].solverRotation * solverLocalPositions[i + 1];
            }

            // Reconstruct solver rotations to protect from invalid Quaternions
            for (var i = 0; i < bones.Length; i++)
                bones[i].solverRotation = Quaternion.LookRotation(bones[i].solverRotation * Vector3.forward,
                    bones[i].solverRotation * Vector3.up);
        }

        /*
         * Rotate bones to match the solver positions when not using Rotation Limits
         * */
        private void MapToSolverPositions()
        {
            bones[0].transform.position = bones[0].solverPosition;

            for (var i = 0; i < bones.Length - 1; i++)
                if (XY)
                    bones[i].Swing2D(bones[i + 1].solverPosition);
                else
                    bones[i].Swing(bones[i + 1].solverPosition);
        }

        /*
         * Rotate bones to match the solver positions when using Rotation Limits
         * */
        private void MapToSolverPositionsLimited()
        {
            bones[0].transform.position = bones[0].solverPosition;

            for (var i = 0; i < bones.Length; i++)
                if (i < bones.Length - 1)
                    bones[i].transform.rotation = bones[i].solverRotation;
        }

        #region Main Interface

        /// <summary>
        /// Solving stage 1 of the %FABRIK algorithm.
        /// </summary>
        public void SolveForward( Vector3 position )
        {
            if (!initiated)
            {
                if (!Warning.logged) LogWarning("Trying to solve uninitiated FABRIK chain.");
                return;
            }

            OnPreSolve();

            ForwardReach(position);
        }

        /// <summary>
        /// Solving stage 2 of the %FABRIK algorithm.
        /// </summary>
        public void SolveBackward( Vector3 position )
        {
            if (!initiated)
            {
                if (!Warning.logged) LogWarning("Trying to solve uninitiated FABRIK chain.");
                return;
            }

            BackwardReach(position);

            OnPostSolve();
        }

        public override Vector3 GetIKPosition()
        {
            if (target != null) return target.position;
            return IKPosition;
        }

        /// <summary>
        /// Called before each iteration of the solver.
        /// </summary>
        public IterationDelegate OnPreIteration;

        #endregion Main Interface
    }
}