/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：场景加载成功、失败、加载中及场景卸载成功、失败事件定义                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/11 9:12:55                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Core;

namespace SsitEngine.Unity.Scene
{
    /// <summary>
    ///     加载场景成功事件。
    /// </summary>
    public sealed class LoadSceneSuccessEventArgs : SsitEventArgs
    {
        /// <summary>
        ///     初始化加载场景成功事件的新实例。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="duration">加载持续时间。</param>
        /// <param name="userData">用户自定义数据。</param>
        public LoadSceneSuccessEventArgs( string sceneAssetName, float duration, object userData )
        {
            SceneAssetName = sceneAssetName;
            Duration = duration;
            UserData = userData;
        }

        /// <summary>
        ///     获取场景资源名称。
        /// </summary>
        public string SceneAssetName { get; }

        /// <summary>
        ///     获取加载持续时间。
        /// </summary>
        public float Duration { get; }

        /// <summary>
        ///     获取用户自定义数据。
        /// </summary>
        public object UserData { get; }
    }

    /// <summary>
    ///     加载场景更新事件。
    /// </summary>
    public sealed class LoadSceneUpdateEventArgs : SsitEventArgs
    {
        /// <summary>
        ///     初始化加载场景更新事件的新实例。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="progress">加载场景进度。</param>
        /// <param name="userData">用户自定义数据。</param>
        public LoadSceneUpdateEventArgs( string sceneAssetName, float progress, object userData )
        {
            SceneAssetName = sceneAssetName;
            Progress = progress;
            UserData = userData;
        }

        /// <summary>
        ///     获取场景资源名称。
        /// </summary>
        public string SceneAssetName { get; }

        /// <summary>
        ///     获取加载场景进度。
        /// </summary>
        public float Progress { get; }

        /// <summary>
        ///     获取用户自定义数据。
        /// </summary>
        public object UserData { get; }
    }

    /// <summary>
    ///     加载场景失败事件。
    /// </summary>
    public sealed class LoadSceneFailureEventArgs : SsitEventArgs
    {
        /// <summary>
        ///     初始化加载场景失败事件的新实例。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="errorMessage">错误信息。</param>
        /// <param name="userData">用户自定义数据。</param>
        public LoadSceneFailureEventArgs( string sceneAssetName, string errorMessage, object userData )
        {
            SceneAssetName = sceneAssetName;
            ErrorMessage = errorMessage;
            UserData = userData;
        }

        /// <summary>
        ///     获取场景资源名称。
        /// </summary>
        public string SceneAssetName { get; }

        /// <summary>
        ///     获取错误信息。
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        ///     获取用户自定义数据。
        /// </summary>
        public object UserData { get; }
    }

    /// <summary>
    ///     卸载场景成功事件。
    /// </summary>
    public sealed class UnloadSceneSuccessEventArgs : SsitEventArgs
    {
        /// <summary>
        ///     初始化卸载场景成功事件的新实例。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public UnloadSceneSuccessEventArgs( string sceneAssetName, object userData )
        {
            SceneAssetName = sceneAssetName;
            UserData = userData;
        }

        /// <summary>
        ///     获取场景资源名称。
        /// </summary>
        public string SceneAssetName { get; }

        /// <summary>
        ///     获取用户自定义数据。
        /// </summary>
        public object UserData { get; }
    }

    /// <summary>
    ///     卸载场景失败事件。
    /// </summary>
    public sealed class UnloadSceneFailureEventArgs : SsitEventArgs
    {
        /// <summary>
        ///     初始化卸载场景失败事件的新实例。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public UnloadSceneFailureEventArgs( string sceneAssetName, object userData )
        {
            SceneAssetName = sceneAssetName;
            UserData = userData;
        }

        /// <summary>
        ///     获取场景资源名称。
        /// </summary>
        public string SceneAssetName { get; }

        /// <summary>
        ///     获取用户自定义数据。
        /// </summary>
        public object UserData { get; }
    }

    /// <summary>
    ///     加载场景时加载依赖资源事件。
    /// </summary>
    public sealed class LoadSceneDependencyAssetEventArgs : SsitEventArgs
    {
        /// <summary>
        ///     初始化加载场景时加载依赖资源事件的新实例。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="dependencyAssetName">被加载的依赖资源名称。</param>
        /// <param name="loadedCount">当前已加载依赖资源数量。</param>
        /// <param name="totalCount">总共加载依赖资源数量。</param>
        /// <param name="userData">用户自定义数据。</param>
        public LoadSceneDependencyAssetEventArgs( string sceneAssetName, string dependencyAssetName, int loadedCount,
            int totalCount, object userData )
        {
            SceneAssetName = sceneAssetName;
            DependencyAssetName = dependencyAssetName;
            LoadedCount = loadedCount;
            TotalCount = totalCount;
            UserData = userData;
        }

        /// <summary>
        ///     获取场景资源名称。
        /// </summary>
        public string SceneAssetName { get; }

        /// <summary>
        ///     获取被加载的依赖资源名称。
        /// </summary>
        public string DependencyAssetName { get; }

        /// <summary>
        ///     获取当前已加载依赖资源数量。
        /// </summary>
        public int LoadedCount { get; }

        /// <summary>
        ///     获取总共加载依赖资源数量。
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        ///     获取用户自定义数据。
        /// </summary>
        public object UserData { get; }
    }
}