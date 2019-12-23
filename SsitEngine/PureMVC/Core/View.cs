using System.Collections.Generic;
using System.Text;
using SsitEngine.Core;
using SsitEngine.Core.ReferencePool;
using SsitEngine.DebugLog;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;

namespace SsitEngine.PureMVC.Core
{
    /// <summary>
    ///     视图层
    /// </summary>
    public class View : IView
    {
        /// <summary>
        ///     视图静态实例的线程锁
        /// </summary>
        protected static readonly object StaticSyncRoot = new object();

        /// <summary>
        ///     视图静态实例对象
        /// </summary>
        protected static volatile IView instance;

        /// <summary>
        ///     视图操作的线程锁
        /// </summary>
        protected readonly object syncRoot = new object();

        /// <summary>
        ///     视图中介字典
        /// </summary>
        protected IDictionary<string, IMediator> mediatorMap;

        //protected IDictionary<ushort, IList<IObserver>> observerMap;

        /// <summary>
        ///     观察者字典链表
        /// </summary>
        /// <remarks>
        ///     1.数组利用下标定位，时间复杂度为O(1)，链表定位元素时间复杂度O(n)；
        ///     2.数组插入或删除元素的时间复杂度O( n)，链表的时间复杂度O(1)。
        ///     而且插入/删除元素时，为了保证元素的顺序性，必须对后面的元素进行移动。如果你的应用中需要频繁对元素进行插入/删除，那么开销会很大
        /// </remarks>
        protected IDictionary<ushort, LinkNode<IObserver>> observerMap;

        /// <summary>
        ///     创建视图对象
        /// </summary>
        protected View()
        {
            mediatorMap = new Dictionary<string, IMediator>();
            observerMap = new Dictionary<ushort, LinkNode<IObserver>>();
            //构造函数中执行虚方法，会在调用派生类型的构造函数之前，将执行派生类型中此虚拟成员的每个重写（注意：构造方法中的虚方法不能访问派生构造的初始化参数）;
            InitializeView();
        }

