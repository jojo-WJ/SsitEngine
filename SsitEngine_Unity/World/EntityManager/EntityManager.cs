/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：管理程序中所有实体的容器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/5 15:01:34                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using SsitEngine.Core.ObjectPool;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.Unity.Resource;
using SsitEngine.Unity.Timer;

namespace SsitEngine.Unity.Entity
{
    /// <summary>
    ///     实体对象管理器
    /// </summary>
    public partial class EntityManager : ManagerBase<EntityManager>
    {
        /// <summary>
        ///     实体加载列表
        /// </summary>
        private readonly Dictionary<int, int> m_EntitiesBeingLoaded;

        /// <summary>
        ///     释放实体
        /// </summary>
        private readonly HashSet<int> m_EntitiesToReleaseOnLoad;

        /// <summary>
        ///     实体组信息
        /// </summary>
        private readonly Dictionary<int, EntityGroup> m_EntityGroups;

        /// <summary>
        ///     实体信息
        /// </summary>
        private readonly Dictionary<int, EntityInfo> m_EntityInfos;

        private readonly LinkedList<EntityInfo> m_RecycleQueue;

        private IEntityHelper m_EntityHelper;

        //private readonly LoadAssetCallbacks m_LoadAssetCallbacks;
        private IObjectPoolManager m_ObjectPoolManager;
        private IResourceManager m_ResourceManager;
        private int m_Serial;

        /// <summary>
        ///     初始化实体管理器的新实例。
        /// </summary>
        public EntityManager()
        {
            m_EntityInfos = new Dictionary<int, EntityInfo>();
            m_EntityGroups = new Dictionary<int, EntityGroup>();
            m_EntitiesBeingLoaded = new Dictionary<int, int>();
            m_EntitiesToReleaseOnLoad = new HashSet<int>();
            m_RecycleQueue = new LinkedList<EntityInfo>();
            //m_LoadAssetCallbacks = new LoadAssetCallbacks(LoadEntitySuccessCallback, LoadEntityFailureCallback, LoadEntityUpdateCallback, LoadEntityDependencyAssetCallback);
            m_ObjectPoolManager = null;
            m_ResourceManager = null;
            m_EntityHelper = null;
            m_Serial = 0;
        }

        #region 实体事件处理

        /// <summary>
        ///     实体管理器的事件处理
        /// </summary>
        /// <param name="notification"></param>
        public override void HandleNotification( INotification notification )
        {
            //todo:事件处理机制
        }

        #endregion

        #region Property

        /// <summary>
        ///     获取实体数量。
        /// </summary>
        public int EntityCount => m_EntityInfos.Count;

        /// <summary>
        ///     获取实体组数量。
        /// </summary>
        public int EntityGroupCount => m_EntityGroups.Count;

        #endregion

        #region 模块接口实现

        /// <inheritdoc />
        public override string ModuleName => typeof(TimerManager).FullName;

        /// <summary>
        ///     计时器模块优先级
        /// </summary>
        public override int Priority => (int) EnModuleType.ENMODULEENTITY;

        /// <summary>
        ///     实体管理器的轮询
        /// </summary>
        /// <param name="elapsed">逻辑流逝时间</param>
        public override void OnUpdate( float elapsed )
        {
            while (m_RecycleQueue.Count > 0)
            {
                var entityInfo = m_RecycleQueue.First.Value;
                m_RecycleQueue.RemoveFirst();
                var entity = entityInfo.Entity;
                var entityGroup = (EntityGroup) entity.EntityGroup;
                if (entityGroup == null) throw new SsitEngineException("Entity group is invalid.");

                entityInfo.Status = EntityStatus.WillRecycle;
                entity.OnRecycle();
                entityInfo.Status = EntityStatus.Recycled;
                entityGroup.UnspawnEntity(entity);
            }

            foreach (var entityGroup in m_EntityGroups) entityGroup.Value.Update(elapsed, GameTime.Realtime);
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            HideAllLoadedEntities();
            m_EntityGroups.Clear();
            m_EntitiesBeingLoaded.Clear();
            m_EntitiesToReleaseOnLoad.Clear();
            m_RecycleQueue.Clear();
        }

        /// <summary>
        ///     隐藏所有已加载的实体。
        /// </summary>
        public void HideAllLoadedEntities()
        {
            HideAllLoadedEntities(null);
        }

        /// <summary>
        ///     隐藏所有已加载的实体。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void HideAllLoadedEntities( object userData )
        {
            while (m_EntityInfos.Count > 0)
                foreach (var entityInfo in m_EntityInfos)
                {
                    InternalHideEntity(entityInfo.Value, userData);
                    break;
                }
        }

        #endregion


        #region 实体组处理

