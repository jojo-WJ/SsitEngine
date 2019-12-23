/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/11 11:23:06                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Unity.Resource;

namespace SsitEngine.Unity.Scene
{
    /// <summary>
    ///     加载场景成功回调函数。
    /// </summary>
    /// <param name="sceneAssetName">要加载的场景资源名称。</param>
    /// <param name="duration">加载持续时间。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void LoadSceneSuccessCallback( string sceneAssetName, float duration, object userData );

    /// <summary>
    ///     加载场景失败回调函数。
    /// </summary>
    /// <param name="sceneAssetName">要加载的场景资源名称。</param>
    /// <param name="status">加载场景状态。</param>
    /// <param name="errorMessage">错误信息。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void LoadSceneFailureCallback( string sceneAssetName, LoadResourceStatus status,
        string errorMessage, object userData );

    /// <summary>
    ///     加载场景更新回调函数。
    /// </summary>
    /// <param name="sceneAssetName">要加载的场景资源名称。</param>
    /// <param name="progress">加载场景进度。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void LoadSceneUpdateCallback( string sceneAssetName, float progress, object userData );

    /// <summary>
    ///     加载场景时加载依赖资源回调函数。
    /// </summary>
    /// <param name="sceneAssetName">要加载的场景资源名称。</param>
    /// <param name="dependencyAssetName">被加载的依赖资源名称。</param>
    /// <param name="loadedCount">当前已加载依赖资源数量。</param>
    /// <param name="totalCount">总共加载依赖资源数量。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void LoadSceneDependencyAssetCallback( string sceneAssetName, string dependencyAssetName,
        int loadedCount, int totalCount, object userData );

    /// <summary>
    ///     卸载场景成功回调函数。
    /// </summary>
    /// <param name="sceneAssetName">要卸载的场景资源名称。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void UnloadSceneSuccessCallback( string sceneAssetName, object userData );

    /// <summary>
    ///     卸载场景失败回调函数。
    /// </summary>
    /// <param name="sceneAssetName">要卸载的场景资源名称。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void UnloadSceneFailureCallback( string sceneAssetName, object userData );

    /// <summary>
    ///     加载场景回调函数集。
    /// </summary>
    public sealed class LoadSceneCallbacks
    {
        /// <summary>
        ///     初始化加载场景回调函数集的新实例。
        /// </summary>
        /// <param name="loadSceneSuccessCallback">加载场景成功回调函数。</param>
        public LoadSceneCallbacks( LoadSceneSuccessCallback loadSceneSuccessCallback )
            : this(loadSceneSuccessCallback, null, null, null)
        {
        }

        /// <summary>
        ///     初始化加载场景回调函数集的新实例。
        /// </summary>
        /// <param name="loadSceneSuccessCallback">加载场景成功回调函数。</param>
        /// <param name="loadSceneFailureCallback">加载场景失败回调函数。</param>
        public LoadSceneCallbacks( LoadSceneSuccessCallback loadSceneSuccessCallback,
            LoadSceneFailureCallback loadSceneFailureCallback )
            : this(loadSceneSuccessCallback, loadSceneFailureCallback, null, null)
        {
        }

        /// <summary>
        ///     初始化加载场景回调函数集的新实例。
        /// </summary>
        /// <param name="loadSceneSuccessCallback">加载场景成功回调函数。</param>
        /// <param name="loadSceneUpdateCallback">加载场景更新回调函数。</param>
        public LoadSceneCallbacks( LoadSceneSuccessCallback loadSceneSuccessCallback,
            LoadSceneUpdateCallback loadSceneUpdateCallback )
            : this(loadSceneSuccessCallback, null, loadSceneUpdateCallback, null)
        {
        }

        /// <summary>
        ///     初始化加载场景回调函数集的新实例。
        /// </summary>
        /// <param name="loadSceneSuccessCallback">加载场景成功回调函数。</param>
        /// <param name="loadSceneDependencyAssetCallback">加载场景时加载依赖资源回调函数。</param>
        public LoadSceneCallbacks( LoadSceneSuccessCallback loadSceneSuccessCallback,
            LoadSceneDependencyAssetCallback loadSceneDependencyAssetCallback )
            : this(loadSceneSuccessCallback, null, null, loadSceneDependencyAssetCallback)
        {
        }

        /// <summary>
        ///     初始化加载场景回调函数集的新实例。
        /// </summary>
        /// <param name="loadSceneSuccessCallback">加载场景成功回调函数。</param>
        /// <param name="loadSceneFailureCallback">加载场景失败回调函数。</param>
        /// <param name="loadSceneUpdateCallback">加载场景更新回调函数。</param>
        public LoadSceneCallbacks( LoadSceneSuccessCallback loadSceneSuccessCallback,
            LoadSceneFailureCallback loadSceneFailureCallback, LoadSceneUpdateCallback loadSceneUpdateCallback,
            object loadSceneDependencyAssetCallback )
            : this(loadSceneSuccessCallback, loadSceneFailureCallback, loadSceneUpdateCallback, null)
        {
        }

        /// <summary>
        ///     初始化加载场景回调函数集的新实例。
        /// </summary>
        /// <param name="loadSceneSuccessCallback">加载场景成功回调函数。</param>
        /// <param name="loadSceneFailureCallback">加载场景失败回调函数。</param>
        /// <param name="loadSceneDependencyAssetCallback">加载场景时加载依赖资源回调函数。</param>
        public LoadSceneCallbacks( LoadSceneSuccessCallback loadSceneSuccessCallback,
            LoadSceneFailureCallback loadSceneFailureCallback,
            LoadSceneDependencyAssetCallback loadSceneDependencyAssetCallback )
            : this(loadSceneSuccessCallback, loadSceneFailureCallback, null, loadSceneDependencyAssetCallback)
        {
        }

        /// <summary>
        ///     初始化加载场景回调函数集的新实例。
        /// </summary>
        /// <param name="loadSceneSuccessCallback">加载场景成功回调函数。</param>
        /// <param name="loadSceneFailureCallback">加载场景失败回调函数。</param>
        /// <param name="loadSceneUpdateCallback">加载场景更新回调函数。</param>
        /// <param name="loadSceneDependencyAssetCallback">加载场景时加载依赖资源回调函数。</param>
        public LoadSceneCallbacks( LoadSceneSuccessCallback loadSceneSuccessCallback,
            LoadSceneFailureCallback loadSceneFailureCallback, LoadSceneUpdateCallback loadSceneUpdateCallback,
            LoadSceneDependencyAssetCallback loadSceneDependencyAssetCallback )
        {
            if (loadSceneSuccessCallback == null)
            {
                throw new SsitEngineException("Load scene success callback is invalid.");
            }

            LoadSceneSuccessCallback = loadSceneSuccessCallback;
            LoadSceneFailureCallback = loadSceneFailureCallback;
            LoadSceneUpdateCallback = loadSceneUpdateCallback;
            LoadSceneDependencyAssetCallback = loadSceneDependencyAssetCallback;
        }

        /// <summary>
        ///     获取加载场景成功回调函数。
        /// </summary>
        public LoadSceneSuccessCallback LoadSceneSuccessCallback { get; }

        /// <summary>
        ///     获取加载场景失败回调函数。
        /// </summary>
        public LoadSceneFailureCallback LoadSceneFailureCallback { get; }

        /// <summary>
        ///     获取加载场景更新回调函数。
        /// </summary>
        public LoadSceneUpdateCallback LoadSceneUpdateCallback { get; }

        /// <summary>
        ///     获取加载场景时加载依赖资源回调函数。
        /// </summary>
        public LoadSceneDependencyAssetCallback LoadSceneDependencyAssetCallback { get; }
    }

    /// <summary>
    ///     卸载场景回调函数集。
    /// </summary>
    public sealed class UnloadSceneCallbacks
    {
        /// <summary>
        ///     初始化卸载场景回调函数集的新实例。
        /// </summary>
        /// <param name="unloadSceneSuccessCallback">卸载场景成功回调函数。</param>
        public UnloadSceneCallbacks( UnloadSceneSuccessCallback unloadSceneSuccessCallback )
            : this(unloadSceneSuccessCallback, null)
        {
        }

        /// <summary>
        ///     初始化卸载场景回调函数集的新实例。
        /// </summary>
        /// <param name="unloadSceneSuccessCallback">卸载场景成功回调函数。</param>
        /// <param name="unloadSceneFailureCallback">卸载场景失败回调函数。</param>
        public UnloadSceneCallbacks( UnloadSceneSuccessCallback unloadSceneSuccessCallback,
            UnloadSceneFailureCallback unloadSceneFailureCallback )
        {
            if (unloadSceneSuccessCallback == null)
            {
                throw new SsitEngineException("Unload scene success callback is invalid.");
            }

            UnloadSceneSuccessCallback = unloadSceneSuccessCallback;
            UnloadSceneFailureCallback = unloadSceneFailureCallback;
        }

        /// <summary>
        ///     获取卸载场景成功回调函数。
        /// </summary>
        public UnloadSceneSuccessCallback UnloadSceneSuccessCallback { get; }

        /// <summary>
        ///     获取卸载场景失败回调函数。
        /// </summary>
        public UnloadSceneFailureCallback UnloadSceneFailureCallback { get; }
    }
}