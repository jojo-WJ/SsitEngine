/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：对象池管理器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/8 15:12:56                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using SsitEngine.Core;
using SsitEngine.Core.ObjectPool;

namespace SsitEngine.Unity.ObjectPool
{
    /// <summary>
    ///     对象池管理器
    /// </summary>
    public class ObjectPoolManager : ManagerBase<ObjectPoolManager>, IObjectPoolManager
    {
        public const int DefaultCapacity = int.MaxValue;
        public const float DefaultExpireTime = float.MaxValue;
        public const int DefaultPriority = 0;

        /// <summary>
        ///     对象池字典
        /// </summary>
        private readonly Dictionary<string, ObjectPoolBase> m_ObjectPools;

        /// <summary>
        ///     初始化对象池管理器的新实例。
        /// </summary>
        public ObjectPoolManager()
        {
            m_ObjectPools = new Dictionary<string, ObjectPoolBase>();
        }

        /// <summary>
        ///     获取对象池数量。
        /// </summary>
        public int Count => m_ObjectPools.Count;

        #region Moudle

        /// <summary>
        ///     获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public override int Priority => (int) EnModuleType.ENMODULEOBJECTPOOL;


        /// <summary>
        ///     对象池管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        public override void OnUpdate( float elapseSeconds )
        {
            foreach (var objectPool in m_ObjectPools)
            {
                objectPool.Value.OnUpdate(elapseSeconds);
            }
        }

        /// <summary>
        ///     关闭并清理对象池管理器。
        /// </summary>
        public override void Shutdown()
        {
            foreach (var objectPool in m_ObjectPools)
            {
                objectPool.Value.Shutdown();
            }

            m_ObjectPools.Clear();
        }

        #endregion

        #region 对象池创建

        /// <inheritdoc />
        public IObjectPool<T> CreateSingleSpawnObjectPool<T>( string name, float autoReleaseInterval,
            int capacity = DefaultCapacity, float expireTime = DefaultExpireTime, int priority = DefaultPriority,
            SsitFunction<T> loadFunction = null, SsitFunction<bool> spawncondition = null ) where T : ObjectBase
        {
            return InternalCreateObjectPool<T>(name, false, autoReleaseInterval, capacity, expireTime, priority);
        }

        /// <inheritdoc />
        public IObjectPool<T> CreateMultiSpawnObjectPool<T>( string name, float autoReleaseInterval,
            int capacity = DefaultCapacity, float expireTime = DefaultExpireTime, int priority = DefaultPriority,
            SsitFunction<T> loadFunction = null, SsitFunction<bool> spawncondition = null ) where T : ObjectBase
        {
            return InternalCreateObjectPool<T>(name, true, autoReleaseInterval, capacity, expireTime, priority);
        }

        #endregion

        #region 对象池查询

        /// <inheritdoc />
        public bool HasObjectPool<T>( string name ) where T : ObjectBase
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new SsitEngineException("Full name is invalid.");
            }

            return m_ObjectPools.ContainsKey(name);
        }

        /// <inheritdoc />
        public IObjectPool<T> GetObjectPool<T>( string name ) where T : ObjectBase
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new SsitEngineException("Full name is invalid.");
            }

            ObjectPoolBase objectPool = null;
            if (m_ObjectPools.TryGetValue(name, out objectPool))
            {
                return objectPool as IObjectPool<T>;
            }

            return null;
        }

        /// <inheritdoc />
        public ObjectPoolBase[] GetAllObjectPools( bool sort )
        {
            if (sort)
            {
                var results = new List<ObjectPoolBase>();
                foreach (var objectPool in m_ObjectPools)
                {
                    results.Add(objectPool.Value);
                }

                results.Sort(ObjectPoolComparer);
                return results.ToArray();
            }
            else
            {
                var index = 0;
                var results = new ObjectPoolBase[m_ObjectPools.Count];
                foreach (var objectPool in m_ObjectPools)
                {
                    results[index++] = objectPool.Value;
                }

                return results;
            }
        }

        #endregion

        #region 对象池释放

        /// <summary>
        ///     释放所有对象池所有对象
        /// </summary>
        public void Release()
        {
            var objectPools = GetAllObjectPools(true);
            foreach (var objectPool in objectPools)
            {
                objectPool.Release();
            }
        }

        /// <summary>
        ///     释放对象池所有未使用的对象
        /// </summary>
        public void ReleaseAllUnused()
        {
            var objectPools = GetAllObjectPools(true);
            foreach (var objectPool in objectPools)
            {
                objectPool.ReleaseAllUnused();
            }
        }

        #endregion

        #region 销毁对象池

        /// <inheritdoc />
        public bool DestroyObjectPool<T>( string name ) where T : ObjectBase
        {
            return InternalDestroyObjectPool(TextUtils.GetFullName<T>(name));
        }

        /// <inheritdoc />
        public bool DestroyObjectPool<T>( IObjectPool<T> objectPool ) where T : ObjectBase
        {
            if (objectPool == null)
            {
                throw new SsitEngineException("Object pool is invalid.");
            }

            return InternalDestroyObjectPool(TextUtils.GetFullName<T>(objectPool.Name));
        }

        #endregion

        #region Internal Members

        private IObjectPool<T> InternalCreateObjectPool<T>( string name, bool allowMultiSpawn,
            float autoReleaseInterval, int capacity, float expireTime, int priority,
            SsitFunction<T> loadFunction = null, SsitFunction<bool> spawncondition = null ) where T : ObjectBase
        {
            if (HasObjectPool<T>(name))
            {
                throw new SsitEngineException(TextUtils.Format("Already exist object pool '{0}'.",
                    TextUtils.GetFullName<T>(name)));
            }

            var objectPool = new ObjectPool<T>(name, allowMultiSpawn, autoReleaseInterval, capacity, expireTime,
                priority, loadFunction, spawncondition);
            m_ObjectPools.Add(TextUtils.GetFullName<T>(name), objectPool);
            return objectPool;
        }

        private bool InternalDestroyObjectPool( string fullName )
        {
            ObjectPoolBase objectPool = null;
            if (m_ObjectPools.TryGetValue(fullName, out objectPool))
            {
                objectPool.Shutdown();
                return m_ObjectPools.Remove(fullName);
            }

            return false;
        }

        private int ObjectPoolComparer( ObjectPoolBase a, ObjectPoolBase b )
        {
            return a.Priority.CompareTo(b.Priority);
        }

        #endregion
    }
}