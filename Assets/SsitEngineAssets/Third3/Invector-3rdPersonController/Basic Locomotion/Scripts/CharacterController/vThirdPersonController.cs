using System.Collections;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace Invector.CharacterController
{
    [vClassHeader("THIRD PERSON CONTROLLER", iconName = "controllerIcon")]
    public class vThirdPersonController : vThirdPersonAnimator
    {
        #region Variables

        public static vThirdPersonController instance;

        #endregion

        protected virtual void Awake()
        {
            groundLayer = LayerUtils.PlayerGroudMask;

            StartCoroutine(UpdateRaycast()); // limit raycasts calls for better performance
        }

        protected virtual void Start()
        {
            if (instance == null)
            {
                instance = this;
                //test
                //DontDestroyOnLoad(this.gameObject);
                gameObject.name = gameObject.name + " Instance";
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Returns the collider which contains the point within its bounding box.
        /// </summary>
        /// <param name="point">The point to determine if it is within the bounding box of the character.</param>
        /// <returns>The collider which contains the point within its bounding box. Can be null.</returns>
        public Collider BoundsCountains( Vector3 point )
        {
            //Init();
            //for (int i = 0; i < mcol; ++i)
            //{
            //    if (m_Colliders[i].bounds.Contains(point))
            //    {
            //        return m_Colliders[i];
            //    }
            //}
            if (_capsuleCollider.bounds.Contains(point)) return _capsuleCollider;
            return null;
        }

        #region Locomotion Actions

        public virtual void Sprint( bool value )
        {
            if (value)
            {
                if (currentStamina > 0 && input.sqrMagnitude > 0.1f)
                    if (isGrounded && !isCrouching)
                        isSprinting = !isSprinting;
            }
            else if (currentStamina <= 0 || input.sqrMagnitude < 0.1f || isCrouching || !isGrounded || actions ||
                     isStrafing && !strafeSpeed.walkByDefault && (direction >= 0.5 || direction <= -0.5 || speed <= 0))
            {
                isSprinting = false;
            }
        }

        public virtual void Crouch()
        {
            if (isGrounded && !actions)
            {
                if (isCrouching && CanExitCrouch())
                    isCrouching = false;
                else
                    isCrouching = true;
            }
        }

        public virtual void Strafe()
        {
            isStrafing = !isStrafing;
        }

        public virtual void Jump()
        {
            if (customAction) return;

            // know if has enough stamina to make this action
            var staminaConditions = currentStamina > jumpStamina;
            // conditions to do this action
            var jumpConditions = !isCrouching && isGrounded && !actions && staminaConditions && !isJumping;
            // return if jumpCondigions is false
            if (!jumpConditions) return;
            // trigger jump behaviour
            jumpCounter = jumpTimer;
            isJumping = true;
            // trigger jump animations
            if (input.sqrMagnitude < 0.1f)
                animator.CrossFadeInFixedTime("Jump", 0.1f);
            else
                animator.CrossFadeInFixedTime("JumpMove", .2f);
            // reduce stamina
            ReduceStamina(jumpStamina, false);
            currentStaminaRecoveryDelay = 1f;
        }


        public virtual void Roll()
        {
            var staminaCondition = currentStamina > rollStamina;
            // can roll even if it's on a quickturn or quickstop animation
            var actionsRoll = !actions || actions && quickStop;
            // general conditions to roll
            var rollConditions = (input != Vector2.zero || speed > 0.25f) && actionsRoll && isGrounded &&
                                 staminaCondition && !isJumping;

            if (!rollConditions || isRolling) return;

            animator.SetTrigger("ResetState");
            animator.CrossFadeInFixedTime("Roll", 0.1f);
            ReduceStamina(rollStamina, false);
            currentStaminaRecoveryDelay = 2f;
        }

        public void DoRoll()
        {
            if (isRolling) return;

            animator.SetTrigger("ResetState");
            animator.CrossFadeInFixedTime("Roll", 0.1f);
            ReduceStamina(rollStamina, false);
            currentStaminaRecoveryDelay = 2f;
        }

        /// <summary>
        /// Use another transform as  reference to rotate
        /// </summary>
        /// <param name="referenceTransform"></param>
        public virtual void RotateWithAnotherTransform( Transform referenceTransform )
        {
            var newRotation = new Vector3(transform.eulerAngles.x, referenceTransform.eulerAngles.y,
                transform.eulerAngles.z);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newRotation),
                strafeSpeed.rotationSpeed * Time.fixedDeltaTime);
            targetRotation = transform.rotation;
        }

        #endregion

        #region Check Action Triggers 

        /// <summary>
        /// Call this in OnTriggerEnter or OnTriggerStay to check if enter in triggerActions     
        /// </summary>
        /// <param name="other">collider trigger</param>                         
        public virtual void CheckTriggers( Collider other )
        {
            try
            {
                CheckForAutoCrouch(other);
            }
            catch (UnityException e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        /// <summary>
        /// Call this in OnTriggerExit to check if exit of triggerActions 
        /// </summary>
        /// <param name="other"></param>
        public virtual void CheckTriggerExit( Collider other )
        {
            AutoCrouchExit(other);
        }

        #region Update Raycasts  

        protected IEnumerator UpdateRaycast()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                AutoCrouch();
                StopMove();
            }
        }

        #endregion

        #region Crouch Methods

        protected virtual void AutoCrouch()
        {
            if (autoCrouch)
                isCrouching = true;

            if (autoCrouch && !inCrouchArea && CanExitCrouch())
            {
                autoCrouch = false;
                isCrouching = false;
            }
        }

        protected virtual bool CanExitCrouch()
        {
            // radius of SphereCast
            var radius = _capsuleCollider.radius * 0.9f;
            // Position of SphereCast origin stating in base of capsule
            var pos = transform.position + Vector3.up * (colliderHeight * 0.5f - colliderRadius);
            // ray for SphereCast
            var ray2 = new Ray(pos, Vector3.up);

            // sphere cast around the base of capsule for check ground distance
            if (Physics.SphereCast(ray2, radius, out groundHit, headDetect - colliderRadius * 0.1f, autoCrouchLayer))
                return false;
            return true;
        }

        protected virtual void AutoCrouchExit( Collider other )
        {
            if (other.CompareTag("AutoCrouch")) inCrouchArea = false;
        }

        protected virtual void CheckForAutoCrouch( Collider other )
        {
            if (other.gameObject.CompareTag("AutoCrouch"))
            {
                autoCrouch = true;
                inCrouchArea = true;
            }
        }

        #endregion

        #endregion
    }
}