

using UnityEngine;

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// Sets a quest state.
    /// </summary>
    public class SetQuestStateQuestAction : QuestAction
    {

        [Tooltip("ID of quest. Leave blank to set this quest's state.")]
        [SerializeField]
        private string m_questID;

        [Tooltip("New quest state.")]
        [SerializeField]
        private QuestState m_state;

        public string questID
        {
            get { return (string.IsNullOrEmpty(m_questID) && quest != null) ? quest.Id : m_questID; }
            set { m_questID = value; }
        }

        public QuestState state
        {
            get { return m_state; }
            set { m_state = value; }
        }

        public override string GetEditorName()
        {
            return string.IsNullOrEmpty(questID) ? ("Set Quest State: " + state) : ("Set Quest State: Quest '" + questID + "' to " + state);
        }

        public override void Execute(QuestNode node)
        {
            QuestUtility.SetQuestState(questID, state);
        }

    }

}
