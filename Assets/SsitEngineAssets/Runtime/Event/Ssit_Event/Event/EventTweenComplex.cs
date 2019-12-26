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
    /// <summary>
    /// 复合Tween
    /// </summary>
    public class EventTweenComplex : Ss_Event
    {
        private List<TrasInfo> mPath;

        /// <summary>
        /// 动画序列
        /// </summary>
        private Sequence se;

        public EventTweenComplex( int id, float time ) : base(id, time)
        {
            mPath = new List<TrasInfo>();
        }

        public Camera Cam => Camera.main;

        public override void Init( EventDataInfo info )
        {
            var tweenInfo = info as EventTweenComplexInfo;
            if (tweenInfo == null)
            {
                Debug.LogError(" tweenInfo expression");
                return;
            }
            mPath.Clear();
            for (var i = 0; i < tweenInfo.path.Count; i++) AddPath(tweenInfo.path[i]);
            base.Init(info);
        }

        public void AddPath( TrasInfo info )
        {
            mPath.Add(info);
        }

        protected override void ExecuteImpl()
        {
            if (Cam == null)
                return;

            //MainProcess.inputStateManager.switchTo(MainProcess.inputStateManager.viewpointTranslationManipulator);
            //MainProcess.inputStateManager.viewpointTranslationManipulator.TranslateViewpoint(mPath[0].postion
            //    , mPath[0].rotate
            //    , null
            //    , () => { OnPlayEnd(0, null); }
            //    , Time);
            se = DOTween.Sequence();

            for (var i = 0; i < mPath.Count; i++)
            {
                var tt = mPath[i];
                se.Append(Cam.transform.DOMove(mPath[i].postion, mPath[i].duration));
                se.Join(Cam.transform.DOLocalRotate(mPath[i].rotate, mPath[i].duration));
            }

            se.SetAutoKill();
            se.OnComplete(() => { OnPlayEnd(0, null); });

            se.Play();
        }

        protected override void AbortImpl()
        {
            if (se.IsPlaying())
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