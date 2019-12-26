using UnityEngine;

namespace Invector.CharacterController.TopDownShooter
{
    [vClassHeader("TopDown Controller")]
    public class vTopDownController : vThirdPersonController
    {
        private Camera cam;
        private Vector3 camForward;
        private Vector2 joystickMousePos;
        private Vector3 lookDirection;

        [HideInInspector] public Vector3 lookPos;

        public bool rotateToMousePoint;
        private float topDownHorizontal;

        [HideInInspector] public Vector3 topDownMove;

        private float topDownVertical;

        public override void Init()
        {
            base.Init();
            cam = Camera.main;
        }

        public override void UpdateMotor()
        {
            base.UpdateMotor();
            UpdateCameraToTopDown();
        }

        protected override void ControlLocomotion()
        {
            if (lockMovement || currentHealth <= 0) return;
            TopDownMovement();
            TopDownRotation();
        }

        protected override void StrafeVelocity( float velocity )
        {
            var v = new Vector3(topDownMove.x, 0, topDownMove.z) * (velocity > 0 ? velocity : 1f);

            v.y = _rigidbody.velocity.y;
            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, v, 20f * Time.deltaTime);
        }

        public override void StrafeMovement()
        {
            TopDownMovement();
        }

        #region TopDownMethods

        protected virtual void TopDownMovement()
        {
            if (topDownMove.magnitude > 1)
                topDownMove.Normalize();

            ConvertToTopDownDirection();

            if (strafeSpeed.walkByDefault)
                TopDownSpeed(0.5f);
            else
                TopDownSpeed(1f);

            if (isSprinting)
            {
                speed += 0.5f;
                strafeMagnitude += 0.5f;
            }

            if (stopMove)
            {
                speed = 0f;
                strafeMagnitude = 0f;
            }

            animator.SetFloat("InputMagnitude", strafeMagnitude, .2f, Time.deltaTime);
        }

        protected virtual void UpdateCameraToTopDown()
        {
            lookPos = vMousePositionHandler.Instance.worldMousePosition;

            if (cam != null)
            {
                camForward = Quaternion.Euler(0, -90, 0) * cam.transform.right;
                topDownMove = input.y * camForward + input.x * cam.transform.right;
            }
            else
            {
                topDownMove = input.y * Vector3.forward + input.x * Vector3.right;
            }
        }

        protected virtual void TopDownRotation()
        {
            if (!customAction && !actions)
            {
                if (locomotionType.Equals(LocomotionType.OnlyStrafe) && !isStrafing) isStrafing = true;
                lookDirection = !locomotionType.Equals(LocomotionType.OnlyFree) && isStrafing
                    ? lookPos - transform.position
                    : topDownMove;
                lookDirection.y = 0f;
                if (lookDirection != Vector3.zero)
                {
                    var rotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotation,
                        (isStrafing ? strafeSpeed.rotationSpeed : freeSpeed.rotationSpeed) * Time.deltaTime);
                }
            }
        }

        protected virtual void ConvertToTopDownDirection()
        {
            var localMove = transform.InverseTransformDirection(topDownMove);
            topDownHorizontal = localMove.x;
            topDownVertical = localMove.z;
        }

        protected virtual void TopDownSpeed( float value )
        {
            speed = Mathf.Clamp(topDownVertical, -1f, 1f);
            direction = Mathf.Clamp(topDownHorizontal, -1f, 1f);
            var newInput = new Vector2(speed, direction);
            strafeMagnitude = Mathf.Clamp(newInput.magnitude, 0, value);
        }

        #endregion
    }
}