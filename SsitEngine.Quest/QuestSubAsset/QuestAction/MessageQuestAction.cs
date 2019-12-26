using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// Sends a message to the MessageSystem.
    /// </summary>
    public class MessageQuestAction : QuestAction
    {
        [Tooltip("Message to send.")] [SerializeField]
        private string m_message;

        [Tooltip("Parameter to send with message.")] [SerializeField]
        private string m_parameter;

        [Tooltip("ID of message sender. Can also be {QUESTERID} or {QUESTGIVERID}. If blank, uses quest giver's ID.")]
        [SerializeField]
        private string m_senderID;

        [Tooltip("Required message sender.")] [SerializeField]
        private QuestMessageParticipant m_senderSpecifier = QuestMessageParticipant.QuestGiver;

        [Tooltip(
            "ID of message target. Can also be {QUESTERID} or {QUESTGIVERID}. Leave blank to broadcast to all listeners.")]
        [SerializeField]
        private string m_targetID;

        [Tooltip("Required message target.")] [SerializeField]
        private QuestMessageParticipant m_targetSpecifier = QuestMessageParticipant.Any;

        [Tooltip("Optional value to pass with message.")] [SerializeField]
        private MessageValue m_value = new MessageValue();

        /// <summary>
        /// Required message sender.
        /// </summary>
        public QuestMessageParticipant SenderSpecifier
        {
            get => m_senderSpecifier;
            set => m_senderSpecifier = value;
        }

        /// <summary>
        /// ID of message sender. If blank, uses quest giver's ID.
        /// </summary>
        public string SenderId
        {
            get
            {
                if (SenderSpecifier == QuestMessageParticipant.QuestGiver || string.IsNullOrEmpty(m_senderID) ||
                    m_senderID == QuestMachineTags.QUESTGIVER)
                {
                    return quest != null ? quest.QuestGiverId : null;
                }
                return QuestMachineTags.GetIDBySpecifier(SenderSpecifier, m_senderID);
            }
            set => m_senderID = value;
        }

        /// <summary>
        /// Required message target.
        /// </summary>
        public QuestMessageParticipant TargetSpecifier
        {
            get => m_targetSpecifier;
            set => m_targetSpecifier = value;
        }

        /// <summary>
        /// ID of message target. Can also be {QUESTERID} or {QUESTGIVERID}. Leave blank to broadcast to all listeners.
        /// </summary>
        public string TargetId
        {
            get => QuestMachineTags.GetIDBySpecifier(TargetSpecifier, m_targetID);
            set => m_targetID = value;
        }

        /// <summary>
        /// Message to send.
        /// </summary>
        public string Message
        {
            get => m_message;
            set => m_message = value;
        }

        /// <summary>
        /// Parameter to send with message.
        /// </summary>
        public string Parameter
        {
            get => m_parameter;
            set => m_parameter = value;
        }

        /// <summary>
        /// Optional value to pass with message.
        /// </summary>
        public MessageValue Value
        {
            get => m_value;
            set => m_value = value;
        }

        public string RuntimeSenderId => QuestMachineTags.ReplaceTags(SenderId, quest);

        public string RuntimeTargetId => QuestMachineTags.ReplaceTags(TargetId, quest);

        public string RuntimeMessage => QuestMachineTags.ReplaceTags(Message, quest);

        public string RuntimeParameter => QuestMachineTags.ReplaceTags(Parameter, quest);

        public string RuntimeStringValue => QuestMachineTags.ReplaceTags(Value.StringValue, quest);

        public override string GetEditorName()
        {
            return string.IsNullOrEmpty(m_message)
                ? "Message"
                : "Message: " + m_message + " " + m_parameter + " " + m_value.EditorNameValue();
        }

        /// <summary>
        /// 事件执行派生方法
        /// </summary>
        /// <param name="node">任务节点</param>
        public override void Execute( QuestNode node )
        {
            if (Value == null)
            {
                Value = new MessageValue();
            }
            var msgid = TextUtils.ToUshort(RuntimeMessage);
            if (msgid == 0)
            {
                return;
            }
            switch (Value.ValueType)
            {
                case MessageValueType.Int:
                    QuestUtility.SendNotification(msgid, RuntimeSenderId, RuntimeTargetId, RuntimeParameter,
                        Value.IntValue);
                    break;
                case MessageValueType.String:
                    QuestUtility.SendNotification(msgid, RuntimeSenderId, RuntimeTargetId, RuntimeParameter,
                        RuntimeStringValue);
                    break;
                default:
                {
                    if (node != null)
                    {
                        QuestUtility.SendNotification(msgid, RuntimeSenderId, RuntimeTargetId, RuntimeParameter, node);
                    }
                    else
                    {
                        QuestUtility.SendNotification(msgid, RuntimeSenderId, RuntimeTargetId, RuntimeParameter, quest);
                    }
                }

                    break;
            }
        }

        public override void AddTagsToDictionary()
        {
            AddTagsToDictionary(SenderId);
            AddTagsToDictionary(TargetId);
            AddTagsToDictionary(Message);
            AddTagsToDictionary(Parameter);
            if (Value != null && Value.ValueType == MessageValueType.String)
            {
                AddTagsToDictionary(Value.StringValue);
            }
        }
    }
}