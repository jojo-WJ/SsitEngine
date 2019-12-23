/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：泛型有限状态机                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/18 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace SsitEngine.Fsm
{
    /// <summary>
    ///     有限状态机。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    public class Fsm<T> : FsmBase, IFsm<T> where T : class
    {
        private readonly Dictionary<string, AllocatedObject> m_datas;
        private readonly Dictionary<int, FsmState<T>> m_states;
        private float m_currentStateTime;
        private bool m_isDestroyed;

        /// <summary>
        ///     状态改变回调
        /// </summary>
        public FsmStateChangeHandler<T> OnStateChangedHandler;

        /// <summary>
        ///     状态检测回调
        /// </summary>
        public FsmStateCheckHandler OnStateCheckHandler;


        /// <summary>
        ///     初始化有限状态机的新实例。
        /// </summary>
        /// <param name="name">有限状态机名称。</param>
        /// <param name="owner">有限状态机持有者。</param>
        /// <param name="states">有限状态机状态集合。</param>
        public Fsm( string name, T owner, params FsmState<T>[] states )
            : base(name)
        {
            if (owner == null)
            {
                throw new SsitEngineException("FSM owner is invalid.");
            }

            if (states == null || states.Length < 1)
            {
                throw new SsitEngineException("FSM states is invalid.");
            }

            Owner = owner;
            m_states = new Dictionary<int, FsmState<T>>();
            m_datas = new Dictionary<string, AllocatedObject>();

            foreach (var state in states)
            {
                if (state == null)
                {
                    throw new SsitEngineException("FSM states is invalid.");
                }

                var stateName = state.GetStateId();
                if (m_states.ContainsKey(stateName))
                {
                    throw new SsitEngineException(TextUtils.Format("FSM '{0}' state '{1}' is already exist.",
                        TextUtils.GetFullName<T>(name), stateName));
                }

                m_states.Add(stateName, state);
                state.OnInit(this);
            }

            m_currentStateTime = 0f;
            CurrentState = null;
            m_isDestroyed = false;
        }

        /// <summary>
        ///     获取有限状态机持有者类型。
        /// </summary>
        public override Type OwnerType => typeof(T);

        /// <summary>
        ///     获取当前有限状态机状态名称。
        /// </summary>
        public override string CurrentStateName => CurrentState != null ? CurrentState.GetType().FullName : null;

        /// <summary>
        ///     获取有限状态机持有者。
        /// </summary>
        public T Owner { get; }

        /// <summary>
        ///     获取有限状态机中状态的数量。
        /// </summary>
        public override int FsmStateCount => m_states.Count;

        /// <summary>
        ///     获取有限状态机是否正在运行。
        /// </summary>
        public override bool IsRunning => CurrentState != null;

        /// <summary>
        ///     获取有限状态机是否被销毁。
        /// </summary>
        public override bool IsDestroyed => m_isDestroyed;

        /// <summary>
        ///     获取当前有限状态机状态。
        /// </summary>
        public FsmState<T> CurrentState { get; private set; }

        /// <summary>
        ///     获取当前有限状态机状态持续时间。
        /// </summary>
        public override float CurrentStateTime => m_currentStateTime;

        /// <summary>
        ///     开始有限状态机。
        /// </summary>
        /// <param name="stateId">要开始的有限状态机状态类型。</param>
        public void Start( int stateId )
        {
            if (IsRunning)
            {
                throw new SsitEngineException("FSM is running, can not start again.");
            }

            var state = GetState(stateId);

            if (state == null)
            {
                throw new SsitEngineException(TextUtils.Format(
                    "FSM '{0}' can not start state '{1}' which is not exist.", TextUtils.GetFullName<T>(Name),
                    stateId));
            }

            m_currentStateTime = 0f;
            CurrentState = state;
            CurrentState.OnEnter(this);
        }

        /// <summary>
        ///     是否存在有限状态机状态。
        /// </summary>
        /// <param name="stateType">要检查的有限状态机状态类型。</param>
        /// <returns>是否存在有限状态机状态。</returns>
        public bool HasState( int stateType )
        {
            return m_states.ContainsKey(stateType);
        }

        /// <summary>
        ///     获取有限状态机状态。
        /// </summary>
        /// <param name="stateId">要获取的有限状态机状态类型。</param>
        /// <returns>要获取的有限状态机状态。</returns>
        public FsmState<T> GetState( int stateId )
        {
            FsmState<T> state = null;
            if (m_states.TryGetValue(stateId, out state))
            {
                return state;
            }
            return null;
        }

        /// <summary>
        ///     获取有限状态机的所有状态。
        /// </summary>
        /// <returns>有限状态机的所有状态。</returns>
        public FsmState<T>[] GetAllStates()
        {
            var index = 0;
            var results = new FsmState<T>[m_states.Count];
            foreach (var state in m_states)
            {
                results[index++] = state.Value;
            }

            return results;
        }

        /// <summary>
        ///     获取有限状态机的所有状态。
        /// </summary>
        /// <param name="results">有限状态机的所有状态。</param>
        public void GetAllStates( List<FsmState<T>> results )
        {
            if (results == null)
            {
                throw new SsitEngineException("Results is invalid.");
            }

            results.Clear();
            foreach (var state in m_states)
            {
                results.Add(state.Value);
            }
        }

        /// <summary>
        ///     抛出有限状态机事件。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="eventId">事件编号。</param>
        public void FireEvent( object sender, int eventId )
        {
            if (CurrentState == null)
            {
                throw new SsitEngineException("Current state is invalid.");
            }

            CurrentState.OnEvent(this, sender, eventId, null);
        }

        /// <summary>
        ///     抛出有限状态机事件。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="eventId">事件编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void FireEvent( object sender, int eventId, object userData )
        {
            if (CurrentState == null)
            {
                throw new SsitEngineException("Current state is invalid.");
            }

            CurrentState.OnEvent(this, sender, eventId, userData);
        }

        /// <summary>
        ///     是否存在有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>有限状态机数据是否存在。</returns>
        public bool HasData( string name )
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new SsitEngineException("Data name is invalid.");
            }

            return m_datas.ContainsKey(name);
        }

        /// <summary>
        ///     获取有限状态机数据。
        /// </summary>
        /// <typeparam name="TData">要获取的有限状态机数据的类型。</typeparam>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>要获取的有限状态机数据。</returns>
        public TData GetData<TData>( string name ) where TData : AllocatedObject
        {
            return (TData) GetData(name);
        }

        /// <summary>
        ///     获取有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>要获取的有限状态机数据。</returns>
        public AllocatedObject GetData( string name )
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new SsitEngineException("Data name is invalid.");
            }

            AllocatedObject data = null;
            if (m_datas.TryGetValue(name, out data))
            {
                return data;
            }

            return null;
        }

        /// <summary>
        ///     设置有限状态机数据。
        /// </summary>
        /// <typeparam name="TData">要设置的有限状态机数据的类型。</typeparam>
        /// <param name="name">有限状态机数据名称。</param>
        /// <param name="data">要设置的有限状态机数据。</param>
        public void SetData<TData>( string name, TData data ) where TData : AllocatedObject
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new SsitEngineException("Data name is invalid.");
            }

            m_datas[name] = data;
        }

        /// <summary>
        ///     设置有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <param name="data">要设置的有限状态机数据。</param>
        public void SetData( string name, AllocatedObject data )
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new SsitEngineException("Data name is invalid.");
            }

            m_datas[name] = data;
        }

        /// <summary>
        ///     移除有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>是否移除有限状态机数据成功。</returns>
        public bool RemoveData( string name )
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new SsitEngineException("Data name is invalid.");
            }

            return m_datas.Remove(name);
        }

        /// <summary>
        ///     切换当前有限状态机状态。
        /// </summary>
        /// <param name="stateId">要切换到的有限状态机状态Id。</param>
        public bool ChangeState( int stateId )
        {
            if (CurrentState == null)
            {
                throw new SsitEngineException("Current state is invalid.");
            }

            var state = GetState(stateId);
            if (state == null)
            {
                throw new SsitEngineException(TextUtils.Format(
                    "FSM '{0}' can not change state to '{1}' which is not exist.", TextUtils.GetFullName<T>(Name),
                    stateId));
            }

            var canChange = true;

            //检测状态条件
            if (OnStateCheckHandler != null)
            {
                canChange = OnStateCheckHandler(Owner, CurrentState.GetStateId(), stateId);
            }
            if (canChange)
            {
                var lastStateId = CurrentState.GetStateId();
                CurrentState.OnExit(this, false);
                m_currentStateTime = 0f;
                CurrentState = state;
                CurrentState.OnEnter(this);

                if (OnStateChangedHandler != null)
                {
                    OnStateChangedHandler(Owner, lastStateId, stateId);
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     有限状态机轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        public override void OnUpdate( float elapseSeconds )
        {
            if (CurrentState == null)
            {
                return;
            }

            m_currentStateTime += elapseSeconds;
            CurrentState.OnUpdate(this, elapseSeconds);
        }

        /// <summary>
        ///     关闭并清理有限状态机。
        /// </summary>
        public override void Shutdown()
        {
            if (CurrentState != null)
            {
                CurrentState.OnExit(this, true);
                CurrentState = null;
                m_currentStateTime = 0f;
            }

            foreach (var state in m_states)
            {
                state.Value.OnDestroy(this);
            }

            m_states.Clear();
            m_datas.Clear();

            OnStateChangedHandler = null;
            OnStateCheckHandler = null;

            m_isDestroyed = true;
        }

        private void OnEvent( IFsm<T> fsm, object sender, object userdata )
        {
            throw new NotImplementedException();
        }


        #region 非安全接口暴漏针对外部切换的使用

        /*public void ForceChangeState( int stateId )
        {
            if (m_currentState == null)
            {
                throw new SsitEngineException("Current state is invalid. not execute start method");
            }
            m_currentStateTime = 0f;
            FsmState<T> state = GetState(stateId);
            if (state == null)
            {
                throw new SsitEngineException(TextUtils.Format("FSM '{0}' can not change state to '{1}' which is not exist.", TextUtils.GetFullName<T>(Name), stateId));
            }
            m_currentState = state;
        }

        public void ForceEnterState( FsmState<T> state)
        {
            state.OnEnter(this);
        }

        public void ForceUpdateState( FsmState<T> state ,float elapsed)
        {
            state.OnUpdate(this, elapsed);
        }

        public void ForceExitState( FsmState<T> state ,bool isShutdown)
        {
            state.OnExit(this, isShutdown);
        }*/

        #endregion
    }
}