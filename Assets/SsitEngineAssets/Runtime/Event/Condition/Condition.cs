/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/27 18:39:39                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using SsitEngine.DebugLog;
using SsitEngine.Unity;
using UnityEngine;

namespace SsitEngine.Event.Condition
{
    [Serializable]
    public class Condition : MonoBehaviour
    {
        private bool m_isChecking;

        /// <summary>
        /// Delegate to call when the condition becomes true.
        /// </summary>
        protected Action trueAction = delegate { };

        /// <summary>
        /// True if the condition is currently monitoring the requirements that would make it true.
        /// </summary>
        protected bool IsChecking
        {
            get => m_isChecking;
            set => m_isChecking = true;
        }

        /// <summary>
        /// Tells the condition to start checking; when true, call SetTrue().
        /// </summary>
        /// <param name="trueAction">The method to invoke when the condition becomes true.</param>
        public virtual void StartChecking( Action trueAction )
        {
            IsChecking = true;
            this.trueAction = trueAction;
        }

        /// <summary>
        /// Tells the condition to stop checking.
        /// </summary>
        public virtual void StopChecking()
        {
            IsChecking = false;
        }

        /// <summary>
        /// Sets the condition true, invoking the trueAction.
        /// </summary>
        public virtual void SetTrue()
        {
            if (Engine.Debug)
                SsitDebug.Debug("QuestMachine: " + GetType().Name + ".SetTrue()");
            trueAction();
        }
    }
}