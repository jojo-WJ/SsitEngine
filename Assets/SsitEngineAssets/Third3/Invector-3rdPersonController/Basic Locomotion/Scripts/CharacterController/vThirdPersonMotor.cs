using System;
using Invector.EventSystems;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.CharacterController
{
    public abstract class vThirdPersonMotor : vCharacter
    {
        public override void Init()
        {
            base.Init();

            // we use AnimatePhysics because it works better with the MatchTarget method for Actions
            animator.updateMode = AnimatorUpdateMode.AnimatePhysics;

            // slides the character through walls and edges
            frictionPhysics = new PhysicMaterial();
            frictionPhysics.name = "frictionPhysics";
            frictionPhysics.staticFriction = .25f;
            frictionPhysics.dynamicFriction = .25f;
            frictionPhysics.frictionCombine = PhysicMaterialCombine.Multiply;

            // prevents the collider from slipping on ramps
            maxFrictionPhysics = new PhysicMaterial();
            maxFrictionPhysics.name = "maxFrictionPhysics";
            maxFrictionPhysics.staticFriction = 1f;
            maxFrictionPhysics.dynamicFriction = 1f;
            maxFrictionPhysics.frictionCombine = PhysicMaterialCombine.Maximum;

            // air physics 
            slippyPhysics = new PhysicMaterial();
            slippyPhysics.name = "slippyPhysics";
            slippyPhysics.staticFriction = 0f;
            slippyPhysics.dynamicFriction = 0f;
            slippyPhysics.frictionCombine = PhysicMaterialCombine.Minimum;

            // rigidbody info
            _rigidbody = GetComponent<Rigidbody>();

            // capsule collider info
            _capsuleCollider = GetComponent<CapsuleCollider>();

            // save your collider preferences 
            colliderCenter = GetComponent<CapsuleCollider>().center;
            colliderRadius = GetComponent<CapsuleCollider>().radius;
            colliderHeight = GetComponent<CapsuleCollider>().height;

            // avoid collision detection with inside colliders 
            var AllColliders = GetComponentsInChildren<Collider>();
            var thisCollider = GetComponent<Collider>();
            for (var i = 0; i < AllColliders.Length; i++) Physics.IgnoreCollision(thisCollider, AllColliders[i]);

            // health info
            currentHealth = maxHealth;
            currentHealthRecoveryDelay = healthRecoveryDelay;
            currentStamina = maxStamina;
        }

        public virtual void UpdateMotor()
        {
            CheckHealth();
            CheckStamina();
            CheckGround();
            CheckRagdoll();

            ControlCapsuleHeight();
            ControlJumpBehaviour();
            ControlLocomotion();
            StaminaRecovery();
            HealthRecovery();
        }

        [Serializable]
        public class vMovementSpeed
        {
            [Tooltip("Speed to Crouch using rigibody force or extra speed if you're using RootMotion")]
            public float crouchSpeed = 2f;

            [Tooltip("Rotation speed of the character")]
            public float rotationSpeed = 10f;

            [Tooltip("Speed to Run using rigibody force or extra speed if you're using RootMotion")]
            public float runningSpeed = 3f;

            [Tooltip("Speed to Sprint using rigibody force or extra speed if you're using RootMotion")]
            public float sprintSpeed = 4f;

            [Tooltip(
                "Character will walk by default and run when the sprint input is pressed. The Sprint animation will not play")]
            public bool walkByDefault;

            [Tooltip("Speed to Walk using rigibody force or extra speed if you're using RootMotion")]
            public float walkSpeed = 2f;
        }

        #region Variables               

        #region Stamina

        [Header("--- Debug Info ---")] public bool debugWindow;

        [vEditorToolbar("Stamina")] public float sprintStamina = 30f;

        public float jumpStamina = 30f;
        public float rollStamina = 25f;
        public UnityEvent OnStaminaEnd;

        #endregion

        #region Layers

        [vEditorToolbar("Layers")] [Tooltip("Layers that the character can walk on")]
        public LayerMask groundLayer = 1 << 0;

        [Tooltip("What objects can make the character auto crouch")]
        public LayerMask autoCrouchLayer = 1 << 0;

        [Tooltip(
            "[SPHERECAST] ADJUST IN PLAY MODE - White Spherecast put just above the head, this will make the character Auto-Crouch if something hit the sphere.")]
        public float headDetect = 0.95f;

        [Tooltip("Select the layers the your character will stop moving when close to")]
        public LayerMask stopMoveLayer;

        [Tooltip("[RAYCAST] Stopmove Raycast Height")]
        public float stopMoveHeight = 0.65f;

        [Tooltip("[RAYCAST] Stopmove Raycast Distance")]
        public float stopMoveDistance = 0.5f;

        #endregion

        #region Character Variables       

        [vEditorToolbar("Locomotion")]
        [Tooltip(
            "Turn off if you have 'in place' animations and use this values above to move the character, or use with root motion as extra speed")]
        public bool useRootMotion;

        public enum LocomotionType
        {
            FreeWithStrafe,
            OnlyStrafe,
            OnlyFree
        }

        public LocomotionType locomotionType = LocomotionType.FreeWithStrafe;

        public vMovementSpeed freeSpeed, strafeSpeed;

        [Tooltip(
            "Use this to rotate the character using the World axis, or false to use the camera axis - CHECK for Isometric Camera")]
        public bool rotateByWorld;

        [Tooltip("Check this to use the TurnOnSpot animations")]
        public bool turnOnSpotAnim;

        [Tooltip("Can control the roll direction")]
        public bool rollControl;

        [Tooltip(
            "Put your Random Idle animations at the AnimatorController and select a value to randomize, 0 is disable.")]
        public float randomIdleTime;

        [vEditorToolbar("Jump")] [Tooltip("Check to control the character while jumping")]
        public bool jumpAirControl = true;

        [Tooltip("How much time the character will be jumping")]
        public float jumpTimer = 0.3f;

        [HideInInspector] public float jumpCounter;

        [Tooltip("Add Extra jump speed, based on your speed input the character will move forward")]
        public float jumpForward = 3f;

        [Tooltip("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
        public float jumpHeight = 4f;

        [vEditorToolbar("Grounded")] [Tooltip("Distance to became not grounded")] [SerializeField]
        protected float groundMinDistance = 0.25f;

        [SerializeField] protected float groundMaxDistance = 0.5f;

        [Tooltip("ADJUST IN PLAY MODE - Offset height limit for sters - GREY Raycast in front of the legs")]
        public float stepOffsetEnd = 0.45f;

        [Tooltip(
            "ADJUST IN PLAY MODE - Offset height origin for sters, make sure to keep slight above the floor - GREY Raycast in front of the legs")]
        public float stepOffsetStart;

        [Tooltip("Higher value will result jittering on ramps, lower values will have difficulty on steps")]
        public float stepSmooth = 4f;

        [Tooltip("Max angle to walk")] public float slopeLimit = 45f;

        [Tooltip("Apply extra gravity when the character is not grounded")]
        public float extraGravity = -10f;

        [Tooltip(
            "Turn the Ragdoll On when falling at high speed (check VerticalVelocity) - leave the value with 0 if you don't want this feature")]
        public float ragdollVel = -16f;

        protected float groundDistance;
        public RaycastHit groundHit;

        #endregion

        #region Actions

        public bool isStrafing
        {
            get => _isStrafing || lockInStrafe;
            set => _isStrafing = value;
        }

        // movement bools
        [HideInInspector] public bool
            isGrounded,
            isCrouching,
            inCrouchArea,
            isSprinting,
            isSliding,
            stopMove,
            autoCrouch;

        // action bools
        [HideInInspector] public bool
            isRolling,
            isJumping,
            isGettingUp,
            inTurn,
            quickStop,
            landHigh;

        [HideInInspector] public bool customAction;

        // one bool to rule then all
        [HideInInspector] public bool actions => isRolling || quickStop || landHigh || customAction;

        protected void RemoveComponents()
        {
            if (!removeComponentsAfterDie) return;
            if (_capsuleCollider != null) Destroy(_capsuleCollider);
            if (_rigidbody != null) Destroy(_rigidbody);
            if (animator != null) Destroy(animator);
            var comps = GetComponents<MonoBehaviour>();
            for (var i = 0; i < comps.Length; i++) Destroy(comps[i]);
        }

        #endregion

        #region Direction Variables

        //[HideInInspector]
        public Vector3 targetDirection;

        [HideInInspector] public Quaternion targetRotation;

        [HideInInspector] public float strafeMagnitude;

        [HideInInspector] public Quaternion freeRotation;

        //[HideInInspector]
        public bool keepDirection;

        //[HideInInspector]
        public Vector2 oldInput;

        #endregion

        #region Components                       

        [HideInInspector] public Rigidbody _rigidbody; // access the Rigidbody component

        [HideInInspector]
        public PhysicMaterial
            frictionPhysics, maxFrictionPhysics, slippyPhysics; // create PhysicMaterial for the Rigidbody

        [HideInInspector] public CapsuleCollider _capsuleCollider; // access CapsuleCollider information

        #endregion

        #region Hide Variables

        [HideInInspector] public bool lockMovement;

        [HideInInspector] public bool lockInStrafe;

        [HideInInspector] public bool lockSpeed;

        [HideInInspector] public bool lockRotation;

        [HideInInspector] public bool forceRootMotion;

        [HideInInspector]
        public float colliderRadius, colliderHeight; // storage capsule collider extra information        

        [HideInInspector] public Vector3 colliderCenter; // storage the center of the capsule collider info        

        [HideInInspector] public Vector2 input; // generate input for the controller        

        [HideInInspector] public float speed, direction, verticalVelocity; // general variables to the locomotion

        [HideInInspector] public float velocity; // velocity to apply to rigdibody

        // get Layers from the Animator Controller
        [HideInInspector]
        public AnimatorStateInfo baseLayerInfo, underBodyInfo, rightArmInfo, leftArmInfo, fullBodyInfo, upperBodyInfo;

        private bool _isStrafing;

        #endregion

        public bool SimpleAnim { get; set; }

        #endregion

        #region Health & Stamina

        public override void TakeDamage( vDamage damage, bool hitReaction = true )
        {
            // don't apply damage if the character is rolling, you can add more conditions here
            if (currentHealth <= 0 || !damage.ignoreDefense && isRolling)
                return;

            // reduce the current health by the damage amount.
            currentHealth -= damage.damageValue;
            currentHealthRecoveryDelay = currentHealth <= 0 ? 0 : healthRecoveryDelay;

            // turn the ragdoll on if the weapon is checked with 'activeRagdoll' 
            if (damage.activeRagdoll) onActiveRagdoll.Invoke();

            // only trigger the hitReaction animation when the character is not doing any action
            var hitReactionConditions = !actions || !customAction;
            if (hitReactionConditions && currentHealth > 0)
            {
                animator.SetInteger("HitDirection", (int) transform.HitAngle(damage.sender.position));
                // trigger hitReaction animation
                if (hitReaction)
                {
                    // set the ID of the reaction based on the attack animation state of the attacker - Check the MeleeAttackBehaviour script
                    animator.SetInteger("ReactionID", damage.reaction_id);
                    animator.SetTrigger("TriggerReaction");
                }
                else
                {
                    animator.SetInteger("RecoilID", damage.recoil_id);
                    animator.SetTrigger("TriggerRecoil");
                }
            }
            onReceiveDamage.Invoke(damage);
            // apply vibration on the gamepad   
            // vInput.instance.GamepadVibration( 0.25f );
        }

        public void ReduceStamina( float value, bool accumulative )
        {
            if (accumulative) currentStamina -= value * Time.deltaTime;
            else currentStamina -= value;
            if (currentStamina < 0)
            {
                currentStamina = 0;
                OnStaminaEnd.Invoke();
            }
        }

        public void DeathBehaviour()
        {
            // lock the player input
            lockMovement = true;
            // change the culling mode to render the animation until finish
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            // trigger die animation            
            if (deathBy == DeathBy.Animation || deathBy == DeathBy.AnimationWithRagdoll)
                animator.SetBool("isDead", true);
        }

        private void CheckHealth()
        {
            if (currentHealth <= 0 && !isDead)
            {
                isDead = true;
                if (onDead != null) onDead.Invoke(gameObject);
            }
            else if (currentHealth > 0)
            {
                isDead = false;
            }
        }

        private void HealthRecovery()
        {
            if (currentHealth <= 0 || healthRecovery == 0) return;
            if (currentHealthRecoveryDelay > 0)
            {
                currentHealthRecoveryDelay -= Time.deltaTime;
            }
            else
            {
                if (currentHealth > maxHealth)
                    currentHealth = maxHealth;
                if (currentHealth < maxHealth)
                    currentHealth = Mathf.Lerp(currentHealth, maxHealth, healthRecovery * Time.deltaTime);
            }
        }

        private void CheckStamina()
        {
            // check how much stamina this action will consume
            if (isSprinting)
            {
                currentStaminaRecoveryDelay = 0.25f;
                ReduceStamina(sprintStamina, true);
            }
        }

        public void StaminaRecovery()
        {
            if (currentStaminaRecoveryDelay > 0)
            {
                currentStaminaRecoveryDelay -= Time.deltaTime;
            }
            else
            {
                if (currentStamina > maxStamina)
                    currentStamina = maxStamina;
                if (currentStamina < maxStamina)
                    currentStamina += staminaRecovery;
            }
        }

        #endregion

        #region Locomotion 

        protected virtual void ControlLocomotion()
        {
            if (lockMovement || currentHealth <= 0) return;

            if (locomotionType.Equals(LocomotionType.FreeWithStrafe) && !isStrafing ||
                locomotionType.Equals(LocomotionType.OnlyFree))
                FreeMovement();
            else if (locomotionType.Equals(LocomotionType.OnlyStrafe) ||
                     locomotionType.Equals(LocomotionType.FreeWithStrafe) && isStrafing)
                StrafeMovement();
        }

        public virtual void StrafeMovement()
        {
            isStrafing = true;

            if (strafeSpeed.walkByDefault)
                StrafeLimitSpeed(0.5f);
            else
                StrafeLimitSpeed(1f);
            if (stopMove) strafeMagnitude = 0f;
            animator.SetFloat("InputMagnitude", strafeMagnitude, .2f, Time.deltaTime);
        }

        protected virtual void StrafeLimitSpeed( float value )
        {
            var limitInput = isSprinting ? 1.5f : 1f;
            var _input = input * limitInput;
            var _speed = Mathf.Clamp(_input.y, -limitInput, limitInput);
            var _direction = Mathf.Clamp(_input.x, -limitInput, limitInput);
            speed = _speed;
            direction = _direction;
            var newInput = new Vector2(speed, direction);
            strafeMagnitude = Mathf.Clamp(newInput.magnitude, 0, value * limitInput);
        }

        public virtual void FreeMovement()
        {
            // set speed to both vertical and horizontal inputs
            speed = Mathf.Abs(input.x) + Mathf.Abs(input.y);
            // limits the character to walk by default
            if (freeSpeed.walkByDefault)
                speed = Mathf.Clamp(speed, 0, 0.5f);
            else
                speed = Mathf.Clamp(speed, 0, 1f);
            // add 0.5f on sprint to change the animation on animator
            if (isSprinting) speed += 0.5f;
            if (stopMove || lockSpeed) speed = 0f;

            animator.SetFloat("InputMagnitude", speed, .2f, Time.deltaTime);

            var conditions = !actions || quickStop || isRolling && rollControl;
            if (input != Vector2.zero && targetDirection.magnitude > 0.1f && conditions && !lockRotation)
            {
                var lookDirection = targetDirection.normalized;
                freeRotation = Quaternion.LookRotation(lookDirection, transform.up);
                var diferenceRotation = freeRotation.eulerAngles.y - transform.eulerAngles.y;
                var eulerY = transform.eulerAngles.y;
                // apply free directional rotation while not turning180 animations
                if (isGrounded || !isGrounded && jumpAirControl)
                {
                    if (diferenceRotation < 0 || diferenceRotation > 0) eulerY = freeRotation.eulerAngles.y;
                    var euler = new Vector3(transform.eulerAngles.x, eulerY, transform.eulerAngles.z);
                    if (inTurn) return;
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(euler),
                        freeSpeed.rotationSpeed * Time.deltaTime);
                }
                if (!keepDirection)
                    oldInput = input;
                if (Vector2.Distance(oldInput, input) > 0.9f && keepDirection)
                    keepDirection = false;
            }
        }

        public virtual void ControlSpeed( float velocity )
        {
            if (Time.deltaTime == 0) return;
            if (SimpleAnim) return;
            // use RootMotion and extra speed values to move the character
            if (useRootMotion && !actions && !customAction)
            {
                //Debug.Log("ControlSpeed useRootMotion" + velocity);
                this.velocity = velocity;
                var deltaPosition =
                    new Vector3(animator.deltaPosition.x, transform.position.y, animator.deltaPosition.z);
                var v = deltaPosition * (velocity > 0 ? velocity : 1f) / Time.deltaTime;
                v.y = _rigidbody.velocity.y;
                _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, v, 20f * Time.deltaTime);
            }
            // use only RootMotion 
            else if (actions || customAction || lockMovement || forceRootMotion)
            {
                //Debug.Log("ControlSpeed only RootMotion" + velocity);

                this.velocity = velocity;
                var v = Vector3.zero;
                v.y = _rigidbody.velocity.y;
                _rigidbody.velocity = v;
                transform.position = animator.rootPosition;
                if (forceRootMotion)
                    transform.rotation = animator.rootRotation;
            }
            //use only Rigibody Force to move the character (ideal for 'inplace' animations and better behaviour in general) 
            else
            {
                //Debug.Log("ControlSpeed use only Rigibody" + velocity);

                if (isStrafing)
                    StrafeVelocity(velocity);
                else
                    FreeVelocity(velocity);
            }
        }

        protected virtual void StrafeVelocity( float velocity )
        {
            var v = transform.TransformDirection(new Vector3(input.x, 0, input.y)) * (velocity > 0 ? velocity : 1f);
            v.y = _rigidbody.velocity.y;
            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, v, 20f * Time.deltaTime);
        }

        protected virtual void FreeVelocity( float velocity )
        {
            var _targetVelocity = transform.forward * velocity * speed;
            _targetVelocity.y = _rigidbody.velocity.y;
            _rigidbody.velocity = _targetVelocity;
            _rigidbody.AddForce(transform.forward * (velocity * speed) * Time.deltaTime, ForceMode.VelocityChange);
        }

        protected void StopMove()
        {
            if (input.sqrMagnitude < 0.1 || !isGrounded) return;

            RaycastHit hitinfo;
            var ray = new Ray(transform.position + new Vector3(0, stopMoveHeight, 0), targetDirection.normalized);

            if (Physics.Raycast(ray, out hitinfo, _capsuleCollider.radius + stopMoveDistance, stopMoveLayer))
            {
                var hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);

                if (hitinfo.distance <= stopMoveDistance && hitAngle > 85)
                    stopMove = true;
                else if (hitAngle >= slopeLimit + 1f && hitAngle <= 85)
                    stopMove = true;
            }
            else if (Physics.Raycast(ray, out hitinfo, 1f, groundLayer))
            {
                var hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);
                if (hitAngle >= slopeLimit + 1f && hitAngle <= 85)
                    stopMove = true;
            }
            else
            {
                stopMove = false;
            }
        }

        #endregion

        #region Jump Methods

        protected void ControlJumpBehaviour()
        {
            if (!isJumping) return;

            jumpCounter -= Time.deltaTime;
            if (jumpCounter <= 0)
            {
                jumpCounter = 0;
                isJumping = false;
            }
            // apply extra force to the jump height   
            var vel = _rigidbody.velocity;
            vel.y = jumpHeight;
            _rigidbody.velocity = vel;
        }

        public void AirControl()
        {
            if (isGrounded) return;
            if (!jumpFwdCondition) return;

            var velY = transform.forward * jumpForward * speed;
            velY.y = _rigidbody.velocity.y;
            var velX = transform.right * jumpForward * direction;
            velX.x = _rigidbody.velocity.x;

            EnableGravityAndCollision(0f);

            if (jumpAirControl)
            {
                if (isStrafing)
                {
                    _rigidbody.velocity = new Vector3(velX.x, velY.y, _rigidbody.velocity.z);
                    var vel = transform.forward * (jumpForward * speed) + transform.right * (jumpForward * direction);
                    _rigidbody.velocity = new Vector3(vel.x, _rigidbody.velocity.y, vel.z);
                }
                else
                {
                    var vel = transform.forward * (jumpForward * speed);
                    _rigidbody.velocity = new Vector3(vel.x, _rigidbody.velocity.y, vel.z);
                }
            }
            else
            {
                var vel = transform.forward * jumpForward;
                _rigidbody.velocity = new Vector3(vel.x, _rigidbody.velocity.y, vel.z);
            }
        }

        protected bool jumpFwdCondition
        {
            get
            {
                var p1 = transform.position + _capsuleCollider.center + Vector3.up * -_capsuleCollider.height * 0.5F;
                var p2 = p1 + Vector3.up * _capsuleCollider.height;
                return Physics.CapsuleCastAll(p1, p2, _capsuleCollider.radius * 0.5f, transform.forward, 0.6f,
                           groundLayer).Length == 0;
            }
        }

        #endregion

        #region Ground Check                

        protected virtual void CheckGround()
        {
            CheckGroundDistance();

            if (isDead || customAction)
            {
                isGrounded = true;
                return;
            }

            // change the physics material to very slip when not grounded
            _capsuleCollider.material = isGrounded && GroundAngle() <= slopeLimit + 1 ? frictionPhysics : slippyPhysics;

            if (!isJumping && isGrounded && input == Vector2.zero)
            {
                _rigidbody.drag = 1000;
                _capsuleCollider.material = maxFrictionPhysics;
            }
            else if (isGrounded && input != Vector2.zero)
            {
                _rigidbody.drag = 0;
                _capsuleCollider.material = frictionPhysics;
            }
            else
            {
                _rigidbody.drag = 0;
                _capsuleCollider.material = slippyPhysics;
            }

            // we don't want to stick the character grounded if one of these bools is true
            var checkGroundConditions = !isRolling;

            var magVel = (float) Math.Round(new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z).magnitude, 2);
            //var magVel = (float)System.Math.Round(new Vector3(input.x, 0, input.y).magnitude, 2);

            magVel = Mathf.Clamp(magVel, 0, 1);

            var groundCheckDistance = groundMinDistance;
            if (magVel > 0.25f) groundCheckDistance = groundMaxDistance;
            if (checkGroundConditions)
            {
                // clear the checkground to free the character to attack on air
                var onStep = StepOffset();
                //Debug.Log($"groundDistance{groundDistance}");
                if (groundDistance <= 0.05f)
                {
                    isGrounded = true;
                    Sliding();
                }
                else
                {
                    if (groundDistance >= groundCheckDistance)
                    {
                        isGrounded = false;
                        // check vertical velocity
                        verticalVelocity = _rigidbody.velocity.y;
                        // apply extra gravity when falling
                        if (!onStep && !isJumping)
                            _rigidbody.AddForce(transform.up * extraGravity * Time.deltaTime, ForceMode.VelocityChange);
                    }
                    else if (!onStep && !isJumping)
                    {
                        _rigidbody.AddForce(transform.up * (extraGravity * 2 * Time.deltaTime),
                            ForceMode.VelocityChange);
                    }
                }
            }
        }

        protected void CheckGroundDistance()
        {
            if (isDead) return;
            if (_capsuleCollider != null)
            {
                // radius of the SphereCast
                var radius = _capsuleCollider.radius * 0.9f;
                var dist = 10f;
                // position of the SphereCast origin starting at the base of the capsule
                var pos = transform.position + Vector3.up * _capsuleCollider.radius;
                // ray for RayCast
                var ray1 = new Ray(transform.position + new Vector3(0, colliderHeight / 2, 0), Vector3.down);
                // ray for SphereCast
                var ray2 = new Ray(pos, -Vector3.up);
                // raycast for check the ground distance
                if (Physics.Raycast(ray1, out groundHit, colliderHeight / 2 + 2f, groundLayer))
                    dist = transform.position.y - groundHit.point.y;
#if (UNITY_EDITOR)
                {
                    var startPos = transform.position + new Vector3(0, colliderHeight / 2, 0);
                    var endPos = startPos + (colliderHeight / 2 + 2f) * Vector3.down;
                    Debug.DrawLine(startPos, endPos, Color.red);
                }
#endif
                // sphere cast around the base of the capsule to check the ground distance
                if (Physics.SphereCast(ray2, radius, out groundHit, _capsuleCollider.radius + 2f, groundLayer))
                {
                    // check if sphereCast distance is small than the ray cast distance
                    if (dist > groundHit.distance - _capsuleCollider.radius * 0.1f)
                        dist = groundHit.distance - _capsuleCollider.radius * 0.1f;
#if (UNITY_EDITOR)
                    {
                        var startPos2 = pos;
                        var endPos2 = startPos2 + groundHit.distance * Vector3.down;
                        Debug.DrawLine(startPos2, endPos2, Color.blue);
                        Handles.color = Color.white;
                    }
#endif
                }
                groundDistance = (float) Math.Round(dist, 2);
            }
        }

        /// <summary>
        /// Return the ground angle
        /// </summary>
        /// <returns></returns>
        public virtual float GroundAngle()
        {
            var groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);
            return groundAngle;
        }

        /// <summary>
        /// Return the angle of ground based on movement direction
        /// </summary>
        /// <returns></returns>
        public virtual float GroundAngleFromDirection()
        {
            var dir = isStrafing && input.magnitude > 0
                ? (transform.right * input.x + transform.forward * input.y).normalized
                : transform.forward;
            var movimentAngle = Vector3.Angle(dir, groundHit.normal) - 90;
            return movimentAngle;
        }

        /// <summary>
        /// Prototype to align capsule collider with surface normal
        /// </summary>
        protected virtual void AlignWithSurface()
        {
            var ray = new Ray(transform.position, -transform.up);
            RaycastHit hit;
            var surfaceRot = transform.rotation;

            if (Physics.Raycast(ray, out hit, 1.5f, groundLayer))
                surfaceRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.localRotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, surfaceRot, 10f * Time.deltaTime);
        }

        private void Sliding()
        {
            var onStep = StepOffset();
            var groundAngleTwo = 0f;
            RaycastHit hitinfo;
            var ray = new Ray(transform.position, -transform.up);

            if (Physics.Raycast(ray, out hitinfo, 1f, groundLayer))
                groundAngleTwo = Vector3.Angle(Vector3.up, hitinfo.normal);

            if (GroundAngle() > slopeLimit + 1f && GroundAngle() <= 85 &&
                groundAngleTwo > slopeLimit + 1f && groundAngleTwo <= 85 &&
                groundDistance <= 0.05f && !onStep)
            {
                isSliding = true;
                isGrounded = false;
                var slideVelocity = (GroundAngle() - slopeLimit) * 2f;
                slideVelocity = Mathf.Clamp(slideVelocity, 0, 10);
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, -slideVelocity, _rigidbody.velocity.z);
            }
            else
            {
                isSliding = false;
                isGrounded = true;
            }
        }

        private bool StepOffset()
        {
            if (input.sqrMagnitude < 0.1 || !isGrounded) return false;

            var _hit = new RaycastHit();
            var _movementDirection = isStrafing && input.magnitude > 0
                ? (transform.right * input.x + transform.forward * input.y).normalized
                : transform.forward;
            var rayStep =
                new Ray(
                    transform.position + new Vector3(0, stepOffsetEnd, 0) +
                    _movementDirection * (_capsuleCollider.radius + 0.05f), Vector3.down);

            if (Physics.Raycast(rayStep, out _hit, stepOffsetEnd - stepOffsetStart, groundLayer) &&
                !_hit.collider.isTrigger)
                if (_hit.point.y >= transform.position.y && _hit.point.y <= transform.position.y + stepOffsetEnd)
                {
                    var _speed = Mathf.Clamp(input.magnitude, 0, 1f);
                    var velocityDirection = _hit.point - transform.position;
                    var vel = _rigidbody.velocity;
                    vel.y = (velocityDirection * stepSmooth * (_speed * (velocity > 1 ? velocity : 1))).y;
                    _rigidbody.velocity = vel;
                    return true;
                }
            return false;
        }

        #endregion

        #region Colliders Check

        private void ControlCapsuleHeight()
        {
            if (isCrouching || isRolling || landHigh)
            {
                _capsuleCollider.center = colliderCenter / 1.5f;
                _capsuleCollider.height = colliderHeight / 1.5f;
            }
            else
            {
                // back to the original values
                _capsuleCollider.center = colliderCenter;
                _capsuleCollider.radius = colliderRadius;
                _capsuleCollider.height = colliderHeight;
            }
        }

        /// <summary>
        /// Disables rigibody gravity, turn the capsule collider trigger and reset all input from the animator.
        /// </summary>
        public void DisableGravityAndCollision()
        {
            animator.SetFloat("InputHorizontal", 0f);
            animator.SetFloat("InputVertical", 0f);
            animator.SetFloat("VerticalVelocity", 0f);
            _rigidbody.useGravity = false;
            _capsuleCollider.isTrigger = true;
        }

        /// <summary>
        /// Turn rigidbody gravity on the uncheck the capsulle collider as Trigger when the animation has finish playing
        /// </summary>
        /// <param name="normalizedTime">Check the value of your animation Exit Time and insert here</param>
        public void EnableGravityAndCollision( float normalizedTime )
        {
            // enable collider and gravity at the end of the animation
            if (baseLayerInfo.normalizedTime >= normalizedTime)
            {
                _capsuleCollider.isTrigger = false;
                _rigidbody.useGravity = true;
            }
        }

        #endregion

        #region Camera Methods

        public virtual void Rotate( Quaternion rot, bool ignoreLerp = false )
        {
            if (ignoreLerp)
                transform.rotation = rot;
            else
                transform.rotation =
                    Quaternion.Lerp(transform.rotation, rot, strafeSpeed.rotationSpeed * Time.deltaTime);
            targetDirection = transform.forward;
        }

        public virtual void RotateToTarget( Transform target )
        {
            if (target)
            {
                var rot = Quaternion.LookRotation(target.position - transform.position);
                var newPos = new Vector3(transform.eulerAngles.x, rot.eulerAngles.y, transform.eulerAngles.z);
                targetRotation = Quaternion.Euler(newPos);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newPos),
                    strafeSpeed.rotationSpeed * Time.deltaTime);
            }
        }

        public virtual void RotateToDirection( Vector3 direction, bool ignoreLerp = false )
        {
            var rot = Quaternion.LookRotation(direction);
            var newPos = new Vector3(transform.eulerAngles.x, rot.eulerAngles.y, transform.eulerAngles.z);
            targetRotation = Quaternion.Euler(newPos);
            //if (ignoreLerp)
            //{
            //    transform.rotation = Quaternion.Euler(newPos);
            //}
            //else
            //{
            //    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newPos), strafeSpeed.rotationSpeed * Time.deltaTime);
            //}
            targetDirection = direction;
        }

        public virtual void DirectRotateToDirection( Vector3 direction, bool ignoreLerp = false )
        {
            var rot = Quaternion.LookRotation(direction);
            var newPos = new Vector3(transform.eulerAngles.x, rot.eulerAngles.y, transform.eulerAngles.z);
            targetRotation = Quaternion.Euler(newPos);
            if (ignoreLerp)
                transform.rotation = targetRotation;
            else
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation,
                    strafeSpeed.rotationSpeed * Time.deltaTime);
            targetDirection = direction;
        }

        /// <summary>
        /// Update the targetDirection variable using referenceTransform or just input.Rotate by word  the referenceDirection
        /// </summary>
        /// <param name="referenceTransform"></param>
        public virtual void UpdateTargetDirection( Transform referenceTransform = null )
        {
            if (referenceTransform && !rotateByWorld)
            {
                var forward = keepDirection
                    ? referenceTransform.forward
                    : referenceTransform.TransformDirection(Vector3.forward);
                forward.y = 0;

                forward = keepDirection ? forward : referenceTransform.TransformDirection(Vector3.forward);
                forward.y = 0; //set to 0 because of referenceTransform rotation on the X axis

                //get the right-facing direction of the referenceTransform
                var right = keepDirection
                    ? referenceTransform.right
                    : referenceTransform.TransformDirection(Vector3.right);

                // determine the direction the player will face based on input and the referenceTransform's right and forward directions
                targetDirection = input.x * right + input.y * forward;
            }
            else
            {
                targetDirection = keepDirection ? targetDirection : new Vector3(input.x, 0, input.y);
            }
        }

        #endregion

        #region Ragdoll 

        private void CheckRagdoll()
        {
            if (ragdollVel == 0) return;

            // check your verticalVelocity and assign a value on the variable RagdollVel at the Player Inspector
            if (verticalVelocity <= ragdollVel && groundDistance <= 0.1f) onActiveRagdoll.Invoke();
        }

        public override void ResetRagdoll()
        {
            lockMovement = false;
            verticalVelocity = 0f;
            ragdolled = false;
            _rigidbody.WakeUp();
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            _rigidbody.useGravity = true;
            _rigidbody.isKinematic = false;
            _capsuleCollider.isTrigger = false;
        }

        public override void EnableRagdoll()
        {
            animator.SetFloat("InputHorizontal", 0f);
            animator.SetFloat("InputVertical", 0f);
            animator.SetFloat("VerticalVelocity", 0f);
            ragdolled = true;
            _capsuleCollider.isTrigger = true;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;

            lockMovement = true;
        }

        #endregion

        #region Debug

        public virtual string DebugInfo( string additionalText = "" )
        {
            var debugInfo = string.Empty;
            if (debugWindow)
            {
                var delta = Time.smoothDeltaTime;
                var fps = 1 / delta;

                debugInfo =
                    "FPS " + fps.ToString("#,##0 fps") + "\n" +
                    "inputVertical = " + input.y.ToString("0.0") + "\n" +
                    "inputHorizontal = " + input.x.ToString("0.0") + "\n" +
                    "verticalVelocity = " + verticalVelocity.ToString("0.00") + "\n" +
                    "groundDistance = " + groundDistance.ToString("0.00") + "\n" +
                    "groundAngle = " + GroundAngle().ToString("0.00") + "\n" +
                    "useGravity = " + _rigidbody.useGravity + "\n" +
                    "colliderIsTrigger = " + _capsuleCollider.isTrigger + "\n" +
                    "\n" + "--- Movement Bools ---" + "\n" +
                    "onGround = " + isGrounded + "\n" +
                    "lockMovement = " + lockMovement + "\n" +
                    "stopMove = " + stopMove + "\n" +
                    "sliding = " + isSliding + "\n" +
                    "sprinting = " + isSprinting + "\n" +
                    "crouch = " + isCrouching + "\n" +
                    "strafe = " + isStrafing + "\n" +
                    "landHigh = " + landHigh + "\n" +
                    "\n" + "--- Actions Bools ---" + "\n" +
                    "roll = " + isRolling + "\n" +
                    "isJumping = " + isJumping + "\n" +
                    "ragdoll = " + ragdolled + "\n" +
                    "actions = " + actions + "\n" +
                    "customAction = " + customAction + "\n" + additionalText;
            }
            return debugInfo;
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying && debugWindow)
            {
                // debug auto crouch
                var posHead = transform.position + Vector3.up * (colliderHeight * 0.5f - colliderRadius);
                var ray1 = new Ray(posHead, Vector3.up);
                Gizmos.DrawWireSphere(ray1.GetPoint(headDetect - colliderRadius * 0.1f), colliderRadius * 0.9f);
                // debug stopmove            
                var ray3 = new Ray(transform.position + new Vector3(0, stopMoveHeight, 0), transform.forward);
                Debug.DrawRay(ray3.origin, ray3.direction * (_capsuleCollider.radius + stopMoveDistance), Color.blue);
                // debug slopelimit            
                var ray4 = new Ray(transform.position + new Vector3(0, colliderHeight / 3.5f, 0), transform.forward);
                Debug.DrawRay(ray4.origin, ray4.direction * 1f, Color.cyan);
                // debug stepOffset
                var dir = isStrafing && input.magnitude > 0
                    ? (transform.right * input.x + transform.forward * input.y).normalized
                    : transform.forward;
                var ray5 = new Ray(
                    transform.position + new Vector3(0, stepOffsetEnd, 0) + dir * (_capsuleCollider.radius + 0.05f),
                    Vector3.down);
                Debug.DrawRay(ray5.origin, ray5.direction * (stepOffsetEnd - stepOffsetStart), Color.yellow);
            }
        }

        #endregion
    }
}