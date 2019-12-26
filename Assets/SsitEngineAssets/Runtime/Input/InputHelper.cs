/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：操作辅助器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/16 14:45:51                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using Framework.SceneObject;
using SsitEngine;
using SsitEngine.Core.ReferencePool;
using SsitEngine.Unity.SceneObject;
using SsitEngine.Unity.SsitInput;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Framework.SsitInput
{
    /// <summary>
    /// 操作辅助器
    /// </summary>
    /// remark(Need上层平台Warp此类)
    public class InputHelper : AllocatedObject, IInputHandlerHelper
    {
        private readonly int m_cacheHitCount = 5;
        public UnityAction<EnMouseEventType> EventOnInputEvent;
        public UnityAction<EnMouseEventType> EventOnInputEventBefore;
        /// <summary>
        /// 操作器接口
        /// </summary>
        protected InputManager m_inputManager;

//        private bool m_isSelectionPossible = false;
//        private bool m_isCursorAction = false;
        private bool m_isCursorActionInThisFrame;
        private bool m_isCursorPosChange;
        private Vector3 m_lastCursorActionStartPos = -100f * Vector3.one;
        protected int m_lastGUITouchFrame = -100;


        /// <summary>
        /// 可交互
        /// </summary>
        public bool IsInteractable { get; set; }

        public bool HasFoucs { get; set; }

        public bool IsCursonSomething { get; set; }

        /// <summary>
        /// 初始化操作辅助器
        /// </summary>
        public void InitHelper()
        {
            //todo:inherit class override
        }

        /// <summary>
        /// 初始化操作辅助器
        /// </summary>
        /// <param name="inputManager"></param>
        /// <returns></returns>
        public InputDeviceBase[] InitInputDevice( IInputManager inputManager )
        {
            m_inputManager = inputManager as InputManager;
            var inputDevices = new InputDeviceBase[2];
            inputDevices[0] = new DeviceKeyboard(this);
            inputDevices[1] = new DeviceMouse(this);


            m_raycastHits = new RaycastHit[m_cacheHitCount];
            m_isCursorPosChange = false;
            IsCursonSomething = false;
            return inputDevices;
        }

        public void EnableCursor( bool enableCursor )
        {
            if (IsInteractable)
            {
                m_isCursorActionInThisFrame |= enableCursor;
            }
        }


        public void Update()
        {
            var isMouseOverGUI = IsMouseOverGUI();
            IsInteractable = !isMouseOverGUI /*且需要编辑状态的判断*/;
        }

        /// <summary>
        /// 设置鼠标移动事件
        /// </summary>
        /// <param name="cursorScreenCoords"></param>
        public void SetCursorPosition( Vector3 cursorScreenCoords )
        {
            //todo:记录鼠标位置发生改变，需要重新进行射线检测
            m_isCursorPosChange = true;

            if (m_inputManager.CursorAction)
            {
                var detechPos = cursorScreenCoords + InputManager.Instance.DetectOffset;
                var layerMask = OnDetectLayerCallBack();

                if (detechPos != cursorScreenCoords && !IsPointerOverUI(detechPos))
                {
                    m_cursorRay = Camera.main.ScreenPointToRay(detechPos);
                    IsCursonSomething = Physics.Raycast(m_cursorRay, out m_hitInfo, float.MaxValue, layerMask);
                }
                else if (IsInteractable)
                {
                    m_cursorRay = Camera.main.ScreenPointToRay(detechPos);
                    IsCursonSomething = Physics.Raycast(m_cursorRay, out m_hitInfo, float.MaxValue, layerMask);
                }
                else
                {
                    IsCursonSomething = false;
                }
            }
        }

        public override void Shutdown()
        {
            m_raycastHits = null;
            mCheckQueueCache.Clear();
            mCheckQueueCache = null;
            base.Shutdown();
        }

        /// <summary>
        /// 检测是否点击在UI上
        /// </summary>
        /// <returns></returns>
        protected bool IsMouseOverGUI()
        {
            // check if mouse is over UI
            var isMouseOverDelegate = IsCursorOverUI();
            if (isMouseOverDelegate)
            {
                m_lastGUITouchFrame = Time.frameCount;
            }
            // two frames after touch/mouse over GUI
            return isMouseOverDelegate || Time.frameCount - m_lastGUITouchFrame <= 1;
        }

        /// <summary>
        /// 检测是否在UI上
        /// </summary>
        /// <returns></returns>
        public bool IsCursorOverUI()
        {
            // check touchscreen input
            switch (m_inputManager.InputDevice)
            {
                case InputDevice.MouseKeyboard:
                {
                    // check mouse input
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IPHONE || UNITY_WP8 || UNITY_WP_8_1)
			// don't care about mouse on these platforms, since it is simulated anyway
			return false;
#else
                    //EventSystem.current.isFocused
                    // check mouse if no touch detected
                    return EventSystem.current.IsPointerOverGameObject() ||
                           // input at the edge of the screen is not allowed (3d UI could have a 1 pixel offset to the edge of the screen)
                           IsAtScreensEdge(Input.mousePosition);
#endif
                }
                case InputDevice.Joystick:
                case InputDevice.Mobile:
                {
                    var touches = Input.touches;
                    for (var t = 0; t < touches.Length; t++)
                    {
                        if (EventSystem.current.IsPointerOverGameObject(touches[t].fingerId) ||
                            // input at the edge of the screen is not allowed (3d UI could have a 1 pixel offset to the edge of the screen)
                            IsAtScreensEdge(touches[t].position))
                        {
                            return true;
                        }
                    }
                }
                    break;
            }

            return false;
        }

        /// <summary>
        /// 屏幕边缘检测
        /// </summary>
        /// <param name="p_pos"></param>
        /// <returns></returns>
        protected bool IsAtScreensEdge( Vector2 p_pos )
        {
            const float edgeSize = 4f;
            return p_pos.x < edgeSize || Screen.width < p_pos.x + edgeSize || p_pos.y < edgeSize ||
                   Screen.height < p_pos.y + edgeSize;
        }

        private LayerMask OnDetectLayerCallBack()
        {
            var obj = InputManager.Instance.CurSelectSceneInstance;
            if (obj != null)
            {
                var attri = obj.LinkObject?.GetAttribute();
                if (attri != null)
                {
                    var layer = (EnItemLayerType) attri.PlaceLayer;
                    switch (layer)
                    {
                        case EnItemLayerType.EN_LA_Default:
                            return LayerUtils.DragDetectedLayerMask;
                        case EnItemLayerType.EN_LA_Road:
                            return 1 << LayerUtils.Road;
                        case EnItemLayerType.EN_LA_Ground:
                            return 1 << LayerUtils.Ground;
                        case EnItemLayerType.EN_LA_RoadAndGround:
                            return (1 << LayerUtils.Road) |
                                   (1 << LayerUtils.Ground);
                    }
                }
            }
            return LayerUtils.DragDetectedLayerMask;
        }

        public static bool IsPointerOverUI( Vector2 screenPosition )
        {
            //实例化点击事件
            var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            //将屏幕坐标赋值给点击事件
            eventDataCurrentPosition.position = new Vector2(screenPosition.x, screenPosition.y);
            var results = new List<RaycastResult>();
            //从坐标处发射射线
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        #region 摄像机移动与旋转

        /// <inheritdoc />
        public void MoveCamera( Vector3 fromScreenCoords, Vector3 toScreenCoords, bool convertDir = false,
            bool ignoreUI = false )
        {
            if (CameraController.instance == null)
            {
                return;
            }

            if (ignoreUI && !IsInteractable)
            {
                return;
            }

            CameraController.instance.MoveCamera(fromScreenCoords, toScreenCoords, convertDir, ignoreUI);
        }

        public void RotateCamera( Vector3 fromScreenCoords, Vector3 toScreenCoords, bool ignoreUI = false )
        {
            if (CameraController.instance == null)
            {
                return;
            }
            if (ignoreUI && !IsInteractable)
            {
            }


            //CameraController.instance.RotateCamera(fromScreenCoords, toScreenCoords, ignoreUI);
        }

        /// <inheritdoc />
        public void RotateCameraAroundPivot( Vector3 fromScreenCoords, Vector3 toScreenCoords, bool ignoreUI = false )
        {
            if (CameraController.instance == null)
            {
                return;
            }
            if (ignoreUI && !IsInteractable)
            {
                return;
            }


            CameraController.instance.RotateCameraAroundPivot(fromScreenCoords, toScreenCoords, ignoreUI);
        }

        #endregion

        #region 鼠标事件处理机制

        //检测队列
        private RaycastHit[] m_raycastHits;

        //当前拖拽检测碰撞信息
        private RaycastHit m_hitInfo;

        //当前对象点击的碰撞信息
        private RaycastHit m_ObjHitInfo;

        //点击后触发的道具队列
        public Queue<RaycastHit> mCheckQueueCache = new Queue<RaycastHit>();

        protected Ray m_cursorRay;

        public RaycastHit HitInfo => m_hitInfo;

        public void ProcessMouseEvent( EnMouseEventType mouseEventType )
        {
            EventOnInputEventBefore?.Invoke(mouseEventType);
            
            //焦点锁定
            switch (mouseEventType)
            {
                case EnMouseEventType.LeftDown:
                {
                    if (!IsInteractable)
                    {
                        HasFoucs = false;
                    }
                    else
                    {
                        HasFoucs = true;
                    }
                }

                    break;
            }
            //UI锁定
            if (!IsInteractable || m_inputManager.LockInput)
            {
                return;
            }

            EventOnInputEvent?.Invoke(mouseEventType);

            if (m_inputManager.CursorAction)
            {
                return;
            }
            // 更新当前检测队列
            if (m_isCursorPosChange)
            {
                m_isCursorPosChange = false;

                if (m_raycastHits == null)
                {
                    m_raycastHits = new RaycastHit[m_cacheHitCount];
                }
                m_cursorRay = Camera.main.ScreenPointToRay(m_inputManager.PlatInput.mousePosition);
                var count = Physics.RaycastNonAlloc(m_cursorRay, m_raycastHits, float.MaxValue,
                    LayerUtils.InputDetectLayer.value);
                if (count != 0)
                {
                    IsCursonSomething = true;
                    for (var i = m_cacheHitCount - 1; count < m_cacheHitCount && i >= count; i--)
                    {
                        m_raycastHits[i].distance = float.MaxValue;
                    }
                    // 对检测进行排序
                    Array.Sort(m_raycastHits, SortDetectResult);
                    // 缓存入队列
                    AddCheckQueue();
                }
                else
                {
                    IsCursonSomething = false;
                }
            }
            else
            {
                IsCursonSomething = true;
            }

            if (!IsCursonSomething)
            {
                return;
            }
            BaseSceneInstance baseObjcet = null;

            // 根据不同的类型选择不同方式过滤检测队列
            switch (mouseEventType)
            {
                case EnMouseEventType.LeftSingleDown:
                    // 取缓存队列碰撞信息
                    if (mCheckQueueCache.Count == 0)
                        // 队列为空时，重置队列
                    {
                        AddCheckQueue();
                    }
                    m_ObjHitInfo = PostRayCast();
                    if (m_ObjHitInfo.collider != null)
                    {
                        var col = m_ObjHitInfo.collider;
                        baseObjcet = col.GetComponentInParent<BaseSceneInstance>();
                        //过滤点不穿的对象
                        var attri = baseObjcet?.LinkObject?.GetAttribute();
                        if (attri != null && !attri.IsThrough)
                        {
                            ClearCheckQueue();
                        }
                    }
                    break;
                default:
                    m_ObjHitInfo = m_raycastHits[0];
                    if (m_ObjHitInfo.collider != null)
                    {
                        var col = m_ObjHitInfo.collider;
                        baseObjcet = col.GetComponentInParent<BaseSceneInstance>();
                    }
                    break;
            }

            // 获取消息体通过消息池
            var eventArgs = ReferencePool.Acquire<InputEventArgs>();
            eventArgs.type = mouseEventType;
            eventArgs.target = baseObjcet;
            eventArgs.point = m_ObjHitInfo.point;
            eventArgs.normal = m_ObjHitInfo.normal;
            eventArgs.data = m_ObjHitInfo.collider;
            PostGroundRayCast(ref eventArgs);
            if (m_inputManager.EventOnInputEvent != null)
            {
                m_inputManager.EventOnInputEvent.Invoke(eventArgs);
            }

            // 释放对象到消息池
            ReferencePool.Release(eventArgs);
        }


        private int SortDetectResult( RaycastHit x, RaycastHit y )
        {
            if (x.collider == null && y.collider == null)
            {
                return 0;
            }
            if (x.collider != null && y.collider != null)
            {
                if (x.distance > y.distance)
                {
                    return 1;
                }
                if (x.distance < y.distance)
                {
                    return -1;
                }
            }
            if (x.collider == null && y.collider != null)
            {
                return 1;
            }

            if (x.collider != null && y.collider == null)
            {
                return -1;
            }
            return 0;
        }

        public void AddCheckQueue()
        {
            mCheckQueueCache.Clear();
            for (var i = 0; i < m_raycastHits.Length; i++)
            {
                if (m_raycastHits[i].collider == null && m_raycastHits[i].distance >= 99999)
                {
                    continue;
                }
                mCheckQueueCache.Enqueue(m_raycastHits[i]);
            }
        }

        public void PostGroundRayCast( ref InputEventArgs args )
        {
            for (var i = 0; i < m_raycastHits.Length; i++)
            {
                if (m_raycastHits[i].collider == null || m_raycastHits[i].distance >= 99999)
                {
                    continue;
                }

                if (m_raycastHits[i].collider.gameObject.layer == LayerUtils.Road)
                {
                    args.isGroud = true;
                    args.groud = m_raycastHits[i].point;
                }
            }
        }

        public void ClearCheckQueue()
        {
            mCheckQueueCache.Clear();
        }

        public RaycastHit PostRayCast()
        {
            try
            {
                return mCheckQueueCache.Dequeue();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion
    }
}