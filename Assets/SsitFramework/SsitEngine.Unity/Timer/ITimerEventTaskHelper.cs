/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/29 14:32:25                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.Timer
{
    /// <summary>
    ///     计时器任务辅助器接口
    /// </summary>
    public interface ITimerEventTaskHelper
    {
        /// <summary>
        ///     创建计时器任务
        /// </summary>
        /// <param name="type">计时任务类型</param>
        /// <param name="priority">计时任务优先级</param>
        /// <param name="second">计时任务周期</param>
        /// <param name="span">计时任务间隔</param>
        /// <param name="func">计时回调</param>
        /// <param name="data">自定义数据</param>
        /// <returns>计时任务</returns>
        TimerEventTask CreateTimeEventTask( TimerEventType type, int priority, float second, float span,
            OnTimerEventHandler func, object data = null );

        /// <summary>
        ///     移除计时器任务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        bool RemoveTimeEventTask( TimerEventTask task );
    }
}