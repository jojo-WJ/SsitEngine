/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/26 20:19:32                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Runtime.InteropServices;

namespace SsitEngine
{
    /// <summary>
    ///     框架工具类相关常量配置
    /// </summary>
    public static class SsitFrameUtils
    {
        //全局常量

        /// <summary>
        ///     内核版本
        /// </summary>
        public const string VersionId = "1.1.0";

        /// <summary>
        ///     消息间隔
        /// </summary>
        public const int MsgSpan = 2000;

        /// <summary>
        ///     默认优先级
        /// </summary>
        public const int DefaultPriority = 0;

        /// <summary>
        ///     获取对象内存地址
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string GetMemoryAddress( object o ) // 获取引用类型的内存地址方法    
        {
            var h = GCHandle.Alloc(o, GCHandleType.WeakTrackResurrection);
            var addr = GCHandle.ToIntPtr(h);
            return " 0x" + addr.ToString("X");
        }
    }
}