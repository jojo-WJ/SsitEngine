/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/7/15 11:35:53                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Unity.UI;
using UnityEngine;

namespace Framework.Event
{
    public class EventUIContent : Ss_Event
    {
        private string content;
        private bool state;

        public EventUIContent( int id, float time ) : base(id, time)
        {
        }


        public override void Init( EventDataInfo info )
        {
            var config = info as EventUIContentInfo;
            if (config == null)
            {
                Debug.LogError(" tweenInfo expression");
                return;
            }

            content = config.content;
            state = config.state;
            base.Init(info);
        }


        protected override void ExecuteImpl()
        {
            var key = state ? UIMsg.OpenForm : UIMsg.CloseForm;
            //Facade.Instance.SendNotification((ushort)key, En_UIForm.UIHintForm, content);
//            Facade.Instance.SendNotification((ushort)UIHintFormEvent.RefreshContent, content);
        }

        protected override void endImpl()
        {
            // Facade.Instance.SendNotification((ushort)UIMsg.CloseForm, En_UIForm.UIHintForm);
        }

        protected override void AbortImpl()
        {
            // Facade.Instance.SendNotification((ushort)UIMsg.CloseForm, En_UIForm.UIHintForm);
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }
    }
}