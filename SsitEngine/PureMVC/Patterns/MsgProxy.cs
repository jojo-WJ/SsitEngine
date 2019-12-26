using SsitEngine.PureMVC.Interfaces;

namespace SsitEngine.PureMVC.Patterns
{
    /// <summary>
    /// 代理
    /// </summary>
    public class MsgProxy : Notifier, IProxy
    {
        /// <summary>
        /// 代理对象的名称
        /// </summary>
        public static string NAME = "MsgProxy";

        /// <summary>
        /// 消息集合
        /// </summary>
        protected ushort[] m_msgList;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="proxyName"></param>
        /// <param name="data"></param>
        public MsgProxy( string proxyName, object data = null )
        {
            ProxyName = proxyName ?? NAME;
            if (data != null)
            {
                Data = data;
            }
        }

        /// <summary>
        /// the proxy name
        /// </summary>
        public string ProxyName { get; protected set; }

        /// <summary>
        /// the data object
        /// </summary>
        public object Data { get; set; }


        /// <summary>
        /// Called by the Model when the Proxy is registered
        /// </summary>
        public virtual void OnRegister()
        {
            RegisterMsg(m_msgList);
        }

        /// <summary>
        /// Called by the Model when the Proxy is removed
        /// </summary>
        public virtual void OnRemove()
        {
            UnRegisterMsg(m_msgList);
        }

        /// <summary>
        /// 消息注册
        /// </summary>
        /// <param name="msgs"></param>
        public void RegisterMsg( params ushort[] msgs )
        {
            if (null == msgs)
            {
                return;
            }
            foreach (var t in msgs)
            {
                Facade.Instance.RegisterObservers(this, t, HandleNotification);
            }
        }

        /// <summary>
        /// 消息注销
        /// </summary>
        /// <param name="msgs"></param>
        public void UnRegisterMsg( params ushort[] msgs )
        {
            if (null == msgs)
            {
                return;
            }
            foreach (var t in msgs)
            {
                Facade.Instance.RemoveObservers(this, t);
            }
        }

        /// <summary>
        /// 消息回调
        /// </summary>
        /// <param name="notification"></param>
        protected virtual void HandleNotification( INotification notification )
        {
        }
    }
}