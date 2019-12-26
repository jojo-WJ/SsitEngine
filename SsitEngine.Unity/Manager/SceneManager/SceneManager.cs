/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：场景管理器                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月11日                             
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using SsitEngine.Unity.Resource;

namespace SsitEngine.Unity.Scene
{
    /// <summary>
    ///     场景管理器。
    /// </summary>
    public sealed class SceneManager : ManagerBase<SceneManager>, ISceneManager
    {
        #region Variable

        /// <summary>
        ///     资源加载器的接口
        /// </summary>
        private IResourceManager m_resourceManager;

        /// <summary>
        ///     加载过的场景资源名称
        /// </summary>
        private readonly List<string> m_loadedSceneAssetNames;

        /// <summary>
        ///     加载中的场景资源名称
        /// </summary>
        private readonly List<string> m_loadingSceneAssetNames;

        /// <summary>
        ///     需要卸载的场景资源名称
        /// </summary>
        private readonly List<string> m_unloadingSceneAssetNames;

        /// <summary>
        ///     场景加载回调集合
        /// </summary>
        private readonly LoadSceneCallbacks m_loadSceneCallbacks;

        /// <summary>
        ///     场景卸载回调集合
        /// </summary>
        private readonly UnloadSceneCallbacks m_unloadSceneCallbacks;

        private EventHandler<LoadSceneSuccessEventArgs> m_loadSceneSuccessEventHandler;
        private EventHandler<LoadSceneFailureEventArgs> m_loadSceneFailureEventHandler;
        private EventHandler<LoadSceneUpdateEventArgs> m_loadSceneUpdateEventHandler;
        private EventHandler<LoadSceneDependencyAssetEventArgs> m_loadSceneDependencyAssetEventHandler;
        private EventHandler<UnloadSceneSuccessEventArgs> m_unloadSceneSuccessEventHandler;
        private EventHandler<UnloadSceneFailureEventArgs> m_unloadSceneFailureEventHandler;

        #endregion

        #region Property

        /// <summary>
        ///     当前场景
        /// </summary>
        public Level Level { get; private set; }

        /// <summary>
        ///     加载场景成功事件。
        /// </summary>
        public event EventHandler<LoadSceneSuccessEventArgs> LoadSceneSuccess
        {
            add => m_loadSceneSuccessEventHandler += value;
            remove => m_loadSceneSuccessEventHandler -= value;
        }

        /// <summary>
        ///     加载场景失败事件。
        /// </summary>
        public event EventHandler<LoadSceneFailureEventArgs> LoadSceneFailure
        {
            add => m_loadSceneFailureEventHandler += value;
            remove => m_loadSceneFailureEventHandler -= value;
        }

        /// <summary>
        ///     加载场景更新事件。
        /// </summary>
        public event EventHandler<LoadSceneUpdateEventArgs> LoadSceneUpdate
        {
            add => m_loadSceneUpdateEventHandler += value;
            remove => m_loadSceneUpdateEventHandler -= value;
        }

        /// <summary>
        ///     加载场景时加载依赖资源事件。
        /// </summary>
        public event EventHandler<LoadSceneDependencyAssetEventArgs> LoadSceneDependencyAsset
        {
            add => m_loadSceneDependencyAssetEventHandler += value;
            remove => m_loadSceneDependencyAssetEventHandler -= value;
        }

        /// <summary>
        ///     卸载场景成功事件。
        /// </summary>
        public event EventHandler<UnloadSceneSuccessEventArgs> UnloadSceneSuccess
        {
            add => m_unloadSceneSuccessEventHandler += value;
            remove => m_unloadSceneSuccessEventHandler -= value;
        }

        /// <summary>
        ///     卸载场景失败事件。
        /// </summary>
        public event EventHandler<UnloadSceneFailureEventArgs> UnloadSceneFailure
        {
            add => m_unloadSceneFailureEventHandler += value;
            remove => m_unloadSceneFailureEventHandler -= value;
        }

        #endregion

        #region Initialization & Destruction

