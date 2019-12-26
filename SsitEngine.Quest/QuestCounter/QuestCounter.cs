using System;
using System.Collections.Generic;
using SsitEngine.DebugLog;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// Holds an integer that a quest can use to track a value. Counters can be updated by
    /// messages (e.g., "Kill"+"Orc") or a DataSynchronizer. When the value changes, the 
    /// counter invokes a changed event.
    /// </summary>
    [Serializable]
    public class QuestCounter : IMediator
    {
        #region Messages

        public void SetListeners( bool enable )
        {
            if (!Application.isPlaying || enable && m_isListening || !enable && !m_isListening)
            {
                return;
            }
            m_isListening = enable;
            if (enable)
            {
                if (Engine.Debug)
                {
                    SsitDebug.Debug("QuestCounter:RegisterMediator: MessageMediator Name:: " + GetHashCode());
                }
                Facade.Instance.RegisterMediator(this);
            }
            else
            {
                if (Engine.Debug)
                {
                    SsitDebug.Debug("QuestCounter:RemoveMediator: MessageMediator Name:: " + GetHashCode());
                }
                Facade.Instance.RemoveMediator(MediatorName);
            }
        }

        #endregion

        #region Serialized Fields

        [Tooltip("Quest counter name.")] [SerializeField]
        private string m_name;

        [Tooltip("当前值。这个值总是夹在最小值和最大值之间。确保正确设置最小值和最大值。")] [SerializeField]
        private int m_currentValue;

        [Tooltip("将初始值设置为最小值和最大值之间的随机值")] [SerializeField]
        private bool m_randomizeInitialValue;

        [Tooltip("Min value.")] [SerializeField]
        private int m_minValue;

        [Tooltip("Max value.")] [SerializeField]
        private int m_maxValue = 100;

        [Tooltip("计数器的更新模式")] [SerializeField] private QuestCounterUpdateMode m_updateMode;

        [Tooltip("当更新模式为消息时，这些消息会影响计数器值")] [SerializeField]
        private List<QuestCounterMessageEvent> m_messageEventList = new List<QuestCounterMessageEvent>();

        #endregion

        #region Accessor Properties for Serialized Fields

        /// <summary>
        /// Quest counter name.
        /// </summary>
        public string name
        {
            get => m_name;
            set => m_name = value;
        }

        /// <summary>
        /// Current value. When you set this property, it notifies listeners.
        /// </summary>
        public int currentValue
        {
            get => m_currentValue;
            set => SetValue(value);
        }

        /// <summary>
        /// Initialize the current value to a random value between min and max value.
        /// </summary>
        public bool randomizeInitialValue
        {
            get => m_randomizeInitialValue;
            set => m_randomizeInitialValue = value;
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
        /// How this counter updates its value.
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

        public event QuestCounterParameterDelegate changed = delegate { };

        #endregion

        #region Private Fields

        private bool m_isListening;

        private Quest m_quest;

        private string questID => m_quest != null ? m_quest.Id : string.Empty;

        #endregion

        #region Initialization

        public QuestCounter()
        {
        }

        public QuestCounter( string name, int currentValue, int minValue, int maxValue,
            QuestCounterUpdateMode updateMode )
        {
            m_name = name;
            m_currentValue = currentValue;
            m_minValue = minValue;
            m_maxValue = maxValue;
            m_updateMode = QuestCounterUpdateMode.Messages;
        }

        public void SetRuntimeReferences( Quest quest )
        {
            m_quest = quest;
        }

        public void InitializeToRandomValue()
        {
            if (!randomizeInitialValue)
            {
                return;
            }
            m_currentValue = Random.Range(minValue, maxValue + 1);
        }

        public void SetValue( int newValue,
            QuestCounterSetValueMode setValueMode = QuestCounterSetValueMode.InformListeners )
        {
            m_currentValue = Mathf.Clamp(newValue, minValue, maxValue);
            if (setValueMode != QuestCounterSetValueMode.DontInformListeners)
            {
                var informDataSync = updateMode == QuestCounterUpdateMode.DataSync &&
                                     setValueMode != QuestCounterSetValueMode.DontInformDataSync;
                if (informDataSync)
                {
                    QuestUtility.SendNotification((ushort) EnQuestEvent.DataSourceValueChangedMessage, this, null, name,
                        currentValue);
                }
                QuestUtility.SendNotification((ushort) EnQuestEvent.QuestCounterChangedMessage, this, questID, name,
                    currentValue);
                try
                {
                    changed(this);
                }
                catch (Exception e) // Don't let exceptions in user-added events break our code.
                {
                    if (Debug.isDebugBuild)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }

        #endregion

        #region IMeditor

        public string MediatorName => "QuestCounter_" + GetHashCode();

        public object ViewComponent
        {
            get => this;
            set { }
        }

        public IList<ushort> ListNotificationInterests()
        {
            IList<ushort> msgs = new List<ushort>();

            switch (updateMode)
            {
                case QuestCounterUpdateMode.DataSync:
                    msgs.Add((ushort) EnQuestEvent.DataSourceValueChangedMessage);

                    break;
                case QuestCounterUpdateMode.Messages:
                    msgs.Add((ushort) EnQuestEvent.SetQuestCounter);
                    msgs.Add((ushort) EnQuestEvent.IncrementQuestCounter);
                    break;
                default:
                    if (Debug.isDebugBuild)
                    {
                        Debug.LogWarning(
                            "Quest Machine: Internal error. Unrecognized counter update mode '" + updateMode +
                            "'. Please contact the developer.", m_quest);
                    }
                    break;
            }
            if (messageEventList != null)
            {
                for (var i = 0; i < messageEventList.Count; i++)
                {
                    var messageEvent = messageEventList[i];
                    if (messageEvent != null)
                    {
                        var messgage = TextUtils.ToUshort(messageEvent.message);
                        if (messgage != 0)
                        {
                            msgs.Add(messgage);
                        }
                        //msgs.Add(QuestMachineTags.ReplaceTags(messageEvent.message, m_quest));
                    }
                }
            }


            return msgs;
        }

        public void HandleNotification( INotification notification )
        {
            var messageArgs = notification.Body as QuestMessageArgs;
            if (messageArgs == null)
            {
                return;
            }
            if (Engine.Debug)
            {
                Debug.Log(
                    "Quest Machine: QuestCounter[" + name + "].OnMessage(" + messageArgs.Id + ", " +
                    messageArgs.parameter + ")", m_quest);
            }
            var msg = (EnQuestEvent) messageArgs.msgId;
            switch (msg)
            {
                case EnQuestEvent.DataSourceValueChangedMessage:
                    m_currentValue = messageArgs.IntValue;
                    break;
                case EnQuestEvent.SetQuestCounter:
                    m_currentValue = messageArgs.IntValue;
                    break;
                case EnQuestEvent.IncrementQuestCounter:
                    m_currentValue += messageArgs.IntValue;
                    break;
                default:
                    if (messageEventList == null)
                    {
                        break;
                    }
                    for (var i = 0; i < messageEventList.Count; i++)
                    {
                        var messageEvent = messageEventList[i];
                        if (messageEvent != null && messageArgs.Matches(messageEvent.message) &&
                            QuestUtility.IsRequiredID(messageArgs.sender,
                                QuestMachineTags.ReplaceTags(messageEvent.senderID, m_quest)) &&
                            QuestUtility.IsRequiredID(messageArgs.target,
                                QuestMachineTags.ReplaceTags(messageEvent.targetID, m_quest)))
                        {
                            switch (messageEvent.operation)
                            {
                                case QuestCounterMessageEvent.Operation.ModifyByLiteralValue:
                                    m_currentValue += messageEvent.literalValue;
                                    break;
                                case QuestCounterMessageEvent.Operation.ModifyByParameter:
                                    m_currentValue += messageArgs.IntValue;
                                    break;
                                case QuestCounterMessageEvent.Operation.SetToLiteralValue:
                                    m_currentValue = messageEvent.literalValue;
                                    break;
                                case QuestCounterMessageEvent.Operation.SetToParameter:
                                    m_currentValue = messageArgs.IntValue;
                                    break;
                            }
                        }
                    }
                    break;
            }
            SetValue(m_currentValue, QuestCounterSetValueMode.DontInformDataSync);
        }

        public void OnRegister()
        {
        }

        public void OnRemove()
        {
        }

        #endregion
    }
}