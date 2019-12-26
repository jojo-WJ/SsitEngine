using Framework.Procedure;
using SSIT.proto;
using SsitEngine.Unity.Procedure;

namespace Framework.Helper
{
    /// <summary>
    /// 提示消息辅助类
    /// </summary>
    public class PopTipHelper
    {
        /// <summary>
        /// 将提示消息同步给其他端
        /// </summary>
        /// <param name="msgInfo">要同步的消息</param>
        /// <param name="isSync2ChatForm">是否将该飘字窗体消息同步到聊天窗口  默认同步</param>
        public static void PopTipToMirror(MessageInfo msgInfo, bool isSync2ChatForm = true)
        {
            if (ProcedureManager.Instance.CurrentProcedure.StateId == (int) ENProcedureType.ProcedureSinglePlayer)
            {
                PopTip(new Data.MessageInfo(){ MessageContent = msgInfo.messageContent}, isSync2ChatForm);
                return;
            }

            //CSPopTipRequest request = new CSPopTipRequest() { popInfo = msgInfo };
            //NetSocket.MessagePackage package = new NetSocket.MessagePackage(ConstMessageID.CSPopTipRequest, request);
            //Facade.Instance.SendNotification((ushort)EnMirrorEvent.SendMessage, package, true);

            if (isSync2ChatForm)
            {
                if (string.IsNullOrEmpty(msgInfo.time))
                {
                    msgInfo.time = System.DateTime.Now.ToString("HH:mm");
                }

                if (string.IsNullOrEmpty(msgInfo.userDisplayName))
                {
                    msgInfo.userDisplayName = GlobalManager.Instance.CachePlayer.GetAttribute().Name;
                }

                if (string.IsNullOrEmpty(msgInfo.groupName))
                {
                    msgInfo.groupName = GlobalManager.Instance.CachePlayer.GetAttribute().Profession;
                }

                //CSChatMessageRequest chatRequest = new CSChatMessageRequest() { attachInfo = msgInfo };
                //NetSocket.MessagePackage chatPackage = new NetSocket.MessagePackage(ConstMessageID.CSChatMessageRequest, chatRequest);
                //Facade.Instance.SendNotification((ushort)EnMirrorEvent.SendMessage, chatPackage, true);
            }
        }

        /// <summary>
        /// 将提示消息同步给其他端
        /// </summary>
        /// <param name="content">同步的内容</param>
        /// <param name="userDisplayName">操作内容的角色名</param>
        /// <param name="groupName">组名</param>
        /// <param name="highlightContent">高亮内容 ps: 如何高亮就得在PopTipForm中设置了</param>
        /// <param name="time">事间</param>
        /// <param name="msgType">消息类型</param>
        /// <param name="isSync2ChatForm">是否将该飘字窗体消息同步到聊天窗口  默认同步</param>
        public static void PopTipToMirror(string content, string userDisplayName,
                                          string groupName = "", string highlightContent = "",
                                          string time = "", SSIT.proto.EnMessageType msgType = SSIT.proto.EnMessageType.ACTION
                                          , bool isSync2ChatForm = true)
        {
            MessageInfo msgInfo = new MessageInfo()
            {
                userDisplayName = userDisplayName,
                messageContent = content,
                groupName = groupName,
                messageHighlightContent = highlightContent,
                //MessageType = msgType,
                time = time
            };
            //CSPopTipRequest request = new CSPopTipRequest() { popInfo = msgInfo };
            //NetSocket.MessagePackage package = new NetSocket.MessagePackage(ConstMessageID.CSPopTipRequest, request);
            //Facade.Instance.SendNotification((ushort)EnMirrorEvent.SendMessage, package, true);

            if (isSync2ChatForm)
            {
                if (string.IsNullOrEmpty(time))
                {
                    msgInfo.time = System.DateTime.Now.ToString("HH:mm");
                }

                if (string.IsNullOrEmpty(msgInfo.userDisplayName))
                {
                    msgInfo.userDisplayName = GlobalManager.Instance.CachePlayer.GetAttribute().Name;
                }

                if (string.IsNullOrEmpty(msgInfo.groupName))
                {
                    msgInfo.groupName = GlobalManager.Instance.CachePlayer.GetAttribute().Profession;
                }

                //CSChatMessageRequest chatRequest = new CSChatMessageRequest() { attachInfo = msgInfo };
                //NetSocket.MessagePackage chatPackage = new NetSocket.MessagePackage(ConstMessageID.CSChatMessageRequest, chatRequest);
                //Facade.Instance.SendNotification((ushort)EnMirrorEvent.SendMessage, chatPackage, true);
            }

        }

