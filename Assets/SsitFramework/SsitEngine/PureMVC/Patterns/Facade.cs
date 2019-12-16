using System;
using System.Text;
using SsitEngine.Core;
using SsitEngine.Core.ReferencePool;
using SsitEngine.PureMVC.Core;
using SsitEngine.PureMVC.Interfaces;

namespace SsitEngine.PureMVC.Patterns
{
    /// <summary>
    ///     程序包装器
    /// </summary>
    public class Facade : IFacade
    {
        /// <summary>
        ///     静态对象实例的线程对象锁
        /// </summary>
        protected static readonly object StaticSyncRoot = new object();

        /// <summary>
        ///     包装器的静态实例对象
        /// </summary>
        protected static volatile IFacade instance;

        /// <summary>
        ///     MVC C层接口对象
        /// </summary>
        protected IController controller;

        /// <summary>
        ///     MVC M层接口对象
        /// </summary>
        protected IModel model;

        /// <summary>
        ///     MVC V层接口对象
        /// </summary>
        protected IView view;

        /// <summary>
        ///     创建程序外包装器
        /// </summary>
        protected Facade()
        {
            InitializeFacade();
        }

        /// <summary>
        ///     包装器的静态实例对象属性
        /// </summary>
        public static IFacade Instance
        {
            get
            {
                if (instance == null)
                    lock (StaticSyncRoot)
                    {
                        if (instance == null)
                            instance = new Facade();
                    }
                return instance;
            }
        }

        /// <summary>
        ///     外部获取实例接口
        /// </summary>
        /// <param name="facadeFunc">包装器的实例委托</param>
        /// <returns>外包装器</returns>
        public static IFacade GetInstance( Func<IFacade> facadeFunc )
        {
            if (instance == null) instance = facadeFunc();
            return instance;
        }

        /// <summary>
        ///     调试信息
        /// </summary>
        /// <returns></returns>
        public virtual string ToDebug()
        {
            var sb = new StringBuilder();
            sb.AppendLine(((View) view).ToDebug());
            sb.AppendLine(((Model) model).ToDebug());
            sb.AppendLine(((Controller) controller).ToDebug());
            return sb.ToString();
        }

        #region Proxy

        /// <summary>
        ///     Register an <c>IProxy</c> with the <c>Model</c> by name.
        /// </summary>
        /// <param name="proxy">the <c>IProxy</c> instance to be registered with the <c>Model</c>.</param>
        public virtual void RegisterProxy( IProxy proxy )
        {
            model.RegisterProxy(proxy);
        }

        /// <summary>
        ///     Retrieve an <c>IProxy</c> from the <c>Model</c> by name.
        /// </summary>
        /// <param name="proxyName">the name of the proxy to be retrieved.</param>
        /// <returns>the <c>IProxy</c> instance previously registered with the given <c>proxyName</c>.</returns>
        public virtual IProxy RetrieveProxy( string proxyName )
        {
            return model.RetrieveProxy(proxyName);
        }

        /// <summary>
        ///     Remove an <c>IProxy</c> from the <c>Model</c> by name.
        /// </summary>
        /// <param name="proxyName">the <c>IProxy</c> to remove from the <c>Model</c>.</param>
        /// <returns>the <c>IProxy</c> that was removed from the <c>Model</c></returns>
        public virtual IProxy RemoveProxy( string proxyName )
        {
            return model.RemoveProxy(proxyName);
        }

        /// <summary>
        ///     Check if a Proxy is registered
        /// </summary>
        /// <param name="proxyName"></param>
        /// <returns>whether a Proxy is currently registered with the given <c>proxyName</c>.</returns>
        public virtual bool HasProxy( string proxyName )
        {
            return model.HasProxy(proxyName);
        }

        #endregion

        #region Command

        /// <inheritdoc />
        public void RegisterCommand( ushort msgId, Type commandType )
        {
            controller.RegisterCommand(msgId, commandType);
        }

        /// <inheritdoc />
        public void RegisterCommand<T>( ushort msgId, T commandType, bool isAddPool = false )
            where T : SimpleCommand, new()
        {
            controller.RegisterCommand(msgId, commandType, isAddPool);
        }

        /// <summary>
        ///     Register an <c>ICommand</c> with the <c>Controller</c> by Notification name.
        /// </summary>
        /// <param name="msgId">the name of the <c>INotification</c> to associate the <c>ICommand</c> with</param>
        /// <param name="commandFunc">a reference to the Class of the <c>ICommand</c></param>
        public virtual void RegisterCommand( ushort msgId, Func<ICommand> commandFunc )
        {
            controller.RegisterCommand(msgId, commandFunc);
        }

