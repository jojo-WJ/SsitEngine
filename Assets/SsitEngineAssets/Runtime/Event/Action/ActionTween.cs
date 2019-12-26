/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/29 12:15:08                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using DG.Tweening;
using DG.Tweening.Core;
using Framework;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace SsitEngine.Unity.Action
{
    [Serializable]
    public class TweenerParam
    {
        public Vector3 beginValueV3;
        public float duration;
        public Vector3 endValueV3;
        public int LoopCount;
        public LoopType loopType = LoopType.Restart;
        public En_SwitchState state;
        public Component target;
        public TargetType targetType;
        public Tweener tween;
        public DOTweenAnimationType type;
    }

    public class ActionTween : ActionBase
    {
        [AddDescribe("自动释放Tween")] public bool isAutosKill;

        [AddDescribe("是否是标志开关")] public bool isFlag;

        public TweenerParam[] mTweenerParams;

        public override void Execute( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            if (mTweenerParams.Length <= 0) return;

            var state = (En_SwitchState) actionParam.ParseByDefault(0);
            if (!isFlag)
            {
                for (var i = 0; i < mTweenerParams.Length; i++)
                {
                    var tween = mTweenerParams[i];
                    if (tween.state == state)
                    {
                        mTweenerParams[i].tween?.Kill();
                        if (!mTweenerParams[i].target)
                            return;
                        CreateTween(mTweenerParams[i]);
                        mTweenerParams[i].tween.OnComplete(m_onComplete.Invoke);
                        mTweenerParams[i].tween.SetAutoKill(isAutosKill);
                        mTweenerParams[i].tween.Play();
                    }
                }
                return;
            }
            switch (state)
            {
                case En_SwitchState.Off:
                {
                    for (var i = 0; i < mTweenerParams.Length; i++)
                        if (isAutosKill)
                        {
                            mTweenerParams[i].tween?.Kill();

                            CreateTween(mTweenerParams[i], true);
                            mTweenerParams[i].tween.OnComplete(m_onComplete.Invoke);
                            mTweenerParams[i].tween.SetAutoKill(isAutosKill);
                            mTweenerParams[i].tween.Play();
                        }
                        else
                        {
                            mTweenerParams[i].tween?.PlayBackwards();
                        }
                }
                    break;
                case En_SwitchState.On:
                {
                    for (var i = 0; i < mTweenerParams.Length; i++)
                    {
                        mTweenerParams[i].tween?.Kill();
                        CreateTween(mTweenerParams[i]);
                        mTweenerParams[i].tween.OnComplete(m_onComplete.Invoke);
                        mTweenerParams[i].tween.SetAutoKill(isAutosKill);
                        if (isAutosKill)
                            mTweenerParams[i].tween.Play();
                        else
                            mTweenerParams[i].tween.PlayForward();
                    }
                }
                    break;
            }
        }

        private void CreateTween( TweenerParam param, bool backward = false )
        {
            var targetPos = backward ? param.beginValueV3 : param.endValueV3;
            switch (param.type)
            {
                case DOTweenAnimationType.None:
                    break;
                case DOTweenAnimationType.Move:
                {
                    if (param.target == null)
                    {
                        Debug.LogWarning(
                            string.Format(
                                "{0} :: This param.tween's TO param.target is NULL, a Vector3 of (0,0,0) will be used instead",
                                gameObject.name), gameObject);
                        targetPos = Vector3.zero;
                    }
                }
                    switch (param.targetType)
                    {
                        case TargetType.Transform:
                            param.tween = ((Transform) param.target).DOMove(targetPos, param.duration);
                            break;
                        case TargetType.Rigidbody2D:
                            param.tween = ((Rigidbody2D) param.target).DOMove(targetPos, param.duration);
                            break;
                        case TargetType.Rigidbody:
                            param.tween = ((Rigidbody) param.target).DOMove(targetPos, param.duration);
                            break;
                    }
                    break;
                case DOTweenAnimationType.LocalMove:
                    param.tween = ((Transform) param.target).DOLocalMove(targetPos, param.duration);
                    break;
                case DOTweenAnimationType.Rotate:
                    switch (param.targetType)
                    {
                        case TargetType.Transform:
                            param.tween = ((Transform) param.target).DORotate(targetPos, param.duration);
                            break;
                        case TargetType.Rigidbody:
                            param.tween = ((Rigidbody) param.target).DORotate(targetPos, param.duration);
                            break;
                    }
                    break;
                case DOTweenAnimationType.LocalRotate:
                    if (param.target is Transform targetTrans)
                        param.tween = targetTrans.DOLocalRotate(targetPos, param.duration);

                    break;
                case DOTweenAnimationType.Scale:
                    param.tween = ((Transform) param.target).DOScale(targetPos, param.duration);
                    break;
                /*case DOTweenAnimationType.Color:
                    switch (param.targetType)
                    {
                        case TargetType.SpriteRenderer:
                            param.tween = ((SpriteRenderer)param.target).DOColor(endValueColor, param.duration);
                            break;
                        case TargetType.Renderer:
                            param.tween = ((Renderer)param.target).material.DOColor(endValueColor, param.duration);
                            break;
                        case TargetType.Image:
                            param.tween = ((Image)param.target).DOColor(endValueColor, param.duration);
                            break;
                        case TargetType.Text:
                            param.tween = ((Text)param.target).DOColor(endValueColor, param.duration);
                            break;
                        case TargetType.Light:
                            param.tween = ((Light)param.target).DOColor(endValueColor, param.duration);
                            break;*/
                case DOTweenAnimationType.Fade:
                    switch (param.targetType)
                    {
                        case TargetType.Renderer:
                            param.tween = ((Renderer) param.target).material.DOFade(targetPos.x, param.duration);
                            break;
                        case TargetType.Light:
                            param.tween = ((Light) param.target).DOIntensity(targetPos.x, param.duration);
                            param.tween.SetEase(Ease.Linear);
                            if (param.LoopCount != 0 && targetPos.x != 0)
                                param.tween.SetLoops(param.LoopCount, param.loopType);

                            break;
                    }
                    break;
                /*case DOTweenAnimationType.ShakePosition:
                    {
                        param.tween = ((Transform)param.target).DOShakePosition(param.duration, targetPos, optionalInt0, optionalFloat0, optionalBool0);
                    }
                    break;
                case DOTweenAnimationType.ShakeScale:
                    param.tween = transform.DOShakeScale(param.duration, targetPos, optionalInt0, optionalFloat0);
                    break;
                case DOTweenAnimationType.ShakeRotation:
                    param.tween = transform.DOShakeRotation(param.duration, targetPos, optionalInt0, optionalFloat0);
                    break;
                case DOTweenAnimationType.CameraBackgroundColor:
                    param.tween = ((Camera)param.target).DOColor(endValueColor, param.duration);
                    break;*/
            }
            if (param.tween == null) return;
            param.tween.Pause();
        }

        private void OnDestroy()
        {
            if (isAutosKill)
                return;
            for (var i = 0; i < mTweenerParams.Length; i++)
            {
                mTweenerParams[i].tween.Kill();
                mTweenerParams[i].tween = null;
            }
        }
    }
}