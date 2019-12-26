/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/29 14:29:06                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Core;
using SsitEngine.Core.ReferencePool;

namespace SsitEngine.Unity.Timer
{
    /// <summary>
    ///     计时器任务辅助器
    /// </summary>
    public class TimerEventTaskHelper : ITimerEventTaskHelper
    {
        /// <summary>
        ///     id构造器
        /// </summary>
        protected UlGuid ulGuidGenerator;

        /// <summary>
        ///     构造方法
        /// </summary>
        public TimerEventTaskHelper()
        {
            ulGuidGenerator = new UlGuid(1000);
        }

        /// <inheritdoc />
        public TimerEventTask CreateTimeEventTask( TimerEventType type, int priority, float second, float span,
            OnTimerEventHandler hander, object data = null )
        {
            if (type == TimerEventType.TeveNone || type == TimerEventType.TeveOnce && second == 0)
            {
                if (hander != null)
                {
                    hander(null, 0, data);
                }
                return null;
            }

            if (Engine.Instance.HasModule(typeof(TimerManager).FullName))
            {
                var task = ReferencePool.Acquire<TimerEventTask>();
                task.SetEventTask(ulGuidGenerator.GenerateNewId(), priority, type, second, span, hander, data);
                return task;
            }

            return null;
        }

        /// <inheritdoc />
        public bool RemoveTimeEventTask( TimerEventTask task )
        {
            if (task != null && Engine.Instance.HasModule(typeof(TimerManager).FullName))
            {
                TimerManager.Instance?.RemoveTimerEventTask(task);
                return true;
            }
            return false;
        }
    }
}