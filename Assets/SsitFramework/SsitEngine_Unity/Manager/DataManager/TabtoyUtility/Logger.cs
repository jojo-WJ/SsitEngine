using SsitEngine.DebugLog;

namespace Tabtoy
{
    public class Logger
    {
        private static Logger _instance;

        private Logger()
        {
        }

        public static Logger Instance()
        {
            if (_instance == null)
            {
                _instance = new Logger();
            }
            return _instance;
        }

        private void WriteLine( DebugLogLevel level, string msg )
        {
            switch (level)
            {
                case DebugLogLevel.Info:
                    SsitDebug.Debug(LevelToString(level) + msg);
                    break;
                case DebugLogLevel.Warning:
                    SsitDebug.Warning(LevelToString(level) + msg);
                    break;
                case DebugLogLevel.Error:
                    SsitDebug.Error(LevelToString(level) + msg);
                    break;
                default:
                    SsitDebug.Debug(LevelToString(level) + msg);
                    break;
            }
        }

        public void DebugLine( string fmt, params object[] args )
        {
            var text = string.Format(fmt, args);

            WriteLine(DebugLogLevel.Debug, text);
        }

        public void InfoLine( string fmt, params object[] args )
        {
            var text = string.Format(fmt, args);

            WriteLine(DebugLogLevel.Info, text);
        }

        public void WarningLine( string fmt, params object[] args )
        {
            var text = string.Format(fmt, args);

            WriteLine(DebugLogLevel.Warning, text);
        }

        public void ErrorLine( string fmt, params object[] args )
        {
            var text = string.Format(fmt, args);

            WriteLine(DebugLogLevel.Error, text);
        }

        private string LevelToString( DebugLogLevel level )
        {
            switch (level)
            {
                case DebugLogLevel.Debug:
                    return "tabtoy [Debug] ";
                case DebugLogLevel.Info:
                    return "tabtoy [Info] ";
                case DebugLogLevel.Warning:
                    return "tabtoy [Warn] ";
                case DebugLogLevel.Error:
                    return "tabtoy [Error] ";
            }

            return "tabtoy [Unknown] ";
        }
    }
}