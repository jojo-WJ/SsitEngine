/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：对象池。                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月8日                             
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using SsitEngine.DebugLog;

namespace SsitEngine.Core.ObjectPool
{
    /// <summary>
    ///     对象池。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    public sealed class ObjectPool<T> : ObjectPoolBase, IObjectPool<T> where T : ObjectBase
    {
        private readonly LinkedList<ObjectPoolWarp<T>> m_objects;
        private float m_autoReleaseTime;
        private int m_capacity;
        private float m_expireTime;

        /// <summary>
        ///     对象池对象注册对象加载回调
        /// </summary>
        private SsitFunction<T> m_loadFunction;

        private SsitFunction<bool> m_spawnCondition;

        #region Initialization & Destruction

        /// <summary>
        ///     初始化对象池的新实例。
        /// </summary>
        /// <param name="name">对象池名称。</param>
        /// <param name="allowMultiSpawn">是否允许对象被多次获取。</param>
        /// <param name="autoReleaseInterval">对象池自动释放可释放对象的间隔秒数。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <param name="loadFunc">对象池的对象加载回调。</param>
        /// <param name="spawnCondition">对象池的过滤附加条件。</param>
        public ObjectPool( string name, bool allowMultiSpawn, float autoReleaseInterval, int capacity, float expireTime,
            int priority, SsitFunction<T> loadFunc = null, SsitFunction<bool> spawnCondition = null ) : base(name)
        {
            m_objects = new LinkedList<ObjectPoolWarp<T>>();
            AllowMultiSpawn = allowMultiSpawn;
            AutoReleaseInterval = autoReleaseInterval;
            Capacity = capacity;
            ExpireTime = expireTime;
            Priority = priority;
            m_autoReleaseTime = 0f;

            m_loadFunction = loadFunc;
            m_spawnCondition = spawnCondition;
        }

        #endregion

        #region Property

        /// <summary>
        ///     获取对象池对象类型。
        /// </summary>
        public override Type ObjectType => typeof(T);

        /// <summary>
        ///     获取对象池中对象的数量。
        /// </summary>
        public override int Count => m_objects.Count;

        /// <summary>
        ///     获取对象池中能被释放的对象的数量。
        /// </summary>
        public override int CanReleaseCount => GetCanReleaseObjects().Count;

        /// <summary>
        ///     获取是否允许对象被多次获取。
        /// </summary>
        public override bool AllowMultiSpawn { get; }

        /// <summary>
        ///     获取或设置对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public override float AutoReleaseInterval { get; set; }

        /// <summary>
        ///     获取或设置对象池的容量。
        /// </summary>
        public override int Capacity
        {
            get => m_capacity;
            set
            {
                if (value < 0) throw new SsitEngineException("Capacity is invalid.");

                if (m_capacity == value) return;

                //SsitDebug.Debug("Object pool '{0}' capacity changed from '{1}' to '{2}'.", TextUtils.GetFullName<T>(Name), m_capacity.ToString(), value.ToString());
                m_capacity = value;
                Release();
            }
        }

        /// <summary>
        ///     获取或设置对象池对象过期秒数。
        /// </summary>
        public override float ExpireTime
        {
            get => m_expireTime;

            set
            {
                if (value < 0f) throw new SsitEngineException("ExpireTime is invalid.");

                if (ExpireTime == value) return;

                //SsitDebug.Debug("Object pool '{0}' expire time changed from '{1}' to '{2}'.", TextUtils.GetFullName<T>(Name), m_expireTime.ToString(), value.ToString());
                m_expireTime = value;
                Release();
            }
        }

        /// <summary>
        ///     获取或设置对象池的优先级。
        /// </summary>
        public override int Priority { get; set; }

        #endregion

        #region 获取对象

        /// <summary>
        ///     创建对象。
        /// </summary>
        /// <param name="obj">对象。</param>
        /// <param name="spawned">对象是否已被获取。</param>
        public bool Register( T obj, bool spawned )
        {
            if (obj == null) throw new SsitEngineException("Object is invalid.");

            SsitDebug.Debug(spawned ? "Object pool '{0}' create and spawned one." : "Object pool '{0}' create one.",
                TextUtils.GetFullName<T>(Name));
            obj.Initial();
            if (m_objects.Count == m_capacity)
            {
                SsitDebug.Debug("对象池已超出容量,注册失败");
                return false;
            }

            var poolWarp = new ObjectPoolWarp<T>(name, obj, spawned);
            m_objects.AddLast(poolWarp);
            Release();
            return true;
        }

        /// <summary>
        ///     检查对象。
        /// </summary>
        /// <returns>要检查的对象是否存在。</returns>
        public bool CanSpawn()
        {
            foreach (var obj in m_objects)
                if (AllowMultiSpawn || !obj.IsInUse)
                    return true;

            return false;
        }

        /// <summary>
        ///     获取对象。
        /// </summary>
        /// <returns>要获取的对象。</returns>
        public T Spawn()
        {
            foreach (var obj in m_objects)
            {
                if (obj.Name != name) continue;

                var attachCondition = m_spawnCondition == null ? true : m_spawnCondition();

                if (attachCondition && (AllowMultiSpawn || !obj.IsInUse))
                {
                    SsitDebug.Debug("Object pool '{0}' spawn one.", TextUtils.GetFullName<T>(Name));
                    return obj.Spawn();
                }
            }

            if (m_loadFunction == null) return null;

            if (m_objects.Count == m_capacity)
            {
                SsitDebug.Debug("对象池已超出容量");
                var ret = m_loadFunction();
                ret.Initial();
                return ret;
            }

            var warp = InternalRegister(false);
            return warp.Spawn();
        }

        /// <summary>
        ///     获取对象池未使用的对象
        /// </summary>
        /// <param name="condition">获取的条件</param>
        /// <returns></returns>
        public T Spawn( Predicate<T> condition )
        {
            foreach (var obj in m_objects)
            {
                if (obj.Name != name) continue;
                if (!obj.IsInUse && condition(obj.Data))
                {
                    SsitDebug.Debug("Object pool '{0}' spawn one.", TextUtils.GetFullName<T>(Name));
                    return obj.Spawn();
                }
            }

            return null;
        }

        #endregion

        #region 回收对象

        /// <summary>
        ///     回收对象。
        /// </summary>
        /// <param name="obj">要回收的内部对象。</param>
        public void Unspawn( T obj )
        {
            if (obj == null) throw new SsitEngineException("Object is invalid.");

            if (m_objects.First(x => { return x.Data == obj; }) == null)
            {
                SsitDebug.Debug("对象池容量超出容错处理");
                obj.Release(false);
                return;
            }
            Unspawn(obj.Target);
        }


        /// <summary>
        ///     回收对象。
        /// </summary>
        /// <param name="target">要回收的对象。</param>
        public void Unspawn( object target )
        {
            if (target == null) throw new SsitEngineException("Target is invalid.");

            foreach (var obj in m_objects)
                if (obj.Data.Target == target)
                {
                    obj.Unspawn();
                    Release();
                    return;
                }
            throw new SsitEngineException(TextUtils.Format("Can not find target in object pool '{0}'.",
                TextUtils.GetFullName<T>(Name)));
        }

        #endregion

        #region 释放对象

        /// <summary>
        ///     释放对象池中的可释放对象。
        /// </summary>
        public override void Release()
        {
            Release(m_objects.Count - m_capacity, DefaultReleaseObjectFilterCallback);
        }

        /// <summary>
        ///     释放对象池中的可释放对象。
        /// </summary>
        /// <param name="toReleaseCount">尝试释放对象数量。</param>
        public override void Release( int toReleaseCount )
        {
            Release(toReleaseCount, DefaultReleaseObjectFilterCallback);
        }

        /// <summary>
        ///     释放对象池中的可释放对象。
        /// </summary>
        /// <param name="releaseObjectFilterCallback">释放对象筛选函数。</param>
        public void Release( ReleaseObjectFilterCallback<T> releaseObjectFilterCallback )
        {
            Release(m_objects.Count - m_capacity, releaseObjectFilterCallback);
        }

        /// <summary>
        ///     释放对象池中的可释放对象。
        /// </summary>
        /// <param name="toReleaseCount">尝试释放对象数量。</param>
        /// <param name="releaseObjectFilterCallback">释放对象筛选函数。</param>
        public void Release( int toReleaseCount, ReleaseObjectFilterCallback<T> releaseObjectFilterCallback )
        {
            if (releaseObjectFilterCallback == null)
                throw new SsitEngineException("Release object filter callback is invalid.");

            m_autoReleaseTime = 0f;
            if (toReleaseCount < 0) toReleaseCount = 0;

            var expireTime = DateTime.MinValue;

            //correct timer
            if (m_expireTime < float.MaxValue) expireTime = DateTime.Now.AddSeconds(-m_expireTime);

            var canReleaseObjects = GetCanReleaseObjects();
            var toReleaseObjects = releaseObjectFilterCallback(canReleaseObjects, toReleaseCount, expireTime);
            if (toReleaseObjects == null || toReleaseObjects.Count <= 0) return;

            foreach (var toReleaseObject in toReleaseObjects)
            {
                if (toReleaseObject == null) throw new SsitEngineException("Can not release null object.");

                var found = false;

                foreach (var obj in m_objects)
                {
                    if (obj.Data != toReleaseObject) continue;

                    m_objects.Remove(obj);
                    obj.Release(false);
                    SsitDebug.Debug("Object pool '{0}' release one.", TextUtils.GetFullName<T>(Name));
                    found = true;
                    break;
                }

                if (!found) throw new SsitEngineException("Can not release object which is not found.");
            }
        }

        /// <summary>
        ///     释放对象池中的所有未使用对象。
        /// </summary>
        public override void ReleaseAllUnused()
        {
            var current = m_objects.First;
            while (current != null)
            {
                if (current.Value.IsInUse || current.Value.Locked || !current.Value.CustomCanReleaseFlag)
                {
                    current = current.Next;
                    continue;
                }

                var next = current.Next;
                m_objects.Remove(current);
                current.Value.Release(false);
                SsitDebug.Debug("Object pool '{0}' release '{1}'.", TextUtils.GetFullName<T>(Name), current.Value.Name);
                current = next;
            }
        }

        #endregion

        #region 对象附加操作（锁定、调级）

        /// <summary>
        ///     设置对象是否被加锁。
        /// </summary>
        /// <param name="obj">要设置被加锁的内部对象。</param>
        /// <param name="locked">是否被加锁。</param>
        public void SetLocked( T obj, bool locked )
        {
            if (obj == null) throw new SsitEngineException("Object is invalid.");

            SetLocked(obj.Target, locked);
        }

        /// <summary>
        ///     设置对象是否被加锁。
        /// </summary>
        /// <param name="target">要设置被加锁的对象。</param>
        /// <param name="locked">是否被加锁。</param>
        public void SetLocked( object target, bool locked )
        {
            if (target == null) throw new SsitEngineException("Target is invalid.");

            foreach (var obj in m_objects)
                if (obj.Data.Target == target)
                {
                    obj.Locked = locked;
                    return;
                }

            throw new SsitEngineException(TextUtils.Format("Can not find target in object pool '{0}'.",
                TextUtils.GetFullName<T>(Name)));
        }

        /// <summary>
        ///     设置对象的优先级。
        /// </summary>
        /// <param name="obj">要设置优先级的内部对象。</param>
        /// <param name="priority">优先级。</param>
        public void SetPriority( T obj, int priority )
        {
            if (obj == null) throw new SsitEngineException("Object is invalid.");

            SetPriority(obj.Target, priority);
        }

        /// <summary>
        ///     设置对象的优先级。
        /// </summary>
        /// <param name="target">要设置优先级的对象。</param>
        /// <param name="priority">优先级。</param>
        public void SetPriority( object target, int priority )
        {
            if (target == null) throw new SsitEngineException("Target is invalid.");

            foreach (var obj in m_objects)
                if (obj.Data.Target == target)
                {
                    obj.Priority = priority;
                    return;
                }

            throw new SsitEngineException(TextUtils.Format("Can not find target in object pool '{0}'.",
                TextUtils.GetFullName<T>(Name)));
        }

        #endregion

        #region 回调处理

        /// <summary>
        ///     设置对象池的加载回调
        /// </summary>
        /// <param name="func"></param>
        public void SetLoadFunction( SsitFunction<T> func )
        {
            m_loadFunction = func;
        }

        /// <summary>
        ///     设置对象池获取的过滤附加条件
        /// </summary>
        /// <param name="condition">附加条件</param>
        public void SetSpawnCondition( SsitFunction<bool> condition )
        {
            m_spawnCondition = condition;
        }

        #endregion

        #region Moudle

        /// <summary>
        ///     对象池管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        public override void OnUpdate( float elapseSeconds )
        {
            m_autoReleaseTime += elapseSeconds;
            if (m_autoReleaseTime < AutoReleaseInterval) return;

            //SsitDebug.Debug("Object pool '{0}' auto release start.", TextUtils.GetFullName<T>(Name));
            Release();
            //SsitDebug.Debug("Object pool '{0}' auto release complete.", TextUtils.GetFullName<T>(Name));
        }

        /// <summary>
        ///     对象池关闭并销毁
        /// </summary>
        public override void Shutdown()
        {
            var current = m_objects.First;
            while (current != null)
            {
                var next = current.Next;
                m_objects.Remove(current);
                current.Value.Release(true);
                current = next;
            }

            m_loadFunction = null;
            m_spawnCondition = null;
        }

        #endregion

        #region Internal Members

        /// <summary>
        ///     获取对象池可以释放的对象
        /// </summary>
        /// <returns></returns>
        private LinkedList<T> GetCanReleaseObjects()
        {
            var canReleaseObjects = new LinkedList<T>();

            foreach (var obj in m_objects)
            {
                if (obj.IsInUse || obj.Locked || !obj.CustomCanReleaseFlag) continue;

                canReleaseObjects.AddLast(obj.Data);
            }

            return canReleaseObjects;
        }

        /// <summary>
        ///     默认释放对象过滤回调
        /// </summary>
        /// <param name="candidateObjects">候选对象列表</param>
        /// <param name="toReleaseCount">释放个数</param>
        /// <param name="expireTime">过期时间</param>
        /// <returns></returns>
        private LinkedList<T> DefaultReleaseObjectFilterCallback( LinkedList<T> candidateObjects, int toReleaseCount,
            DateTime expireTime )
        {
            var toReleaseObjects = new LinkedList<T>();

            if (expireTime > DateTime.MinValue)
            {
                var current = candidateObjects.First;
                while (current != null)
                {
                    //filter expire object
                    if (current.Value.LastUseTime <= expireTime)
                    {
                        toReleaseObjects.AddLast(current.Value);
                        var next = current.Next;
                        candidateObjects.Remove(current);
                        current = next;
                        continue;
                    }

                    current = current.Next;
                }

                toReleaseCount -= toReleaseObjects.Count;
            }
            //sort and filter lower priority or more last user time
            for (var i = candidateObjects.First; toReleaseCount > 0 && i != null; i = i.Next)
            {
                for (var j = i.Next; j != null; j = j.Next)
                    if (i.Value.Priority > j.Value.Priority || i.Value.Priority == j.Value.Priority &&
                        i.Value.LastUseTime > j.Value.LastUseTime)
                    {
                        var temp = i.Value;
                        i.Value = j.Value;
                        j.Value = temp;
                    }

                toReleaseObjects.AddLast(i.Value);
                toReleaseCount--;
            }

            return toReleaseObjects;
        }

        /// <summary>
        ///     内部创建对象。
        /// </summary>
        /// <param name="spawned">对象是否已被获取。</param>
        /// <remarks>不建议使用</remarks>
        private ObjectPoolWarp<T> InternalRegister( bool spawned )
        {
            T target = null;
            if (m_loadFunction == null)
                throw new SsitEngineException("the loadFunction of the auto create pool object is null");
            target = m_loadFunction();
            target.Initial();

            var poolWarp = new ObjectPoolWarp<T>(name, target, spawned);
            m_objects.AddLast(poolWarp);

            Release();
            return poolWarp;
        }

        /// <summary>
        ///     获取所有对象信息。
        /// </summary>
        /// <returns>所有对象信息。</returns>
        [Obsolete]
        public override ObjectInfo[] GetAllObjectInfos()
        {
            var index = 0;
            var results = new ObjectInfo[m_objects.Count];
            foreach (var obj in m_objects)
                results[index++] = new ObjectInfo(name, obj.Locked, obj.Priority, obj.LastUseTime, obj.SpawnCount);
            return results;
        }

        #endregion
    }
}