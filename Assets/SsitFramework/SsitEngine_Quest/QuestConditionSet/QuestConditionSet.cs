using UnityEngine;
using System;
using System.Collections.Generic;

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// 管理一组条件，当为真时调用委托
    /// 1、条件列表、条件模式、条件判定结果
    /// 2、设置任务引用实例及资产拷贝
    /// 3、条件执行逻辑（检测开始、停止、满足、计数）
    /// </summary>
    [Serializable]
    public class QuestConditionSet
    {

        #region Serialized Fields

        [Tooltip( "Conditions in this condition set." )]
        [SerializeField]
        private List<QuestCondition> m_conditionList = new List<QuestCondition>();

        [Tooltip( "How many conditions need to be true." )]
        [SerializeField]
        private ConditionCountMode m_conditionCountMode;

        [Tooltip( "If the Condition Count Mode is Min, at least this many conditions must be true." )]
        [SerializeField]
        private int m_minConditionCount;

        [HideInInspector]
        [SerializeField]
        private int m_numTrueConditions;

        #endregion

        #region Accessors Properties for Serialized Fields

        /// <summary>
        /// Conditions in this condition set.
        /// </summary>
        public List<QuestCondition> ConditionList
        {
            get { return m_conditionList; }
            set { m_conditionList = value; }
        }

        /// <summary>
        /// How many conditions need to be true for the connection to be true.
        /// </summary>
        public ConditionCountMode ConditionCountMode
        {
            get { return m_conditionCountMode; }
            set { m_conditionCountMode = value; }
        }

        /// <summary>
        /// If the Condition Count Mode is Min, at least this many conditions must be true.
        /// </summary>
        public int MinConditionCount
        {
            get { return m_minConditionCount; }
            set { m_minConditionCount = value; }
        }

        /// <summary>
        /// The number of conditions that have reported true.
        /// </summary>
        public int NumTrueConditions
        {
            get { return m_numTrueConditions; }
            set { m_numTrueConditions = value; }
        }

        public bool IsPassing
        {
            get { return m_isPassing; }
            set { m_isPassing = value; }
        }


        #endregion

        #region Private Fields

        private System.Action m_trueAction = delegate { };

        private bool m_isChecking = false;
        private bool m_isPassing = false;

        #endregion

        #region Initialization and Destruction

        /// <summary>
        /// 设置运行时的实例
        /// </summary>
        /// <param name="quest"></param>
        /// <param name="questNode"></param>
        public void SetRuntimeReferences( Quest quest, QuestNode questNode )
        {
            if (ConditionList == null) return;
            for (int i = 0; i < ConditionList.Count; i++)
            {
                if (ConditionList[i] != null)
                    ConditionList[i].SetRuntimeReferences( quest, questNode );
            }
        }

        public void CloneSubassetsInto( QuestConditionSet copy )
        {
            if (copy == null)
            {
                if (Debug.isDebugBuild)
                    Debug.LogWarning( "Quest Machine: QuestConditionSet.CloneSubassetsInto() failed because copy is invalid." );
                return;
            }
            copy.ConditionList = QuestSubasset.CloneList( ConditionList );
        }

        public void DestroySubassets()
        {
            QuestSubasset.DestroyList( ConditionList );
        }

        #endregion

        #region Condition Checking

        /// <summary>
        /// Resets the true condition count and starts checking conditions.
        /// </summary>
        /// <param name="trueAction"></param>
        public void StartChecking( System.Action trueAction )
        {
            if (m_isPassing || m_isChecking || ConditionList == null)
                return;
            m_isChecking = true;
            m_trueAction = trueAction;
            NumTrueConditions = 0;
            for (int i = 0; i < ConditionList.Count; i++)
            {
                UnityEngine.Assertions.Assert.IsNotNull( ConditionList[i], "Quest Machine: conditionList element " + i + " is null. Does your Conditions list have an invalid entry?" );
                if (ConditionList[i] != null)
                    ConditionList[i].StartChecking( OnTrueCondition );
            }
        }

        /// <summary>
        /// Stops checking conditions.
        /// </summary>
        public void StopChecking()
        {
            if (!m_isChecking || ConditionList == null)
            {
                m_isPassing = true;
                return;
            }
            for (int i = 0; i < ConditionList.Count; i++)
            {
                //--- Don't assert; may be null because application is quitting:
                // UnityEngine.Assertions.Assert.IsNotNull(conditionList[i], "Quest Machine: conditionList element " + i + " is null. Does your Conditions list have an invalid entry?");
                if (ConditionList[i] != null) ConditionList[i].StopChecking();
            }
            m_isChecking = false;
        }

        /// <summary>
        /// True if the conditions are met.
        /// </summary>
        public bool AreConditionsMet
        {
            get
            {
                if (ConditionList == null || ConditionList.Count == 0)
                    return true;
                switch (ConditionCountMode)
                {
                    case ConditionCountMode.All:
                        return (NumTrueConditions >= ConditionList.Count);
                    case ConditionCountMode.Any:
                        return (NumTrueConditions > 0);
                    case ConditionCountMode.Min:
                        return (NumTrueConditions >= MinConditionCount);
                    default:
                        if (Debug.isDebugBuild)
                            Debug.LogWarning( "Quest Machine: Internal error. Unrecognized condition count mode '" + ConditionCountMode + "'. Please contact the developer." );
                        return false;
                }
            }
        }

        private void OnTrueCondition()
        {
            NumTrueConditions++;
            if (AreConditionsMet)
                SetTrue();
        }

        private void SetTrue()
        {
            m_trueAction();
        }

        public static int ConditionCount( QuestConditionSet conditionSet )
        {
            return (conditionSet != null && conditionSet.ConditionList != null) ? conditionSet.ConditionList.Count : 0;
        }

        #endregion

    }

}
