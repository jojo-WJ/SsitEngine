//#define Active_Proto
/*
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：                                                    
*│　作    者：Xuxin                                              
*│　版    本：1.0.0                                                 
*│　创建时间：2019/12/24 10:46:57                             
*└──────────────────────────────────────────────────────────────┘
*/

using Framework.SceneObject;
using Mirror;
using SsitEngine.DebugLog;
using UnityEngine;

namespace Framework.Mirror
{
    public class NetworkUtils
    {
#if Active_Proto
        
        #region Unet 广播事件处理

        //private static bool isSync = true;//真对Unet or mirror 进行服务端标记
        private static Dictionary<ConstMessageID, SsitAction<ConstMessageID, MessagePackage>> messageMaps = null;

        /// <summary>
        /// 注册播放消息类型
        /// </summary>
        public static void InitMessageMap()
        {
            /*
            if (messageMaps == null)
            {
                messageMaps = new Dictionary<ConstMessageID, SsitAction<ConstMessageID, MessagePackage>>();
            }
            messageMaps.Clear();
            //New
            messageMaps.Add(ConstMessageID.CSSWarpMembresRequest, OnSwarpMemberCallBack);
            messageMaps.Add(ConstMessageID.CSSpawnSceneObjectRequest, OnSpawnSceneObjectCallBack);
            messageMaps.Add(ConstMessageID.CSLoadSceneRequest, OnLoadSceneCallBack);
            messageMaps.Add(ConstMessageID.CSLoadSceneProcessRequest, OnLoadSceneProcessCallBack);
            messageMaps.Add(ConstMessageID.CSDestorySceneObjectRequest, OnDestorySceneObjectCallBack);
            messageMaps.Add(ConstMessageID.CSAssignClientAuthorityRequest, OnAssignClientAuthorityCallBack);//hack:尚未实现
            messageMaps.Add(ConstMessageID.CSSyncSceneObjInfoRequest, OnSyncSceneObjInfoRequest);
            messageMaps.Add(ConstMessageID.CSEnvironmentRequest, OnEnvironmentRequestCallBack);
            messageMaps.Add(ConstMessageID.CSChatMessageRequest, OnChatMessageRequestCallBack);
            messageMaps.Add(ConstMessageID.CSFollowClientRequest, OnFollowClientRequestCallBack);
            messageMaps.Add(ConstMessageID.CSSyncPlayerInfoRequest, OnSyncPlayerInfoRequestCallBack);
            messageMaps.Add(ConstMessageID.CSPopTipRequest, OnPopTipRequestCallBack);
            messageMaps.Add(ConstMessageID.CSAccidentReportRequest, OnAccidentReportRequestCallBack);
            messageMaps.Add(ConstMessageID.CSRoutePlanRequest, CSRoutePlanRequestCallBack);
            messageMaps.Add(ConstMessageID.CSPulishTaskRequest, CSPulishTaskRequestCallBack);
            messageMaps.Add(ConstMessageID.CSCompleteTaskRequest, CSCompleteTaskRequestCallBack);
            messageMaps.Add(ConstMessageID.CSCountPresonRequest, CSCountPresonRequestCallBack);
            messageMaps.Add(ConstMessageID.CSEndDrillRequest, CSEndDrillRequestCallBack);
            messageMaps.Add(ConstMessageID.CSEvacuateRequest, CSEvacuateRequestCallBack);
            messageMaps.Add(ConstMessageID.CSSyncTransRequest, OnCSSyncTransRequest);
            messageMaps.Add(ConstMessageID.CSInterComRequest,CSInterComCallBack);
            messageMaps.Add(ConstMessageID.CSLobbyChatMessageRequest,CSLobbyChatMessageCallBack);
            */

        }


        public static bool ProcessMessageEvent(ConstMessageID msgType, MessagePackage messageBoady)
        {
            if (!messageMaps.ContainsKey(msgType))
            {
                SsitDebug.Error("The NetworkJsonMessageType:  " + msgType + "  isn't bind to ConstNotification");
                return false;
            }

            messageMaps[msgType].Invoke(msgType, messageBoady);
            return true;
        }


        /// <summary>
        /// 清除消息字典
        /// </summary>
        private void ClearMessageMap()
        {
            if (messageMaps != null)
            {
                messageMaps.Clear();
                messageMaps = null;
            }
        }
        #endregion

        #region 广播消息回调

