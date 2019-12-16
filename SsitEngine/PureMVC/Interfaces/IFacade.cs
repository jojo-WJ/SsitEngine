using System;
using SsitEngine.Core;
using SsitEngine.PureMVC.Patterns;

namespace SsitEngine.PureMVC.Interfaces
{
    /// <summary>
    ///     The interface definition for a PureMVC Facade.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The Facade Pattern suggests providing a single
    ///         class to act as a central point of communication
    ///         for a subsystem.
    ///     </para>
    ///     <para>
    ///         In PureMVC, the Facade acts as an interface between
    ///         the core MVC actors (Model, View, Controller) and
    ///         the rest of your application.
    ///     </para>
    /// </remarks>
    /// <seealso cref="IModel" />
    /// <seealso cref="IView" />
    /// <seealso cref="IController" />
    /// <seealso cref="ICommand" />
    /// <seealso cref="INotification" />
    public interface IFacade : INotifier
    {
        /// <summary>
        ///     Register an <c>IProxy</c> with the <c>Model</c> by name.
        /// </summary>
        /// <param name="proxy">the <c>IProxy</c> to be registered with the <c>Model</c>.</param>
        void RegisterProxy( IProxy proxy );

        /// <summary>
        ///     Retrieve a <c>IProxy</c> from the <c>Model</c> by name.
        /// </summary>
        /// <param name="proxyName">the name of the <c>IProxy</c> instance to be retrieved.</param>
        /// <returns>the <c>IProxy</c> previously regisetered by <c>proxyName</c> with the <c>Model</c>.</returns>
        IProxy RetrieveProxy( string proxyName );

        /// <summary>
        ///     Remove an <c>IProxy</c> instance from the <c>Model</c> by name.
        /// </summary>
        /// <param name="proxyName">the <c>IProxy</c> to remove from the <c>Model</c>.</param>
        /// <returns>the <c>IProxy</c> that was removed from the <c>Model</c></returns>
        IProxy RemoveProxy( string proxyName );

        /// <summary>
        ///     Check if a Proxy is registered
        /// </summary>
        /// <param name="proxyName"></param>
        /// <returns>whether a Proxy is currently registered with the given <c>proxyName</c>.</returns>
        bool HasProxy( string proxyName );

        /// <summary>
        ///     注册消息id对应的命令类型
        /// </summary>
        /// <param name="msgId">消息id</param>
        /// <param name="commandType">命令类型</param>
        void RegisterCommand( ushort msgId, Type commandType );

        /// <summary>
        ///     Register an <c>ICommand</c> with the <c>Controller</c>.
        /// </summary>
        /// <param name="msgId">the name of the <c>INotification</c> to associate the <c>ICommand</c> with.</param>
        /// <param name="commandClassRef">a reference to the <c>FuncDelegate</c> of the <c>ICommand</c></param>
        void RegisterCommand( ushort msgId, Func<ICommand> commandClassRef );

        /// <summary>
        ///     注册命令
        /// </summary>
        /// <typeparam name="T">指定的命令类型</typeparam>
        /// <param name="msgId">消息id</param>
        /// <param name="commandType">命令的getType（）</param>
        /// <param name="isAddPool">是否添加到命令池</param>
        void RegisterCommand<T>( ushort msgId, T commandType, bool isAddPool ) where T : SimpleCommand, new();

        /// <summary>
        ///     Remove a previously registered <c>ICommand</c> to <c>INotification</c> mapping from the Controller.
        /// </summary>
        /// <param name="msgId">the name of the <c>INotification</c> to remove the <c>ICommand</c> mapping for</param>
        void RemoveCommand( ushort msgId );

        /// <summary>
        ///     Check if a Command is registered for a given Notification
        /// </summary>
        /// <param name="msgId"></param>
        /// <returns>whether a Command is currently registered for the given <c>msgId</c>.</returns>
        bool HasCommand( ushort msgId );

        /// <summary>
        ///     Register an <c>IMediator</c> instance with the <c>View</c>.
        /// </summary>
        /// <param name="mediator">a reference to the <c>IMediator</c> instance</param>
        void RegisterMediator( IMediator mediator );

        /// <summary>
        ///     Retrieve an <c>IMediator</c> instance from the <c>View</c>.
        /// </summary>
        /// <param name="mediatorName">the name of the <c>IMediator</c> instance to retrievve</param>
        /// <returns>the <c>IMediator</c> previously registered with the given <c>mediatorName</c>.</returns>
        IMediator RetrieveMediator( string mediatorName );

        /// <summary>
        ///     Remove a <c>IMediator</c> instance from the <c>View</c>.
        /// </summary>
        /// <param name="mediatorName">name of the <c>IMediator</c> instance to be removed</param>
        /// <returns>the <c>IMediator</c> instance previously registered with the given <c>mediatorName</c>.</returns>
        IMediator RemoveMediator( string mediatorName );

        /// <summary>
        ///     Check if a Mediator is registered or not
        /// </summary>
        /// <param name="mediatorName"></param>
        /// <returns>whether a Mediator is registered with the given <c>mediatorName</c>.</returns>
        bool HasMediator( string mediatorName );

        /// <summary>
        ///     订阅事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msgId"></param>
        /// <param name="hander"></param>
        void RegisterObservers( object sender, ushort msgId, SsitAction<INotification> hander );

        /// <summary>
        ///     取消订阅事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msgId"></param>
        void RemoveObservers( object sender, ushort msgId );

        /// <summary>
        ///     Notify <c>Observer</c>s.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This method is left public mostly for backward
        ///         compatibility, and to allow you to send custom
        ///         notification classes using the facade.
        ///     </para>
        ///     <para>
        ///         Usually you should just call sendNotification
        ///         and pass the parameters, never having to
        ///         construct the notification yourself.
        ///     </para>
        /// </remarks>
        /// <param name="notification">the <c>INotification</c> to have the <c>View</c> notify <c>Observers</c> of.</param>
        void NotifyObservers( INotification notification );
    }
}