        /// <summary>
        ///     初始化场景管理器的新实例。
        /// </summary>
        public SceneManager()
        {
            m_loadedSceneAssetNames = new List<string>();
            m_loadingSceneAssetNames = new List<string>();
            m_unloadingSceneAssetNames = new List<string>();
            m_loadSceneCallbacks = new LoadSceneCallbacks(LoadSceneSuccessCallback, LoadSceneFailureCallback,
                LoadSceneUpdateCallback, LoadSceneDependencyAssetCallback);
            m_unloadSceneCallbacks = new UnloadSceneCallbacks(UnloadSceneSuccessCallback, UnloadSceneFailureCallback);
            m_resourceManager = null;
            m_loadSceneSuccessEventHandler = null;
            m_loadSceneFailureEventHandler = null;
            m_loadSceneUpdateEventHandler = null;
            m_loadSceneDependencyAssetEventHandler = null;
            m_unloadSceneSuccessEventHandler = null;
            m_unloadSceneFailureEventHandler = null;
        }

        /// <summary>
        ///     关闭并清理场景管理器。
        /// </summary>
        public override void Shutdown()
        {
            var loadedSceneAssetNames = m_loadedSceneAssetNames.ToArray();
            foreach (var loadedSceneAssetName in loadedSceneAssetNames)
            {
                if (IsSceneUnloading(loadedSceneAssetName))
                {
                    continue;
                }

                UnloadScene(loadedSceneAssetName);
            }

            m_loadedSceneAssetNames.Clear();
            m_loadingSceneAssetNames.Clear();
            m_unloadingSceneAssetNames.Clear();
        }

        #endregion

        #region IModule

        /// <summary>
        ///     获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public override int Priority => (int) EnModuleType.ENMODULEDEFAULT;


        /// <summary>
        ///     场景管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        public override void OnUpdate( float elapseSeconds )
        {
        }

        #endregion

        #region Public Members

        /// <summary>
        ///     设置资源管理器。
        /// </summary>
        /// <param name="resourceManager">资源管理器。</param>
        public void SetResourceManager( IResourceManager resourceManager )
        {
            if (resourceManager == null)
            {
                throw new SsitEngineException("Resource manager is invalid.");
            }

            m_resourceManager = resourceManager;
        }

        /// <summary>
        ///     获取场景是否已加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否已加载。</returns>
        public bool IsSceneLoaded( string sceneAssetName )
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new SsitEngineException("Scene asset name is invalid.");
            }

