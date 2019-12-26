/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/27 15:16:59                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.Timer
{
    /// <summary>
    ///     计时模块消息事件枚举
    /// </summary>
    public enum EnTimerEvent
    {
        /// <summary>
        ///     添加计时任务
        /// </summary>
        AddTimerEvent = EnMsgCenter.TimerEvent + 1,

        /// <summary>
        ///     移除计时任务
        /// </summary>
        RemoveTimerEvent,

        /// <summary>
        ///     事件标记位
        /// </summary>
        MaxValue
    }
}