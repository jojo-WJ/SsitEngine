/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/7/18 16:04:11                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.PureMVC.Patterns;
using UnityEngine;

namespace Framework.Event
{
    public class EventMessage : Ss_Event
    {
        public EnEventMessage messagName = EnEventMessage.None;
        public string value;

        public EventMessage( int id, float time ) : base(id, time)
        {
        }

        public override void Init( EventDataInfo info )
        {
            var config = info as EventMessageInfo;
            if (config == null)
            {
                Debug.LogError(" EventMessage expression");
                return;
            }

            messagName = config.messageName;
            value = config.value;
            base.Init(info);
        }

        protected override void ExecuteImpl()
        {
            if (messagName != EnEventMessage.None)
            {
                Facade.Instance.SendNotification((ushort) messagName, value);
            }
        }

        protected override void endImpl()
        {
        }

        protected override void AbortImpl()
        {
            if (messagName != EnEventMessage.None)
            {
                var msg = (int) messagName + 1;
                Facade.Instance.SendNotification((ushort) msg, value);
            }
        }
    }
}