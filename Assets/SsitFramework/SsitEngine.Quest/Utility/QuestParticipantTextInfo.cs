using System;
using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// Holds text info about a quest partcipant without referencing the participant,
    /// to decouple QuestGiver/Quester from Quest.
    /// </summary>
    [Serializable]
    public class QuestParticipantTextInfo
    {
        [SerializeField] private string m_displayName;

        [SerializeField] private string m_id;

        [SerializeField] private Sprite m_image;


        public QuestParticipantTextInfo( string id, string displayName, Sprite image )
        {
            m_id = id;
            m_displayName = displayName;
            m_image = image;
        }


        public string Id => m_id;

        public string DisplayName => m_displayName;

        public Sprite Image => m_image;
    }
}