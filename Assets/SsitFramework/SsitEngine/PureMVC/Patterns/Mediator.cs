using System.Collections.Generic;
using SsitEngine.PureMVC.Interfaces;

namespace SsitEngine.PureMVC.Patterns
{
    /// <summary>
    ///     视图中介对象
    /// </summary>
    public class Mediator : Notifier, IMediator
    {
        /// <summary>
        ///     中介对象的名称
        /// </summary>
        public const string NAME = "Mediator";


        /// <summary>
        ///     创建一个中介对象
        /// </summary>
        public Mediator() : this("Mediator", null)
        {
        }

        /// <summary>
        ///     创建一个中介对象
        /// </summary>
        /// <param name="mediatorName">中介名称</param>
        public Mediator( string mediatorName ) : this(mediatorName, null)
        {
        }

        /// <summary>
        ///     创建一个中介对象
        /// </summary>
        /// <param name="mediatorName">中介名称</param>
        /// <param name="view">视图对象</param>
        public Mediator( string mediatorName, object view = null )
        {
            MediatorName = mediatorName ?? NAME;
            ViewComponent = view;
        }

        /// <summary>the mediator name</summary>
        public string MediatorName { get; protected set; }

        /// <summary>
        ///     用此种方式合理规避拆装箱
        /// </summary>
        public object ViewComponent { get; set; }

        /// <summary>
        ///     消息注册列表
        /// </summary>
        /// <returns></returns>
        public virtual IList<ushort> ListNotificationInterests()
        {
            return new ushort[0];
        }

        /// <summary>
        ///     消息回调处理
        /// </summary>
        /// <param name="notification"></param>
        public virtual void HandleNotification( INotification notification )
        {
        }

        /// <summary>
        ///     注册
        /// </summary>
        public virtual void OnRegister()
        {
        }

        /// <summary>
        ///     移除
        /// </summary>
        public virtual void OnRemove()
        {
        }
    }
}