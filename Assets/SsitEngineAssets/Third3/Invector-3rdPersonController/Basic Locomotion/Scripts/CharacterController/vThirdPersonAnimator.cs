using UnityEngine;

namespace Invector.CharacterController
{
    public abstract class vThirdPersonAnimator : vThirdPersonMotor
    {
        public virtual void UpdateAnimator()
        {
            if (animator == null || !animator.enabled) return;

            LayerControl();
            ActionsControl();

            RandomIdle();

            // trigger by input
            RollAnimation();

            // trigger at any time using conditions
            TriggerLandHighAnimation();

            LocomotionAnimation();
            DeadAnimation();
        }

        public void LayerControl()
        {
            baseLayerInfo = animator.GetCurrentAnimatorStateInfo(baseLayer);
            underBodyInfo = animator.GetCurrentAnimatorStateInfo(underBodyLayer);
            rightArmInfo = animator.GetCurrentAnimatorStateInfo(rightArmLayer);
            leftArmInfo = animator.GetCurrentAnimatorStateInfo(leftArmLayer);
            upperBodyInfo = animator.GetCurrentAnimatorStateInfo(upperBodyLayer);
            fullBodyInfo = animator.GetCurrentAnimatorStateInfo(fullbodyLayer);
        }

        public void ActionsControl()
        {
            // to have better control of your actions, you can filter the animations state using bools 
            // this way you can know exactly what animation state the character is playing

            landHigh = baseLayerInfo.IsName("LandHigh");
            quickStop = baseLayerInfo.IsName("QuickStop");

            isRolling = baseLayerInfo.IsName("Roll");
            inTurn = baseLayerInfo.IsName("TurnOnSpot");

            // locks player movement while a animation with tag 'LockMovement' is playing
            lockMovement = IsAnimatorTag("LockMovement");
            // ! -- you can add the Tag "CustomAction" into a AnimatonState and the character will not perform any Melee action -- !            
            customAction = IsAnimatorTag("CustomAction");
        }

        #region Variables        

        // match cursorObject to help animation to reach their cursorObject
        [HideInInspector] public Transform matchTarget;

        // head track control, if you want to turn off at some point, make it 0
        [HideInInspector] public float lookAtWeight;

        private float randomIdleCount, randomIdle;
        private Vector3 lookPosition;
        private float _speed;
        private float _direction;
        private bool triggerDieBehaviour;

        private int baseLayer => animator.GetLayerIndex("Base Layer");
        private int underBodyLayer => animator.GetLayerIndex("UnderBody");
        private int rightArmLayer => animator.GetLayerIndex("RightArm");
        private int leftArmLayer => animator.GetLayerIndex("LeftArm");
        private int upperBodyLayer => animator.GetLayerIndex("UpperBody");
        private int fullbodyLayer => animator.GetLayerIndex("FullBody");

        #endregion

        #region Locomotion Animations

        private void RandomIdle()
        {
            if (input != Vector2.zero || actions) return;

            if (randomIdleTime > 0)
            {
                if (input.sqrMagnitude == 0 && !isCrouching && _capsuleCollider.enabled && isGrounded)
                {
                    randomIdleCount += Time.fixedDeltaTime;
                    if (randomIdleCount > 6)
                    {
                        randomIdleCount = 0;
                        animator.SetTrigger("IdleRandomTrigger");
                        animator.SetInteger("IdleRandom", Random.Range(1, 4));
                    }
                }
                else
                {
                    randomIdleCount = 0;
                    animator.SetInteger("IdleRandom", 0);
                }
            }
        }

        public void LocomotionAnimation()
        {
            animator.SetBool("IsStrafing", isStrafing);
            animator.SetBool("IsCrouching", isCrouching);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("isDead", isDead);
            animator.SetFloat("GroundDistance", groundDistance);

            if (!isGrounded)
                animator.SetFloat("VerticalVelocity", verticalVelocity);

            if (isStrafing)
                // strafe movement get the input 1 or -1
                animator.SetFloat("InputHorizontal", !stopMove && !lockMovement ? direction : 0f, 0.25f,
                    Time.deltaTime);

            animator.SetFloat("InputVertical", !stopMove && !lockMovement ? speed : 0f, 0.25f, Time.deltaTime);

            if (turnOnSpotAnim)
            {
                GetTurnOnSpotDirection(transform, Camera.main.transform, ref _speed, ref _direction, input);
                FreeTurnOnSpot(_direction * 180);
            }
        }

        public virtual void OnAnimatorMove()
        {
            if (!enabled) return;

            // we implement this function to override the default root motion.
            // this allows us to modify the positional speed before it's applied.
            if (isGrounded)
            {
                transform.rotation = animator.rootRotation;

                //strafe extra speed
                if (isStrafing)
                {
                    var _speed = Mathf.Abs(strafeMagnitude);
                    if (_speed <= 0.5f)
                        ControlSpeed(strafeSpeed.walkSpeed);
                    else if (_speed > 0.5f && _speed <= 1f)
                        ControlSpeed(strafeSpeed.runningSpeed);
                    else
                        ControlSpeed(strafeSpeed.sprintSpeed);

                    if (isCrouching)
                        ControlSpeed(strafeSpeed.crouchSpeed);
                }
                else if (!isStrafing)
                {
                    //free extra speed
                    if (speed <= 0.5f)
                        ControlSpeed(freeSpeed.walkSpeed);
                    else if (speed > 0.5 && speed <= 1f)
                        ControlSpeed(freeSpeed.runningSpeed);
                    else
                        ControlSpeed(freeSpeed.sprintSpeed);

                    if (isCrouching)
                        ControlSpeed(freeSpeed.crouchSpeed);
                }
            }
        }

