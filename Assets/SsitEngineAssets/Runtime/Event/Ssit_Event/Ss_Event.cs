/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/7/13 16:50:20                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using SsitEngine;
using Framework.SceneObject;
using SsitEngine.Unity.Timer;
using UnityEngine;

namespace Framework.Event
{
    using EventList = List<Ss_Event>;
    using Target = BaseObject;
    using TimerEventList = List<TimerEventTask>;


    /// <summary>
    /// 事件抽象基类
    /// </summary>
    public abstract class Ss_Event : AllocatedObject
    {
        public delegate void OnEndFunc( Ss_Event eve );

        public delegate void OnStartFunc( Ss_Event eve, object data );

        protected bool isEventEnding;
        protected bool isExecuted;

        //事件绑定者
        protected Target mBinder;
        protected object mData;
        protected EventList mEndEvents;


        protected int mId;
        protected bool mIsAbort;

        protected bool mIsEndWithParent;

        //事件初始化
        protected bool mIsInit;
        protected OnEndFunc mOnEndFunc;
        protected OnStartFunc mOnStartFunc;
        protected Ss_Event mParent;

        //handler
        protected EventList mStartEvents;

        protected float mTime;
        protected TimerEventList mTimerEvents;
        protected Target mTrigger;
        protected EventType mType;
        protected string mValue;

        public Ss_Event( int id, float time )
        {
            mParent = null;

            mId = id;
            mTime = time;
            mType = EventType.EVE_NONE;
            mStartEvents = new EventList();
            mEndEvents = new EventList();
            mData = null;
            mValue = "";
            mIsEndWithParent = false;
            mIsAbort = false;
            mTimerEvents = new TimerEventList();

            mIsInit = false;
            mBinder = null;
            mTrigger = null;

            mOnStartFunc = null;
            mOnEndFunc = null;
        }

        #region Property

        public int Id => mId;

        public EventType Type => mType;

        public object Data => mData;

        public bool IsInit
        {
            get => mIsInit;
            set => mIsInit = value;
        }

        public bool IsEndWithParent
        {
            get => mIsEndWithParent;
            set => mIsEndWithParent = value;
        }

        public float Time
        {
            get => mTime;
            set => mTime = value;
        }

        public Ss_Event Parent
        {
            get => mParent;
            set => mParent = value;
        }

        public BaseObject Binder
        {
            get => mBinder;
            set => mBinder = value;
        }

        public BaseObject Trigger
        {
            get => mTrigger;
            set => mTrigger = value;
        }

        public string Value
        {
            get => mValue;
            set => mValue = value;
        }

        public void SetOnStartFunc( OnStartFunc func )
        {
            mOnStartFunc = func;
        }

        public EventList GetStartEventList()
        {
            return mStartEvents;
        }

        public void SetOnEndFunc( OnEndFunc func )
        {
            mOnEndFunc = func;
        }

        public EventList GetEndEventList()
        {
            return mEndEvents;
        }

        #endregion

        #region Event Handler

        public void AddStartEvent( Ss_Event eve )
        {
            eve.Parent = this;
            eve.SetOnStartFunc(mOnStartFunc);
            eve.SetOnEndFunc(mOnEndFunc);

            mStartEvents.Add(eve);
        }

        public void RemoveStartEvent( Ss_Event eve )
        {
            mStartEvents.Remove(eve);
            EventManager.Instance.DestroyEvent(eve);
        }


        public void AddEndEvent( Ss_Event eve )
        {
            if (eve == null)
                return;

            eve.Parent = this;
            eve.SetOnStartFunc(mOnStartFunc);
            eve.SetOnEndFunc(mOnEndFunc);

            mEndEvents.Add(eve);
        }

        public void RemoveEndEvent( Ss_Event eve )
        {
            mEndEvents.Remove(eve);
            EventManager.Instance.DestroyEvent(eve);
        }

        #endregion

        #region 事件执行回调

        private void OnEvent( TimerEventTask e, float timeElapsed, object data )
        {
            if (data is Ss_Event eve) eve.Execute(mData);
        }

        protected void OnPlayEnd( float timeElapsed, object data )
        {
            if (mBinder != null)
            {
                //todo:绑定者触发
            }

            if (mOnEndFunc != null)
                mOnEndFunc(this);

            ApplyEnd();
        }

        private void ApplyStart()
        {
            startImpl();

            for (var i = 0; i < mStartEvents.Count; ++i)
            {
                var eve = mStartEvents[i];

                eve.Binder = mBinder;
                eve.Trigger = mTrigger;

                if (eve.Time == 0)
                {
                    OnEvent(null, 0, eve);
                }
                else
                {
                    var tv = SsitApplication.Instance.AddTimerEvent(TimerEventType.TeveOnce, 0, eve.Time, 0, OnEvent,
                        eve);
                    if (tv != null)
                        eve.mTimerEvents.Add(tv);
                }
            }
        }

