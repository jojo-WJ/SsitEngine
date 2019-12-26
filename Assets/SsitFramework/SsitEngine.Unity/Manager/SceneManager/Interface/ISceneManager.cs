/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月11日                             
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using SsitEngine.Unity.Resource;

namespace SsitEngine.Unity.Scene
{
    /// <summary>
    ///     场景管理器接口。
    /// </summary>
    public interface ISceneManager
    {
        /// <summary>
        ///     加载场景成功事件。
        /// </summary>
        event EventHandler<LoadSceneSuccessEventArgs> LoadSceneSuccess;

        /// <summary>
        ///     加载场景失败事件。
        /// </summary>
        event EventHandler<LoadSceneFailureEventArgs> LoadSceneFailure;

        /// <summary>
        ///     加载场景更新事件。
        /// </summary>
        event EventHandler<LoadSceneUpdateEventArgs> LoadSceneUpdate;

        /// <summary>
        ///     加载场景时加载依赖资源事件。
        /// </summary>
        event EventHandler<LoadSceneDependencyAssetEventArgs> LoadSceneDependencyAsset;

        /// <summary>
        ///     卸载场景成功事件。
        /// </summary>
        event EventHandler<UnloadSceneSuccessEventArgs> UnloadSceneSuccess;

        /// <summary>
        ///     卸载场景失败事件。
        /// </summary>
        event EventHandler<UnloadSceneFailureEventArgs> UnloadSceneFailure;

        /// <summary>
        ///     设置资源管理器。
        /// </summary>
        /// <param name="resourceManager">资源管理器。</param>
        void SetResourceManager( IResourceManager resourceManager );

        /// <summary>
        ///     获取场景是否已加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否已加载。</returns>
        bool IsSceneLoaded( string sceneAssetName );

        /// <summary>
        ///     获取已加载场景的资源名称。
        /// </summary>
        /// <returns>已加载场景的资源名称。</returns>
        string[] GetLoadedSceneAssetNames();


        /// <summary>
        ///     获取场景是否正在加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在加载。</returns>
        bool IsSceneLoading( string sceneAssetName );

        /// <summary>
        ///     获取正在加载场景的资源名称。
        /// </summary>
        /// <returns>正在加载场景的资源名称。</returns>
        string[] GetLoadingSceneAssetNames();

        /// <summary>
        ///     获取场景是否正在卸载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在卸载。</returns>
        bool IsSceneUnloading( string sceneAssetName );

        /// <summary>
        ///     获取正在卸载场景的资源名称。
        /// </summary>
        /// <returns>正在卸载场景的资源名称。</returns>
        string[] GetUnloadingSceneAssetNames();

        /// <summary>
        ///     加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        void LoadScene( string sceneAssetName, int priority = SsitFrameUtils.DefaultPriority, object userData = null );

        /// <summary>
        ///     卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        void UnloadScene( string sceneAssetName, object userData = null );
    }
}