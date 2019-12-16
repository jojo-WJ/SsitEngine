/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/26 19:07:08                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.PureMVC.Interfaces;
using UnityEngine;

namespace SsitEngine.Unity
{
    /// <summary>
    ///     重写Mono基类
    /// </summary>
    public class MonoBase : MonoBehaviour
    {
        /// <summary>
        ///     消息回调
        /// </summary>
        /// <param name="notification"></param>
        public virtual void HandleNotification( INotification notification )
        {
        }
    }
}