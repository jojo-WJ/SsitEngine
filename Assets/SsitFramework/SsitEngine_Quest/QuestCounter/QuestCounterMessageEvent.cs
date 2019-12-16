

using UnityEngine;
using System;

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// Specifies how to modify a counter when it receives a message from the MessageSystem.
    /// </summary>
    [Serializable]
    public class QuestCounterMessageEvent
    {

        [Tooltip("Required message sender.")]
        [SerializeField]
        private QuestMessageParticipant m_senderSpecifier = QuestMessageParticipant.Any;

        [Tooltip("Required message sender ID, or any sender if blank. Can also be {QUESTERID} or {QUESTGIVERID}. Sender must have a Quest Giver or Entity component.")]
        [SerializeField]
        private string m_senderID;

        [Tooltip("Required message target.")]
        [SerializeField]
        private QuestMessageParticipant m_targetSpecifier = QuestMessageParticipant.Any;

        [Tooltip("ID of message target. Can also be {QUESTERID} or {QUESTGIVERID}. Leave blank to listen for any target.")]
        [SerializeField]
        private string m_targetID;

        [SerializeField]
        private string m_message;

        [SerializeField]
        private string m_parameter;

        public enum Operation { ModifyByParameter, SetToParameter, ModifyByLiteralValue, SetToLiteralValue }

        [SerializeField]
        private Operation m_operation;

        [SerializeField]
        private int m_literalValue;

        /// <summary>
        /// Required message sender.
        /// </summary>
        public QuestMessageParticipant senderSpecifier
        {
            get { return m_senderSpecifier; }
            set { m_senderSpecifier = value; }
        }

        /// <summary>
        /// Required message sender ID, or any sender if blank. Can also be {QUESTERID} or {QUESTGIVERID}. Sender must have a Quest Giver or Entity component.
        /// </summary>
        public string senderID
        {
            get { return QuestMachineTags.GetIDBySpecifier(senderSpecifier, m_senderID); }
            set { m_senderID = value; }
        }

        /// <summary>
        /// Required message target.
        /// </summary>
        public QuestMessageParticipant targetSpecifier
        {
            get { return m_targetSpecifier; }
            set { m_targetSpecifier = value; }
        }

        /// <summary>
        /// ID of message target. Can also be {QUESTERID} or {QUESTGIVERID}. Leave blank to listen for any target.
        /// </summary>
        public string targetID
        {
            get { return QuestMachineTags.GetIDBySpecifier(targetSpecifier, m_targetID); }
            set { m_targetID = value; }
        }

        /// <summary>
        /// Message that counter should listen for.
        /// </summary>
        public string message
        {
            get { return m_message; }
            set { m_message = value; }
        }

        /// <summary>
        /// Parameter that must be paired with the message, or blank for any.
        /// </summary>
        public string parameter
        {
            get { return m_parameter; }
            set { m_parameter = value; }
        }

        /// <summary>
        /// What to do when the message is received.
        /// </summary>
        public Operation operation
        {
            get { return m_operation; }
            set { m_operation = value; }
        }

        /// <summary>
        /// Value to use when operation is ModifyByLiteralValue or SetToLiteralValue.
        /// </summary>
        public int literalValue
        {
            get { return m_literalValue; }
            set { m_literalValue = value; }
        }

        public QuestCounterMessageEvent() { }

        public QuestCounterMessageEvent(string targetID, string message, string parameter, Operation operation, int literalValue)
        {
            m_targetID = targetID;
            m_message = message;
            m_parameter = parameter;
            m_operation = operation;
            m_literalValue = literalValue;
        }

    }
}
