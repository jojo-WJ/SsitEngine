/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：系统进程                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/1 18:30:09                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace Framework.Procedure
{
    /// <summary>
    /// 流程类型
    /// </summary>
    public enum ENProcedureType
    {
        /// <summary>
        /// 加载流程
        /// </summary>
        ProcedureLoading = 1,

        /// <summary>
        /// 开始流程
        /// </summary>
        ProcedureStartUp,

        /// <summary>
        /// 登陆流程
        /// </summary>
        ProcedureLogin,

        /// <summary>
        /// 主流程
        /// </summary>
        ProcedureMain,
        ProcedureEmergency,
        ProcedureRestore,
        ProcedureReplay,
        /// <summary>
        /// 单人技能培训进程
        /// </summary>
        ProcedureSinglePlayer
    }

    public enum ENProcedureStatu
    {
        AssetLoading = 0,
        AssetLoaded,
        SceneLoading,
        SceneLoaded,
        AllPlayerLoaded,
    }
}