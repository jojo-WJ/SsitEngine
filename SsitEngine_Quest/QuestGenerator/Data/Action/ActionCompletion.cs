using System;
using System.Collections.Generic;
using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// Defines how to know when an action is completed.
    /// </summary>
    [Serializable]
    public class ActionCompletion
    {
        public enum ConditionType
        {
            Default,
            Timer
        }

        public enum Mode
        {
            Message,
            Counter
        }

        // For Counters:

        [Tooltip("Counter name. Entity name will be prefixed to counter name.")] [SerializeField]
        private string m_baseCounterName;

        [Tooltip("what the type of action is?")] [SerializeField]
        private ConditionType m_conditionType;

        [Tooltip("Max value.")] [SerializeField]
        private CounterValueConditionMode m_counterValueMode = CounterValueConditionMode.AtLeast;

        [Tooltip("Initial value.")] [SerializeField]
        private int m_initialValue;

        [Tooltip("Max value.")] [SerializeField]
        private int m_maxValue = 100;

        [Tooltip("Require this message for completion.")] [SerializeField]
        private string m_message;

        [Tooltip("When Update Mode is Messages, these messages affect the counter value.")] [SerializeField]
        private List<QuestCounterMessageEvent> m_messageEventList = new List<QuestCounterMessageEvent>();

        [Tooltip("Min value.")] [SerializeField]
        private int m_minValue;

        [Tooltip("How the action is completed.")] [SerializeField]
        private Mode m_mode;

        [Tooltip("Required this message parameter for completion.")] [SerializeField]
        private string m_parameter;

        [Tooltip("Required value.")] [SerializeField]
        private int m_requiredValue = 1;
        // For Messages:

        [Tooltip(
            "Required message sender ID, or any sender if blank. Can also be {QUESTERID} or {QUESTGIVERID}. Sender must have a Quest Giver or Entity component.")]
        [SerializeField]
        private string m_senderID;

        [Tooltip(
            "Required message target ID, or any target if blank. Can also be {QUESTERID} or {QUESTGIVERID}. Target must have a Quest Giver or Entity component.")]
        [SerializeField]
        private string m_targetID;

        [Tooltip("How this counter updates it value.")] [SerializeField]
        private QuestCounterUpdateMode m_updateMode;

        /// <summary>
        /// Mode by which the action is completed.
        /// </summary>
        public Mode mode
        {
            get => m_mode;
            set => m_mode = value;
        }

        /// <summary>
        /// Required message sender ID, or any sender if blank. Can also be {QUESTERID} or {QUESTGIVERID}. Sender must have a Quest Giver or Entity component.
        /// </summary>
        public string senderID
        {
            get => m_senderID;
            set => m_senderID = value;
        }

        /// <summary>
        /// Required message target ID, or any target if blank. Can also be {QUESTERID} or {QUESTGIVERID}. Target must have a Quest Giver or Entity component.
        /// </summary>
        public string targetID
        {
            get => m_targetID;
            set => m_targetID = value;
        }

        /// <summary>
        /// Required message.
        /// </summary>
        public string message
        {
            get => m_message;
            set => m_message = value;
        }

        /// <summary>
        /// Required message parameter (or blank).
        /// </summary>
        public string parameter
        {
            get => m_parameter;
            set => m_parameter = value;
        }

        /// <summary>
        /// Base counter name that will be prefixed by entity type name.
        /// </summary>
        public string baseCounterName
        {
            get => m_baseCounterName;
            set => m_baseCounterName = value;
        }

        /// <summary>
        /// Initial value.
        /// </summary>
        public int initialValue
        {
            get => m_initialValue;
            set => m_initialValue = value;
        }

        /// <summary>
        /// Min value.
        /// </summary>
        public int minValue
        {
            get => m_minValue;
            set => m_minValue = value;
        }

        /// <summary>
        /// Max value.
        /// </summary>
        public int maxValue
        {
            get => m_maxValue;
            set => m_maxValue = value;
        }

        /// <summary>
        /// The required value for the counter.
        /// </summary>
        public int requiredValue
        {
            get => m_requiredValue;
            set => m_requiredValue = value;
        }

        /// <summary>
        /// How the counter value applies to the condition.
        /// </summary>
        public CounterValueConditionMode counterValueMode
        {
            get => m_counterValueMode;
            set => m_counterValueMode = value;
        }

        /// <summary>
        /// How the counter updates its value.
        /// </summary>
        public QuestCounterUpdateMode updateMode
        {
            get => m_updateMode;
            set => m_updateMode = value;
        }

        /// <summary>
        /// When updateMode is Messages, these message events affect the counter value.
        /// </summary>
        public List<QuestCounterMessageEvent> messageEventList
        {
            get => m_messageEventList;
            set => m_messageEventList = value;
        }

        public ConditionType conditionType
        {
            get => m_conditionType;

            set => m_conditionType = value;
        }
    }
}