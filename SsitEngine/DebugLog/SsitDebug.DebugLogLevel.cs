/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/18 16:51:31                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.DebugLog
{
    /// <summary>
    ///     游戏框架日志等级。
    /// </summary>
    public enum DebugLogLevel
    {
        /// <summary>
        ///     调试。
        /// </summary>
        Debug,

        /// <summary>
        ///     信息。
        /// </summary>
        Info,

        /// <summary>
        ///     警告。
        /// </summary>
        Warning,

        /// <summary>
        ///     错误。
        /// </summary>
        Error,

        /// <summary>
        ///     严重错误。
        /// </summary>
        Fatal
    }
}