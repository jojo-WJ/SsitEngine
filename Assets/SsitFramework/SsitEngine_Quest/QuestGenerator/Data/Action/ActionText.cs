using System;
using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// Action text for a quest node state.
    /// </summary>
    [Serializable]
    public class ActionStateText
    {
        [SerializeField] private string m_alertText;

        [SerializeField] private string m_dialogueText;

        [SerializeField] private string m_hudText;

        [SerializeField] private string m_journalText;

        /// <summary>
        /// Shown in dialogue UI when the task created by this action is active.
        /// </summary>
        public string dialogueText
        {
            get => m_dialogueText;
            set => m_dialogueText = value;
        }

        /// <summary>
        /// Shown in journal UI when the task created by this action is active.
        /// </summary>
        public string journalText
        {
            get => m_journalText;
            set => m_journalText = value;
        }

        /// <summary>
        /// Shown in HUD when the task created by this action is active.
        /// </summary>
        public string hudText
        {
            get => m_hudText;
            set => m_hudText = value;
        }

        /// <summary>
        /// Shown in alert UI when 
        /// </summary>
        public string alertText
        {
            get => m_alertText;
            set => m_alertText = value;
        }
    }

    /// <summary>
    /// UI text for actions to use when creating quests.
    /// </summary>
    [Serializable]
    public class ActionText
    {
        [Tooltip(
            "Text to use when the task is active. Optionally specify message to send when node becomes active; use ':' to separate message and parameter.")]
        [SerializeField]
        private ActionStateText m_activeText = new ActionStateText();

        [Tooltip(
            "Text to use when the task is complete. Optionally specify message to send when node is completed; use ':' to separate message and parameter.")]
        [SerializeField]
        private ActionStateText m_completedText = new ActionStateText();

        [SerializeField] private string m_successText;

        /// <summary>
        /// Text to use when the task is active.
        /// </summary>
        public ActionStateText activeText
        {
            get => m_activeText;
            set => m_activeText = value;
        }

        /// <summary>
        /// Text to use when the task is complete.
        /// </summary>
        public ActionStateText completedText
        {
            get => m_completedText;
            set => m_completedText = value;
        }

        /// <summary>
        /// Shown in dialogue UI when returning to giver if this is the goal action.
        /// </summary>
        public string successText
        {
            get => m_successText;
            set => m_successText = value;
        }
    }
}