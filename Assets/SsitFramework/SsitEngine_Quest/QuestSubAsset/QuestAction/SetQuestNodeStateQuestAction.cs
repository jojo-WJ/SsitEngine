

using UnityEngine;

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// Sets a quest node state.
    /// </summary>
    public class SetQuestNodeStateQuestAction : QuestAction
    {

        [Tooltip("ID of quest. Leave blank to set this quest's state.")]
        [SerializeField]
        private string m_questID;

        [Tooltip("ID of quest node. Leave blank to set this quest node's state.")]
        [SerializeField]
        private string m_questNodeID;

        [Tooltip("New quest node state.")]
        [SerializeField]
        private QuestNodeState m_state;

        /// <summary>
        /// ID of quest. Leave blank to set this quest's state.
        /// </summary>
        public string questID
        {
            get { return (string.IsNullOrEmpty(m_questID) && quest != null) ? quest.Id : m_questID; }
            set { m_questID = value; }
        }

        /// <summary>
        /// ID of quest node. Leave blank to set this quest node's state.
        /// </summary>
        public string questNodeID
        {
            get { return (string.IsNullOrEmpty(m_questNodeID) && questNode != null) ? questNode.Id : m_questNodeID; }
            set { m_questNodeID = value; }
        }

        public QuestNodeState state
        {
            get { return m_state; }
            set { m_state = value; }
        }

        public override string GetEditorName()
        {
            if (string.IsNullOrEmpty(questID) && string.IsNullOrEmpty(questNodeID))
            {
                return "Set Quest Node State: " + state;
            }
            else if (string.IsNullOrEmpty(questID))
            {
                return "Set Quest Node State: '" + questNodeID + "' to " + state;
            }
            else if (string.IsNullOrEmpty(questNodeID))
            {
                return "Set Quest Node State: Quest '" + questID + "' Node (unspecified) to " + state;
            }
            else
            {
                return "Set Quest Node State: Quest '" + questID + "' Node '" + questNodeID + "' to " + state;
            }
        }

        public override void Execute(QuestNode node)
        {
            QuestUtility.SetQuestNodeState(questID, questNodeID, state);
        }

    }

}
