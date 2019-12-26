/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/1 10:30:49                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.Procedure
{
    /// <summary>
    ///     有限状态机事件响应函数。
    /// </summary>
    /// <param name="procedureManager">进程管理器。</param>
    /// <param name="sender">事件源。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void ProcedureEventHandler( IProcedureManager procedureManager, object sender, object userData );
}