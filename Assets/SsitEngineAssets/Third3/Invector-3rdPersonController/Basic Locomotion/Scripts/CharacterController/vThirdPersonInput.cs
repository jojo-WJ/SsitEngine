using System.Collections;
using Framework.SsitInput;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;

#endif

namespace Invector.CharacterController
{
    public class MoveSyncAnimHandle : UnityEvent<float, float>
    {
    }

    [vClassHeader("Input Manager", iconName = "inputIcon")]
    public class vThirdPersonInput : vMonoBehaviour
    {
        public MoveSyncAnimHandle OmMoveSyncAnimHandle = new MoveSyncAnimHandle();

        #region Variables

        public bool lockInput;
        public bool lockCamera;
        public bool rotateToCameraWhileStrafe = true;

        [Header("Default Inputs")]
        public GenericInput horizontalInput = new GenericInput("Horizontal", "LeftAnalogHorizontal", "Horizontal");

        public GenericInput verticallInput = new GenericInput("Vertical", "LeftAnalogVertical", "Vertical");
        public GenericInput jumpInput = new GenericInput("Space", "X", "X");
        public GenericInput rollInput = new GenericInput("Q", "B", "B");
        public GenericInput strafeInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");
        public GenericInput sprintInput = new GenericInput("LeftShift", "LeftStickClick", "LeftStickClick");
        public GenericInput crouchInput = new GenericInput("C", "Y", "Y");

        [Header("Camera Settings")]
        public GenericInput rotateCameraXInput = new GenericInput("Mouse X", "RightAnalogHorizontal", "Mouse X");

        public GenericInput rotateCameraYInput = new GenericInput("Mouse Y", "RightAnalogVertical", "Mouse Y");
        public GenericInput cameraZoomInput = new GenericInput("Mouse ScrollWheel", "", "");
        [HideInInspector] public vThirdPersonCamera tpCamera; // acess camera info                
        [HideInInspector] public string customCameraState; // generic string to change the CameraState        

        [HideInInspector]
        public string customlookAtPoint; // generic string to change the CameraPoint of the Fixed Point Mode        

        [HideInInspector] public bool changeCameraState; // generic bool to change the CameraState        

        [HideInInspector]
        public bool smoothCameraState; // generic bool to know if the state will change with or without lerp  

        [HideInInspector] public bool keepDirection; // keep the current direction in case you change the cameraState
        protected Vector2 oldInput;

        public UnityEvent OnLateUpdate;
        // isometric cursor

        [HideInInspector] public vThirdPersonController cc; // access the ThirdPersonController component
        [HideInInspector] public vHUDController hud; // acess vHUDController component        
        protected bool updateIK;
        protected bool isInit;

        public Animator animator
        {
            get
            {
                if (cc == null) cc = GetComponent<vThirdPersonController>();
                if (cc.animator == null) return GetComponent<Animator>();
                return cc.animator;
            }
        }

        #endregion

        #region Initialize Character, Camera & HUD when LoadScene

        protected virtual void Start()
        {
            cc = GetComponent<vThirdPersonController>();

            if (cc != null)
                cc.Init();

            if (vThirdPersonController.instance == cc || vThirdPersonController.instance == null)
            {
#if UNITY_5_4_OR_NEWER
                SceneManager.sceneLoaded += OnLevelFinishedLoading;
#endif

                StartCoroutine(CharacterInit());
            }
        }

#if UNITY_5_4_OR_NEWER
        private void OnLevelFinishedLoading( Scene scene, LoadSceneMode mode )
        {
            try
            {
                StartCoroutine(CharacterInit());
            }
            catch
            {
                SceneManager.sceneLoaded -= OnLevelFinishedLoading;
            }
        }
#else
        public void OnLevelWasLoaded(int level)
        {
            try
            {
                StartCoroutine(CharacterInit());
            }
            catch
            {

            }
        }
#endif

        protected virtual IEnumerator CharacterInit()
        {
            yield return new WaitForEndOfFrame();
            if (tpCamera == null)
            {
                tpCamera = FindObjectOfType<vThirdPersonCamera>();
                if (tpCamera && tpCamera.target != transform) tpCamera.SetMainTarget(transform);
            }
            if (hud == null && vHUDController.instance != null)
            {
                hud = vHUDController.instance;
                hud.Init(cc);
            }
        }

        #endregion

        protected virtual void LateUpdate()
        {
            if (cc == null || lockInput || Time.timeScale == 0) return;
            if (!updateIK && animator.updateMode == AnimatorUpdateMode.AnimatePhysics) return;
            CameraInput(); // update camera input           
            UpdateCameraStates(); // update camera states
            OnLateUpdate.Invoke();
            updateIK = false;
        }

        protected virtual void FixedUpdate()
        {
            cc.AirControl(); // update air behaviour
            updateIK = true;
        }

        protected virtual void Update()
        {
            if (cc == null || lockInput || Time.timeScale == 0)
            {
                cc.input = Vector2.zero;
                cc.speed = 0f;
                cc.animator.SetFloat("InputVertical", 0f, 0.2f, Time.deltaTime);
                cc.animator.SetFloat("InputMagnitude", 0f, 0.2f, Time.deltaTime);
                return;
            }

            InputHandle(); // update input methods
            cc.UpdateMotor(); // call ThirdPersonMotor methods
            cc.UpdateAnimator(); // call ThirdPersonAnimator methods
            UpdateHUD(); // update hud graphics            
        }

        protected virtual void InputHandle()
        {
            ExitGameInput();

            if (!cc.lockMovement && !cc.ragdolled)
            {
                MoveCharacter();
                SprintInput();
                CrouchInput();
                StrafeInput();
                JumpInput();
                RollInput();
            }
        }

