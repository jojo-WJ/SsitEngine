using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SsitEngine.EzReplay;

namespace Framework.Data
{
    /// <summary>
    /// 聊天消息数据
    /// </summary>
    [Serializable]
    public class MessageInfo : InfoData, ISerializable
    {
        public EnMessageType MessageType; //消息类型

        public string GroupName; //组别

        //public string Title;
        // public string UserName;//用户名
        public string UserDisplayName; //用户显示名称
        public string Time; //消息发出时间
        public string MessageContent; //消息内容
        public string MessageHighlightContent; //消息内容中需要特别显示的

        public static MessageInfo Generate( EnMessageType type, string content, string highlightContent = null )
        {
            var messageInfo = new MessageInfo();
            messageInfo.MessageType = type;
            messageInfo.MessageContent = content;
            messageInfo.MessageHighlightContent = highlightContent;

            return messageInfo;
        }

        #region 序列化

#if EZRE_PROTO_INFO
#else
        public MessageInfo()
        {
        }

        public MessageInfo( SerializationInfo info, StreamingContext context )
        {
            MessageType = (EnMessageType) info.GetValue("MessageType", typeof(EnMessageType));
            GroupName = info.GetString("GroupName");
            //Title = info.GetString("Title");
            //UserName = info.GetString("UserName");
            UserDisplayName = info.GetString("UserDisplayName");
            Time = info.GetString("Time");
            MessageContent = info.GetString("MessageContent");
            MessageHighlightContent = info.GetString("MessageHighlightContent");
        }

        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue("MessageType", MessageType);
            info.AddValue("GroupName", GroupName);
            //info.AddValue("Title", this.Title);
            //info.AddValue("UserName", this.UserName);
            info.AddValue("UserDisplayName", UserDisplayName);
            info.AddValue("Time", Time);
            info.AddValue("MessageContent", MessageContent);
            info.AddValue("MessageHighlightContent", MessageHighlightContent);
        }
#endif

        #endregion
    }

    /// <summary>
    /// 聊天集合
    /// </summary>
    [Serializable]
    public class MessageBoxProxyInfo : InfoData
    {
        public int cacheIndex;

        [NonSerialized] public List<MessageInfo> messageCache = new List<MessageInfo>();

        public void AddMessageInfo( MessageInfo info )
        {
            if (messageCache == null) messageCache = new List<MessageInfo>();
            messageCache.Add(info);
            cacheIndex = messageCache.Count;
            isChange = true;
        }

        #region 序列化

        public MessageBoxProxyInfo()
        {
        }

        public MessageBoxProxyInfo( SerializationInfo info, StreamingContext context )
        {
            cacheIndex = info.GetInt32("cacheIndex");
        }

        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue("cacheIndex", cacheIndex);
        }

        public override bool IsDifferentTo( SavedBase state, Object2PropertiesMapping o2m )
        {
            return isChange;
        }

        #endregion
    }
}