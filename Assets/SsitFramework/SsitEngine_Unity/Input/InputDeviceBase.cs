/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：驱动基类                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月16日                             
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.PureMVC.Interfaces;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    ///     驱动基类
    /// </summary>
    public abstract class InputDeviceBase
    {
        /// <summary>
        ///     驱动辅助器
        /// </summary>
        protected IInputHandlerHelper inputHandlerHelper;

        /// <summary>
        ///     驱动激活标识
        /// </summary>
        protected bool m_enable;

        /// <summary>
        ///     创建一个操作驱动
        /// </summary>
        /// <param name="inputHandlerHelper"></param>
        public InputDeviceBase( IInputHandlerHelper inputHandlerHelper )
        {
            this.inputHandlerHelper = inputHandlerHelper;
        }

        /// <summary>
        ///     驱动名称
        /// </summary>
        public abstract string DeviceName { get; }

        /// <summary>
        ///     设置或禁用驱动
        /// </summary>
        public bool Enable
        {
            get => m_enable;
            set => m_enable = value;
        }

        /// <summary>
        ///     驱动更新
        /// </summary>
        public abstract void Update();

        /// <summary>
        ///     驱动销毁
        /// </summary>
        public abstract void Destroy();

        /// <summary>
        ///     驱动事件回调
        /// </summary>
        /// <param name="obj"></param>
        public virtual void HandleNotification( INotification obj )
        {
        }
    }
}