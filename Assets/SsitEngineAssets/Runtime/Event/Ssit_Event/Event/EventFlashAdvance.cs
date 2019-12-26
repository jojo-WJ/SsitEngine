/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/7/17 18:38:11                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework;
using Framework.Tip;
using HighlightingSystem;
using SsitEngine.Unity;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using SsitEngine.Unity.Timer;
using UnityEngine;

namespace Framework.Event
{
    public class EventFlashAdvance : Ss_Event
    {
        private string bindName;
        private Highlighter cc;


        private string content;
        private float duration;
        private float frequency;
        private Color m_color;
        private Color m_color1;
        private TagTip m_tagTip;
        private BaseObject mBinderObj;


        private TimerEventTask mEvent;
        private GameObject mTipBinderObj;
        private Vector3 offset;

        public EventFlashAdvance( int id, float time ) : base(id, time)
        {
        }

        public override void Init( EventDataInfo info )
        {
            var config = info as EventFlashAdvanceInfo;
            if (config == null)
            {
                Debug.LogError(" tweenInfo expression");
                return;
            }

            bindName = config.BindName;

            //

            if (string.IsNullOrEmpty(config.binderId))
            {
                mTipBinderObj = GameObject.Find(bindName);
            }
            else
            {
                mBinderObj = ObjectManager.Instance.GetObject<BaseObject>(config.binderId);
                mTipBinderObj = mBinderObj?.GetRepresent();
            }

            if (mTipBinderObj == null) Debug.LogError("BindName is not find" + bindName);

            duration = config.duration;
            duration = config.duration;
            frequency = config.frequency;

            m_color = config.nomalColor;
            m_color1 = config.lightColor;

            offset = config.offset;
            content = config.content;


            base.Init(info);
        }


        protected override void ExecuteImpl()
        {
            //SoundManager.Load(clipName);
            if (mTipBinderObj == null)
                return;

            if (mBinderObj != null && mBinderObj.SceneInstance.Type == EnObjectType.BoundingBox)
            {
                mBinderObj.GetRepresent().GetComponentInChildren<Renderer>(true).enabled = true;
                mBinderObj.SetProperty(EnPropertyId.Show2DTag, true.DeParseByDefault());
            }
            else
            {
                if (!string.IsNullOrEmpty(content))
                {
                    m_tagTip = TagTipManager.Instance.GetTip();
                    m_tagTip.SetTipInfo(mTipBinderObj, content, offset);
                }
            }

            cc = mTipBinderObj.GetOrAddComponent<Highlighter>();
            cc.TweenStart();

            //cc.seeThrough = true;
            if (frequency <= 0)
                cc?.ConstantOn(m_color1);
            else
                cc.FlashingOn(m_color1, m_color, frequency);

            //cc.seeThrough = true;
            mEvent = SsitApplication.Instance.AddTimerEvent(TimerEventType.TeveOnce, 0, duration, 0, OnEvent);
        }

        private void OnEvent( TimerEventTask eve, float timeelapsed, object data )
        {
            OnPlayEnd(timeelapsed, data);
            mEvent = null;
        }

        protected override void endImpl()
        {
            base.endImpl();
            if (m_tagTip) m_tagTip.SetTagInfo(null);

            if (frequency <= 0)
            {
                cc?.ConstantOffImmediate();
                ;
            }
            else
            {
                cc?.FlashingOff();
            }
            Object.Destroy(cc);
        }

        protected override void AbortImpl()
        {
            if (mEvent != null) SsitApplication.Instance.RemoveTimerEvent(mEvent);

            if (m_tagTip) m_tagTip.SetTagInfo(null);

            if (cc)
            {
                if (frequency <= 0)
                {
                    cc?.ConstantOffImmediate();
                    ;
                }
                else
                {
                    cc?.FlashingOff();
                }
            }
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }
    }
}