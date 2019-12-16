using System;
using UnityEngine;

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// A quest list with an ID.
    /// </summary>
    [AddComponentMenu( "" )] // Just a base class.
    public class IdentifiableQuestListContainer : QuestListContainer
    {

        #region Serialized Fields

        [Tooltip( "The ID that uniquely identifies this entity. If unassigned, uses the Quest Entity's Display Name if present." )]
        [SerializeField]
        protected string m_id;

        [Tooltip( "The name shown in UIs. If blank, uses the Quest Entity's Display Name if present." )]
        [SerializeField]
        protected string m_displayName;

        [Tooltip( "The image shown in UIs. If unassigned, uses the Quest Entity's Image if present." )]
        [SerializeField]
        protected Sprite m_image;

        #endregion

        #region Property Accessors to Serialized Fields

        //private string m_fallbackID = null; // Use this ID (QuestEntity's) if id QuestGiver's ID isn't set.

        /// <summary>
        /// The ID that uniquely identifies this quest giver. When the quester (e.g., player) accepts
        /// a quest from this quest giver, the quester's instance will have a reference to this ID so
        /// the quester knows who gave the quest.
        /// </summary>
        public string id
        {
            get
            {
                if (!string.IsNullOrEmpty( m_id ))
                    return m_id;
                //if (string.IsNullOrEmpty( m_fallbackID ))
                //{
                //    var entity = GetComponentInChildren<QuestEntity>();
                //    m_fallbackID = (entity != null) ? entity.DisplayName : name;
                //}
                //return m_fallbackID;
                return null;
            }
            set
            {
                m_id = value;
            }
        }

        /// <summary>
        /// The name shown in UIs. If blank, uses the Quest Entity's Display Name if present.
        /// </summary>
        public string displayName
        {
            get { return m_displayName; }
            set { m_displayName = value; }
        }

        /// <summary>
        /// The image shown in UIs. If blank, uses the Quest Entity's Image if present.
        /// </summary>
        public Sprite image
        {
            get { return m_image; }
            set { m_image = value; }
        }

        #endregion

        #region Initialization

        public override void OnEnable()
        {
            if (!string.IsNullOrEmpty( id ))
            {
                //Debug.Log( "RegisterQuestListContainer" );
                QuestUtility.RegisterQuestListContainer( this );
            }
        }

        public override void OnDisable()
        {
            if (!string.IsNullOrEmpty( id ))
            {
                //Debug.Log( "UnregisterQuestListContainer" );
                QuestUtility.UnregisterQuestListContainer( this );
            }
        }

        #endregion

    }

}
