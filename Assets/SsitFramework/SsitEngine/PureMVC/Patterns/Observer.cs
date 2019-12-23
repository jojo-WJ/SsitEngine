using SsitEngine.Core;
using SsitEngine.PureMVC.Interfaces;

namespace SsitEngine.PureMVC.Patterns
{
    /// <summary>
    ///     A base <c>IObserver</c> implementation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         An <c>Observer</c> is an object that encapsulates information
    ///         about an interested object with a method that should
    ///         be called when a particular <c>INotification</c> is broadcast.
    ///     </para>
    ///     <para>
    ///         In PureMVC, the <c>Observer</c> class assumes these responsibilities:
    ///         <list type="bullet">
    ///             <item>Encapsulate the notification (callback) method of the interested object.</item>
    ///             <item>Encapsulate the notification context (this) of the interested object.</item>
    ///             <item>Provide methods for setting the notification method and context.</item>
    ///             <item>Provide a method for notifying the interested object.</item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <seealso cref="PureMVC.Core.View" />
    /// <seealso cref="NotificationBase" />
    public class Observer : IObserver
    {
        /// <summary>
        ///     观察者的线程对象锁
        /// </summary>
        protected readonly object m_syncRoot = new object();

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The notification method on the interested object should take
        ///         one parameter of type <c>INotification</c>
        ///     </para>
        /// </remarks>
        /// <param name="notifyMethod">the notification method of the interested object</param>
        /// <param name="notifyContext">the notification context of the interested object</param>
        public Observer( object notifyContext, SsitAction<INotification> notifyMethod )
        {
            NotifyMethod = notifyMethod;
            NotifyContext = notifyContext;
        }

        /// <summary>
        ///     回调方法
        /// </summary>
        public SsitAction<INotification> NotifyMethod { get; set; }

        /// <summary>
        ///     上下文对象
        /// </summary>
        public object NotifyContext { get; set; }

        /// <summary>
        ///     Notify the interested object.
        /// </summary>
        /// <param name="Notification">the <c>INotification</c> to pass to the interested object's notification method.</param>
        public virtual void NotifyObserver( INotification Notification )
        {
            NotifyMethod(Notification);
        }

        /// <summary>
        ///     Compare an object to the notification context.
        /// </summary>
        /// <param name="obj">the object to compare</param>
        /// <returns>indicating if the object and the notification context are the same</returns>
        public bool CompareNotifyContext( object obj )
        {
            lock (m_syncRoot)
            {
                if (obj is IObserver observer)
                {
                    return NotifyContext.Equals(observer.NotifyContext) && NotifyMethod.Equals(observer.NotifyMethod);
                }
                return NotifyContext.Equals(obj);
            }
        }
    }
}