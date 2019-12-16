/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：抽象引用对象                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/28 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace SsitEngine.Core.EventPool
{
    /// <summary>
    ///     事件池模式。
    /// </summary>
    [Flags]
    public enum EventPoolMode
    {
        /// <summary>
        ///     默认事件池模式，即必须存在有且只有一个事件处理函数
        /// </summary>
        Default = 0,

        /// <summary>
        ///     允许不存在事件处理函数
        /// </summary>
        AllowNoHandler = 1,

        /// <summary>
        ///     允许存在多个事件处理函数
        /// </summary>
        AllowMultiHandler = 2,

        /// <summary>
        ///     允许存在重复的事件处理函数
        /// </summary>
        AllowDuplicateHandler = 4,

        /// <summary>
        ///     允许直接处理函数（这个操作不是线程安全的，事件会立刻分发）
        /// </summary>
        AllowFireNowHandler = 5
    }
}