        /// <summary>
        /// 将提示消息同步给其他端
        /// </summary>
        public static void PopTipToAllChat(MessageInfo msgInfo)
        {
            if (string.IsNullOrEmpty(msgInfo.time))
            {
                msgInfo.time = System.DateTime.Now.ToString("HH:mm");
            }

            if (string.IsNullOrEmpty(msgInfo.userDisplayName))
            {
                msgInfo.userDisplayName = GlobalManager.Instance.CachePlayer.GetAttribute().Name;
            }

            if (string.IsNullOrEmpty(msgInfo.groupName))
            {
                msgInfo.groupName = GlobalManager.Instance.CachePlayer.GetAttribute().Profession;
            }

            //CSChatMessageRequest chatRequest = new CSChatMessageRequest() { attachInfo = msgInfo };
            //NetSocket.MessagePackage chatPackage = new NetSocket.MessagePackage(ConstMessageID.CSChatMessageRequest, chatRequest);
            //Facade.Instance.SendNotification((ushort)EnMirrorEvent.SendMessage, chatPackage, true);
        }

        /// <summary>
        /// 本地飘字窗体的显示
        /// </summary>
        /// <param name="msgInfo"></param>
        /// <param name="isSync2ChatForm">是否将该飘字窗体消息同步到聊天窗口  默认同步</param>
        public static void PopTip(Data.MessageInfo msgInfo, bool isSync2ChatForm = true)
        {
            //Facade.Instance.SendNotification((ushort)ConstNotification.ShowPopTips, msgInfo);
            if (isSync2ChatForm)
            {
                if (string.IsNullOrEmpty(msgInfo.Time))
                {
                    msgInfo.Time = System.DateTime.Now.ToString("HH:mm");
                }
                if (string.IsNullOrEmpty(msgInfo.UserDisplayName))
                {
                    msgInfo.UserDisplayName = GlobalManager.Instance.CachePlayer.GetAttribute().Name;
                }

                if (string.IsNullOrEmpty(msgInfo.GroupName))
                {
                    msgInfo.GroupName = GlobalManager.Instance.CachePlayer.GetAttribute().Profession;
                }

                //Facade.Instance.SendNotification((ushort)UIManoeuvreChatFormEvent.ReceiveChatMsg, msgInfo);
            }
        }

        /// <summary>
        /// 本地的飘字窗体
        /// 特殊的显示逻辑放在PopTipsForm 具体显示逻辑需自己实现
        /// </summary>
        /// <param name="content">飘窗内容</param>
        /// <param name="userDisplayName">角色名</param>
        /// <param name="groupName">组名</param>
        /// <param name="highlightContent">高亮内容</param>
        /// <param name="time">发送时间</param>
        /// <param name="msgType">消息类型</param>
        /// <param name="isSync2ChatForm">是否将该飘字窗体消息同步到聊天窗口  默认同步</param>
        public static void PopTip(string content, string userDisplayName,
                                          string groupName = "", string highlightContent = "",
                                          string time = "", EnMessageType msgType = EnMessageType.ACTION
                                           , bool isSync2ChatForm = false)
        {
            Data.MessageInfo msgInfo = new Data.MessageInfo()
            {
                UserDisplayName = userDisplayName,
                MessageContent = content,
                GroupName = groupName,
                MessageHighlightContent = highlightContent,
                MessageType = msgType,
                Time = time,
            };
            //Facade.Instance.SendNotification((ushort)ConstNotification.ShowPopTips, msgInfo);

            if (isSync2ChatForm)
            {
                if (string.IsNullOrEmpty(time))
                {
                    msgInfo.Time = System.DateTime.Now.ToString("HH:mm");
                }
                if (string.IsNullOrEmpty(msgInfo.UserDisplayName))
                {
                    msgInfo.UserDisplayName = GlobalManager.Instance.CachePlayer.GetAttribute().Name;
                }

                if (string.IsNullOrEmpty(msgInfo.GroupName))
                {
                    msgInfo.GroupName = GlobalManager.Instance.CachePlayer.GetAttribute().Profession;
                }

                //Facade.Instance.SendNotification((ushort)UIManoeuvreChatFormEvent.ReceiveChatMsg, msgInfo);
            }
        }


    }
}