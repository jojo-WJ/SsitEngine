using UnityEngine;

namespace RootMotion.FinalIK
{
    public partial class Grounding
    {
        /// <summary>
        /// The %Grounding %Leg.
        /// </summary>
        public class Leg
        {
            private Grounding grounding;
            private RaycastHit heelHit;

            public bool invertFootCenter;
            private Vector3 lastPosition;
            private float lastTime, deltaTime;

            /// <summary>
            /// Gets the current rotation offset of the foot.
            /// </summary>
            public Quaternion rotationOffset = Quaternion.identity;

            private Quaternion toHitNormal, r;
            private Vector3 up = Vector3.up;

            /// <summary>
            /// Returns true distance from foot to ground is less that maxStep
            /// </summary>
            public bool isGrounded { get; private set; }

            /// <summary>
            /// Gets the current IK position of the foot.
            /// </summary>
            public Vector3 IKPosition { get; private set; }

            /// <summary>
            /// Returns true, if the leg is valid and initiated
            /// </summary>
            public bool initiated { get; private set; }

            /// <summary>
            /// The height of foot from ground.
            /// </summary>
            public float heightFromGround { get; private set; }

            /// <summary>
            /// Velocity of the foot
            /// </summary>
            public Vector3 velocity { get; private set; }

            /// <summary>
            /// Gets the foot Transform.
            /// </summary>
            public Transform transform { get; private set; }

            /// <summary>
            /// Gets the current IK offset.
            /// </summary>
            public float IKOffset { get; private set; }

            // Gets the height from ground clamped between min and max step height
            public float stepHeightFromGround => Mathf.Clamp(heightFromGround, -grounding.maxStep, grounding.maxStep);

            // The foot's height from ground in the animation
            private float rootYOffset => grounding.GetVerticalOffset(transform.position,
                grounding.root.position - up * grounding.heightOffset);

            // Initiates the Leg
            public void Initiate( Grounding grounding, Transform transform )
            {
                initiated = false;
                this.grounding = grounding;
                this.transform = transform;
                up = Vector3.up;
                IKPosition = transform.position;
                rotationOffset = Quaternion.identity;

                initiated = true;
                OnEnable();
            }

            // Should be called each time the leg is (re)activated
            public void OnEnable()
            {
                if (!initiated) return;

                lastPosition = transform.position;
                lastTime = Time.deltaTime;
            }

            // Set everything to 0
            public void Reset()
            {
                lastPosition = transform.position;
                lastTime = Time.deltaTime;
                IKOffset = 0f;
                IKPosition = transform.position;
                rotationOffset = Quaternion.identity;
            }

            // Raycasting, processing the leg's position
            public void Process()
            {
                if (!initiated) return;
                if (grounding.maxStep <= 0) return;

                deltaTime = Time.time - lastTime;
                lastTime = Time.time;
                if (deltaTime == 0f) return;

                up = grounding.up;
                heightFromGround = Mathf.Infinity;

                // Calculating velocity
                velocity = (transform.position - lastPosition) / deltaTime;
                velocity = grounding.Flatten(velocity);
                lastPosition = transform.position;

                var prediction = velocity * grounding.prediction;

                if (grounding.footRadius <= 0) grounding.quality = Quality.Fastest;

                // Raycasting
                switch (grounding.quality)
                {
                    // The fastest, single raycast
                    case Quality.Fastest:

                        var predictedHit = GetRaycastHit(prediction);
                        SetFootToPoint(predictedHit.normal, predictedHit.point);
                        break;

                    // Medium, 3 raycasts
                    case Quality.Simple:

                        heelHit = GetRaycastHit(Vector3.zero);
                        var f = grounding.GetFootCenterOffset();
                        if (invertFootCenter) f = -f;
                        var toeHit = GetRaycastHit(f + prediction);
                        var sideHit = GetRaycastHit(grounding.root.right * grounding.footRadius * 0.5f);

                        var planeNormal = Vector3.Cross(toeHit.point - heelHit.point, sideHit.point - heelHit.point)
                            .normalized;
                        if (Vector3.Dot(planeNormal, up) < 0) planeNormal = -planeNormal;

                        SetFootToPlane(planeNormal, heelHit.point, heelHit.point);
                        break;

                    // The slowest, raycast and a capsule cast
                    case Quality.Best:
                        heelHit = GetRaycastHit(invertFootCenter ? -grounding.GetFootCenterOffset() : Vector3.zero);
                        var capsuleHit = GetCapsuleHit(prediction);

                        SetFootToPlane(capsuleHit.normal, capsuleHit.point, heelHit.point);
                        break;
                }

                // Is the foot grounded?
                isGrounded = heightFromGround < grounding.maxStep;

                var offsetTarget = stepHeightFromGround;
                if (!grounding.rootGrounded) offsetTarget = 0f;

                IKOffset = Interp.LerpValue(IKOffset, offsetTarget, grounding.footSpeed, grounding.footSpeed);
                IKOffset = Mathf.Lerp(IKOffset, offsetTarget, deltaTime * grounding.footSpeed);

                var legHeight = grounding.GetVerticalOffset(transform.position, grounding.root.position);
                var currentMaxOffset = Mathf.Clamp(grounding.maxStep - legHeight, 0f, grounding.maxStep);

                IKOffset = Mathf.Clamp(IKOffset, -currentMaxOffset, IKOffset);

                RotateFoot();

                // Update IK values
                IKPosition = transform.position - up * IKOffset;

                var rW = grounding.footRotationWeight;
                rotationOffset = rW >= 1 ? r : Quaternion.Slerp(Quaternion.identity, r, rW);
            }

