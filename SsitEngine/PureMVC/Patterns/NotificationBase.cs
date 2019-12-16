using SsitEngine.PureMVC.Interfaces;

namespace SsitEngine.PureMVC.Patterns
{
    /// <summary>
    ///     消息通知基类
    /// </summary>
    public class NotificationBase : INotification
    {
        // 0---65535

        /// <summary>
        ///     创建一个消息通知
        /// </summary>
        /// <param name="msgId">消息id</param>
        public NotificationBase( ushort msgId )
        {
            Id = msgId;
            Body = null;
        }

        /// <summary>
        ///     创建一个消息通知
        /// </summary>
        /// <param name="msgId">消息id</param>
        /// <param name="body">消息体</param>
        public NotificationBase( ushort msgId, object body )
        {
            Id = msgId;
            Body = body;
        }

        /// <summary>
        ///     消息id
        /// </summary>
        public ushort Id { get; }

        /// <summary>
        ///     消息体
        /// </summary>
        public object Body { get; set; }

        /// <summary>
        ///     重写消息to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return TextUtils.Format("Notification module: {0} Notification msgId: {0} Body: {1}", GetModule(), Id,
                Body.ToString());
        }

        #region 消息域

        /// <summary>
        ///     获取消息域索引
        /// </summary>
        /// <returns></returns>
        public int GetModule()
        {
            var tmpid = Id / SsitFrameUtils.MsgSpan;
            return tmpid * SsitFrameUtils.MsgSpan;
        }

        #endregion
    }
}