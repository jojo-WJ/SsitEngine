/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/29 12:12:40                     
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;

namespace Framework.SceneObject.Trigger
{
    [RequireComponent(typeof(SphereCollider))]
    public class TriggerSphere : Trigger
    {
        private bool isTrigger;

        protected override void OnTriggerEnter( Collider other )
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
        protected override void OnTriggerExit( Collider other )
        {
        }
    }
}