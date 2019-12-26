using System;
using SsitEngine.DebugLog;
using SsitEngine.Unity;
using SsitEngine.Unity.Timer;
using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// Quest condition that becomes true after a specified duration.
    /// </summary>
    [Serializable]
    public class TimerQuestConditionPro : QuestCondition
    {
        private QuestCounter m_counter;

        [Tooltip("Counter to track time left.")] [SerializeField]
        private int m_counterIndex;

        [Tooltip("How the counter value applies to the condition.")] [SerializeField]
        private CounterValueConditionMode m_counterValueMode = CounterValueConditionMode.AtLeast;

        private TimerEventTask m_event;

        [Tooltip("The required value for the Counter Value Mode.")] [SerializeField]
        private QuestNumber m_requiredCounterValue;

        /// <summary>
        /// Index of a counter defined in the quest. Inspect the quest's main info to view/edit counters.
        /// </summary>
        public int counterIndex
        {
            get => m_counterIndex;
            set => m_counterIndex = value;
        }

        /// <summary>
        /// The required value for the Counter Value Mode.
        /// </summary>
        public QuestNumber requiredCounterValue
        {
            get => m_requiredCounterValue;
            set => m_requiredCounterValue = value;
        }


        public override string GetEditorName()
        {
            var counter = quest != null ? quest.GetCounter(counterIndex) : null;
            return counter != null ? "Timer: " + counter.name : "Timer";
        }


        public override void StartChecking( System.Action trueAction )
        {
            base.StartChecking(trueAction);
            m_counter = quest != null ? quest.GetCounter(counterIndex) : null;
            //QuestTimerManager.RegisterTimer(this);
            m_event = Engine.Instance.Platform.AddTimerEvent(TimerEventType.TeveSpanUntil,
                SsitFrameUtils.DefaultPriority, m_requiredCounterValue.literalValue, 0, OnTimeCallBack);
            if (m_event != null)
            {
                if (m_counter != null)
                {
                    m_counter.currentValue = m_requiredCounterValue.literalValue;
                }
                if (Engine.Debug)
                {
                    SsitDebug.Debug("Timer StartChecking 添加计时器事件");
                }
            }
        }


        public override void StopChecking()
        {
            base.StopChecking();
            Engine.Instance.Platform.RemoveTimerEvent(m_event);
            if (Engine.Debug)
            {
                SsitDebug.Debug("Timer StartChecking 移除计时器事件");
            }
        }


        private void OnTimeCallBack( TimerEventTask eve, float timeElapsed, object data )
        {
            if (m_counter == null)
            {
                return;
            }

            var temp = Mathf.CeilToInt(m_requiredCounterValue.literalValue - timeElapsed);
            //防止每帧发送通知
            if (m_counter.currentValue != temp)
            {
                m_counter.currentValue = temp;
            }
            if (timeElapsed >= m_requiredCounterValue.literalValue)
            {
                if (Engine.Debug)
                {
                    SsitDebug.Debug(
                        "Quest Machine: TimerQuestCondition '" + m_counter.name +
                        "' timer ran out. Setting condition true." + "timeElapsed:" + timeElapsed, quest);
                }
                StopChecking();
                SetTrue();
            }
        }
    }
}