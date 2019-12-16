using SsitEngine.PureMVC.Interfaces;

namespace SsitEngine.PureMVC.Patterns
{
    /// <summary>
    ///     通知基类
    /// </summary>
    public class Notifier : AllocatedObject, INotifier
    {
        /// <inheritdoc />
        public void SendNotification( ushort msgId, object body )
        {
            Facade.Instance.SendNotification(msgId, body);
        }

        /// <inheritdoc />
        public void SendNotification( ushort msgId, object body, params object[] values )
        {
            Facade.Instance.SendNotification(msgId, body, values);
        }

        /// <summary>
        ///     发送一个无消息体的通知
        /// </summary>
        /// <param name="msgId">消息id</param>
        public void SendNotification( ushort msgId )
        {
            Facade.Instance.SendNotification(msgId);
        }
    }
}