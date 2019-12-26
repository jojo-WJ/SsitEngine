using System.Collections.Generic;
using Framework.SsitInput;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Core.ReferencePool;
using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    /// 鼠标驱动器
    /// </summary>
    public class DeviceMouse : InputDeviceBase
    {
        /// <summary>
        /// 触摸驱动名称
        /// </summary>
        public const string DEVICEMOUSE = "DEVICEMOUSE";

        /// <summary>
        /// 双击检测时长
        /// </summary>
        private readonly float doubleDuration = 0.2f;

        /// <summary>
        /// 长按检测时长
        /// </summary>
        private readonly float hoverDuration = 0.35f;

        /// <summary>
        /// 是否鼠标缩放
        /// </summary>
        private readonly bool isMouseZoomable = true;

        public GenericInput cameraZoomInput = new GenericInput("Mouse ScrollWheel", "", "");


        /// <summary>
        /// 鼠标是否触发移动事件
        /// </summary>
        private bool isMouseMoveable = true;

        /// <summary>
        /// 是否鼠标旋转
        /// </summary>
        private bool isMouseRotateable = true;

        private Dictionary<string, Vector3> m_ButtonDownPos;

        private Dictionary<string, float> m_ButtonDownTime;

        private Dictionary<string, float> m_ButtonUpTime;
        private Vector3 m_lastMousePosition = Vector3.zero;
        private float m_lastTouchTime = -100f;

        /// <summary>
        /// 鼠标滚轮灵敏度
        /// </summary>
        private Vector3 m_mouseLookStart = Vector3.zero;

        [Header("Camera Settings")]
        public GenericInput rotateCameraXInput = new GenericInput("Mouse X", "RightAnalogHorizontal", "Mouse X");

        public GenericInput rotateCameraYInput = new GenericInput("Mouse Y", "RightAnalogVertical", "Mouse Y");

        /// <summary>
        /// 创建鼠标驱动器
        /// </summary>
        /// <param name="inputHandlerHelper"></param>
        public DeviceMouse( IInputHandlerHelper inputHandlerHelper )
            : base(inputHandlerHelper)
        {
            m_lastMousePosition = Input.mousePosition;
            m_ButtonDownTime = new Dictionary<string, float>();
            m_ButtonUpTime = new Dictionary<string, float>();

            //Facade.Instance.RegisterObservers(this, (ushort)EnEngineEvent.OnApplicationFocusChange, OnFoucsChange);
            Framework.SsitInput.InputManager.Instance.RegisterDeviceMsg(this, (ushort) EnGlobalEvent.OnChangeInputMode);
            ((InputHelper) inputHandlerHelper).EventOnInputEvent += OnEventCallBack;
            ((InputHelper) inputHandlerHelper).EventOnInputEventBefore += OnEventCallBackBefore;
        }

        /// <summary>
        /// 驱动名称
        /// </summary>
        public override string DeviceName => DEVICEMOUSE;

        public bool IsVaild { get; set; }


        private void OnFoucsChange( INotification obj )
        {
            m_lastMousePosition = Vector3.zero;
        }

        /// <summary>
        /// 驱动轮询
        /// </summary>
        public override void Update()
        {
            var isAlt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);

            // no mouse if touch screen is in use
            //if (Input.touchCount != 0)
            //{
            //    m_lastTouchTime = Time.realtimeSinceStartup;
            //    m_lastMousePosition = Input.mousePosition;
            //    return;
            //}
            //if (Time.realtimeSinceStartup - m_lastTouchTime <= 1f)
            //{
            //    m_lastMousePosition = Input.mousePosition;
            //    return;
            //}

            // cursor activation
            var isChange = m_lastMousePosition != Input.mousePosition;
            if (isChange)
            {
                inputHandlerHelper.SetCursorPosition(Input.mousePosition);
            }
            inputHandlerHelper.EnableCursor(Input.GetMouseButton(0) && !isAlt);


            // zoom (only if mouse is on screen and not over UI)
            if (isMouseZoomable && inputHandlerHelper.IsInteractable)
            {
                var mouseWheel = cameraZoomInput.GetAxis();
                //if (Mathf.Abs(mouseWheel) > float.Epsilon)
                {
                    // 矫正平台配置限定
                    //PlatformConfig config = Engine.Instance.Platform.PlatformConfig;
                    ZoomCamera(mouseWheel /*Vector3.forward * mouseWheel * config.zoomSpeed*/);
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (m_ButtonDownTime == null)
                {
                    m_ButtonDownTime = new Dictionary<string, float>();
                }
                if (m_ButtonDownPos == null)
                {
                    m_ButtonDownPos = new Dictionary<string, Vector3>();
                }
                var time = -1f;

                if (m_ButtonDownTime.TryGetValue(EnMouseEventType.LeftDown.ToString(), out time))
                {
                    inputHandlerHelper.ProcessMouseEvent(EnMouseEventType.LeftDown);

                    if (time != Time.unscaledTime && time + doubleDuration > Time.unscaledTime)
                    {
                        inputHandlerHelper.ProcessMouseEvent(EnMouseEventType.LeftDouble);
                    }

                    m_ButtonDownTime[EnMouseEventType.LeftDown.ToString()] = Time.unscaledTime;
                    m_ButtonDownPos[EnMouseEventType.LeftDown.ToString()] = Input.mousePosition;
                }
                else
                {
                    inputHandlerHelper.ProcessMouseEvent(EnMouseEventType.LeftDown);
                    m_ButtonDownTime.Add(EnMouseEventType.LeftDown.ToString(), Time.unscaledTime);
                    m_ButtonDownPos.Add(EnMouseEventType.LeftDown.ToString(), Input.mousePosition);
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (m_ButtonDownTime == null)
                {
                    m_ButtonUpTime = new Dictionary<string, float>();
                }
                var upTime = -1f;
                var pos = Vector3.zero;

                if (m_ButtonUpTime.TryGetValue(EnMouseEventType.LeftUp.ToString(), out upTime))
                {
                    var downTime = -1f;
                    m_ButtonDownTime.TryGetValue(EnMouseEventType.LeftDown.ToString(), out downTime);
                    if (downTime > upTime)
                    {
                        m_ButtonUpTime[EnMouseEventType.LeftUp.ToString()] = upTime = Time.unscaledTime;

                        if (downTime + hoverDuration <= Time.unscaledTime)
                        {
                            inputHandlerHelper.ProcessMouseEvent(EnMouseEventType.LeftHover);
                        }
                        else
                        {
                            inputHandlerHelper.ProcessMouseEvent(EnMouseEventType.LeftUp);
                        }
                    }
                }
                else
                {
                    inputHandlerHelper.ProcessMouseEvent(EnMouseEventType.LeftUp);
                    m_ButtonUpTime.Add(EnMouseEventType.LeftUp.ToString(), Time.unscaledTime);
                }

                if (m_ButtonDownPos.TryGetValue(EnMouseEventType.LeftDown.ToString(), out pos))
                {
                    if (pos == Input.mousePosition)
                    {
                        inputHandlerHelper.ProcessMouseEvent(EnMouseEventType.LeftSingleDown);
                    }
                }
                MoveCamera(Vector3.zero, isAlt);
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (!isChange)
                {
                    inputHandlerHelper.SetCursorPosition(Input.mousePosition);
                }
                if (m_ButtonDownTime == null)
                {
                    m_ButtonDownTime = new Dictionary<string, float>();
                }
                var time = -1f;
                if (m_ButtonDownTime.TryGetValue(EnMouseEventType.RightDown.ToString(), out time))
                {
                    inputHandlerHelper.ProcessMouseEvent(EnMouseEventType.RightDown);
                    if (time != Time.unscaledTime && time + doubleDuration > Time.unscaledTime)
                    {
                        inputHandlerHelper.ProcessMouseEvent(EnMouseEventType.RightDouble);
                    }
                    m_ButtonDownTime[EnMouseEventType.RightDown.ToString()] = Time.unscaledTime;
                }
                else
                {
                    inputHandlerHelper.ProcessMouseEvent(EnMouseEventType.RightDown);
                    m_ButtonDownTime.Add(EnMouseEventType.RightDown.ToString(), Time.unscaledTime);
                }
            }
            if (Input.GetMouseButtonUp(1))
            {
                if (m_ButtonDownTime == null)
                {
                    m_ButtonUpTime = new Dictionary<string, float>();
                }
                var upTime = -1f;
                if (m_ButtonUpTime.TryGetValue(EnMouseEventType.RightUp.ToString(), out upTime))
                {
                    var downTime = -1f;
                    m_ButtonDownTime.TryGetValue(EnMouseEventType.RightDown.ToString(), out downTime);
                    if (downTime > upTime)
                    {
                        m_ButtonUpTime[EnMouseEventType.RightUp.ToString()] = upTime = Time.unscaledTime;
                        if (downTime + hoverDuration <= Time.unscaledTime)
                        {
                            inputHandlerHelper.ProcessMouseEvent(EnMouseEventType.RightHover);
                        }
                        else
                        {
                            inputHandlerHelper.ProcessMouseEvent(EnMouseEventType.RightUp);
                        }
                    }
                }
                else
                {
                    inputHandlerHelper.ProcessMouseEvent(EnMouseEventType.RightUp);
                    m_ButtonUpTime.Add(EnMouseEventType.RightUp.ToString(), Time.unscaledTime);
                }
            }

            // camera direction 暂不启用，摄像机那块实在受不了
            if (isMouseRotateable && Input.GetMouseButton(1) || !isMouseRotateable && isAlt && Input.GetMouseButton(1))
            {
                //hack:效果优化-->迁移到辅助器判断
                if (Mathf.Approximately(m_mouseLookStart.sqrMagnitude, 0))
                {
                    m_mouseLookStart = Input.mousePosition;
                }
                else if ((m_mouseLookStart - Input.mousePosition).magnitude > 1e-3f /*0.001f*/)
                {
                    RotateCamera(m_mouseLookStart, Input.mousePosition,
                        new Vector2(rotateCameraXInput.GetAxis(), rotateCameraYInput.GetAxis()), isAlt);
                    m_mouseLookStart = Input.mousePosition;
                }
                else
                {
                    RotateCamera(m_mouseLookStart, Input.mousePosition, Vector2.zero, isAlt);
                    m_mouseLookStart = Input.mousePosition;
                }
            }
            else if (isMouseMoveable && Input.GetMouseButton(0))
            {
                var moveDelta = m_mouseLookStart - Input.mousePosition;
                if (Mathf.Approximately(m_mouseLookStart.sqrMagnitude, 0))
                {
                    m_mouseLookStart = Input.mousePosition;
                }
                else if (moveDelta.magnitude > 1e-3f /*0.001f*/)
                {
                    MoveCamera(new Vector3(-rotateCameraXInput.GetAxis(), -rotateCameraYInput.GetAxis()), isAlt);
                    m_mouseLookStart = Input.mousePosition;
                }
                else
                {
                    MoveCamera(Vector3.zero, isAlt);
                    m_mouseLookStart = Input.mousePosition;
                }
            }
            else
            {
                m_mouseLookStart = Vector3.zero;
            }

            m_lastMousePosition = Input.mousePosition;
        }

        /// <summary>
        /// 旋转摄像机
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="curPos"></param>
        /// <param name="isAlt"></param>
        protected virtual void RotateCamera( Vector3 startPos, Vector3 curPos, Vector2 moveDelta, bool isAlt )
        {
            if (!IsVaild)
            {
                moveDelta = Vector2.zero;
            }

            var eventArgs = ReferencePool.Acquire<MouseEventArgs>();
            eventArgs.SetArgs(EnMouseEventType.LeftDown, startPos, curPos, moveDelta);

            // 发送事件消息
            Facade.Instance.SendNotification((ushort) EnInputEvent.RotateCamera, eventArgs, isAlt);

            // 释放引用
            ReferencePool.Release(eventArgs);

            //if (!isAlt)
            //{
            //    inputHandlerHelper.RotateCameraAroundPivot(Input.mousePosition, m_mouseLookStart);
            //}
            //else
            //{
            //    inputHandlerHelper.RotateCamera(Input.mousePosition, m_mouseLookStart);
            //}
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="moveDelta"></param>
        /// <param name="isAlt"></param>
        protected virtual void MoveCamera( Vector3 moveDelta, bool isAlt )
        {
            if (!isAlt)
            {
                if (!IsVaild || !inputHandlerHelper.HasFoucs)
                {
                    moveDelta = Vector2.zero;
                }

                var eventArgs = ReferencePool.Acquire<MouseEventArgs>();
                eventArgs.SetArgs(EnMouseEventType.LeftDown, Vector2.zero, Vector2.zero, moveDelta);

                // 发送事件消息
                Facade.Instance.SendNotification((ushort) EnInputEvent.MoveCamera, eventArgs);

                // 释放引用
                ReferencePool.Release(eventArgs);
            }
            //else
            //{
            //    inputHandlerHelper.RotateCamera(Input.mousePosition, m_mouseLookStart);
            //}
        }

        protected virtual void ZoomCamera( float zoom )
        {
            if (!inputHandlerHelper.IsInteractable)
            {
                return;
            }

            var eventArgs = ReferencePool.Acquire<MouseEventArgs>();
            eventArgs.SetArgs(EnMouseEventType.LeftDown, Vector2.zero, Vector2.zero, new Vector2(zoom, 0));

            // 发送事件消息
            Facade.Instance.SendNotification((ushort) EnInputEvent.ZoomCamera, eventArgs);

            // 释放引用
            ReferencePool.Release(eventArgs);
        }

        public override void HandleNotification( INotification obj )
        {
            switch (obj.Id)
            {
                case (ushort) EnGlobalEvent.OnChangeInputMode:
                    OnChangeInputMode(obj);
                    break;
            }
        }

        private void OnChangeInputMode( INotification obj )
        {
            //SsitDebug.Debug("OnChangeInputMode [Device Mouse]" + (EnInputMode)obj.Body);

            switch ((EnInputMode) obj.Body)
            {
                case EnInputMode.Editor:
                    isMouseMoveable = false;
                    isMouseRotateable = true;
                    break;
                case EnInputMode.Free:
                    isMouseMoveable = true;
                    isMouseRotateable = true;
                    break;
                case EnInputMode.Control:
                    isMouseMoveable = true;
                    break;
            }
        }


        private void OnEventCallBack( EnMouseEventType arg0 )
        {
            switch (arg0)
            {
                case EnMouseEventType.LeftDown:
                    IsVaild = inputHandlerHelper.IsInteractable;
                    break;
                case EnMouseEventType.RightDown:
                    IsVaild = inputHandlerHelper.IsInteractable;
                    break;
            }
        }
        
        private void OnEventCallBackBefore( EnMouseEventType arg0 )
        {
            switch (arg0)
            {
                case EnMouseEventType.LeftUp:
                case EnMouseEventType.LeftHover:
                    IsVaild = false;
                    break;
                case EnMouseEventType.RightUp:
                case EnMouseEventType.RightHover:
                    IsVaild = false;
                    break;
            }
        }        
        
        /// <summary>
        /// 驱动销毁
        /// </summary>
        public override void Destroy()
        {
            m_ButtonDownTime = null;
            m_ButtonUpTime = null;
            m_ButtonDownPos = null;
            Facade.Instance.RemoveObservers(this, (ushort) EnEngineEvent.OnApplicationFocusChange);
        }
    }
}