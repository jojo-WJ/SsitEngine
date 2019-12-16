/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：有限状态委托                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/18 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Fsm
{
    /// <summary>
    ///     有限状态机事件响应函数。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    /// <param name="fsm">有限状态机引用。</param>
    /// <param name="sender">事件源。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void FsmEventHandler<T>( IFsm<T> fsm, object sender, object userData ) where T : class;

    /// <summary>
    ///     有限状态机事件响应函数。
    /// </summary>
    /// <param name="sender">事件源。</param>
    /// <param name="preState">上个状态。</param>
    /// <param name="curState">当前状态。</param>
    public delegate void FsmStateChangeHandler<T>( T sender, int preState, int curState ) where T : class;

    /// <summary>
    ///     有限状态机事件响应函数。
    /// </summary>
    /// <param name="sender">事件源。</param>
    /// <param name="curState">当前状态。</param>
    /// <param name="tarState">目标状态。</param>
    public delegate bool FsmStateCheckHandler( object sender, int curState, int tarState );
}