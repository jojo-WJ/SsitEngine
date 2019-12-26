/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：实体组                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/5 15:06:43                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using SsitEngine.Core.ObjectPool;

namespace SsitEngine.Unity.Entity
{
    public partial class EntityManager
    {
        /// <summary>
        ///     实体组。
        /// </summary>
        private sealed class EntityGroup : IEntityGroup
        {
            private readonly LinkedList<IEntity> m_Entities;
            private readonly IObjectPool<EntityInstanceObject> m_InstancePool;

            /// <summary>
            ///     初始化实体组的新实例。
            /// </summary>
            /// <param name="groupId">实体组Id。</param>
            /// <param name="instanceAutoReleaseInterval">实体实例对象池自动释放可释放对象的间隔秒数。</param>
            /// <param name="instanceCapacity">实体实例对象池容量。</param>
            /// <param name="instanceExpireTime">实体实例对象池对象过期秒数。</param>
            /// <param name="instancePriority">实体实例对象池的优先级。</param>
            /// <param name="entityGroupHelper">实体组辅助器。</param>
            /// <param name="objectPoolManager">对象池管理器。</param>
            public EntityGroup( int groupId, float instanceAutoReleaseInterval, int instanceCapacity,
                float instanceExpireTime, int instancePriority, IEntityGroupHelper entityGroupHelper,
                IObjectPoolManager objectPoolManager )
            {
                if (entityGroupHelper == null)
                {
                    throw new SsitEngineException("Entity group helper is invalid.");
                }

                GroupId = groupId;
                Helper = entityGroupHelper;
                m_InstancePool = objectPoolManager.CreateSingleSpawnObjectPool<EntityInstanceObject>(
                    TextUtils.Format("Entity Instance Pool ({0})", groupId), 2f, instanceCapacity, instanceExpireTime,
                    instancePriority);
                m_InstancePool.AutoReleaseInterval = instanceAutoReleaseInterval;
                m_Entities = new LinkedList<IEntity>();
            }

            /// <summary>
            ///     获取实体组Id。
            /// </summary>
            public int GroupId { get; }

            /// <summary>
            ///     获取实体组中实体数量。
            /// </summary>
            public int EntityCount => m_Entities.Count;

            /// <summary>
            ///     获取或设置实体组实例对象池自动释放可释放对象的间隔秒数。
            /// </summary>
            public float InstanceAutoReleaseInterval
            {
                get => m_InstancePool.AutoReleaseInterval;
                set => m_InstancePool.AutoReleaseInterval = value;
            }

            /// <summary>
            ///     获取或设置实体组实例对象池的容量。
            /// </summary>
            public int InstanceCapacity
            {
                get => m_InstancePool.Capacity;
                set => m_InstancePool.Capacity = value;
            }

            /// <summary>
            ///     获取或设置实体组实例对象池对象过期秒数。
            /// </summary>
            public float InstanceExpireTime
            {
                get => m_InstancePool.ExpireTime;
                set => m_InstancePool.ExpireTime = value;
            }

            /// <summary>
            ///     获取或设置实体组实例对象池的优先级。
            /// </summary>
            public int InstancePriority
            {
                get => m_InstancePool.Priority;
                set => m_InstancePool.Priority = value;
            }

            /// <summary>
            ///     获取实体组辅助器。
            /// </summary>
            public IEntityGroupHelper Helper { get; }

            public int Name => throw new NotImplementedException();

