/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：事件池（应用于队列事件响应（网络消息处理、消息系统处理））                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/28 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using SsitEngine.Core.ReferencePool;

namespace SsitEngine.Core.EventPool
{
    /// <summary>
    ///     事件池。
    /// </summary>
    /// <typeparam name="T">事件类型。</typeparam>
    /// <remarks>
    ///     1、板主要维护订阅事件列表m_eventHandlers和消息处理队列m_events
    ///     2、消息订阅和注销
    ///     3、消息的线程安全抛出
    /// </remarks>
    public sealed partial class EventPool<T> where T : BaseEventArgs
    {
        private readonly Dictionary<ushort, LinkedList<EventHandler<T>>> m_eventHandlers;
        private readonly EventPoolMode m_eventPoolMode;
        private readonly Queue<Event> m_events;
        private EventHandler<T> m_defaultHandler;

        /// <summary>
        ///     初始化事件池的新实例。
        /// </summary>
        /// <param name="mode">事件池模式。</param>
        public EventPool( EventPoolMode mode )
        {
            m_eventHandlers = new Dictionary<ushort, LinkedList<EventHandler<T>>>();
            if (mode != EventPoolMode.AllowFireNowHandler) m_events = new Queue<Event>();
            m_eventPoolMode = mode;
            m_defaultHandler = null;
        }

        /// <summary>
        ///     获取事件处理函数的数量。
        /// </summary>
        public int EventHandlerCount => m_eventHandlers.Count;

        /// <summary>
        ///     获取事件数量。
        /// </summary>
        public int EventCount => m_events.Count;

        /// <summary>
        ///     事件池轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update( float elapseSeconds, float realElapseSeconds )
        {
            if (m_eventPoolMode == EventPoolMode.AllowFireNowHandler)
                return;
            lock (m_events)
            {
                while (m_events.Count > 0)
                {
                    var e = m_events.Dequeue();
                    HandleEvent(e.Sender, e.EventArgs);
                }
            }
        }

        /// <summary>
        ///     关闭并清理事件池。
        /// </summary>
        public void Shutdown()
        {
            Clear();
            m_eventHandlers.Clear();
            m_defaultHandler = null;
        }

        /// <summary>
        ///     清理事件。
        /// </summary>
        public void Clear()
        {
            if (m_eventPoolMode == EventPoolMode.AllowFireNowHandler)
                return;
            lock (m_events)
            {
                m_events.Clear();
            }
        }

        /// <summary>
        ///     获取事件处理函数的数量。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <returns>事件处理函数的数量。</returns>
        public int Count( ushort id )
        {
            LinkedList<EventHandler<T>> handlers = null;
            if (m_eventHandlers.TryGetValue(id, out handlers)) return handlers.Count;

            return 0;
        }

        /// <summary>
        ///     检查是否存在事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要检查的事件处理函数。</param>
        /// <returns>是否存在事件处理函数。</returns>
        public bool Check( ushort id, EventHandler<T> handler )
        {
            if (handler == null) throw new SsitEngineException("Event handler is invalid.");

            LinkedList<EventHandler<T>> handlers = null;
            if (!m_eventHandlers.TryGetValue(id, out handlers)) return false;

            return handlers.Contains(handler);
        }

        /// <summary>
        ///     订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要订阅的事件处理函数。</param>
        public void Subscribe( ushort id, EventHandler<T> handler )
        {
            if (handler == null) throw new SsitEngineException("Event handler is invalid.");

            LinkedList<EventHandler<T>> handlers = null;
            if (!m_eventHandlers.TryGetValue(id, out handlers))
            {
                handlers = new LinkedList<EventHandler<T>>();
                handlers.AddLast(handler);
                m_eventHandlers.Add(id, handlers);
            }
            else if ((m_eventPoolMode & EventPoolMode.AllowMultiHandler) == 0)
            {
                throw new SsitEngineException(TextUtils.Format("Event '{0}' not allow multi handler.", id.ToString()));
            }
            else if ((m_eventPoolMode & EventPoolMode.AllowDuplicateHandler) == 0 && Check(id, handler))
            {
                throw new SsitEngineException(TextUtils.Format("Event '{0}' not allow duplicate handler.",
                    id.ToString()));
            }
            else
            {
                handlers.AddLast(handler);
            }
        }

        /// <summary>
        ///     取消订阅事件处理函数。
        /// </summary>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理函数。</param>
        public void Unsubscribe( ushort id, EventHandler<T> handler )
        {
            if (handler == null) throw new SsitEngineException("Event handler is invalid.");

            LinkedList<EventHandler<T>> handlers = null;
            if (!m_eventHandlers.TryGetValue(id, out handlers))
                throw new SsitEngineException(TextUtils.Format("Event '{0}' not exists any handler.", id.ToString()));

            if (!handlers.Remove(handler))
                throw new SsitEngineException(TextUtils.Format("Event '{0}' not exists specified handler.",
                    id.ToString()));
        }

        /// <summary>
        ///     设置默认事件处理函数。
        /// </summary>
        /// <param name="handler">要设置的默认事件处理函数。</param>
        public void SetDefaultHandler( EventHandler<T> handler )
        {
            m_defaultHandler = handler;
        }

        /// <summary>
        ///     抛出事件，这个操作是线程安全的，即使不在主线程中抛出，也可保证在主线程中回调事件处理函数，但事件会在抛出后的下一帧分发。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        public void Fire( object sender, T e )
        {
            if (m_eventPoolMode == EventPoolMode.AllowFireNowHandler)
                return;
            var eventNode = new Event(sender, e);
            lock (m_events)
            {
                m_events.Enqueue(eventNode);
            }
        }

        /// <summary>
        ///     抛出事件立即模式，这个操作不是线程安全的，事件会立刻分发。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        public void FireNow( object sender, T e )
        {
            HandleEvent(sender, e);
        }

        /// <summary>
        ///     处理事件结点。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="e">事件参数。</param>
        private void HandleEvent( object sender, T e )
        {
            var eventId = e.Id;
            var noHandlerException = false;
            LinkedList<EventHandler<T>> handlers = null;
            if (m_eventHandlers.TryGetValue(eventId, out handlers) && handlers.Count > 0)
            {
                var current = handlers.First;
                while (current != null)
                {
                    var next = current.Next;
                    current.Value(sender, e);
                    current = next;
                }
            }
            else if (m_defaultHandler != null)
            {
                m_defaultHandler(sender, e);
            }
            else if ((m_eventPoolMode & EventPoolMode.AllowNoHandler) == EventPoolMode.AllowNoHandler)
            {
                noHandlerException = true;
            }

            ReferencePool.ReferencePool.Release((IReference) e);

            if (noHandlerException)
                throw new SsitEngineException(TextUtils.Format("Event '{0}' not allow no handler.",
                    eventId.ToString()));
        }
    }
}