/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/27 11:37:18                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity
{
    /// <summary>
    ///     管理器的基类
    /// </summary>
    public class ManagerBase<T> : SingletonMono<T>, IModule where T : SingletonMono<T>
    {
        /// <summary>
        ///     是否中断
        /// </summary>
        protected bool isShutdown;

        /// <summary>
        ///     视图控件
        /// </summary>
        public virtual object ViewComponent => this;

        /// <inheritdoc />
        public virtual string ModuleName => typeof(T).FullName;

        /// <inheritdoc />
        public virtual int Priority => (int) EnModuleType.ENMODULEDEFAULT;

        /// <inheritdoc />
        public virtual void OnUpdate( float elapseSeconds )
        {
        }

        /// <inheritdoc />
        public virtual void Shutdown()
        {
        }


        /// <inheritdoc />
        public override void OnSingletonInit()
        {
            isShutdown = false;
        }
    }
}