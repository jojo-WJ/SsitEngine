/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：框架入口                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/18 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace SsitEngine.Fsm
{
    /// <summary>
    ///     有限状态机接口。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    public interface IFsm<T> where T : class
    {
        /// <summary>
        ///     获取有限状态机名称。
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     获取有限状态机持有者。
        /// </summary>
        T Owner { get; }

        /// <summary>
        ///     获取有限状态机中状态的数量。
        /// </summary>
        int FsmStateCount { get; }

        /// <summary>
        ///     获取有限状态机是否正在运行。
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        ///     获取有限状态机是否被销毁。
        /// </summary>
        bool IsDestroyed { get; }

        /// <summary>
        ///     获取当前有限状态机状态。
        /// </summary>
        FsmState<T> CurrentState { get; }

        /// <summary>
        ///     获取当前有限状态机状态持续时间。
        /// </summary>
        float CurrentStateTime { get; }

        /// <summary>
        ///     开始有限状态机。
        /// </summary>
        /// <param name="stateId">要开始的有限状态机状态Id。</param>
        void Start( int stateId );

        /// <summary>
        ///     是否存在有限状态机状态。
        /// </summary>
        /// <param name="stateId">要检查的有限状态机状态Id。</param>
        /// <returns>是否存在有限状态机状态。</returns>
        bool HasState( int stateId );

        /// <summary>
        ///     获取有限状态机状态。
        /// </summary>
        /// <param name="stateId">要获取的有限状态机状态Id。</param>
        /// <returns>要获取的有限状态机状态。</returns>
        FsmState<T> GetState( int stateId );

        /// <summary>
        ///     获取有限状态机的所有状态。
        /// </summary>
        /// <returns>有限状态机的所有状态。</returns>
        FsmState<T>[] GetAllStates();

        /// <summary>
        ///     获取有限状态机的所有状态。
        /// </summary>
        /// <param name="results">有限状态机的所有状态。</param>
        void GetAllStates( List<FsmState<T>> results );

        /// <summary>
        ///     切换当前有限状态机状态。
        /// </summary>
        /// <param name="stateId">要切换到的有限状态机状态Id。</param>
        bool ChangeState( int stateId );

        /// <summary>
        ///     抛出有限状态机事件。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="eventId">事件编号。</param>
        void FireEvent( object sender, int eventId );

        /// <summary>
        ///     抛出有限状态机事件。
        /// </summary>
        /// <param name="sender">事件源。</param>
        /// <param name="eventId">事件编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        void FireEvent( object sender, int eventId, object userData );

        /// <summary>
        ///     是否存在有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>有限状态机数据是否存在。</returns>
        bool HasData( string name );

        /// <summary>
        ///     获取有限状态机数据。
        /// </summary>
        /// <typeparam name="TData">要获取的有限状态机数据的类型。</typeparam>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>要获取的有限状态机数据。</returns>
        TData GetData<TData>( string name ) where TData : AllocatedObject;

        /// <summary>
        ///     获取有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>要获取的有限状态机数据。</returns>
        AllocatedObject GetData( string name );

        /// <summary>
        ///     设置有限状态机数据。
        /// </summary>
        /// <typeparam name="TData">要设置的有限状态机数据的类型。</typeparam>
        /// <param name="name">有限状态机数据名称。</param>
        /// <param name="data">要设置的有限状态机数据。</param>
        void SetData<TData>( string name, TData data ) where TData : AllocatedObject;

        /// <summary>
        ///     设置有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <param name="data">要设置的有限状态机数据。</param>
        void SetData( string name, AllocatedObject data );

        /// <summary>
        ///     移除有限状态机数据。
        /// </summary>
        /// <param name="name">有限状态机数据名称。</param>
        /// <returns>是否移除有限状态机数据成功。</returns>
        bool RemoveData( string name );
    }
}