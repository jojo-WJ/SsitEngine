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
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using SsitEngine.Unity.Timer;
using UnityEngine;

namespace Framework.Event
{
    public class EventUITag : Ss_Event
    {
        private string bindName;
        private string content;
        private float duration;
        private TagTip m_tagTip;
        private GameObject mBinderGo;
        private BaseObject mBinderObj;

        private TimerEventTask mEvent;
        private Vector3 offset;

        public EventUITag( int id, float time ) : base(id, time)
        {
        }

        public override void Init( EventDataInfo info )
        {
            var config = info as EventUITagInfo;
            if (config == null)
            {
                Debug.LogError(" tweenInfo expression");
                return;
            }

            bindName = config.BindName;

            //

            if (string.IsNullOrEmpty(config.binderId))
            {
                mBinderGo = GameObject.Find(bindName);
            }
            else
            {
                mBinderObj = ObjectManager.Instance.GetObject<BaseObject>(config.binderId);
                mBinderGo = mBinderObj?.GetRepresent();
            }

            if (!string.IsNullOrEmpty(bindName) && mBinderGo == null)
                Debug.LogError("BindName is not find" + bindName);

            duration = config.duration;
            offset = config.offset;
            content = config.content;
            base.Init(info);
        }


        protected override void ExecuteImpl()
        {
            //SoundManager.Load(clipName);
            if (mBinderGo == null)
                return;

            if (mBinderObj != null && mBinderObj.SceneInstance.Type == EnObjectType.BoundingBox)
            {
                mBinderObj.GetRepresent().GetComponentInChildren<Renderer>(true).enabled = true;
                mBinderObj.SetProperty(EnPropertyId.Show2DTag, true.DeParseByDefault());
            }
            else
            {
                m_tagTip = TagTipManager.Instance.GetTip();

                m_tagTip.SetTipInfo(mBinderGo, content, offset);
            }

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
        }

        protected override void AbortImpl()
        {
            if (mEvent != null) SsitApplication.Instance.RemoveTimerEvent(mEvent);

            if (m_tagTip) m_tagTip.SetTagInfo(null);
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }
    }
}