        /// <summary>
        /// 交换座位
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private static void OnSwarpMemberCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            if (arg2.MessageBody is CSSWarpMembersRequest sMembresRequest)
            {

                switch (sMembresRequest.swarpInfo.state)
                {
                    case 0:
                        // 请求
                        {
                            // 当前位置没有人或者交换目标位置没有人
                            if (sMembresRequest.swarpInfo.tarInfo.empty == 1 || string.IsNullOrEmpty(sMembresRequest.swarpInfo.tarInfo.userID))
                            {
                                // 直接交换
                                Facade.Instance.SendNotification((ushort)EnGlobalEvent.UpdateRoomInfo, sMembresRequest.swarpInfo);
                                return;
                            }
                            SCSWarpMembersResult result = new SCSWarpMembersResult() { swarpInfo = sMembresRequest.swarpInfo };
                            MessagePackage message = new MessagePackage(ConstMessageID.SCSWarpMembresResult, result);
                            int clientConnId = GetConnectionIdByNetId(sMembresRequest.swarpInfo.tarInfo.connId);
                            NetworkServer.SendToClient(clientConnId, message);
                        }
                        break;
                    case 1:
                        // 接受
                        // 更新服务器房间模型代理
                        Facade.Instance.SendNotification((ushort)EnGlobalEvent.UpdateRoomInfo, sMembresRequest.swarpInfo);
                        break;
                    case 2:
                        //拒绝
                        {
                            //给交换目标发送消息
                            SCSWarpMembersResult result = new SCSWarpMembersResult() { swarpInfo = sMembresRequest.swarpInfo };
                            MessagePackage message = new MessagePackage(ConstMessageID.SCSWarpMembresResult, result);
                            NetworkServer.SendToClient(sMembresRequest.swarpInfo.curInfo.connId, message);
                        }

                        break;
                }

            }

        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private static void OnLoadSceneCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSLoadSceneRequest request = arg2.MessageBody as CSLoadSceneRequest;
            var info = request?.levelInfo;
            if (info == null)
            {
                SsitDebug.Error("OnLoadScene msseage is exception");
                return;
            }

            //创建返回消息体
            SCLoadSceneResult result = new SCLoadSceneResult { levelInfo = info };

            NetworkServer.SendToAll(new MessagePackage(ConstMessageID.SCLoadSceneResult, result));
        }

        /// <summary>
        /// 加载场景进度回调
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private static void OnLoadSceneProcessCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSLoadSceneProcessRequest request = arg2.MessageBody as CSLoadSceneProcessRequest;

