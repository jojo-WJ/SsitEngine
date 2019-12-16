/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：抽象引用对象                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/28 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Core.ReferencePool;

namespace SsitEngine.Core.EventPool
{
    /// <summary>
    ///     事件基类。
    /// </summary>
    public abstract class BaseEventArgs : SsitEventArgs, IReference
    {
        /// <summary>
        ///     获取事件id。
        /// </summary>
        public abstract ushort Id { get; }

        /// <summary>
        ///     清理引用。
        /// </summary>
        public abstract void Clear();
    }
}