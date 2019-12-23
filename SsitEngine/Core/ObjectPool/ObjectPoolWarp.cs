/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：对象池持有对象得基类                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/8 10:52:07                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace SsitEngine.Core.ObjectPool
{
    /// <summary>
    ///     对象池对象包装器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPoolWarp<T> : AllocatedObject<T> where T : ObjectBase
    {
        /// <summary>
        ///     对象池的名称
        /// </summary>
        private readonly string m_name;

        /// <summary>
        ///     对象上次使用时间
        /// </summary>
        private DateTime m_lastUseTime;

        /// <summary>
        ///     锁定标识
        /// </summary>
        private bool m_locked;

        /// <summary>
        ///     优先级
        /// </summary>
        private int m_priority;

        /// <summary>
        ///     实例获取次数
        /// </summary>
        private int m_spawnCount;

        /*/// <summary>
        /// 是否正在使用
        /// </summary>
        private bool m_isUsing;*/

        /// <summary>
        ///     初始化对象的新实例。
        /// </summary>
        /// <param name="name">对象池名称。</param>
        /// <param name="target">对象。</param>
        public ObjectPoolWarp( string name, T target ) : this(name, target, false, 0)
        {
        }

        /// <summary>
        ///     初始化对象的新实例。
        /// </summary>
        /// <param name="name">对象池名称。</param>
        /// <param name="target">对象。</param>
        /// <param name="locked">对象是否被加锁。</param>
        public ObjectPoolWarp( string name, T target, bool locked ) : this(name, target, locked, 0)
        {
        }

        /// <summary>
        ///     初始化对象的新实例。
        /// </summary>
        /// <param name="name">对象池名称。</param>
        /// <param name="target">对象。</param>
        /// <param name="priority">对象的优先级。</param>
        public ObjectPoolWarp( string name, T target, int priority ) : this(name, target, false, priority)
        {
        }

        /// <summary>
        ///     初始化对象的新实例。
        /// </summary>
        /// <param name="name">对象池名称。</param>
        /// <param name="target">对象。</param>
        /// <param name="locked">对象是否被加锁。</param>
        /// <param name="priority">对象的优先级。</param>
        public ObjectPoolWarp( string name, T target, bool locked, int priority ) : base(target)
        {
            m_name = name;
            m_locked = locked;
            m_priority = priority;
            m_lastUseTime = DateTime.Now;
            //m_isUsing = false;
        }

        /// <summary>
        ///     获取对象池名称。
        /// </summary>
        public string Name => m_name;

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

        /// <summary>
        ///     获取对象是否正在使用。
        /// </summary>
        public bool IsInUse => m_spawnCount > 0;

        /// <summary>
        ///     获取对象的获取计数。
        /// </summary>
        public int SpawnCount => m_spawnCount;

        /// <summary>
        ///     获取对象时的事件。
        /// </summary>
        public virtual T Spawn()
        {
            m_spawnCount++;
            data.LastUseTime = DateTime.Now;
            data.OnSpawn();
            return data;
        }

        /// <summary>
        ///     回收对象时的事件。
        /// </summary>
        public virtual void Unspawn()
        {
            m_spawnCount--;
            data.LastUseTime = DateTime.Now;
            data.OnUnspawn();
            if (m_spawnCount < 0)
            {
                throw new SsitEngineException("Spawn count is less than 0.");
            }
        }

        /// <summary>
        ///     释放对象。
        /// </summary>
        /// <param name="isShutdown">是否是关闭对象池时触发。</param>
        public void Release( bool isShutdown )
        {
            if (data != null)
            {
                Data.Release(isShutdown);
            }
            if (isShutdown)
            {
                Shutdown();
            }
        }

        /// <summary>
        ///     对象轮询
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public virtual void OnUpdate( float elapseSeconds, float realElapseSeconds )
        {
            if (data != null)
            {
                Data.OnUpdate(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        ///     销毁对象。
        /// </summary>
        public override void Shutdown()
        {
            if (data != null)
            {
                Data.Shutdown();
            }
        }
    }
}