            return m_loadedSceneAssetNames.Contains(sceneAssetName);
        }

        /// <summary>
        ///     获取场景是否正在加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在加载。</returns>
        public bool IsSceneLoading( string sceneAssetName )
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new SsitEngineException("Scene asset name is invalid.");
            }

            return m_loadingSceneAssetNames.Contains(sceneAssetName);
        }

        /// <summary>
        ///     获取场景是否正在卸载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在卸载。</returns>
        public bool IsSceneUnloading( string sceneAssetName )
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new SsitEngineException("Scene asset name is invalid.");
            }

            return m_unloadingSceneAssetNames.Contains(sceneAssetName);
        }

        /// <summary>
        ///     获取已加载场景的资源名称。
        /// </summary>
        /// <returns>已加载场景的资源名称。</returns>
        public string[] GetLoadedSceneAssetNames()
        {
            return m_loadedSceneAssetNames.ToArray();
        }

        /// <summary>
        ///     获取正在加载场景的资源名称。
        /// </summary>
        /// <returns>正在加载场景的资源名称。</returns>
        public string[] GetLoadingSceneAssetNames()
        {
            return m_loadingSceneAssetNames.ToArray();
        }

        /// <summary>
        ///     获取正在卸载场景的资源名称。
        /// </summary>
        /// <returns>正在卸载场景的资源名称。</returns>
        public string[] GetUnloadingSceneAssetNames()
        {
            return m_unloadingSceneAssetNames.ToArray();
        }

        #endregion

        #region Core Members

        /// <summary>
        ///     加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadScene( string sceneAssetName, int priority = SsitFrameUtils.DefaultPriority,
            object userData = null )
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new SsitEngineException("Scene asset name is invalid.");
            }

            if (m_resourceManager == null)
            {
                throw new SsitEngineException("You must set resource manager first.");
            }

            if (IsSceneUnloading(sceneAssetName))
            {
                throw new SsitEngineException(TextUtils.Format("Scene asset '{0}' is being unloaded.", sceneAssetName));
            }

            if (IsSceneLoading(sceneAssetName))
            {
                throw new SsitEngineException(TextUtils.Format("Scene asset '{0}' is being loaded.", sceneAssetName));
            }

            if (IsSceneLoaded(sceneAssetName))
            {
                throw new SsitEngineException(TextUtils.Format("Scene asset '{0}' is already loaded.", sceneAssetName));
            }

            m_loadingSceneAssetNames.Add(sceneAssetName);
            m_resourceManager.LoadScene(sceneAssetName, priority, m_loadSceneCallbacks, userData);
        }

        public void LoadScene( Level level, object userData = null )
        {
            Level = level;
            var resouceName = level.GetResoucesName();
            if (string.IsNullOrEmpty(resouceName))
            {
                throw new SsitEngineException("Scene asset name is invalid.");
            }

            if (m_resourceManager == null)
            {
                throw new SsitEngineException("You must set resource manager first.");
            }
            level.Load();
            m_loadingSceneAssetNames.Add(level.GetResoucesName());
        }

        /// <summary>
        ///     卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void UnloadScene( string sceneAssetName, object userData = null )
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new SsitEngineException("Scene asset name is invalid.");
            }

            if (m_resourceManager == null)
            {
                throw new SsitEngineException("You must set resource manager first.");
            }

            //if (IsSceneUnloading(sceneAssetName))
            //{
            //    throw new SsitEngineException(TextUtils.Format("Scene asset '{0}' is being unloaded.", sceneAssetName));
            //}

            //if (IsSceneLoading(sceneAssetName))
            //{
            //    throw new SsitEngineException(TextUtils.Format("Scene asset '{0}' is being loaded.", sceneAssetName));
            //}

            //if (!IsSceneLoaded(sceneAssetName))
            //{
            //    throw new SsitEngineException(TextUtils.Format("Scene asset '{0}' is not loaded yet.", sceneAssetName));
            //}

            if (Level != null)
            {
                Level.Unload();
            }

            //m_unloadingSceneAssetNames.Add(sceneAssetName);
            m_resourceManager.UnloadScene(sceneAssetName, m_unloadSceneCallbacks, userData);
        }

        /// <summary>
        ///     卸载场景。
        /// </summary>
        /// <param name="level">指定的卸载场景。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void UnloadScene( Level level, object userData )
        {
            if (level == null)
            {
                return;
            }
            var sceneAssetName = level.GetResoucesName();
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new SsitEngineException("Scene asset name is invalid.");
            }

            if (m_resourceManager == null)
            {
                throw new SsitEngineException("You must set resource manager first.");
            }

            //if (IsSceneUnloading(sceneAssetName))
            //{
            //    throw new SsitEngineException(TextUtils.Format("Scene asset '{0}' is being unloaded.", sceneAssetName));
            //}

            //if (IsSceneLoading(sceneAssetName))
            //{
            //    throw new SsitEngineException(TextUtils.Format("Scene asset '{0}' is being loaded.", sceneAssetName));
            //}

            //if (!IsSceneLoaded(sceneAssetName))
            //{
            //    throw new SsitEngineException(TextUtils.Format("Scene asset '{0}' is not loaded yet.", sceneAssetName));
            //}
            level.Unload();
            m_unloadingSceneAssetNames.Remove(sceneAssetName);
            m_resourceManager.UnloadScene(sceneAssetName, m_unloadSceneCallbacks, userData);
        }

        /// <summary>
        ///     卸载场景。
        /// </summary>
        /// <param name="level">指定的卸载场景。</param>
        public void UnloadScene( Level level )
        {
            if (level == null)
            {
                return;
            }
            var sceneAssetName = level.GetResoucesName();
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new SsitEngineException("Scene asset name is invalid.");
            }

            if (m_resourceManager == null)
            {
                throw new SsitEngineException("You must set resource manager first.");
            }

            //if (IsSceneUnloading(sceneAssetName))
            //{
            //    throw new SsitEngineException(TextUtils.Format("Scene asset '{0}' is being unloaded.", sceneAssetName));
            //}

            //if (IsSceneLoading(sceneAssetName))
            //{
            //    throw new SsitEngineException(TextUtils.Format("Scene asset '{0}' is being loaded.", sceneAssetName));
            //}

            //if (!IsSceneLoaded(sceneAssetName))
            //{
            //    throw new SsitEngineException(TextUtils.Format("Scene asset '{0}' is not loaded yet.", sceneAssetName));
            //}
            level.Unload();
            m_unloadingSceneAssetNames.Remove(sceneAssetName);
        }

        #endregion

        #region Internal Members

        private void LoadSceneSuccessCallback( string sceneAssetName, float duration, object userData )
        {
            m_loadingSceneAssetNames.Remove(sceneAssetName);
            m_loadedSceneAssetNames.Add(sceneAssetName);
            if (m_loadSceneSuccessEventHandler != null)
            {
                m_loadSceneSuccessEventHandler(this, new LoadSceneSuccessEventArgs(sceneAssetName, duration, userData));
            }
        }

        private void LoadSceneFailureCallback( string sceneAssetName, LoadResourceStatus status, string errorMessage,
            object userData )
        {
            m_loadingSceneAssetNames.Remove(sceneAssetName);
            var appendErrorMessage =
                TextUtils.Format("Load scene failure, scene asset name '{0}', status '{1}', error message '{2}'.",
                    sceneAssetName, status.ToString(), errorMessage);
            if (m_loadSceneFailureEventHandler != null)
            {
                m_loadSceneFailureEventHandler(this,
                    new LoadSceneFailureEventArgs(sceneAssetName, appendErrorMessage, userData));
                return;
            }

            throw new SsitEngineException(appendErrorMessage);
        }

        private void LoadSceneUpdateCallback( string sceneAssetName, float progress, object userData )
        {
            if (m_loadSceneUpdateEventHandler != null)
            {
                m_loadSceneUpdateEventHandler(this, new LoadSceneUpdateEventArgs(sceneAssetName, progress, userData));
            }
        }

        private void LoadSceneDependencyAssetCallback( string sceneAssetName, string dependencyAssetName,
            int loadedCount, int totalCount, object userData )
        {
            if (m_loadSceneDependencyAssetEventHandler != null)
            {
                m_loadSceneDependencyAssetEventHandler(this,
                    new LoadSceneDependencyAssetEventArgs(sceneAssetName, dependencyAssetName, loadedCount, totalCount,
                        userData));
            }
        }

        private void UnloadSceneSuccessCallback( string sceneAssetName, object userData )
        {
            m_unloadingSceneAssetNames.Remove(sceneAssetName);
            m_loadedSceneAssetNames.Remove(sceneAssetName);
            if (m_unloadSceneSuccessEventHandler != null)
            {
                m_unloadSceneSuccessEventHandler(this, new UnloadSceneSuccessEventArgs(sceneAssetName, userData));
            }
        }

        private void UnloadSceneFailureCallback( string sceneAssetName, object userData )
        {
            m_unloadingSceneAssetNames.Remove(sceneAssetName);
            if (m_unloadSceneFailureEventHandler != null)
            {
                m_unloadSceneFailureEventHandler(this, new UnloadSceneFailureEventArgs(sceneAssetName, userData));
                return;
            }

            throw new SsitEngineException(TextUtils.Format("Unload scene failure, scene asset name '{0}'.",
                sceneAssetName));
        }

        #endregion
    }
}