        /// <summary>
        ///     Remove a previously registered <c>ICommand</c> to <c>INotification</c> mapping from the Controller.
        /// </summary>
        /// <param name="msgId">the name of the <c>INotification</c> to remove the <c>ICommand</c> mapping for</param>
        public virtual void RemoveCommand( ushort msgId )
        {
            controller.RemoveCommand(msgId);
        }

        /// <summary>
        ///     Check if a Command is registered for a given Notification
        /// </summary>
        /// <param name="msgId"></param>
        /// <returns>whether a Command is currently registered for the given <c>msgId</c>.</returns>
        public virtual bool HasCommand( ushort msgId )
        {
            return controller.HasCommand(msgId);
        }

        #endregion

        #region Mediator

        /// <summary>
        ///     Register a <c>IMediator</c> with the <c>View</c>.
        /// </summary>
        /// <param name="mediator">a reference to the <c>IMediator</c></param>
        public virtual void RegisterMediator( IMediator mediator )
        {
            view.RegisterMediator(mediator);
        }

        /// <summary>
        ///     Retrieve an <c>IMediator</c> from the <c>View</c>.
        /// </summary>
        /// <param name="mediatorName"></param>
        /// <returns>the <c>IMediator</c> previously registered with the given <c>mediatorName</c>.</returns>
        public virtual IMediator RetrieveMediator( string mediatorName )
        {
            return view.RetrieveMediator(mediatorName);
        }

        /// <summary>
        ///     Remove an <c>IMediator</c> from the <c>View</c>.
        /// </summary>
        /// <param name="mediatorName">name of the <c>IMediator</c> to be removed.</param>
        /// <returns>the <c>IMediator</c> that was removed from the <c>View</c></returns>
        public virtual IMediator RemoveMediator( string mediatorName )
        {
            return view.RemoveMediator(mediatorName);
        }

        /// <summary>
        ///     Check if a Mediator is registered or not
        /// </summary>
        /// <param name="mediatorName"></param>
        /// <returns>whether a Mediator is registered with the given <c>mediatorName</c>.</returns>
        public virtual bool HasMediator( string mediatorName )
        {
            return view.HasMediator(mediatorName);
        }

        #endregion

        #region Observers

        /// <inheritdoc />
        public virtual void RegisterObservers( object sender, ushort msgId, SsitAction<INotification> hander )
        {
            view.RegisterObserver(msgId, new Observer(sender, hander));
        }

        /// <inheritdoc />
        public virtual void RemoveObservers( object sender, ushort msgId )
        {
            view.RemoveObserver(msgId, sender);
        }

        #endregion

        #region Notify

        /// <inheritdoc />
        public void NotifyObservers( INotification notification )
        {
            view.NotifyObservers(notification);
        }

        /// <summary>
        ///     发布消息
        /// </summary>
        /// <param name="msgId">消息id</param>
        public void SendNotification( ushort msgId )
        {
            var args = ReferencePool.Acquire<MvEventArgs>();
            args.SetEventArgs(msgId);
            NotifyObservers(args);
        }

        /// <summary>
        ///     发布消息
        /// </summary>
        /// <param name="msgId">消息id</param>
        /// <param name="body">消息体</param>
        public void SendNotification( ushort msgId, object body )
        {
            var args = ReferencePool.Acquire<MvEventArgs>();
            args.SetEventArgs(msgId, body);
            NotifyObservers(args);
        }

        /// <summary>
        ///     发布消息
        /// </summary>
        /// <param name="msgId">消息id</param>
        /// <param name="body">消息体</param>
        /// <param name="values">可变消息参数组</param>
        public void SendNotification( ushort msgId, object body, params object[] values )
        {
            var args = ReferencePool.Acquire<MvEventArgs>();
            args.SetEventArgs(msgId, body, values);
            NotifyObservers(args);
        }

        #endregion

        #region Initialize

        /// <summary>
        ///     初始化包装器
        /// </summary>
        protected virtual void InitializeFacade()
        {
            InitializeModel();
            InitializeController();
            InitializeView();
        }

        /// <summary>
        ///     初始化控制器
        /// </summary>
        protected virtual void InitializeController()
        {
            if (controller != null)
                return;
            controller = Controller.Instance;
        }

        /// <summary>
        ///     初始化模型
        /// </summary>
        protected virtual void InitializeModel()
        {
            if (model != null)
                return;
            model = Model.Instance;
        }

        /// <summary>
        ///     初始化视图
        /// </summary>
        protected virtual void InitializeView()
        {
            if (view != null)
                return;
            view = View.Instance;
        }

        #endregion
    }
}