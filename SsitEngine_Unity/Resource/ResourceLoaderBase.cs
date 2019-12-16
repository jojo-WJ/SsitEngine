/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：资源加载器接口                                                    
*│　作   者：Jusam                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/04/29                        
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;
using UnityEngine.Events;

namespace SsitEngine.Unity.Resource
{
    
    /// <summary>
    ///     资源加载器接口
    /// </summary>
    internal interface IResourceLoader
    {
        /// <summary>
        ///     从Resource目录中加载资源
        /// </summary>
        /// <param name="Id">资源的ID标识</param>
        /// <returns>未实例化的资源</returns>
        T LoadAsset<T>( int Id ) where T : Object;

        /// <summary>
        ///     从Resource目录中加载资源
        /// </summary>
        /// <param name="Id">资源的ID标识</param>
        /// <returns>未实例化的资源</returns>
        void LoadAsset<T>( int Id, UnityAction<T> callBack ) where T : Object;

        /// <summary>
        ///     从Resource目录中加载资源
        /// </summary>
        /// <param name="Id">资源的ID标识</param>
        /// <returns>未实例化的资源</returns>
        void SyncLoadAsset<T>( int Id, UnityAction<T> callBack ) where T : Object;

        /// <summary>
        ///     从Resource目录中加载资源
        /// </summary>
        /// <param name="resPath">资源的相对路径</param>
        /// <returns>未实例化的资源</returns>
        void SyncLoadAsset<T>( string resPath, UnityAction<T> callBack ) where T : Object;

        T LoadAsset<T>( string resPath ) where T : Object;
    }

    /// <summary>
    ///     资源加载辅助器接口
    /// </summary>
    internal interface IResourceLoaderHelper
    {
        /// <summary>
        ///     从Resource目录中加载资源
        /// </summary>
        /// <param name="Id">资源的ID标识</param>
        /// <returns>未实例化的资源</returns>
        T LoadAsset<T>( int Id ) where T : Object;

        /// <summary>
        ///     从Resource目录中加载资源
        /// </summary>
        /// <param name="Id">资源的ID标识</param>
        /// <returns>未实例化的资源</returns>
        void LoadAsset<T>( int Id, UnityAction<T> callBack ) where T : Object;

        /// <summary>
        ///     从Resource目录中加载资源
        /// </summary>
        /// <param name="id">资源的ID标识</param>
        /// <returns>未实例化的资源</returns>
        void SyncLoadAsset<T>( int id, UnityAction<T> callBack ) where T : Object;

        /// <summary>
        ///     从Resource目录中加载资源
        /// </summary>
        /// <param name="resPath">资源的相对路径</param>
        /// <returns>未实例化的资源</returns>
        void SyncLoadAsset<T>( string resPath, UnityAction<T> callBack ) where T : Object;

        /// <summary>
        ///     根据资源路径加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resPath"></param>
        /// <returns></returns>
        T LoadAsset<T>( string resPath ) where T : Object;

        /// <summary>
        ///     资源卸载
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">卸载对象资源</param>
        void UnloadAsset<T>( Object obj );
    }

}