/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/29 14:59:47                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Framework;
using Framework.Logic;
using UnityEngine;

namespace Framework.SceneObject.Trigger
{
    /// <summary>
    /// 给门定制的缓存触发器
    /// </summary>
    public class TriggerCacheSphere : Trigger
    {
        /// <summary>
        /// 我的缓存对象
        /// </summary>
        private HashSet<Collider> mCacheColliders = new HashSet<Collider>();


        protected override void OnTriggerEnter( Collider other )
        {
            if (other.CompareTag(ConstValue.c_sPlayerTag))
            {
                mCacheColliders.Add(other);
                PostProcess();
            }
        }

        protected override void OnTriggerExit( Collider other )
        {
            if (other.CompareTag(ConstValue.c_sPlayerTag))
            {
                mCacheColliders.Remove(other);
                PostProcess();
            }
        }


        protected void PostProcess()
        {
            if (mCacheColliders.Count > 0)
                m_onActionEvent.Invoke(this, m_actionId, ((int) En_SwitchState.On).ToString(), null);
            else
                m_onActionEvent.Invoke(this, m_actionId, ((int) En_SwitchState.Off).ToString(), null);
        }


        private void OnDestroy()
        {
            mCacheColliders.Clear();
            mCacheColliders = null;
        }
    }
}