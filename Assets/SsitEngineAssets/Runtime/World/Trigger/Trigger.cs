using System;
using Mirror;
using SsitEngine.Event.Condition;
using SsitEngine.Unity.SceneObject;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.SceneObject.Trigger
{
    [Serializable]
    public class ActionEvent : UnityEvent<object, EnPropertyId, string, object>
    {
    }

    public class Trigger : MonoBehaviour
    {
        [Tooltip("仅本端响应")] [SerializeField] public bool isLocalClient;

        [Tooltip("仅服务器响应")] public bool isOnlyServer;

        [Tooltip("关联行为ID")] [SerializeField] protected EnPropertyId m_actionId;

        [SerializeField] protected ConditionSet m_condition;

        [SerializeField] public ActionEvent m_onActionEvent;

        public EnPropertyId ActionId => m_actionId;

        private void Awake()
        {
            var col = GetComponentInChildren<Collider>();
            if (col)
            {
                if (!GlobalManager.Instance.IsSync || isOnlyServer && NetworkServer.active)
                {
                }
                else
                {
                    //非服务端禁用碰撞
                    col.enabled = false;
                }
            }
        }

        public virtual void Init( BaseObject obi )
        {
        }

        /// <summary>
        /// 进入触发器
        /// </summary>
        /// <param name="other"></param>
        protected virtual void OnTriggerEnter( Collider other )
        {
        }

        //private void OnTriggerStay( Collider other )
        //{
        //    if (other.tag.Equals( "Player" ) && mTriggleObj != null)
        //    {
        //        BaseSceneObject player = other.GetComponent<BaseSceneObject>();
        //        if (player != null && mTriggleObj.Check( player ))
        //        {
        //            mTriggleObj.Stay( player );
        //        }
        //    }

        //}

        /// <summary>
        /// 退出触发器
        /// </summary>
        /// <param name="other"></param>
        protected virtual void OnTriggerExit( Collider other )
        {
        }


        protected virtual void PostTrigger()
        {
        }
    }
}