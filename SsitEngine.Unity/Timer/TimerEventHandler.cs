/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/20 17:36:33                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.Timer
{
    /// <summary>
    ///     时间计时器的响应方法
    /// </summary>
    /// <param name="eve">委托任务事件</param>
    /// <param name="timeElapsed">响应时长</param>
    /// <param name="data">用户自定义数据</param>
    public delegate void OnTimerEventHandler( TimerEventTask eve, float timeElapsed, object data );
}