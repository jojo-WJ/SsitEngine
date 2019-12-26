/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/3 11:28:07                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Framework.SsitInput;
using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    /// Acts as a common base class for any type of Unity input. Works with keyboard/mouse, controller, and mobile input.
    /// </summary>
    public class CrossPlatInput : SsitInput
    {
        [Tooltip("Should the cursor be disabled?")] [SerializeField]
        protected bool m_DisableCursor;

        [Tooltip("Should the cursor be enabled when the escape key is pressed?")] [SerializeField]
        protected bool m_EnableCursorWithEscape;

        [Tooltip("Specifies if any input type should be forced.")] [SerializeField]
        protected InputDevice m_ForceInput;

        private InputBase m_Input;
        private List<string> m_JoystickDownList;
        private HashSet<string> m_JoystickDownSet;

        [Tooltip("If the cursor is enabled with escape should the look vector be prevented from updating?")]
        [SerializeField]
        protected bool m_PreventLookVectorChanges = true;

        private List<string> m_ToAddJoystickDownList;
        private bool m_UseVirtualInput;

        public virtual Vector2 mousePosition
        {
            get
            {
                var inputDevice = m_ForceInput;
                switch (inputDevice)
                {
                    case InputDevice.MouseKeyboard:
                        return Input.mousePosition;
                    case InputDevice.Joystick:
                        //joystickMousePos.x += Input.GetAxis("RightAnalogHorizontal") * joystickSensitivity;
                        //joystickMousePos.x = Mathf.Clamp(joystickMousePos.x, -(Screen.width * 0.5f), (Screen.width * 0.5f));
                        //joystickMousePos.y += Input.GetAxis("RightAnalogVertical") * joystickSensitivity;
                        //joystickMousePos.y = Mathf.Clamp(joystickMousePos.y, -(Screen.height * 0.5f), (Screen.height * 0.5f));
                        //var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                        //var result = joystickMousePos + screenCenter;
                        //result.x = Mathf.Clamp(result.x, 0, Screen.width);
                        //result.y = Mathf.Clamp(result.y, 0, Screen.height);
                        return Vector2.zero; //result;
                    case InputDevice.Mobile:
                        return Input.GetTouch(0).deltaPosition;

                    default: return Input.mousePosition;
                }
            }
        }

        #region Input

        /// <summary>
        /// Enables or disables gameplay input. An example of when it will not be enabled is when there is a fullscreen UI over the main camera.
        /// </summary>
        /// <param name="enable">True if the input is enabled.</param>
        protected override void EnableGameplayInput( bool enable )
        {
            base.EnableGameplayInput(enable);

            if (enable && m_DisableCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        #endregion

        /// <summary>
        /// Returns true if the pointer is over a UI element.
        /// </summary>
        /// <returns>True if the pointer is over a UI element.</returns>
        public override bool IsPointerOverUI()
        {
            // The input will always be over a UI element with virtual inputs.
            if (m_Input is VirtualInput) return false;
            return base.IsPointerOverUI();
        }

        #region Property

        /// <summary>
        /// 禁用光标
        /// </summary>
        public bool DisableCursor
        {
            get => m_DisableCursor;
            set
            {
                if (m_Input is VirtualInput) m_DisableCursor = false;
                m_DisableCursor = value;
                if (m_DisableCursor)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
                else if (!m_DisableCursor)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        /// <summary>
        /// 按键ESC显示光标
        /// </summary>
        public bool EnableCursorWithEscape
        {
            get => m_EnableCursorWithEscape;
            set => m_EnableCursorWithEscape = value;
        }

        /// <summary>
        /// 按键ESC禁用玩家控制
        /// </summary>
        public bool PreventLookMovementWithEscape
        {
            get => m_PreventLookVectorChanges;
            set => m_PreventLookVectorChanges = value;
        }

        #endregion

        #region Mono

        public void Init( InputDevice forceInput )
        {
            m_ForceInput = forceInput;
            m_UseVirtualInput = m_ForceInput == InputDevice.Mobile || m_ForceInput == InputDevice.Joystick;
#if !UNITYEDITOR && (UNITY_IPHONE || UNITY_ANDROID || UNITY_WP_8_1 || UNITY_BLACKBERRY)
            if (m_ForceInput != ForceInputType.Standalone) {
                var virtualControlsManager = FindObjectOfType<VirtualControlsManager>();
                m_UseVirtualInput = virtualControlsManager != null;
            }
#endif
            if (m_UseVirtualInput)
            {
                m_Input = new VirtualInput();
                // The cursor must be enabled for virtual controls to allow the drag events to occur.
                m_DisableCursor = false;
            }
            else
            {
                m_Input = new StandaloneInput();
            }
            m_Input.Initialize(this);
        }

        /// <summary>
        /// The component has been enabled.
        /// </summary>
        private void OnEnable()
        {
            if (!m_UseVirtualInput && m_DisableCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        /// <summary>
        /// Update the joystick and cursor state values.
        /// </summary>
        private void LateUpdate()
        {
            if (!m_UseVirtualInput)
            {
                // The joystick is no longer down after the axis is 0.
                if (IsControllerConnected())
                {
                    if (m_JoystickDownList != null)
                        for (var i = m_JoystickDownList.Count - 1; i > -1; --i)
                            if (m_Input.GetAxisRaw(m_JoystickDownList[i]) <= 0.1f)
                            {
                                m_JoystickDownSet.Remove(m_JoystickDownList[i]);
                                m_JoystickDownList.RemoveAt(i);
                            }
                    // GetButtonDown doesn't immediately add the button name to the set to prevent the GetButtonDown from returning false
                    // if it is called twice within the same frame.
                    if (m_ToAddJoystickDownList != null && m_ToAddJoystickDownList.Count > 0)
                    {
                        if (m_JoystickDownList == null) m_JoystickDownList = new List<string>();
                        for (var i = 0; i < m_ToAddJoystickDownList.Count; ++i)
                        {
                            m_JoystickDownSet.Add(m_ToAddJoystickDownList[i]);
                            m_JoystickDownList.Add(m_ToAddJoystickDownList[i]);
                        }
                        m_ToAddJoystickDownList.Clear();
                    }
                }

                // Enable the cursor if the escape key is pressed. Disable the cursor if it is visbile but should be disabled upon press.
                if (m_EnableCursorWithEscape && Input.GetKeyDown(KeyCode.Escape))
                {
                    DisableCursor = !DisableCursor;
                    //Cursor.visible = true;
                    if (m_PreventLookVectorChanges) 
                        OnApplicationFocus(false);
                }
                /*else if (Cursor.visible && m_DisableCursor && !IsPointerOverUI() && (UnityEngine.Input.GetKeyDown(KeyCode.Mouse0) || UnityEngine.Input.GetKeyDown(KeyCode.Mouse1)))
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    if (m_PreventLookVectorChanges)
                    {
                        OnApplicationFocus(true);
                    }
                }*/
#if UNITY_EDITOR
                // The cursor should be visible when the game is paused.
                if (!Cursor.visible && Time.deltaTime == 0)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
#endif
            }
        }

        /// <summary>
        /// Does the game have focus?
        /// </summary>
        /// <param name="hasFocus">True if the game has focus.</param>
        protected override void OnApplicationFocus( bool hasFocus )
        {
            base.OnApplicationFocus(hasFocus);

            if (hasFocus && m_DisableCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        #endregion

        #region Input Internal

        /// <summary>
        /// Internal method which returns true if the button is being pressed.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True of the button is being pressed.</returns>
        protected override bool GetButtonInternal( string name )
        {
            if (m_Input.GetButton(name, InputBase.ButtonAction.GetButton)) return true;
            if (IsControllerConnected() && Mathf.Abs(m_Input.GetAxisRaw(name)) == 1) return true;
            return false;
        }

        /// <summary>
        /// Internal method which returns true if the button was pressed this frame.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True if the button is pressed this frame.</returns>
        protected override bool GetButtonDownInternal( string name )
        {
            if (IsControllerConnected() && Mathf.Abs(m_Input.GetAxisRaw(name)) == 1)
            {
                if (m_JoystickDownSet == null) m_JoystickDownSet = new HashSet<string>();
                // The button should only be considered down on the first frame.
                if (m_JoystickDownSet.Contains(name)) return false;
                if (m_ToAddJoystickDownList == null) m_ToAddJoystickDownList = new List<string>();
                m_ToAddJoystickDownList.Add(name);
                return true;
            }
            return m_Input.GetButton(name, InputBase.ButtonAction.GetButtonDown);
        }

        /// <summary>
        /// Internal method which returnstrue if the button is up.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <returns>True if the button is up.</returns>
        protected override bool GetButtonUpInternal( string name )
        {
            if (IsControllerConnected())
            {
                if (m_JoystickDownSet == null) m_JoystickDownSet = new HashSet<string>();
                if (m_JoystickDownSet.Contains(name) && m_Input.GetAxisRaw(name) <= 0.1f)
                {
                    m_JoystickDownSet.Remove(name);
                    return true;
                }
                return false;
            }
            return m_Input.GetButton(name, InputBase.ButtonAction.GetButtonUp);
        }

        /// <summary>
        /// Internal method which returns the value of the axis with the specified name.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the axis.</returns>
        protected override float GetAxisInternal( string name )
        {
            return m_Input.GetAxis(name);
        }

        /// <summary>
        /// Internal method which returns the value of the raw axis with the specified name.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the raw axis.</returns>
        protected override float GetAxisRawInternal( string name )
        {
            return m_Input.GetAxisRaw(name);
        }

        #endregion

        #region 平台控制器

        /// <summary>
        /// Associates the VirtualControlsManager with the VirtualInput object.
        /// </summary>
        /// <param name="virtualControlsManager">The VirtualControlsManager to associate with the VirtualInput object.</param>
        /// <returns>True if the virtual controls were registered.</returns>
        public bool RegisterVirtualControlsManager( VirtualControlsManager virtualControlsManager )
        {
            if (m_Input is VirtualInput)
            {
                (m_Input as VirtualInput).RegisterVirtualControlsManager(virtualControlsManager);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the VirtualControlsManager association.
        /// </summary>
        public void UnegisterVirtualControlsManager()
        {
            if (m_Input is VirtualInput) (m_Input as VirtualInput).UnregisterVirtualControlsManager();
        }

        #endregion
    }
}