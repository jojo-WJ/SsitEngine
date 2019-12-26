using UnityEngine;
using UnityEngine.EventSystems;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    ///     鼠标驱动器
    /// </summary>
    public class InputDeviceMouse : InputDeviceBase
    {
        /// <summary>
        ///     触摸驱动名称
        /// </summary>
        public const string DEVICEMOUSE = "DEVICEMOUSE";

        private Vector3 m_lastMousePosition = Vector3.zero;
        private float m_lastTouchTime = -100f;

        /// <summary>
        ///     鼠标滚轮灵敏度
        /// </summary>
        private Vector3 m_mouseLookStart = Vector3.zero;

        /// <summary>
        ///     创建鼠标驱动器
        /// </summary>
        /// <param name="inputHandlerHelper"></param>
        public InputDeviceMouse( IInputHandlerHelper inputHandlerHelper )
            : base(inputHandlerHelper)
        {
            m_lastMousePosition = Input.mousePosition;
        }

        /// <summary>
        ///     驱动名称
        /// </summary>
        public override string DeviceName => DEVICEMOUSE;

        /// <summary>
        ///     驱动轮询
        /// </summary>
        public override void Update()
        {
            var isAlt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);

            // no mouse if touch screen is in use
            if (Input.touchCount != 0)
            {
                m_lastTouchTime = Time.realtimeSinceStartup;
                m_lastMousePosition = Input.mousePosition;
                return;
            }
            if (Time.realtimeSinceStartup - m_lastTouchTime <= 1f)
            {
                m_lastMousePosition = Input.mousePosition;
                return;
            }

            // cursor activation
            if (m_lastMousePosition != Input.mousePosition)
            {
                inputHandlerHelper.SetCursorPosition(Input.mousePosition);
            }
            inputHandlerHelper.EnableCursor(Input.GetMouseButton(0) && !isAlt);

            // camera direction
            if (Input.GetMouseButton(1) /*|| (isAlt && Input.GetMouseButton(0))*/)
            {
                //hack:效果优化-->迁移到辅助器判断
                /*if (Mathf.Approximately(m_mouseLookStart.sqrMagnitude, 0))
                {
                    m_mouseLookStart = Input.mousePosition;
                }
                else*/
                if ((m_mouseLookStart - Input.mousePosition).magnitude > 1e-3f /*0.001f*/)
                {
                    if (!isAlt)
                    {
                        inputHandlerHelper.RotateCameraAroundPivot(Input.mousePosition, m_mouseLookStart);
                    }
                    else
                    {
                        inputHandlerHelper.RotateCamera(Input.mousePosition, m_mouseLookStart);
                    }
                    m_mouseLookStart = Input.mousePosition;
                }
            }
            else if (Input.GetMouseButton(0))
            {
                var moveDelta = m_mouseLookStart - Input.mousePosition;
                if (Mathf.Approximately(m_mouseLookStart.sqrMagnitude, 0))
                {
                    m_mouseLookStart = Input.mousePosition;
                }
                else if (moveDelta.magnitude > 1e-3f /*0.001f*/)
                {
                    MoveCamera(moveDelta, isAlt);
                    m_mouseLookStart = Input.mousePosition;
                }
            }
            else
            {
                m_mouseLookStart = Vector3.zero;
            }

            // zoom (only if mouse is on screen and not over UI)
            if (EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
            {
                var mouseWheel = Input.GetAxis("Mouse ScrollWheel");
                if (mouseWheel != 0f)
                {
                    // 矫正平台配置限定
                    var config = Engine.Instance.Platform.PlatformConfig;
                    inputHandlerHelper.MoveCamera(Vector3.zero, Vector3.forward * mouseWheel * config.zoomSpeed, true);
                }
            }
        }

        /// <summary>
        ///     移动
        /// </summary>
        /// <param name="moveDelta"></param>
        /// <param name="isAlt"></param>
        protected virtual void MoveCamera( Vector3 moveDelta, bool isAlt )
        {
            if (!isAlt)
            {
                inputHandlerHelper.MoveCamera(Vector3.zero, moveDelta, true);
            }
            //else
            //{
            //    inputHandlerHelper.RotateCamera(Input.mousePosition, m_mouseLookStart);
            //}
        }

        /// <summary>
        ///     驱动销毁
        /// </summary>
        public override void Destroy()
        {
        }
    }
}