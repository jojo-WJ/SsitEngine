/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/29 12:14:09                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.Timer
{
    /// <summary>
    ///     事件触发类型
    /// </summary>
    public enum TimerEventType
    {
        /// <summary>
        ///     无
        /// </summary>
        TeveNone = 0,

        /// <summary>
        ///     每帧触发
        /// </summary>
        TeveAlways,

        /// <summary>
        ///     在指定的时间延迟后触发一次
        /// </summary>
        TeveOnce,

        /// <summary>
        ///     在指定的时间段里间隔触发
        /// </summary>
        TeveSpanUntil,

        /// <summary>
        ///     延迟指定的时间段后连续间隔触发
        /// </summary>
        TeveDelaySpanAlways
    }
}