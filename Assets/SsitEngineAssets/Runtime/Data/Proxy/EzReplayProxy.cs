using Framework.Data;
using Framework.Procedure;
using SsitEngine.DebugLog;
using SsitEngine.EzReplay;
using SsitEngine.Unity.Procedure;
using System.Collections.Generic;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Unity.UI;
using Table;
using MessageInfo = Framework.Data.MessageInfo;

namespace Framework.Logic
{
    public class EzReplayProxy : MsgProxy
    {
        public new static string NAME = "EzReplayProxy";

        //cache 多线程访问
        //private RoomInfo mCacheRoomInfo;
        private List<MessageInfo> mMessageCacheInfos;
        private List<PathVertexesInfo> mPathCacheInfos;

        private EZReplayManager mAgent;

        public EzReplayProxy( EZReplayManager agent ) : base(NAME)
        {
            mAgent = agent;
        }

        #region 子类实现

        /// <summary>
        /// 注册代理
        /// </summary>
        public override void OnRegister()
        {
            /*RoomProxy roomProxy = Facade.Instance.RetrieveProxy(RoomProxy.NAME) as RoomProxy;
            if (roomProxy != null)
            {
                //回放代理临时缓存回放数据以备线程调用
                mCacheRoomInfo = (RoomInfo) roomProxy.GetRoomInfo().Clone();
            }*/
            /*else
            {
                SsitDebug.Debug("记录异常");
            }*/

            m_msgList = new ushort[]
            {
                (ushort) EnEzReplayEvent.Init,
                (ushort) EnEzReplayEvent.Mark,
                (ushort) EnEzReplayEvent.Record,
                (ushort) EnEzReplayEvent.Save,
                (ushort) EnEzReplayEvent.Upload,
            };

            base.OnRegister();

         }

        /// <summary>
        /// 注销代理
        /// </summary>
        public override void OnRemove()
        {
            // Replay
            base.OnRemove();
            
            mAgent = null;
            //mCacheRoomInfo = null;

            if (mMessageCacheInfos != null)
                mMessageCacheInfos.Clear();
            mMessageCacheInfos = null;

            if (mPathCacheInfos != null)
                mPathCacheInfos.Clear();
            mPathCacheInfos = null;
        }
        
        /// <summary>
        /// 消息回调
        /// </summary>
        /// <param name="notification"></param>
        protected override void HandleNotification( INotification notification )
        {
            // Replay
            switch (notification.Id)
            {
                case (ushort) EnEzReplayEvent.Init: 
                    OnEZReplayInit(notification);
                    break;
                case (ushort) EnEzReplayEvent.Mark:
                    OnEzReplayMark(notification);
                    break;
                case (ushort) EnEzReplayEvent.Record:
                    OnEzReplayRecord(notification);
                    break;
                case (ushort) EnEzReplayEvent.Save:
                    OnEzReplaySave(notification);
                    break;
                case (ushort) EnEzReplayEvent.Upload:
                    OnEzReplayUpload(notification);
                    break;
            }
        }

        #endregion

        #region 对外接口

//        public RoomInfo GetCacheRoomInfo()
//        {
//            return mCacheRoomInfo;
//        }

        public List<MessageInfo> GetCacheMessageInfo()
        {
            return mMessageCacheInfos;
        }

        public List<PathVertexesInfo> GetCachePathInfo()
        {
            return mPathCacheInfos;
        }

        public void SetCacheMessageInfo( List<MessageInfo> messageInfos )
        {
            mMessageCacheInfos = messageInfos;
        }

        #endregion

        #region Internal Members

        /// <summary>
        /// 回放播放初始化
        /// </summary>
        /// <param name="notification"></param>
        public void OnEZReplayInit( INotification notification )
        {
            Object2PropertiesMappingListWrapper replayData = notification.Body as Object2PropertiesMappingListWrapper;
            if (replayData == null)
            {
                return;
            }
            //RoomInfo room = replayData.roomInfo;
            // set scene res by sceneProxy
//            if (room == null)
//            {
//                SsitDebug.Error("回放房间信息不存在");
//                return;
//            }

            //Reset group control handler
//            room.SetGroupControl();
//
//            //send gloabl
//            GlobalManager.Instance.ReplayMode = ActionMode.PLAY;
//            Facade.Instance.SendNotification((ushort) EnGlobalEvent.InitRoomInfo, room, (int) EnGroupType.EN_GUIDER);
//            Facade.Instance.SendNotification((ushort) EnGlobalEvent.InitGlobalProxy, replayData);
//
//            //get sceneName
//            SceneDefine define =
//                DataManager.Instance.GetData<SceneDefine>((int) EnLocalDataType.DATA_SCENE, room.SceneId);
//
//            if (define == null)
//            {
//                SsitDebug.Error($"回放场景当前版本不兼容{room.SceneId}");
//                return;
//            }
            //change procedure
            //ProcedureManager.Instance.ChangeProcedure((int) ENProcedureType.ProcedureReplay, define.SceneFileName);
        }

        private void OnEzReplayMark( INotification obj )
        {
            ISave tt = obj.Body as ISave;
            if (tt == null)
            {
                SsitDebug.Error("OnEzReplayMark is exception");
                return;
            }
            EZReplayManager.Instance.Mark4Recording(tt.GetRepresent(), tt, null);
        }

