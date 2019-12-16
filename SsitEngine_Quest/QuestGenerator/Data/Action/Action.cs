using System;
using System.Collections.Generic;
using UnityEngine;

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// Actions are tasks that quest givers can ask questers to do to an entity.
    /// </summary>
    [Serializable]
    public class Action
    {
        // This description field is for your public reference."
        [TextArea]
        [SerializeField]
        private string m_description;

        // The display name of this action.
        [SerializeField]
        private string m_displayName;

        // Content for each quest node state and UI category (dialogue, journal, HUD).
        [SerializeField]
        private ActionText m_actionText = new ActionText();

        // this action/condition require check param
        [SerializeField]
        private string m_activeRequiredValue;

        // this action/condition list
        [SerializeField]
        private List<ActionCompletion> m_completion;

        //Send this to Message System when this action's node becomes active. Use ':' to separate parameter from message."
        [SerializeField]
        private string m_sendMessageOnActive;

        //Send this to Message System when this action's node is completed. Use ':' to separate parameter from message.
        [SerializeField]
        private string m_sendMessageOnCompletion;

        //[SerializeField]
        //private Motive[] m_motives;

        //[SerializeField]
        //private ActionRequirement[] m_requirements;

        //[SerializeField]
        //private ActionEffect[] m_effects;

        /// <summary>
        /// Description field for your own public reference.
        /// </summary>
        public string Description
        {
            get { return m_description; }
            set { m_description = value; }
        }

        /// <summary>
        /// The display name of this action.
        /// </summary>
        public string DisplayName
        {
            get { return m_displayName; }
            set { m_displayName = value; }
        }

        /// <summary>
        /// Content for each quest node state and UI category (dialogue, journal, HUD).
        /// </summary>
        public ActionText ActionText
        {
            get { return m_actionText; }
            set { m_actionText = value; }
        }

        /// <summary>
        /// 当前事件的完成条件列表
        /// </summary>
        public List<ActionCompletion> Completion
        {
            get { return m_completion; }
            set { m_completion = value; }
        }

        /// <summary>
        /// 条件激活时发送的消息参数（对当前正在执行的操作对象进行状态访问）
        /// </summary>
        public string ActiveRequiredValue
        {
            get
            {
                return m_activeRequiredValue;
            }

            set
            {
                m_activeRequiredValue = value;
            }
        }

        /// <summary>
        /// 条件激活时发送的消息名称
        /// </summary>
        public string SendMessageOnActive
        {
            get { return m_sendMessageOnActive; }
            set { m_sendMessageOnActive = value; }
        }

        /// <summary>
        /// 条件成立时发送的消息名称
        /// </summary>
        public string SendMessageOnCompletion
        {
            get { return m_sendMessageOnCompletion; }
            set { m_sendMessageOnCompletion = value; }
        }

        //public Motive[] Motives
        //{
        //    get { return m_motives; }
        //    set { m_motives = value; }
        //}
        //public ActionRequirement[] Requirements
        //{
        //    get { return m_requirements; }
        //    set { m_requirements = value; }
        //}

        //public ActionEffect[] Effects
        //{
        //    get { return m_effects; }
        //    set { m_effects = value; }
        //}
    }

}