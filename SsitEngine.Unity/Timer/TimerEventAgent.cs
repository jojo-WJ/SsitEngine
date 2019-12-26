/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/29 12:16:45                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Core.ReferencePool;
using SsitEngine.Core.TaskPool;

namespace SsitEngine.Unity.Timer
{
    /// <summary>
    ///     计时任务代理
    /// </summary>
    public class TimerEventAgent : ITaskAgent<TimerEventTask>
    {
        /// <summary>
        ///     代理任务
        /// </summary>
        private TimerEventTask m_task;

        /// <summary>
        ///     构造计时任务代理
        /// </summary>
        public TimerEventAgent()
        {
            m_task = null;
        }

        /// <summary>
        ///     获取代理任务
        /// </summary>
        public TimerEventTask Task => m_task;

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public void Update( float elapseSeconds, float realElapseSeconds )
        {
            m_task.OnUpdate(elapseSeconds, realElapseSeconds);
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            if (m_task != null)
                // 回收引用
            {
                ReferencePool.Release((IReference) m_task);
            }
        }

        /// <inheritdoc />
        public void Start( TimerEventTask task, float elapseSeconds )
        {
            m_task = task;
            m_task.SetSrc(elapseSeconds);
        }

        /// <inheritdoc />
        public void Reset()
        {
            if (m_task != null)
            {
                m_task.Shutdown();
                m_task = null;
            }
        }
    }
}