using System.Collections.Generic;
using System.Text;
using SsitEngine.PureMVC.Interfaces;

namespace SsitEngine.PureMVC.Core
{
    /// <summary>
    ///     模型对象
    /// </summary>
    public class Model : IModel
    {
        /// <summary>
        ///     数据模型的静态实例对象锁
        /// </summary>
        protected static readonly object m_staticSyncRoot = new object();

        /// <summary>
        ///     MVC Model模型对象实例
        /// </summary>
        protected static volatile IModel m_instance;

        /// <summary>
        ///     线程锁
        /// </summary>
        protected readonly object m_syncRoot = new object();

        /// <summary>
        ///     模型字典
        /// </summary>
        protected IDictionary<string, IProxy> m_proxyMap;

        /// <summary>
        ///     创建模型对象
        /// </summary>
        protected Model()
        {
            m_proxyMap = new Dictionary<string, IProxy>();
            InitializeModel();
        }

        /// <summary>
        ///     MVC Model模型对象实例
        /// </summary>
        public static IModel Instance
        {
            get
            {
                if (m_instance == null)
                    lock (m_staticSyncRoot)
                    {
                        if (m_instance == null)
                            m_instance = new Model();
                    }
                return m_instance;
            }
        }

        /// <summary>
        ///     注册代理
        /// </summary>
        /// <param name="proxy">代理名称</param>
        public virtual void RegisterProxy( IProxy proxy )
        {
            lock (m_syncRoot)
            {
                m_proxyMap[proxy.ProxyName] = proxy;
            }
            proxy.OnRegister();
        }

        /// <summary>
        ///     获取对象代理名称的数据模型代理对象
        /// </summary>
        /// <param name="proxyName">指定的代理名称</param>
        /// <returns>代理对象</returns>
        public virtual IProxy RetrieveProxy( string proxyName )
        {
            lock (m_syncRoot)
            {
                if (!m_proxyMap.ContainsKey(proxyName))
                    return null;
                return m_proxyMap[proxyName];
            }
        }

        /// <summary>
        ///     是否存在指定名称的代理
        /// </summary>
        /// <param name="proxyName">指定的代理名称</param>
        /// <returns>bool</returns>
        public virtual bool HasProxy( string proxyName )
        {
            lock (m_syncRoot)
            {
                return m_proxyMap.ContainsKey(proxyName);
            }
        }

        /// <summary>
        ///     移除代理
        /// </summary>
        /// <param name="proxyName">指定的代理名称</param>
        /// <returns>代理对象</returns>
        public virtual IProxy RemoveProxy( string proxyName )
        {
            IProxy proxy = null;
            lock (m_syncRoot)
            {
                if (m_proxyMap.ContainsKey(proxyName))
                {
                    proxy = RetrieveProxy(proxyName);
                    m_proxyMap.Remove(proxyName);
                }
            }
            if (proxy != null)
                proxy.OnRemove();
            return proxy;
        }

        /// <summary>
        ///     初始化数据模型
        /// </summary>
        protected virtual void InitializeModel()
        {
        }

        /// <summary>
        ///     MVC M层代理调试信息
        /// </summary>
        /// <returns>调试信息</returns>
        public virtual string ToDebug()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-----------------proxys-----------------");
            lock (m_syncRoot)
            {
                sb.AppendLine(m_proxyMap.KeysToString());
            }
            sb.AppendLine("----------------- proxys end-----------------");
            return sb.ToString();
        }
    }
}