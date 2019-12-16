using UnityEngine;

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// Sets a quest indicator.
    /// </summary>
    public class SetIndicatorQuestAction : QuestAction
    {

        [Tooltip("ID of quest for which this indicator state applies, or blank for this quest.")]
        [SerializeField]
        private string m_questID;

        [Tooltip("ID of entity whose indicator to set, or blank to set quest giver.")]
        [SerializeField]
        private string m_entityID;

        [Tooltip("Quest indicator state to set on the entity.")]
        [SerializeField]
        private QuestIndicatorState m_questIndicatorState;

        public string questID
        {
            get { return m_questID; }
            set { m_questID = value; }
        }

        public string entityID
        {
            get { return m_entityID; }
            set { m_entityID = value; }
        }

        public QuestIndicatorState questIndicatorState
        {
            get { return m_questIndicatorState; }
            set { m_questIndicatorState = value; }
        }

        public string runtimeEntityID
        {
            get
            {
                var s = entityID;
                return (string.IsNullOrEmpty(s) && quest != null) ? quest.QuestGiverId : QuestMachineTags.ReplaceTags(s, quest);
            }
        }

        public override string GetEditorName()
        {
            if (string.IsNullOrEmpty(entityID)) return "Set Indicator";
            return "Set Indicator: " + questID + " " + entityID + " " + questIndicatorState;
        }

        public override void Execute(QuestNode node)
        {
            var affectedQuest = string.IsNullOrEmpty(questID) ? this.quest : QuestUtility.GetQuestInstance(questID);
            if (affectedQuest == null) return;
            affectedQuest.SetQuestIndicatorState(runtimeEntityID, questIndicatorState);
        }

    }

}
