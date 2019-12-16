/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：系统流程管理器（处理系统主状态、场景加载、UI管理、音效管理、资源释放管理）                                                   
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/18 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Diagnostics;

namespace SsitEngine.Unity.Procedure
{
    /// <summary>
    ///     流程管理器。
    /// </summary>
    public sealed class ProcedureManager : ManagerBase<ProcedureManager>, IProcedureManager
    {
        private readonly Dictionary<int, ProcedureBase> m_procedureMaps;

        //当前进程
        private ProcedureBase m_currProcedure;

        //下一个进程

        /// <summary>
        ///     初始化流程管理器的新实例。
        /// </summary>
        public ProcedureManager()
        {
            m_currProcedure = null;
            NextProcedure = null;
            m_procedureMaps = new Dictionary<int, ProcedureBase>();
            NextProcedure = null;
        }

        /// <summary>
        ///     获取当前流程。
        /// </summary>
        public ProcedureBase CurrentProcedure
        {
            get
            {
                if (m_currProcedure == null) throw new SsitEngineException("You must initialize procedure first.");

                return m_currProcedure;
            }
        }

        /// <summary>
        ///     获取上个进程
        /// </summary>
        public ProcedureBase NextProcedure { get; private set; }

        /// <summary>
        ///     初始化流程管理器。
        /// </summary>
        /// <param name="procedures">流程管理器包含的流程。</param>
        public void Initialize( params ProcedureBase[] procedures )
        {
            for (var i = 0; i < procedures.Length; i++)
            {
                var stateId = procedures[i].StateId;
                if (m_procedureMaps.ContainsKey(stateId))
                    throw new SsitEngineException(TextUtils.Format("流程管理器已经包含流程ID{0}", stateId));
                procedures[i].OnInit(this);
                m_procedureMaps.Add(stateId, procedures[i]);
            }
        }

        /// <summary>
        ///     是否存在流程。
        /// </summary>
        /// <param name="procedureId">要检查的流程类型。</param>
        /// <returns>是否存在流程。</returns>
        public bool HasProcedure( int procedureId )
        {
            return m_procedureMaps.ContainsKey(procedureId);
        }

        /// <summary>
        ///     获取流程。
        /// </summary>
        /// <typeparam name="T">要获取的流程类型。</typeparam>
        /// <returns>要获取的流程。</returns>
        public ProcedureBase GetProcedure<T>( int procedureId ) where T : ProcedureBase
        {
            return GetState(procedureId) as T;
        }

        /// <summary>
        ///     抛出有限状态机事件。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="eventId">事件编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void FireEvent( object sender, int eventId, object userData = null )
        {
            if (m_currProcedure == null) throw new SsitEngineException("Current state is invalid.");
            m_currProcedure.OnEvent(this, sender, eventId, userData);
        }

        /// <summary>
        ///     进程转换（进程Type）
        /// </summary>
        /// <param name="procedureId">进程Id</param>
        /// <param name="sceneName">进程切换场景名称</param>
        public void StartProcedure( int procedureId, string sceneName = null )
        {
            var procedure = GetState(procedureId);
            if (procedure == null)
                throw new SsitEngineException(TextUtils.Format(
                    "FSM '{0}' can not change procedure to '{1}' which is not exist.", ModuleName, procedureId));
            procedure.SceneName = sceneName ?? procedure.SceneName;
            NextProcedure = procedure;

            OnUpdate(0);
        }


        /// <summary>
        ///     进程转换（进程Type）
        /// </summary>
        /// <param name="procedureId">进程Id</param>
        /// <param name="sceneName">进程切换场景名称</param>
        public void ChangeProcedure( int procedureId, string sceneName = null )
        {
            var procedure = GetState(procedureId);
            if (procedure == null)
                throw new SsitEngineException(TextUtils.Format(
                    "FSM '{0}' can not change procedure to '{1}' which is not exist.", ModuleName, procedureId));
            procedure.SceneName = sceneName ?? procedure.SceneName;
            NextProcedure = procedure;
        }

        #region Internal Memebers

        /// <summary>
        ///     获取流程状态。
        /// </summary>
        /// <param name="stateId">要获取的流程状态类型。</param>
        /// <returns>要获取的流程状态。</returns>
        private ProcedureBase GetState( int stateId )
        {
            ProcedureBase state = null;
            if (m_procedureMaps.TryGetValue(stateId, out state)) return state;
            return null;
        }

        #endregion

        #region IModule 接口实现

        /// <summary>
        ///     获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public override int Priority => (int) EnModuleType.ENMODULEPROCEDUAL;


        /// <summary>
        ///     流程管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        public override void OnUpdate( float elapseSeconds )
        {
            if (null != m_currProcedure)
                if (m_currProcedure.IsLoaded)
                {
                    if (!m_currProcedure.IsEntered)
                    {
                        m_currProcedure.OnEnter(this);
                        return;
                    }
                    m_currProcedure.OnUpdate(this, elapseSeconds);
                }

            if (null != NextProcedure)
            {
                if (null != m_currProcedure)
                {
                    m_currProcedure.OnExit(this, false);
                    Engine.Instance.Platform.ReleaseResources();
                }
                //flag 上个进程
                //NextProcedure = m_currProcedure;
                m_currProcedure = NextProcedure;
                NextProcedure = null;
                Debug.Assert(CurrentProcedure != null, "当前进程不为空");
                m_currProcedure.OnPrestrain();
                Engine.Instance.Platform.StartPlatCoroutine(CurrentProcedure.StartLoading());
            }
        }

        /// <summary>
        ///     关闭并清理流程管理器。
        /// </summary>
        public override void Shutdown()
        {
            if (isShutdown)
                return;
            isShutdown = true;
            if (m_currProcedure != null)
            {
                m_currProcedure.OnExit(this, true);
                m_currProcedure = null;
            }

            foreach (var state in m_procedureMaps) state.Value.OnDestroy(this);
            m_procedureMaps.Clear();
        }

        #endregion
    }
}