        /// <summary>
        /// 回放录制回调
        /// </summary>
        /// <param name="obj"></param>
        private void OnEzReplayRecord( INotification obj )
        {
            mAgent.Record();
        }

        /// <summary>
        /// 回放保存
        /// </summary>
        private void OnEzReplaySave( INotification obj )
        {
            /*RoomProxy roomProxy = Facade.Instance.RetrieveProxy(RoomProxy.NAME) as RoomProxy;
            if (roomProxy != null)
            {
                Facade.Instance.SendNotification((ushort) UIMsg.OpenForm, En_UIForm.LoadingForm,
                    DataContentProxy.GetTipContent(EnText.DataCollectTip));

                mAgent.Stop();

                //回放代理临时缓存回放数据以备线程调用
                //mCacheRoomInfo = (RoomInfo)roomProxy.GetRoomInfo().Clone();
                mMessageCacheInfos = ((SceneMessageProxy) Facade.Instance.RetrieveProxy(SceneMessageProxy.NAME))
                    .GetAllMessageCache();
                mPathCacheInfos = ((DrawArrowProxy) Facade.Instance.RetrieveProxy(DrawArrowProxy.NAME)).GetAllCacheInfo();
*/

                /*EzreplayInfo ezreplayInfo = new EzreplayInfo();
                RoomInfo info = roomProxy.GetRoomInfo();

                ezreplayInfo.sceneName = SceneManager.Instance.Level.SceneName;

                ezreplayInfo.accidentName = info.AccidentName;
                ezreplayInfo.userCount = roomProxy.GetAllUserCount();
                ezreplayInfo.hostUserName = roomProxy.GetCurrentMemberInfo().UserName;
                ezreplayInfo.beginTime =
                    Util.ConvertLongToDateTime(mAgent.RecordBeginTime).ToString("yyyy/MM/dd HH:mm:ss");
                mAgent.RecordEndTime = Util.ConvertDataTimeToLong(DateTime.Now);
                //mAgent.StopWatch();
                TimeSpan dt = Util.ConvertLongToDateTime(mAgent.RecordEndTime) -
                              Util.ConvertLongToDateTime(mAgent.RecordBeginTime);
                //this.tt += Time.deltaTime;
                //Debug.Log("expression"+tt);
                ezreplayInfo.durationTime = $"{dt.Hours:D2}:{dt.Minutes:D2}:{dt.Seconds:D2}";
                ezreplayInfo.evaluateState = 0;
                // todo:等待前天数据接入
                ezreplayInfo.schemeID = roomProxy.GetRoomInfo().SchemeId.ToString();
                //向服务器推送回放信息
                //access:服务器接入
                MessagePackage package = new MessagePackage(ConstMessageID.CSAddEzreplay,
                    new CSAddEzreplay() {info = ezreplayInfo});
                SendNotification((ushort) EnSocketEvent.SendMessage, package);*/
            //}
        }

        /// <summary>
        /// 回放上传
        /// </summary>
        private void OnEzReplayUpload( INotification notification )
        {
            /*SCAddEzreplayResult result = notification.Body as SCAddEzreplayResult;
            //test dirty data
            //Facade.Instance.SendNotification((ushort)EnGlobalEvent.Exit);
            //return;

            if (result == null)
            {
                SsitDebug.Error("添加回放到网络返回类型异常");
                return;
            }

            if (result.resultCode == 0)
            {
                SsitDebug.Error("上传开始");
                Facade.Instance.SendNotification((ushort) UIMsg.OpenForm, En_UIForm.LoadingForm,
                    DataContentProxy.GetTipContent(EnText.UpLoadTip));

                mAgent.Stop();
                mAgent.ClearCache();
                byte[] ret = null;
                // access:显示加载界面

                ThreadUtils.RunThread(
                    delegate { ret = mAgent.InternalSaveToStream(); },
                    delegate
                    {
                        if (ret == null || ret.Length <= 0)
                        {
                            SsitDebug.Error("回放序列化失败");
                            return;
                        }

                        //上传
                        ResourcesManager.Instance.UpLoadAsset(ret, (int) NetWorkFileMsgEnum.Accident_Replay,
                            mAgent.GetFileName(), result.resultGUID,
                            ( string msg ) =>
                            {
                                //access:界面接入
                                SsitDebug.Error("上传成功");
                                Facade.Instance.SendNotification((ushort) UIMsg.CloseForm, En_UIForm.LoadingForm);

                                EZReplayManager.Instance.Exit();
                                Facade.Instance.SendNotification((ushort) EnGlobalEvent.Exit);
                            },
                            ( string msg ) =>
                            {
                                //access:界面接入
                                SsitDebug.Error("上传失败");
                            } /*,
                            (float value) =>
                            {
                                SsitDebug.Error($"上传中{value}");
                            }#1#);
                    });
            }
            else*/
            {
                Facade.Instance.SendNotification((ushort) EnGlobalEvent.Exit);
                //SsitDebug.Error("添加回放到网络返回失败" + result.resultString);
            }
        }

        #endregion

        
        public void ClearCacheInfo()
        {
            //mCacheRoomInfo = null;
            mMessageCacheInfos = null;
        }
    }
}