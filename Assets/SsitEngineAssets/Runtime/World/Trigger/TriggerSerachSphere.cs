/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/29 14:59:47                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework;
using SsitEngine.Unity;
using SsitEngine.Unity.Timer;
using UnityEngine;

namespace Framework.SceneObject.Trigger
{
    /// <summary>
    /// 给门定制的缓存触发器
    /// </summary>
    public class TriggerSerachSphere : Trigger
    {
        private bool isInit;

        private TimerEventTask m_eventTask;
        private bool mIsTrigger;
        private BaseObject mOwner;

        [SerializeField] private float mRadius = 20f;

        [SerializeField] private ObjectManager.SearchRelation SearchRelation = ObjectManager.SearchRelation.CR_Gas;

        public bool IsTrigger
        {
            get => mIsTrigger;
            set
            {
                if (mIsTrigger != value)
                {
                    mIsTrigger = value;
                    var index = mIsTrigger ? (int) En_SwitchState.On : (int) En_SwitchState.Off;

                    //mOwner?.ChangeProperty(this, EnPropertyId.Authority, string.Empty, null);
                    m_onActionEvent.Invoke(this, m_actionId, index.ToString(), null);
                }
            }
        }

        public override void Init( BaseObject obj )
        {
            mOwner = obj;
            isInit = true;

            if (isActiveAndEnabled)
                m_eventTask =
                    Engine.Instance.Platform.AddTimerEvent(TimerEventType.TeveDelaySpanAlways, 0, 0, 1, TimerEvent);
        }

        private void OnEnable()
        {
            if (isInit && m_eventTask == null)
                m_eventTask =
                    Engine.Instance.Platform.AddTimerEvent(TimerEventType.TeveDelaySpanAlways, 0, 0, 1, TimerEvent);
            //IsTrigger = false;
        }

        private void OnDisable()
        {
            if (m_eventTask != null)
            {
                Engine.Instance.Platform.RemoveTimerEvent(m_eventTask);
                IsTrigger = false;
            }
            m_eventTask = null;
        }


        /// <summary>
        /// 范围检测 300米
        /// 如果发生事故且没有报警则报警
        /// 如果没有发生事故且有报警则取消报警
        /// </summary>
        /// <param name="eve"></param>
        /// <param name="timeElapsed"></param>
        /// <param name="data"></param>
        private void TimerEvent( TimerEventTask eve, float timeElapsed, object data )
        {
            var objects = ObjectManager.Instance.GetSceneObject(gameObject, EnFactoryType.SceneObjectFactory,
                SearchRelation, ObjectManager.SearchAreaType.SA_Fan, new Vector2(mRadius, 360));

            if (objects.Count > 0)
                IsTrigger = true;
            else
                IsTrigger = false;
        }


        private void OnDestroy()
        {
            if (m_eventTask != null) Engine.Instance.Platform.RemoveTimerEvent(m_eventTask);
            m_eventTask = null;
        }

#if UNITY_EDITOR_WIN

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, mRadius);
            Gizmos.color = Color.white;
        }
#endif
    }
}