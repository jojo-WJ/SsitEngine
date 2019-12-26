using Framework;
using Framework.Logic;
using Framework.Procedure;
using Framework.SceneObject;
using Framework.SsitInput;
using SsitEngine.Core;
using SsitEngine.DebugLog;
using SsitEngine.EzReplay;
using SsitEngine.Unity;
using SsitEngine.Unity.Procedure;
using System.Collections.Generic;
using Framework.Data;
using Framework.Mirror;
using SSIT.proto;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum EnInputMode
{
    None,
    /// <summary>
    /// 编辑
    /// </summary>
    Editor,
    /// <summary>
    /// 漫游
    /// </summary>
    Free,
    /// <summary>
    /// 控制
    /// </summary>
    Control,
    /// <summary>
    /// 锁定
    /// </summary>
    Lock,
}

public class ClientExitParam
{
    public string userId;
    public bool isClient;
    public bool isLobby;
    public bool isExit;
    public bool isEnd;
}

public class GlobalManager : ManagerBase<GlobalManager>
{
    public const int c_sDefaultServerNet = 999999;

    // 全局操作模式
    private EnInputMode m_inputMode = EnInputMode.None;

    // 房间信息代理
    //private RoomProxy m_roomProxy;
    // 场景信息代理
    private SceneInfoProxy m_sceneInfoProxy;
    // 场景聊天信息代理
    private SceneMessageProxy m_messageProxy;
    // 场景划线代理
    private DrawArrowProxy m_arrowProxy;

    /// <summary>
    /// 全局事件列表
    /// </summary>
    private Dictionary<ushort, SsitAction<INotification>> mEventMaps;

    /// <summary>
    /// 全局同步标记
    /// </summary>
    private bool m_isSync = false;

    /// <summary>
    /// 全局回放标记
    /// </summary>
    private ActionMode m_replayMode = ActionMode.STOPPED;

    /// <summary>
    /// 全局当前控制角色
    /// </summary>
    private Player m_player;
    private Player m_cachePlayer;


    private AudioSource m_bjzAudioSource;

    public override void OnSingletonInit()
    {
        mEventMaps = new Dictionary<ushort, SsitAction<INotification>>();
        InitEventMap();
        //m_roomProxy = null;
        SceneManager.sceneLoaded += OnSceneLoadCallBack;
    }

    #region 模块接口实现

    /// <inheritdoc />
    public override string ModuleName
    {
        get
        {
            return typeof(GlobalManager).FullName;
        }
    }

    /// <summary>
    /// 计时器模块优先级
    /// </summary>
    public override int Priority
    {
        get
        {
            return (int)EnModuleType.ENMODULETIMER;
        }
    }



    /// <summary>
    /// 计时器刷新
    /// </summary>
    /// <param name="elapsed"></param>
    public override void OnUpdate(float elapsed)
    {

    }

    /// <inheritdoc />
    public override void Shutdown()
    {
//        if (m_roomProxy != null)
//        {
//            Facade.Instance.RemoveProxy(RoomProxy.NAME);
//            m_roomProxy = null;
//        }
        mEventMaps.Clear();
        mEventMaps = null;
    }


    #endregion

    #region 消息处理

    void OnEnable()
    {
        m_msgList = new[]
        {
            //global
            (ushort) EnGlobalEvent.Start,
            (ushort) EnGlobalEvent.Exit,


            // lobby
            (ushort) EnGlobalEvent.InitRoomInfo,
            (ushort) EnGlobalEvent.ApplyRoomInfo,
            (ushort) EnGlobalEvent.UpdateRoomState,
            (ushort) EnGlobalEvent.UpdateRoomInfo,
            (ushort) EnGlobalEvent.OnUpdateRoomInfo,
            (ushort) EnMirrorEvent.OnLobbyClientEnter,
            (ushort) EnMirrorEvent.OnLobbyClientReady,
            (ushort) EnMirrorEvent.OnRoomClientExit,
            (ushort) EnRoomEvent.OnLobbyMemberExchange,

            // input
            (ushort) EnGlobalEvent.ChangeInputMode,

            // ermergency
            (ushort)EnGlobalEvent.SceneLoadProcess,
            (ushort)EnGlobalEvent.ChangeEnvInfo,
            (ushort)EnGlobalEvent.ReceiveChatMsg,
            (ushort)EnGlobalEvent.InitGlobalProxy,
            (ushort)EnGlobalEvent.RemoveGlobalProxy,

            (ushort)EnGlobalEvent.FollowClient,
        };
        RegisterMsg(m_msgList);


    }


