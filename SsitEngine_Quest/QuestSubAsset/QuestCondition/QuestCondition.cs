using UnityEngine;
using System.Collections.Generic;
using SsitEngine.DebugLog;
using SsitEngine.Unity;

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// 任务条件的抽象基类
    /// 1、设置任务节点引用
    /// 2、条件检测（开始检测、停止检测）
    /// 3、处理handles
    /// </summary>
    public abstract class QuestCondition : QuestSubasset
    {

        /// <summary>
        /// Delegate to call when the condition becomes true.
        /// </summary>
        protected System.Action trueAction = delegate { };

        private bool m_isChecking = false;

        /// <summary>
        /// True if the condition is currently monitoring the requirements that would make it true.
        /// </summary>
        protected bool IsChecking
        {
            get { return m_isChecking; }
            set { m_isChecking = true; }
        }

   
        public override void SetRuntimeReferences(Quest quest, QuestNode questNode)
        {
            base.SetRuntimeReferences(quest, questNode);
            IsChecking = false;
        }

        /// <summary>
        /// Tells the condition to start checking; when true, call SetTrue().
        /// </summary>
        /// <param name="trueAction">The method to invoke when the condition becomes true.</param>
        public virtual void StartChecking(System.Action trueAction)
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
                SsitDebug.Debug( "QuestMachine: " + GetType().Name + ".SetTrue()", quest );
            trueAction();
        }

    }

}