        public void FreeTurnOnSpot( float direction )
        {
            var inTransition = animator.IsInTransition(0);
            float directionDampTime = inTurn || inTransition ? 1000000 : 0;
            animator.SetFloat("TurnOnSpotDirection", direction, directionDampTime, Time.deltaTime);
        }

        public void GetTurnOnSpotDirection( Transform root, Transform camera, ref float _speed, ref float _direction,
            Vector2 input )
        {
            var rootDirection = root.forward;
            var stickDirection = new Vector3(input.x, 0, input.y);

            // Get camera rotation.    
            var CameraDirection = camera.forward;
            CameraDirection.y = 0.0f; // kill Y
            var referentialShift = Quaternion.FromToRotation(Vector3.forward, CameraDirection);
            // Convert joystick input in Worldspace coordinates            
            var moveDirection = rotateByWorld ? stickDirection : referentialShift * stickDirection;

            var speedVec = new Vector2(input.x, input.y);
            _speed = Mathf.Clamp(speedVec.magnitude, 0, 1);

            if (_speed > 0.01f) // dead zone
            {
                var axis = Vector3.Cross(rootDirection, moveDirection);
                _direction = Vector3.Angle(rootDirection, moveDirection) / 180.0f * (axis.y < 0 ? -1 : 1);
            }
            else
            {
                _direction = 0.0f;
            }
        }

        #endregion

        #region Action Animations  

        protected virtual void RollAnimation()
        {
            if (isRolling)
            {
                autoCrouch = true;

                if (isStrafing && (input != Vector2.zero || speed > 0.25f))
                {
                    var rot = rollRotation;
                    var eulerAngles = new Vector3(transform.eulerAngles.x, rot.eulerAngles.y, transform.eulerAngles.z);
                    transform.eulerAngles = eulerAngles;
                }

                if (baseLayerInfo.normalizedTime > 0.1f && baseLayerInfo.normalizedTime < 0.3f)
                    _rigidbody.useGravity = false;

                // prevent the character to rolling up 
                if (verticalVelocity >= 1)
                    _rigidbody.velocity = Vector3.ProjectOnPlane(_rigidbody.velocity, groundHit.normal);

                // reset the rigidbody a little ealier to the character fall while on air
                if (baseLayerInfo.normalizedTime > 0.3f)
                    _rigidbody.useGravity = true;
            }
        }

        protected virtual Quaternion rollRotation
        {
            get
            {
                var newDir = Vector3.RotateTowards(transform.forward, targetDirection, 25f * Time.fixedDeltaTime, 0.0f);
                return Quaternion.LookRotation(newDir);
            }
        }

        protected virtual void DeadAnimation()
        {
            if (!isDead) return;

            if (!triggerDieBehaviour)
            {
                triggerDieBehaviour = true;
                DeathBehaviour();
            }

            // death by animation
            if (deathBy == DeathBy.Animation)
            {
                if (fullBodyInfo.IsName("Dead"))
                    if (fullBodyInfo.normalizedTime >= 0.99f && groundDistance <= 0.15f)
                        RemoveComponents();
            }
            // death by animation & ragdoll after a time
            else if (deathBy == DeathBy.AnimationWithRagdoll)
            {
                if (fullBodyInfo.IsName("Dead"))
                    // activate the ragdoll after the animation finish played
                    if (fullBodyInfo.normalizedTime >= 0.8f)
                        onActiveRagdoll.Invoke();
            }
            // death by ragdoll
            else if (deathBy == DeathBy.Ragdoll)
            {
                onActiveRagdoll.Invoke();
            }
        }

        public virtual void SetActionState( int value )
        {
            animator.SetInteger("ActionState", value);
        }

        public virtual void MatchTarget( Vector3 matchPosition, Quaternion matchRotation, AvatarTarget target,
            MatchTargetWeightMask weightMask, float normalisedStartTime, float normalisedEndTime )
        {
            if (animator.isMatchingTarget || animator.IsInTransition(0))
                return;

            var normalizeTime = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f);

            if (normalizeTime > normalisedEndTime)
                return;

            animator.MatchTarget(matchPosition, matchRotation, target, weightMask, normalisedStartTime,
                normalisedEndTime);
        }

        #endregion

        #region Trigger Animations       

        public void TriggerAnimationState( string animationClip, float transition )
        {
            animator.CrossFadeInFixedTime(animationClip, transition);
        }

        public bool IsAnimatorTag( string tag )
        {
            if (animator == null) return false;
            if (baseLayerInfo.IsTag(tag)) return true;
            if (underBodyInfo.IsTag(tag)) return true;
            if (rightArmInfo.IsTag(tag)) return true;
            if (leftArmInfo.IsTag(tag)) return true;
            if (upperBodyInfo.IsTag(tag)) return true;
            if (fullBodyInfo.IsTag(tag)) return true;
            return false;
        }

        private void TriggerLandHighAnimation()
        {
            if (landHigh)
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                vInput.instance.GamepadVibration(0.25f);
#endif
            }
        }

        #endregion
    }
}