    void OnDisable()
    {
        UnRegisterMsg(m_msgList);
    }


    private void InitEventMap()
    {
        mEventMaps.Clear();

        // 注册全局事件
        mEventMaps.Add((ushort)EnGlobalEvent.Start, OnGlobalStart);
        mEventMaps.Add((ushort)EnGlobalEvent.Exit, OnGlobalExit);

        // 注册大厅房间代理事件
        mEventMaps.Add((ushort)EnGlobalEvent.InitRoomInfo, InitRoomInfo);
        mEventMaps.Add((ushort)EnGlobalEvent.UpdateRoomState, UpdateRoomState);
        mEventMaps.Add((ushort)EnGlobalEvent.ApplyRoomInfo, ApplyRoomInfo);
        mEventMaps.Add((ushort)EnGlobalEvent.UpdateRoomInfo, UpdateRoomInfo);
        mEventMaps.Add((ushort)EnGlobalEvent.OnUpdateRoomInfo, OnUpdateRoomInfo);
        mEventMaps.Add((ushort)EnMirrorEvent.OnLobbyClientEnter, OnLobbyClientEnter);
        mEventMaps.Add((ushort)EnMirrorEvent.OnLobbyClientReady, OnLobbyClientReady);
        mEventMaps.Add((ushort)EnRoomEvent.OnLobbyMemberExchange, OnLobbyMemberExchange);
        mEventMaps.Add((ushort)EnMirrorEvent.OnRoomClientExit, OnLobbyClientExit);

        // 注册演练事件
        mEventMaps.Add((ushort)EnGlobalEvent.InitGlobalProxy, OnInitGlobalProxy);
        mEventMaps.Add((ushort)EnGlobalEvent.RemoveGlobalProxy, OnRemoveGlobalProxy);

        mEventMaps.Add((ushort)EnGlobalEvent.SceneLoadProcess, OnLoadSceneProcess);
        mEventMaps.Add((ushort)EnGlobalEvent.ChangeEnvInfo, OnChangeEnvInfo);
        mEventMaps.Add((ushort)EnGlobalEvent.ReceiveChatMsg, OnReceiveChatMsg);
        mEventMaps.Add((ushort)EnGlobalEvent.FollowClient, OnFollowClient);


        // 注册input事件
        mEventMaps.Add((ushort)EnGlobalEvent.ChangeInputMode, OnChangeInputMode);

    }




    /// <inheritdoc />
    public override void HandleNotification(INotification notification)
    {
        ProcessMessageEvent(notification.Id, notification);
    }

    public bool ProcessMessageEvent(ushort msgType, INotification notification)
    {
        if (null == mEventMaps)
            return false;

        if (!mEventMaps.ContainsKey(msgType))
        {
            SsitDebug.Error("globalManager ProcessMessageEvent:  " + msgType + "  isn't bind to mEventMaps");
            return false;
        }

        mEventMaps[msgType].Invoke(notification);
        return true;
    }

    #endregion

    #region Internal Callback

    #region Unity LoadLevelCallback

    private void OnSceneLoadCallBack(UnityEngine.SceneManagement.Scene arg0, LoadSceneMode loadMode)
    {
        // mulity audioListener deal
        AudioListener[] audioListeners = GameObject.FindObjectsOfType<AudioListener>();

        for (int i = 0; i < audioListeners.Length; i++)
        {
            audioListeners[i].enabled = false;
        }

        // 2018-09-19 20:10:59 Shell Lee
        // 存在多个AudioListener会导致声音播放出问题：
        // 1、人物走动时候，声音听不到了【editor正常，发布版不正常】
        if (Camera.main && Camera.main.GetComponentInChildren<AudioListener>(true))
        {
            Camera.main.GetComponentInChildren<AudioListener>(true).enabled = true;
        }
        else
        {
            if (audioListeners.Length > 0)
            {
                audioListeners[0].enabled = true;
            }
        }
    }

    #endregion

    #region Global

    private void OnGlobalStart(INotification notification)
    {
        ENProcedureType notificationBody = (ENProcedureType)notification.Body;
        SsitDebug.Info("系统进入主进程" + notificationBody);
        ProcedureManager.Instance.StartProcedure((int)notificationBody);
    }