        protected virtual void ApplyEnd()
        {
            if (isEventEnding) return;
            isEventEnding = true;
            endImpl();
            //结束前置伴随父事件执行的循环事件（比如说给事件添加一个守卫监测（随时解除父事件运行））
            for (var i = 0; i < mStartEvents.Count; ++i)
            {
                var eve = mStartEvents[i];

                if (eve.IsEndWithParent)
                    eve.OnPlayEnd(0, null);
            }
            //处理触发后事件
            //UnityEngine.Debug.Log("current Event"+ this.mType+"触发后事件：：" + mEndEvents.Count);
            for (var i = 0; i < mEndEvents.Count; ++i)
            {
                var eve = mEndEvents[i];

                //是否延迟执行
                if (eve.IsEndWithParent)
                {
                    eve.OnPlayEnd(0, null);
                }
                else
                {
                    if (!eve.mIsInit)
                    {
                        eve.Binder = mBinder;
                        eve.Trigger = mTrigger;
                    }
                    if (eve.Time == 0)
                    {
                        OnEvent(null, 0, eve);
                    }
                    else
                    {
                        var tv = SsitApplication.Instance.AddTimerEvent(TimerEventType.TeveOnce, 0, eve.Time, 0,
                            OnEvent, eve);
                        if (tv != null)
                            eve.mTimerEvents.Add(tv);
                    }
                }
            }
            //EventManager.Instance.DestroyEvent(this);
        }

        public virtual void Close()
        {
            OnPlayEnd(0, null);
        }

        public void OnDirectCallEvent( float elapsed, object data )
        {
            OnEvent(null, elapsed, data);
        }

        #endregion

        #region 事件执行

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="data"></param>
        public void Execute( object data )
        {
            if (isExecuted)
            {
                Debug.Log("当前事件已经执行过了，fuck····");
                return;
            }
            isExecuted = true;
            mTimerEvents.Clear();
            mIsAbort = false;
            mData = data;
            ApplyStart();
            if (mOnStartFunc != null)
                mOnStartFunc(this, mData);
            ExecuteImpl();
        }


        /// <summary>
        /// 阻断（destory调用）
        /// </summary>
        public void Abort()
        {
            if (!isExecuted || mIsAbort)
                return;
            mIsAbort = true;
            var itor = mTimerEvents.GetEnumerator();
            while (itor.MoveNext())
            {
                SsitApplication.Instance.RemoveTimerEvent(itor.Current);
                itor.Dispose();
            }
            var eveIt = mStartEvents.GetEnumerator();
            while (eveIt.MoveNext())
            {
                if (eveIt.Current != null)
                    eveIt.Current.Abort();
                eveIt.Dispose();
            }
            AbortImpl();
            eveIt = mEndEvents.GetEnumerator();
            while (eveIt.MoveNext())
            {
                if (eveIt.Current != null)
                    eveIt.Current.Abort();
                eveIt.Dispose();
            }
        }


        public override void Shutdown()
        {
            var eveIt = mStartEvents.GetEnumerator();
            while (eveIt.MoveNext())
                eveIt.Current.Shutdown();
            eveIt.Dispose();
            eveIt = mEndEvents.GetEnumerator();
            while (eveIt.MoveNext())
                eveIt.Current.Shutdown();
            eveIt.Dispose();

            //UnityEngine.Debug.Log("时间事件流产"+mTimerEvents.Count);
            var itor = mTimerEvents.GetEnumerator();
            while (itor.MoveNext())
                SsitApplication.Instance.RemoveTimerEvent(itor.Current);
            itor.Dispose();
            mTimerEvents.Clear();

            mTrigger = null;
            mStartEvents.Clear();
            mEndEvents.Clear();

            mTimerEvents.Clear();
            base.Shutdown();
        }


        #region 虚方法/重写

        public virtual void Init( EventDataInfo info )
        {
            IsInit = true;
        }

        /// <summary>
        /// 事件开始
        /// </summary>
        protected virtual void startImpl()
        {
        }

        /// <summary>
        /// 事件执行
        /// </summary>
        protected abstract void ExecuteImpl();

        /// <summary>
        /// 事件终止
        /// </summary>
        protected abstract void AbortImpl();

        /// <summary>
        /// 事件结束
        /// </summary>
        protected virtual void endImpl()
        {
        }

        #endregion

        #endregion
    }
}