            if (Facade.Instance.RetrieveProxy(RoomProxy.NAME) is RoomProxy roomProxy)
            {
                if (request != null)
                {
                    //创建返回消息体
                    SCLoadSceneProcessResult result = new SCLoadSceneProcessResult
                    {
                        userId = request.userId,
                        readyState = request.readyState,
                        readyTime = request.readyTime
                    };
                    Facade.Instance.SendNotification((ushort)EnGlobalEvent.SceneLoadProcess, result);
                    NetworkLobbyManagerExt lobby = NetworkManager.singleton as NetworkLobbyManagerExt;
                    lobby?.LoadStatusChanged();
                    NetworkServer.SendToAll(new MessagePackage(ConstMessageID.SCLoadSceneProcessResult, result));
                    return;
                }
            }
            SsitDebug.Error("OnLoadSceneProcessCallBack is exception");

        }

        /// <summary>
        /// 卵生对象
        /// </summary>
        /// <param name="arg1">消息id</param>
        /// <param name="arg2">消息体</param>
        private static void OnSpawnSceneObjectCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSSpawnSceneObjectRequest request = arg2.MessageBody as CSSpawnSceneObjectRequest;
            var info = request?.spawnInfo;
            if (info == null)
            {
                SsitDebug.Error("mirror spawn msseage is exception");
                return;
            }

            SceneObject obj = SsitApplication.Instance.CreateObject(null, info.dataId, null, info);

            //info.
            SCSpawnSceneObjectResult result = new SCSpawnSceneObjectResult();
            result.guid = obj.Guid;
            result.spawnInfo = info;
            NetworkServer.SendToAll<MessagePackage>(new MessagePackage(ConstMessageID.SCSpawnSceneObjectResult, result));
        }

        /// <summary>
        /// 客户端销毁对象请求
        /// </summary>
        /// <param name="arg1">消息id</param>
        /// <param name="arg2">消息体</param>
        private static void OnDestorySceneObjectCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSDestorySceneObjectRequest request = arg2.MessageBody as CSDestorySceneObjectRequest;
            string guid = request?.guid;
            if (string.IsNullOrEmpty(guid))
            {
                SsitDebug.Error("mirror OnDestorySceneObjectCallBack msseage is exception");
                return;
            }

            SsitApplication.Instance.DestoryObject(guid);
            Facade.Instance.SendNotification((ushort)EnInputEvent.FinishDeleteObject);

            //info.
            SCDestorySceneObjectResult result = new SCDestorySceneObjectResult();
            result.guid = guid;
            result.netId = result.netId;
            NetworkServer.SendToAll<MessagePackage>(new MessagePackage(ConstMessageID.SCDestorySceneObjectResult, result));
        }

        /// <summary>
        /// 客户端获取权限请求
        /// </summary>
        /// <param name="arg1">消息id</param>
        /// <param name="arg2">消息体</param>
        private static void OnAssignClientAuthorityCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSAssignClientAuthorityRequest request = arg2.MessageBody as CSAssignClientAuthorityRequest;
            if (request == null)
                return;
            var obj = ObjectManager.Instance.GetObject(request.guid);
            if (obj == null)
            {
                SsitDebug.Error("请求对象不存在");
                return;
            }
            NetworkConnection clientConn = GetConnectionByNetId(request.netId);

            if (clientConn != null && obj is Player player)
            {
                NetworkIdentity ni = player.GetRepresent().GetComponent<NetworkIdentity>();
                if (ni && ni.localPlayerAuthority)
                {
                    var clientAuthorityOwner = ni.clientAuthorityOwner;
                    if (clientConn == clientAuthorityOwner)
                        SsitDebug.Warning("权限已经设置过了 fuck");
                    bool resultState = ni.AssignClientAuthority(clientConn);
                    //info.
                    SCAssignClientAuthorityResult result = new SCAssignClientAuthorityResult();
                    result.guid = request.guid;
                    result.state = resultState ? 1 : 0;
                    NetworkServer.SendToClient(clientConn.connectionId, new MessagePackage(ConstMessageID.SCAssignClientAuthorityResult, result));
                }
            }

        }

        /// <summary>
        /// 客户端同步物体属性的消息
        /// </summary>
        /// <param name="arg1">消息id</param>
        /// <param name="arg2">消息体</param>
        private static void OnSyncSceneObjInfoRequest(ConstMessageID arg1, MessagePackage arg2)
        {
            CSSyncSceneObjInfoRequest request = arg2.MessageBody as CSSyncSceneObjInfoRequest;
            if (request == null)
                return;
            SCSyncSceneObjInfoResult result = new SCSyncSceneObjInfoResult();

            result.sceneObjInfo.AddRange(request.sceneObjInfo);
            Facade.Instance.SendNotification((ushort)EnObjectEvent.SyncSceneObjInfoResult, result);

            //result info
            NetworkServer.SendToAll(new MessagePackage(ConstMessageID.SCSyncSceneObjInfoResult, result));
        }


        private static void OnCSSyncTransRequest(ConstMessageID arg1, MessagePackage arg2)
        {
            CSSyncTransRequest request = arg2.MessageBody as CSSyncTransRequest;
            if (request == null)
                return;
            SCSyncTransResult result = new SCSyncTransResult();

            result.transInfo = request.transInfo;
            Facade.Instance.SendNotification((ushort)EnObjectEvent.SyncSyncTransResult, result);
            //result info
            NetworkServer.SendToAll(new MessagePackage(ConstMessageID.SCSyncTransResult, result));
        }

        /// <summary>
        /// 天气改变的回调
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private static void OnEnvironmentRequestCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSEnvironmentRequest request = arg2.MessageBody as CSEnvironmentRequest;
            EnvironmentInfo info = request?.enviInfo[0];

            if (info == null)
            {
                return;
            }

            SCEnvironmentResult result = new SCEnvironmentResult();
            result.enviInfo.Add(info);


            Facade.Instance.SendNotification((ushort)EnGlobalEvent.ChangeEnvInfo, result);

            NetworkServer.SendToAll<MessagePackage>(new MessagePackage(ConstMessageID.SCEnvironmentResult, result));
        }

        /// <summary>
        /// 场景内聊天消息回调
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private static void OnChatMessageRequestCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSChatMessageRequest request = arg2.MessageBody as CSChatMessageRequest;
            if (request == null || request.attachInfo == null)
            {
                return;
            }
            SCChatMessageResult result = new SCChatMessageResult();
            result.attachInfo = request.attachInfo;

            Facade.Instance.SendNotification((ushort)EnGlobalEvent.ReceiveChatMsg, result);
            NetworkServer.SendToAll<MessagePackage>(new MessagePackage(ConstMessageID.SCChatMessageResult, result));
        }

        /// <summary>
        /// 导调端监控请求
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public static void OnFollowClientRequestCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSFollowClientRequest request = arg2.MessageBody as CSFollowClientRequest;
            if (request == null)
            {
                return;
            }

            SCFollowClientResult result = new SCFollowClientResult();
            result.isFollow = request.isFollow;
            result.userID = request.userID;
            Facade.Instance.SendNotification((ushort)EnGlobalEvent.FollowClient, result);
            NetworkServer.SendToAll<MessagePackage>(new MessagePackage(ConstMessageID.SCFollowClientResult, result));

            //TODO:正常来说只需要给被监听的视角的人发送一下就行了，不用发送给全体
            //  NetworkServer.SendToClient(int.Parse( request.userID), new MessagePackage(ConstMessageID.SCFollowClientResult, result));
        }

        /// <summary>
        /// 同步角色属性回调
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private static void OnSyncPlayerInfoRequestCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSSyncPlayerInfoRequest request = arg2.MessageBody as CSSyncPlayerInfoRequest;
            var info = request?.playerInfo;
            if (info == null)
            {
                SsitDebug.Error("mirror spawn msseage is exception");
                return;
            }

            //同步属性字段 there is not deal,to objectManager
            
            //同步装备
            for (int i = 0; i < info.equips.Count; i++)
            {
                var equip = info.equips[i];
                if (string.IsNullOrEmpty(equip.guid))
                {
                    // new item
                    Item item = SsitApplication.Instance.CreateItem(null, equip.dataId);
                    info.guid = item.Guid;
                }
                else
                {
                    // there is not deal,if item'state is changed 
                }
            }
            //同步交互道具 there is not deal,to objectManager

            //同步状态 there is not deal,to objectManager

            //info.
            SCSyncPlayerInfoResult result = new SCSyncPlayerInfoResult();
            result.playerInfo = info;
            Facade.Instance.SendNotification((ushort)EnObjectEvent.SyncPlayerInfoResult, result);
            NetworkServer.SendToAll<MessagePackage>(new MessagePackage(ConstMessageID.SCSyncPlayerInfoResult, result));
        }

        /// <summary>
        /// 飘窗提示信息的回调
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private static void OnPopTipRequestCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSPopTipRequest request = arg2.MessageBody as CSPopTipRequest;
            if (request == null)
            {
                return;
            }
            SCPopTipResult result = new SCPopTipResult()
            {
                popInfo = request.popInfo,
            };
            Facade.Instance.SendNotification((ushort)ConstNotification.ShowPopTips, result);
            NetworkServer.SendToAll<MessagePackage>(new MessagePackage(ConstMessageID.SCPopTipResult, result));
        }

        /// <summary>
        /// 险情汇报的回调
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private static void OnAccidentReportRequestCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSAccidentReportRequest request = arg2.MessageBody as CSAccidentReportRequest;
            if (request == null)
            {
                return;
            }
            SCAccidentReportResult result = new SCAccidentReportResult()
            {
                accidentInfo = request.accidentInfo,
                reportorName = request.reportorName,
            };

            // Facade.Instance.SendNotification((ushort)ConstNotification.ShowPopTips, result);
            Facade.Instance.SendNotification((ushort)EnObjectEvent.RecordAccidentInfo, result);
            NetworkServer.SendToAll<MessagePackage>(new MessagePackage(ConstMessageID.SCAccidentReportResult, result));
        }

        /// <summary>
        /// 绘制路线同步回调
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private static void CSRoutePlanRequestCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSRoutePlanRequest request = arg2.MessageBody as CSRoutePlanRequest;
            if (request == null)
            {
                return;
            }

            CSRoutePlanResult result = new CSRoutePlanResult();
            result.routeInfo.AddRange(request.routeInfo);

            Facade.Instance.SendNotification((ushort)EnDarwArrowEvent.SyncAddDrawnArrow, result);
            NetworkServer.SendToAll<MessagePackage>(new MessagePackage(ConstMessageID.SCRoutePlanResult, result));
        }

        /// <summary>
        /// 发布任务
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private static void CSPulishTaskRequestCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSPulishTaskRequest request = arg2.MessageBody as CSPulishTaskRequest;
            if (request == null)
            {
                return;
            }

            SCPulishTaskResult result = new SCPulishTaskResult()
            {
                taskInfo = request.taskInfo
            };

            //Facade.Instance.SendNotification((ushort)ConstNotification.AddArrowPathNetwork, result);
            NetworkServer.SendToAll<MessagePackage>(new MessagePackage(ConstMessageID.SCPulishTaskResult, result));
        }

        /// <summary>
        /// 完成任务
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private static void CSCompleteTaskRequestCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSCompleteTaskRequest request = arg2.MessageBody as CSCompleteTaskRequest;
            if (request == null)
            {
                return;
            }

            SCCompleteTaskResult result = new SCCompleteTaskResult()
            {
                taskInfo = request.taskInfo
            };

            NetworkServer.SendToAll<MessagePackage>(new MessagePackage(ConstMessageID.SCCompleteTaskResult, result));
        }

        /// <summary>
        /// 清点人数
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private static void CSCountPresonRequestCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSCountPresonRequest request = arg2.MessageBody as CSCountPresonRequest;
            if (request == null)
            {
                return;
            }

            SCCountPresonResult result = new SCCountPresonResult();
            result.countInfo.AddRange(request.countInfo);

            NetworkServer.SendToAll<MessagePackage>(new MessagePackage(ConstMessageID.SCCountPresonResult, result));
        }

        /// <summary>
        /// //结束演练
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private static void CSEndDrillRequestCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSEndDrillRequest request = arg2.MessageBody as CSEndDrillRequest;
            if (request == null)
            {
                return;
            }

            SCEndDrillResult result = new SCEndDrillResult();

            NetworkServer.SendToAll<MessagePackage>(new MessagePackage(ConstMessageID.SCEndDrillResult, result));
        }

        /// <summary>
        /// 人员疏散
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public static void CSEvacuateRequestCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSEvacuateRequest request = arg2.MessageBody as CSEvacuateRequest;
            if (request == null)
            {
                return;
            }

            var playerList = ObjectManager.Instance.GetNpcList();
            var EvacuateInfoList = request.evacuateInfo;
            for (int i = 0; i < playerList.Count; i++)
            {
                if (EvacuateInfoList.Count > i)
                {
                    playerList[i].PlayerController.SetTargetPosition(EvacuateInfoList[i].targetPos,false);
                }
            }
        }


        private static void CSLobbyChatMessageCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSLobbyChatMessageRequest request = arg2.MessageBody as CSLobbyChatMessageRequest;
            if (request == null)
            {
                return;
            }

            SCLobbyChatMessageResult result = new SCLobbyChatMessageResult();
            result.Message = request.Message;
            Facade.Instance.SendNotification((ushort)ConstNotification.SCLobbyChatMessageResult, result);
            NetworkServer.SendToAll<MessagePackage>(new MessagePackage(ConstMessageID.SCLobbyChatMessageResult, result));
        }


        private static void  CSInterComCallBack(ConstMessageID arg1, MessagePackage arg2)
        {
            CSPulishTaskRequest request = arg2.MessageBody as CSPulishTaskRequest;
            if (request == null)
            {
                return;
            }

            SCPulishTaskResult result = new SCPulishTaskResult()
            {
                taskInfo = request.taskInfo
            };

            //Facade.Instance.SendNotification((ushort)ConstNotification.AddArrowPathNetwork, result);
            NetworkServer.SendToAll<MessagePackage>(new MessagePackage(ConstMessageID.SCInterComResult, result));
        }
        
        #endregion

