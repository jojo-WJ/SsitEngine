/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/18 16:51:31                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Diagnostics;

namespace SsitEngine.DebugLog
{
    /// <summary>
    ///     游戏框架日志类。
    /// </summary>
    public static partial class SsitDebug
    {
        private static ILogHelper s_logHelper;

        /// <summary>
        ///     设置游戏框架日志辅助器。
        /// </summary>
        /// <param name="logHelper">要设置的游戏框架日志辅助器。</param>
        public static void SetLogHelper( ILogHelper logHelper )
        {
            s_logHelper = logHelper;
        }

        /// <summary>
        ///     记录调试级别日志，用于记录调试类日志信息。
        /// </summary>
        /// <param name="message">日志内容。</param>
        /// <remarks>仅在带有 DEBUG 预编译选项时生效。</remarks>
        [Conditional("DEBUG")]
        public static void Debug( object message )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Debug, message);
        }

        /// <summary>
        ///     记录调试级别日志，用于记录调试类日志信息。
        /// </summary>
        /// <param name="message">日志内容。</param>
        /// <remarks>仅在带有 DEBUG 预编译选项时生效。</remarks>
        [Conditional("DEBUG")]
        public static void Debug( string message )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Debug, message);
        }

        /// <summary>
        ///     记录调试级别日志，用于记录调试类日志信息。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="arg0">日志参数 0。</param>
        /// <remarks>仅在带有 DEBUG 预编译选项时生效。</remarks>
        [Conditional("DEBUG")]
        public static void Debug( string format, object arg0 )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Debug, TextUtils.Format(format, arg0));
        }

        /// <summary>
        ///     记录调试级别日志，用于记录调试类日志信息。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="arg0">日志参数 0。</param>
        /// <param name="arg1">日志参数 1。</param>
        /// <remarks>仅在带有 DEBUG 预编译选项时生效。</remarks>
        [Conditional("DEBUG")]
        public static void Debug( string format, object arg0, object arg1 )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Debug, TextUtils.Format(format, arg0, arg1));
        }

        /// <summary>
        ///     记录调试级别日志，用于记录调试类日志信息。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="arg0">日志参数 0。</param>
        /// <param name="arg1">日志参数 1。</param>
        /// <param name="arg2">日志参数 2。</param>
        /// <remarks>仅在带有 DEBUG 预编译选项时生效。</remarks>
        [Conditional("DEBUG")]
        public static void Debug( string format, object arg0, object arg1, object arg2 )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Debug, TextUtils.Format(format, arg0, arg1, arg2));
        }

        /// <summary>
        ///     记录调试级别日志，用于记录调试类日志信息。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="args">日志参数。</param>
        /// <remarks>仅在带有 DEBUG 预编译选项时生效。</remarks>
        [Conditional("DEBUG")]
        public static void Debug( string format, params object[] args )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Debug, TextUtils.Format(format, args));
        }

        /// <summary>
        ///     打印信息级别日志，用于记录程序正常运行日志信息。
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void Info( object message )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Info, message);
        }

        /// <summary>
        ///     打印信息级别日志，用于记录程序正常运行日志信息。
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void Info( string message )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Info, message);
        }

        /// <summary>
        ///     打印信息级别日志，用于记录程序正常运行日志信息。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="arg0">日志参数 0。</param>
        public static void Info( string format, object arg0 )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Info, TextUtils.Format(format, arg0));
        }

        /// <summary>
        ///     打印信息级别日志，用于记录程序正常运行日志信息。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="arg0">日志参数 0。</param>
        /// <param name="arg1">日志参数 1。</param>
        public static void Info( string format, object arg0, object arg1 )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Info, TextUtils.Format(format, arg0, arg1));
        }

        /// <summary>
        ///     打印信息级别日志，用于记录程序正常运行日志信息。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="arg0">日志参数 0。</param>
        /// <param name="arg1">日志参数 1。</param>
        /// <param name="arg2">日志参数 2。</param>
        public static void Info( string format, object arg0, object arg1, object arg2 )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Info, TextUtils.Format(format, arg0, arg1, arg2));
        }

        /// <summary>
        ///     打印信息级别日志，用于记录程序正常运行日志信息。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="args">日志参数。</param>
        public static void Info( string format, params object[] args )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Info, TextUtils.Format(format, args));
        }

        /// <summary>
        ///     打印警告级别日志，建议在发生局部功能逻辑错误，但尚不会导致游戏崩溃或异常时使用。
        /// </summary>
        /// <param name="message">日志内容。</param>
        public static void Warning( object message )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Warning, message);
        }

        /// <summary>
        ///     打印警告级别日志，建议在发生局部功能逻辑错误，但尚不会导致游戏崩溃或异常时使用。
        /// </summary>
        /// <param name="message">日志内容。</param>
        public static void Warning( string message )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Warning, message);
        }

        /// <summary>
        ///     打印警告级别日志，建议在发生局部功能逻辑错误，但尚不会导致游戏崩溃或异常时使用。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="arg0">日志参数 0。</param>
        public static void Warning( string format, object arg0 )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Warning, TextUtils.Format(format, arg0));
        }

        /// <summary>
        ///     打印警告级别日志，建议在发生局部功能逻辑错误，但尚不会导致游戏崩溃或异常时使用。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="arg0">日志参数 0。</param>
        /// <param name="arg1">日志参数 1。</param>
        public static void Warning( string format, object arg0, object arg1 )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Warning, TextUtils.Format(format, arg0, arg1));
        }

        /// <summary>
        ///     打印警告级别日志，建议在发生局部功能逻辑错误，但尚不会导致游戏崩溃或异常时使用。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="arg0">日志参数 0。</param>
        /// <param name="arg1">日志参数 1。</param>
        /// <param name="arg2">日志参数 2。</param>
        public static void Warning( string format, object arg0, object arg1, object arg2 )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Warning, TextUtils.Format(format, arg0, arg1, arg2));
        }

        /// <summary>
        ///     打印警告级别日志，建议在发生局部功能逻辑错误，但尚不会导致游戏崩溃或异常时使用。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="args">日志参数。</param>
        public static void Warning( string format, params object[] args )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Warning, TextUtils.Format(format, args));
        }

        /// <summary>
        ///     打印错误级别日志，建议在发生功能逻辑错误，但尚不会导致游戏崩溃或异常时使用。
        /// </summary>
        /// <param name="message">日志内容。</param>
        public static void Error( object message )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Error, message);
        }

        /// <summary>
        ///     打印错误级别日志，建议在发生功能逻辑错误，但尚不会导致游戏崩溃或异常时使用。
        /// </summary>
        /// <param name="message">日志内容。</param>
        public static void Error( string message )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Error, message);
        }

        /// <summary>
        ///     打印错误级别日志，建议在发生功能逻辑错误，但尚不会导致游戏崩溃或异常时使用。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="arg0">日志参数 0。</param>
        public static void Error( string format, object arg0 )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Error, TextUtils.Format(format, arg0));
        }

        /// <summary>
        ///     打印错误级别日志，建议在发生功能逻辑错误，但尚不会导致游戏崩溃或异常时使用。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="arg0">日志参数 0。</param>
        /// <param name="arg1">日志参数 1。</param>
        public static void Error( string format, object arg0, object arg1 )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Error, TextUtils.Format(format, arg0, arg1));
        }

        /// <summary>
        ///     打印错误级别日志，建议在发生功能逻辑错误，但尚不会导致游戏崩溃或异常时使用。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="arg0">日志参数 0。</param>
        /// <param name="arg1">日志参数 1。</param>
        /// <param name="arg2">日志参数 2。</param>
        public static void Error( string format, object arg0, object arg1, object arg2 )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Error, TextUtils.Format(format, arg0, arg1, arg2));
        }

        /// <summary>
        ///     打印错误级别日志，建议在发生功能逻辑错误，但尚不会导致游戏崩溃或异常时使用。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="args">日志参数。</param>
        public static void Error( string format, params object[] args )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Error, TextUtils.Format(format, args));
        }

        /// <summary>
        ///     打印严重错误级别日志，建议在发生严重错误，可能导致游戏崩溃或异常时使用，此时应尝试重启进程或重建游戏框架。
        /// </summary>
        /// <param name="message">日志内容。</param>
        public static void Fatal( object message )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Fatal, message);
        }

        /// <summary>
        ///     打印严重错误级别日志，建议在发生严重错误，可能导致游戏崩溃或异常时使用，此时应尝试重启进程或重建游戏框架。
        /// </summary>
        /// <param name="message">日志内容。</param>
        public static void Fatal( string message )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Fatal, message);
        }

        /// <summary>
        ///     打印严重错误级别日志，建议在发生严重错误，可能导致游戏崩溃或异常时使用，此时应尝试重启进程或重建游戏框架。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="arg0">日志参数 0。</param>
        public static void Fatal( string format, object arg0 )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Fatal, TextUtils.Format(format, arg0));
        }

        /// <summary>
        ///     打印严重错误级别日志，建议在发生严重错误，可能导致游戏崩溃或异常时使用，此时应尝试重启进程或重建游戏框架。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="arg0">日志参数 0。</param>
        /// <param name="arg1">日志参数 1。</param>
        public static void Fatal( string format, object arg0, object arg1 )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Fatal, TextUtils.Format(format, arg0, arg1));
        }

        /// <summary>
        ///     打印严重错误级别日志，建议在发生严重错误，可能导致游戏崩溃或异常时使用，此时应尝试重启进程或重建游戏框架。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="arg0">日志参数 0。</param>
        /// <param name="arg1">日志参数 1。</param>
        /// <param name="arg2">日志参数 2。</param>
        public static void Fatal( string format, object arg0, object arg1, object arg2 )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Fatal, TextUtils.Format(format, arg0, arg1, arg2));
        }

        /// <summary>
        ///     打印严重错误级别日志，建议在发生严重错误，可能导致游戏崩溃或异常时使用，此时应尝试重启进程或重建游戏框架。
        /// </summary>
        /// <param name="format">日志格式。</param>
        /// <param name="args">日志参数。</param>
        public static void Fatal( string format, params object[] args )
        {
            if (s_logHelper == null) return;

            s_logHelper.Log(DebugLogLevel.Fatal, TextUtils.Format(format, args));
        }
    }
}