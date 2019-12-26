using Framework.Quest;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.QuestManager;
using UnityEngine;

namespace Framework.SceneObject
{
    public class SceneTriggerInstance : BaseSceneInstance
    {
        public override void Init( string guid = null )
        {
            base.Init(guid);
            mType = EnObjectType.Trigger;
        }

        private void OnTriggerEnter( Collider other )
        {
            //Debug.LogError( "进入触发器：" + Guid );

            var co = other.GetComponent<PlayerInstance>();
            if (co != null)
                if (co.HasAuthority)
                    //access : 任务系统接入
                    //Facade.Instance.SendMessage( En_QuestsMsg.En_ArrivedTo.ToString(), null, co.GroupUID, QuestParamGenerator.GeneratorParam( En_QuestsMsg.En_RangeEnterTrigger, this.ObjectGUID ) ,co.OwnerUid);
                    Facade.Instance.SendNotification((ushort) En_QuestsMsg.En_ArrivedTo,
                        new QuestMessageArgs(0, this, co.LinkObject.Guid, m_guid));
        }

        //private void OnTriggerStay( Collider other )
        //{

        //}

        private void OnTriggerExit( Collider other )
        {
            var co = other.GetComponent<PlayerInstance>();
            if (co != null)
            {
                //Facade.Instance.SendMessage( En_QuestsMsg.En_RangeQuitTrigger.ToString(), null, co.GroupUID, QuestParamGenerator.GeneratorParam( En_QuestsMsg.En_RangeQuitTrigger, this.ObjectGUID ),co.OwnerUid );
            }
        }
    }
}