    private void OnGlobalExit(INotification notification)
    {
        SsitDebug.Info("系统退出当前进程 => ENProcedureType.ProcedureMain");

        //保存并上传回放
        if (GlobalManager.Instance.ReplayMode == ActionMode.READY)
        {
            Facade.Instance.SendNotification((ushort)EnEzReplayEvent.Save);
            return;
        }

        ProcedureManager.Instance.ChangeProcedure((int)ENProcedureType.ProcedureMain);
    }

    #endregion

    #region 大厅房间

    private void InitRoomInfo(INotification notification)
    {
//        MvEventArgs args = notification as MvEventArgs;
//        if (args == null)
//            return;
//        var roomInfo = args.Body as RoomInfo;
//
//        if (roomInfo == null)
//        {
//            SsitDebug.Error("房间信息异常");
//            return;
//        }
//        roomInfo.mState = EnRoomState.EN_Connecting;
//
//        if (m_roomProxy == null)
//        {
//            m_roomProxy = new RoomProxy(roomInfo);
//            Facade.Instance.RegisterProxy(m_roomProxy);
//        }
//        else
//        {
//            m_roomProxy.Apply(roomInfo);
//        }
//
//        //        if (args.IntValue)
//        //        {
//        m_roomProxy.SetDefaultCurrentMember(args.IntValue);
        //}
    }

    private void UpdateRoomState(INotification notification)
    {
//        if (m_roomProxy == null)
//        {
//            SsitDebug.Error("房间信息异常");
//            return;
//        }
//
//        EnRoomState roomState = (EnRoomState)notification.Body;
//
//
//        m_roomProxy.GetRoomInfo().mState = roomState;
//        switch (roomState)
//        {
//            case EnRoomState.EN_Playing:
//                //TODO 给java 服务器推送状态
//                CSEnterGame csEnterGame = new CSEnterGame();
//                csEnterGame.roomGuid = m_roomProxy.GetRoomInfo().ID;
//                MessagePackage messagePackage = new MessagePackage(ConstMessageID.CSEnterGame, csEnterGame, EnProtoFlag.En_New);
//                Facade.Instance.SendNotification((ushort)EnSocketEvent.SendMessage, messagePackage);
//
//                break;
//        }
    }

    public void ApplyRoomInfo(INotification notification)
    {
//        if (m_roomProxy == null)
//        {
//            SsitDebug.Error("房间信息异常");
//            return;
//        }
//
//        m_roomProxy.ApplyRoomInfo((SCGroupInfoResult)notification.Body);
    }

    private void UpdateRoomInfo(INotification notification)
    {
//        if (m_roomProxy == null)
//        {
//            SsitDebug.Error("房间信息异常");
//            return;
//        }
//        m_roomProxy.UpdateRoomInfo((SwarpMemberInfo)notification.Body);
    }

    private void OnUpdateRoomInfo(INotification notification)
    {
//        if (m_roomProxy == null)
//        {
//            SsitDebug.Error("房间信息异常");
//            return;
//        }
//        m_roomProxy.OnUpdateRoomInfo((SCMembersResult)notification.Body);
    }

    private void OnLobbyClientEnter(INotification notification)
    {
//        if (m_roomProxy == null)
//        {
//            SsitDebug.Error("房间信息异常");
//            return;
//        }
//        m_roomProxy.OnLobbyClientEnter((int)notification.Body);
    }

    private void OnLobbyMemberExchange(INotification notification)
    {
//        if (m_roomProxy == null)
//        {
//            SsitDebug.Error("房间信息异常");
//            return;
//        }
//        m_roomProxy.OnLobbyMemberExchange((MemberInfo)notification.Body);
    }

    private void OnLobbyClientReady(INotification notification)
    {
//        if (m_roomProxy == null)
//        {
//            SsitDebug.Error("房间信息异常");
//            return;
//        }
//
//        if (notification is MvEventArgs msgArgs)
//        {
//            m_roomProxy.UpdateMemberState((string)msgArgs.Body, msgArgs.BoolValue);
//        }
    }

    private void OnLobbyClientExit(INotification notification)
    {
//        if (m_roomProxy == null)
//        {
//            //SsitDebug.Error("房间信息异常");
//            return;
//        }


        // 移除数据代理
//        if (notification.Body is ClientExitParam msgArgs)
//        {
//            if (msgArgs.isEnd)
//            {
//                CachePlayer = null;
//                Player = null;
//
//                Facade.Instance.RemoveProxy(RoomProxy.NAME);
//                m_roomProxy = null;
//                return;
//            }
//
//            MemberInfo temp = null;
//            if (msgArgs.isClient)
//            {
//                temp = m_roomProxy.FindMemberInfoByUserId(msgArgs.userId);
//            }
//            else
//            {
//                temp = m_roomProxy.UpdateMemberState(msgArgs.userId, false, true, msgArgs.isLobby);
//            }
//
//            //hack:提示端进行退出弹窗
//            Facade.Instance.SendNotification((ushort)UIMsg.OpenForm, En_UIForm.TipForm, new TipContent()
//            {
//                content = TextUtils.Format(DataContentProxy.GetTipContent(EnText.ExitRoomTip), temp?.UserName),
//                formType = En_ConfirmFormType.En_Succeed,
//            });
//        }
    }



