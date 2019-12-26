/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：系统状态进程                                                   
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/18 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.Procedure
{
    /// <summary>
    ///     流程管理器接口。
    /// </summary>
    public interface IProcedureManager
    {
        /// <summary>
        ///     获取当前流程。
        /// </summary>
        ProcedureBase CurrentProcedure { get; }

        /// <summary>
        ///     获取要加载的进程
        /// </summary>
        ProcedureBase NextProcedure { get; }

        /*
        /// <summary>
        /// 获取当前流程持续时间。
        /// </summary>
        float CurrentProcedureTime
        {
            get;
        }
        */

        /// <summary>
        ///     初始化流程管理器。
        /// </summary>
        /// <param name="procedureFsm">有限状态机管理器。</param>
        void Initialize( params ProcedureBase[] procedureFsm );


        /// <summary>
        ///     是否存在流程。
        /// </summary>
        /// <param name="procedureId">要检查的流程类型。</param>
        /// <returns>是否存在流程。</returns>
        bool HasProcedure( int procedureId );

        /// <summary>
        ///     获取流程。
        /// </summary>
        /// <typeparam name="T">要获取的流程类型。</typeparam>
        /// <param name="procedureId">要获取的流程类型。</param>
        /// <returns>要获取的流程。</returns>
        ProcedureBase GetProcedure<T>( int procedureId ) where T : ProcedureBase;

        /// <summary>
        ///     抛出流程状态机事件。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="eventId">事件编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        void FireEvent( object sender, int eventId, object userData = null );
    }
}