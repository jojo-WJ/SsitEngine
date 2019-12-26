/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：时间调度器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年6月3日                            
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Reflection;
using SsitEngine.Core.ReferencePool;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SsitEngine.Unity
{
    /// <summary>
    ///     ScheduledEventBase is reused by the Scheduler to store and call the scheduled delegate.
    /// </summary>
    public abstract class ScheduledEventBase
    {
        /// <summary>
        ///     Specifies where the event should be invoked.
        /// </summary>
        public enum InvokeLocation
        {
            Update, // Within MonoBehaviour.Update.
            FixedUpdate // Within MonoBehaviour.FixedUpdate.
        }

        protected float m_EndTime;
        protected InvokeLocation m_InvokeLocation;

        public float EndTime => m_EndTime;
        public InvokeLocation Location => m_InvokeLocation;
        public bool Active { get; set; }

        /// <summary>
        ///     Executes the delegate.
        /// </summary>
        public abstract void Invoke();

#if UNITY_EDITOR
        /// <summary>
        ///     Returns the target of the action. Used by the Scheduler inspector.
        /// </summary>
        /// <returns>The value of the target.</returns>
        public abstract object Target { get; }

        /// <summary>
        ///     Returns the Method of the action. Used by the Scheduler inspector.
        /// </summary>
        /// <returns>The info of the Method.</returns>
        public abstract MethodInfo Method { get; }
#endif
    }

    /// <summary>
    ///     Subclass of ScheduledEventBase whcih allows for execution of a delegate with a no parameters.
    /// </summary>
    public class ScheduledEvent : ScheduledEventBase, IReference
    {
        private Action m_Action;

        /// <summary>
        ///     释放引用对象
        /// </summary>
        public void Clear()
        {
            m_Action = null;
        }

        /// <summary>
        ///     Initializes the ScheduledEvent to the specified parameters. This method is used instead of a constructor because
        ///     the ScheduledEventBase is pooled.
        /// </summary>
        /// <param name="endTime">The time at which the event should be executed.</param>
        /// <param name="invokeLocation">Specifies where the event shoud be invoked.</param>
        /// <param name="action">The delegate to execute.</param>
        public void Initialize( float endTime, InvokeLocation invokeLocation, Action action )
        {
            m_InvokeLocation = invokeLocation;
            m_EndTime = endTime;
            m_Action = action;
        }

        /// <summary>
        ///     Executes the delegate.
        /// </summary>
        public override void Invoke()
        {
            m_Action();
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Returns the target of the action. Used by the Scheduler inspector.
        /// </summary>
        /// <returns>The value of the target.</returns>
        public override object Target => m_Action.Target;

        /// <summary>
        ///     Returns the Method of the action. Used by the Scheduler inspector.
        /// </summary>
        /// <returns>The info of the Method.</returns>
        public override MethodInfo Method => m_Action.Method;
#endif
    }

    /// <summary>
    ///     Subclass of ScheduledEventBase whcih allows for execution of a delegate with a single parameter.
    /// </summary>
    public class ScheduledEvent<T> : ScheduledEventBase, IReference
    {
        private Action<T> m_Action;
        private T m_Value;

        /// <summary>
        ///     释放引用对象
        /// </summary>
        public void Clear()
        {
            m_Action = null;
            m_Value = default;
        }

        /// <summary>
        ///     Initializes the ScheduledEvent to the specified parameters. This method is used instead of a constructor because
        ///     the ScheduledEventBase is pooled.
        /// </summary>
        /// <param name="endTime">The time at which the event should be executed.</param>
        /// <param name="invokeLocation">Specifies where the event shoud be invoked.</param>
        /// <param name="action">The delegate to execute.</param>
        /// <param name="value">The value to execute the delegate with.</param>
        public void Initialize( float endTime, InvokeLocation invokeLocation, Action<T> action, T value )
        {
            m_EndTime = endTime;
            m_InvokeLocation = invokeLocation;
            m_Action = action;
            m_Value = value;
        }

        /// <summary>
        ///     Executes the delegate.
        /// </summary>
        public override void Invoke()
        {
            m_Action(m_Value);
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Returns the target of the action. Used by the Scheduler inspector.
        /// </summary>
        /// <returns>The value of the target.</returns>
        public override object Target => m_Action.Target;

        /// <summary>
        ///     Returns the Method of the action. Used by the Scheduler inspector.
        /// </summary>
        /// <returns>The info of the Method.</returns>
        public override MethodInfo Method => m_Action.Method;
#endif
    }

    /// <summary>
    ///     Subclass of ScheduledEventBase whcih allows for execution of a delegate with two parameters.
    /// </summary>
    public class ScheduledEvent<T, U> : ScheduledEventBase, IReference
    {
        private Action<T, U> m_Action;
        private T m_Value1;
        private U m_Value2;

        /// <summary>
        ///     释放引用对象
        /// </summary>
        public void Clear()
        {
            m_Action = null;
            m_Value1 = default;
            m_Value2 = default;
        }

        /// <summary>
        ///     Initializes the ScheduledEvent to the specified parameters. This method is used instead of a constructor because
        ///     the ScheduledEventBase is pooled.
        /// </summary>
        /// <param name="endTime">The time at which the event should be executed.</param>
        /// <param name="invokeLocation">Specifies where the event shoud be invoked.</param>
        /// <param name="action">The delegate to execute.</param>
        /// <param name="value1">The value to execute the delegate with.</param>
        /// <param name="value2">The second value to execute the delegate with.</param>
        public void Initialize( float endTime, InvokeLocation invokeLocation, Action<T, U> action, T value1, U value2 )
        {
            m_EndTime = endTime;
            m_InvokeLocation = invokeLocation;
            m_Action = action;
            m_Value1 = value1;
            m_Value2 = value2;
        }

        /// <summary>
        ///     Executes the delegate.
        /// </summary>
        public override void Invoke()
        {
            m_Action(m_Value1, m_Value2);
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Returns the target of the action. Used by the Scheduler inspector.
        /// </summary>
        /// <returns>The value of the target.</returns>
        public override object Target => m_Action.Target;

        /// <summary>
        ///     Returns the Method of the action. Used by the Scheduler inspector.
        /// </summary>
        /// <returns>The info of the Method.</returns>
        public override MethodInfo Method => m_Action.Method;
#endif
    }

    /// <summary>
    ///     Subclass of ScheduledEventBase whcih allows for execution of a delegate with three parameters.
    /// </summary>
    public class ScheduledEvent<T, U, V> : ScheduledEventBase, IReference
    {
        private Action<T, U, V> m_Action;
        private T m_Value1;
        private U m_Value2;
        private V m_Value3;

        /// <summary>
        ///     释放引用对象
        /// </summary>
        public void Clear()
        {
            m_Action = null;
            m_Value1 = default;
            m_Value2 = default;
            m_Value3 = default;
        }

        /// <summary>
        ///     Initializes the ScheduledEvent to the specified parameters. This method is used instead of a constructor because
        ///     the ScheduledEventBase is pooled.
        /// </summary>
        /// <param name="endTime">The time at which the event should be executed.</param>
        /// <param name="invokeLocation">Specifies where the event shoud be invoked.</param>
        /// <param name="action">The delegate to execute.</param>
        /// <param name="value1">The value to execute the delegate with.</param>
        /// <param name="value2">The second value to execute the delegate with.</param>
        /// <param name="value3">The third value to execute the delegate with.</param>
        public void Initialize( float endTime, InvokeLocation invokeLocation, Action<T, U, V> action, T value1,
            U value2, V value3 )
        {
            m_EndTime = endTime;
            m_InvokeLocation = invokeLocation;
            m_Action = action;
            m_Value1 = value1;
            m_Value2 = value2;
            m_Value3 = value3;
        }

        /// <summary>
        ///     Executes the delegate.
        /// </summary>
        public override void Invoke()
        {
            m_Action(m_Value1, m_Value2, m_Value3);
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Returns the target of the action. Used by the Scheduler inspector.
        /// </summary>
        /// <returns>The value of the target.</returns>
        public override object Target => m_Action.Target;

        /// <summary>
        ///     Returns the Method of the action. Used by the Scheduler inspector.
        /// </summary>
        /// <returns>The info of the Method.</returns>
        public override MethodInfo Method => m_Action.Method;
#endif
    }

    /// <summary>
    ///     Executes an event at some point in the future.
    /// </summary>
    public class Scheduler : MonoBehaviour
    {
        private static Scheduler s_Instance;
        private static bool s_Initialized;

        [Tooltip("The maximum number of events that can be scheduled at any one time.")] [SerializeField]
        protected int m_MaxEventCount = 200;

        private static Scheduler Instance
        {
            get
            {
                if (!s_Initialized)
                {
                    s_Instance = new GameObject("Scheduler").AddComponent<Scheduler>();
                    s_Initialized = true;
                }
                return s_Instance;
            }
        }

        public ScheduledEventBase[] ActiveUpdateEvents { get; private set; }

        public int ActiveUpdateEventCount { get; private set; }

        public ScheduledEventBase[] ActiveFixedUpdateEvents { get; private set; }

        public int ActiveFixedUpdateEventCount { get; private set; }

        /// <summary>
        ///     Initialize the default values.
        /// </summary>
        private void Awake()
        {
            ActiveUpdateEvents = new ScheduledEventBase[m_MaxEventCount];
            ActiveFixedUpdateEvents = new ScheduledEventBase[m_MaxEventCount];
        }

        /// <summary>
        ///     The object has been enabled.
        /// </summary>
        private void OnEnable()
        {
            // The object may have been enabled outside of the scene unloading.
            if (s_Instance == null)
            {
                s_Instance = this;
                s_Initialized = true;
                SceneManager.sceneUnloaded -= SceneUnloaded;
            }
        }

        /// <summary>
        ///     Executes any event with a InvokeLocation of Update and the  current time is greater than or equal to the end time
        ///     of the ScheduledEventBase.
        /// </summary>
        private void Update()
        {
            var count = ActiveUpdateEventCount;
            for (var i = 0; i < count; ++i)
            {
                if (ActiveUpdateEvents[i].EndTime <= Time.time)
                {
                    var prevCount = ActiveUpdateEventCount;
                    Invoke(ActiveUpdateEvents[i], i);
                    // An event may have been removed because of the invoke. When the element is removed the next element replace it. The scheduler shouldn't
                    // skip over this element and should back up within the list by the number of elements removed.
                    if (ActiveUpdateEventCount < prevCount)
                    {
                        var diff = prevCount - ActiveUpdateEventCount;
                        count -= diff;
                        i = Mathf.Max(i - diff, 0);
                    }
                }
            }
        }

        /// <summary>
        ///     Executes any event with a InvokeLocation of FixedUpdate and the  current time is greater than or equal to the end
        ///     time of the ScheduledEventBase.
        /// </summary>
        private void FixedUpdate()
        {
            var count = ActiveFixedUpdateEventCount;
            for (var i = 0; i < count; ++i)
            {
                if (ActiveFixedUpdateEvents[i].EndTime <= Time.time)
                {
                    var prevCount = ActiveFixedUpdateEventCount;
                    Invoke(ActiveFixedUpdateEvents[i], i);
                    // An event may have been removed because of the invoke. When the element is removed the next element replace it. The scheduler shouldn't
                    // skip over this element and should back up within the list by the number of elements removed.
                    if (ActiveFixedUpdateEventCount < prevCount)
                    {
                        var diff = prevCount - ActiveFixedUpdateEventCount;
                        count -= diff;
                        i = Mathf.Max(i - diff, 0);
                    }
                }
            }
        }

        /// <summary>
        ///     Schedule a new event to occur after the specified delay within the Update loop.
        /// </summary>
        /// <param name="delay">The time to wait before triggering the event.</param>
        /// <param name="action">The event to occur.</param>
        /// <returns>The ScheduledEventBase instance, useful if the event should be cancelled.</returns>
        public static ScheduledEventBase Schedule( float delay, Action action )
        {
            // Objects may be wanting to be scheduled as the game is stopping but the Scheduler has already been destroyed. Ensure the Scheduler is still valid.
            if (Instance == null)
            {
                return null;
            }

            return Instance.AddEventInternal(delay, ScheduledEventBase.InvokeLocation.Update, action);
        }

        /// <summary>
        ///     Schedule a new event to occur after the specified delay within the FixedUpdate loop.
        /// </summary>
        /// <param name="delay">The time to wait before triggering the event.</param>
        /// <param name="action">The event to occur.</param>
        /// <returns>The ScheduledEventBase instance, useful if the event should be cancelled.</returns>
        public static ScheduledEventBase ScheduleFixed( float delay, Action action )
        {
            // Objects may be wanting to be scheduled as the game is stopping but the Scheduler has already been destroyed. Ensure the Scheduler is still valid.
            if (Instance == null)
            {
                return null;
            }

            return Instance.AddEventInternal(delay, ScheduledEventBase.InvokeLocation.FixedUpdate, action);
        }

        /// <summary>
        ///     Internal method which adds a new event to be executed in the future.
        /// </summary>
        /// <param name="delay">The delay from the current time to execute the event.</param>
        /// <param name="invokeLocation">Specifies where the event shoud be invoked.</param>
        /// <param name="action">The delegate to execute after the specified delay.</param>
        /// <returns>The ScheduledEventBase instance, useful if the event should be cancelled.</returns>
        private ScheduledEventBase AddEventInternal( float delay, ScheduledEventBase.InvokeLocation invokeLocation,
            Action action )
        {
            // Don't add the event if the game hasn't started.
            if (enabled == false)
            {
                return null;
            }

            if (delay == 0)
            {
                action();
                return null;
            }
            var scheduledEvent = ReferencePool.Acquire<ScheduledEvent>();
            // A delay of -1 indicates that the event reoccurs forever and must manually be cancelled.
            scheduledEvent.Initialize(delay == -1 ? -1 : Time.time + delay, invokeLocation, action);
            AddScheduledEvent(scheduledEvent);
            return scheduledEvent;
        }

        /// <summary>
        ///     Adds the scheduled event to the ActiveUpdateEvents or ActiveFixedUpdate events array.
        /// </summary>
        /// <param name="scheduledEvent">The scheduled event to add.</param>
        private void AddScheduledEvent( ScheduledEventBase scheduledEvent )
        {
            if (scheduledEvent.Location == ScheduledEventBase.InvokeLocation.Update)
            {
                if (ActiveUpdateEventCount >= m_MaxEventCount)
                {
                    Debug.LogError(
                        "Error: The ActiveEvents array is full so the new event cannot be added. The Scheduler.MaxEventCount value should be increased.");
                    return;
                }
                ActiveUpdateEvents[ActiveUpdateEventCount] = scheduledEvent;
                ActiveUpdateEventCount++;
            }
            else
            {
                // FixedUpdate.
                if (ActiveFixedUpdateEventCount >= m_MaxEventCount)
                {
                    Debug.LogError(
                        "Error: The ActiveEvents array is full so the new event cannot be added. The Scheduler.MaxEventCount value should be increased.");
                    return;
                }
                ActiveFixedUpdateEvents[ActiveFixedUpdateEventCount] = scheduledEvent;
                ActiveFixedUpdateEventCount++;
            }
            scheduledEvent.Active = true;
        }

        /// <summary>
        ///     Add a new event with an argument to be executed in the future within the Update loop.
        /// </summary>
        /// <param name="delay">The delay from the current time to execute the event.</param>
        /// <param name="action">The delegate to execute after the specified delay.</param>
        /// <param name="value">The value to use when invoking the delegate.</param>
        /// <returns>The ScheduledEventBase instance, useful if the event should be cancelled.</returns>
        public static ScheduledEventBase Schedule<T>( float delay, Action<T> action, T value )
        {
            // Objects may be wanting to be scheduled as the game is stopping but the Scheduler has already been destroyed. Ensure the Scheduler is still valid.
            if (Instance == null)
            {
                return null;
            }

            return Instance.AddEventInternal(delay, ScheduledEventBase.InvokeLocation.Update, action, value);
        }

        /// <summary>
        ///     Add a new event with an argument to be executed in the future within the FixedUpdate loop.
        /// </summary>
        /// <param name="delay">The delay from the current time to execute the event.</param>
        /// <param name="action">The delegate to execute after the specified delay.</param>
        /// <param name="value">The value to use when invoking the delegate.</param>
        /// <returns>The ScheduledEventBase instance, useful if the event should be cancelled.</returns>
        public static ScheduledEventBase ScheduleFixed<T>( float delay, Action<T> action, T value )
        {
            // Objects may be wanting to be scheduled as the game is stopping but the Scheduler has already been destroyed. Ensure the Scheduler is still valid.
            if (Instance == null)
            {
                return null;
            }

            return Instance.AddEventInternal(delay, ScheduledEventBase.InvokeLocation.FixedUpdate, action, value);
        }

        /// <summary>
        ///     Internal method which adds a new event with an argument to be executed in the future.
        /// </summary>
        /// <param name="delay">The delay from the current time to execute the event.</param>
        /// <param name="invokeLocation">Specifies where the event shoud be invoked.</param>
        /// <param name="action">The delegate to execute after the specified delay.</param>
        /// <param name="value">The value to use when invoking the delegate.</param>
        /// <returns>The ScheduledEventBase instance, useful if the event should be cancelled.</returns>
        private ScheduledEventBase AddEventInternal<T>( float delay, ScheduledEventBase.InvokeLocation invokeLocation,
            Action<T> action, T value )
        {
            if (delay == 0)
            {
                action(value);
                return null;
            }
            var scheduledEvent = ReferencePool.Acquire<ScheduledEvent<T>>();
            // A delay of -1 indicates that the event reoccurs forever and must manually be cancelled.
            scheduledEvent.Initialize(delay == -1 ? -1 : Time.time + delay, invokeLocation, action, value);
            AddScheduledEvent(scheduledEvent);
            return scheduledEvent;
        }

        /// <summary>
        ///     Add a new event with two arguments which will be executed in the future within the Update loop.
        /// </summary>
        /// <param name="delay">The delay from the current time to execute the event.</param>
        /// <param name="action">The delegate to execute after the specified delay.</param>
        /// <param name="value1">The first value to use when invoking the delegate.</param>
        /// <param name="value2">The second value to use when invoking the delegate.</param>
        /// <returns>The ScheduledEventBase instance, useful if the event should be cancelled.</returns>
        public static ScheduledEventBase Schedule<T, U>( float delay, Action<T, U> action, T value1, U value2 )
        {
            // Objects may be wanting to be scheduled as the game is stopping but the Scheduler has already been destroyed. Ensure the Scheduler is still valid.
            if (Instance == null)
            {
                return null;
            }

            return Instance.AddEventInternal(delay, ScheduledEventBase.InvokeLocation.Update, action, value1, value2);
        }

        /// <summary>
        ///     Add a new event with two arguments which will be executed in the future within the FixedUpdate loop.
        /// </summary>
        /// <param name="delay">The delay from the current time to execute the event.</param>
        /// <param name="action">The delegate to execute after the specified delay.</param>
        /// <param name="value1">The first value to use when invoking the delegate.</param>
        /// <param name="value2">The second value to use when invoking the delegate.</param>
        /// <returns>The ScheduledEventBase instance, useful if the event should be cancelled.</returns>
        public static ScheduledEventBase ScheduleFixed<T, U>( float delay, Action<T, U> action, T value1, U value2 )
        {
            // Objects may be wanting to be scheduled as the game is stopping but the Scheduler has already been destroyed. Ensure the Scheduler is still valid.
            if (Instance == null)
            {
                return null;
            }

            return Instance.AddEventInternal(delay, ScheduledEventBase.InvokeLocation.FixedUpdate, action, value1,
                value2);
        }

        /// <summary>
        ///     Internal method which adds a new event with two arguments which will be executed in the future.
        /// </summary>
        /// <param name="delay">The delay from the current time to execute the event.</param>
        /// <param name="invokeLocation">Specifies where the event shoud be invoked.</param>
        /// <param name="action">The delegate to execute after the specified delay.</param>
        /// <param name="value1">The first value to use when invoking the delegate.</param>
        /// <param name="value2">The second value to use when invoking the delegate.</param>
        /// <returns>The ScheduledEventBase instance, useful if the event should be cancelled.</returns>
        private ScheduledEventBase AddEventInternal<T, U>( float delay,
            ScheduledEventBase.InvokeLocation invokeLocation, Action<T, U> action, T value1, U value2 )
        {
            if (delay == 0)
            {
                action(value1, value2);
                return null;
            }
            var scheduledEvent = ReferencePool.Acquire<ScheduledEvent<T, U>>();
            // A delay of -1 indicates that the event reoccurs forever and must manually be cancelled.
            scheduledEvent.Initialize(delay == -1 ? -1 : Time.time + delay, invokeLocation, action, value1, value2);
            AddScheduledEvent(scheduledEvent);
            return scheduledEvent;
        }

        /// <summary>
        ///     Add a new event with three arguments which will be executed in the future within the Update loop.
        /// </summary>
        /// <param name="delay">The delay from the current time to execute the event.</param>
        /// <param name="action">The delegate to execute after the specified delay.</param>
        /// <param name="value1">The first value to use when invoking the delegate.</param>
        /// <param name="value2">The second value to use when invoking the delegate.</param>
        /// <param name="value3">The third value to use when invoking the delegate.</param>
        /// <returns>The ScheduledEventBase instance, useful if the event should be cancelled.</returns>
        public static ScheduledEventBase Schedule<T, U, V>( float delay, Action<T, U, V> action, T value1, U value2,
            V value3 )
        {
            // Objects may be wanting to be scheduled as the game is stopping but the Scheduler has already been destroyed. Ensure the Scheduler is still valid.
            if (Instance == null)
            {
                return null;
            }

            return Instance.AddEventInternal(delay, ScheduledEventBase.InvokeLocation.Update, action, value1, value2,
                value3);
        }

        /// <summary>
        ///     Add a new event with three arguments which will be executed in the future within the FixedUpdate loop.
        /// </summary>
        /// <param name="delay">The delay from the current time to execute the event.</param>
        /// <param name="action">The delegate to execute after the specified delay.</param>
        /// <param name="value1">The first value to use when invoking the delegate.</param>
        /// <param name="value2">The second value to use when invoking the delegate.</param>
        /// <param name="value3">The third value to use when invoking the delegate.</param>
        /// <returns>The ScheduledEventBase instance, useful if the event should be cancelled.</returns>
        public static ScheduledEventBase ScheduleFixed<T, U, V>( float delay, Action<T, U, V> action, T value1,
            U value2, V value3 )
        {
            // Objects may be wanting to be scheduled as the game is stopping but the Scheduler has already been destroyed. Ensure the Scheduler is still valid.
            if (Instance == null)
            {
                return null;
            }

            return Instance.AddEventInternal(delay, ScheduledEventBase.InvokeLocation.FixedUpdate, action, value1,
                value2, value3);
        }

        /// <summary>
        ///     Internal method which adds a new event with three arguments which will be executed in the future.
        /// </summary>
        /// <param name="delay">The delay from the current time to execute the event.</param>
        /// <param name="invokeLocation">Specifies where the event shoud be invoked.</param>
        /// <param name="action">The delegate to execute after the specified delay.</param>
        /// <param name="value1">The first value to use when invoking the delegate.</param>
        /// <param name="value2">The second value to use when invoking the delegate.</param>
        /// <param name="value3">The third value to use when invoking the delegate.</param>
        /// <returns>The ScheduledEventBase instance, useful if the event should be cancelled.</returns>
        private ScheduledEventBase AddEventInternal<T, U, V>( float delay,
            ScheduledEventBase.InvokeLocation invokeLocation, Action<T, U, V> action, T value1, U value2, V value3 )
        {
            if (delay == 0)
            {
                action(value1, value2, value3);
                return null;
            }
            var scheduledEvent = ReferencePool.Acquire<ScheduledEvent<T, U, V>>();
            // A delay of -1 indicates that the event reoccurs forever and must manually be cancelled.
            scheduledEvent.Initialize(delay == -1 ? -1 : Time.time + delay, invokeLocation, action, value1, value2,
                value3);
            AddScheduledEvent(scheduledEvent);
            return scheduledEvent;
        }

        /// <summary>
        ///     Cancels an event.
        /// </summary>
        /// <param name="scheduledEvent">The event to cancel.</param>
        public static void Cancel( ScheduledEventBase scheduledEvent )
        {
            // Objects may be wanting to be cancelled as the game is stopping but the Scheduler has already been destroyed. Ensure the Scheduler is still valid.
            if (s_Instance == null)
            {
                return;
            }

            Instance.CancelEventInternal(scheduledEvent);
        }

        /// <summary>
        ///     Internal method to cancel an event.
        /// </summary>
        /// <param name="scheduledEvent">The event to cancel.</param>
        private void CancelEventInternal( ScheduledEventBase scheduledEvent )
        {
            if (scheduledEvent == null)
            {
                return;
            }

            if (scheduledEvent != null && scheduledEvent.Active)
            {
                var removeIndex = -1;
                if (scheduledEvent.Location == ScheduledEventBase.InvokeLocation.Update)
                {
                    for (var i = ActiveUpdateEventCount - 1; i >= 0; --i)
                    {
                        if (ActiveUpdateEvents[i] == scheduledEvent)
                        {
                            removeIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    for (var i = ActiveFixedUpdateEventCount - 1; i >= 0; --i)
                    {
                        if (ActiveFixedUpdateEvents[i] == scheduledEvent)
                        {
                            removeIndex = i;
                            break;
                        }
                    }
                }
                if (removeIndex != -1)
                {
                    RemoveActiveEvent(removeIndex, scheduledEvent.Location);
                    // 回收引用
                    ReferencePool.Release((IReference) scheduledEvent);
                }
            }
        }

        /// <summary>
        ///     Invokes the scheduled event with the specified index.
        /// </summary>
        /// <param name="scheduledEvent">The event that should be invoked.</param>
        /// <param name="index">The index of the event to invoke.</param>
        private void Invoke( ScheduledEventBase scheduledEvent, int index )
        {
            // An end time of -1 indicates that the event reoccurs forever and must manually be cancelled.
            if (scheduledEvent.EndTime != -1)
                // Remove the event from the list before the invoke to prevent the invoked method from adding a new event and changing the order.
            {
                RemoveActiveEvent(index, scheduledEvent.Location);
            }
            scheduledEvent.Invoke();
            if (scheduledEvent.EndTime != -1)
                // 回收引用
            {
                ReferencePool.Release((IReference) scheduledEvent);
            }
        }

        /// <summary>
        ///     Removes the active event at the specified index.
        /// </summary>
        /// <param name="index">The index of the active event that should be removed.</param>
        /// <param name="location">The location that the event is invoked.</param>
        private void RemoveActiveEvent( int index, ScheduledEventBase.InvokeLocation location )
        {
            if (location == ScheduledEventBase.InvokeLocation.Update)
            {
                ActiveUpdateEvents[index].Active = false;
                for (var i = index + 1; i < ActiveUpdateEventCount; ++i)
                {
                    ActiveUpdateEvents[i - 1] = ActiveUpdateEvents[i];
                }
                ActiveUpdateEventCount--;
                ActiveUpdateEvents[ActiveUpdateEventCount] = null;
            }
            else
            {
                ActiveFixedUpdateEvents[index].Active = false;
                for (var i = index + 1; i < ActiveFixedUpdateEventCount; ++i)
                {
                    ActiveFixedUpdateEvents[i - 1] = ActiveFixedUpdateEvents[i];
                }
                ActiveFixedUpdateEventCount--;
                ActiveFixedUpdateEvents[ActiveFixedUpdateEventCount] = null;
            }
        }

        /// <summary>
        ///     Reset the initialized variable when the scene is no longer loaded.
        /// </summary>
        /// <param name="scene">The scene that was unloaded.</param>
        private void SceneUnloaded( UnityEngine.SceneManagement.Scene scene )
        {
            s_Initialized = false;
            s_Instance = null;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }

        /// <summary>
        ///     The object has been disabled.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneUnloaded += SceneUnloaded;
        }
    }
}