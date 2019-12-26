using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// Hybrid %IK solver designed for mapping a character to a VR headset and 2 hand controllers 
    /// </summary>
    public partial class IKSolverVR : IKSolver
    {
        /// <summary>
        /// Spine solver for IKSolverVR.
        /// </summary>
        [Serializable]
        public class Spine : BodyPart
        {
            private Quaternion anchorRelativeToHead = Quaternion.identity;

            [Tooltip("Determines how much the body will follow the position of the head.")]
            /// <summary>
            /// Determines how much the body will follow the position of the head.
            /// </summary>
            [Range(0f, 1f)]
            public float bodyPosStiffness = 0.55f;

            [Tooltip("Determines how much the body will follow the rotation of the head.")]
            /// <summary>
            /// Determines how much the body will follow the rotation of the head.
            /// </summary>
            [Range(0f, 1f)]
            public float bodyRotStiffness = 0.1f;

            [Tooltip("Clamps chest rotation.")]
            /// <summary>
            /// Clamps chest rotation.
            /// </summary>
            [Range(0f, 1f)]
            public float chestClampWeight = 0.5f;

            private Vector3 chestForward;

            [Tooltip("If 'Chest Goal Weight' is greater than 0, the chest will be turned towards this Transform.")]
            /// <summary>
            /// If chestGoalWeight is greater than 0, the chest will be turned towards this Transform.
            /// </summary>
            public Transform chestGoal;

            [Tooltip("Rotational weight of the chest target.")]
            /// <summary>
            /// Rotational weight of the chest target.
            /// </summary>
            [Range(0f, 1f)]
            public float chestGoalWeight;

            /// <summary>
            /// Position offset of the chest. Will be reset to Vector3.zero after each update.
            /// </summary>
            [NonSerialized] [HideInInspector] public Vector3 chestPositionOffset;

            private Quaternion chestRelativeRotation = Quaternion.identity;

            /// <summary>
            /// Rotation offset of the chest. Will be reset to Quaternion.identity after each update.
            /// </summary>
            [NonSerialized] [HideInInspector] public Quaternion chestRotationOffset = Quaternion.identity;

            private Quaternion chestTargetRotation = Quaternion.identity;

            [NonSerialized] [HideInInspector] public Vector3 faceDirection;

            /// <summary>
            /// The goal position for the chest. If chestGoalWeight > 0, the chest will be turned towards this position.
            /// </summary>
            [NonSerialized] [HideInInspector] public Vector3 goalPositionChest;

            private bool hasChest;
            private bool hasNeck;

            [Tooltip("Clamps head rotation.")]
            /// <summary>
            /// Clamps head rotation.
            /// </summary>
            [Range(0f, 1f)]
            public float headClampWeight = 0.6f;

            private Vector3 headDeltaPosition;
            private float headHeight;
            [NonSerialized] [HideInInspector] public Vector3 headPosition;

            /// <summary>
            /// Position offset of the head. Will be applied on top of head target position and reset to Vector3.zero after each update.
            /// </summary>
            [NonSerialized] [HideInInspector] public Vector3 headPositionOffset;

            private Quaternion headRotation = Quaternion.identity;

            /// <summary>
            /// Rotation offset of the head. Will be applied on top of head target rotation and reset to Quaternion.identity after each update.
            /// </summary>
            [NonSerialized] [HideInInspector] public Quaternion headRotationOffset = Quaternion.identity;

            [Tooltip("The head target.")]
            /// <summary>
            /// The head target.
            /// </summary>
            public Transform headTarget;

            /// <summary>
            /// Target position of the head. Will be overwritten if target is assigned.
            /// </summary>
            [NonSerialized] [HideInInspector] public Vector3 IKPositionHead;

            /// <summary>
            /// Target position of the pelvis. Will be overwritten if target is assigned.
            /// </summary>
            [NonSerialized] [HideInInspector] public Vector3 IKPositionPelvis;

            /// <summary>
            /// Target rotation of the head. Will be overwritten if target is assigned.
            /// </summary>
            [NonSerialized] [HideInInspector] public Quaternion IKRotationHead = Quaternion.identity;

            /// <summary>
            /// Target rotation of the pelvis. Will be overwritten if target is assigned.
            /// </summary>
            [NonSerialized] [HideInInspector] public Quaternion IKRotationPelvis = Quaternion.identity;

            private float length;
            [NonSerialized] [HideInInspector] public Vector3 locomotionHeadPositionOffset;

            [Tooltip("How much will the pelvis maintain it's animated position?")]
            /// <summary>
            /// How much will the pelvis maintain it's animated position?
            /// </summary>
            [Range(0f, 1f)]
            public float maintainPelvisPosition = 0.2f;

            [Tooltip(
                "Will automatically rotate the root of the character if the head target has turned past this angle.")]
            /// <summary>
            /// Will automatically rotate the root of the character if the head target has turned past this angle.
            /// </summary>
            [Range(0f, 180f)]
            public float maxRootAngle = 25f;

            [Tooltip("Minimum height of the head from the root of the character.")]
            /// <summary>
            /// Minimum height of the head from the root of the character.
            /// </summary>
            public float minHeadHeight = 0.8f;

            [Tooltip(
                "Moves the body horizontally along -character.forward axis by that value when the player is crouching.")]
            /// <summary>
            /// Moves the body horizontally along -character.forward axis by that value when the player is crouching.
            /// </summary>
            public float moveBodyBackWhenCrouching = 0.5f;

            [Tooltip("Determines how much the chest will rotate to the rotation of the head.")]
            /// <summary>
            /// Determines how much the chest will rotate to the rotation of the head.
            /// </summary>
            [FormerlySerializedAs("chestRotationWeight")]
            [Range(0f, 1f)]
            public float neckStiffness = 0.2f;

            private Quaternion pelvisDeltaRotation = Quaternion.identity;
            private readonly int pelvisIndex = 0;
            private readonly int spineIndex = 1;
            private int chestIndex = -1, neckIndex = -1, headIndex = -1;

            /// <summary>
            /// Position offset of the pelvis. Will be applied on top of pelvis target position and reset to Vector3.zero after each update.
            /// </summary>
            [NonSerialized] [HideInInspector] public Vector3 pelvisPositionOffset;

            [Tooltip("Positional weight of the pelvis target.")]
            /// <summary>
            /// Positional weight of the pelvis target.
            /// </summary>
            [Range(0f, 1f)]
            public float pelvisPositionWeight;

            private Quaternion pelvisRelativeRotation = Quaternion.identity;

            /// <summary>
            /// Rotation offset of the pelvis. Will be reset to Quaternion.identity after each update.
            /// </summary>
            [NonSerialized] [HideInInspector] public Quaternion pelvisRotationOffset = Quaternion.identity;

            [Tooltip("Rotational weight of the pelvis target.")]
            /// <summary>
            /// Rotational weight of the pelvis target.
            /// </summary>
            [Range(0f, 1f)]
            public float pelvisRotationWeight;

            [Tooltip("The pelvis target, useful with seated rigs.")]
            /// <summary>
            /// The pelvis target, useful with seated rigs.
            /// </summary>
            public Transform pelvisTarget;

            [Tooltip("Positional weight of the head target.")]
            /// <summary>
            /// Positional weight of the head target.
            /// </summary>
            [Range(0f, 1f)]
            public float positionWeight = 1f;

            [Tooltip("The amount of rotation applied to the chest based on hand positions.")]
            /// <summary>
            /// The amount of rotation applied to the chest based on hand positions.
            /// </summary>
            [Range(0f, 1f)]
            public float rotateChestByHands = 1f;

            [Tooltip("Rotational weight of the head target.")]
            /// <summary>
            /// Rotational weight of the head target.
            /// </summary>
            [Range(0f, 1f)]
            public float rotationWeight = 1f;

            private float sizeMlp;

            public VirtualBone pelvis => bones[pelvisIndex];
            public VirtualBone firstSpineBone => bones[spineIndex];

            public VirtualBone chest
            {
                get
                {
                    if (hasChest) return bones[chestIndex];
                    return bones[spineIndex];
                }
            }

            private VirtualBone neck => bones[neckIndex];
            public VirtualBone head => bones[headIndex];

            public Quaternion anchorRotation { get; private set; }

            protected override void OnRead( Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck,
                bool hasShoulders, bool hasToes, int rootIndex, int index )
            {
                var pelvisPos = positions[index];
                var pelvisRot = rotations[index];
                var spinePos = positions[index + 1];
                var spineRot = rotations[index + 1];
                var chestPos = positions[index + 2];
                var chestRot = rotations[index + 2];
                var neckPos = positions[index + 3];
                var neckRot = rotations[index + 3];
                var headPos = positions[index + 4];
                var headRot = rotations[index + 4];

                if (!hasChest)
                {
                    chestPos = spinePos;
                    chestRot = spineRot;
                }

                if (!initiated)
                {
                    this.hasChest = hasChest;
                    this.hasNeck = hasNeck;
                    headHeight = V3Tools.ExtractVertical(headPos - positions[0], rotations[0] * Vector3.up, 1f)
                        .magnitude;

                    var boneCount = 3;
                    if (hasChest) boneCount++;
                    if (hasNeck) boneCount++;
                    bones = new VirtualBone[boneCount];

                    chestIndex = hasChest ? 2 : 1;

                    neckIndex = 1;
                    if (hasChest) neckIndex++;
                    if (hasNeck) neckIndex++;

                    headIndex = 2;
                    if (hasChest) headIndex++;
                    if (hasNeck) headIndex++;

                    bones[0] = new VirtualBone(pelvisPos, pelvisRot);
                    bones[1] = new VirtualBone(spinePos, spineRot);
                    if (hasChest) bones[chestIndex] = new VirtualBone(chestPos, chestRot);
                    if (hasNeck) bones[neckIndex] = new VirtualBone(neckPos, neckRot);
                    bones[headIndex] = new VirtualBone(headPos, headRot);

                    pelvisRotationOffset = Quaternion.identity;
                    chestRotationOffset = Quaternion.identity;
                    headRotationOffset = Quaternion.identity;

                    anchorRelativeToHead = Quaternion.Inverse(headRot) * rotations[0];

                    // Forward and up axes
                    pelvisRelativeRotation = Quaternion.Inverse(headRot) * pelvisRot;
                    chestRelativeRotation = Quaternion.Inverse(headRot) * chestRot;

                    chestForward = Quaternion.Inverse(chestRot) * (rotations[0] * Vector3.forward);

                    faceDirection = rotations[0] * Vector3.forward;

                    IKPositionHead = headPos;
                    IKRotationHead = headRot;
                    IKPositionPelvis = pelvisPos;
                    IKRotationPelvis = pelvisRot;
                    goalPositionChest = chestPos + rotations[0] * Vector3.forward;
                }

                bones[0].Read(pelvisPos, pelvisRot);
                bones[1].Read(spinePos, spineRot);
                if (hasChest) bones[chestIndex].Read(chestPos, chestRot);
                if (hasNeck) bones[neckIndex].Read(neckPos, neckRot);
                bones[headIndex].Read(headPos, headRot);

                var spineLength = Vector3.Distance(pelvisPos, headPos);
                sizeMlp = spineLength / 0.7f;
            }

            public override void PreSolve()
            {
                if (headTarget != null)
                {
                    IKPositionHead = headTarget.position;
                    IKRotationHead = headTarget.rotation;
                }

                if (chestGoal != null) goalPositionChest = chestGoal.position;

                if (pelvisTarget != null)
                {
                    IKPositionPelvis = pelvisTarget.position;
                    IKRotationPelvis = pelvisTarget.rotation;
                }

                headPosition = V3Tools.Lerp(head.solverPosition, IKPositionHead, positionWeight);
                headRotation = QuaTools.Lerp(head.solverRotation, IKRotationHead, rotationWeight);
            }

            public override void ApplyOffsets()
            {
                headPosition += headPositionOffset;

                var rootUp = rootRotation * Vector3.up;
                if (rootUp == Vector3.up)
                {
                    headPosition.y = Math.Max(rootPosition.y + minHeadHeight, headPosition.y);
                }
                else
                {
                    var toHead = headPosition - rootPosition;
                    var hor = V3Tools.ExtractHorizontal(toHead, rootUp, 1f);
                    var ver = toHead - hor;
                    var dot = Vector3.Dot(ver, rootUp);
                    if (dot > 0f)
                    {
                        if (ver.magnitude < minHeadHeight) ver = ver.normalized * minHeadHeight;
                    }
                    else
                    {
                        ver = -ver.normalized * minHeadHeight;
                    }

                    headPosition = rootPosition + hor + ver;
                }

                headRotation = headRotationOffset * headRotation;

                headDeltaPosition = headPosition - head.solverPosition;
                pelvisDeltaRotation =
                    QuaTools.FromToRotation(pelvis.solverRotation, headRotation * pelvisRelativeRotation);

                anchorRotation = headRotation * anchorRelativeToHead;
            }

            private void CalculateChestTargetRotation( VirtualBone rootBone, Arm[] arms )
            {
                chestTargetRotation = headRotation * chestRelativeRotation;

                // Use hands to adjust c
                AdjustChestByHands(ref chestTargetRotation, arms);

                faceDirection = Vector3.Cross(anchorRotation * Vector3.right, rootBone.readRotation * Vector3.up) +
                                anchorRotation * Vector3.forward;
            }

            public void Solve( VirtualBone rootBone, Leg[] legs, Arm[] arms )
            {
                CalculateChestTargetRotation(rootBone, arms);

                // Root rotation
                if (maxRootAngle < 180f)
                {
                    var faceDirLocal = Quaternion.Inverse(rootBone.solverRotation) * faceDirection;
                    var angle = Mathf.Atan2(faceDirLocal.x, faceDirLocal.z) * Mathf.Rad2Deg;

                    var rotation = 0f;
                    var maxAngle = maxRootAngle;

                    if (angle > maxAngle) rotation = angle - maxAngle;
                    if (angle < -maxAngle) rotation = angle + maxAngle;

                    rootBone.solverRotation = Quaternion.AngleAxis(rotation, rootBone.readRotation * Vector3.up) *
                                              rootBone.solverRotation;
                }


                var animatedPelvisPos = pelvis.solverPosition;
                var rootUp = rootBone.solverRotation * Vector3.up;

                // Translate pelvis to make the head's position & rotation match with the head target
                TranslatePelvis(legs, headDeltaPosition, pelvisDeltaRotation);

                // Solve a FABRIK pass to squash/stretch the spine
                FABRIKPass(animatedPelvisPos, rootUp);

                // Bend the spine to look towards chest target rotation
                Bend(bones, pelvisIndex, chestIndex, chestTargetRotation, chestRotationOffset, chestClampWeight, false,
                    neckStiffness);

                if (chestGoalWeight > 0f)
                {
                    var c = Quaternion.FromToRotation(bones[chestIndex].solverRotation * chestForward,
                                goalPositionChest - bones[chestIndex].solverPosition) *
                            bones[chestIndex].solverRotation;
                    Bend(bones, pelvisIndex, chestIndex, c, chestRotationOffset, chestClampWeight, false,
                        chestGoalWeight);
                }

                InverseTranslateToHead(legs, false, false, Vector3.zero, 1f);

                FABRIKPass(animatedPelvisPos, rootUp);

                Bend(bones, neckIndex, headIndex, headRotation, headClampWeight, true, 1f);

                SolvePelvis();
            }

            private void FABRIKPass( Vector3 animatedPelvisPos, Vector3 rootUp )
            {
                var startPos = Vector3.Lerp(pelvis.solverPosition, animatedPelvisPos, maintainPelvisPosition) +
                               pelvisPositionOffset - chestPositionOffset;
                var endPos = headPosition - chestPositionOffset;
                var startOffset = rootUp * (bones[bones.Length - 1].solverPosition - bones[0].solverPosition).magnitude;

                VirtualBone.SolveFABRIK(bones, startPos, endPos, 1f, 1f, 1, mag, startOffset);
            }

            private void SolvePelvis()
            {
                // Pelvis target
                if (pelvisPositionWeight > 0f)
                {
                    var headSolverRotation = head.solverRotation;

                    var delta = (IKPositionPelvis + pelvisPositionOffset - pelvis.solverPosition) *
                                pelvisPositionWeight;
                    foreach (var bone in bones) bone.solverPosition += delta;

                    var bendNormal = anchorRotation * Vector3.right;

                    if (hasChest && hasNeck)
                    {
                        VirtualBone.SolveTrigonometric(bones, pelvisIndex, spineIndex, headIndex, headPosition,
                            bendNormal, pelvisPositionWeight * 0.6f);
                        VirtualBone.SolveTrigonometric(bones, spineIndex, chestIndex, headIndex, headPosition,
                            bendNormal, pelvisPositionWeight * 0.6f);
                        VirtualBone.SolveTrigonometric(bones, chestIndex, neckIndex, headIndex, headPosition,
                            bendNormal, pelvisPositionWeight * 1f);
                    }
                    else if (hasChest && !hasNeck)
                    {
                        VirtualBone.SolveTrigonometric(bones, pelvisIndex, spineIndex, headIndex, headPosition,
                            bendNormal, pelvisPositionWeight * 0.75f);
                        VirtualBone.SolveTrigonometric(bones, spineIndex, chestIndex, headIndex, headPosition,
                            bendNormal, pelvisPositionWeight * 1f);
                    }
                    else if (!hasChest && hasNeck)
                    {
                        VirtualBone.SolveTrigonometric(bones, pelvisIndex, spineIndex, headIndex, headPosition,
                            bendNormal, pelvisPositionWeight * 0.75f);
                        VirtualBone.SolveTrigonometric(bones, spineIndex, neckIndex, headIndex, headPosition,
                            bendNormal, pelvisPositionWeight * 1f);
                    }
                    else if (!hasNeck && !hasChest)
                    {
                        VirtualBone.SolveTrigonometric(bones, pelvisIndex, spineIndex, headIndex, headPosition,
                            bendNormal, pelvisPositionWeight);
                    }

                    head.solverRotation = headSolverRotation;
                }
            }

            public override void Write( ref Vector3[] solvedPositions, ref Quaternion[] solvedRotations )
            {
                // Pelvis
                solvedPositions[index] = bones[0].solverPosition;
                solvedRotations[index] = bones[0].solverRotation;

                // Spine
                solvedRotations[index + 1] = bones[1].solverRotation;

                // Chest
                if (hasChest) solvedRotations[index + 2] = bones[chestIndex].solverRotation;

                // Neck
                if (hasNeck) solvedRotations[index + 3] = bones[neckIndex].solverRotation;

                // Head
                solvedRotations[index + 4] = bones[headIndex].solverRotation;
            }

            public override void ResetOffsets()
            {
                // Reset offsets to zero
                pelvisPositionOffset = Vector3.zero;
                chestPositionOffset = Vector3.zero;
                headPositionOffset = locomotionHeadPositionOffset; // Vector3.zero;
                pelvisRotationOffset = Quaternion.identity;
                chestRotationOffset = Quaternion.identity;
                headRotationOffset = Quaternion.identity;
            }

            private void AdjustChestByHands( ref Quaternion chestTargetRotation, Arm[] arms )
            {
                var h = Quaternion.Inverse(anchorRotation);

                var pLeft = h * (arms[0].position - headPosition) / sizeMlp;
                var pRight = h * (arms[1].position - headPosition) / sizeMlp;

                var c = Vector3.forward;
                c.x += pLeft.x * Mathf.Abs(pLeft.x);
                c.x += pLeft.z * Mathf.Abs(pLeft.z);
                c.x += pRight.x * Mathf.Abs(pRight.x);
                c.x -= pRight.z * Mathf.Abs(pRight.z);
                c.x *= 5f * rotateChestByHands;

                var angle = Mathf.Atan2(c.x, c.z) * Mathf.Rad2Deg;
                var q = Quaternion.AngleAxis(angle, rootRotation * Vector3.up);

                chestTargetRotation = q * chestTargetRotation;

                var t = Vector3.up;
                t.x += pLeft.y;
                t.x -= pRight.y;
                t.x *= 0.5f * rotateChestByHands;

                angle = Mathf.Atan2(t.x, t.y) * Mathf.Rad2Deg;
                q = Quaternion.AngleAxis(angle, rootRotation * Vector3.back);

                chestTargetRotation = q * chestTargetRotation;
            }

            // Move the pelvis so that the head would remain fixed to the anchor
            public void InverseTranslateToHead( Leg[] legs, bool limited, bool useCurrentLegMag, Vector3 offset,
                float w )
            {
                var delta = (headPosition + offset - head.solverPosition) * w * (1f - pelvisPositionWeight);

                var p = pelvis.solverPosition + delta;
                MovePosition(limited ? LimitPelvisPosition(legs, p, useCurrentLegMag) : p);
            }

            // Move and rotate the pelvis
            private void TranslatePelvis( Leg[] legs, Vector3 deltaPosition, Quaternion deltaRotation )
            {
                // Rotation
                var p = head.solverPosition;

                deltaRotation = QuaTools.ClampRotation(deltaRotation, chestClampWeight, 2);

                var r = Quaternion.Slerp(Quaternion.identity, deltaRotation, bodyRotStiffness);
                r = Quaternion.Slerp(r, QuaTools.FromToRotation(pelvis.solverRotation, IKRotationPelvis),
                    pelvisRotationWeight);
                VirtualBone.RotateAroundPoint(bones, 0, pelvis.solverPosition, pelvisRotationOffset * r);

                deltaPosition -= head.solverPosition - p;

                // Position
                // Move the body back when head is moving down
                var m = rootRotation * Vector3.forward;
                var deltaY = V3Tools.ExtractVertical(deltaPosition, rootRotation * Vector3.up, 1f).magnitude;
                var backOffset = deltaY * -moveBodyBackWhenCrouching * headHeight;
                deltaPosition += m * backOffset;

                /*
                if (backOffset < 0f) {
                    foreach (Leg leg in legs) leg.heelPositionOffset += Vector3.up * backOffset * backOffset; // TODO Ignoring root rotation
                }
                */

                MovePosition(LimitPelvisPosition(legs, pelvis.solverPosition + deltaPosition * bodyPosStiffness,
                    false));
            }

            // Limit the position of the pelvis so that the feet/toes would remain fixed
            private Vector3 LimitPelvisPosition( Leg[] legs, Vector3 pelvisPosition, bool useCurrentLegMag, int it = 2 )
            {
                // Cache leg current mag
                if (useCurrentLegMag)
                    foreach (var leg in legs)
                        leg.currentMag = Vector3.Distance(leg.thigh.solverPosition, leg.lastBone.solverPosition);

                // Solve a 3-point constraint
                for (var i = 0; i < it; i++)
                    foreach (var leg in legs)
                    {
                        var delta = pelvisPosition - pelvis.solverPosition;
                        var wantedThighPos = leg.thigh.solverPosition + delta;
                        var toWantedThighPos = wantedThighPos - leg.position;
                        var maxMag = useCurrentLegMag ? leg.currentMag : leg.mag;
                        var limitedThighPos = leg.position + Vector3.ClampMagnitude(toWantedThighPos, maxMag);
                        pelvisPosition += limitedThighPos - wantedThighPos;

                        // TODO rotate pelvis to accommodate, rotate the spine back then
                    }

                return pelvisPosition;
            }

            // Bending the spine to the head effector
            private void Bend( VirtualBone[] bones, int firstIndex, int lastIndex, Quaternion targetRotation,
                float clampWeight, bool uniformWeight, float w )
            {
                if (w <= 0f) return;
                if (bones.Length == 0) return;
                var bonesCount = lastIndex + 1 - firstIndex;
                if (bonesCount < 1) return;

                var r = QuaTools.FromToRotation(bones[lastIndex].solverRotation, targetRotation);
                r = QuaTools.ClampRotation(r, clampWeight, 2);

                var step = uniformWeight ? 1f / bonesCount : 0f;

                for (var i = firstIndex; i < lastIndex + 1; i++)
                {
                    if (!uniformWeight) step = Mathf.Clamp((i - firstIndex + 1) / bonesCount, 0, 1f);
                    VirtualBone.RotateAroundPoint(bones, i, bones[i].solverPosition,
                        Quaternion.Slerp(Quaternion.identity, r, step * w));
                }
            }

            // Bending the spine to the head effector
            private void Bend( VirtualBone[] bones, int firstIndex, int lastIndex, Quaternion targetRotation,
                Quaternion rotationOffset, float clampWeight, bool uniformWeight, float w )
            {
                if (w <= 0f) return;
                if (bones.Length == 0) return;
                var bonesCount = lastIndex + 1 - firstIndex;
                if (bonesCount < 1) return;

                var r = QuaTools.FromToRotation(bones[lastIndex].solverRotation, targetRotation);
                r = QuaTools.ClampRotation(r, clampWeight, 2);

                var step = uniformWeight ? 1f / bonesCount : 0f;

                for (var i = firstIndex; i < lastIndex + 1; i++)
                {
                    if (!uniformWeight) step = Mathf.Clamp((i - firstIndex + 1) / bonesCount, 0, 1f);
                    VirtualBone.RotateAroundPoint(bones, i, bones[i].solverPosition,
                        Quaternion.Slerp(Quaternion.Slerp(Quaternion.identity, rotationOffset, step), r, step * w));
                }
            }
        }
    }
}