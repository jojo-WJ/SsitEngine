/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：时间计时器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/20 17:36:33                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Core.TaskPool;
using SsitEngine.DebugLog;
using SsitEngine.PureMVC.Interfaces;

namespace SsitEngine.Unity.Timer
{
    /// <summary>
    ///     计时器管理器
    /// </summary>
    public class TimerManager : ManagerBase<TimerManager>
    {
        /// <summary>
        ///     暂停计时器
        /// </summary>
        protected bool isPause;

        private TaskPool<TimerEventTask> m_taskPool;
        private float m_timeSinceStartup;

        /// <summary>
        ///     获取计时器请求代理总数量。
        /// </summary>
        public int TotalAgentCount => m_taskPool.TotalAgentCount;

        /// <summary>
        ///     获取可用计时器代理数量。
        /// </summary>
        public int FreeAgentCount => m_taskPool.FreeAgentCount;

        /// <summary>
        ///     获取工作中计时器代理代理数量。
        /// </summary>
        public int WaitingTaskCount => m_taskPool.WaitingTaskCount;

        /// <summary>
        ///     获取工作中计时器代理代理数量。
        /// </summary>
        public int WorkingAgentCount => m_taskPool.WorkingAgentCount;

        #region 单列接口实现

        /// <inheritdoc />
        public override void OnSingletonInit()
        {
            m_taskPool = new TaskPool<TimerEventTask>();
            Reset();
        }

        #endregion

        /// <summary>
        ///     添加时间事件回调不经过消息系统
        /// </summary>
        /// <param name="task">计时器任务</param>
        /// <returns></returns>
        protected internal TimerEventTask AddTimerEvent( TimerEventTask task )
        {
            if (task.TimerType == TimerEventType.TeveNone ||
                task.TimerType == TimerEventType.TeveOnce && task.Duration == 0)
            {
                if (task.Handler != null)
                {
                    task.Handler(null, 0, task.Data);
                }
                return null;
            }

            AddTimerEventTask(task);
            return task;
        }

        /// <summary>
        ///     添加计时器任务
        /// </summary>
        /// <param name="task">计时器任务</param>
        public void AddTimerEventTask( TimerEventTask task )
        {
            if (task == null)
            {
                return;
            }


            m_taskPool.AddTask(task);

            // 计时器不等同于可以等待的机制处理，他需要即时性的一些概念
            if (WaitingTaskCount > FreeAgentCount)
            {
                if (TotalAgentCount > Engine.Instance.PlatConfig.timeTaskAgentMaxCount)
                {
                    SsitDebug.Error("计时器代理超出平台配置的最大个数。。。");
                    return;
                }
                AddTimerEventAgent();
            }
        }

        /// <summary>
        ///     添加计时器代理
        /// </summary>
        private void AddTimerEventAgent()
        {
            var agent = new TimerEventAgent();
            m_taskPool.AddAgent(agent);
        }

        /// <summary>
        ///     移除计时器任务。
        /// </summary>
        /// <param name="serialId">要移除计时器任务的序列编号。</param>
        /// <returns>是否移除计时器任务成功。</returns>
        protected internal bool RemoveTimerEventTask( ulong serialId )
        {
            if (m_taskPool == null)
            {
                return false;
            }
            return m_taskPool.RemoveTask(serialId) != null;
        }

        /// <summary>
        ///     移除计时器任务。
        /// </summary>
        /// <param name="task">要移除计时器任务。</param>
        /// <returns>是否移除计时器任务成功。</returns>
        protected internal bool RemoveTimerEventTask( TimerEventTask task )
        {
            if (m_taskPool == null)
            {
                return false;
            }
            return m_taskPool.RemoveTask(task);
        }

        /// <summary>
        ///     移除所有请求任务。
        /// </summary>
        public void RemoveAllWebRequests()
        {
            if (m_taskPool == null)
            {
                return;
            }
            m_taskPool.RemoveAllTasks();
        }

        /// <summary>
        ///     计时器重置
        /// </summary>
        public void Reset()
        {
            m_timeSinceStartup = 0;
        }

        #region 模块接口实现

        /// <inheritdoc />
        public override string ModuleName => typeof(TimerManager).FullName;

        /// <summary>
        ///     计时器模块优先级
        /// </summary>
        public override int Priority => (int) EnModuleType.ENMODULETIMER;

        /// <summary>
        ///     计时器刷新
        /// </summary>
        /// <param name="elapsed"></param>
        public override void OnUpdate( float elapsed )
        {
            if (elapsed < 1e-3f)
            {
                if (elapsed < 0f)
                {
                    SsitDebug.Debug("Timer is error." + "Timer::update");
                }
                return;
            }
            if (GameTime.IsPaused)
            {
                return;
            }
            m_timeSinceStartup += elapsed;
            m_taskPool.Update(elapsed, m_timeSinceStartup);
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            Reset();
            m_taskPool.RemoveAllTasks();
            m_taskPool = null;
        }

        #endregion

        #region 消息处理

        private void OnEnable()
        {
            m_msgList = new[]
            {
                (ushort) EnTimerEvent.AddTimerEvent,
                (ushort) EnTimerEvent.RemoveTimerEvent
            };
            RegisterMsg(m_msgList);
        }

        private void OnDisable()
        {
            UnRegisterMsg(m_msgList);
        }

        /// <inheritdoc />
        public override void HandleNotification( INotification notification )
        {
            switch (notification.Id)
            {
                case (ushort) EnTimerEvent.AddTimerEvent:
                {
                    var eve = notification.Body as TimerEventTask;
                    AddTimerEventTask(eve);
                }
                    break;
                case (ushort) EnTimerEvent.RemoveTimerEvent:
                {
                    var eve = notification.Body as TimerEventTask;
                    RemoveTimerEventTask(eve);
                }
                    break;
            }
        }

        #endregion
    }
}