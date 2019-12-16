/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：抽象引用对象                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/28 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Core.EventPool
{
    public partial class EventPool<T>
    {
        /// <summary>
        ///     事件结点。
        /// </summary>
        private sealed class Event
        {
            public Event( object sender, T e )
            {
                Sender = sender;
                EventArgs = e;
            }

            public object Sender { get; }

            public T EventArgs { get; }
        }
    }
}