/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/18 16:02:54                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity
{
    /// <summary>
    ///     模块类型
    /// </summary>
    public enum EnModuleType
    {
        /// <summary>
        ///     计时器模块
        /// </summary>
        ENMODULETIMER = 10,

        ENMODULEDATA = 15,

        /// <summary>
        ///     对象池模块
        /// </summary>
        ENMODULEOBJECTPOOL = 99,

        /// <summary>
        ///     框架基础模块
        /// </summary>
        ENMODULEBASE = 100,
        ENMODULERESOURCE = 200,

        /// <summary>
        ///     场景实体模块
        /// </summary>
        ENMODULEENTITY = 300,
        ENMODULEUI = 400,

        ENMODULEDEFAULT = 800,
        ENMODULECUSTOM1 = 1000,
        ENMODULECUSTOM2 = 2000,
        ENMODULECUSTOM3 = 3000,
        ENMODULECUSTOM4 = 4000,
        ENMODULECUSTOM5 = 5000,
        ENMODULECUSTOM6 = 6000,

        /// <summary>
        ///     进程模块
        /// </summary>
        ENMODULEPROCEDUAL = 9999
    }
}