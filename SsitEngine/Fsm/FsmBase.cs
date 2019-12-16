/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：有限状态机基类                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/18 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace SsitEngine.Fsm
{
    /// <summary>
    ///     有限状态机基类。
    /// </summary>
    public abstract class FsmBase
    {
        /// <summary>
        ///     初始化有限状态机基类的新实例。
        /// </summary>
        public FsmBase() : this(null)
        {
        }

        /// <summary>
        ///     初始化有限状态机基类的新实例。
        /// </summary>
        /// <param name="name">有限状态机名称。</param>
        public FsmBase( string name )
        {
            Name = name ?? string.Empty;
        }

        /// <summary>
        ///     获取有限状态机名称。
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     获取有限状态机持有者类型。
        /// </summary>
        public abstract Type OwnerType { get; }

        /// <summary>
        ///     获取有限状态机中状态的数量。
        /// </summary>
        public abstract int FsmStateCount { get; }

        /// <summary>
        ///     获取有限状态机是否正在运行。
        /// </summary>
        public abstract bool IsRunning { get; }

        /// <summary>
        ///     获取有限状态机是否被销毁。
        /// </summary>
        public abstract bool IsDestroyed { get; }

        /// <summary>
        ///     获取当前有限状态机状态名称。
        /// </summary>
        public abstract string CurrentStateName { get; }

        /// <summary>
        ///     获取当前有限状态机状态持续时间。
        /// </summary>
        public abstract float CurrentStateTime { get; }

        /// <summary>
        ///     有限状态机轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        public abstract void OnUpdate( float elapseSeconds );

        /// <summary>
        ///     关闭并清理有限状态机。
        /// </summary>
        public abstract void Shutdown();
    }
}