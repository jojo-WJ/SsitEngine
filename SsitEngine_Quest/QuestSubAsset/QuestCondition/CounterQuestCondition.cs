using System;
using SsitEngine.DebugLog;
using SsitEngine.Unity;
using UnityEngine;

namespace SsitEngine.QuestManager
{
    public enum CounterValueConditionMode
    {
        AtLeast,
        AtMost
    }

    /// <summary>
    /// Quest condition that becomes true when a counter reaches a specified value.
    /// </summary>
    public class CounterQuestCondition : QuestCondition
    {
        [NonSerialized] private QuestCounter m_counter;

        [Tooltip("Index of a counter defined in the quest. Inspect the quest's main info to view/edit counters.")]
        [SerializeField]
        private int m_counterIndex;

        [Tooltip("How the counter value applies to the condition.")] [SerializeField]
        private CounterValueConditionMode m_counterValueMode = CounterValueConditionMode.AtLeast;

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
        /// How the counter value applies to the condition.
        /// </summary>
        public CounterValueConditionMode counterValueMode
        {
            get => m_counterValueMode;
            set => m_counterValueMode = value;
        }

        /// <summary>
        /// The required value for the Counter Value Mode.
        /// </summary>
        public QuestNumber requiredCounterValue
        {
            get => m_requiredCounterValue;
            set => m_requiredCounterValue = value;
        }

        public QuestCounter counter
        {
            get
            {
                if (m_counter == null)
                {
                    m_counter = quest.GetCounter(counterIndex);
                }
                return m_counter;
            }
        }

        public override string GetEditorName()
        {
            var counter = quest != null ? quest.GetCounter(counterIndex) : null;
            if (counter == null || requiredCounterValue == null)
            {
                return "Counter";
            }
            return "Counter: " + counter.name +
                   (counterValueMode == CounterValueConditionMode.AtLeast ? " >= " : " <= ") +
                   requiredCounterValue.GetEditorName(quest);
        }

        public override void StartChecking( System.Action trueAction )
        {
            base.StartChecking(trueAction);
            if (quest == null)
            {
                if (Debug.isDebugBuild)
                {
                    Debug.LogWarning(
                        "Quest Machine: CounterQuestCondition was passed a null quest. Can't start checking.");
                }
                return;
            }
            if (counter == null)
            {
                if (Debug.isDebugBuild)
                {
                    Debug.LogWarning("Quest Machine: CounterQuestCondition can't find a counter at index " +
                                     counterIndex +
                                     "' in quest '" + quest.GetEditorName() + "'.  Can't start checking.");
                }
                return;
            }
            if (IsCounterConditionTrue(counter))
            {
                SetTrue();
            }
            else
            {
                counter.changed -= OnCounterChanged;
                counter.changed += OnCounterChanged;
            }
        }

        public override void StopChecking()
        {
            base.StopChecking();
            if (counter != null)
            {
                counter.changed -= OnCounterChanged;
            }
        }

        protected virtual void OnCounterChanged( QuestCounter counter )
        {
            if (IsCounterConditionTrue(counter))
            {
                SetTrue();
            }
        }

        protected virtual bool IsCounterConditionTrue( QuestCounter counter )
        {
            if (counter == null)
            {
                return false;
            }
            if (requiredCounterValue == null)
            {
                if (Engine.Debug)
                {
                    SsitDebug.Warning(
                        "QuestMachine: QuestCounterCondition.OnCounterChanged(" + counter.name + " = " +
                        counter.currentValue + "): requiredCounterValue field is null. Please contact the developer.",
                        quest);
                }
                return false;
            }
            if (Engine.Debug)
            {
                SsitDebug.Debug(
                    "QuestMachine: QuestCounterCondition.OnCounterChanged(" + counter.name + " = " +
                    counter.currentValue + ")", quest);
            }
            switch (counterValueMode)
            {
                case CounterValueConditionMode.AtLeast:
                    if (counter.currentValue >= requiredCounterValue.GetValue(quest))
                    {
                        return true;
                    }
                    break;
                case CounterValueConditionMode.AtMost:
                    if (counter.currentValue <= requiredCounterValue.GetValue(quest))
                    {
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}