            // Get predicted Capsule hit from the middle of the foot
            private RaycastHit GetCapsuleHit( Vector3 offsetFromHeel )
            {
                var hit = new RaycastHit();
                var f = grounding.GetFootCenterOffset();
                if (invertFootCenter) f = -f;
                var origin = transform.position + f;
                hit.point = origin - up * grounding.maxStep * 2f;
                hit.normal = up;

                // Start point of the capsule
                var capsuleStart = origin + grounding.maxStep * up;
                // End point of the capsule depending on the foot's velocity.
                var capsuleEnd = capsuleStart + offsetFromHeel;

                if (Physics.CapsuleCast(capsuleStart, capsuleEnd, grounding.footRadius, -up, out hit,
                    grounding.maxStep * 3, grounding.layers)
                ) // Safeguarding from a CapsuleCast bug in Unity that might cause it to return NaN for hit.point when cast against large colliders.
                    if (float.IsNaN(hit.point.x))
                    {
                        hit.point = origin - up * grounding.maxStep * 2f;
                        hit.normal = up;
                    }
                return hit;
            }

            // Get simple Raycast from the heel
            private RaycastHit GetRaycastHit( Vector3 offsetFromHeel )
            {
                var hit = new RaycastHit();
                var origin = transform.position + offsetFromHeel;
                hit.point = origin - up * grounding.maxStep * 2f;
                hit.normal = up;
                if (grounding.maxStep <= 0f) return hit;

                Physics.Raycast(origin + grounding.maxStep * up, -up, out hit, grounding.maxStep * 3, grounding.layers);
                return hit;
            }

            // Rotates ground normal with respect to maxFootRotationAngle
            private Vector3 RotateNormal( Vector3 normal )
            {
                if (grounding.quality == Quality.Best) return normal;
                return Vector3.RotateTowards(up, normal, grounding.maxFootRotationAngle * Mathf.Deg2Rad, deltaTime);
            }

            // Set foot height from ground relative to a point
            private void SetFootToPoint( Vector3 normal, Vector3 point )
            {
                toHitNormal = Quaternion.FromToRotation(up, RotateNormal(normal));

                heightFromGround = GetHeightFromGround(point);
            }

            // Set foot height from ground relative to a plane
            private void SetFootToPlane( Vector3 planeNormal, Vector3 planePoint, Vector3 heelHitPoint )
            {
                planeNormal = RotateNormal(planeNormal);
                toHitNormal = Quaternion.FromToRotation(up, planeNormal);

                var pointOnPlane = V3Tools.LineToPlane(transform.position + up * grounding.maxStep, -up, planeNormal,
                    planePoint);

                // Get the height offset of the point on the plane
                heightFromGround = GetHeightFromGround(pointOnPlane);

                // Making sure the heel doesn't penetrate the ground
                var heelHeight = GetHeightFromGround(heelHitPoint);
                heightFromGround = Mathf.Clamp(heightFromGround, -Mathf.Infinity, heelHeight);
            }

            // Calculate height offset of a point
            private float GetHeightFromGround( Vector3 hitPoint )
            {
                return grounding.GetVerticalOffset(transform.position, hitPoint) - rootYOffset;
            }

            // Adding ground normal offset to the foot's rotation
            private void RotateFoot()
            {
                // Getting the full target rotation
                var rotationOffsetTarget = GetRotationOffsetTarget();

                // Slerping the rotation offset
                r = Quaternion.Slerp(r, rotationOffsetTarget, deltaTime * grounding.footRotationSpeed);
            }

            // Gets the target hit normal offset as a Quaternion
            private Quaternion GetRotationOffsetTarget()
            {
                if (grounding.maxFootRotationAngle <= 0f) return Quaternion.identity;
                if (grounding.maxFootRotationAngle >= 180f) return toHitNormal;
                return Quaternion.RotateTowards(Quaternion.identity, toHitNormal, grounding.maxFootRotationAngle);
            }
        }
    }
}