#endif
        #region Extension 
        
        /// <summary>
        /// 获取权限，但对gameplayer的权限处理比较特殊
        /// </summary>
        /// <param name="go"></param>
        /// <param name="forceGamePlayer">是否强制获取角色操作权限，默认智能控制自己组的角色</param>
        public static void RequestAuthority(GameObject go, bool forceGamePlayer = false)
        {
            if (forceGamePlayer || !go.GetComponentInChildren<PlayerInstance>(true))
            {
                NetworkIdentity ni = go.GetComponentInChildren<NetworkIdentity>(true);
                if (NetworkClient.isConnected && ni && ni.isLocalPlayer)
                {
                    NetPlayerAgent localNetPlayerAgent = ClientScene.localPlayer.GetComponent<NetPlayerAgent>();
                    if (localNetPlayerAgent)
                    {
                        localNetPlayerAgent.CmdRequestAuthority(ni);
                    }
                    else
                    {
                        SsitDebug.Error("同步逻辑异常");
                    }
                }
            }
        }

        public static int GetConnectionIdByNetId(int netId)
        {
            if (NetworkServer.active)
            {
                if (NetworkIdentity.spawned.TryGetValue((uint)netId, out NetworkIdentity ret))
                {
                    return ret.connectionToClient.connectionId;
                }
            }

            return -1;
        }

        public static NetworkConnection GetConnectionByNetId(int netId)
        {
            if (NetworkServer.active)
            {
                if (NetworkIdentity.spawned.TryGetValue((uint)netId, out NetworkIdentity ret))
                {
                    return ret.connectionToClient;
                }
            }

            return null;
        }
        #endregion
    }
}
