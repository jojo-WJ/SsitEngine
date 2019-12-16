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
    public static partial class Version
    {
        /// <summary>
        ///     版本号辅助器接口。
        /// </summary>
        public interface IVersionHelper
        {
            /// <summary>
            ///     获取游戏版本号。
            /// </summary>
            string AppVersion { get; }

            /// <summary>
            ///     获取内部游戏版本号。
            /// </summary>
            int InternalGameVersion { get; }
        }
    }
}