            /// <summary>
            ///     实体组中是否存在实体。
            /// </summary>
            /// <param name="entityId">实体序列编号。</param>
            /// <returns>实体组中是否存在实体。</returns>
            public bool HasEntity( int entityId )
            {
                foreach (var entity in m_Entities)
                {
                    if (entity.Id == entityId)
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            ///     实体组中是否存在实体。
            /// </summary>
            /// <param name="entityAssetName">实体资源名称。</param>
            /// <returns>实体组中是否存在实体。</returns>
            public bool HasEntity( string entityAssetName )
            {
                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new SsitEngineException("Entity asset name is invalid.");
                }

                foreach (var entity in m_Entities)
                {
                    if (entity.EntityAssetName == entityAssetName)
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            ///     从实体组中获取实体。
            /// </summary>
            /// <param name="entityId">实体序列编号。</param>
            /// <returns>要获取的实体。</returns>
            public IEntity GetEntity( int entityId )
            {
                foreach (var entity in m_Entities)
                {
                    if (entity.Id == entityId)
                    {
                        return entity;
                    }
                }

                return null;
            }

            /// <summary>
            ///     从实体组中获取实体。
            /// </summary>
            /// <param name="entityAssetName">实体资源名称。</param>
            /// <returns>要获取的实体。</returns>
            public IEntity GetEntity( string entityAssetName )
            {
                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new SsitEngineException("Entity asset name is invalid.");
                }

                foreach (var entity in m_Entities)
                {
                    if (entity.EntityAssetName == entityAssetName)
                    {
                        return entity;
                    }
                }

                return null;
            }

            /// <summary>
            ///     从实体组中获取实体。
            /// </summary>
            /// <param name="entityAssetName">实体资源名称。</param>
            /// <returns>要获取的实体。</returns>
            public IEntity[] GetEntities( string entityAssetName )
            {
                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new SsitEngineException("Entity asset name is invalid.");
                }

                var results = new List<IEntity>();
                foreach (var entity in m_Entities)
                {
                    if (entity.EntityAssetName == entityAssetName)
                    {
                        results.Add(entity);
                    }
                }

                return results.ToArray();
            }

            /// <summary>
            ///     从实体组中获取实体。
            /// </summary>
            /// <param name="entityAssetName">实体资源名称。</param>
            /// <param name="results">要获取的实体。</param>
            public void GetEntities( string entityAssetName, List<IEntity> results )
            {
                if (string.IsNullOrEmpty(entityAssetName))
                {
                    throw new SsitEngineException("Entity asset name is invalid.");
                }

                if (results == null)
                {
                    throw new SsitEngineException("Results is invalid.");
                }

                results.Clear();
                foreach (var entity in m_Entities)
                {
                    if (entity.EntityAssetName == entityAssetName)
                    {
                        results.Add(entity);
                    }
                }
            }

            /// <summary>
            ///     从实体组中获取所有实体。
            /// </summary>
            /// <returns>实体组中的所有实体。</returns>
            public IEntity[] GetAllEntities()
            {
                var results = new List<IEntity>();
                foreach (var entity in m_Entities)
                {
                    results.Add(entity);
                }

                return results.ToArray();
            }

            /// <summary>
            ///     从实体组中获取所有实体。
            /// </summary>
            /// <param name="results">实体组中的所有实体。</param>
            public void GetAllEntities( List<IEntity> results )
            {
                if (results == null)
                {
                    throw new SsitEngineException("Results is invalid.");
                }

                results.Clear();
                foreach (var entity in m_Entities)
                {
                    results.Add(entity);
                }
            }

            public void SetEntityInstanceLocked( object entityInstance, bool locked )
            {
                if (entityInstance == null)
                {
                    throw new SsitEngineException("Entity instance is invalid.");
                }

                m_InstancePool.SetLocked(entityInstance, locked);
            }

            public void SetEntityInstancePriority( object entityInstance, int priority )
            {
                if (entityInstance == null)
                {
                    throw new SsitEngineException("Entity instance is invalid.");
                }

                m_InstancePool.SetPriority(entityInstance, priority);
            }

            /// <summary>
            ///     实体组轮询。
            /// </summary>
            /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
            /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
            public void Update( float elapseSeconds, float realElapseSeconds )
            {
                var current = m_Entities.First;
                while (current != null)
                {
                    var next = current.Next;
                    current.Value.OnUpdate(elapseSeconds, realElapseSeconds);
                    current = next;
                }
            }

            /// <summary>
            ///     往实体组增加实体。
            /// </summary>
            /// <param name="entity">要增加的实体。</param>
            public void AddEntity( IEntity entity )
            {
                m_Entities.AddLast(entity);
            }

            /// <summary>
            ///     从实体组移除实体。
            /// </summary>
            /// <param name="entity">要移除的实体。</param>
            public void RemoveEntity( IEntity entity )
            {
                m_Entities.Remove(entity);
            }

            public void RegisterEntityInstanceObject( EntityInstanceObject obj, bool spawned )
            {
                m_InstancePool.Register(obj, spawned);
            }

            public EntityInstanceObject SpawnEntityInstanceObject( string name )
            {
                return m_InstancePool.Spawn();
            }

            public void UnspawnEntity( IEntity entity )
            {
                m_InstancePool.Unspawn(entity.Handle);
            }
        }
    }
}