using Framework.Quest;
using Framework.SceneObject;
using SsitEngine;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.QuestManager;
using UnityEngine;
using QuestHelper = Framework.Quest.QuestHelper;

namespace Framework.JQuest
{
    /// <summary>
    /// Sends a message to the MessageSystem.
    /// </summary>
    public class MessageTypeQuestAction : QuestAction
    {
        [Tooltip("Required message sender.")] [SerializeField]
        private QuestMessageParticipant m_senderSpecifier = QuestMessageParticipant.QuestGiver;

        [Tooltip("ID of message sender. Can also be {QUESTERID} or {QUESTGIVERID}. If blank, uses quest giver's ID.")]
        [SerializeField]
        private string m_senderID;

        [Tooltip("Required message target.")] [SerializeField]
        private QuestMessageParticipant m_targetSpecifier = QuestMessageParticipant.Any;

        [Tooltip(
            "ID of message target. Can also be {QUESTERID} or {QUESTGIVERID}. Leave blank to broadcast to all listeners.")]
        [SerializeField]
        private string m_targetID;

        [Tooltip("Message to send.")] [SerializeField]
        private string m_message;

        [Tooltip("Parameter to send with message.")] [SerializeField]
        private string m_parameter;

        [Tooltip("Optional value to pass with message.")] [SerializeField]
        private MessageValue m_value = new MessageValue();

        /// <summary>
        /// Required message sender.
        /// </summary>
        public QuestMessageParticipant SenderSpecifier
        {
            get { return m_senderSpecifier; }
            set { m_senderSpecifier = value; }
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
                    return (quest != null) ? quest.QuestGiverId : null;
                }
                else
                {
                    return QuestMachineTags.GetIDBySpecifier(SenderSpecifier, m_senderID);
                }
            }
            set { m_senderID = value; }
        }

        /// <summary>
        /// Required message target.
        /// </summary>
        public QuestMessageParticipant TargetSpecifier
        {
            get { return m_targetSpecifier; }
            set { m_targetSpecifier = value; }
        }

        /// <summary>
        /// ID of message target. Can also be {QUESTERID} or {QUESTGIVERID}. Leave blank to broadcast to all listeners.
        /// </summary>
        public string TargetId
        {
            get { return QuestMachineTags.GetIDBySpecifier(TargetSpecifier, m_targetID); }
            set { m_targetID = value; }
        }

        /// <summary>
        /// Message to send.
        /// </summary>
        public string Message
        {
            get { return m_message; }
            set { m_message = value; }
        }

        /// <summary>
        /// Parameter to send with message.
        /// </summary>
        public string Parameter
        {
            get { return m_parameter; }
            set { m_parameter = value; }
        }

        /// <summary>
        /// Optional value to pass with message.
        /// </summary>
        public MessageValue Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public string RuntimeSenderId
        {
            get { return QuestMachineTags.ReplaceTags(SenderId, quest); }
        }

        public string RuntimeTargetId
        {
            get { return QuestMachineTags.ReplaceTags(TargetId, quest); }
        }

        public string RuntimeMessage
        {
            get { return QuestMachineTags.ReplaceTags(Message, quest); }
        }

        public string RuntimeParameter
        {
            get { return QuestMachineTags.ReplaceTags(Parameter, quest); }
        }

        public string RuntimeStringValue
        {
            get { return QuestMachineTags.ReplaceTags(Value.StringValue, quest); }
        }

        public override string GetEditorName()
        {
            return string.IsNullOrEmpty(m_message)
                ? "Message"
                : ("Message: " + m_message + " " + m_parameter + " " + m_value.EditorNameValue());
        }

        public override void Execute( QuestNode node )
        {
            if (Value == null)
                Value = new MessageValue();
            ushort msgid = TextUtils.ToUshort(RuntimeMessage);
            if (msgid == 0)
            {
                return;
            }

            //todo:检测当前人物装备是否已经装备
            switch ((En_QuestsMsg) msgid)
            {
                case En_QuestsMsg.En_InteractiveActive:
                    OnInteractiveActive();
                    break;
                case En_QuestsMsg.En_WearEquipActive:
                    OnWearQuestActive();
                    break;
                case En_QuestsMsg.En_ArrivedToActive:
                    OnArrivedToActive();
                    break;
                case En_QuestsMsg.En_TechnologyActive:
                    OnTechnologyActive();
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
                AddTagsToDictionary(Value.StringValue);
        }

        #region Internal Members

        private void OnTechnologyActive()
        {
        }

        private void OnArrivedToActive()
        {
        }

        private void OnWearQuestActive()
        {
            //Parameter Test
            if (string.IsNullOrEmpty(RuntimeTargetId))
                return;

            switch (quest.CompleteMode)
            {
                case QuestCompleteMode.SingleComplet:
                    //单人技能
                {
                    Player player =
                        ObjectManager.Instance.GetObject<Player>(RuntimeTargetId, EnFactoryType.PlayerFactory);

                    string[] quipParms = QuestHelper.DeGeneratorParam(En_QuestsMsg.En_WearEquip, Parameter);

                    //检测人物装备是否包含任务所需装备
                    for (int i = 0; i < quipParms.Length; i++)
                    {
                        if (!player.HasEquip(quipParms[i].ParseByDefault(0)))
                        {
                            return;
                        }
                    }

                    Facade.Instance.SendNotification((ushort) En_QuestsMsg.En_WearEquip,
                        new QuestMessageArgs(0, this, player.Guid, Parameter));
                }
                    break;
                case QuestCompleteMode.CooprationComplete:
                    //同队所有人
                {
                }
                    break;
            }
        }

        private void OnInteractiveActive()
        {
        }

        #endregion
    }
}