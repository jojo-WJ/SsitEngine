/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/11 12:17:01                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.Msic
{
    /// <summary>
    ///     Enum identifying the loading state of the resource
    /// </summary>
    public enum LoadingState
    {
        /// Not loaded
        LoadstateUnloaded,

        /// Loading is in progress
        LoadstateLoading,

        /// Fully loaded
        LoadstateLoaded,

        /// <summary>
        ///     更新中
        /// </summary>
        LoadstateUpdating,

        /// <summary>
        ///     更新完毕
        /// </summary>
        LoadstateUpdated,

        /// <summary>
        ///     卸载中
        /// </summary>
        LoadstateUnloading,

        /// Fully prepared
        LoadstatePrepared,

        /// Preparing is in progress
        LoadstatePreparing
    }

    /// <summary>
    ///     Unity对象镜像（对持有资源的进一步封装）
    ///     针对一些资源配置文件的加载
    /// </summary>
    public abstract class ResourceBase : AllocatedObject
    {
        // 资源收集器
        protected bool mIsCollectd;

        // 句柄
        public LoadingState mLoadingState;

        /// <summary>
        ///     构造方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="handle"></param>
        public ResourceBase()
        {
            mLoadingState = LoadingState.LoadstateUnloaded;
        }

        /// <summary>
        ///     设置资源收集标识
        /// </summary>
        /// <param name="b"></param>
        public virtual void SetCollected( bool b )
        {
            mIsCollectd = b;
        }

        /// <summary>
        ///     加载配置
        /// </summary>
        public virtual void PrepareImpl()
        {
            // Load from specified 'name'
        }

        /// <summary>
        ///     加载实现
        /// </summary>
        public virtual void LoadImpl()
        {
        }

        /// <summary>
        ///     更新
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        ///     卸载配置文件
        /// </summary>
        public virtual void UnprepareImpl()
        {
        }

        /// <summary>
        ///     卸载实现
        /// </summary>
        public virtual void UnloadImpl()
        {
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            base.Shutdown();
        }
    }
}