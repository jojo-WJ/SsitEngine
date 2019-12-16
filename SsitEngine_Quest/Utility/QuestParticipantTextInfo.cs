using UnityEngine;
using System;

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// Holds text info about a quest partcipant without referencing the participant,
    /// to decouple QuestGiver/Quester from Quest.
    /// </summary>
    [Serializable]
    public class QuestParticipantTextInfo
    {

        [SerializeField]
        private string m_id;

        [SerializeField]
        private string m_displayName;

        [SerializeField]
        private Sprite m_image;


        public string Id { get { return m_id; } }
        
        public string DisplayName { get { return m_displayName; } }

        public Sprite Image { get { return m_image; } }


        public QuestParticipantTextInfo(string id, string displayName, Sprite image)
        {
            m_id = id;
            m_displayName = displayName;
            m_image = image;
        }

    }
}
