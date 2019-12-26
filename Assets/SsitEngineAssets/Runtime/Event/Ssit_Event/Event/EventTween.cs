/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/7/15 11:35:53                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Framework.Event
{
    public class EventTween : Ss_Event
    {
        private float duration;
        private Ease easeType = Ease.Linear;
        private List<TrasInfo> mPath;

        /// <summary>
        /// 动画序列
        /// </summary>
        private Sequence se;

        public EventTween( int id, float time ) : base(id, time)
        {
            mPath = new List<TrasInfo>();
        }

        public Camera Cam => Camera.main;

        public override void Init( EventDataInfo info )
        {
            var tweenInfo = info as EventTweenInfo;
            if (tweenInfo == null)
            {
                Debug.LogError(" tweenInfo expression");
                return;
            }

            duration = tweenInfo.duration;
            easeType = tweenInfo.tweenType;
            mPath.Clear();
            if (tweenInfo.path != null)
                for (var i = 0; i < tweenInfo.path.Count; i++)
                    AddPath(tweenInfo.path[i]);

            base.Init(info);
        }

        public void AddPath( TrasInfo info )
        {
            mPath.Add(info);
        }

        protected override void ExecuteImpl()
        {
            if (Cam == null || mPath.Count == 0)
            {
                OnPlayEnd(duration, null);
                return;
            }

            se = DOTween.Sequence();

            if (mPath.Count > 1)
            {
                var path = mPath.ConvertAll(x => { return x.postion; }).ToArray();
                se.Append(Cam.transform.DOPath(path, duration, gizmoColor: Color.cyan).SetEase(easeType));
            }
            else
            {
                se.Append(Cam.transform.DOMove(mPath[0].postion, duration).SetEase(easeType));
                se.Join(Cam.transform.DOLocalRotate(mPath[0].rotate, duration).SetEase(easeType));
                se.SetAutoKill();
                se.OnComplete(() => { OnPlayEnd(0, null); });
            }

            se.Play();
        }

        protected override void AbortImpl()
        {
            if (se != null && se.IsPlaying())
                se.Kill();
        }

        public override void Shutdown()
        {
            mPath.Clear();
            mPath = null;
            base.Shutdown();
        }
    }
}