        #region Generic Methods

        // you can use these methods with Playmaker or AdventureCreator to have better control on cutscenes and events.

        /// <summary>
        /// Lock all the Input from the Player
        /// </summary>
        /// <param name="value"></param>
        public void LockInput( bool value )
        {
            lockInput = value;
            if (value) cc.input = Vector2.zero;
        }

        /// <summary>
        /// Show/Hide Cursor
        /// </summary>
        /// <param name="value"></param>
        public void ShowCursor( bool value )
        {
            Cursor.visible = value;
        }

        /// <summary>
        /// Lock the Camera Input
        /// </summary>
        /// <param name="value"></param>
        public void LockCamera( bool value )
        {
            lockCamera = value;
        }

        /// <summary>
        /// Limits the character to walk only, useful for cutscenes and 'indoor' areas
        /// </summary>
        /// <param name="value"></param>
        public void LimitToWalk( bool value )
        {
            cc.freeSpeed.walkByDefault = value;
            cc.strafeSpeed.walkByDefault = value;
        }

        #endregion

        #region Basic Locomotion Inputs      

        public virtual void MoveCharacter( Vector3 position, bool rotateToDirection = true )
        {
            var dir = position - transform.position;
            var targetDir = cc.isStrafing ? transform.InverseTransformDirection(dir).normalized : dir.normalized;
            cc.input.x = targetDir.x;
            cc.input.y = targetDir.z;
            if (!keepDirection)
                oldInput = cc.input;
            if (rotateToDirection && cc.isStrafing)
            {
                targetDir.y = 0;
                cc.RotateToDirection(dir);
            }
            OmMoveSyncAnimHandle.Invoke(targetDir.x, targetDir.z);
        }

        protected virtual void MoveCharacter()
        {
            // gets input from mobile      
            var x = horizontalInput.GetAxis();
            var y = verticallInput.GetAxis();

            cc.input.x = horizontalInput.GetAxis();
            cc.input.y = verticallInput.GetAxis();
            // update oldInput to compare with current Input if keepDirection is true
            if (!keepDirection)
                oldInput = cc.input;
            OmMoveSyncAnimHandle.Invoke(x, y);
        }

        protected virtual void StrafeInput()
        {
            if (strafeInput.GetButtonDown())
                cc.Strafe();
        }

        protected virtual void SprintInput()
        {
            if (sprintInput.GetButtonDown())
                cc.Sprint(true);
            else
                cc.Sprint(false);
        }

        protected virtual void CrouchInput()
        {
            if (crouchInput.GetButtonDown())
                cc.Crouch();
        }

        protected virtual void JumpInput()
        {
            if (jumpInput.GetButtonDown())
                cc.Jump();
        }

        protected virtual void RollInput()
        {
            if (rollInput.GetButtonDown())
                cc.Roll();
        }

        protected virtual void ExitGameInput()
        {
            // just a example to quit the application 
            //if (Input.GetKeyDown(KeyCode.Escape))
            //{
            //    if (!Cursor.visible)
            //        Cursor.visible = true;
            //    else
            //        Application.Quit();
            //}
        }

        protected virtual void OnTriggerStay( Collider other )
        {
            cc.CheckTriggers(other);
        }

        protected virtual void OnTriggerExit( Collider other )
        {
            cc.CheckTriggerExit(other);
        }

        #endregion

        #region Camera Methods

        public virtual void CameraInput()
        {
            if (!Camera.main) Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
            if (!keepDirection) cc.UpdateTargetDirection(Camera.main.transform);
            RotateWithCamera(Camera.main.transform);

            if (tpCamera == null || lockCamera)
                return;
            var Y = rotateCameraYInput.GetAxis();
            var X = rotateCameraXInput.GetAxis();
            var zoom = cameraZoomInput.GetAxis();

            tpCamera.RotateCamera(X, Y);
            tpCamera.Zoom(zoom);

            // change keedDirection from input diference
            if (keepDirection && Vector2.Distance(cc.input, oldInput) > 0.2f) keepDirection = false;
        }

        protected virtual void UpdateCameraStates()
        {
            // CAMERA STATE - you can change the CameraState here, the bool means if you want lerp of not, make sure to use the same CameraState String that you named on TPCameraListData

            if (tpCamera == null)
            {
                tpCamera = FindObjectOfType<vThirdPersonCamera>();
                if (tpCamera == null)
                    return;
                if (tpCamera)
                {
                    tpCamera.SetMainTarget(transform);
                    tpCamera.Init();
                }
            }

            if (changeCameraState && !cc.isStrafing)
                tpCamera.ChangeState(customCameraState, customlookAtPoint, smoothCameraState);
            else if (cc.isCrouching)
                tpCamera.ChangeState("Crouch", true);
            else if (cc.isStrafing)
                tpCamera.ChangeState("Strafing", true);
            else
                tpCamera.ChangeState("Default", true);
        }

        protected virtual void RotateWithCamera( Transform cameraTransform )
        {
            if (rotateToCameraWhileStrafe && cc.isStrafing && !cc.actions && !cc.lockMovement)
            {
                // smooth align character with aim position               
                if (tpCamera != null && tpCamera.lockTarget)
                    cc.RotateToTarget(tpCamera.lockTarget);
                // rotate the camera around the character and align with when the character move
                else //if (cc.input != Vector2.zero) 保持方向一直朝向相机方向
                    cc.RotateWithAnotherTransform(cameraTransform);
            }
        }

        #endregion

        #region HUD       

        public virtual void UpdateHUD()
        {
            if (hud == null)
                return;

            hud.UpdateHUD(cc);
        }

        #endregion
    }
}