    #endregion

    #region 应急演练

    private void OnInitGlobalProxy(INotification obj)
    {
        m_sceneInfoProxy = new SceneInfoProxy();
        Facade.Instance.RegisterProxy(m_sceneInfoProxy);

        if (GlobalManager.Instance.ReplayMode == ActionMode.PLAY)
        {
            Object2PropertiesMappingListWrapper args = obj.Body as Object2PropertiesMappingListWrapper;
            if (args == null)
            {
                SsitDebug.Error("OnInitGlobalProxy 回放数据异常[458]");
                return;
            }
            m_messageProxy = new SceneMessageProxy(new MessageBoxProxyInfo() { messageCache = args.messageInfos });
            m_arrowProxy = new DrawArrowProxy(new ArrowProxyInfo() { arrowCache = args.pathInfos });
        }
        else
        {
            m_messageProxy = new SceneMessageProxy(new MessageBoxProxyInfo());
            m_arrowProxy = new DrawArrowProxy(new ArrowProxyInfo());

        }
        Facade.Instance.RegisterProxy(m_messageProxy);
        Facade.Instance.RegisterProxy(m_arrowProxy);
    }

    private void OnRemoveGlobalProxy(INotification obj)
    {
        Facade.Instance.RemoveProxy(SceneInfoProxy.NAME);
        m_sceneInfoProxy = null;
        Facade.Instance.RemoveProxy(SceneMessageProxy.NAME);
        m_messageProxy = null;
        Facade.Instance.RemoveProxy(DrawArrowProxy.NAME);
        m_arrowProxy = null;
    }

    private void OnLoadSceneProcess(INotification obj)
    {
        if (obj.Body is SCLoadSceneProcessResult result)
        {
//            var member = m_roomProxy.GetRoomInfo().GetMemberByUserID(result.userId);
//            member?.SetLoadStatu(result.readyState, result.readyTime);
        }
    }

    /// <summary>
    /// 天气（环境）改变回调
    /// </summary>
    /// <param name="obj"></param>
    private void OnChangeEnvInfo(INotification obj)
    {
        if (obj.Body is SCEnvironmentResult result)
        {
            //var member = m_roomProxy.GetRoomInfo().GetMemberByUserID(result.userId);
            //member?.SetLoadStatu(result.readyState, result.readyTime);
            EnvironmentInfo info = result?.enviInfo[0];
            WeatherInfo weatherInfo = new WeatherInfo()
            {
                Weather = (Framework.EnWeather)info.Weather,
                WindLevel = info.WindLevel,
                WindDirection = (Framework.EnWindDirection)info.WindDirection,
                WindVelocity = info.WindVelocity
            };
            m_sceneInfoProxy?.SetWeather(weatherInfo);
            //Facade.Instance.SendNotification((ushort)UIGuiderFormEvent.WeatherChanged);
        }
    }

    /// <summary>
    /// 接收到服务器的聊天消息
    /// </summary>
    /// <param name="obj"></param>
    private void OnReceiveChatMsg(INotification obj)
    {
        if (obj.Body is SCChatMessageResult result)
        {
            Framework.Data.MessageInfo msgInfo = new Framework.Data.MessageInfo()
            {
                GroupName = result.attachInfo.groupName,
                MessageContent = result.attachInfo.messageContent,
                MessageType = (Framework.EnMessageType)result.attachInfo.MessageType,
                MessageHighlightContent = result.attachInfo.messageHighlightContent,
                Time = result.attachInfo.time,
                UserDisplayName = result.attachInfo.userDisplayName,
            };
            m_messageProxy.AddMessage(msgInfo);
            //Facade.Instance.SendNotification((ushort)UIManoeuvreChatFormEvent.ReceiveChatMsg, msgInfo);
        }

    }

