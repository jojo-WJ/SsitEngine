/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/7/17 18:16:14                     
*└──────────────────────────────────────────────────────────────┘
*/

using HighlightingSystem;
using SsitEngine.Unity;
using Framework.SceneObject;
using SsitEngine.Unity.Timer;
using UnityEngine;

namespace Framework.Event

{
    public class EventFlash : Ss_Event
    {
        private string bindName;
        private Highlighter cc;
        private float duration;

        private float frequency;
        private Color m_color;
        private Color m_color1;
        private GameObject mBinderObj;
        private TimerEventTask mEvent;

        public EventFlash( int id, float time ) : base(id, time)
        {
        }

        public override void Init( EventDataInfo info )
        {
            var config = info as EventFlashInfo;
            if (config == null)
            {
                Debug.LogError(" tweenInfo expression");
                return;
            }

            bindName = config.BindName;

            //Debug.Log("bindName" + bindName);

            if (string.IsNullOrEmpty(config.binderId))
                mBinderObj = GameObject.Find(bindName);
            else
                mBinderObj = ObjectManager.Instance.GetObject<BaseObject>(config.binderId).GetRepresent();

            duration = config.duration;
            frequency = config.frequency;

            m_color = config.nomalColor;
            m_color1 = config.lightColor;
            base.Init(info);
        }


        protected override void ExecuteImpl()
        {
            //SoundManager.Load(clipName);
            if (mBinderObj == null) return;

            cc = mBinderObj.GetOrAddComponent<Highlighter>();
            cc.TweenStart();

            //cc.seeThrough = true;
            if (frequency <= 0)
                cc?.ConstantOn(m_color1);
            else
                //Debug.Log("FlashingOn" + Color.red + " " + m_color1 + "  " + m_color);
                //cc.FlashingOn(Color.white, Color.red, frequency);
                cc.FlashingOn(m_color1, m_color, frequency);
            mEvent = SsitApplication.Instance.AddTimerEvent(TimerEventType.TeveOnce, 0, duration, 0, OnEvent);
        }

        private void OnFlash( float timeelapsed, object data )
        {
            //cc.seeThrough = true;
            if (frequency <= 0)
                cc?.ConstantOn(m_color1);
            else
                cc.FlashingOn(m_color, m_color1, frequency);
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