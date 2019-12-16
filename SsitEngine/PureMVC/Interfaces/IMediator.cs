using System.Collections.Generic;

namespace SsitEngine.PureMVC.Interfaces
{
    /// <summary>
    ///     视图中介层接口
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        ///     Get or Set the <c>IMediator</c> instance name
        /// </summary>
        string MediatorName { get; }

        /// <summary>
        ///     Get or Set the <c>IMediator</c>'s view component.
        /// </summary>
        object ViewComponent { get; set; }


        /// <summary>
        ///     List <c>INotification</c> interests.
        /// </summary>
        /// <returns> an <c>Array</c> of the <c>INotification</c> names this <c>IMediator</c> has an interest in.</returns>
        IList<ushort> ListNotificationInterests();

        /// <summary>
        ///     Handle an <c>INotification</c>.
        /// </summary>
        /// <param name="notification">notification the <c>INotification</c> to be handled</param>
        void HandleNotification( INotification notification );

        /// <summary>
        ///     Called by the View when the Mediator is registered
        /// </summary>
        void OnRegister();

        /// <summary>
        ///     Called by the View when the Mediator is removed
        /// </summary>
        void OnRemove();
    }
}