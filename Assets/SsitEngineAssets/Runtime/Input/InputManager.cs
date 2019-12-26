using System.Collections.Generic;
using System.Linq;
using Framework.Data;
using Framework.Helper;
using Framework.Logic;
using Framework.SceneObject;
using Framework.SSITInput.EventHandler;
using Mirror;
using Packages.Rider.Editor.Util;
using SsitEngine;
using SsitEngine.Core;
using SsitEngine.Core.ReferencePool;
using SsitEngine.DebugLog;
using Framework.Navigator;
using Framework.Utility;
using SSIT.proto;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Unity;
using SsitEngine.Unity.Data;
using SsitEngine.Unity.Resource;
using SsitEngine.Unity.SceneObject;
using SsitEngine.Unity.SsitInput;
using SsitEngine.Unity.UI;
using Table;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.SsitInput
{
    public class MouseClickEventVo
    {
        public EnMouseEvent cType;
        public Vector3 groundHit;
        public Vector3 position;
        public BaseSceneInstance sceneInstance;
    }

    /// <summary>
    /// 场景预制信息,目前仅用于创建场景内物体
    /// </summary>
    public class ScenePrefabInfo : InfoData
    {
        public InputManager.ENObjectCreateMode createMode = InputManager.ENObjectCreateMode.Drag; //拖拽状态

        public UDraggedObjectState dragStatu = UDraggedObjectState.NONE; //拖拽状态
        public int IconId; //界面显示图标
        public int ItemId;
        public string Name; //显示名称
        public string ResoucesName; //消息参数,目前仅为预制路径
        public string Type; //预制类型,目前未使用
        public bool isInitstance;
    }


    public class InputManager : ManagerBase<InputManager>, IInputManager
    {
        #region Variable

        /// <summary>
        /// 物体编辑相对空间
        /// </summary>
        public enum EnObjectEditSpace
        {
            SELF,
            WORLD
        }

        /// <summary>
        /// 物体编辑模式
        /// </summary>
        public enum EnObjectEditMode
        {
            NO_EDIT,
            MOVE,
            ROTATE,
            SCALE,
            SMART
        }

        /// <summary>
        /// 物体生成模式
        /// </summary>
        public enum ENObjectCreateMode
        {
            Drag,
            Click,
            Draw
        }


        /// <summary>
        /// 操作模式
        /// </summary>
        public EnInputMode inputMode = EnInputMode.None;

        // lock

        //private EventHandlerManager handler;

        private Material inputDefaultMaterial;
        private Color inputNormalColor;
        private Color inputNotPlaceColor;

        private NavigatorPathIndicator _mPathIndicator;
        private Vector3 _mPathBegin;

        public bool LockInput { get; set; }

        #endregion

        #region 初始化 And 重置

        public override void OnSingletonInit()
        {
            InitPlatInput();

            //初始化线路指示器池
            Pool<NavigatorPathIndicator>.CreatePool(2,
                delegate
                {
                    var ret = Resources.Load<NavigatorPathIndicator>("SceneObject/PathIndicator");
                    //var ret = ResourcesManager.Instance.LoadAsset<NavigatorPathIndicator>("SceneObject/PathIndicator");
                    ret.transform.SetParent(transform);
                    return ret;
                },
                Destroy);

            LockInput = true;
            m_eventOnPlacedVaild = InternalPlacedVaildCallback;
            inputDefaultMaterial = new Material(Shader.Find("Standard"));
            inputNormalColor = new Color(0, 1, 1, 0.5f);
            inputNotPlaceColor = new Color(1, 0, 0, 0.5f);

            _mPathIndicator = Resources.Load<NavigatorPathIndicator>("SceneObject/Draw/DrawIndicator");
            //_mPathIndicator = ResourcesManager.Instance.LoadAsset<NavigatorPathIndicator>("SceneObject/Draw/DrawIndicator");
            _mPathIndicator.transform.SetParent(transform);
            _mPathIndicator.SetWidth(0.02f);
            _mPathIndicator.ResetPath();
        }

        public void InitProxy()
        {
            // 移除过时的代理和中介管理
            //Facade.Instance.RemoveProxy(SceneStateProxy.NAME);
            //Facade.Instance.RemoveMediator(InputManagerMediator.NAME);
            Instance.LockInput = false;

            EventOnInputEvent.AddListener(ProcessMouseEvent);


            // 注册代理
            /*handler = gameObject.GetOrAddComponent<EventHandlerManager>();
            EventOnInputEvent.AddListener(handler.Handle);
            var helper = (InputHelper) m_inputHandlerHelper;
            helper.EventOnInputEvent -= handler.OnInputEventCallBack;
            helper.EventOnInputEvent += handler.OnInputEventCallBack;*/
            //SceneStateProxy proxy = new SceneStateProxy();
            //Facade.Instance.RegisterProxy(proxy);

            // 通知全局系统进行自由模式
            Facade.Instance.SendNotification((ushort) EnGlobalEvent.ChangeInputMode, EnInputMode.Free);
            // 注册中介
            //Facade.Instance.RegisterMediator(new InputManagerMediator(this, proxy));
        }

        public void RemoveProxy()
        {
            Instance.LockInput = true;
            Facade.Instance.SendNotification((ushort) EnGlobalEvent.ChangeInputMode, EnInputMode.None);
            //des handler
            /*if (handler)
                Destroy(handler);*/

            //des res
            DeletePreviewInstance(true);

            // 清除回调事件
            EventOnInputEvent.RemoveAllListeners();

            EventOnRightClick.RemoveAllListeners();
            EventOnDoubleRightClick.RemoveAllListeners();
            EventOnRightHover.RemoveAllListeners();
            EventOnLeftClick.RemoveAllListeners();
            EventOnDoubleLeftClick.RemoveAllListeners();
            EventOnLeftHoverClick.RemoveAllListeners();
            EventOnDoubleRightClick.RemoveAllListeners();
            EventOnLeftUpClick.RemoveAllListeners();

            Pool<NavigatorPathIndicator>.Instance.ClearPool();
        }

        #endregion

        #region 模块接口实现

        /// <summary>
        /// 计时器模块优先级
        /// </summary>
        public override int Priority => (int) EnModuleType.ENMODULEBASE;

        /// <summary>
        /// 计时器刷新
        /// </summary>
        /// <param name="elapsed">逻辑流逝时间</param>
        public override void OnUpdate( float elapsed )
        {
            if (LockInput || Camera.main == null)
                return;

            if (m_inputHandlerHelper == null)
                return;
            m_inputHandlerHelper.Update();

            if (m_inputDeviceMaps == null)
                return;

            for (var i = 0; i < m_inputDeviceMaps.Length; i++) 
                m_inputDeviceMaps[i].Update();

            UpdateSmartMove();
            UpdateNewObjectDragAndDrop();
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            if (isShutdown || m_inputDeviceMaps == null)
                return;

            isShutdown = true;

            for (var i = 0; i < m_inputDeviceMaps.Length; i++) 
                m_inputDeviceMaps[i].Destroy();

            m_inputDeviceMaps = null;
            Pool<NavigatorPathIndicator>.Instance.Destroy();
            base.Shutdown();
            isShutdown = true;
        }

        #endregion

        #region 路径绘制

        public NavigatorPathIndicator DrawPathIndicator( Vector3[] points )
        {
            var pathIndicator = Pool<NavigatorPathIndicator>.Instance.GetFreeObject();
            //create equip instance
            pathIndicator.UpdatePath(points);
            pathIndicator.gameObject.SetActive(true);
            return pathIndicator;
        }

        public void ReleasePathIndicator( NavigatorPathIndicator pathIndicator )
        {
            pathIndicator.gameObject.SetActive(false);
            Pool<NavigatorPathIndicator>.Instance.ReleaseObject(pathIndicator);
        }

        #endregion

        #region 输入平台控制

        /// <summary>
        /// 我的平台操作器
        /// </summary>
        private CrossPlatInput m_mInput;

        public delegate void OnChangeInputType( InputDevice type );

        public event OnChangeInputType onChangeInputType;

        private InputDevice _inputType = InputDevice.MouseKeyboard;

        [HideInInspector]
        public InputDevice InputDevice
        {
            get => _inputType;
            set
            {
                _inputType = value;
                //m_mInput.f
                OnChangeInput();
            }
        }

        /// <summary>
        /// 我的平台操作器
        /// </summary>
        public CrossPlatInput PlatInput => m_mInput;

        private void InitPlatInput()
        {
            if (m_mInput == null)
            {
                m_mInput = gameObject.GetOrAddComponent<CrossPlatInput>();
                m_mInput.Init(_inputType);
            }
            //todo:移动端附加虚拟输入管理器
        }

        private bool isMobileInput()
        {
#if UNITY_EDITOR && UNITY_MOBILE
            if (EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
            {
                return true;
            }

#elif MOBILE_INPUT
            if (EventSystem.current.IsPointerOverGameObject() || (Input.touches.Length > 0))
                return true;
#endif
            return false;
        }

        private void OnChangeInput()
        {
            if (onChangeInputType != null) onChangeInputType(InputDevice);
        }

        #endregion

        #region 底层接口实现

        /// <summary>
        /// 驱动辅助器
        /// </summary>
        private IInputHandlerHelper m_inputHandlerHelper;

        /// <summary>
        /// 驱动列表
        /// </summary>
        private InputDeviceBase[] m_inputDeviceMaps;

        /// <summary>
        /// 操作摄像机
        /// </summary>
        private Camera m_cam;

        /// <summary>
        /// 摄像机
        /// </summary>
        public Camera Cam
        {
            get
            {
                if (m_cam == null) m_cam = Camera.main;
                return m_cam;
            }
        }


        #region Public Members

        /// <summary>
        /// 设置驱动助手
        /// </summary>
        /// <param name="inputHandlerHelper"></param>
        public void SetInputHander( IInputHandlerHelper inputHandlerHelper )
        {
            m_inputHandlerHelper = inputHandlerHelper;
            m_inputDeviceMaps = inputHandlerHelper.InitInputDevice(this);
            m_inputHandlerHelper.InitHelper();
        }

        /// <summary>
        /// 获取驱动助手
        /// </summary>
        /// <returns></returns>
        public IInputHandlerHelper GetInputHander()
        {
            return m_inputHandlerHelper;
        }

        /// <summary>
        /// 获取驱动助手
        /// </summary>
        /// <typeparam name="T">辅助器类型</typeparam>
        /// <returns></returns>
        public T GetInputHander<T>() where T : class, IInputHandlerHelper
        {
            return m_inputHandlerHelper as T;
        }

        /// <summary>
        /// 驱动是否受理
        /// </summary>
        /// <param name="deviceName">驱动名称</param>
        /// <returns></returns>
        public bool IsDeviceSet( string deviceName )
        {
            if (m_inputDeviceMaps != null) return m_inputDeviceMaps.First(x => x.DeviceName == deviceName) != null;

            if (Engine.Debug) SsitDebug.Debug("当前系统无此名称的驱动");
            return false;
        }

        /// <summary>
        /// 激活或禁用相应名称的操作驱动器
        /// </summary>
        /// <param name="deviceName">驱动名称</param>
        /// <param name="enable">参数值</param>
        public void EnableDevice( string deviceName, bool enable )
        {
            var device = GetDeviceByName(deviceName);
            if (device != null) device.Enable = enable;
        }

        /// <summary>
        /// 获取驱动管理器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DeviceName"></param>
        /// <returns></returns>
        public T GetDeviceByName<T>( string DeviceName ) where T : InputDeviceBase
        {
            if (m_inputDeviceMaps != null) return m_inputDeviceMaps.First(x => x.DeviceName == DeviceName) as T;

            if (Engine.Debug) SsitDebug.Debug("当前系统无此名称的驱动");
            return null;
        }

        /// <summary>
        /// 获取驱动管理器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DeviceName"></param>
        /// <returns></returns>
        public InputDeviceBase GetDeviceByName( string DeviceName )
        {
            if (m_inputDeviceMaps != null) return m_inputDeviceMaps.First(x => x.DeviceName == DeviceName);

            if (Engine.Debug) SsitDebug.Debug("当前系统无此名称的驱动");
            return null;
        }

        #endregion

        #region EventCallback

        /// <summary>
        /// 注册驱动事件
        /// </summary>
        /// <param name="device"></param>
        /// <param name="msgList"></param>
        public void RegisterDeviceMsg( InputDeviceBase device, params ushort[] msgList )
        {
            for (var i = 0; i < msgList.Length; i++)
                Facade.Instance.RegisterObservers(this, msgList[i], device.HandleNotification);
        }

        /// <summary>
        /// 移除驱动事件
        /// </summary>
        /// <param name="device"></param>
        /// <param name="msgList"></param>
        public void UnRegisterDeviceMsg( InputDeviceBase device, params ushort[] msgList )
        {
            for (var i = 0; i < msgList.Length; i++) Facade.Instance.RemoveObservers(this, msgList[i]);
        }

        #endregion

        #endregion

        #region 对象检测

        private EnSceneState m_currentState = EnSceneState.NORMAL; //当前场景编辑状态
        public Stack<EnSceneState> PrevState = new Stack<EnSceneState>(); //场景状态记录堆栈

        public BaseSceneInstance currentSelectedInstance; //当前场景选中物体
        public BaseSceneInstance lastSelectedInstance; //上个场景选中物体

        //private string m_objectResourcePath = null;
        private ScenePrefabInfo m_objectRes; //场景创建的资源数据
        public BaseSceneInstance m_previewInstance; //场景拖拽中的预览实例

        private bool m_isPlaceable;
        private bool m_isObjectPlaceable;

        private string m_dragMessage = "";

        public bool CanPlaced
        {
            get => m_isPlaceable;
            set
            {
                if (m_isPlaceable != value)
                {
                    m_isPlaceable = value;

                    if (m_previewInstance != null)
                    {
                        if (m_isPlaceable)
                            m_previewInstance.SetItemMaterial(m_previewInstance.gameObject, inputDefaultMaterial,
                                inputNormalColor);
                        else
                            m_previewInstance.SetItemMaterial(m_previewInstance.gameObject, inputDefaultMaterial,
                                inputNotPlaceColor);
                    }
                }
            }
        }

        [SerializeField] private EnObjectEditSpace m_objectEditSpace = EnObjectEditSpace.SELF;

        [SerializeField] private EnObjectEditMode m_objectEditMode = EnObjectEditMode.MOVE;

        public EnObjectEditSpace ObjectEditSpace
        {
            get => m_objectEditSpace;
            set => m_objectEditSpace = value;
        }

        public EnObjectEditMode ObjectEditMode
        {
            get => m_objectEditMode;
            set => m_objectEditMode = value;
        }

        public ENObjectCreateMode DraggedObjectState { get; set; } = ENObjectCreateMode.Drag;


        public bool CursorAction { get; private set; }

        public bool IsSelectedObjectSmartMoved =>
            currentSelectedInstance != null && currentSelectedInstance.IsSmartMove
                                            && m_objectEditMode == EnObjectEditMode.SMART;

        public BaseSceneInstance CurSelectSceneInstance
        {
            get => currentSelectedInstance;
            set
            {
                if (currentSelectedInstance != value)
                {
                    lastSelectedInstance = currentSelectedInstance ?? lastSelectedInstance;

                    ResetState();
                }
                currentSelectedInstance = value;

                if (currentSelectedInstance != null)
                    currentSelectedInstance.OnDisVisiableEvent = OnDisVisiableEvent;
                else if (lastSelectedInstance != null) lastSelectedInstance.OnDisVisiableEvent = null;
            }
        }

        private void OnDisVisiableEvent( BaseSceneInstance arg0 )
        {
            SetSelectedObject(null);
            Facade.Instance.SendNotification((ushort) EnInputEvent.SelectDestoryObject);
        }


        #region 鼠标拖拽处理机制

        /// <summary>
        /// 拖拽偏移量[UI相对检测位置和对象所在位置的偏移]
        /// </summary>
        public Vector3 DetectOffset { get; set; }

        /// <summary>
        /// 设置拖拽对象的信息（UI注册回调）
        /// </summary>
        /// <param name="resInfo">资源名称</param>
        public void SetDraggableObject( ScenePrefabInfo resInfo )
        {
            if (resInfo == null)
            {
                CursorAction = false;
                DeletePreviewInstance(true);
                _mPathIndicator.ResetPath();
                return;
            }

            switch (resInfo.dragStatu)
            {
                case UDraggedObjectState.BEGINDRAG:
                    CursorAction = true;
                    m_objectRes = resInfo;
                    break;
                case UDraggedObjectState.ENDRAG:
                    CursorAction = false;
                    m_objectRes = resInfo;
                    break;
            }

            /*if (m_cursorActionOnObject != null)
            {
                m_isSceneInstanceFound = false;
                m_dragMessage = "";
                // todo：实例预览实例
                m_cursorActionOnObject = null;
                m_isObjectPlaceable = IsObjectPlaceable();
            }
            else
            {
                m_isObjectPlaceable = IsObjectPlaceable();
            }*/

            // 更新操作状态
            if (CursorAction)
            {
                ChangeSceneState(EnSceneState.NEWOBJECT);
            }
            else
            {
                DetectOffset = Vector3.zero;
                RevertPrevState();
                //Debug.Log($"SceneState{SceneState}");
            }

            //更新开始检测事件
            UpdateIsObjectPlaceable();
            //m_inputHandlerHelper.IsCursonSomething = false;
        }

        /// <summary>
        /// 更新新物体的拖拽和掉落
        /// </summary>
        private void UpdateNewObjectDragAndDrop()
        {
            if (m_inputHandlerHelper == null)
                return;
            if (m_objectRes != null /*后期更改为资源id!=0*/)
            {
                //Debug.Log($"expression{m_inputHandlerHelper.IsInteractable} {m_inputHandlerHelper.IsCursonSomething}");

                // reset drag icon text it could have been changed with
                // 如果摄像机检测到UI即不可交互状态
                if (!m_inputHandlerHelper.IsInteractable)
                {
                    // 如果预览实例不为空则删除预览实例
                    if (m_previewInstance != null)
                        Destroy(m_previewInstance.gameObject);
                }
                // check if the icon is being dragged and the cursor is over something
                else if (m_inputHandlerHelper.IsCursonSomething)
                {
                    // make a preview of the dragged object
                    if (IsObjectDraggedInUI())
                    {
                        var m_cursorHitInfo = m_inputHandlerHelper.HitInfo;
                        // 对象可以被拖拽
                        if (m_cursorHitInfo.collider != null &&
                            OnObjectDrag(m_cursorHitInfo) /*hit point is on terrain*/)
                        {
                            // 实例化拖拽对象
                            if (m_previewInstance == null)
                            {
                                CreatePreviewInstance(m_cursorHitInfo);
                            }
                            else
                            {
                                SmartMove(m_previewInstance);
                                if (m_objectRes.createMode == ENObjectCreateMode.Draw)
                                    _mPathIndicator.DrawEnd(m_previewInstance.GetPosition());
                            }
                        }
                    }
                    // place object if cursor was released while the object was over something
                    else if (m_isObjectPlaceable)
                    {
                        PlaceObject();
                    }
                    else
                    {
                        DeletePreviewInstance();
                    }
                }
                else
                {
                    DeletePreviewInstance();
                }

                // icon hiding and coloring
                if (!m_inputHandlerHelper.IsInteractable || !IsObjectDraggedInUI())
                {
                    // keep icon color if icon is over 2d GUI
                    Facade.Instance.SendNotification((ushort) EnInputEvent.EDraggedObjectState,
                        EDraggedObjectState.NONE);
                }
                else if (m_previewInstance != null)
                {
                    if (m_objectRes.createMode == ENObjectCreateMode.Drag)
                    {
                        // hide icon if a 3d preview is drawn
                        Facade.Instance.SendNotification((ushort) EnInputEvent.EDraggedObjectState,
                            EDraggedObjectState.IN_3D_PREVIEW);
                    }
                    else
                    {
                        SmartMove(m_previewInstance);
                        if (m_objectRes.createMode == ENObjectCreateMode.Draw)
                            _mPathIndicator.DrawEnd(m_previewInstance.GetPosition());
                    }
                }
                else
                {
                    // show icon in red since it cannot be placed here
                    Facade.Instance.SendNotification((ushort) EnInputEvent.EDraggedObjectState,
                        EDraggedObjectState.NOT_PLACEABLE);
                }
            }
        }

        /// <summary>
        /// 拖拽中检测
        /// </summary>
        /// <returns></returns>
        private bool OnObjectDrag( RaycastHit hit )
        {
            // 拖拽中检测是否可放置到当前碰撞信息上
            var isPlaceable = m_isObjectPlaceable;
            if (m_eventOnPlacedVaild != null)
                isPlaceable = m_eventOnPlacedVaild.Invoke(m_objectRes, hit);
            // 如果由于任何其他原因禁用了3D GUI，则隐藏预览实例
            if (!isPlaceable)
            {
                if (m_objectRes.createMode == ENObjectCreateMode.Drag)
                {
                    if (m_previewInstance != null)
                        Destroy(m_previewInstance.gameObject);
                }
                else
                {
                    if (m_previewInstance == null)
                        CreatePreviewInstance(hit, true);
                }
            }
            //record
            switch (m_objectRes.createMode)
            {
                case ENObjectCreateMode.Click:
                case ENObjectCreateMode.Draw:
                    CanPlaced = isPlaceable;
                    break;
            }

            return isPlaceable;
        }

        /// <summary>
        /// 刷新对象放置的标记
        /// </summary>
        public void UpdateIsObjectPlaceable()
        {
            m_isObjectPlaceable = IsObjectPlaceable();
        }


        /// <summary>
        /// 是否可以放置
        /// </summary>
        /// <returns></returns>
        private bool IsObjectPlaceable()
        {
            // 如果当前鼠标跟踪对象不为空
            if (m_previewInstance != null)
            {
                // 判断鼠标跟踪的实例对象是否可以放置
                if (!IsObjectPlaceable(m_previewInstance, m_objectRes))
                {
                    m_dragMessage = "max. # reached!";
                    return false;
                }
                m_dragMessage = "";
                return true;
            }

            return false;
        }

        /// <summary>
        /// 对象是否可以放置
        /// </summary>
        /// <param name="pInstance"></param>
        /// <param name="resInfo"></param>
        /// <returns></returns>
        public bool IsObjectPlaceable( BaseSceneInstance pInstance, ScenePrefabInfo resInfo )
        {
            // check if there is an instance count limit
            //m_isSceneInstanceFound = false;
            //if (p_object.MaxInstancesInLevel != 0)
            //{
            //    //int count = ObjectManager.Instance.FindSameResourceCount(p_resourcePath);

            //    // the limit (MaxInstancesInLevel) is already reached
            //    //if (p_object.MaxInstancesInLevel <= count)
            //    {
            //        return false;
            //    }
            //}
            //else
            //{
            //    //m_isSceneInstanceFound = ObjectManager.Instance.FindObjectByRepresent(p_object.gameObject) != null;
            //}
            return true;
        }

        /// <summary>
        /// 放置对象
        /// </summary>
        private bool PlaceObject()
        {
            if (m_previewInstance != null)
            {
                switch (m_objectRes.createMode)
                {
                    case ENObjectCreateMode.Click:
                    {
                        if (!CanPlaced)
                        {
                            //PopTipHelper.PopTipForm(EnText.PlaceFailedTip,En_ConfirmFormType.En_Failed);
                            DeletePreviewInstance(true);
                            return false;
                        }
                    }
                        break;
                    case ENObjectCreateMode.Draw:
                        DeletePreviewInstance(true);
                        PlaceDraw(CanPlaced);
                        if (!CanPlaced)
                        {
                            //PopTipHelper.PopTipForm(EnText.DrawFailedTip,En_ConfirmFormType.En_Failed);
                            return false;
                        }
                        else
                            // 销毁预览实例
                            return true;
                    case ENObjectCreateMode.Drag:
                        break;
                }

                // 单机版
                if (!GlobalManager.Instance.IsSync)
                {
                    var transform1 = m_previewInstance.transform;
                    var eventArgs = new ObjectCreateEventArgs(0, m_objectRes, transform1.position,
                        transform1.rotation);

                    SsitApplication.Instance.CreateEditorObject(m_objectRes.ID, m_objectRes.ItemId,
                        InternalCreatedCallBack,
                        eventArgs);
                }
                else
                {
                    var request = new CSSpawnSceneObjectRequest();
                    request.spawnInfo = new SpawnSceneObjectInfo();

                    request.spawnInfo.dataId = m_objectRes.ItemId;

                    var transform1 = m_previewInstance.transform;
                    request.spawnInfo.position = transform1.position;
                    request.spawnInfo.rotation = transform1.rotation;
                    request.spawnInfo.scale = transform1.localScale;

                    if (NetworkServer.active)
                        request.spawnInfo.netId = GlobalManager.c_sDefaultServerNet;
                    else if (NetworkClient.active)
                        request.spawnInfo.netId = (int) NetworkClient.connection.playerController.netId;
                    //todo:物体创建组+User
                    //request.spawnInfo.groupId = 
                    SsitApplication.Instance.SpawnObject(null, m_objectRes.ItemId, null, request);
                }

                // 销毁预览实例
                DeletePreviewInstance(true);
                return true;
            }
            return false;
        }


        /// <summary>
        /// 放置后处理机制
        /// </summary>
        /// <param name="p_newInstance"></param>
        public void OnNewObjectPlaced( BaseSceneInstance p_newInstance )
        {
            //对象激活碰撞/刚体
            p_newInstance.SetColliderActive(true);
            SelectNewObjectAndNotifyListeners(p_newInstance);
        }

        /// <summary>
        /// 新创建的物体是否要跟踪编辑（或者手动点击激活编辑功能）
        /// </summary>
        /// <param name="p_newInstance">创建对象实例</param>
        public void SelectNewObjectAndNotifyListeners( BaseSceneInstance p_newInstance )
        {
            // select new object
            //设置场景状态进入创建状态
            SetSelectedObject(p_newInstance);
            // notify listeners that an object has been placed
            //if (OnObjectPlaced != null)
            //{
            //    OnObjectPlaced(p_gui3d, new LE_ObjectPlacedEvent(p_newInstance));
            //}
        }

        /// <summary>
        /// 非UI拖拽中
        /// </summary>
        /// <returns></returns>
        private bool IsObjectDraggedInUI()
        {
            if (m_objectRes == null || m_objectRes.dragStatu == UDraggedObjectState.ENDRAG
                                    || m_objectRes.dragStatu == UDraggedObjectState.Cancel)
            {
                return false;
            }
            return true;
        }

        private void PlaceDraw( bool canPlace )
        {
            var eventArgs = ReferencePool.Acquire<InputEventArgs>();
            eventArgs.point = m_previewInstance.transform.position;
            eventArgs.type = canPlace ? EnMouseEventType.Custom1 : EnMouseEventType.Custom2;
            if (canPlace)
            {
                _mPathIndicator.DrawBegin(m_previewInstance.transform.position);
            }

            EventOnInputEvent.Invoke(eventArgs);
            // 释放对象到消息池
            ReferencePool.Release(eventArgs);
        }

        #endregion

        #region 点击处理机制

        /// <summary>
        /// 选择对象
        /// </summary>
        /// <param name="pInstance"></param>
        /// <param name="point"></param>
        public void SelectObject( BaseSceneInstance pInstance )
        {
            //设置选中物体
            if (pInstance != null && pInstance is ITriggle t)
                switch (t.TriggleType)
                {
                    case TriggleType.MouseClick_Triggle:
                        pInstance = t.OnPostTriggle(Vector3.zero);
                        break;
                }

            if (inputMode == EnInputMode.Lock) return;

            //todo
            if (currentSelectedInstance != pInstance)
                //ResetState();
                SetSelectedObject(pInstance);
            //if (pInstance == null)
            //{
            //    //Facade.Instance.SendNotification((ushort)ConstNotification.SelectedObject, null);
            //}
            //else
            //{
            //    if (pInstance.Type != EnObjectType.GamePlayer)
            //    {
            //        Facade.Instance.SendNotification((ushort)ConstNotification.UpdateMemberSelect, -1);
            //    }
            //    else
            //    {
            //        var ntplayer = pInstance.GetComponent<PlayerInstance>();
            //        if (ntplayer != null)
            //        {
            //            Facade.Instance.SendNotification((ushort)ConstNotification.UpdateMemberSelect, ntplayer.PlayerIndex);
            //        }
            //    }
            //}

            //Facade.Instance.SendNotification((ushort)EnInputEvent.SelectObject, p_object, priorSelectedObject);
        }

        /// <summary>
        /// 选择对象
        /// </summary>
        /// <param name="pInstance"></param>
        public void SelectObject( BaseSceneInstance pInstance, Vector3 point )
        {
            //设置选中物体
            if (pInstance != null && pInstance is ITriggle t)
                switch (t.TriggleType)
                {
                    case TriggleType.MouseClick_Triggle:
                        pInstance = t.OnPostTriggle(point);
                        break;
                    case TriggleType.MouseClick_Draw_Triggle:
                        t.OnPostTriggle(point);
                        break;
                }

            if (inputMode == EnInputMode.Lock) return;

            //todo
            if (currentSelectedInstance != pInstance)
                //ResetState();
                SetSelectedObject(pInstance);
            //if (pInstance == null)
            //{
            //    //Facade.Instance.SendNotification((ushort)ConstNotification.SelectedObject, null);
            //}
            //else
            //{
            //    if (pInstance.Type != EnObjectType.GamePlayer)
            //    {
            //        Facade.Instance.SendNotification((ushort)ConstNotification.UpdateMemberSelect, -1);
            //    }
            //    else
            //    {
            //        var ntplayer = pInstance.GetComponent<PlayerInstance>();
            //        if (ntplayer != null)
            //        {
            //            Facade.Instance.SendNotification((ushort)ConstNotification.UpdateMemberSelect, ntplayer.PlayerIndex);
            //        }
            //    }
            //}

            //Facade.Instance.SendNotification((ushort)EnInputEvent.SelectObject, p_object, priorSelectedObject);
        }

        public void SetSelectedObject( BaseSceneInstance sceneInstance,
            EnSceneState enSceneState = EnSceneState.SELECEDOBJECT, bool isDelete = false )
        {
            if (m_currentState.Equals(EnSceneState.EDITOBJECT))
                return;

            ResetFocus();

            if (currentSelectedInstance != null)
                ObjectManager.Instance?.SendNotification((ushort) EnPropertyId.Selected, currentSelectedInstance, false,
                    sceneInstance);

            CurSelectSceneInstance = sceneInstance;

            if (sceneInstance != null)
            {
                ObjectManager.Instance?.SendNotification((ushort) EnPropertyId.Selected, sceneInstance, true,
                    lastSelectedInstance);
                ChangeSceneState(enSceneState);
            }
        }

        #endregion

        #region 场景状态处理

        public EnSceneState SceneState
        {
            get => m_currentState;
            set
            {
                if (m_currentState != value)
                {
                    m_currentState = value;
                    UpdateSceneState();
                    Facade.Instance.SendNotification((ushort) EnInputEvent.FinishChangeSceneState, m_currentState);
                    return;
                }

                m_currentState = value;
            }
        }

        public void ChangeSceneState( EnSceneState sceneState )
        {
            if (!SceneState.Equals(sceneState)) PrevState.Push(SceneState);
            SceneState = sceneState;
        }

        public void RevertPrevState()
        {
            if (PrevState.Count > 0) SceneState = PrevState.Pop();
        }

        public void ResetState()
        {
            PrevState.Clear();
            SceneState = EnSceneState.NORMAL;
        }

        private void UpdateSceneState()
        {
            switch (m_currentState)
            {
                case EnSceneState.NEWOBJECT:
                    //todo:NewObject();
                    if (CursorAction)
                        Facade.Instance.SendNotification((ushort) EnGlobalEvent.ChangeInputMode, EnInputMode.Editor);

                    break;
                case EnSceneState.EDITOBJECT:
                    //hack:处理编辑状态
                    if (currentSelectedInstance != null && currentSelectedInstance.CanEdit)
                    {
                        // todo:转换全局操作模式到编辑模式
                        currentSelectedInstance.SetColliderActive(false);

                        Facade.Instance.SendNotification((ushort) EnGlobalEvent.ChangeInputMode, EnInputMode.Editor);
                        //m_currentSelectedObject.EditSpace = m_objectEditSpace;
                        //m_currentSelectedObject.EditMode = m_objectEditMode;
                    }
                    break;
                case EnSceneState.NEWLINE:
                    break;
                case EnSceneState.EDITLINE:
                    break;
                case EnSceneState.NORMAL:
                    currentSelectedInstance?.SetColliderActive(true);
                    Facade.Instance.SendNotification((ushort) EnInputEvent.TriggerObject);
                    Facade.Instance.SendNotification((ushort) UIMsg.CloseForm, En_UIForm.SceneObjectEditForm,
                        currentSelectedInstance);
                    Facade.Instance.SendNotification((ushort) UIMsg.CloseForm, En_UIForm.EmergencyObjectEditorForm,
                        currentSelectedInstance);
                    Facade.Instance.SendNotification((ushort) EnInputEvent.SelectObject);

                    //Facade.Instance.SendNotification((ushort)ConstNotification.UnDisplayObjectInfo);
                    // 如果需要缓存上个控制状态，则返回上个控制状态(默认人物状态)[体验不好舍弃]
                    //Facade.Instance.SendNotification((ushort)EnGlobalEvent.ChangeInputMode, EnInputMode.Free);
                    switch (inputMode)
                    {
                        case EnInputMode.None:
                            break;
                        case EnInputMode.Editor:
                        {
                            //var tt = GlobalManager.Instance.PlayerCacheInstance;
                            //if (tt != null)
                            //{
                            //    CameraController.instance.SetPlayer(tt.GetRepresent().transform);
                            //    tt.PlayerController.OnInputAttach(true);
                            //}
                            //else
                            {
                                Facade.Instance.SendNotification((ushort) EnGlobalEvent.ChangeInputMode,
                                    EnInputMode.Free);
                            }
                        }
                            break;
                        case EnInputMode.Free:
                        {
                            //var tt = GlobalManager.Instance.PlayerCacheInstance;
                            //if (tt != null)
                            //{
                            //    CameraController.instance.SetPlayer(tt.GetRepresent().transform);
                            //    tt.PlayerController.OnInputAttach(true);
                            //}
                        }
                            break;
                        case EnInputMode.Control:
                            break;
                        case EnInputMode.Lock:
                            break;
                    }

                    break;
                case EnSceneState.SELECEDOBJECT:
                    //   Facade.Instance.SendNotification((ushort)ConstNotification.SelectedObject, m_currentSelectedObject);
                    Facade.Instance.SendNotification((ushort) EnInputEvent.TriggerObject,
                        currentSelectedInstance); //通知界面操作

                    if (currentSelectedInstance.CanEdit)
                    {
                        if (Utilitys.CheckAuthiroty(currentSelectedInstance))
                            //access :界面接入
                            Facade.Instance.SendNotification((ushort) UIMsg.OpenForm, new UIParam
                            {
                                formId = (int) En_UIForm.SceneObjectEditForm,
                                isAsync = false
                            }, currentSelectedInstance);

                        //Facade.Instance.SendNotification((ushort)ConstNotification.ResetWidget);

                        //Facade.Instance.SendNotification((ushort)ConstNotification.UpdateEditPanelPosition);

                        //if (m_currentSelectedObject.Type == EnObjectType.Mound)
                        //{
                        //    Facade.Instance.SendNotification((ushort)ConstNotification.FilterWidget, "DeleteButton");
                        //}
                    }
                    else if (currentSelectedInstance is IOnOff)
                    {
                    }
                    break;
            }
        }

        #endregion

        #region 操作运动处理机制

        /// <summary>
        /// 更新选择物体的位置
        /// </summary>
        private void UpdateSmartMove()
        {
            if (m_inputHandlerHelper.IsCursonSomething && // first check if cursor if over something
                IsSelectedObjectSmartMoved && // check if the currently selected object is being smart moved
                CursorAction)
            {
                SmartMove(currentSelectedInstance);
                Facade.Instance.SendNotification((ushort) EnInputEvent.UpdateEditPanelPosition);
            }
        }

        /// <summary>
        /// 步进运动
        /// </summary>
        /// <param name="p_obj"></param>
        private void SmartMove( BaseSceneInstance p_obj )
        {
            var lasPos = p_obj.transform.position;

            p_obj.transform.position = m_inputHandlerHelper.HitInfo.point;
            // todo:扩展对象网格捕捉功能

            // if placement option IsPlacementRotationByNormal is true rotate the object accordingly
            if (p_obj.IsPlacementRotationByNormal) p_obj.transform.up = m_inputHandlerHelper.HitInfo.normal;

            // 产生变化再去同步

            p_obj.DoMove(lasPos, p_obj.transform.position);
        }

        #endregion


        /// <summary>
        /// 对象聚焦
        /// </summary>
        public void Focus()
        {
            if (currentSelectedInstance != null)
                // calculate objects size
                CameraController.instance.FocusMode(currentSelectedInstance.gameObject, null);
            /*if (OnFocused != null)
                {
                    OnFocused(center, distance);
                }*/
        }

        private void ResetFocusView()
        {
            CameraController.instance.ResetView();
        }

        /// <summary>
        /// 对象销毁
        /// </summary>
        public void CreatePreviewInstance( RaycastHit hitInfo, bool force = false )
        {
            if (m_objectRes != null)
            {
                //update async has mada !!!!
                var temp = Resources.Load<BaseSceneInstance>(m_objectRes.ResoucesName);
                //var temp = ResourcesManager.Instance.LoadAsset<BaseSceneInstance>(m_objectRes.ResoucesName);
                if (temp)
                {
                    m_previewInstance = temp;
                    m_previewInstance.transform.position = hitInfo.point;
                    m_previewInstance.name = "LE_GUI3dObject Preview Instance";
                    //需不要换层（建议换）
                    LayerUtils.MoveToLayer(m_previewInstance.transform, LayerMask.NameToLayer("Ignore Raycast"));
                    //刚体也需要干掉（拖拽会产生影响）
                    var rigidbodies = m_previewInstance.GetComponentsInChildren<Rigidbody>();
                    for (var i = 0; i < rigidbodies.Length; i++) Destroy(rigidbodies[i]);

                    if (m_objectRes.createMode == ENObjectCreateMode.Click
                        || m_objectRes.createMode == ENObjectCreateMode.Draw)
                    {
                        var colliders = m_previewInstance.GetComponentsInChildren<Collider>();
                        for (var i = 0; i < rigidbodies.Length; i++) 
                            Destroy(colliders[i]);

                        if (force)
                            temp.SetItemMaterial(temp.gameObject, inputDefaultMaterial, inputNotPlaceColor);
                        else
                            temp.SetItemMaterial(temp.gameObject, inputDefaultMaterial, inputNormalColor);
                    }
                }
            }
        }

        /// <summary>
        /// 对象销毁
        /// </summary>
        public void DeletePreviewInstance( bool force = false )
        {
            if (m_objectRes != null && m_objectRes.createMode == ENObjectCreateMode.Drag || force)
            {
                if (m_previewInstance != null)
                {
                    Destroy(m_previewInstance.gameObject);
                }
            }
            if (force)
            {
                m_objectRes = null;
            }
        }

        /// <summary>
        /// 对象克隆
        /// </summary>
        public void CloneObject()
        {
            if (currentSelectedInstance != null && IsObjectPlaceable(currentSelectedInstance, m_objectRes))
                // remove selection
                //m_currentSelectedObject.EditMode = EnObjectEditMode.NO_EDIT;
                currentSelectedInstance.IsSelected = false;
            // selection state would be applied at the end of the frame, but we need it right now
            // m_selectedObject.ApplySelectionState();
            // todo:clone
        }

        #endregion

        #region 消息处理

        public OnInputEvent EventOnInputEvent = new OnInputEvent();

        //鼠标右键
        public OnInputEvent EventOnRightClick = new OnInputEvent();
        public OnInputEvent EventOnRightUpClick = new OnInputEvent();
        public OnInputEvent EventOnDoubleRightClick = new OnInputEvent();
        public OnInputEvent EventOnRightHover = new OnInputEvent();

        //鼠标左键
        public OnInputEvent EventOnLeftClick = new OnInputEvent();
        public OnInputEvent EventOnLeftUpClick = new OnInputEvent();
        public OnInputEvent EventOnDoubleLeftClick = new OnInputEvent();
        public OnInputEvent EventOnLeftHoverClick = new OnInputEvent();

        public UnityAction<Vector3, float> m_eventOnFocused;

        // 放置有效性检测回调事件
        public SsitFunction<ScenePrefabInfo, RaycastHit, bool> m_eventOnPlacedVaild;

        private void OnEnable()
        {
            m_msgList = new[]
            {
                //global
                (ushort) EnInputEvent.SetDraggedObject,
                (ushort) EnInputEvent.UDraggedObjectstate,
                (ushort) EnGlobalEvent.OnChangeInputMode,
                (ushort) EnInputEvent.OnEditorMove,
                (ushort) EnInputEvent.OnForceEditorMove,
                (ushort) EnInputEvent.FinishAddObject,
                //input 
                (ushort) EnKeyEventType.resetInput
            };
            RegisterMsg(m_msgList);
        }

        private void OnDisable()
        {
            UnRegisterMsg(m_msgList);
        }

        /// <summary>
        /// 消息中心回调处理
        /// </summary>
        /// <param name="notification"></param>
        public override void HandleNotification( INotification notification )
        {
            switch (notification.Id)
            {
                case (ushort) EnInputEvent.SetDraggedObject:
                    SelectObject(null);
                    SetDraggableObject((ScenePrefabInfo) notification.Body);
                    break;
                case (ushort) EnInputEvent.UDraggedObjectstate:
                    break;
                case (ushort) EnGlobalEvent.OnChangeInputMode:
                    InternalOnChangeInputMode(notification);
                    break;
                case (ushort) EnInputEvent.OnEditorMove:
                    if (notification is MvEventArgs args)
                        InteranlOnEditorMove((Vector2) args.Body, args.BoolValue, (EnObjectEditMode) args.Values[1]);
                    break;
                case (ushort) EnInputEvent.OnForceEditorMove:
                    SelectObject(null);
                    break;
                case (ushort) EnInputEvent.FinishAddObject:
                    InteranlFinishedAddObject(notification);
                    break;
                case (ushort) EnKeyEventType.resetInput:
                    ResetFocusView();
                    break;
            }
        }


        /// <summary>
        /// 刷新当前检测对象 
        /// </summary>
        /// <param name="arg0"></param>
        private void ProcessMouseEvent( InputEventArgs arg0 )
        {
            //Debug.Log("expression" + arg0.type + "  " + arg0.target);
            switch (arg0.type)
            {
                case EnMouseEventType.LeftDown:
                    EventOnLeftClick?.Invoke(arg0);
                    break;
                case EnMouseEventType.LeftSingleDown:
                    SelectObject(arg0.target, arg0.point);
                    break;
                case EnMouseEventType.LeftUp:
                    EventOnLeftUpClick?.Invoke(arg0);
                    break;
                case EnMouseEventType.RightDown:
                    if (EventOnRightClick != null)
                        EventOnRightClick?.Invoke(arg0);
                    break;
                case EnMouseEventType.LeftDouble:
                    EventOnDoubleLeftClick?.Invoke(arg0);
                    break;
                case EnMouseEventType.RightUp:
                    EventOnRightUpClick?.Invoke(arg0);
                    break;
                case EnMouseEventType.RightDouble:
                    EventOnDoubleRightClick?.Invoke(arg0);
                    break;
                case EnMouseEventType.LeftHover:
                    EventOnLeftHoverClick?.Invoke(arg0);
                    break;
                case EnMouseEventType.RightHover:
                    EventOnRightHover?.Invoke(arg0);
                    break;
            }
        }

        #endregion

        #region Event handler

        /// <summary>
        /// 操作方式改变回调
        /// </summary>
        /// <param name="notification">消息体</param>
        private void InternalOnChangeInputMode( INotification notification )
        {
            inputMode = (EnInputMode) notification.Body;
            SsitDebug.Info($"OnChangeInputMode {inputMode}");
            switch (inputMode)
            {
                case EnInputMode.Free:
                    GlobalManager.Instance.Player = null;
                    SetSelectedObject(null);
                    // ReSharper disable once Unity.NoNullPropagation
                    CameraController.instance?.SwitchTo(ENCameraModeType.CrossPlatformManipulator);
                    break;
                case EnInputMode.Editor:
                    GlobalManager.Instance.Player = null;
                    // ReSharper disable once Unity.NoNullPropagation
                    CameraController.instance?.SwitchTo(ENCameraModeType.CrossPlatformManipulator);
                    break;
                case EnInputMode.Control:
                    break;
            }
        }

        /// <summary>
        /// 编辑模式改变回调
        /// </summary>
        /// <param name="offset">定点偏移</param>
        /// <param name="editPosition">编辑标识</param>
        /// <param name="mode">编辑模式</param>
        private void InteranlOnEditorMove( Vector2 offset, bool editPosition, EnObjectEditMode mode )
        {
            CursorAction = editPosition;
            // totest 增加偏移量-48.2, -52.5, 0.0

            // 更新操作状态
            if (CursorAction)
            {
                DetectOffset = offset;
                ChangeSceneState(EnSceneState.EDITOBJECT);
            }
            else
            {
                DetectOffset = Vector3.zero;
                RevertPrevState();
            }

            m_inputHandlerHelper.IsCursonSomething = false;
            //Debug.Log($"expression{DetectOffset}");

            m_objectEditMode = mode;
        }

        /// <summary>
        /// 对象放置有效化检测机制回调
        /// </summary>
        /// <param name="arg0">拖拽对象信息</param>
        /// <param name="arg1">射线检测信息</param>
        /// <returns></returns>
        private bool InternalPlacedVaildCallback( ScenePrefabInfo arg0, RaycastHit arg1 )
        {
            var proxy = DataManager.Instance.GetDataProxy<DataItemProxy>((int) EnLocalDataType.DATA_ITEM);
            var itemDefine = proxy.GetItemData(arg0.ItemId);
            if (itemDefine != null)
            {
                var layer = (EnItemLayerType) itemDefine.PlaceLayer;
                //Debug.Log("InternalPlacedVaildCallback layer" + arg1.collider.gameObject.layer);
                switch (layer)
                {
                    case EnItemLayerType.EN_LA_Default:
                        return true;
                    case EnItemLayerType.EN_LA_Road:
                        return arg1.collider.gameObject.layer == LayerUtils.Road;
                    case EnItemLayerType.EN_LA_Ground:
                        return arg1.collider.gameObject.layer == LayerUtils.Ground;
                    case EnItemLayerType.EN_LA_RoadAndGround:
                        return arg1.collider.gameObject.layer == LayerUtils.Road ||
                               arg1.collider.gameObject.layer == LayerUtils.Ground;
                }
            }

            return true;
        }

        /// <summary>
        /// 单机版本物体创建回调(同步时不生效)
        /// </summary>
        /// <param name="baseObject">底层交互对象</param>
        /// <param name="render">上层渲染对象</param>
        /// <param name="data">自定义的数据</param>
        private void InternalCreatedCallBack( BaseObject baseObject, object render, object data )
        {
            var args = data as ObjectCreateEventArgs;
            var obj = render as GameObject;
            if (args != null && obj != null)
            {
                obj.transform.position = args.Pos;
                obj.transform.rotation = args.Rot;
                
                SsitApplication.Instance.OnCreatedFunc(baseObject, render, null);
                var sceneInstance = baseObject.SceneInstance;
                if (sceneInstance) 
                    OnNewObjectPlaced(sceneInstance);
            }
        }


        private void InteranlFinishedAddObject( INotification notification )
        {
            if (SceneState == EnSceneState.NEWOBJECT) ResetState();

            var sceneInstance = notification.Body as BaseSceneInstance;
            if (sceneInstance) 
                OnNewObjectPlaced(sceneInstance);
        }

        #endregion

        #region Internal Method

        public void ResetFocus()
        {
            m_inputHandlerHelper.HasFoucs = true;
        }

        protected virtual void SetItemMaterial( GameObject item, Material material, Color color )
        {
            var itemRenderers = item.GetComponentsInChildren<Renderer>();
            for (var i = 0; i < itemRenderers.Length; i++)
            {
                if (material != null) itemRenderers[i].material = material;
                SetMaterial(itemRenderers[i].material, color);
            }
        }

        protected virtual void SetMaterial( Material material, Color color )
        {
            if (material != null)
            {
                material.EnableKeyword("_EMISSION");

                if (material.HasProperty("_Color")) material.color = color;

                if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", ColorDarken(color, 50));
            }
        }

        /// <summary>
        /// 获取指定色的百分比灰度
        /// </summary>
        /// <param name="color">The source colour to apply the darken to.</param>
        /// <param name="percent">The percent to darken the colour by.</param>
        /// <returns>The new colour with the darken applied.</returns>
        public Color ColorDarken( Color color, float percent )
        {
            return new Color(NumberPercent(color.r, percent), NumberPercent(color.g, percent),
                NumberPercent(color.b, percent), color.a);
        }

        /// <summary>
        /// The NumberPercent method is used to determine the percentage of a given value【0 - 1】.
        /// </summary>
        /// <param name="value">The value to determine the percentage from</param>
        /// <param name="percent">The percentage to find within the given value.</param>
        /// <returns>A float containing the percentage value based on the given input.</returns>
        public float NumberPercent( float value, float percent )
        {
            percent = Mathf.Clamp(percent, 0f, 100f);
            return percent == 0f ? value : value - percent / 100f;
        }

        #endregion
    }
}