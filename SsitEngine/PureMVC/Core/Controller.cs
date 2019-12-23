using System;
using System.Collections.Generic;
using System.Text;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;

namespace SsitEngine.PureMVC.Core
{
    /// <summary>
    ///     控制层
    /// </summary>
    public class Controller : IController
    {
        /// <summary>
        ///     单列构造线程锁
        /// </summary>
        protected static readonly object m_staticSyncRoot = new object();

        /// <summary>Singleton instance</summary>
        protected static IController instance;

        /// <summary>Mapping of Notification names to Command Class references</summary>
        protected readonly Dictionary<ushort, Func<ICommand>> commandMap;

        /// <summary>
        ///     多线程锁
        /// </summary>
        protected readonly object m_syncRoot = new object();

        /// <summary>Local reference to View</summary>
        protected IView view;

        /// <summary>
        ///     创建MVC的C层对象
        /// </summary>
        protected Controller()
        {
            commandMap = new Dictionary<ushort, Func<ICommand>>();
            InitializeController();
        }

        /// <summary>
        ///     MVC 控制C层的实例
        /// </summary>
        public static IController Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (m_staticSyncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new Controller();
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        ///     Register a particular <c>ICommand</c> class as the handler
        ///     for a particular <c>INotification</c>.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If a <c>ICommand</c> has already been registered to
        ///         handle <c>INotification</c>s with this name, it is no longer
        ///         used, the new <c>Func</c> is used instead.
        ///     </para>
        ///     <para>
        ///         The Observer for the new ICommand is only created if this the
        ///         first time an ICommand has been regisered for this Notification name.
        ///     </para>
        /// </remarks>
        /// <param name="notificationName">the name of the <c>INotification</c></param>
        /// <param name="commandFunc">the <c>Func Delegate</c> of the <c>ICommand</c></param>
        public virtual void RegisterCommand( ushort notificationName, Func<ICommand> commandFunc )
        {
            lock (m_syncRoot)
            {
                if (!commandMap.ContainsKey(notificationName))
                {
                    view.RegisterObserver(notificationName, new Observer(this, ExecuteCommand));
                }
                commandMap[notificationName] = commandFunc;
            }
        }

        /// <summary>
        ///     注册命令（已弃用）
        /// </summary>
        /// <typeparam name="T">命令类型</typeparam>
        /// <param name="notificationName">消息名称</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="isAddPool">添加到池</param>
        public virtual void RegisterCommand<T>( ushort notificationName, T commandType, bool isAddPool = false )
            where T : SimpleCommand, new()
        {
        }

        /// <summary>
        ///     注册命令（已弃用）
        /// </summary>
        /// <param name="notificationName">消息名称</param>
        /// <param name="commandType">消息类型</param>
        public virtual void RegisterCommand( ushort notificationName, Type commandType )
        {
            lock (m_syncRoot)
            {
                if (!commandMap.ContainsKey(notificationName))
                {
                    view.RegisterObserver(notificationName, new Observer(this, ExecuteCommand));
                }
                commandMap[notificationName] = () => { return Activator.CreateInstance(commandType) as ICommand; };
            }
        }

        /// <summary>
        ///     If an <c>ICommand</c> has previously been registered
        ///     to handle a the given <c>INotification</c>, then it is executed.
        /// </summary>
        /// <param name="notification">note an <c>INotification</c></param>
        public virtual void ExecuteCommand( INotification notification )
        {
            Func<ICommand> commandFunc = null;
            lock (m_syncRoot)
            {
                if (commandMap.TryGetValue(notification.Id, out commandFunc))
                {
                    var commandInstance = commandFunc();
                    commandInstance.Execute(notification);
                }
            }
        }

        /// <summary>
        ///     Remove a previously registered <c>ICommand</c> to <c>INotification</c> mapping.
        /// </summary>
        /// <param name="notificationName">the name of the <c>INotification</c> to remove the <c>ICommand</c> mapping for</param>
        public virtual void RemoveCommand( ushort notificationName )
        {
            lock (m_syncRoot)
            {
                if (commandMap.ContainsKey(notificationName))
                {
                    view.RemoveObserver(notificationName, this);
                    commandMap.Remove(notificationName);
                }
            }
        }

        /// <summary>
        ///     Check if a Command is registered for a given Notification
        /// </summary>
        /// <param name="notificationName">消息名称/id</param>
        /// <returns>whether a Command is currently registered for the given <c>notificationName</c>.</returns>
        public virtual bool HasCommand( ushort notificationName )
        {
            lock (m_syncRoot)
            {
                return commandMap.ContainsKey(notificationName);
            }
        }

        /// <summary>
        ///     Initialize the Singleton <c>Controller</c> instance
        /// </summary>
        /// <remarks>
        ///     <para>Called automatically by the constructor</para>
        ///     <para>
        ///         Please aware that if you are using a subclass of <c>View</c>
        ///         in your application, you should also subclass <c>Controller</c>
        ///         and override the <c>initializeController</c> method in the following way:
        ///     </para>
        ///     <example>
        ///         <code>
        ///             // ensure that the Controller is talking to my IView implementation
        ///             public override void initializeController()
        ///             {
        ///                 view = MyView.getInstance(() => new MyView());
        ///             }
        ///         </code>
        ///     </example>
        /// </remarks>
        protected virtual void InitializeController()
        {
            view = View.Instance;
        }

        /// <summary>
        ///     C层调式使用
        /// </summary>
        /// <returns>调试信息</returns>
        public virtual string ToDebug()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-----------------commands-----------------");
            lock (m_syncRoot)
            {
                sb.AppendLine(commandMap.KeysToString());
            }
            sb.AppendLine("----------------- control end-----------------");
            return sb.ToString();
        }
    }
}