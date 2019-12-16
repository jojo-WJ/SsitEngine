/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：对象池基类                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/8 13:36:27                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace SsitEngine.Core.ObjectPool
{
    /// <summary>
    ///     对象池对象基类
    /// </summary>
    public abstract class ObjectBase : AllocatedObject
    {
        /// <summary>
        ///     对象名称
        /// </summary>
        private readonly string m_name;

        /// <summary>
        ///     使用时间节点
        /// </summary>
        protected DateTime m_lastUseTime;

        /// <summary>
        ///     对象锁
        /// </summary>
        protected bool m_locked;

        /// <summary>
        ///     对象优先级
        /// </summary>
        protected int m_priority;

        /// <summary>
        ///     对象目标
        /// </summary>
        protected object m_target;

        /// <summary>
        ///     初始化对象的新实例。
        /// </summary>
        /// <param name="target">对象。</param>
        public ObjectBase( object target ) : this(null, target, false, 0)
        {
        }

        /// <summary>
        ///     初始化对象的新实例。
        /// </summary>
        /// <param name="name">对象名称。</param>
        /// <param name="target">对象。</param>
        public ObjectBase( string name, object target )
            : this(name, target, false, 0)
        {
        }

        /// <summary>
        ///     初始化对象的新实例。
        /// </summary>
        /// <param name="name">对象名称。</param>
        /// <param name="target">对象。</param>
        /// <param name="locked">对象是否被加锁。</param>
        /// <param name="priority">对象的优先级。</param>
        public ObjectBase( string name, object target, bool locked, int priority )
        {
            if (target == null) throw new SsitEngineException(TextUtils.Format("Target '{0}' is invalid.", name));

            m_name = name ?? string.Empty;
            m_target = target;
            m_locked = locked;
            m_priority = priority;
            m_lastUseTime = DateTime.Now;
        }

        /// <summary>
        ///     获取对象。
        /// </summary>
        public object Target => m_target;

        /// <summary>
        ///     获取或设置对象是否被加锁。
        /// </summary>
        public bool Locked
        {
            get => m_locked;
            set => m_locked = value;
        }

        /// <summary>
        ///     获取或设置对象的优先级。
        /// </summary>
        public int Priority
        {
            get => m_priority;
            set => m_priority = value;
        }

        /// <summary>
        ///     获取自定义释放检查标记。
        /// </summary>
        public virtual bool CustomCanReleaseFlag => true;

        /// <summary>
        ///     获取对象上次使用时间。
        /// </summary>
        public DateTime LastUseTime
        {
            get => m_lastUseTime;
            internal set => m_lastUseTime = value;
        }

        #region Members Method

        /// <summary>
        ///     对象初始化
        /// </summary>
        protected internal virtual void Initial()
        {
        }

        /// <summary>
        ///     获取对象时的事件。
        /// </summary>
        protected internal virtual void OnSpawn()
        {
        }

        /// <summary>
        ///     回收对象时的事件。
        /// </summary>
        protected internal virtual void OnUnspawn()
        {
        }

        /// <summary>
        ///     释放对象。
        /// </summary>
        /// <param name="isShutdown">是否是关闭对象池时触发。</param>
        protected internal abstract void Release( bool isShutdown );

        /// <summary>
        ///     对象轮询
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        protected internal virtual void OnUpdate( float elapseSeconds, float realElapseSeconds )
        {
        }

        /// <summary>
        ///     关闭并清理对象池对象。
        /// </summary>
        public override void Shutdown()
        {
            base.Shutdown();
        }

        #endregion
    }
}