        /// <summary>
        ///     视图静态实例对象属性
        /// </summary>
        public static IView Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (StaticSyncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new View();
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        ///     注册观察者
        /// </summary>
        /// <param name="notificationName">消息id</param>
        /// <param name="observer">观察者</param>
        public virtual void RegisterObserver( ushort notificationName, IObserver observer )
        {
            lock (syncRoot)
            {
                if (!observerMap.ContainsKey(notificationName))
                {
                    //创建head节点
                    //observerMap[notificationName] = new LinkNode<IObserver>();
                    //添加next节点
                    observerMap[notificationName] = new LinkNode<IObserver>(observer);
                    //observerMap[notificationName].Next = new LinkNode<IObserver>(observer);
                }
                else
                {
                    if (HasObserver(notificationName, observer))
                    {
                        return;
                    }

                    var tempObserver = observerMap[notificationName];
                    while (tempObserver.Next != null)
                    {
                        tempObserver = tempObserver.Next;
                    }
                    tempObserver.Next = new LinkNode<IObserver>(observer);
                }
            }
        }

        /// <inheritdoc />
        public virtual void NotifyObservers( INotification notification )
        {
            lock (syncRoot)
            {
                LinkNode<IObserver> headNode = null;
                if (observerMap.TryGetValue(notification.Id, out headNode))
                {
                    //get first node
                    var curNode = headNode;
                    while (curNode != null)
                    {
                        // Notify Observers from the working link
                        if (curNode.Data == null)
                        {
                            SsitDebug.Error($"notification.Id {notification.Id} is exception");
                        }
                        curNode.Data?.NotifyObserver(notification);

                        curNode = curNode.Next;
                    }
                }
                // 回收引用
                ReferencePool.Release((IReference) notification);
            }
        }

        /// <inheritdoc />
        public virtual void RemoveObserver( ushort msgId, object notifyContext )
        {
            lock (syncRoot)
            {
                LinkNode<IObserver> topNode = null;

                if (observerMap.TryGetValue(msgId, out topNode))
                {
                    var headNode = topNode;

                    //判断头节点
                    LinkNode<IObserver> front = null;
                    //headNode = front.Next;

                    while (headNode != null)
                    {
                        if (headNode.Data.CompareNotifyContext(notifyContext))
                        {
                            if (front != null)
                            {
                                front.Next = headNode.Next;
                            }
                            else
                            {
                                topNode = headNode.Next;
                            }
                            headNode = headNode.Next;
                            continue;
                        }
                        front = headNode;
                        headNode = headNode.Next;
                    }

                    observerMap[msgId] = topNode;
                    // 仅剩头部节点释放整个Key
                    if (observerMap[msgId] == null)
                    {
                        observerMap.Remove(msgId);
                    }
                }
            }
        }

        /// <inheritdoc />
        public virtual void RegisterMediator( IMediator mediator )
        {
            lock (syncRoot)
            {
                if (mediatorMap.ContainsKey(mediator.MediatorName))
                {
                    return;
                }
                mediatorMap[mediator.MediatorName] = mediator;
                var interests = mediator.ListNotificationInterests();
                if (interests.Count > 0)
                {
                    // Create Observer referencing this mediator's handlNotification method
                    IObserver observer = new Observer(mediator, mediator.HandleNotification);

                    // Register Mediator as Observer for its list of Notification interests
                    for (var i = 0; i < interests.Count; i++)
                    {
                        RegisterObserver(interests[i], observer);
                    }
                }
            }
            mediator.OnRegister();
        }

        /// <summary>
        ///     获取对应名称的视图代理对象
        /// </summary>
        /// <param name="mediatorName">指定的中介者名称</param>
        /// <returns>中介对象</returns>
        public virtual IMediator RetrieveMediator( string mediatorName )
        {
            lock (syncRoot)
            {
                if (!mediatorMap.ContainsKey(mediatorName))
                {
                    return null;
                }
                return mediatorMap[mediatorName];
            }
        }

        /// <summary>
        ///     移除对应名称的视图代理对象
        /// </summary>
        /// <param name="mediatorName">指定的中介者名称</param>
        /// <returns>中介对象</returns>
        public virtual IMediator RemoveMediator( string mediatorName )
        {
            IMediator mediator = null;
            lock (syncRoot)
            {
                if (!mediatorMap.ContainsKey(mediatorName))
                {
                    return null;
                }
                mediator = mediatorMap[mediatorName];
                // for every notification this mediator is interested in...
                var interests = mediator.ListNotificationInterests();
                for (var i = 0; i < interests.Count; i++)
                    // remove the observer linking the mediator 
                    // to the notification interest
                {
                    RemoveObserver(interests[i], mediator);
                }
                mediatorMap.Remove(mediatorName);
            }
            mediator.OnRemove();
            return mediator;
        }

        /// <summary>
        ///     检测指定名称的中介对象man's
        /// </summary>
        /// <param name="mediatorName">指定的中介名称</param>
        /// <returns>是否存在指定名称的中介对象</returns>
        public virtual bool HasMediator( string mediatorName )
        {
            lock (syncRoot)
            {
                return mediatorMap.ContainsKey(mediatorName);
            }
        }

        /// <summary>
        ///     初始化视图对象
        /// </summary>
        protected virtual void InitializeView()
        {
        }

        /// <summary>
        ///     是否有相同观察者
        /// </summary>
        /// <param name="msgId">消息id</param>
        /// <param name="notifyContext">指定的观察者对象</param>
        /// <returns></returns>
        public bool HasObserver( ushort msgId, object notifyContext )
        {
            lock (syncRoot)
            {
                LinkNode<IObserver> topNode = null;

                if (observerMap.TryGetValue(msgId, out topNode))
                {
                    var headNode = topNode;

                    while (headNode != null)
                    {
                        if (headNode.Data.CompareNotifyContext(notifyContext))
                        {
                            return true;
                        }
                        headNode = headNode.Next;
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///     调试信息
        /// </summary>
        /// <returns>调试信息</returns>
        public virtual string ToDebug()
        {
            var sb = new StringBuilder();
            sb.AppendLine("-----------------mediators-----------------");
            lock (syncRoot)
            {
                sb.AppendLine(mediatorMap.KeysToString());
                sb.AppendLine("-----------------observers-----------------");
                sb.AppendLine(observerMap.KeysToString());
                sb.AppendLine("----------------- view end-----------------");
            }
            return sb.ToString();
        }
    }
}