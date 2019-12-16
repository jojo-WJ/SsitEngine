/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：操作辅助器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/16 14:45:51                     
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;
using UnityEngine.EventSystems;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    ///     操作辅助器
    /// </summary>
    /// remark(Need上层平台Warp此类)
    public class InputHandlerHelper : AllocatedObject, IInputHandlerHelper
    {
        private const float LOOK_AROUND_ROTATION_SPEED = 300;
        protected Vector3 m_camPivot = Vector3.zero;

        /// <summary>
        ///     检测碰撞信息
        /// </summary>
        protected RaycastHit m_cursorHitInfo;

        /// <summary>
        ///     检测射线
        /// </summary>
        protected Ray m_cursorRay;

        /// <summary>
        ///     鼠标点击位置
        /// </summary>
        protected Vector3 m_cursorScreenCoords = -1f * Vector3.zero;

        /// <summary>
        ///     操作器接口
        /// </summary>
        protected IInputManager m_inputManager;

        protected bool m_isCameraMoveMenet = true;
        protected bool m_isCursorOverSomething;

        protected int m_lastGUITouchFrame = -100;

        /// <summary>
        ///     射线检测层级
        /// </summary>
        protected int m_layerMask;

        /// <summary>
        ///     创建一个操作辅助器
        /// </summary>
        public InputHandlerHelper()
        {
            m_cursorRay = new Ray();
            // 范围限制

            // 去除编辑器播放控制的层级
            m_layerMask = ~((1 << 8) | (1 << 2));
            m_cursorHitInfo = default;
            m_isCursorOverSomething = false;
        }

        /// <summary>
        ///     初始化操作辅助器
        /// </summary>
        public virtual void InitHelper()
        {
            //todo:inherit class override
        }

        /// <summary>
        ///     初始化操作辅助器
        /// </summary>
        /// <param name="inputManager"></param>
        /// <returns></returns>
        public virtual InputDeviceBase[] InitInputDevice( IInputManager inputManager )
        {
            m_inputManager = inputManager;
            var inputDevices = new InputDeviceBase[3];
            inputDevices[0] = new InputDeviceKeyboard(this);
            inputDevices[1] = new InputDeviceMouse(this);
            inputDevices[2] = new InputDeviceTouchScreen(this);
            return inputDevices;
        }

        /// <inheritdoc />
        public void SetCursorPosition( Vector3 cursorScreenCoords )
        {
            m_cursorScreenCoords = cursorScreenCoords;
            m_cursorRay = Camera.main.ScreenPointToRay(cursorScreenCoords);
            m_isCursorOverSomething = Physics.Raycast(m_cursorRay, out m_cursorHitInfo, 1000f, m_layerMask);
        }

        /// <inheritdoc />
        public void EnableCursor( bool enableCursor )
        {
        }

        /// <inheritdoc />
        public void MoveCamera( Vector3 fromScreenCoords, Vector3 toScreenCoords, bool convertDir = false,
            bool ignoreUI = false )
        {
            if (!m_isCameraMoveMenet) return;
            if (ignoreUI && !IsInteractable) return;
            var cam = m_inputManager.Cam;
            if (cam != null)
            {
                var camDist = EstimateDistanceToLevel(EEstimateDistanceMode.AVERAGE);

                Vector3 camMove;
                var dir = toScreenCoords - fromScreenCoords;
                //本地坐标转世界坐标
                dir = cam.transform.TransformDirection(dir);
                if (!convertDir) dir.y = 0;
                if (cam.orthographic)
                {
                    var forwardDir = Vector3.Dot(dir, cam.transform.forward);
                    dir -= cam.transform.forward * forwardDir;
                    cam.orthographicSize =
                        Mathf.Clamp(cam.orthographicSize - forwardDir * (cam.orthographicSize / Screen.width), 5, 1000);
                    camMove = dir * (cam.orthographicSize / Screen.width);
                }
                else
                {
                    camMove = dir * (camDist / Screen.width);
                }
                cam.transform.position += camMove;

                UpdateCameraPivotPoint();
            }
        }

        /// <inheritdoc />
        public void RotateCamera( Vector3 fromScreenCoords, Vector3 toScreenCoords, bool ignoreUI = false )
        {
            if (!m_isCameraMoveMenet) return;
            if (ignoreUI && !IsInteractable) return;

            if (Mathf.Approximately(toScreenCoords.sqrMagnitude, 0))
            {
                UpdateCameraPivotPoint();
                return;
            }
            // create two rays starting from the given screen coordinates
            var cam = m_inputManager.Cam;

            if (cam != null)
            {
                var fromRay = cam.ScreenPointToRay(fromScreenCoords);
                var toRay = cam.ScreenPointToRay(toScreenCoords);
                if (cam.orthographic)
                {
                    fromRay.direction = (fromRay.origin + fromRay.direction * 100f - cam.transform.position).normalized;
                    toRay.direction = (toRay.origin + toRay.direction * 100f - cam.transform.position).normalized;
                }
                var angle = Vector3.Angle(fromRay.direction, toRay.direction);
                var axis = Vector3.Cross(fromRay.direction, toRay.direction).normalized;
                // slow down rotation if camera looks straight down or up
                var factor = Mathf.Abs(cam.transform.forward.y);
                const float MAX_FACTOR = 0.9f;
                if (factor > MAX_FACTOR)
                {
                    factor = (1f - factor) / (1f - MAX_FACTOR);
                    factor = factor * factor;
                    var upDownRelAmount = Vector3.Dot(axis, cam.transform.right);
                    axis -= (1f - factor) * (axis - upDownRelAmount * cam.transform.right);
                }
                // rotate the camera by the angle of the created rays
                cam.transform.Rotate(axis, -angle, Space.World);
                // keep the camera up direction
                if (cam.transform.up.y > 0)
                    cam.transform.LookAt(cam.transform.position + cam.transform.forward, Vector3.up);
                else
                    cam.transform.LookAt(cam.transform.position + cam.transform.forward, Vector3.down);
                UpdateCameraPivotPoint();
            }
        }

        /// <inheritdoc />
        public void RotateCameraAroundPivot( Vector3 fromScreenCoords, Vector3 toScreenCoords, bool ignoreUI = false )
        {
            if (!m_isCameraMoveMenet) return;
            if (ignoreUI && !IsInteractable) return;

            if (Mathf.Approximately(toScreenCoords.sqrMagnitude, 0))
            {
                UpdateCameraPivotPoint();
                return;
            }
            var cam = m_inputManager.Cam;
            if (cam != null)
            {
                // 计算屏幕滑动距离
                var relativeScreenDist = new Vector2(toScreenCoords.x, toScreenCoords.y) -
                                         new Vector2(fromScreenCoords.x, fromScreenCoords.y);

                // 计算距离与屏幕的缩放比
                relativeScreenDist.x /= Screen.width;
                relativeScreenDist.y /= Screen.height;

                // 矫正平台配置限定
                var config = Engine.Instance.Platform.PlatformConfig;
                // 垂直方向旋转受限
                if (Mathf.Abs(relativeScreenDist.y) > config.rotPermissiDelta.y)
                {
                    var angle = relativeScreenDist.y * config.rotSpeed + cam.transform.localEulerAngles.x;
                    angle = Mathf.Clamp(angle, config.rotXAxisMin, config.rotXAxisMax) -
                            cam.transform.localEulerAngles.x;
                    cam.transform.RotateAround(m_camPivot, cam.transform.right, angle);
                }

                if (Mathf.Abs(relativeScreenDist.x) > config.rotPermissiDelta.x)
                    cam.transform.RotateAround(m_camPivot, cam.transform.up, -relativeScreenDist.x * config.rotSpeed);


                if (cam.transform.up.y > 0)
                    cam.transform.LookAt(m_camPivot, Vector3.up);
                else
                    cam.transform.LookAt(m_camPivot, Vector3.down);
            }
        }

        /// <inheritdoc />
        public virtual void Update()
        {
            // initialization

            var isMouseOverGUI = IsMouseOverGUI();
            IsInteractable = !isMouseOverGUI /*且需要编辑状态的判断*/;

            // update key combos
            // UpdateKeyCombos();
            // update camera pivot point
            //UpdateCameraPivotPoint();
        }


        /// <summary>
        ///     检测是否点击在UI上
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsMouseOverGUI()
        {
            // check if mouse is over UI
            var isMouseOverDelegate = IsCursorOverUI();
            if (isMouseOverDelegate) m_lastGUITouchFrame = Time.frameCount;
            // two frames after touch/mouse over GUI
            return isMouseOverDelegate || Time.frameCount - m_lastGUITouchFrame <= 1;
        }

        /*protected virtual bool IsMouseOverGUI()
        {
            // check if mouse is over UI
            bool isMouseOverDelegate = IsCursorOverUI();
            // check if camera perspective gizmo is clicked
            bool isMouseOverGizmo = false;
            if (m_inputManager.IsWithCameraPerspectiveGizmos && !isMouseOverDelegate)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.y = Mathf.Clamp(Screen.height - mousePos.y, 0, Screen.height);
                mousePos.x = Mathf.Clamp(mousePos.x, 0, Screen.width);
                var perspectiveGizmoRect = m_inputManager.PerspectiveGizmoRect;
#if UNITY_ANDROID || UNITY_IPHONE || UNITY_WP8 || UNITY_WP_8_1
				isMouseOverGizmo = Input.GetMouseButton(0) && perspectiveGizmoRect.Contains(mousePos);
#else
                isMouseOverGizmo = perspectiveGizmoRect.Contains(mousePos);
#endif
                if (!isMouseOverGizmo)
                {
                    Touch[] touches = Input.touches;
                    for (int t = 0; t < Input.touchCount; t++)
                    {
                        Vector2 touch = touches[t].position;
                        Vector3 pos = new Vector3(touch.x, Mathf.Clamp(Screen.height - touch.y, 0, Screen.height), 0f);
                        if (perspectiveGizmoRect.Contains(pos))
                        {
                            isMouseOverGizmo = true;
                            break;
                        }
                    }
                }
            }
            if (isMouseOverDelegate || isMouseOverGizmo)
            {
                m_lastGUITouchFrame = Time.frameCount;
            }
            // two frames after touch/mouse over GUI
            return isMouseOverDelegate || isMouseOverGizmo || Time.frameCount - m_lastGUITouchFrame <= 1;
        }*/

        /// <summary>
        ///     检测是否在UI上
        /// </summary>
        /// <returns></returns>
        public virtual bool IsCursorOverUI()
        {
            // check touchscreen input
            var touches = Input.touches;
            for (var t = 0; t < touches.Length; t++)
                if (EventSystem.current.IsPointerOverGameObject(touches[t].fingerId) ||
                    // input at the edge of the screen is not allowed (3d UI could have a 1 pixel offset to the edge of the screen)
                    IsAtScreensEdge(touches[t].position))
                    return true;

            // check mouse input
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IPHONE || UNITY_WP8 || UNITY_WP_8_1)
			// don't care about mouse on these platforms, since it is simulated anyway
			return false;
#else
            // check mouse if no touch detected
            return EventSystem.current.IsPointerOverGameObject() ||
                   // input at the edge of the screen is not allowed (3d UI could have a 1 pixel offset to the edge of the screen)
                   IsAtScreensEdge(Input.mousePosition);
#endif
        }

        #region Property

        /// <summary>
        ///     摄像机旋转轴点
        /// </summary>
        public Vector3 CamPivot
        {
            get => m_camPivot;
            set => m_camPivot = value;
        }

        /// <summary>
        ///     鼠标检测到对象
        /// </summary>
        public bool IsCursorOverSomething
        {
            get
            {
                CheckCursor();
                return m_isCursorOverSomething;
            }
        }

        /// <summary>
        ///     可交互
        /// </summary>
        public bool IsInteractable { get; set; }

        public bool HasFoucs { get; set; }

        public bool IsCursonSomething { get; set; }

        public RaycastHit HitInfo { get; set; }

        #endregion


        #region Internal

        /// <summary>
        ///     检测碰撞信息
        /// </summary>
        protected void CheckCursor()
        {
            if (m_isCursorOverSomething && (m_cursorHitInfo.collider == null || m_cursorHitInfo.transform == null))
                m_isCursorOverSomething = false;
        }

        /// <summary>
        ///     更新摄像机的旋转轴点
        /// </summary>
        private void UpdateCameraPivotPoint()
        {
            var cam = m_inputManager.Cam;
            if (cam != null)
            {
                //Hack:仅固定焦点距离算法在接近地面时有时候效果非常不好
                var lookPosition = cam.transform.position;
                var lookDirection = cam.transform.forward;
                var fromRay = new Ray(cam.transform.position, cam.transform.forward);
                if (Physics.Raycast(fromRay, out var hit, 1000f, m_layerMask))
                {
                    m_camPivot = hit.point;
                    // 高度与俯仰角关联，高度越高，俯仰角越大
                    if (Vector3.Distance(lookPosition, m_camPivot) >
                        lookPosition.y * Mathf.Cos(Mathf.Deg2Rad * 45)) // 45°
                        m_camPivot = lookPosition + lookDirection * lookPosition.y * Mathf.Cos(Mathf.Deg2Rad * 45);
                }
                else
                {
                    //计算焦点的矢量方向
                    var camToPivot = m_camPivot - lookPosition;

                    //计算矢量方向与摄像机关系
                    //      |
                    //      |
                    //------|-------
                    //      |
                    //      |
                    var pivotDirection = Vector3.Dot(lookDirection, camToPivot);
                    if (pivotDirection < 1f)
                    {
                        //矫正方向偏移距离
                        var corrector = pivotDirection - 1.5f;
                        m_camPivot = lookPosition - lookDirection * corrector;
                        camToPivot = m_camPivot - lookPosition;
                        pivotDirection = Vector3.Dot(lookDirection, camToPivot);
                    }
                    var pivotOffset = Vector3.Cross(lookDirection, camToPivot).magnitude;
                    if (pivotOffset > 1f) m_camPivot = lookPosition + lookDirection * pivotDirection;
                }
            }
        }

        /// <summary>
        ///     屏幕边缘检测
        /// </summary>
        /// <param name="p_pos"></param>
        /// <returns></returns>
        protected bool IsAtScreensEdge( Vector2 p_pos )
        {
            const float edgeSize = 4f;
            return p_pos.x < edgeSize || Screen.width < p_pos.x + edgeSize || p_pos.y < edgeSize ||
                   Screen.height < p_pos.y + edgeSize;
        }

        public void ProcessMouseEvent( EnMouseEventType mouseEventType )
        {
        }

        /// <summary>
        ///     估算最小距离
        /// </summary>
        protected enum EEstimateDistanceMode
        {
            AVERAGE,
            NEAREST
        }

        /// <summary>
        ///     视锥碰撞动态优化
        /// </summary>
        /// <param name="p_mode"></param>
        /// <returns></returns>
        protected float EstimateDistanceToLevel( EEstimateDistanceMode p_mode )
        {
            var camDist = 300f;
            var cam = m_inputManager.Cam;
            if (cam != null)
            {
                // raycast four rays to find out the approximate distance between camera and level
                float hitCount = 0;
                var hitValue = Vector3.zero;
                var nearestHit = float.MaxValue;
                for (float x = 0; x < 2; x++)
                for (float y = 0; y < 2; y++)
                {
                    var temPos = new Vector3(cam.rect.width * Screen.width * (0.25f + 0.5f * x),
                        cam.rect.height * Screen.height * (0.25f + 0.5f * y), 0f);
                    var ray = cam.ScreenPointToRay(temPos);
                    //if (Engine.Debug)
                    //{
                    //    Debug.DrawLine(cam.transform.position, temPos, Color.green, 0.3f);
                    //}
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        hitCount++;
                        if (p_mode == EEstimateDistanceMode.AVERAGE)
                        {
                            hitValue += hit.point;
                        }
                        else
                        {
                            var distToCam = (hit.point - cam.transform.position).magnitude;
                            if (nearestHit > distToCam)
                            {
                                nearestHit = distToCam;
                                hitValue = hit.point;
                            }
                        }
                    }
                }
                if (hitCount > 0)
                {
                    if (p_mode == EEstimateDistanceMode.AVERAGE) hitValue /= hitCount;
                    camDist = (hitValue - cam.transform.position).magnitude;
                }
            }
            return camDist;
        }


        /// <summary>
        ///     摄像机的高度动态变化
        /// </summary>
        /// <returns></returns>
        private float EstimateHightToLevel()
        {
            //todo:摄像机高度动态变化摄像机的移速|缩放速度
            var cam = m_inputManager.Cam;

            if (cam) return cam.transform.position.y /* factor*/;

            return 1;
        }

        #endregion
    }
}