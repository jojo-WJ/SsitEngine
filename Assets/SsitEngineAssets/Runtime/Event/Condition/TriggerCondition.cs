/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/29 15:25:19                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using Framework.Logic;
using UnityEngine;

namespace SsitEngine.Event.Condition
{
    public class TriggerCondition : Condition
    {
        /// <summary>
        /// 我的缓存对象
        /// </summary>
        private readonly List<Collider> mCacheColliders = new List<Collider>();

        [AddDescribe("标签判断")] public string mCheckLayerMask;

        private void OnTriggerEnter( Collider other )
        {
            if (other.CompareTag(ConstValue.c_sPlayerTag)) mCacheColliders.Add(other);
        }

        private void OnTriggerExit( Collider other )
        {
            mCacheColliders.Remove(other);
        }

        public override void StartChecking( Action trueAction )
        {
            base.StartChecking(trueAction);
        }

        public override void StopChecking()
        {
            base.StopChecking();
        }

        private void OnTimeCallBack()
        {
            StopChecking();
            SetTrue();
        }
    }
}