/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：加载资源的状态                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/11 11:25:40                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.Resource
{
    /// <summary>
    ///     加载资源状态。
    /// </summary>
    public enum LoadResourceStatus
    {
        /// <summary>
        ///     加载资源完成。
        /// </summary>
        Ok = 0,

        /// <summary>
        ///     资源尚未准备完毕。
        /// </summary>
        NotReady,

        /// <summary>
        ///     资源不存在于磁盘上。
        /// </summary>
        NotExist,

        /// <summary>
        ///     依赖资源错误。
        /// </summary>
        DependencyError,

        /// <summary>
        ///     资源类型错误。
        /// </summary>
        TypeError,

        /// <summary>
        ///     加载子资源错误。
        /// </summary>
        ChildAssetError,

        /// <summary>
        ///     加载场景资源错误。
        /// </summary>
        SceneAssetError
    }
}