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
    public static partial class SsitDebug
    {
        /// <summary>
        ///     游戏框架日志辅助器接口。
        /// </summary>
        public interface ILogHelper
        {
            /// <summary>
            ///     记录日志。
            /// </summary>
            /// <param name="level">游戏框架日志等级。</param>
            /// <param name="message">日志内容。</param>
            void Log( DebugLogLevel level, object message );
        }
    }
}