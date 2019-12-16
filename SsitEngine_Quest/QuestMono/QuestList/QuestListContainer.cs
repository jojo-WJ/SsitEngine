using UnityEngine;
using System;
using System.Collections.Generic;
using SsitEngine.DebugLog;
using SsitEngine.Unity;

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// 维护对象上的任务列表
    /// </summary>
    public class QuestListContainer : MonoBehaviour
    {

        #region Serialized Fields

        [Tooltip( "Forward quest state events to listeners that have registered for events such as questBecameOfferable, questStateChanged, and questNodeStateChanged." )]
        [SerializeField]
        private bool m_forwardEventsToListeners = false;

        [Tooltip( "Include in saved game data." )]
        [SerializeField]
        private bool m_includeInSavedGameData = false;

        [Tooltip( "The current quest list. At runtime, these are runtime instances of quests." )]
        [SerializeField]
        private List<Quest> m_questList = new List<Quest>();

        [Tooltip( "IDs of static quests that have been deleted and shouldn't be instantiated." )]
        [SerializeField]
        private List<string> m_deletedStaticQuests = new List<string>();

        #endregion

        #region Property Accessors to Serialized Fields

        /// <summary>
        /// Forward quest state events to listeners that have registered to events such as
        /// questBecameOfferable, questStateChanged, and questNodeStateChanged.
        /// </summary>
        public bool forwardEventsToListeners
        {
            get { return m_forwardEventsToListeners; }
            set { m_forwardEventsToListeners = value; }
        }

        /// <summary>
        /// Include in saved game data, which is used for saved games and scene persistence.
        /// </summary>
        public bool includeInSavedGameData
        {
            get { return m_includeInSavedGameData; }
            set { m_includeInSavedGameData = value; }
        }

        /// <summary>
        /// Quest assets.
        /// </summary>
        public List<Quest> questList
        {
            get { return m_questList; }
            protected set { m_questList = value; }
        }

        /// <summary>
        /// IDs of static quests that have been deleted and shouldn't be instantiated.
        /// </summary>
        public List<string> deletedStaticQuests
        {
            get { return m_deletedStaticQuests; }
            protected set { m_deletedStaticQuests = value; }
        }

        #endregion

        #region Mono
        public virtual void Awake() { }

        public virtual void Start() { }

        public virtual void Reset() { }

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        public virtual void OnDestroy()
        {
            DestroyQuestInstances();
        }
        #endregion

        #region Runtime Properties

        /// <summary>
        /// The original design-time quest list.
        /// </summary>
        protected List<Quest> originalQuestList { get; set; }

        /// <summary>
        /// Raised when a quest is added to the list.
        /// </summary>
        public event QuestParameterDelegate questAdded = delegate { };

        /// <summary>
        /// Raised when a quest is removed from the list.
        /// </summary>
        public event QuestParameterDelegate questRemoved = delegate { };

        /// <summary>
        /// Raised when a quest in the list becomes offerable.
        /// </summary>
        public event QuestParameterDelegate questBecameOfferable = delegate { };

        /// <summary>
        /// Raised when the state of a quest in the list changes.
        /// </summary>
        public event QuestParameterDelegate questStateChanged = delegate { };

        /// <summary>
        /// Raised when the state of a quest node in a quest in the list changes.
        /// </summary>
        public event QuestNodeParameterDelegate questNodeStateChanged = delegate { };

        #endregion

        #region Initialization


        public void OnInit()
        {
            originalQuestList = questList;
            InstantiateQuestAssets();
        }


        /// <summary>
        /// Instantiates copies of quest assets into the runtime
        /// quest list and enables their autostart and offer condition checking.
        /// </summary>
        private void InstantiateQuestAssets()
        {
            questList = new List<Quest>();
            AddQuests( originalQuestList );
        }

        public void DestroyQuestInstances()
        {
            if (questList == null || questList.Count == 0) return;
            for (int i = questList.Count - 1; i >= 0; i--)
            {
                DeleteQuest( questList[i] );
            }
        }

        /// <summary>
        /// Resets the quest list container to its original list.
        /// </summary>
        public virtual void ResetToOriginalState()
        {
            DestroyQuestInstances();
            InstantiateQuestAssets();
        }

        #endregion

        #region Add/Remove Quests

        public virtual void AddQuests( List<Quest> listToAdd )
        {
            if (listToAdd == null) return;
            for (int i = 0; i < listToAdd.Count; i++)
            {
                AddQuest( listToAdd[i] );
            }
        }

        public virtual Quest AddQuest( Quest quest )
        {
            if (quest == null)
                return null;
            if (deletedStaticQuests.Contains( quest.Id ))
                return null;
            var instance = quest.IsAsset ? quest.Clone() : quest;
            if (instance == null)
                return null;
            questList.Add( instance );
            QuestUtility.RegisterQuestInstance( instance );
            RegisterForQuestEvents( instance );
            instance.RuntimeStartup();
            return instance;
        }

        public virtual Quest FindQuest( string questID )
        {
            if (string.IsNullOrEmpty( questID ))
                return null;
            for (int i = 0; i < questList.Count; i++)
            {
                var quest = questList[i];
                if (quest == null)
                    continue;
                if (string.Equals( questID, quest.Id ))
                    return quest;
            }
            return null;
        }


        public virtual bool ContainsQuest( string questID )
        {
            return FindQuest( questID ) != null;
        }


        public virtual void DeleteQuest( string questID )
        {
            DeleteQuest( FindQuest( questID ) );
        }

        public virtual void DeleteQuest( Quest quest )
        {
            if (quest == null)
                return;
            questList.Remove( quest );
            if (!quest.IsProcedurallyGenerated)
            {
                //var questID = StringField.GetStringValue(quest.id  );
                if (!deletedStaticQuests.Contains( quest.Id ))
                    deletedStaticQuests.Add( quest.Id );
            }
            UnregisterForQuestEvents( quest );
            QuestUtility.UnregisterQuestInstance( quest );
            Quest.DestroyInstance( quest );
        }

        public virtual void RegisterForQuestEvents( Quest quest )
        {
            if (quest == null || !forwardEventsToListeners)
                return;
            if (Engine.Debug)
            {
                SsitDebug.Debug( "绑定监听事件" );
            }
            questAdded( quest );
            quest.OnQuestOfferable += OnOnQuestBecameOfferable;
            quest.OnQueststateChanged += OnQuestOnQueststateChanged;
            for (int i = 0; i < quest.NodeList.Count; i++)
            {
                quest.NodeList[i].OnStateChanged += OnQuestNodeOnStateChanged;
            }
        }

        public virtual void UnregisterForQuestEvents( Quest quest )
        {
            if (quest == null || !forwardEventsToListeners) return;
            questRemoved( quest );
            quest.OnQuestOfferable -= OnOnQuestBecameOfferable;
            quest.OnQueststateChanged -= OnQuestOnQueststateChanged;
            for (int i = 0; i < quest.NodeList.Count; i++)
            {
                quest.NodeList[i].OnStateChanged -= OnQuestNodeOnStateChanged;
            }
        }

        public virtual void OnOnQuestBecameOfferable( Quest quest )
        {
            questBecameOfferable( quest );
        }

        public virtual void OnQuestOnQueststateChanged( Quest quest )
        {
            questStateChanged( quest );
        }

        public virtual void OnQuestNodeOnStateChanged( QuestNode questNode )
        {
            questNodeStateChanged( questNode );
        }

        #endregion

    }
}