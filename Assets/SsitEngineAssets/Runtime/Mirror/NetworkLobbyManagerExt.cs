//#define Active_Proto
/*
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：                                                    
*│　作    者：Xuxin                                              
*│　版    本：1.0.0                                                 
*│　创建时间：2019/12/24 10:46:57                             
*└──────────────────────────────────────────────────────────────┘
*/

using Framework.Data;
using Framework.NetSocket;
using Mirror;
using SSIT.proto;
using SsitEngine.DebugLog;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework.Mirror
{
    public class NetworkLobbyManagerExt : NetworkRoomManager
    {
        
        /// <summary>
        /// Called just after GamePlayer object is instantiated and just before it replaces LobbyPlayer object.
        /// This is the ideal point to pass any data like player name, credentials, tokens, colors, etc.
        /// into the GamePlayer object as it is about to enter the Online scene.
        /// </summary>
        /// <param name="roomPlayer"></param>
        /// <param name="gamePlayer"></param>
        /// <returns>true unless some code in here decides it needs to abort the replacement</returns>
        public override bool OnRoomServerSceneLoadedForPlayer( GameObject roomPlayer, GameObject gamePlayer )
        {
            NetPlayerAgent playerAgent = gamePlayer.GetComponent<NetPlayerAgent>();
            var rp = roomPlayer.GetComponent<RoomPlayer>();
            playerAgent.index = rp.index;
            playerAgent.guid = rp.userId;
            
            return true;
        }


        /*
            This code below is to demonstrate how to do a Start button that only appears for the Host player
            showStartButton is a local bool that's needed because OnLobbyServerPlayersReady is only fired when
            all players are ready, but if a player cancels their ready state there's no callback to set it back to false
            Therefore, allPlayersReady is used in combination with showStartButton to show/hide the Start button correctly.
            Setting showStartButton false when the button is pressed hides it in the game scene since NetworkLobbyManager
            is set as DontDestroyOnLoad = true.
        */

        public bool allPlayersLoaded = false;

        public bool AllPlayersLoaded
        {
            get { return allPlayersReady; }
        }

        public override void OnRoomServerPlayersReady()
        {
            // calling the base method calls ServerChangeScene as soon as all players are in Ready state.
            if (isHeadless)
            {
                base.OnRoomServerPlayersReady();
            }
            else
            {
                //todo:通知界面
                Debug.LogError("当前系统无显卡程序无法进行");
                //showStartButton = true;
            }
        }

        #region 消息处理

        void OnEnable()
        {
            m_msgList = new ushort[]
            {
                (ushort) EnMirrorEvent.CreateRoom,
                (ushort) EnMirrorEvent.EnterRoom,
                (ushort) EnMirrorEvent.ExitRoom,
                (ushort) EnMirrorEvent.SendMessage,
                (ushort) EnMirrorEvent.StartGame,
                (ushort) EnMirrorEvent.LoadScene,
                (ushort) EnMirrorEvent.LoadSceneProcess,

                (ushort) EnMirrorEvent.OnLobbyClientConnect
            };
            RegisterMsg(m_msgList);
        }

        void OnDisable()
        {
            UnRegisterMsg(m_msgList);
        }

        /// <inheritdoc />
        public override void HandleNotification( INotification notification )
        {
            /*switch (notification.Id)
            {
                case (ushort) EnMirrorEvent.CreateRoom:
                    OnCreateLobby(notification.Body as RoomInfo);
                    break;
                case (ushort) EnMirrorEvent.EnterRoom:
                    OnEnterLobby(notification.Body as RoomInfo);
                    break;
                case (ushort) EnMirrorEvent.ExitRoom:
                    OnExitLobby();
                    break;
                case (ushort) EnMirrorEvent.SendMessage:
                    OnSendMessage(notification);
                    break;
                case (ushort) EnMirrorEvent.OnLobbyClientConnect:
                    //OnDisconnectLobby();
                    break;
                case (ushort) EnMirrorEvent.StartGame:
                    OnStartGame((int) notification.Body);
                    break;
                case (ushort) EnMirrorEvent.LoadScene:
                    OnSceneLoad(notification.Body as SCLoadSceneResult);
                    break;
                case (ushort) EnMirrorEvent.LoadSceneProcess:
                    OnLoadSceneProcess(notification.Body as SCLoadSceneProcessResult);
                    break;
            }*/
        }

        #endregion

        #region client handlers

        public override void OnRoomClientEnter()
        {
        }

        public override void OnRoomClientExit()
        {
        }

        public override void OnRoomClientConnect( NetworkConnection conn )
        {
        }


        public override void OnRoomClientDisconnect( NetworkConnection conn )
        {
            UnityEngine.Debug.LogError("OnLobbyClientDisconnect---------------------");

            //hack:处理异常中断的情况
            //Facade.Instance.SendNotification((ushort)EnMirrorEvent.OnLobbyClientDisconnect);
            Facade.Instance.SendNotification((ushort) EnGlobalEvent.Exit);

            // 回到大厅列表
            //OnDisconnectLobby();
        }

        public override void OnRoomStartClient()
        {
            Debug.Log("OnLobbyStartClient");
            networkSceneName = "";
            // 注册自定义系统消息
            //NetworkClient.RegisterHandler<MessagePackage>(OnClientBoardCastMessage);
            //Facade.Instance.SendNotification((ushort) EnGlobalEvent.UpdateRoomState, EnRoomState.EN_Connected);

            // ProcedureManager.Instance.ChangeProcedure((int)ENProcedureType.ProcedureLobby);
        }

        public override void OnRoomStopClient()
        {
            
        }

        /*public override void OnClientChangeScene( string newSceneName, SceneOperation sceneOperation )
        {
            base.OnClientChangeScene(newSceneName,sceneOperation);
        }*/

        #endregion

        #region lobby server virtuals

        public override void OnRoomStartHost()
        {
            // 注册自定义系统消息
            Debug.Log("OnLobbyStartHost");
        }

        public override void OnRoomStopHost()
        {
        }

        public override void OnRoomStartServer()
        {
            Debug.Log("OnLobbyStartServer");
            networkSceneName = "";
            // 注册自定义系统消息
            //NetworkServer.RegisterHandler<MessagePackage>(OnServerBoardCastMessage);
            // 注册服务端消息机制
            //NetworkUtils.InitMessageMap();
            //Facade.Instance.SendNotification((ushort) EnGlobalEvent.UpdateRoomState, EnRoomState.EN_Connected);
        }

        public override void OnRoomServerConnect( NetworkConnection conn )
        {
        }

        public override void OnRoomServerDisconnect( NetworkConnection conn )
        {
            SsitDebug.Debug("OnLobbyServerDisconnect" + conn.connectionId);
        }


        public override void OnServerSceneChanged( string sceneName )
        {
            //if (sceneName != LobbyScene && allPlayersLoaded)
            //{
            //    // call SceneLoadedForPlayer on any players that become ready while we were loading the scene.
            //    foreach (PendingPlayer pending in pendingPlayers)
            //        SceneLoadedForPlayer(pending.conn, pending.lobbyPlayer);

            //    pendingPlayers.Clear();
            //}

            //todo:修正
            //OnLobbyServerSceneChanged(sceneName);
        }

        public override void OnRoomServerSceneChanged( string sceneName )
        {
        }

        #endregion

        #region lobby client virtuals

        public override void OnRoomClientSceneChanged( NetworkConnection conn )
        {
            //通知演练进程场景加载完成
            //ProcedureManager.Instance.CurrentProcedure.OnEvent(ProcedureManager.Instance, this, (int)ENProcedureStatu.SceneLoaded, null);
        }

        #endregion

#if Active_Proto
        #region Create And Destory Lobby

        public void OnCreateLobby(RoomInfo roomInfo)
        {
            //启动房间
            StartServer();
            // hack:host测试
            // StartHost();
            //给服务器推送加入房间消息
            CSEnterDrillRoom cSJoinLobby = new CSEnterDrillRoom() { drillGuid = roomInfo.ID };
            MessagePackage package = new MessagePackage(ConstMessageID.CSJoinLobbyRoom, cSJoinLobby);
            Facade.Instance.SendNotification((ushort)EnSocketEvent.SendMessage, package);
        }

        /// <summary>
        /// 进入大厅
        /// </summary>
        /// <param name="roomInfo">房间信息</param>
        private void OnEnterLobby(RoomInfo roomInfo)
        {
            //var items = roomInfo.IP.Split(':');
            //if (0 != items.Length)
            //{
            //    networkAddress = items[items.Length - 1];

            //    // 注册全局数据代理
            //    Facade.Instance.SendNotification((ushort)EnGlobalEvent.InitRoomInfo,roomInfo);
            //    // 建立客户端和主机端的tcp连接，在连接回调中处理连接成功的事件
            //    StartClient();
            //    // todo:弹出连接中的显示界面
            //}
            networkAddress = roomInfo.IP;
            StartClient();
        }

        private void OnExitLobby()
        {
            // stop
            if (NetworkClient.active && !NetworkServer.active)
            {
                StopClient();
            }
            else if (NetworkServer.active || NetworkClient.isConnected)
            {

                StopHost();
            }

            OnDisconnectLobby();

        }

        private void OnStartGame(int sceneId)
        {
            // 检测加入成员准备状态
            ReadyStatusChanged();


            // 如果所有人准备中进入演练场景
            if (allPlayersReady)
            {
                //检测本地场景数据
                SceneDefine define =
 DataManager.Instance.GetData<SceneDefine>((int)EnLocalDataType.DATA_SCENE, sceneId);
                if (define == null)
                {
                    SsitDebug.Error("本地场景数据异常");
                    return;
                }
                //更新房间状态并通知java服进行房间信息更新
                Facade.Instance.SendNotification((ushort)EnGlobalEvent.UpdateRoomState, EnRoomState.EN_Playing);

                /*ServerChangeScene(GameplayScene);*/
                //这种做法会造成服务器和客户端的场景、资源加载不同步的现象，加长场景加载的时间作用
                //因此进行场景优化做法：
                //1、通知客机进行资源加载；
                //2、集成mirror场景加载；
                //Facade.Instance.SendNotification((ushort)EnMirrorEvent.SendMessage,);
                //access: 不接受java的反馈
                // 待后期替换java服务器 调整 mrioor机制
                Facade.Instance.SendNotification((ushort)UIMsg.OpenForm, En_UIForm.LoadingForm);
                OnServerLoadScene(sceneId, define.SceneFileName);
            }
            else
            {
                //todo:提示界面人员未准备或者人员准备通知界面刷新开始演练的按钮具体根据设计需求来定
                SsitDebug.Debug("todo:提示界面人员未准备或者人员准备通知界面刷新开始演练的按钮具体根据设计需求来定");
                return;
            }
            // hack:replace to procedure deal
            // GameplayScene = "HBSH_LXCZ";
        }

        private void OnDisconnectLobby()
        {
            //向服务端发送关闭房间命令
            var roompxy = Facade.Instance.RetrieveProxy(RoomProxy.NAME) as RoomProxy;
            if (roompxy != null)
            {
                //Debug.LogError(roompxy.GetRoomInfo().ID);
                MessagePackage package1 =
 new MessagePackage(ConstMessageID.CSExitDrillRoom, new CSExitDrillRoom() { roomGuid =
 roompxy.GetRoomInfo().ID }, EnProtoFlag.En_New);
                Facade.Instance.SendNotification((ushort)EnSocketEvent.SendMessage, package1);
            }
            Facade.Instance.SendNotification((ushort)EnMirrorEvent.OnLobbyClientExit, new ClientExitParam()
            {
                userId = null,
                isClient = true,
                isExit = true,
                isEnd = true,
            });
        }


        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="sceneResult">指定的场景信息</param>
        private void OnSceneLoad(SCLoadSceneResult sceneResult)
        {
            if (NetworkClient.active && !NetworkServer.active)
            {
                OnClientLoadScene(sceneResult.levelInfo.sceneName);
            }
        }

        /// <summary>
        /// 加载进度及加载状态
        /// </summary>
        /// <param name="loadResult"></param>
        private void OnLoadSceneProcess(SCLoadSceneProcessResult loadResult)
        {
            var curProcedure = ProcedureManager.Instance.CurrentProcedure;
            if (curProcedure.StateId != (int)ENProcedureType.ProcedureEmergency)
            {
                //SsitDebug.Error("系统进程异常中 OnLoadSceneProcess");
                return;
            }

            Facade.Instance.SendNotification((ushort)EnGlobalEvent.SceneLoadProcess, loadResult);

            if (loadResult.readyState != (int)LoadedType.finished)
            {
                return;
            }

            curProcedure.OnEvent(ProcedureManager.Instance, this, (int)ENProcedureStatu.AllPlayerLoaded, loadResult);
        }

        /// <summary>
        /// 发送消息广播回调
        /// </summary>
        /// <param name="notification">消息包</param>
        private void OnSendMessage(INotification notification)
        {
            MvEventArgs args = notification as MvEventArgs;
            if (args == null)
                return;
            var message = args.Body as MessagePackage;
            //是否指定服务器激活【服务器直接中转消息处理器】;
            bool isServerActive = args.BoolValue;
            if (NetworkClient.active && !NetworkServer.active)
            {
                NetworkClient.Send<MessagePackage>(message);
            }
            else if (NetworkServer.active || NetworkClient.isConnected)
            {
                if (isServerActive)
                {
                    OnServerBoardCastMessage(null, message);
                }
                else
                {
                    NetworkServer.SendToAll<MessagePackage>(message);
                }

            }
        }

        #endregion

        #region Custom Message CallBack

        /// <summary>
        /// 客户端消息接受回调
        /// </summary>
        /// <param name="arg1">网络连接信息</param>
        /// <param name="arg2">指定的消息包</param>
        private void OnClientBoardCastMessage(NetworkConnection arg1, MessagePackage arg2)
        {
            ushort notify = ProtoMessageMap.Instance.GetValue((ConstMessageID)arg2.MessageId);
            //Debug.Log("expression"+(ConstMessageID)arg2.MessageId);
            Facade.Instance.SendNotification(notify, arg2.MessageBody);
        }

        /// <summary>
        /// 服务端消息接受回调
        /// </summary>
        /// <param name="arg1">网络连接信息</param>
        /// <param name="arg2">指定的消息包</param>
        private void OnServerBoardCastMessage(NetworkConnection arg1, MessagePackage arg2)
        {
            NetworkUtils.ProcessMessageEvent((ConstMessageID)arg2.MessageId, arg2);
        }

        /// <summary>
        /// 客户端加载场景
        /// </summary>
        /// <param name="newSceneName">指定的加载场景</param>
        private void OnClientLoadScene(string newSceneName)
        {
            #if Active_Proto
            //todo：加载场景Bundle
            Facade.Instance.SendNotification((ushort)UIMsg.OpenForm, En_UIForm.LoadingForm);

            var procedure =
 ProcedureManager.Instance.GetProcedure<ProcedureEmergency>((int)ENProcedureType.ProcedureEmergency);
            if (procedure == null)
            {
                SsitDebug.Error("ProcedureEmergency is not register [ NetworkLobbyManagerExt OnClientChangeScene 175] ");
                return;
            }
            Facade.Instance.SendNotification((ushort)UIMsg.OpenForm, En_UIForm.LoadingForm);
            ProcedureManager.Instance.ChangeProcedure((int)ENProcedureType.ProcedureEmergency, newSceneName);
            GameplayScene = newSceneName;
            if (string.IsNullOrEmpty(GameplayScene))
            {
                Debug.LogError("ClientChangeScene empty scene name");
                return;
            }

            if (LogFilter.Debug)
                Debug.Log("ClientChangeScene newSceneName:" + GameplayScene + " networkSceneName:" + networkSceneName);

            //if (GameplayScene == networkSceneName)
            //{
            //    return;
            //}
            // vis2k: pause message handling while loading scene. otherwise we will process messages and then lose all
            // the state as soon as the load is finishing, causing all kinds of bugs because of missing state.
            // (client may be null after StopClient etc.)
            if (LogFilter.Debug)
                Debug.Log("ClientChangeScene: pausing handlers while scene is loading to avoid data loss after scene was loaded.");
            //Transport.activeTransport.enabled = false;
            // Let client prepare for scene change
            OnClientChangeScene(GameplayScene);
            //This should probably not change if additive is used          
            networkSceneName = newSceneName;
            ProcedureManager.Instance.GetProcedure<ProcedureEmergency>((int)ENProcedureType.ProcedureEmergency).SubscribeEvent((int)ENProcedureStatu.SceneLoaded, OnClientChangedSceneCallback);
            #endif
        }

        /// <summary>
        /// 客户端场景加载完成回调
        /// </summary>
        /// <param name="proceduremanager">进程管理器</param>
        /// <param name="sender">发送者</param>
        /// <param name="userdata">用户自定义的数据</param>
        private void OnClientChangedSceneCallback(IProcedureManager proceduremanager, object sender, object userdata)
        {
            //场景完成回调
            FinishLoadScene();
        }

        /// <summary>
        /// 服务器加载场景
        /// </summary>
        /// <param name="sceneId">指定的加载场景id</param>
        /// <param name="newSceneName">指定的加载场景名称</param>
        private void OnServerLoadScene(int sceneId, string newSceneName)
        {
            if (string.IsNullOrEmpty(newSceneName))
            {
                Debug.LogError("ServerChangeScene empty scene name");
                return;
            }
            if (LogFilter.Debug)
                Debug.Log("ServerChangeScene " + newSceneName);
            NetworkServer.SetAllClientsNotReady();
            networkSceneName = newSceneName;
            allPlayersLoaded = false;
            //清除起始位置的缓存
            startPositionIndex = 0;
            startPositions.Clear();
            //跳转响应演练进程
            Facade.Instance.SendNotification((ushort)UIMsg.OpenForm, En_UIForm.LoadingForm);

            ProcedureManager.Instance.ChangeProcedure((int)ENProcedureType.ProcedureEmergency, newSceneName);

            //todo:发送所有客机进行场景加载
            LevelInfo level = new LevelInfo() { sceneId = sceneId, sceneName = newSceneName, physicsMode = 0, sceneMode
 = 0 };
            MessagePackage package =
 new MessagePackage(ConstMessageID.CSLoadSceneRequest, new CSLoadSceneRequest() { levelInfo = level });
            Facade.Instance.SendNotification((ushort)EnMirrorEvent.SendMessage, package, true);

            //订阅进程资源加载事件
            ProcedureManager.Instance.GetProcedure<ProcedureEmergency>((int)ENProcedureType.ProcedureEmergency).SubscribeEvent((int)ENProcedureStatu.SceneLoaded, OnServerChangedSceneCallback);
            ProcedureManager.Instance.GetProcedure<ProcedureEmergency>((int)ENProcedureType.ProcedureEmergency).SubscribeEvent((int)ENProcedureStatu.AllPlayerLoaded, OnAllClientChangedSceneCallback);

        }

        /// <summary>
        /// 检测所有客户端加载完成
        /// note：因为客户端在加载场景过程中消息系统是进行屏蔽的，避免消息的丢失，因此需要在所有客户端场景加载完成过程后，服务端进行消息广播
        /// </summary>
        internal override void LoadStatusChanged()
        {
            int CurrentPlayers = 0;
            int ReadyPlayers = 0;

            //if (lobbySlots.Count != 0)
            {
                if (Facade.Instance.RetrieveProxy(RoomProxy.NAME) is RoomProxy roomProxy)
                {
                    var clinet = roomProxy.GetAllAvailableUser();
                    for (int i = 0; i < clinet.Count; i++)
                    {
                        var mm = clinet[i];
                        CurrentPlayers++;
                        if (mm.loadResult.IsLoaded)
                            ReadyPlayers++;
                    }
                }
            }


            if (CurrentPlayers == ReadyPlayers)
            {
                allPlayersLoaded = true;
                SCLoadSceneProcessResult result = new SSIT.proto.SCLoadSceneProcessResult() { readyState = 3, readyTime
 = 1 };
                MessagePackage msg = new MessagePackage(MessageID.SCLoadSceneProcessResult, result);
                Facade.Instance.SendNotification((ushort)EnMirrorEvent.SendMessage, msg);
                OnLoadSceneProcess(result);
            }
            else
            {
                allPlayersLoaded = false;
            }
        }

        /// <summary>
        /// 服务器场景加载完成回调
        /// </summary>
        /// <param name="proceduremanager">进程管理器</param>
        /// <param name="sender">发送者</param>
        /// <param name="userdata">用户自定义的数据</param>
        private void OnServerChangedSceneCallback(IProcedureManager proceduremanager, object sender, object userdata)
        {
            //场景完成回调
            FinishLoadScene();
            LoadStatusChanged();
        }

        private void OnAllClientChangedSceneCallback(IProcedureManager proceduremanager, object sender, object userdata)
        {
            //场景完成回调
            if (allPlayersLoaded)
            {
                // call SceneLoadedForPlayer on any players that become ready while we were loading the scene.
                foreach (PendingPlayer pending in pendingPlayers)
                    SceneLoadedForPlayer(pending.conn, pending.lobbyPlayer);

            }
            pendingPlayers.Clear();
        }

        #endregion
#endif
    }
}