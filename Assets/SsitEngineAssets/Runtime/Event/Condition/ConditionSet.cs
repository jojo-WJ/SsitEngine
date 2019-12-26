/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/27 18:42:12                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using SsitEngine.DebugLog;
using SsitEngine.QuestManager;
using SsitEngine.Unity;
using UnityEngine;
using UnityEngine.Assertions;
using Action = System.Action;

namespace SsitEngine.Event.Condition
{
    [Serializable]
    public class ConditionSet
    {
        #region Initialization and Destruction

        public void Destroy()
        {
        }

        #endregion

        #region Serialized Fields

        [Tooltip("Conditions in this condition set.")] [SerializeField]
        private List<Condition> m_conditionList = new List<Condition>();

        [Tooltip("How many conditions need to be true.")] [SerializeField]
        private ConditionCountMode m_conditionCountMode;

        [Tooltip("If the Condition Count Mode is Min, at least this many conditions must be true.")] [SerializeField]
        private int m_minConditionCount;

        [HideInInspector] [SerializeField] private int m_numTrueConditions;

        #endregion

        #region Accessors Properties for Serialized Fields

        /// <summary>
        /// Conditions in this condition set.
        /// </summary>
        public List<Condition> ConditionList
        {
            get => m_conditionList;
            set => m_conditionList = value;
        }

        /// <summary>
        /// How many conditions need to be true for the connection to be true.
        /// </summary>
        public ConditionCountMode ConditionCountMode
        {
            get => m_conditionCountMode;
            set => m_conditionCountMode = value;
        }

        /// <summary>
        /// If the Condition Count Mode is Min, at least this many conditions must be true.
        /// </summary>
        public int MinConditionCount
        {
            get => m_minConditionCount;
            set => m_minConditionCount = value;
        }

        /// <summary>
        /// The number of conditions that have reported true.
        /// </summary>
        public int NumTrueConditions
        {
            get => m_numTrueConditions;
            set => m_numTrueConditions = value;
        }

        public bool IsPassing
        {
            get => m_isPassing;
            set => m_isPassing = value;
        }

        #endregion

        #region Private Fields

        private Action m_trueAction = delegate { };

        private bool m_isChecking;
        private bool m_isPassing;

        #endregion

        #region Condition Checking

        /// <summary>
        /// Resets the true condition count and starts checking conditions.
        /// </summary>
        /// <param name="trueAction"></param>
        public void StartChecking( Action trueAction )
        {
            if (m_isPassing || m_isChecking || ConditionList == null)
                return;
            m_isChecking = true;
            m_trueAction = trueAction;
            NumTrueConditions = 0;
            for (var i = 0; i < ConditionList.Count; i++)
            {
                Assert.IsNotNull(ConditionList[i],
                    "Quest Machine: conditionList element " + i +
                    " is null. Does your Conditions list have an invalid entry?");
                if (ConditionList[i] != null)
                    ConditionList[i].StartChecking(OnTrueCondition);
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
            for (var i = 0; i < ConditionList.Count; i++)
                //--- Don't assert; may be null because application is quitting:
                // UnityEngine.Assertions.Assert.IsNotNull(conditionList[i], "Quest Machine: conditionList element " + i + " is null. Does your Conditions list have an invalid entry?");
                if (ConditionList[i] != null)
                    ConditionList[i].StopChecking();
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
                        return NumTrueConditions >= ConditionList.Count;
                    case ConditionCountMode.Any:
                        return NumTrueConditions > 0;
                    case ConditionCountMode.Min:
                        return NumTrueConditions >= MinConditionCount;
                    default:
                        if (Engine.Debug)
                            SsitDebug.Warning("Quest Machine: Internal error. Unrecognized condition count mode '" +
                                              ConditionCountMode + "'. Please contact the developer.");
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
            return conditionSet != null && conditionSet.ConditionList != null ? conditionSet.ConditionList.Count : 0;
        }

        #endregion
    }
}