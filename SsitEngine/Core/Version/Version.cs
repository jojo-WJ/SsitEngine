/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：静态版本管理器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/19 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine
{
    /// <summary>
    ///     版本号类。
    /// </summary>
    public static partial class Version
    {
        private const string SsitEngineVersionString = "1.0.0";

        private static IVersionHelper s_versionHelper;

        /// <summary>
        ///     获取游戏框架版本号。
        /// </summary>
        public static string SsitEngineVersion => SsitEngineVersionString;

        /// <summary>
        ///     获取游戏版本号。
        /// </summary>
        public static string AppVersion
        {
            get
            {
                if (s_versionHelper == null) return string.Empty;

                return s_versionHelper.AppVersion;
            }
        }

        /// <summary>
        ///     获取内部游戏版本号。
        /// </summary>
        public static int InternalGameVersion
        {
            get
            {
                if (s_versionHelper == null) return 0;

                return s_versionHelper.InternalGameVersion;
            }
        }

        /// <summary>
        ///     设置版本号辅助器。
        /// </summary>
        /// <param name="versionHelper">要设置的版本号辅助器。</param>
        public static void SetVersionHelper( IVersionHelper versionHelper )
        {
            s_versionHelper = versionHelper;
        }
    }
}