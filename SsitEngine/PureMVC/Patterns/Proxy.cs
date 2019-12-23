using SsitEngine.PureMVC.Interfaces;

namespace SsitEngine.PureMVC.Patterns
{
    /// <summary>
    ///     代理
    /// </summary>
    public class Proxy : Notifier, IProxy
    {
        /// <summary>
        ///     代理对象的名称
        /// </summary>
        public static string NAME = "Proxy";

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="proxyName"></param>
        /// <param name="data"></param>
        public Proxy( string proxyName, object data = null )
        {
            ProxyName = proxyName ?? NAME;
            if (data != null)
            {
                Data = data;
            }
        }

        /// <summary>
        ///     the proxy name
        /// </summary>
        public string ProxyName { get; protected set; }

        /// <summary>
        ///     the data object
        /// </summary>
        public object Data { get; set; }


        /// <summary>
        ///     Called by the Model when the Proxy is registered
        /// </summary>
        public virtual void OnRegister()
        {
        }

        /// <summary>
        ///     Called by the Model when the Proxy is removed
        /// </summary>
        public virtual void OnRemove()
        {
        }
    }
}