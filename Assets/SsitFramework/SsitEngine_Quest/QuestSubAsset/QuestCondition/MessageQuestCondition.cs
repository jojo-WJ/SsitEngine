using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.DebugLog;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// 从消息系统收到消息时变为真的请求条件
    /// </summary>
    [Serializable]
    public class MessageQuestCondition : QuestCondition
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

        [Tooltip("Required message target ID, or any target if blank. Can also be {QUESTERID} or {QUESTGIVERID}. Target must have a Quest Giver or Entity component.")]
        [SerializeField]
        private string m_targetID;

        [Tooltip("Required message. Condition is true when this message is received with the parameter below.")]
        [SerializeField]
        private string m_message;

        [Tooltip("Required parameter for message. Condition is true when the message above is received with this parameter. (Leave blank to accept any parameter.)")]
        [SerializeField]
        private string m_parameter;

        [Tooltip("Additional value to expected with the message.")]
        [SerializeField]
        private MessageValue m_value;

        private Dictionary<string, int> receiveEntityMap;
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
        public string SenderId
        {
            get { return QuestMachineTags.GetIDBySpecifier(senderSpecifier, m_senderID); }
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
        /// Required message target ID, or any target if blank. Can also be {QUESTERID} or {QUESTGIVERID}. Target must have a Quest Giver or Entity component.
        /// </summary>
        public string TargetId
        {
            get { return QuestMachineTags.GetIDBySpecifier(TargetSpecifier, m_targetID); }
            set { m_targetID = value; }
        }

        /// <summary>
        /// Required message. Condition is true when this message is received with the parameter below.
        /// </summary>
        public string Message
        {
            get { return m_message; }
            set { m_message = value; }
        }

        /// <summary>
        /// Required parameter for message. Condition is true when the message above is received with this parameter. (Leave blank to accept any parameter.)
        /// </summary>
        public string Parameter
        {
            get { return m_parameter; }
            set { m_parameter = value; }
        }

        /// <summary>
        /// Additional value to expected with the message.
        /// </summary>
        public MessageValue Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public string RuntimeSenderId { get { return QuestMachineTags.ReplaceTags(SenderId, quest); } }

        public string RuntimeTargetId { get { return QuestMachineTags.ReplaceTags(TargetId, quest); } }

        public string RuntimeMessage { get { return QuestMachineTags.ReplaceTags(Message, quest); } }

        public ushort RuntimeUshortMessage { get { return TextUtils.ToUshort(RuntimeMessage); } }

        public string RuntimeParameter { get { return QuestMachineTags.ReplaceTags(Parameter, quest); } }

        public string RuntimeStringValue { get { return (Value != null) ? QuestMachineTags.ReplaceTags(Value.StringValue, quest) : string.Empty; } }

        public override string GetEditorName()
        {
            return string.IsNullOrEmpty(Message) ? "Message" : "Message: " + Message + " " + Parameter + " " + Value.EditorNameValue();
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

        public override void StartChecking(System.Action trueAction)
        {
            base.StartChecking(trueAction);
            //SsitDebug.Debug("StartChecking:: MessageQuestCondition Hash:: " + this.GetHashCode().ToString());

            if (RuntimeUshortMessage != 0)
            {
                Facade.Instance.RegisterObservers(this, RuntimeUshortMessage, HandleNotification);
            }
            else
            {
                SsitDebug.Error($"任务消息id 异常{RuntimeMessage}");
            }
        }

        public override void StopChecking()
        {
            base.StopChecking();
            //SsitDebug.Debug("StopChecking:: MessageQuestCondition Hash:: " + this.GetHashCode().ToString());
            if (receiveEntityMap != null)
            {
                receiveEntityMap.Clear();
                receiveEntityMap = null;
            }
            if (RuntimeUshortMessage != 0)
            {
                Facade.Instance.RemoveObservers(this, RuntimeUshortMessage);
            }
            else
            {
                SsitDebug.Error($"任务消息id 异常{RuntimeMessage}");
            }

        }



        #region HandleNotification

        public void HandleNotification(INotification notification)
        {
            QuestMessageArgs messageArgs = (QuestMessageArgs)notification.Body;

            if (/*!(QuestUtility.IsRequiredID(messageArgs.sender, RuntimeSenderId) &&*/!string.Equals(RuntimeParameter, messageArgs.parameter))
                return;
            SsitDebug.Debug("Quest Machine: MessageQuestCondition.OnMessage( " + messageArgs.msgId + ", " + messageArgs.parameter + ")", quest);
            switch (quest.CompleteMode)
            {
                case QuestCompleteMode.SingleComplet:
                    if (QuestUtility.IsRequiredID(messageArgs.target, RuntimeTargetId))
                    {
                        SetTrue();
                    }
                    break;
                case QuestCompleteMode.CooprationComplete:
                    if (receiveEntityMap == null)
                    {
                        receiveEntityMap = new Dictionary<string, int>();
                    }
                    if (messageArgs.FirstValue == null)
                    {
                        Debug.Log("messageArgs.FirstValue is null");
                        return;
                    }
                    var handler = QuestManager.Instance.GetAttachHandler();
                    if (receiveEntityMap.ContainsKey(messageArgs.StringValue))
                    {
                        receiveEntityMap[messageArgs.StringValue]++;
                    }
                    else
                    {
                        receiveEntityMap.Add(messageArgs.StringValue, 0);
                    }
                    if (handler != null)
                    {
                        bool isTrue = handler(quest, receiveEntityMap.Count);
                        if (isTrue)
                        {
                            SetTrue();
                        }
                    }
                    break;
            }
        }


        private bool IsRequiredValue(QuestMessageArgs messageArgs)
        {
            if (Value == null)
                return true;
            if (Value.ValueType == MessageValueType.None)
                return true;
            if (messageArgs.FirstValue == null)
                return false;
            switch (Value.ValueType)
            {
                case MessageValueType.String:
                    return QuestUtility.ArgToString(messageArgs.FirstValue) == RuntimeStringValue;
                case MessageValueType.Int:
                    return QuestUtility.ArgToInt(messageArgs.FirstValue) == Value.IntValue;
                default:
                    Debug.LogError("Quest Machine: Unhandled MessageValueType " + Value.ValueType + ". Please contact the developer.", quest);
                    return false;
            }
        }
        #endregion

    }

}