        /// <summary>
        ///     增加实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组Id(最好是枚举)。</param>
        /// <param name="instanceAutoReleaseInterval">实体实例对象池自动释放可释放对象的间隔秒数。</param>
        /// <param name="instanceCapacity">实体实例对象池容量。</param>
        /// <param name="instanceExpireTime">实体实例对象池对象过期秒数。</param>
        /// <param name="instancePriority">实体实例对象池的优先级。</param>
        /// <param name="entityGroupHelper">实体组辅助器。</param>
        /// <returns>是否增加实体组成功。</returns>
        public bool AddEntityGroup( int entityGroupName, float instanceAutoReleaseInterval, int instanceCapacity,
            float instanceExpireTime, int instancePriority, IEntityGroupHelper entityGroupHelper )
        {
            if (entityGroupHelper == null) throw new SsitEngineException("Entity group helper is invalid.");

            if (m_ObjectPoolManager == null) throw new SsitEngineException("You must set object pool manager first.");

            if (HasEntityGroup(entityGroupName)) return false;

            m_EntityGroups.Add(entityGroupName,
                new EntityGroup(entityGroupName, instanceAutoReleaseInterval, instanceCapacity, instanceExpireTime,
                    instancePriority, entityGroupHelper, m_ObjectPoolManager));

            return true;
        }

        /// <summary>
        ///     设置对象池管理器。
        /// </summary>
        /// <param name="objectPoolManager">对象池管理器。</param>
        public void SetObjectPoolManager( IObjectPoolManager objectPoolManager )
        {
            if (objectPoolManager == null) throw new SsitEngineException("Object pool manager is invalid.");

            m_ObjectPoolManager = objectPoolManager;
        }

        /// <summary>
        ///     设置资源管理器。
        /// </summary>
        /// <param name="resourceManager">资源管理器。</param>
        public void SetResourceManager( IResourceManager resourceManager )
        {
            if (resourceManager == null) throw new SsitEngineException("Resource manager is invalid.");

            m_ResourceManager = resourceManager;
        }

        /// <summary>
        ///     设置实体辅助器。
        /// </summary>
        /// <param name="entityHelper">实体辅助器。</param>
        public void SetEntityHelper( IEntityHelper entityHelper )
        {
            if (entityHelper == null) throw new SsitEngineException("Entity helper is invalid.");

            m_EntityHelper = entityHelper;
        }

        /// <summary>
        ///     是否存在实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <returns>是否存在实体组。</returns>
        public bool HasEntityGroup( int entityGroupName )
        {
            return m_EntityGroups.ContainsKey(entityGroupName);
        }

        #endregion

        #region 实体加载

        /// <summary>
        ///     获取所有正在加载实体的编号。
        /// </summary>
        /// <returns>所有正在加载实体的编号。</returns>
        public int[] GetAllLoadingEntityIds()
        {
            var index = 0;
            var results = new int[m_EntitiesBeingLoaded.Count];
            foreach (var entityBeingLoaded in m_EntitiesBeingLoaded) results[index++] = entityBeingLoaded.Key;

            return results;
        }

        /// <summary>
        ///     获取所有正在加载实体的编号。
        /// </summary>
        /// <param name="results">所有正在加载实体的编号。</param>
        public void GetAllLoadingEntityIds( List<int> results )
        {
            if (results == null) throw new SsitEngineException("Results is invalid.");

            results.Clear();
            foreach (var entityBeingLoaded in m_EntitiesBeingLoaded) results.Add(entityBeingLoaded.Key);
        }

        /// <summary>
        ///     是否正在加载实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>是否正在加载实体。</returns>
        public bool IsLoadingEntity( int entityId )
        {
            return m_EntitiesBeingLoaded.ContainsKey(entityId);
        }

        /// <summary>
        ///     是否是合法的实体。
        /// </summary>
        /// <param name="entity">实体。</param>
        /// <returns>实体是否合法。</returns>
        public bool IsValidEntity( IEntity entity )
        {
            if (entity == null) return false;

            return HasEntity(entity.Id);
        }

        /// <summary>
        ///     是否存在实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>是否存在实体。</returns>
        public bool HasEntity( int entityId )
        {
            return m_EntityInfos.ContainsKey(entityId);
        }

        /// <summary>
        ///     是否存在实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>是否存在实体。</returns>
        public bool HasEntity( string entityAssetName )
        {
            if (string.IsNullOrEmpty(entityAssetName)) throw new SsitEngineException("Entity asset name is invalid.");

            foreach (var entityInfo in m_EntityInfos)
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                    return true;

            return false;
        }

        #endregion

        #region Internal Members

        private void InternalShowEntity( int entityId, string entityAssetName, EntityGroup entityGroup,
            object entityInstance, bool isNewInstance, float duration, object userData )
        {
            var entity = m_EntityHelper.CreateEntity(entityInstance, entityGroup, userData);
            if (entity == null) throw new SsitEngineException("Can not create entity in helper.");

            var entityInfo = new EntityInfo(entity);
            m_EntityInfos.Add(entityId, entityInfo);
            entityInfo.Status = EntityStatus.WillInit;
            entity.OnInit(entityId, entityAssetName, entityGroup, isNewInstance, userData);
            entityInfo.Status = EntityStatus.Inited;
            entityGroup.AddEntity(entity);
            entityInfo.Status = EntityStatus.WillShow;
            entity.OnShow(userData);
            entityInfo.Status = EntityStatus.Showed;

