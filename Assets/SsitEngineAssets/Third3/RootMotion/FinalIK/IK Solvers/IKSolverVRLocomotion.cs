using System;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// Hybrid %IK solver designed for mapping a character to a VR headset and 2 hand controllers 
    /// </summary>
    public partial class IKSolverVR : IKSolver
    {
        [Serializable]
        public class Locomotion
        {
            [Tooltip(
                "Makes a step only if step target position is at least 'Step Threshold' far from the current footstep or the foot does not reach the current footstep anymore or footstep angle is past this value.")]
            /// <summary>
            /// Makes a step only if step target position is at least 'Step Threshold' far from the current footstep or the foot does not reach the current footstep anymore or footstep angle is past this value.
            /// </summary>
            public float angleThreshold = 60f;

            [HideInInspector] public bool blockingEnabled;
            [HideInInspector] public LayerMask blockingLayers;

            [Tooltip(
                "Multiplies angle of the center of mass - center of pressure vector. Larger value makes the character step sooner if losing balance.")]
            /// <summary>
            /// Multiplies angle of the center of mass - center of pressure vector. Larger value makes the character step sooner if losing balance.
            /// </summary>
            public float comAngleMlp = 1f;

            private Vector3 comVelocity;

            [Tooltip("Tries to maintain this distance between the legs.")]
            /// <summary>
            /// Tries to maintain this distance between the legs.
            /// </summary>
            public float footDistance = 0.3f;

            private Footstep[] footsteps = new Footstep[0];

            [Tooltip("The height offset of the heel by normalized step progress (0 - 1).")]
            /// <summary>
            /// The height offset of the heel by normalized step progress (0 - 1).
            /// </summary>
            public AnimationCurve heelHeight;

            private Vector3 lastComPosition;
            private int leftFootIndex;

            [Tooltip(
                "How much can a leg be extended before it is forced to step to another position? 1 means fully stretched.")]
            /// <summary>
            /// How much can a leg be extended before it is forced to step to another position? 1 means fully stretched.
            /// </summary>
            [Range(0.9f, 1f)]
            public float maxLegStretch = 1f;

            [Tooltip("Maximum magnitude of head/hand target velocity used in prediction.")]
            /// <summary>
            /// Maximum magnitude of head/hand target velocity used in prediction.
            /// </summary>
            public float maxVelocity = 0.4f;

            [Tooltip("Offset for the approximated center of mass.")]
            /// <summary>
            /// Offset for the approximated center of mass.
            /// </summary>
            public Vector3 offset;

            [Tooltip("Called when the left foot has finished a step.")]
            /// <summary>
            /// Called when the left foot has finished a step.
            /// </summary>
            public UnityEvent onLeftFootstep = new UnityEvent();

            [Tooltip("Called when the right foot has finished a step")]
            /// <summary>
            /// Called when the right foot has finished a step
            /// </summary>
            public UnityEvent onRightFootstep = new UnityEvent();

            [HideInInspector] public float raycastHeight = 0.2f;
            [HideInInspector] public float raycastRadius = 0.2f;

            [Tooltip(
                "Rotates the foot while the leg is not stepping to relax the twist rotation of the leg if ideal rotation is past this angle.")]
            /// <summary>
            /// Rotates the foot while the leg is not stepping to relax the twist rotation of the leg if ideal rotation is past this angle.
            /// </summary>
            [Range(0f, 180f)]
            public float relaxLegTwistMinAngle = 20f;

            [Tooltip(
                "The speed of rotating the foot while the leg is not stepping to relax the twist rotation of the leg.")]
            /// <summary>
            /// The speed of rotating the foot while the leg is not stepping to relax the twist rotation of the leg.
            /// </summary>
            public float relaxLegTwistSpeed = 400f;

            private int rightFootIndex;

            [Tooltip(
                "The speed of lerping the root of the character towards the horizontal mid-point of the footsteps.")]
            /// <summary>
            /// The speed of lerping the root of the character towards the horizontal mid-point of the footsteps.
            /// </summary>
            public float rootSpeed = 20f;

            [Tooltip("The height of the foot by normalized step progress (0 - 1).")]
            /// <summary>
            /// The height of the foot by normalized step progress (0 - 1).
            /// </summary>
            public AnimationCurve stepHeight;

            [Tooltip("Interpolation mode of the step.")]
            /// <summary>
            /// Interpolation mode of the step.
            /// </summary>
            public InterpolationMode stepInterpolation = InterpolationMode.InOutSine;

            [Tooltip("The speed of steps.")]
            /// <summary>
            /// The speed of steps
            /// </summary>
            public float stepSpeed = 3f;

            [Tooltip(
                "Makes a step only if step target position is at least this far from the current footstep or the foot does not reach the current footstep anymore or footstep angle is past the 'Angle Threshold'.")]
            /// <summary>
            /// Makes a step only if step target position is at least this far from the current footstep or the foot does not reach the current footstep anymore or footstep angle is past the 'Angle Threshold'.
            /// </summary>
            public float stepThreshold = 0.4f;

            [Tooltip("The amount of head/hand target velocity prediction.")]
            /// <summary>
            /// The amount of head/hand target velocity prediction.
            /// </summary>
            public float velocityFactor = 0.4f;

            [Tooltip("Used for blending in/out of procedural locomotion.")]
            /// <summary>
            /// Used for blending in/out of procedural locomotion.
            /// </summary>
            [Range(0f, 1f)]
            public float weight = 1f;

            /// <summary>
            /// Gets the approximated center of mass.
            /// </summary>
            public Vector3 centerOfMass { get; private set; }

            public Vector3 leftFootstepPosition => footsteps[0].position;

            public Vector3 rightFootstepPosition => footsteps[1].position;

            public Quaternion leftFootstepRotation => footsteps[0].rotation;

            public Quaternion rightFootstepRotation => footsteps[1].rotation;

            public void Initiate( Vector3[] positions, Quaternion[] rotations, bool hasToes )
            {
                leftFootIndex = hasToes ? 17 : 16;
                rightFootIndex = hasToes ? 21 : 20;

                footsteps = new Footstep[2]
                {
                    new Footstep(rotations[0], positions[leftFootIndex], rotations[leftFootIndex],
                        footDistance * Vector3.left),
                    new Footstep(rotations[0], positions[rightFootIndex], rotations[rightFootIndex],
                        footDistance * Vector3.right)
                };
            }

            public void Reset( Vector3[] positions, Quaternion[] rotations )
            {
                lastComPosition = Vector3.Lerp(positions[1], positions[5], 0.25f) + rotations[0] * offset;
                comVelocity = Vector3.zero;

                footsteps[0].Reset(rotations[0], positions[leftFootIndex], rotations[leftFootIndex]);
                footsteps[1].Reset(rotations[0], positions[rightFootIndex], rotations[rightFootIndex]);
            }

            public void AddDeltaRotation( Quaternion delta, Vector3 pivot )
            {
                var toLastComPosition = lastComPosition - pivot;
                lastComPosition = pivot + delta * toLastComPosition;

                foreach (var f in footsteps)
                {
                    f.rotation = delta * f.rotation;
                    f.stepFromRot = delta * f.stepFromRot;
                    f.stepToRot = delta * f.stepToRot;
                    f.stepToRootRot = delta * f.stepToRootRot;

                    var toF = f.position - pivot;
                    f.position = pivot + delta * toF;

                    var toStepFrom = f.stepFrom - pivot;
                    f.stepFrom = pivot + delta * toStepFrom;

                    var toStepTo = f.stepTo - pivot;
                    f.stepTo = pivot + delta * toStepTo;
                }
            }

            public void AddDeltaPosition( Vector3 delta )
            {
                lastComPosition += delta;

                foreach (var f in footsteps)
                {
                    f.position += delta;
                    f.stepFrom += delta;
                    f.stepTo += delta;
                }
            }

            public void Solve( VirtualBone rootBone, Spine spine, Leg leftLeg, Leg rightLeg, Arm leftArm, Arm rightArm,
                int supportLegIndex, out Vector3 leftFootPosition, out Vector3 rightFootPosition,
                out Quaternion leftFootRotation, out Quaternion rightFootRotation, out float leftFootOffset,
                out float rightFootOffset, out float leftHeelOffset, out float rightHeelOffset )
            {
                if (weight <= 0f)
                {
                    leftFootPosition = Vector3.zero;
                    rightFootPosition = Vector3.zero;
                    leftFootRotation = Quaternion.identity;
                    rightFootRotation = Quaternion.identity;
                    leftFootOffset = 0f;
                    rightFootOffset = 0f;
                    leftHeelOffset = 0f;
                    rightHeelOffset = 0f;
                    return;
                }

                var rootUp = rootBone.solverRotation * Vector3.up;

                var leftThighPosition = spine.pelvis.solverPosition +
                                        spine.pelvis.solverRotation * leftLeg.thighRelativeToPelvis;
                var rightThighPosition = spine.pelvis.solverPosition +
                                         spine.pelvis.solverRotation * rightLeg.thighRelativeToPelvis;

                footsteps[0].characterSpaceOffset = footDistance * Vector3.left;
                footsteps[1].characterSpaceOffset = footDistance * Vector3.right;

                var forward = spine.faceDirection;
                var forwardY = V3Tools.ExtractVertical(forward, rootUp, 1f);
                forward -= forwardY;
                var forwardRotation = Quaternion.LookRotation(forward, rootUp);

                //centerOfMass = Vector3.Lerp(spine.pelvis.solverPosition, spine.head.solverPosition, 0.25f) + rootBone.solverRotation * offset;

                var pelvisMass = 1f;
                var headMass = 1f;
                var armMass = 0.2f;
                var totalMass = pelvisMass + headMass + 2f * armMass;

                centerOfMass = Vector3.zero;
                centerOfMass += spine.pelvis.solverPosition * pelvisMass;
                centerOfMass += spine.head.solverPosition * headMass;
                centerOfMass += leftArm.position * armMass;
                centerOfMass += rightArm.position * armMass;
                centerOfMass /= totalMass;

                centerOfMass += rootBone.solverRotation * offset;

                comVelocity = Time.deltaTime > 0f ? (centerOfMass - lastComPosition) / Time.deltaTime : Vector3.zero;
                lastComPosition = centerOfMass;
                comVelocity = Vector3.ClampMagnitude(comVelocity, maxVelocity) * velocityFactor;
                var centerOfMassV = centerOfMass + comVelocity;

                var pelvisPositionGroundLevel =
                    V3Tools.PointToPlane(spine.pelvis.solverPosition, rootBone.solverPosition, rootUp);
                var centerOfMassVGroundLevel = V3Tools.PointToPlane(centerOfMassV, rootBone.solverPosition, rootUp);

                var centerOfPressure = Vector3.Lerp(footsteps[0].position, footsteps[1].position, 0.5f);

                var comDir = centerOfMassV - centerOfPressure;
                var comAngle = Vector3.Angle(comDir, rootBone.solverRotation * Vector3.up) * comAngleMlp;

                // Set support leg
                for (var i = 0; i < footsteps.Length; i++) footsteps[i].isSupportLeg = supportLegIndex == i;

                // Update stepTo while stepping
                for (var i = 0; i < footsteps.Length; i++)
                    if (footsteps[i].isStepping)
                    {
                        var stepTo = centerOfMassVGroundLevel +
                                     rootBone.solverRotation * footsteps[i].characterSpaceOffset;

                        if (!StepBlocked(footsteps[i].stepFrom, stepTo, rootBone.solverPosition))
                            footsteps[i].UpdateStepping(stepTo, forwardRotation, 10f);
                    }
                    else
                    {
                        footsteps[i].UpdateStanding(forwardRotation, relaxLegTwistMinAngle, relaxLegTwistSpeed);
                    }

                // Triggering new footsteps
                if (CanStep())
                {
                    var stepLegIndex = -1;
                    var bestValue = -Mathf.Infinity;

                    for (var i = 0; i < footsteps.Length; i++)
                        if (!footsteps[i].isStepping)
                        {
                            var stepTo = centerOfMassVGroundLevel +
                                         rootBone.solverRotation * footsteps[i].characterSpaceOffset;

                            var legLength = i == 0 ? leftLeg.mag : rightLeg.mag;
                            var thighPos = i == 0 ? leftThighPosition : rightThighPosition;

                            var thighDistance = Vector3.Distance(footsteps[i].position, thighPos);

                            var lengthStep = false;
                            if (thighDistance >= legLength * maxLegStretch)
                            {
                                // * 0.95f) {
                                stepTo = pelvisPositionGroundLevel +
                                         rootBone.solverRotation * footsteps[i].characterSpaceOffset;
                                lengthStep = true;
                            }

                            var collision = false;
                            for (var n = 0; n < footsteps.Length; n++)
                                if (n != i && !lengthStep)
                                {
                                    if (Vector3.Distance(footsteps[i].position, footsteps[n].position) < 0.25f &&
                                        (footsteps[i].position - stepTo).sqrMagnitude <
                                        (footsteps[n].position - stepTo).sqrMagnitude)
                                    {
                                    }
                                    else
                                    {
                                        collision = GetLineSphereCollision(footsteps[i].position, stepTo,
                                            footsteps[n].position, 0.25f);
                                    }
                                    if (collision) break;
                                }

                            var angle = Quaternion.Angle(forwardRotation, footsteps[i].stepToRootRot);

                            if (!collision || angle > angleThreshold)
                            {
                                var stepDistance = Vector3.Distance(footsteps[i].position, stepTo);
                                var sT = Mathf.Lerp(stepThreshold, stepThreshold * 0.1f, comAngle * 0.015f);
                                if (lengthStep) sT *= 0.5f;
                                if (i == 0) sT *= 0.9f;

                                if (!StepBlocked(footsteps[i].position, stepTo, rootBone.solverPosition))
                                    if (stepDistance > sT || angle > angleThreshold)
                                    {
                                        var value = 0f;

                                        value -= stepDistance;

                                        if (value > bestValue)
                                        {
                                            stepLegIndex = i;
                                            bestValue = value;
                                        }
                                    }
                            }
                        }

                    if (stepLegIndex != -1)
                    {
                        var stepTo = centerOfMassVGroundLevel +
                                     rootBone.solverRotation * footsteps[stepLegIndex].characterSpaceOffset;
                        footsteps[stepLegIndex].stepSpeed = Random.Range(stepSpeed, stepSpeed * 1.5f);
                        footsteps[stepLegIndex].StepTo(stepTo, forwardRotation);
                    }
                }

                footsteps[0].Update(stepInterpolation, onLeftFootstep);
                footsteps[1].Update(stepInterpolation, onRightFootstep);

                leftFootPosition = footsteps[0].position;
                rightFootPosition = footsteps[1].position;

                leftFootPosition = V3Tools.PointToPlane(leftFootPosition, leftLeg.lastBone.readPosition, rootUp);
                rightFootPosition = V3Tools.PointToPlane(rightFootPosition, rightLeg.lastBone.readPosition, rootUp);

                leftFootOffset = stepHeight.Evaluate(footsteps[0].stepProgress);
                rightFootOffset = stepHeight.Evaluate(footsteps[1].stepProgress);

                leftHeelOffset = heelHeight.Evaluate(footsteps[0].stepProgress);
                rightHeelOffset = heelHeight.Evaluate(footsteps[1].stepProgress);

                leftFootRotation = footsteps[0].rotation;
                rightFootRotation = footsteps[1].rotation;
            }

            private bool StepBlocked( Vector3 fromPosition, Vector3 toPosition, Vector3 rootPosition )
            {
                if (blockingLayers == -1 || !blockingEnabled) return false;

                var origin = fromPosition;
                origin.y = rootPosition.y + raycastHeight + raycastRadius;

                var direction = toPosition - origin;
                direction.y = 0f;

                RaycastHit hit;

                if (raycastRadius <= 0f)
                    return Physics.Raycast(origin, direction, out hit, direction.magnitude, blockingLayers);
                return Physics.SphereCast(origin, raycastRadius, direction, out hit, direction.magnitude,
                    blockingLayers);
            }

            private bool CanStep()
            {
                foreach (var f in footsteps)
                    if (f.isStepping && f.stepProgress < 0.8f)
                        return false;

                return true;
            }

            private static bool GetLineSphereCollision( Vector3 lineStart, Vector3 lineEnd, Vector3 sphereCenter,
                float sphereRadius )
            {
                var line = lineEnd - lineStart;
                var toSphere = sphereCenter - lineStart;
                var distToSphereCenter = toSphere.magnitude;
                var d = distToSphereCenter - sphereRadius;

                if (d > line.magnitude) return false;

                var q = Quaternion.LookRotation(line, toSphere);

                var toSphereRotated = Quaternion.Inverse(q) * toSphere;

                if (toSphereRotated.z < 0f) return d < 0f;

                return toSphereRotated.y - sphereRadius < 0f;
            }
        }
    }
}