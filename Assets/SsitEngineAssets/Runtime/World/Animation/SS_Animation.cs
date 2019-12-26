using System.Collections.Generic;
using Framework;
using SsitEngine;
using SsitEngine.Unity.Timer;
using Framework.Event;

namespace SsitEngine.Unity.Animation
{
    using EventList = List<Ss_Event>;
    using TimerEventList = List<TimerEventTask>;

    public class SS_Animation : AllocatedObject
    {
        public bool isPlaying;
        public float length;

        protected TimerEventTask mEvent;
        public EventList mEventTree;
        protected OnTimerEventHandler mPlayEndfunc;
        protected TimerEventList mTimerEventList;

        /// <summary>
        /// 播放时间点（当前不支持从时间点播放）
        /// </summary>
        private float time;

        public SS_Animation()
        {
            mEventTree = new EventList();
            mTimerEventList = new TimerEventList();
            mPlayEndfunc = null;
            time = 0;
            isPlaying = false;
        }

        public float Time => time;

        public void SetOnPlayEnd( OnTimerEventHandler func )
        {
            mPlayEndfunc = func;
        }

        public void AddEvent( Ss_Event eve )
        {
            mEventTree.Add(eve);
        }

        public void RemoveEvent( Ss_Event eve )
        {
            mEventTree.Remove(eve);
            EventManager.Instance.DestroyEvent(eve);
        }


        public void Play( OnTimerEventHandler func, object data )
        {
            //Debug.LogError("Play length" + length);
            isPlaying = true;
            mPlayEndfunc = func;
            mEvent = SsitApplication.Instance.AddTimerEvent(TimerEventType.TeveOnce, 0, length, 0, OnPlayEnd, data);
            var timerEvent =
                SsitApplication.Instance.AddTimerEvent(TimerEventType.TeveSpanUntil, 0, length, 0, OnTimeUpdate, data);
            mTimerEventList.Add(timerEvent);
            for (var i = 0; i < mEventTree.Count; ++i)
            {
                //eve.setObject(mParent.getCreator());
                var eve = mEventTree[i];
                //Debug.Log("mEventTree" + eve.Id + "||" + eve.Time);

                var tv = SsitApplication.Instance.AddTimerEvent(TimerEventType.TeveOnce, 0, eve.Time, 0, OnEvent, eve);
                if (tv != null)
                    mTimerEventList.Add(tv);
            }
        }

        private void OnTimeUpdate( TimerEventTask eve, float timeelapsed, object data )
        {
            time = timeelapsed;
        }

        private void OnPlayEnd( TimerEventTask eve, float timeelapsed, object data )
        {
            Stop();
        }

        private void OnEvent( TimerEventTask e, float timeElapsed, object data )
        {
            if (data is Ss_Event eve)
                //Debug.Log("OnEvent timeElapsed" + timeElapsed + "::" + eve.Id);
                eve.Execute(data);
        }

        public void Stop()
        {
            isPlaying = false;
            ClearTimerEvent();
            ClearEvent();
            if (mPlayEndfunc != null) mPlayEndfunc(null, 0, null);
            mPlayEndfunc = null;
        }

        public void ClearEvent()
        {
            var em = EventManager.Instance;
            var itor = mEventTree.GetEnumerator();
            while (itor.MoveNext())
            {
                itor.Current?.Abort();
                em.DestroyEvent(itor.Current);
            }
            itor.Dispose();
            mEventTree.Clear();
        }

        private void ClearTimerEvent()
        {
            //TimeManager.Instance.RemoveTimerEvent(TimerEventType.TEVE_ONCE, eve.Time, 0, OnEvent, eve);
            // Engine.getSingletonPtr().getTimer().removeTimerEvent(mEvent);
            if (mEvent != null) SsitApplication.Instance.RemoveTimerEvent(mEvent);
            var itor = mTimerEventList.GetEnumerator();
            while (itor.MoveNext())
                SsitApplication.Instance.RemoveTimerEvent(itor.Current);
            itor.Dispose();
            mTimerEventList.Clear();
        }

        public EventList GetEventList()
        {
            return mEventTree;
        }

        public override void Shutdown()
        {
            base.Shutdown();

            //if (mEventTree != null)
            //{
            //    EventManager.Instance.DestroyEvent(mEventTree);
            //}

            mEventTree = null;
        }
    }
}