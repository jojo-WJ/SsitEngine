/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/20 16:00:30                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Core.ReferencePool;
using SsitEngine.Core.TaskPool;

namespace SsitEngine.Unity.Timer
{
    /// <summary>
    ///     计时任务
    /// </summary>
    public class TimerEventTask : AllocatedObject, IReference, ITask
    {
        /// <summary>
        ///     当前时间
        /// </summary>
        private float m_currentTime;

        /// <summary>
        ///     计时回调委托
        /// </summary>
        private OnTimerEventHandler m_handler;

        /// <summary>
        ///     当前时间步长
        /// </summary>
        private float m_step;

        /// <summary>
        ///     自定义数据
        /// </summary>
        protected object mData;

        /// <summary>
        ///     计时周期
        /// </summary>
        protected float mDest;

        /// <summary>
        ///     是否激活
        /// </summary>
        protected bool mEnabled;

        /// <summary>
        ///     计时任务Id
        /// </summary>
        protected ulong mId;

        /// <summary>
        ///     任务优先级
        /// </summary>
        protected int mPriority;

        /// <summary>
        ///     计时间隔
        /// </summary>
        protected float mSpan;

        /// <summary>
        ///     计时器时时间
        /// </summary>
        protected float mSrc;

        /// <summary>
        ///     计时任务类型
        /// </summary>
        protected TimerEventType mType;

        /// <summary>
        ///     计时任务构造
        /// </summary>
        public TimerEventTask()
        {
        }

        /// <summary>
        ///     计时任务构造
        /// </summary>
        /// <param name="mId">任务唯一id</param>
        /// <param name="mPriority">任务优先级</param>
        /// <param name="mType">计时类型</param>
        /// <param name="mDest">计时周期</param>
        /// <param name="mSpan">计时间隔</param>
        /// <param name="handler">计时委托</param>
        /// <param name="mData">自定义数据</param>
        public TimerEventTask( ushort mId, int mPriority, TimerEventType mType, float mDest, float mSpan,
            OnTimerEventHandler handler, object mData )
        {
            this.mId = mId;
            this.mPriority = mPriority;
            this.mType = mType;
            this.mDest = mDest;
            this.mSpan = mSpan;
            m_handler = handler;
            this.mData = mData;
            mEnabled = true;
            IsDirty = false;
        }

        /// <summary>
        ///     是否激活
        /// </summary>
        public bool Enabled
        {
            get => mEnabled;
            set => mEnabled = value;
        }

        /// <summary>
        ///     计时任务类型
        /// </summary>
        public TimerEventType TimerType => mType;

        /// <summary>
        ///     计时周期
        /// </summary>
        public float Duration => mDest;

        /// <summary>
        ///     计时回调委托
        /// </summary>
        public OnTimerEventHandler Handler => m_handler;

        /// <summary>
        ///     自定义数据
        /// </summary>
        public object Data => mData;

        /// <inheritdoc />
        public void Clear()
        {
            mId = 0;
            m_currentTime = 0;
            m_step = 0;
            mSrc = 0;
            mDest = 0;
            IsDirty = true;
            m_handler = null;
            mEnabled = false;
            mData = null;
        }

        /// <summary>
        ///     获取任务的唯一Id
        /// </summary>
        public ulong Id => mId;

        /// <inheritdoc />
        public int Priority => mPriority;

        /// <inheritdoc />
        public bool Done => IsDirty;

        /// <summary>
        ///     释放任务资源
        /// </summary>
        public override void Shutdown()
        {
            base.Shutdown();
            // 回收引用'
            ReferencePool.Release(this);
        }

        /// <summary>
        ///     设置计时任务参数
        /// </summary>
        /// <param name="id">任务唯一id</param>
        /// <param name="priority">任务优先级</param>
        /// <param name="type">计时类型</param>
        /// <param name="dest">计时周期</param>
        /// <param name="span">计时间隔</param>
        /// <param name="handler">计时委托</param>
        /// <param name="data">自定义数据</param>
        public void SetEventTask( ulong id, int priority, TimerEventType type, float dest, float span,
            OnTimerEventHandler handler, object data )
        {
            mId = id;
            mPriority = priority;
            mType = type;
            mDest = dest;
            mSpan = span;
            m_handler = handler;
            mData = data;
            mEnabled = true;
            IsDirty = false;
        }

        /// <summary>
        ///     设置任务起始时间
        /// </summary>
        public void SetSrc( float elapsed )
        {
            mSrc = elapsed;
        }

        /// <summary>
        ///     任务进行
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void OnUpdate( float elapseSeconds, float realElapseSeconds )
        {
            if (!mEnabled || IsDirty)
                return;

            switch (mType)
            {
                case TimerEventType.TeveAlways:
                {
                    var t = realElapseSeconds - mSrc;
                    if (t < 0)
                        t = 0.001f;

                    if (m_handler != null)
                        m_handler(this, t, mData);

                    break;
                }
                case TimerEventType.TeveOnce:
                {
                    var t = realElapseSeconds - mSrc;
                    if (t >= mDest)
                    {
                        if (m_handler == null)
                            t = 0;
                        if (m_handler != null)
                            m_handler(this, t, mData);
                        IsDirty = true;
                    }

                    break;
                }
                case TimerEventType.TeveSpanUntil:
                {
                    var t = realElapseSeconds - mSrc;
                    if (t <= mDest)
                    {
                        if (mSpan == 0)
                        {
                            if (m_handler != null)
                                m_handler(this, t, mData);
                        }
                        else
                        {
                            var delta = t - m_currentTime;
                            m_step += delta;
                            if (m_step >= mSpan)
                            {
                                m_handler(this, t, mData);
                                m_step -= mSpan;
                            }
                        }
                    }
                    else
                    {
                        m_handler(this, mDest, mData);
                        IsDirty = true;
                    }
                    m_currentTime = t;
                    break;
                }
                case TimerEventType.TeveDelaySpanAlways:
                {
                    var t = realElapseSeconds - mSrc;
                    if (t >= mDest)
                    {
                        if (mSpan == 0)
                        {
                            m_handler(this, t, mData);
                        }
                        else
                        {
                            var delta = t - m_currentTime;
                            m_step += delta;
                            if (m_step >= mSpan)
                            {
                                m_handler(this, t, mData);
                                m_step -= mSpan;
                            }
                        }
                    }
                    m_currentTime = t;
                    break;
                }
            }
        }
    }
}