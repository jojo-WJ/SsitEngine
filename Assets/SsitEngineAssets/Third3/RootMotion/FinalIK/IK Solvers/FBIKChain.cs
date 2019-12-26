using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// A chain of bones in IKSolverFullBody.
    /// </summary>
    [Serializable]
    public class FBIKChain
    {
        private const float maxLimbLength = 0.99999f;
        private float[] crossFades;
        private float distance;
        private bool initiated;
        private float length;
        private IKSolver.Point p;
        private float pullParentSum;
        private float reachForce;

        private float rootLength;
        private float sqrMag1, sqrMag2, sqrMagDif;

        public FBIKChain()
        {
        }

        public FBIKChain( float pin, float pull, params Transform[] nodeTransforms )
        {
            this.pin = pin;
            this.pull = pull;

            SetNodes(nodeTransforms);

            children = new int[0];
        }

        /*
         * Set nodes to the following bone transforms.
         * */
        public void SetNodes( params Transform[] boneTransforms )
        {
            nodes = new IKSolver.Node[boneTransforms.Length];
            for (var i = 0; i < boneTransforms.Length; i++) nodes[i] = new IKSolver.Node(boneTransforms[i]);
        }

        public int GetNodeIndex( Transform boneTransform )
        {
            for (var i = 0; i < nodes.Length; i++)
                if (nodes[i].transform == boneTransform)
                    return i;
            return -1;
        }

        /*
         * Check if this chain is valid or not.
         * */
        public bool IsValid( ref string message )
        {
            if (nodes.Length == 0)
            {
                message = "FBIK chain contains no nodes.";
                return false;
            }

            foreach (var node in nodes)
                if (node.transform == null)
                {
                    message = "Node transform is null in FBIK chain.";
                    return false;
                }

            return true;
        }

        /*
         * Initiating the chain.
         * */
        public void Initiate( IKSolverFullBody solver )
        {
            initiated = false;

            foreach (var node in nodes) node.solverPosition = node.transform.position;

            // Calculating bone lengths
            CalculateBoneLengths(solver);

            // Initiating child constraints
            foreach (var c in childConstraints) c.Initiate(solver);

            // Initiating the bend constraint
            if (nodes.Length == 3)
            {
                bendConstraint.SetBones(nodes[0].transform, nodes[1].transform, nodes[2].transform);
                bendConstraint.Initiate(solver);
            }

            crossFades = new float[children.Length];

            initiated = true;
        }

        /*
         * Before updating the chain
         * */
        public void ReadPose( IKSolverFullBody solver, bool fullBody )
        {
            if (!initiated) return;

            for (var i = 0; i < nodes.Length; i++)
                nodes[i].solverPosition = nodes[i].transform.position + nodes[i].offset;

            // Calculating bone lengths
            CalculateBoneLengths(solver);

            if (fullBody)
            {
                // Pre-update child constraints
                for (var i = 0; i < childConstraints.Length; i++) childConstraints[i].OnPreSolve(solver);

                if (children.Length > 0)
                {
                    // PullSum
                    var pullSum = nodes[nodes.Length - 1].effectorPositionWeight;
                    for (var i = 0; i < children.Length; i++)
                        pullSum += solver.chain[children[i]].nodes[0].effectorPositionWeight *
                                   solver.chain[children[i]].pull;
                    pullSum = Mathf.Clamp(pullSum, 1f, Mathf.Infinity);

                    // CrossFades
                    for (var i = 0; i < children.Length; i++)
                        crossFades[i] = solver.chain[children[i]].nodes[0].effectorPositionWeight *
                                        solver.chain[children[i]].pull / pullSum;
                }

                // Finding the total pull force by all child chains
                pullParentSum = 0f;
                for (var i = 0; i < children.Length; i++) pullParentSum += solver.chain[children[i]].pull;
                pullParentSum = Mathf.Clamp(pullParentSum, 1f, Mathf.Infinity);

                // Reach force
                if (nodes.Length == 3)
                    reachForce = reach * Mathf.Clamp(nodes[2].effectorPositionWeight, 0f, 1f);
                else reachForce = 0f;

                if (push > 0f && nodes.Length > 1)
                    distance = Vector3.Distance(nodes[0].transform.position,
                        nodes[nodes.Length - 1].transform.position);
            }
        }

        // Calculates all bone lengths as well as lenghts between the chains
        private void CalculateBoneLengths( IKSolverFullBody solver )
        {
            // Calculating bone lengths
            length = 0f;

            for (var i = 0; i < nodes.Length - 1; i++)
            {
                nodes[i].length = Vector3.Distance(nodes[i].transform.position, nodes[i + 1].transform.position);
                length += nodes[i].length;

                if (nodes[i].length == 0)
                {
                    Warning.Log(
                        "Bone " + nodes[i].transform.name + " - " + nodes[i + 1].transform.name +
                        " length is zero, can not solve.", nodes[i].transform);
                    return;
                }
            }

            for (var i = 0; i < children.Length; i++)
            {
                solver.chain[children[i]].rootLength =
                    (solver.chain[children[i]].nodes[0].transform.position - nodes[nodes.Length - 1].transform.position)
                    .magnitude;

                if (solver.chain[children[i]].rootLength == 0f) return;
            }

            if (nodes.Length == 3)
            {
                // Square magnitude of the limb lengths
                sqrMag1 = nodes[0].length * nodes[0].length;
                sqrMag2 = nodes[1].length * nodes[1].length;
                sqrMagDif = sqrMag1 - sqrMag2;
            }
        }

        /*
         * Interpolates the joint position to match the bone's length
        */
        private Vector3 SolveFABRIKJoint( Vector3 pos1, Vector3 pos2, float length )
        {
            return pos2 + (pos1 - pos2).normalized * length;
        }

        /*
         * Calculates the bend direction based on the law of cosines (from IKSolverTrigonometric). 
         * */
        protected Vector3 GetDirToBendPoint( Vector3 direction, Vector3 bendDirection, float directionMagnitude )
        {
            var x = (directionMagnitude * directionMagnitude + sqrMagDif) / 2f / directionMagnitude;
            var y = (float) Math.Sqrt(Mathf.Clamp(sqrMag1 - x * x, 0, Mathf.Infinity));

            if (direction == Vector3.zero) return Vector3.zero;
            return Quaternion.LookRotation(direction, bendDirection) * new Vector3(0f, y, x);
        }

        /*
         * Satisfying child constraints
         * */
        private void SolveChildConstraints( IKSolverFullBody solver )
        {
            for (var i = 0; i < childConstraints.Length; i++) childConstraints[i].Solve(solver);
        }

        /*
         * Solve simple linear constraint
         * */
        private void SolveLinearConstraint( IKSolver.Node node1, IKSolver.Node node2, float crossFade, float distance )
        {
            var dir = node2.solverPosition - node1.solverPosition;

            var mag = dir.magnitude;

            if (distance == mag) return;
            if (mag == 0f) return;

            var offset = dir * (1f - distance / mag);

            node1.solverPosition += offset * crossFade;
            node2.solverPosition -= offset * (1f - crossFade);
        }

        /*
         * FABRIK Forward reach
         * */
        public void ForwardReach( Vector3 position )
        {
            // Lerp last node's solverPosition to position
            nodes[nodes.Length - 1].solverPosition = position;

            for (var i = nodes.Length - 2; i > -1; i--) // Finding joint positions
                nodes[i].solverPosition =
                    SolveFABRIKJoint(nodes[i].solverPosition, nodes[i + 1].solverPosition, nodes[i].length);
        }

        /*
         * FABRIK Backward reach
         * */
        private void BackwardReach( Vector3 position )
        {
            // Solve forst node only if it already hasn't been solved in SolveConstraintSystems
            if (rootLength > 0) position = SolveFABRIKJoint(nodes[0].solverPosition, position, rootLength);
            nodes[0].solverPosition = position;

            // Finding joint positions
            for (var i = 1; i < nodes.Length; i++)
                nodes[i].solverPosition = SolveFABRIKJoint(nodes[i].solverPosition, nodes[i - 1].solverPosition,
                    nodes[i - 1].length);
        }

        #region Main Interface

        /// <summary>
        /// Linear constraint between child chains of a FBIKChain.
        /// </summary>
        [Serializable]
        public class ChildConstraint
        {
            /// <summary>
            /// The first bone.
            /// </summary>
            [SerializeField] private Transform bone1;

            /// <summary>
            /// The second bone.
            /// </summary>
            [SerializeField] private Transform bone2;

            private int chain1Index;
            private int chain2Index;

            // The crossFade value between the connected chains
            private float crossFade, inverseCrossFade;

            /// <summary>
            /// The pull elasticity.
            /// </summary>
            public float pullElasticity;

            /// <summary>
            /// The push elasticity.
            /// </summary>
            public float pushElasticity;

            /*
             * Constructor
             * */
            public ChildConstraint( Transform bone1, Transform bone2, float pushElasticity = 0f,
                float pullElasticity = 0f )
            {
                this.bone1 = bone1;
                this.bone2 = bone2;
                this.pushElasticity = pushElasticity;
                this.pullElasticity = pullElasticity;
            }

            // Gets the nominal (animated) distance between the two bones.
            public float nominalDistance { get; private set; }

            // The constraint is rigid if both push and pull elasticity are 0.
            public bool isRigid { get; private set; }

            /*
             * Initiating the constraint
             * */
            public void Initiate( IKSolverFullBody solver )
            {
                chain1Index = solver.GetChainIndex(bone1);
                chain2Index = solver.GetChainIndex(bone2);

                OnPreSolve(solver);
            }

            /*
             * Updating nominal distance because it might have changed in the animation
             * */
            public void OnPreSolve( IKSolverFullBody solver )
            {
                nominalDistance = Vector3.Distance(solver.chain[chain1Index].nodes[0].transform.position,
                    solver.chain[chain2Index].nodes[0].transform.position);

                isRigid = pushElasticity <= 0 && pullElasticity <= 0;

                // CrossFade
                if (isRigid)
                {
                    var offset = solver.chain[chain1Index].pull - solver.chain[chain2Index].pull;
                    crossFade = 1f - (0.5f + offset * 0.5f);
                }
                else
                {
                    crossFade = 0.5f;
                }

                inverseCrossFade = 1f - crossFade;
            }

            /*
             * Solving the constraint
             * */
            public void Solve( IKSolverFullBody solver )
            {
                if (pushElasticity >= 1 && pullElasticity >= 1) return;

                var direction = solver.chain[chain2Index].nodes[0].solverPosition -
                                solver.chain[chain1Index].nodes[0].solverPosition;

                var distance = direction.magnitude;
                if (distance == nominalDistance) return;
                if (distance == 0f) return;

                var force = 1f;

                if (!isRigid)
                {
                    var elasticity = distance > nominalDistance ? pullElasticity : pushElasticity;
                    force = 1f - elasticity;
                }

                force *= 1f - nominalDistance / distance;

                var offset = direction * force;

                solver.chain[chain1Index].nodes[0].solverPosition += offset * crossFade;
                solver.chain[chain2Index].nodes[0].solverPosition -= offset * inverseCrossFade;
            }
        }

        [Serializable]
        public enum Smoothing
        {
            None,
            Exponential,
            Cubic
        }

        /// <summary>
        /// The pin weight. If closer to 1, the chain will be less influenced by child chains.
        /// </summary>
        [Range(0f, 1f)] public float pin;

        /// <summary>
        /// The weight of pulling the parent chain.
        /// </summary>
        [Range(0f, 1f)] public float pull = 1f;

        /// <summary>
        /// The weight of the end-effector pushing the shoulder/thigh when the end-effector is close to it.
        /// </summary>
        [Range(0f, 1f)] public float push;

        /// <summary>
        /// The amount of push force transferred to the parent (from hand or foot to the body).
        /// </summary>
        [Range(-1f, 1f)] public float pushParent;

        /// <summary>
        /// Only used in 3 segmented chains, pulls the first node closer to the third node.
        /// </summary>
        [Range(0f, 1f)] public float reach = 0.1f;

        /// <summary>
        /// Smoothing the effect of the Reach with the expense of some accuracy.
        /// </summary>
        public Smoothing reachSmoothing = Smoothing.Exponential;

        /// <summary>
        /// Smoothing the effect of the Push.
        /// </summary>
        public Smoothing pushSmoothing = Smoothing.Exponential;

        /// <summary>
        /// The nodes in this chain.
        /// </summary>
        public IKSolver.Node[] nodes = new IKSolver.Node[0];

        /// <summary>
        /// The child chains.
        /// </summary>
        public int[] children = new int[0];

        /// <summary>
        /// The child constraints are used for example for fixing the distance between left upper arm and right upper arm
        /// </summary>
        public ChildConstraint[] childConstraints = new ChildConstraint[0];

        /// <summary>
        /// Gets the bend constraint (if this chain has 3 segments).
        /// </summary>
        /// <value>The bend constraint.</value>
        public IKConstraintBend bendConstraint = new IKConstraintBend();

        #endregion Main Interface

        #region Recursive Methods

        /*
         * Reaching limbs
         * */
        public void Reach( IKSolverFullBody solver )
        {
            if (!initiated) return;

            // Solve children first
            for (var i = 0; i < children.Length; i++) solver.chain[children[i]].Reach(solver);

            if (reachForce <= 0f) return;

            var solverDirection = nodes[2].solverPosition - nodes[0].solverPosition;
            if (solverDirection == Vector3.zero) return;

            var solverLength = solverDirection.magnitude;

            //Reaching
            var straight = solverDirection / solverLength * length;

            var delta = Mathf.Clamp(solverLength / length, 1 - reachForce, 1 + reachForce) - 1f;
            delta = Mathf.Clamp(delta + reachForce, -1f, 1f);

            // Smoothing the effect of Reach with the expense of some accuracy
            switch (reachSmoothing)
            {
                case Smoothing.Exponential:
                    delta *= delta;
                    break;
                case Smoothing.Cubic:
                    delta *= delta * delta;
                    break;
            }

            var offset = straight * Mathf.Clamp(delta, 0f, solverLength);
            nodes[0].solverPosition += offset * (1f - nodes[0].effectorPositionWeight);
            nodes[2].solverPosition += offset;
        }

        /*
         * End-effectors pushing the first nodes
         * */
        public Vector3 Push( IKSolverFullBody solver )
        {
            var sum = Vector3.zero;

            // Get the push from the children
            for (var i = 0; i < children.Length; i++)
                sum += solver.chain[children[i]].Push(solver) * solver.chain[children[i]].pushParent;

            // Apply the push from a child
            nodes[nodes.Length - 1].solverPosition += sum;

            // Calculating the push of THIS chain (passed on to the parent as we're in a recursive method)
            if (nodes.Length < 2) return Vector3.zero;
            if (push <= 0f) return Vector3.zero;

            var solverDirection = nodes[2].solverPosition - nodes[0].solverPosition;
            var solverLength = solverDirection.magnitude;
            if (solverLength == 0f) return Vector3.zero;

            // Get the push force factor
            var f = 1f - solverLength / distance;
            if (f <= 0f) return Vector3.zero;

            // Push smoothing
            switch (pushSmoothing)
            {
                case Smoothing.Exponential:
                    f *= f;
                    break;
                case Smoothing.Cubic:
                    f *= f * f;
                    break;
            }

            // The final push force
            var p = -solverDirection * f * push;

            nodes[0].solverPosition += p;
            return p;
        }

        /*
         * Applying trigonometric IK solver on the 3 segmented chains to relieve tension from the solver and increase accuracy.
         * */
        public void SolveTrigonometric( IKSolverFullBody solver, bool calculateBendDirection = false )
        {
            if (!initiated) return;

            // Solve children first
            for (var i = 0; i < children.Length; i++)
                solver.chain[children[i]].SolveTrigonometric(solver, calculateBendDirection);

            if (nodes.Length != 3) return;

            // Direction of the limb in solver
            var solverDirection = nodes[2].solverPosition - nodes[0].solverPosition;

            // Distance between the first and the last node solver positions
            var solverLength = solverDirection.magnitude;
            if (solverLength == 0f) return;

            // Maximim stretch of the limb
            var maxMag = Mathf.Clamp(solverLength, 0f, length * maxLimbLength);
            var direction = solverDirection / solverLength * maxMag;

            // Get the general world space bending direction
            var bendDirection = calculateBendDirection && bendConstraint.initiated
                ? bendConstraint.GetDir(solver)
                : nodes[1].solverPosition - nodes[0].solverPosition;

            // Get the direction to the trigonometrically solved position of the second node
            var toBendPoint = GetDirToBendPoint(direction, bendDirection, maxMag);

            // Position the second node
            nodes[1].solverPosition = nodes[0].solverPosition + toBendPoint;
        }

        /*
         * Stage 1 of the FABRIK algorithm
         * */
        public void Stage1( IKSolverFullBody solver )
        {
            // Stage 1
            for (var i = 0; i < children.Length; i++) solver.chain[children[i]].Stage1(solver);

            // If is the last chain in this hierarchy, solve immediatelly and return
            if (children.Length == 0)
            {
                ForwardReach(nodes[nodes.Length - 1].solverPosition);
                return;
            }

            var centroid = nodes[nodes.Length - 1].solverPosition;

            // Satisfying child constraints
            SolveChildConstraints(solver);

            // Finding the centroid position of all child chains according to their individual pull weights
            for (var i = 0; i < children.Length; i++)
            {
                var childPosition = solver.chain[children[i]].nodes[0].solverPosition;

                if (solver.chain[children[i]].rootLength > 0)
                    childPosition = SolveFABRIKJoint(nodes[nodes.Length - 1].solverPosition,
                        solver.chain[children[i]].nodes[0].solverPosition, solver.chain[children[i]].rootLength);

                if (pullParentSum > 0)
                    centroid += (childPosition - nodes[nodes.Length - 1].solverPosition) *
                                (solver.chain[children[i]].pull / pullParentSum);
            }

            // Forward reach to the centroid (unless pinned)
            ForwardReach(Vector3.Lerp(centroid, nodes[nodes.Length - 1].solverPosition, pin));
        }

        /*
         * Stage 2 of the FABRIK algorithm.
         * */
        public void Stage2( IKSolverFullBody solver, Vector3 position )
        {
            // Stage 2
            BackwardReach(position);

            var it = Mathf.Clamp(solver.iterations, 2, 4);

            // Iterating child constraints and child chains to make sure they are not conflicting
            if (childConstraints.Length > 0)
                for (var i = 0; i < it; i++)
                    SolveConstraintSystems(solver);

            // Stage 2 for the children
            for (var i = 0; i < children.Length; i++)
                solver.chain[children[i]].Stage2(solver, nodes[nodes.Length - 1].solverPosition);
        }

        /*
         * Iterating child constraints and child chains to make sure they are not conflicting
         * */
        public void SolveConstraintSystems( IKSolverFullBody solver )
        {
            // Satisfy child constraints
            SolveChildConstraints(solver);

            for (var i = 0; i < children.Length; i++)
                SolveLinearConstraint(nodes[nodes.Length - 1], solver.chain[children[i]].nodes[0], crossFades[i],
                    solver.chain[children[i]].rootLength);
        }

        #endregion Recursive Methods
    }
}