    /// <summary>
    /// 客户端监控(仅服务端执行)
    /// </summary>
    /// <param name="obj"></param>
    private void OnFollowClient(INotification obj)
    {
        if (obj.Body is SCFollowClientResult result)
        {
            Player netPlayer = (Player)ObjectManager.Instance.GetObject(result.userID);
            NetPlayerAgent agent = null;
            if (result.isFollow == 1)//1为跟随 0为不跟随
            {
                agent = netPlayer?.GetRepresent()?.GetComponent<NetPlayerAgent>();
            }
            else
            {
                Facade.Instance.SendNotification((ushort)EnGlobalEvent.ChangeInputMode, EnInputMode.Free);
            }
            CameraController.instance.FollowGamePlayer(agent);
        }

        //MvEventArgs args = obj.Body as MvEventArgs;
        //if (args == null)
        //    return;

        //var userId = (string)args.Body;

        //Player netPlayer = (Player)ObjectManager.Instance.GetObject(userId);

        //if (netPlayer != null)
        //{
        //    if (args.BoolValue)
        //    {
        //        Facade.Instance.SendNotification((ushort)EnGlobalEvent.ChangeInputMode, EnInputMode.Free);
        //    }
        //    else
        //    {
        //        NetPlayerAgent agent = netPlayer.GetRepresent()?.GetComponent<NetPlayerAgent>();
        //        if (agent)
        //        {
        //            CameraController.instance.FollowGamePlayer(agent);
        //        }
        //    }

        //    if (IsSync)
        //    {
        //        //param1:userid       userId
        //        //param2:state 0||1   args.boolValue
        //        //Facade.Instance.SendNotification((ushort)EnMirrorEvent.SendMessage,new mes);
        //    }
        //    return;
        //}

        // SsitDebug.Error("OnFollowClient is exception [GloablManager:509]");
    }

    #endregion

    #endregion

    #region 全局操作模式

    public EnInputMode InputMode
    {
        get { return m_inputMode; }
        set
        {
            if (m_inputMode != value/* && onInputModeChange != null*/)
            {
                //var mode = m_inputMode;
                m_inputMode = value;
                //onInputModeChange(this,mode,m_inputMode);
                Facade.Instance.SendNotification((ushort)EnGlobalEvent.OnChangeInputMode, m_inputMode);
            }
        }
    }

    /// <summary>
    /// 全局同步标记
    /// </summary>
    public bool IsSync
    {
        get { return m_isSync; }
        set { m_isSync = value; }
    }

    /// <summary>
    /// 全局当前控制角色
    /// </summary>
    public Player Player
    {
        get { return m_player; }
        set
        {
            if (m_player != value || m_inputMode != EnInputMode.Control)
            {
                if (m_player != null)
                {
                    //卸载操作指令
                    CameraController.instance.SetPlayer(null);
                    m_player.PlayerController.OnInputAttach(false);
                    m_player.PlayerController.StopInput();
                    //m_player.PlayerController.lockInput = true;
                }

                m_cachePlayer = value ?? m_cachePlayer;
                m_player = value;
                //todo：判断权限
                Facade.Instance.SendNotification((ushort)EnInputEvent.UpdateOperators, m_player);
                //绑定摄像机
                if (m_player != null && CameraController.instance != null)
                {
                    // m_player.PlayerController.lockInput = false;
                    CameraController.instance.SetPlayer(m_player.GetRepresent().transform);
                    m_player.PlayerController.OnInputAttach(true);
                    InputManager.Instance.ResetFocus();
                }

            }
        }
    }

    public Player CachePlayer
    {
        get { return m_cachePlayer; }
        set
        {
            if (value != null && m_cachePlayer != value)
            {
                m_cachePlayer = value;
                Facade.Instance.SendNotification((ushort)EnGlobalEvent.ChangePlayer, m_cachePlayer);
            }

        }
    }


    /// <summary>
    /// 报警柱播放器
    /// </summary>
    public AudioSource BJZAudioSource
    {
        get => m_bjzAudioSource;
        set
        {
            if (value == null)
            {
                m_bjzAudioSource = null;
            }
            else if (m_bjzAudioSource == null)
            {
                m_bjzAudioSource = value;
            }
        }
    }

    /// <summary>
    /// 全局回放标记
    /// </summary>
    public ActionMode ReplayMode
    {
        get { return m_replayMode; }
        set { m_replayMode = value; }
    }

    private void OnChangeInputMode(INotification obj)
    {
        InputMode = (EnInputMode)obj.Body;

        switch (InputMode)
        {
            case EnInputMode.Free:
                GlobalManager.Instance.Player = null;
                break;
        }
    }

    #endregion
}
