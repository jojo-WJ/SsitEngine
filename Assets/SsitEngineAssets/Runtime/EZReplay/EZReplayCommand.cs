using SsitEngine.DebugLog;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;

namespace Framework.Logic
{
    public class OnEZReplayInitCommand : SimpleCommand
    {
        public override void Execute( INotification notification )
        {
            //Object2PropertiesMappingListWrapper replayData = notification.Body as Object2PropertiesMappingListWrapper;
            //if (replayData == null) { return; }
            //RoomInfo room = replayData.roomInfo;
            // set scene res by sceneProxy
            //if (room == null)
            {
                SsitDebug.Error("回放房间信息不存在");
            }
            //SceneManagerProxy sceneServerProxy = Facade.Instance.RetrieveProxy(SceneManagerProxy.NAME) as SceneManagerProxy;
            //if (sceneServerProxy == null) { return; }
            //sceneServerProxy.SetResources(room.SceneId, room.SchemeId, true);
            //sceneServerProxy.InitRoomInfo( ref room, false );
            //sceneServerProxy.SetConfigResources( replayData.schemeInfo);

            // 保存房间信息
            //RoomProxy roomProxy = new RoomProxy(room);
            //roomProxy.SetGroupControl();
            //Facade.Instance.RegisterProxy(roomProxy);
            // TODO：设置当前组的信息（默认为导调端）
            //roomProxy.SetDefaultCurrentMember();

            //SceneManagerProxy sceneManagerProxy = Facade.Instance.RetrieveProxy(SceneManagerProxy.NAME) as SceneManagerProxy;
            //if (sceneManagerProxy == null) { return; }
            //Facade.Instance.SendNotification((ushort)ConstNotification.OnLoadingScene, true);
            //SceneMessageProxy messageProxy = Facade.Instance.RetrieveProxy(SceneMessageProxy.NAME) as SceneMessageProxy;
            /*if (messageProxy == null)
            {
                SsitDebug.Error("场景消息框代理不存在");
            }
            else
            {
                //messageBoxProxy.LoadCacheMessages(replayData.messageInfos);
            }

            ProcedureManager.Instance.ChangeProcedure((int)ENProcedureType.ProcedureReplay);*/
        }
    }

    public class CSAddEzreplayCommand : SimpleCommand
    {
        public override void Execute( INotification notification )
        {
        }
    }
}