            /*if (m_ShowEntitySuccessEventHandler != null)
                {
                    m_ShowEntitySuccessEventHandler(this, new ShowEntitySuccessEventArgs(entity, duration, userData));
                }*/
        }

        private void InternalHideEntity( EntityInfo entityInfo, object userData )
        {
            var entity = entityInfo.Entity;
            var childEntities = entityInfo.GetChildEntities();
            foreach (var childEntity in childEntities)
            {
                //HideEntity(childEntity.Id, userData);
            }

            //DetachEntity(entity.Id, userData);
            entityInfo.Status = EntityStatus.WillHide;
            entity.OnHide(userData);
            entityInfo.Status = EntityStatus.Hidden;

            var entityGroup = (EntityGroup) entity.EntityGroup;
            if (entityGroup == null) throw new SsitEngineException("Entity group is invalid.");

            entityGroup.RemoveEntity(entity);
            if (!m_EntityInfos.Remove(entity.Id)) throw new SsitEngineException("Entity info is unmanaged.");

            /*
            if (m_HideEntityCompleteEventHandler != null)
            {
                m_HideEntityCompleteEventHandler(this, new HideEntityCompleteEventArgs(entity.Id, entity.EntityAssetName, entityGroup, userData));
            }
            */

            m_RecycleQueue.AddLast(entityInfo);
        }

        /*private void LoadEntitySuccessCallback( string entityAssetName, object entityAsset, float duration, object userData )
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            if (showEntityInfo == null)
            {
                throw new GameFrameworkException("Show entity info is invalid.");
            }

            m_EntitiesBeingLoaded.Remove(showEntityInfo.EntityId);
            if (m_EntitiesToReleaseOnLoad.Contains(showEntityInfo.SerialId))
            {
                GameFrameworkLog.Debug("Release entity '{0}' (serial id '{1}') on loading success.", showEntityInfo.EntityId.ToString(), showEntityInfo.SerialId.ToString());
                m_EntitiesToReleaseOnLoad.Remove(showEntityInfo.SerialId);
                m_EntityHelper.ReleaseEntity(entityAsset, null);
                return;
            }

            EntityInstanceObject entityInstanceObject = new EntityInstanceObject(entityAssetName, entityAsset, m_EntityHelper.InstantiateEntity(entityAsset), m_EntityHelper);
            showEntityInfo.EntityGroup.RegisterEntityInstanceObject(entityInstanceObject, true);

            InternalShowEntity(showEntityInfo.EntityId, entityAssetName, showEntityInfo.EntityGroup, entityInstanceObject.Target, true, duration, showEntityInfo.UserData);
        }

        private void LoadEntityFailureCallback( string entityAssetName, LoadResourceStatus status, string errorMessage, object userData )
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            if (showEntityInfo == null)
            {
                throw new GameFrameworkException("Show entity info is invalid.");
            }

            m_EntitiesBeingLoaded.Remove(showEntityInfo.EntityId);
            m_EntitiesToReleaseOnLoad.Remove(showEntityInfo.SerialId);
            string appendErrorMessage = Utility.Text.Format("Load entity failure, asset name '{0}', status '{1}', error message '{2}'.", entityAssetName, status.ToString(), errorMessage);
            if (m_ShowEntityFailureEventHandler != null)
            {
                m_ShowEntityFailureEventHandler(this, new ShowEntityFailureEventArgs(showEntityInfo.EntityId, entityAssetName, showEntityInfo.EntityGroup.Name, appendErrorMessage, showEntityInfo.UserData));
                return;
            }

            throw new GameFrameworkException(appendErrorMessage);
        }

        private void LoadEntityUpdateCallback( string entityAssetName, float progress, object userData )
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            if (showEntityInfo == null)
            {
                throw new GameFrameworkException("Show entity info is invalid.");
            }

            if (m_ShowEntityUpdateEventHandler != null)
            {
                m_ShowEntityUpdateEventHandler(this, new ShowEntityUpdateEventArgs(showEntityInfo.EntityId, entityAssetName, showEntityInfo.EntityGroup.Name, progress, showEntityInfo.UserData));
            }
        }

        private void LoadEntityDependencyAssetCallback( string entityAssetName, string dependencyAssetName, int loadedCount, int totalCount, object userData )
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            if (showEntityInfo == null)
            {
                throw new GameFrameworkException("Show entity info is invalid.");
            }

            if (m_ShowEntityDependencyAssetEventHandler != null)
            {
                m_ShowEntityDependencyAssetEventHandler(this, new ShowEntityDependencyAssetEventArgs(showEntityInfo.EntityId, entityAssetName, showEntityInfo.EntityGroup.Name, dependencyAssetName, loadedCount, totalCount, showEntityInfo.UserData));
            }
        }*/

        #endregion
    }
}