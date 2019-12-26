using Framework.SsitInput;
using SsitEngine.Core.ReferencePool;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    /// 按键驱动器
    /// </summary>
    public class DeviceKeyboard : InputDeviceBase
    {
        /// <summary>
        /// 触摸驱动名称
        /// </summary>
        public const string DEVICEKEYBOARD = "DEVICEKEYBOARD";

        private bool cameraDrag = false;
        [Header("Camera Settings Customize")] public int cameraDragInput = 0;
        private ThirdPersonController cc;

        [HideInInspector] public bool changeCameraState; // generic bool to change the CameraState        
        public GenericInput crouchInput = new GenericInput("C", "Y", "Y");
        [HideInInspector] public string customCameraState; // generic string to change the CameraState        

        [HideInInspector]
        public string customlookAtPoint; // generic string to change the CameraPoint of the Fixed Point Mode        


        public GenericInput horizontalArrow = new GenericInput("HorizontalArrow", "HorizontalArrow", "HorizontalArrow");

        public GenericInput horizontalArrowInput = new GenericInput("Horizontal", "LeftAnalogHorizontal", "Horizontal");

        [Header("Default Inputs")]
        public GenericInput horizontalInput = new GenericInput("Horizontal", "LeftAnalogHorizontal", "Horizontal");

        public GenericInput jumpInput = new GenericInput("Space", "X", "X");

        [HideInInspector] public bool keepDirection; // keep the current direction in case you change the cameraState

        //人物控制
        public bool lockCamera;

        private EnInputMode m_inputMode = EnInputMode.None;

        public GenericInput ResetInput = new GenericInput("F", "F", "F");
        public GenericInput rollInput = new GenericInput("Q", "B", "B");
        public bool rotateToCameraWhileStrafe = true;

        [HideInInspector]
        public bool smoothCameraState; // generic bool to know if the state will change with or without lerp  

        public GenericInput sprintInput = new GenericInput("LeftShift", "LeftStickClick", "LeftStickClick");
        public GenericInput strafeInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");


        //public GenericInput escInput = new GenericInput("ESC", "B", "B");
        [HideInInspector] private vThirdPersonCamera tpCamera; // acess camera info         
        protected bool updateIK = false;
        public GenericInput verticalArrow = new GenericInput("VerticalArrow", "VerticalArrow", "VerticalArrow");

        public GenericInput verticallInput = new GenericInput("Vertical", "LeftAnalogVertical", "Vertical");


        /// <summary>
        /// 创建按键驱动器
        /// </summary>
        /// <param name="inputHandlerHelper"></param>
        public DeviceKeyboard( IInputHandlerHelper inputHandlerHelper ) : base(inputHandlerHelper)
        {
            //热键与UI冲突 ==》屏蔽（功能非必要）且界面实在没人改
            if (IsDeviceSupport)
                InitKeyCodeListener();
        }

        /// <summary>
        /// 初始化按键监听列表
        /// </summary>
        protected void InitKeyCodeListener()
        {
            Framework.SsitInput.InputManager.Instance.RegisterDeviceMsg(this, (ushort) EnGlobalEvent.OnChangeInputMode,
                (ushort) EnKeyEventType.moveInput);
        }

        /// <summary>
        /// 驱动轮询
        /// </summary>
        public override void Update()
        {
            switch (m_inputMode)
            {
                case EnInputMode.Editor:
                case EnInputMode.Free:
                {
                    Facade.Instance.SendNotification((ushort) EnKeyEventType.moveInput,
                        new Vector2(horizontalInput.GetAxis(), verticallInput.GetAxis()));
                    if (ResetInput.GetButtonDown())
                        Facade.Instance.SendNotification((ushort) EnKeyEventType.resetInput, false);
                }

                    break;
                case EnInputMode.Control:
                    InputHandle();
                    break;
                case EnInputMode.Lock:
                    break;
            }
        }

        private void InputHandle()
        {
            if (!inputHandlerHelper.HasFoucs)
            {
                Facade.Instance.SendNotification((ushort) EnKeyEventType.moveInput, Vector2.zero);
                return;
            }

            Facade.Instance.SendNotification((ushort) EnKeyEventType.moveInput,
                new Vector2(horizontalInput.GetAxis(), verticallInput.GetAxis()));
            /*Facade.Instance.SendNotification((ushort) EnKeyEventType.moveArrowInput,
                new Vector2(horizontalArrow.GetAxisRaw(), verticalArrow.GetAxisRaw()));
            */


            if (sprintInput.GetButtonDown())
                Facade.Instance.SendNotification((ushort) EnKeyEventType.sprintInput, true);
            else
                Facade.Instance.SendNotification((ushort) EnKeyEventType.sprintInput, false);
            if (crouchInput.GetButtonDown())
                Facade.Instance.SendNotification((ushort) EnKeyEventType.crouchInput, false);
            if (strafeInput.GetButtonDown())
                Facade.Instance.SendNotification((ushort) EnKeyEventType.strafeInput, false);
            if (jumpInput.GetButtonDown())
                Facade.Instance.SendNotification((ushort) EnKeyEventType.jumpInput, false);
            if (rollInput.GetButtonDown())
                Facade.Instance.SendNotification((ushort) EnKeyEventType.rollInput, false);
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="moveDirection"></param>
        protected void MoveCamera( Vector3 moveDirection )
        {
            if (!inputHandlerHelper.HasFoucs)
                return;

            // 矫正平台配置限定
            var config = Engine.Instance.Platform.PlatformConfig;
            moveDirection *= config.movSpeed;
            if (!Mathf.Approximately(moveDirection.sqrMagnitude, 0))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    moveDirection *= config.movAttachSpeed;
                var eventArgs = ReferencePool.Acquire<MouseEventArgs>();
                eventArgs.SetArgs(EnMouseEventType.ScrollWheelDown, Vector2.zero, Vector2.zero, moveDirection);

                // 发送事件消息
                Facade.Instance.SendNotification((ushort) EnInputEvent.MoveCamera, eventArgs);
                // 释放引用
                ReferencePool.Release(eventArgs);
            }
        }


        public override void HandleNotification( INotification obj )
        {
            switch (obj.Id)
            {
                case (ushort) EnGlobalEvent.OnChangeInputMode:
                    OnChangeInputMode(obj);
                    break;
                case (ushort) EnKeyEventType.moveInput:
                {
                    var delta = (Vector2) obj.Body;
                    var moveDirection = Vector3.zero;
                    if (delta.x > 0)
                        moveDirection += Vector3.right;
                    else if (delta.x < 0)
                        moveDirection += Vector3.left;
                    else if (delta.y > 0)
                        moveDirection += Vector3.up /* same to forward*/;
                    else if (delta.y < 0) moveDirection += Vector3.down;
                    MoveCamera(moveDirection);
                }
                    break;
            }
        }

        private void OnChangeInputMode( INotification obj )
        {
            var temp = m_inputMode;
            m_inputMode = (EnInputMode) obj.Body;
            switch (m_inputMode)
            {
                case EnInputMode.Editor:
                case EnInputMode.Free:
                {
                    if (temp == EnInputMode.Control || temp == EnInputMode.Lock)
                        Framework.SsitInput.InputManager.Instance.RegisterDeviceMsg(this,
                            (ushort) EnKeyEventType.moveInput);
                }
                    break;
                case EnInputMode.Control:
                case EnInputMode.Lock:
                {
                    if (temp == EnInputMode.Control || temp == EnInputMode.Free)
                        //移除之前的监听
                        Framework.SsitInput.InputManager.Instance.UnRegisterDeviceMsg(this,
                            (ushort) EnKeyEventType.moveInput);
                }
                    break;
            }
        }

        /// <summary>
        /// 驱动关闭
        /// </summary>
        public override void Destroy()
        {
        }

        #region 子类重写

        /// <summary>
        /// 驱动名称
        /// </summary>
        public override string DeviceName => DEVICEKEYBOARD;

        /// <summary>
        /// 驱动支持检测
        /// </summary>
        public bool IsDeviceSupport => true;

        #endregion

        #region 按键监测

        //public Dictionary<KeyCode, UnityAction<int>> mkeycodeMaps = new Dictionary<KeyCode, UnityAction<int>>();

        //public void AddKeyObserver( int index, UnityAction<int> func )
        //{
        //    if (index >= 9)
        //    {
        //        return;
        //    }
        //    KeyCode key = (KeyCode)(index + (int)KeyCode.Alpha1);
        //    if (!mkeycodeMaps.ContainsKey(key))
        //    {
        //        mkeycodeMaps.Add(key, func);
        //    }
        //    else
        //    {
        //        mkeycodeMaps[key] = func;
        //    }
        //}

        //public void DeleteKeyObserver( int index )
        //{
        //    if (index >= 9)
        //    {
        //        return;
        //    }
        //    KeyCode key = (KeyCode)(index + (int)KeyCode.Alpha1);
        //    if (mkeycodeMaps.ContainsKey(key))
        //    {
        //        mkeycodeMaps.Remove(key);
        //    }
        //}

        //void ClickKeyCode()
        //{
        //    foreach (var kk in mkeycodeMaps)
        //    {
        //        if (UnityEngine.Input.GetKeyDown(kk.Key) && kk.Value != null)
        //        {
        //            kk.Value.Invoke((int)kk.Key - (int)KeyCode.Alpha1);
        //        }
        //    }

        //    if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
        //    {
        //        Facade.Instance.SendNotification((ushort)ConstNotification.EnterKeyDown);
        //    }
        //}

        #endregion
    }
}