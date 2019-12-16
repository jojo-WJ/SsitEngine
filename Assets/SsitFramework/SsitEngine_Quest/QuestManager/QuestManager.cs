using SsitEngine.PureMVC.Interfaces;
using SsitEngine.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace SsitEngine.QuestManager
{
    public delegate bool OnQuestComleteAttachConditionHandler( Quest quest, int count );

    /// <summary>
    /// Quest Manager by Mono warp.
    /// </summary>
    public class QuestManager : ManagerBase<QuestManager>
    {

        #region Serialized Fields

        [Tooltip("What to show in dialogue when quest givers only have completed quests.")]
        [SerializeField]
        private CompletedQuestGlobalDialogueMode m_completedQuestDialogueMode = CompletedQuestGlobalDialogueMode.ShowCompletedQuest;

        [Tooltip("When saving and loading quests to JSON, format the output for readability over minimum size.")]
        [SerializeField]
        private bool m_prettyPrintJson;

        [Tooltip("预案任务资源最大限制")]
        [SerializeField]
        protected int maxQuestsToGenerate = 100;
        #endregion

        #region NotSerialized Field

        const string sysQuestGiverId = "SYSQUESTGIVERID";
        const string sysQuestJournalId = "SYSQUESTJOURNALID";

        /// <summary>
        /// 系统任务发布者
        /// </summary>
        private QuestGiver m_sysGiver;
        /// <summary>
        /// 系统任务日志
        /// </summary>
        private QuestJournal m_sysQuestJournal;

        /// <summary>
        /// 任务构建辅助器
        /// </summary>
        private IQuestHelper m_questHelper;

        /// <summary>
        /// 外部条件判定附加回调
        /// </summary>
        private OnQuestComleteAttachConditionHandler OnQuestComleteAttachConditionCallBack = null;

        private Dictionary<string, Quest> m_questAssets = new Dictionary<string, Quest>(); //key=questID

        private Dictionary<string, List<Quest>> m_questInstances = new Dictionary<string, List<Quest>>(); // key=questID

        private Dictionary<string, IdentifiableQuestListContainer> m_questListContainers = new Dictionary<string, IdentifiableQuestListContainer>(); // key=ID.


        #endregion

        #region Property Accessors to Serialized Fields

        public QuestGiver SysGiver
        {
            get
            {
                return m_sysGiver;
            }

            set
            {
                m_sysGiver = value;
            }
        }

        public QuestJournal SysQuestJournal
        {
            get
            {
                return m_sysQuestJournal;
            }

            set
            {
                m_sysQuestJournal = value;
            }
        }

        public IQuestHelper QuestHelper
        {
            get { return m_questHelper; }
        }

        /// <summary>
        /// What to show in dialogue when quest givers only have completed quests.
        /// </summary>
        public CompletedQuestGlobalDialogueMode CompletedQuestDialogueMode
        {
            get { return m_completedQuestDialogueMode; }
            set { m_completedQuestDialogueMode = value; }
        }

        public Dictionary<string, Quest> QuestAssets
        {
            get { return m_questAssets; }
        }

        public Dictionary<string, List<Quest>> QuestInstances
        {
            get
            {
                return m_questInstances;
            }
        }

        public Dictionary<string, IdentifiableQuestListContainer> QuestListContainers
        {
            get { return m_questListContainers; }
        }

        #endregion

        #region Initialization

        public override void OnSingletonInit()
        {
            SetQuestSystem();
        }

        /// <summary>
        /// 设置系统发布者
        /// </summary>
        public void SetQuestSystem()
        {
            m_sysGiver = this.gameObject.AddComponent<QuestGiver>();
            //RoomProxy roomProxy = Facade.Instance.RetrieveProxy(RoomProxy.NAME) as RoomProxy;
            //if (roomProxy!= null)
            {
                m_sysGiver.id = sysQuestGiverId;
                m_sysGiver.displayName = "任务系统";
                m_sysGiver.OnEnable();
            }
            m_sysQuestJournal = this.gameObject.AddComponent<QuestJournal>();
            {
                m_sysQuestJournal.id = sysQuestJournalId;
                m_sysQuestJournal.displayName = "任务系统";
                m_sysQuestJournal.OnEnable();
            }
            m_sysQuestJournal.forwardEventsToListeners = true;

        }

        public void SetQuestBuildHelper( IQuestHelper questHelper )
        {
            this.m_questHelper = questHelper;
        }

        public void SetQuest( Quest quest )
        {
            if (m_sysGiver.questList.Count < maxQuestsToGenerate)
            {
                m_sysGiver.AddQuest(quest);
            }
            else
            {
                Quest.DestroyInstance(quest);
            }
        }

        public void SetQuests( List<Quest> quests )
        {
            m_sysGiver.AddQuests(quests);
        }

        #endregion

        #region QuestMoudle

        public override int Priority
        {
            get { return (int)EnModuleType.ENMODULECUSTOM5; }
        }

        public override void OnUpdate( float elapseSeconds )
        {

        }

        public override void Shutdown()
        {
            if (!isShutdown)
            {
                isShutdown = true;
                m_questListContainers.Clear();
                m_questListContainers = null;
                m_questAssets.Clear();
                m_questAssets = null;
                m_questInstances.Clear();
                m_questInstances = null;
                m_questHelper = null;
                Destroy(gameObject);
            }
        }

        #endregion

        #region Give Quest

        /// <summary>
        /// Adds an instance of a quest to a quester's list. If the quest's maxTimes are reached,
        /// deletes the quest from the giver. Otherwise starts cooldown timer until it can be
        /// given again.
        /// </summary>
        /// <param name="quest">Quest to give to quester.</param>
        /// <param name="questerTextInfo">Quester's text info.</param>
        /// <param name="questerQuestListContainer">Quester's quest list container.</param>
        public void GiveQuestToQuester( Quest quest, QuestParticipantTextInfo questerTextInfo, QuestListContainer questerQuestListContainer, QuestCompleteMode mode = QuestCompleteMode.SingleComplet )
        {
            m_sysGiver.GiveQuestToQuester(quest, questerTextInfo, questerQuestListContainer, mode);

        }

        public void GiveQuestToQuester( Quest quest, QuestParticipantTextInfo questGiverTextInfo, QuestParticipantTextInfo questerTextInfo, QuestListContainer questerQuestListContainer, QuestCompleteMode mode = QuestCompleteMode.SingleComplet )
        {
            m_sysGiver.GiveQuestToQuester(quest, questGiverTextInfo, questerTextInfo, questerQuestListContainer, mode);
        }

        /// <summary>
        /// Adds an instance of a quest to a quester's list. If the quest's maxTimes are reached,
        /// deletes the quest from the giver. Otherwise starts cooldown timer until it can be
        /// given again.
        /// </summary>
        /// <param name="quest">Quest to give to quester.</param>
        /// <param name="questerQuestListContainer">Quester's quest list container.</param>
        public virtual void GiveQuestToQuester( Quest quest, string questGiverId, string questerId, QuestCompleteMode mode = QuestCompleteMode.SingleComplet )
        {
            if (quest == null) return;
            if (m_sysQuestJournal == null)
            {
                Debug.LogWarning("Quest Machine: " + name + ".GiveQuestToQuester - quester (QuestListContainer) is null.", this);
                return;
            }
            var questGiverTextInfo = new QuestParticipantTextInfo(questGiverId, questGiverId, null);
            var questerTextInfo = new QuestParticipantTextInfo(questerId, questerId, null);
            GiveQuestToQuester(quest, questGiverTextInfo, questerTextInfo, m_sysQuestJournal, mode);
        }
        public virtual void GiveQuestToQuester( Quest quest, QuestListContainer questerQuestListContainer, QuestCompleteMode mode = QuestCompleteMode.SingleComplet )
        {
            if (quest == null) return;
            if (questerQuestListContainer == null)
            {
                Debug.LogWarning("Quest Machine: " + name + ".GiveQuestToQuester - quester (QuestListContainer) is null.", this);
                return;
            }
            var questerTextInfo = new QuestParticipantTextInfo(QuestUtility.GetID(questerQuestListContainer), QuestUtility.GetDisplayName(questerQuestListContainer), null);
            GiveQuestToQuester(quest, questerTextInfo, questerQuestListContainer, mode);
        }

        public virtual void GiveQuestToQuesterObj( Quest quest, QuestListContainer questerQuestListContainer )
        {
            if (quest == null) return;
            if (questerQuestListContainer == null)
            {
                Debug.LogWarning("Quest Machine: " + name + ".GiveQuestToQuester - quester (QuestListContainer) is null.", this);
                return;
            }
            var questerTextInfo = new QuestParticipantTextInfo(QuestUtility.GetID(questerQuestListContainer.gameObject), QuestUtility.GetDisplayName(questerQuestListContainer.gameObject), null);
            GiveQuestToQuester(quest, questerTextInfo, questerQuestListContainer);
        }

        /// <summary>
        /// Adds an instance of a quest to a quester's list. If the quest's maxTimes are reached,
        /// deletes the quest from the giver. Otherwise starts cooldown timer until it can be
        /// given again.
        /// </summary>
        /// <param name="quest">Quest to give to quester.</param>
        /// <param name="questerID">Quester's ID.</param>
        public virtual void GiveQuestToQuester( Quest quest, string questerID )
        {
            GiveQuestToQuester(quest, QuestUtility.GetQuestListContainer(questerID));
        }

        /// <summary>
        /// Gives all quests to a quester.
        /// </summary>
        /// <param name="questerQuestListContainer">Quester's QuestListContainer.</param>
        public virtual void GiveAllQuestsToQuester( QuestListContainer questerQuestListContainer )
        {
            m_sysGiver.GiveAllQuestsToQuester(questerQuestListContainer);
        }

        /// <summary>
        /// Gives all quests to a quester.
        /// </summary>
        /// <param name="questerID">ID of quester.</param>
        public virtual void GiveAllQuestsToQuester( string questerID )
        {
            GiveAllQuestsToQuester(QuestUtility.GetQuestListContainer(questerID));
        }

        /// <summary>
        /// Gives all quests to a quester.
        /// </summary>
        /// <param name="quester">Quester.</param>
        public virtual void GiveAllQuestsToQuester( GameObject quester )
        {
            if (quester == null) return;
            GiveAllQuestsToQuester(quester.GetComponent<QuestListContainer>());
        }

        #endregion

        #region Give Journal

        public void AbandonQuest( Quest quest )
        {
            m_sysQuestJournal.AbandonQuest(quest);
        }

        #endregion

        #region AttachHander

        public void SetAttachHandler( OnQuestComleteAttachConditionHandler handler )
        {
            OnQuestComleteAttachConditionCallBack = handler;
        }

        internal OnQuestComleteAttachConditionHandler GetAttachHandler()
        {
            return OnQuestComleteAttachConditionCallBack;
        }
        #endregion

        #region 消息处理

        /// <summary>
        /// 管理器激活
        /// </summary>
        void OnEnable()
        {
            m_msgList = new ushort[]
            {
                (ushort) EnQuestEvent.QuestStateChangedMessage,
                (ushort) EnQuestEvent.QuestCounterChangedMessage,
                (ushort) EnQuestEvent.SetIndicatorStateMessage,
                (ushort) EnQuestEvent.RefreshIndicatorMessage,
            };
            RegisterMsg(m_msgList);
        }

        /// <summary>
        /// 管理器禁用
        /// </summary>
        void OnDisable()
        {
            UnRegisterMsg(m_msgList);
        }

        public override void HandleNotification( INotification notification )
        {
            QuestMessageArgs messageArgs = notification.Body as QuestMessageArgs;
            if (messageArgs == null)
            {
                return;
            }
            EnQuestEvent msgId = (EnQuestEvent)messageArgs.msgId;

            switch (msgId)
            {
                case EnQuestEvent.QuestStateChangedMessage:
                case EnQuestEvent.QuestCounterChangedMessage:
                    m_sysQuestJournal.RepaintUIs();
                    break;
                case EnQuestEvent.SetIndicatorStateMessage:
                    m_questHelper.CreateQuestIndicator(messageArgs);
                    break;
                case EnQuestEvent.RefreshIndicatorMessage:
                    m_questHelper.RefreshQuestIndicator(messageArgs);

                    break;
            }
        }

        #endregion
    }
}
