/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：框架入口                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/18 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace SsitEngine.Fsm
{
    /// <summary>
    ///     有限状态机状态基类。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    public abstract class FsmState<T> where T : class
    {
        private readonly Dictionary<int, FsmEventHandler<T>> m_eventHandlerMaps;
        private readonly int m_stateId;

        /// <summary>
        ///     初始化有限状态机状态基类的新实例。
        /// </summary>
        public FsmState( int stateId )
        {
            m_stateId = stateId;
            m_eventHandlerMaps = new Dictionary<int, FsmEventHandler<T>>();
        }

        /// <summary>
        ///     获取状态id
        /// </summary>
        /// <returns></returns>
        public int GetStateId()
        {
            return m_stateId;
        }

        /// <summary>
        ///     有限状态机状态初始化时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        protected internal virtual void OnInit( IFsm<T> fsm )
        {
        }

        /// <summary>
        ///     有限状态机状态进入时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        protected internal virtual void OnEnter( IFsm<T> fsm )
        {
        }

        /// <summary>
        ///     有限状态机状态轮询时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        protected internal virtual void OnUpdate( IFsm<T> fsm, float elapseSeconds )
        {
        }

        /// <summary>
        ///     有限状态机状态离开时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        /// <param name="isShutdown">是否是关闭有限状态机时触发。</param>
        protected internal virtual void OnExit( IFsm<T> fsm, bool isShutdown )
        {
        }

        /// <summary>
        ///     有限状态机状态销毁时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        protected internal virtual void OnDestroy( IFsm<T> fsm )
        {
            m_eventHandlerMaps.Clear();
        }

        /// <summary>
        ///     订阅有限状态机事件。
        /// </summary>
        /// <param name="eventId">事件编号。</param>
        /// <param name="eventHandler">有限状态机事件响应函数。</param>
        public void SubscribeEvent( int eventId, FsmEventHandler<T> eventHandler )
        {
            if (eventHandler == null)
            {
                throw new SsitEngineException("Event handler is invalid.");
            }

            if (!m_eventHandlerMaps.ContainsKey(eventId))
            {
                m_eventHandlerMaps[eventId] = eventHandler;
            }
            else
            {
                m_eventHandlerMaps[eventId] += eventHandler;
            }
        }

        /// <summary>
        ///     取消订阅有限状态机事件。
        /// </summary>
        /// <param name="eventId">事件编号。</param>
        /// <param name="eventHandler">有限状态机事件响应函数。</param>
        public void UnsubscribeEvent( int eventId, FsmEventHandler<T> eventHandler )
        {
            if (eventHandler == null)
            {
                throw new SsitEngineException("Event handler is invalid.");
            }

            if (m_eventHandlerMaps.ContainsKey(eventId))
            {
                m_eventHandlerMaps[eventId] -= eventHandler;
            }
        }

        /// <summary>
        ///     切换当前有限状态机状态。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        /// <param name="stateId">要切换到的有限状态机状态id。</param>
        public void ChangeState( IFsm<T> fsm, int stateId )
        {
            var fsmImplement = fsm;
            if (fsmImplement == null)
            {
                throw new SsitEngineException("FSM is invalid.");
            }
            fsmImplement.ChangeState(stateId);
        }

        /// <summary>
        ///     响应有限状态机事件时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        /// <param name="sender">事件源。</param>
        /// <param name="eventId">事件编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnEvent( IFsm<T> fsm, object sender, int eventId, object userData )
        {
            FsmEventHandler<T> eventHandlers = null;
            if (m_eventHandlerMaps.TryGetValue(eventId, out eventHandlers))
            {
                if (eventHandlers != null)
                {
                    eventHandlers(fsm, sender, userData);
                }
            }
        }
    }
}