using System;
using SsitEngine.PureMVC.Patterns;

namespace SsitEngine.PureMVC.Interfaces
{
    /// <summary>
    ///     The interface definition for a PureMVC Controller.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         In PureMVC, an <c>IController</c> implementor
    ///         follows the 'Command and Controller' strategy, and
    ///         assumes these responsibilities:
    ///         <list type="bullet">
    ///             <item>
    ///                 Remembering which <c>ICommand</c>s
    ///                 are intended to handle which <c>INotifications</c>.
    ///             </item>
    ///             <item>
    ///                 Registering itself as an <c>IObserver</c> with
    ///                 the <c>View</c> for each <c>INotification</c>
    ///                 that it has an <c>ICommand</c> mapping for.
    ///             </item>
    ///             <item>
    ///                 Creating a new instance of the proper <c>ICommand</c>
    ///                 to handle a given <c>INotification</c> when notified by the <c>View</c>.
    ///             </item>
    ///             <item>
    ///                 Calling the <c>ICommand</c>'s <c>execute</c>
    ///                 method, passing in the <c>INotification</c>.
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <seealso cref="INotification" />
    /// <seealso cref="ICommand" />
    public interface IController
    {
        /// <summary>
        ///     Register a particular <c>ICommand</c> class as the handler
        ///     for a particular <c>INotification</c>.
        /// </summary>
        /// <param name="notificationName">the name of the <c>INotification</c></param>
        /// <param name="commandClassRef">the FuncDelegate of the <c>ICommand</c></param>
        void RegisterCommand( ushort notificationName, Func<ICommand> commandClassRef );

        /// <summary>
        ///     Register a Command by CommandType
        /// </summary>
        /// <param name="notificationName"></param>
        /// <param name="commandType"></param>
        void RegisterCommand( ushort notificationName, Type commandType );

        /// <summary>
        ///     Register a Command by CommandType
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="notificationName">通知名称</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="isAddPool">添加的命令池</param>
        void RegisterCommand<T>( ushort notificationName, T commandType, bool isAddPool )
            where T : SimpleCommand, new();

        /// <summary>
        ///     Execute the <c>ICommand</c> previously registered as the
        ///     handler for <c>INotification</c>s with the given notification name.
        /// </summary>
        /// <param name="notification">the <c>INotification</c> to execute the associated <c>ICommand</c> for</param>
        void ExecuteCommand( INotification notification );

        /// <summary>
        ///     Remove a previously registered <c>ICommand</c> to <c>INotification</c> mapping.
        /// </summary>
        /// <param name="notificationName">the name of the <c>INotification</c> to remove the <c>ICommand</c> mapping for</param>
        void RemoveCommand( ushort notificationName );

        /// <summary>
        ///     Check if a Command is registered for a given Notification
        /// </summary>
        /// <param name="notificationName">whether a Command is currently registered for the given <c>notificationName</c>.</param>
        /// <returns></returns>
        bool HasCommand( ushort notificationName );
    }
}