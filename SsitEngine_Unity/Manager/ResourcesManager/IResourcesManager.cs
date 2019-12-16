/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：资源管理器的接口                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：time                             
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Unity.Scene;

namespace SsitEngine.Unity.Resource
{
    /// <summary>
    ///     资源管理器的接口
    /// </summary>
    public interface IResourceManager
    {
        //todo:下面两个我需要

        /// <summary>
        ///     异步卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">要卸载场景资源的名称。</param>
        /// <param name="unloadSceneCallbacks">卸载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        void UnloadScene( string sceneAssetName, UnloadSceneCallbacks unloadSceneCallbacks, object userData );

        /// <summary>
        ///     异步加载场景。
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称。</param>
        /// <param name="priority">加载场景资源的优先级。</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        void LoadScene( string sceneAssetName, int priority, LoadSceneCallbacks loadSceneCallbacks, object userData );

        /*
         * /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="sceneAssetName">要加载场景资源的名称</param>
        /// <param name="loadSceneCallbacks">加载场景回调函数</param>
        /// <param name="userData">用户自定义数据</param>
        void LoadScene( string sceneAssetName, LoaderRefProgress loadSceneCallbacks, object userData );*/
    }
}