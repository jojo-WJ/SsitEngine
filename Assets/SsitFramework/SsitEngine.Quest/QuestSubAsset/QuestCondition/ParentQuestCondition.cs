using System;
using SsitEngine.DebugLog;
using SsitEngine.Unity;
using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// Quest condition that becomes true when a specified number of parent nodes are true.
    /// </summary>
    [Serializable]
    public class ParentQuestCondition : QuestCondition
    {
        [Tooltip("If Parent Count Mode is Min, at least this many parents must be true.")] [SerializeField]
        private int m_minParentCount;

        [Tooltip("How many parents must be true.")] [SerializeField]
        private ConditionCountMode m_parentCountMode = ConditionCountMode.All;

        /// <summary>
        /// How many parents must be true.
        /// </summary>
        public ConditionCountMode ParentCountMode
        {
            get => m_parentCountMode;
            set => m_parentCountMode = value;
        }

        /// <summary>
        /// If parentCountMode is ConditionCountMode.Min, at least this many parents must be true.
        /// </summary>
        public int MinParentCount
        {
            get => m_minParentCount;
            set => m_minParentCount = value;
        }

        public override string GetEditorName()
        {
            switch (ParentCountMode)
            {
                case ConditionCountMode.All:
                    return "Parents: All True";
                case ConditionCountMode.Any:
                    return "Parents: Any True";
                case ConditionCountMode.Min:
                    return "Parents: At Least " + MinParentCount + " True";
            }
            return base.GetEditorName();
        }

        public override void StartChecking( System.Action trueAction )
        {
            base.StartChecking(trueAction);
            ConnectToParentNodes(true);
            CheckTrueParentCount();
        }

        public override void StopChecking()
        {
            base.StopChecking();
            ConnectToParentNodes(false);
        }

        protected void ConnectToParentNodes( bool add )
        {
            if (Engine.Debug)
            {
                SsitDebug.Debug(
                    "Quest Machine: ParentCountQuestCondition.ConnectToParentNodes(" +
                    (add ? "listen for parent changes" : "stop listening for parent changes") + ")", quest);
            }
            if (quest == null || quest.NodeList == null || questNode == null || questNode.ParentList == null)
            {
                return;
            }
            for (var i = 0; i < questNode.ParentList.Count; i++)
            {
                var parentNode = questNode.ParentList[i];
                if (parentNode == null)
                {
                    continue;
                }
                parentNode.OnStateChanged -= OnParentOnStateChange;
                if (add)
                {
                    parentNode.OnStateChanged += OnParentOnStateChange;
                }
            }
        }

        protected void OnParentOnStateChange( QuestNode parentNode )
        {
            if (Engine.Debug)
            {
                SsitDebug.Debug(
                    "Quest Machine: ParentCountQuestCondition.OnParentStateChange(" +
                    (parentNode != null ? parentNode.GetEditorName() : "null") + ")", quest);
            }
            var parentIsTrue = parentNode != null && parentNode.GetState() == QuestNodeState.True;
            if (parentIsTrue)
            {
                CheckTrueParentCount();
            }
        }

        protected void CheckTrueParentCount()
        {
            // Count every time this method is called instead of maintaining a counter that we'd have to include in saved games.
            int nonoptionalCount;
            int optionalCount;
            int totalCount;
            CountTrueParents(QuestNodeState.True, out nonoptionalCount, out optionalCount, out totalCount);
            switch (ParentCountMode)
            {
                case ConditionCountMode.Any:
                    if (totalCount >= 1)
                    {
                        SetTrue();
                    }
                    break;
                case ConditionCountMode.All:
                    if (questNode == null || questNode.NonoptionalParentList == null)
                    {
                        break;
                    }
                    if (nonoptionalCount >= questNode.NonoptionalParentList.Count)
                    {
                        SetTrue();
                    }
                    break;
                case ConditionCountMode.Min:
                    if (totalCount >= MinParentCount)
                    {
                        SetTrue();
                    }
                    break;
                default:
                    if (Debug.isDebugBuild)
                    {
                        Debug.LogWarning(
                            "Quest Machine: Unrecognized ConditionCountMode '" + ParentCountMode +
                            "'. Please contact the developer.", quest);
                    }
                    break;
            }
        }

        protected void CountTrueParents( QuestNodeState requiredState, out int nonoptionalCount, out int optionalCount,
            out int totalCount )
        {
            nonoptionalCount = 0;
            optionalCount = 0;
            if (questNode != null && questNode.ParentList != null)
            {
                for (var i = 0; i < questNode.ParentList.Count; i++)
                {
                    var parentNode = questNode.ParentList[i];
                    if (parentNode == null)
                    {
                        continue;
                    }
                    if (parentNode.GetState() != requiredState)
                    {
                        continue;
                    }
                    if (parentNode.IsOptional)
                    {
                        optionalCount++;
                    }
                    else
                    {
                        nonoptionalCount++;
                    }
                }
            }
            totalCount = nonoptionalCount + optionalCount;
        }
    }
}