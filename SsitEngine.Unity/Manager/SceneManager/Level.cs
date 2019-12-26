/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/11 12:07:17                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Unity.Msic;

namespace SsitEngine.Unity.Scene
{
    /// <summary>
    ///     场景镜像
    /// </summary>
    public class Level : ResourceBase
    {
        protected string resourceName;

        public string GetResoucesName()
        {
            return resourceName;
        }

        public override void PrepareImpl()
        {
            mLoadingState = LoadingState.LoadstatePrepared;
        }

        public override void LoadImpl()
        {
        }

        public override void UnloadImpl()
        {
            mLoadingState = LoadingState.LoadstateUnloaded;
        }

        public virtual void Load()
        {
            PrepareImpl();
            LoadImpl();
        }

        public virtual void Unload()
        {
            UnloadImpl();
        }

        public virtual void PostLoadProcess()
        {
        }

        public override void Shutdown()
        {
            resourceName = null;
            base.Shutdown();
        }
    }
}