using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// Head effector for FBBIK.
    /// </summary>
    public class FBBIKHeadEffector : MonoBehaviour
    {
        [Tooltip("The bones to use for bending.")]
        public BendBone[] bendBones = new BendBone[0];

        private int bendBonesCount;

        [Tooltip(
            "The master weight of bending/twisting the spine to the rotation of the head effector. This is similar to CCD, but uses the rotation of the head effector not the position.")]
        [Range(0f, 1f)]
        public float bendWeight = 1f;

        [Tooltip("Clamping the rotation of the body")] [Range(0f, 1f)]
        public float bodyClampWeight = 0.5f;

        [Tooltip("The weight of moving the body along with the head")] [Range(0f, 1f)]
        public float bodyWeight = 0.8f;

        [Tooltip("Bones to use for the CCD pass. Assign spine and/or neck bones.")]
        public Transform[] CCDBones = new Transform[0];

        private int ccdBonesCount;
        private Quaternion[] ccdDefaultLocalRotations = new Quaternion[0];

        [LargeHeader("CCD")]
        [Tooltip(
            "Optional. The master weight of the CCD (Cyclic Coordinate Descent) IK effect that bends the spine towards the head effector before FBBIK solves.")]
        [Range(0f, 1f)]
        public float CCDWeight = 1f;

        public Transform[] chestBones = new Transform[0];
        private int chestBonesCount;

        [LargeHeader("Chest Direction")] public Vector3 chestDirection = Vector3.forward;

        [Range(0f, 1f)] public float chestDirectionWeight = 1f;
        private Vector3[] chestLocalPositions = new Vector3[0];
        private Quaternion[] chestLocalRotations = new Quaternion[0];
        private Quaternion chestRotation;

        [Tooltip("Smoothing the CCD effect.")] [Range(0f, 1000f)]
        public float damper = 500f;

        [Tooltip(
            "If true, will fix head position to this Transform no matter what. Good for making sure the head will not budge away from the VR headset")]
        public bool fixHead;

        [Tooltip(
            "If false, hands will not pull the head away if they are too far. Disabling this will improve performance significantly.")]
        public bool handsPullBody = true;

        [Tooltip("Clamping the rotation of the head")] [Range(0f, 1f)]
        public float headClampWeight = 0.5f;

        private Vector3 headLocalPosition;
        private Quaternion headLocalRotation;
        private Quaternion headRotationRelativeToRoot;

        [Tooltip("Reference to the FBBIK component.")]
        public FullBodyBipedIK ik;

        [Tooltip("Stretch magnitude limit.")] public float maxStretch = 0.1f;

        private Vector3 offset,
            headToBody,
            shoulderCenterToHead,
            headToLeftThigh,
            headToRightThigh,
            leftShoulderPos,
            rightShoulderPos;

        public IKSolver.UpdateDelegate OnPostHeadEffectorFK;

        [LargeHeader("Position")] [Tooltip("Master weight for positioning the head.")] [Range(0f, 1f)]
        public float positionWeight = 1f;

        [LargeHeader("Stretching")]
        [Tooltip(
            "Stretching the spine/neck to help reach the target. This is useful for making sure the head stays locked relative to the VR headset. NB! Stretching is done after FBBIK has solved so if you have the hand effectors pinned and spine bones included in the 'Stretch Bones', the hands might become offset from their target positions.")]
        [Range(0f, 1f)]
        public float postStretchWeight = 1f;

        [Tooltip("The weight of rolling the bones in towards the target")] [Range(0f, 1f)]
        public float roll;

        [LargeHeader("Rotation")] [Tooltip("The weight of rotating the head bone after solving")] [Range(0f, 1f)]
        public float rotationWeight;

        private float shoulderDist, leftShoulderDist, rightShoulderDist;

        [Tooltip("Bones to use for stretching. The more bones you add, the less noticable the effect.")]
        public Transform[] stretchBones = new Transform[0];

        private int stretchBonesCount;

        [Tooltip("If > 0, dampers the stretching effect.")]
        public float stretchDamper;

        private Vector3[] stretchLocalPositions = new Vector3[0];
        private Quaternion[] stretchLocalRotations = new Quaternion[0];

        [Tooltip("The weight of moving the thighs along with the head")] [Range(0f, 1f)]
        public float thighWeight = 0.8f;

        // Register to get messages from the FBBIK
        private void Start()
        {
            ik.solver.OnPreRead += OnPreRead;
            ik.solver.OnPreIteration += Iterate;
            ik.solver.OnPostUpdate += OnPostUpdate;
            ik.solver.OnStoreDefaultLocalState += OnStoreDefaultLocalState;
            ik.solver.OnFixTransforms += OnFixTransforms;

            OnStoreDefaultLocalState();

            headRotationRelativeToRoot = Quaternion.Inverse(ik.references.root.rotation) * ik.references.head.rotation;
        }

        // Store the default local positions and rotations of the bones used by this head effector.
        private void OnStoreDefaultLocalState()
        {
            foreach (var bendBone in bendBones)
                if (bendBone != null)
                    bendBone.StoreDefaultLocalState();

            ccdDefaultLocalRotations = new Quaternion[CCDBones.Length];
            for (var i = 0; i < CCDBones.Length; i++)
                if (CCDBones[i] != null)
                    ccdDefaultLocalRotations[i] = CCDBones[i].localRotation;

            headLocalPosition = ik.references.head.localPosition;
            headLocalRotation = ik.references.head.localRotation;

            stretchLocalPositions = new Vector3[stretchBones.Length];
            stretchLocalRotations = new Quaternion[stretchBones.Length];
            for (var i = 0; i < stretchBones.Length; i++)
                if (stretchBones[i] != null)
                {
                    stretchLocalPositions[i] = stretchBones[i].localPosition;
                    stretchLocalRotations[i] = stretchBones[i].localRotation;
                }

            chestLocalPositions = new Vector3[chestBones.Length];
            chestLocalRotations = new Quaternion[chestBones.Length];
            for (var i = 0; i < chestBones.Length; i++)
                if (chestBones[i] != null)
                {
                    chestLocalPositions[i] = chestBones[i].localPosition;
                    chestLocalRotations[i] = chestBones[i].localRotation;
                }

            bendBonesCount = bendBones.Length;
            ccdBonesCount = CCDBones.Length;
            stretchBonesCount = stretchBones.Length;
            chestBonesCount = chestBones.Length;
        }

        // Fix the bones used by this head effector to their default local state
        private void OnFixTransforms()
        {
            if (!enabled) return;

            foreach (var bendBone in bendBones)
                if (bendBone != null)
                    bendBone.FixTransforms();

            for (var i = 0; i < CCDBones.Length; i++)
                if (CCDBones[i] != null)
                    CCDBones[i].localRotation = ccdDefaultLocalRotations[i];

            ik.references.head.localPosition = headLocalPosition;
            ik.references.head.localRotation = headLocalRotation;

            for (var i = 0; i < stretchBones.Length; i++)
                if (stretchBones[i] != null)
                {
                    stretchBones[i].localPosition = stretchLocalPositions[i];
                    stretchBones[i].localRotation = stretchLocalRotations[i];
                }

            for (var i = 0; i < chestBones.Length; i++)
                if (chestBones[i] != null)
                {
                    chestBones[i].localPosition = chestLocalPositions[i];
                    chestBones[i].localRotation = chestLocalRotations[i];
                }
        }

        // Called by the FBBIK each time before it reads the pose
        private void OnPreRead()
        {
            if (!enabled) return;
            if (!gameObject.activeInHierarchy) return;

            if (ik.solver.iterations == 0) return;

            ik.solver.FABRIKPass = handsPullBody;

            if (bendBonesCount != bendBones.Length || ccdBonesCount != CCDBones.Length ||
                stretchBonesCount != stretchBones.Length ||
                chestBonesCount != chestBones.Length) OnStoreDefaultLocalState();

            // Chest direction
            ChestDirection();

            // Spine Bend
            SpineBend();

            // CCD
            CCDPass();

            // Body
            offset = transform.position - ik.references.head.position;

            shoulderDist = Vector3.Distance(ik.references.leftUpperArm.position, ik.references.rightUpperArm.position);
            leftShoulderDist = Vector3.Distance(ik.references.head.position, ik.references.leftUpperArm.position);
            rightShoulderDist = Vector3.Distance(ik.references.head.position, ik.references.rightUpperArm.position);

            headToBody = ik.solver.rootNode.position - ik.references.head.position;
            headToLeftThigh = ik.references.leftThigh.position - ik.references.head.position;
            headToRightThigh = ik.references.rightThigh.position - ik.references.head.position;

            leftShoulderPos = ik.references.leftUpperArm.position + offset * bodyWeight;
            rightShoulderPos = ik.references.rightUpperArm.position + offset * bodyWeight;

            chestRotation = Quaternion.LookRotation(ik.references.head.position - ik.references.leftUpperArm.position,
                ik.references.rightUpperArm.position - ik.references.leftUpperArm.position);

            if (OnPostHeadEffectorFK != null) OnPostHeadEffectorFK();
        }

        // Bending the spine to the head effector
        private void SpineBend()
        {
            var w = bendWeight * ik.solver.IKPositionWeight;

            if (w <= 0f) return;
            if (bendBones.Length == 0) return;

            var rotation = transform.rotation *
                           Quaternion.Inverse(ik.references.root.rotation * headRotationRelativeToRoot);
            rotation = QuaTools.ClampRotation(rotation, bodyClampWeight, 2);

            var step = 1f / bendBones.Length;

            for (var i = 0; i < bendBones.Length; i++)
                if (bendBones[i].transform != null)
                    bendBones[i].transform.rotation =
                        Quaternion.Lerp(Quaternion.identity, rotation, step * bendBones[i].weight * w) *
                        bendBones[i].transform.rotation;
        }

        // Single CCD pass to make the spine less stiff 
        private void CCDPass()
        {
            var w = CCDWeight * ik.solver.IKPositionWeight;

            if (w <= 0f) return;

            for (var i = CCDBones.Length - 1; i > -1; i--)
            {
                var r = Quaternion.FromToRotation(ik.references.head.position - CCDBones[i].position,
                            transform.position - CCDBones[i].position) * CCDBones[i].rotation;
                var d = Mathf.Lerp((CCDBones.Length - i) / CCDBones.Length, 1f, roll);
                var a = Quaternion.Angle(Quaternion.identity, r);

                a = Mathf.Lerp(0f, a, (damper - a) / damper);

                CCDBones[i].rotation = Quaternion.RotateTowards(CCDBones[i].rotation, r, a * w * d);
            }
        }

        //private float leftArmLength;

        // Called by the FBBIK before each solver iteration
        private void Iterate( int iteration )
        {
            if (!enabled) return;
            if (!gameObject.activeInHierarchy) return;

            if (ik.solver.iterations == 0) return;

            // Shoulders
            leftShoulderPos = transform.position + (leftShoulderPos - transform.position).normalized * leftShoulderDist;
            rightShoulderPos = transform.position +
                               (rightShoulderPos - transform.position).normalized * rightShoulderDist;

            Solve(ref leftShoulderPos, ref rightShoulderPos, shoulderDist);

            LerpSolverPosition(ik.solver.leftShoulderEffector, leftShoulderPos,
                positionWeight * ik.solver.IKPositionWeight, ik.solver.leftShoulderEffector.positionOffset);
            LerpSolverPosition(ik.solver.rightShoulderEffector, rightShoulderPos,
                positionWeight * ik.solver.IKPositionWeight, ik.solver.rightShoulderEffector.positionOffset);

            // Body
            var chestRotationSolved =
                Quaternion.LookRotation(transform.position - leftShoulderPos, rightShoulderPos - leftShoulderPos);
            var rBody = QuaTools.FromToRotation(chestRotation, chestRotationSolved);

            var headToBodySolved = rBody * headToBody;
            LerpSolverPosition(ik.solver.bodyEffector, transform.position + headToBodySolved,
                positionWeight * ik.solver.IKPositionWeight,
                ik.solver.bodyEffector.positionOffset - ik.solver.pullBodyOffset);

            // Thighs
            var rThighs = Quaternion.Lerp(Quaternion.identity, rBody, thighWeight);

            var headToLeftThighSolved = rThighs * headToLeftThigh;
            var headToRightThighSolved = rThighs * headToRightThigh;


            LerpSolverPosition(ik.solver.leftThighEffector, transform.position + headToLeftThighSolved,
                positionWeight * ik.solver.IKPositionWeight,
                ik.solver.bodyEffector.positionOffset - ik.solver.pullBodyOffset +
                ik.solver.leftThighEffector.positionOffset);
            LerpSolverPosition(ik.solver.rightThighEffector, transform.position + headToRightThighSolved,
                positionWeight * ik.solver.IKPositionWeight,
                ik.solver.bodyEffector.positionOffset - ik.solver.pullBodyOffset +
                ik.solver.rightThighEffector.positionOffset);
        }

        // Called by the FBBIK each time it is finished updating
        private void OnPostUpdate()
        {
            if (!enabled) return;
            if (!gameObject.activeInHierarchy) return;

            // Stretching the spine and neck
            PostStretching();

            // Rotate the head bone
            var headRotation = QuaTools.FromToRotation(ik.references.head.rotation, transform.rotation);
            headRotation = QuaTools.ClampRotation(headRotation, headClampWeight, 2);

            ik.references.head.rotation =
                Quaternion.Lerp(Quaternion.identity, headRotation, rotationWeight * ik.solver.IKPositionWeight) *
                ik.references.head.rotation;
        }

        private void ChestDirection()
        {
            var w = chestDirectionWeight * ik.solver.IKPositionWeight;
            if (w <= 0f) return;

            var changed = false;
            chestDirection = V3Tools.ClampDirection(chestDirection, ik.references.root.forward, 0.45f, 2, out changed);

            if (chestDirection == Vector3.zero) return;

            var q = Quaternion.FromToRotation(ik.references.root.forward, chestDirection);
            q = Quaternion.Lerp(Quaternion.identity, q, w * (1f / chestBones.Length));

            foreach (var bone in chestBones) bone.rotation = q * bone.rotation;
        }

        // Stretching the spine/neck to help reach the target. This is most useful for making sure the head stays locked relative to the VR controller
        private void PostStretching()
        {
            var w = postStretchWeight * ik.solver.IKPositionWeight;

            if (w > 0f)
            {
                var stretch = Vector3.ClampMagnitude(transform.position - ik.references.head.position, maxStretch);
                stretch *= w;

                stretchDamper = Mathf.Max(stretchDamper, 0f);
                if (stretchDamper > 0f) stretch /= (1f + stretch.magnitude) * (1f + stretchDamper);

                for (var i = 0; i < stretchBones.Length; i++)
                    if (stretchBones[i] != null)
                        stretchBones[i].position += stretch / stretchBones.Length;
            }
            if (fixHead && ik.solver.IKPositionWeight > 0f) ik.references.head.position = transform.position;
        }

        // Interpolate the solver position of the effector
        private void LerpSolverPosition( IKEffector effector, Vector3 position, float weight, Vector3 offset )
        {
            effector.GetNode(ik.solver).solverPosition =
                Vector3.Lerp(effector.GetNode(ik.solver).solverPosition, position + offset, weight);
        }

        // Solve a simple linear constraint
        private void Solve( ref Vector3 pos1, ref Vector3 pos2, float nominalDistance )
        {
            var direction = pos2 - pos1;

            var distance = direction.magnitude;
            if (distance == nominalDistance) return;
            if (distance == 0f) return;

            var force = 1f;

            force *= 1f - nominalDistance / distance;

            var offset = direction * force * 0.5f;

            pos1 += offset;
            pos2 -= offset;
        }

        // Clean up the delegates
        private void OnDestroy()
        {
            if (ik != null)
            {
                ik.solver.OnPreRead -= OnPreRead;
                ik.solver.OnPreIteration -= Iterate;
                ik.solver.OnPostUpdate -= OnPostUpdate;
                ik.solver.OnStoreDefaultLocalState -= OnStoreDefaultLocalState;
                ik.solver.OnFixTransforms -= OnFixTransforms;
            }
        }

        [Serializable]
        public class BendBone
        {
            private Quaternion defaultLocalRotation = Quaternion.identity;

            [Tooltip("Assign spine and/or neck bones.")]
            public Transform transform;

            [Tooltip("The weight of rotating this bone.")] [Range(0f, 1f)]
            public float weight = 0.5f;

            public BendBone()
            {
            }

            public BendBone( Transform transform, float weight )
            {
                this.transform = transform;
                this.weight = weight;
            }

            public void StoreDefaultLocalState()
            {
                defaultLocalRotation = transform.localRotation;
            }

            public void FixTransforms()
            {
                transform.localRotation = defaultLocalRotation;
            }
        }
    }
}