using System.Collections.Generic;
using SsitEngine.DebugLog;
using SsitEngine.Unity;
using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// A GameObject that can offer quests. 
    /// </summary>
    public class QuestGiver : IdentifiableQuestListContainer
    {
        #region Serialized Fields

        [Tooltip("What to show in dialogue when quest giver only has completed quests.")] [SerializeField]
        private CompletedQuestDialogueMode m_completedQuestDialogueMode = CompletedQuestDialogueMode.SameAsGlobal;

        [Tooltip(
            "The quest dialogue UI to use when conversing with the player. If unassigned, uses the default dialogue UI.")]
        [SerializeField]
        private Object m_questDialogueUI;

        [Tooltip(
            "Frequency in seconds at which to check quest cooldowns in case a quest becomes offerable again and should update the indicator UI. Set to zero to bypass checking.")]
        [SerializeField]
        private float m_cooldownCheckFrequency;

        #endregion

        #region Property Accessors to Serialized Fields

        /// <summary>
        /// What to show in dialogue when quest givers only have completed quests.
        /// </summary>
        public CompletedQuestDialogueMode CompletedQuestDialogueMode
        {
            get
            {
                switch (m_completedQuestDialogueMode)
                {
                    case CompletedQuestDialogueMode.SameAsGlobal:
                        switch (QuestManager.Instance.CompletedQuestDialogueMode)
                        {
                            case CompletedQuestGlobalDialogueMode.ShowCompletedQuest:
                                return CompletedQuestDialogueMode.ShowCompletedQuest;
                            default:
                                return CompletedQuestDialogueMode.ShowNoQuests;
                        }
                    default:
                        return m_completedQuestDialogueMode;
                }
            }
            set => m_completedQuestDialogueMode = value;
        }


        /// <summary>
        /// Frequency in seconds at which to check quest cooldowns in case a quest becomes offerable again and should update the indicator UI.
        /// </summary>
        public float CooldownCheckFrequency
        {
            get => m_cooldownCheckFrequency;
            set
            {
                m_cooldownCheckFrequency = value;
                if (Application.isPlaying)
                {
                    RestartCooldownCheckInvokeRepeating();
                }
            }
        }

        public static string GetDisplayName( QuestGiver questGiver )
        {
            return questGiver != null ? questGiver.displayName : string.Empty;
        }

        #endregion

        #region Runtime Info

        // Runtime info:
        protected List<Quest> NonOfferableQuests { get; set; }
        protected List<Quest> OfferableQuests { get; set; }
        protected List<Quest> ActiveQuests { get; set; }

        protected List<Quest> CompletedQuests { get; set; }

        //protected QuestEntity QuestEntity { get; set; }
        protected GameObject Player { get; set; }
        protected QuestParticipantTextInfo PlayerTextInfo { get; set; }
        protected QuestListContainer PlayerQuestListContainer { get; set; }

        private QuestParticipantTextInfo m_myQuestGiverTextinfo;

        protected QuestParticipantTextInfo myQuestGiverTextInfo
        {
            get
            {
                m_myQuestGiverTextinfo = new QuestParticipantTextInfo(id, displayName, image);
                return m_myQuestGiverTextinfo;
            }
        }

        #endregion

        #region Initialization

        public override void Awake()
        {
            base.Awake();
            NonOfferableQuests = new List<Quest>();
            OfferableQuests = new List<Quest>();
            ActiveQuests = new List<Quest>();
            CompletedQuests = new List<Quest>();
            //QuestEntity = GetComponent<QuestEntity>();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            if (CooldownCheckFrequency > 0)
            {
                InvokeRepeating("UpdateCooldowns", CooldownCheckFrequency, CooldownCheckFrequency);
            }
        }

        public override void OnDisable()
        {
            CancelInvoke("UpdateCooldowns");
            base.OnDisable();
        }

        protected void RestartCooldownCheckInvokeRepeating()
        {
            CancelInvoke("UpdateCooldowns");
            if (CooldownCheckFrequency > 0)
            {
                InvokeRepeating("UpdateCooldowns", CooldownCheckFrequency, CooldownCheckFrequency);
            }
        }

        private void UpdateCooldowns()
        {
            for (var i = 0; i < questList.Count; i++)
            {
                var quest = questList[i];
                if (quest != null && quest.GetState() == QuestState.WaitingToStart)
                {
                    quest.UpdateCooldown();
                }
            }
        }

        public override void Start()
        {
            base.Start();
            BackfillInfoFromEntityType();
            DeleteUnavailableQuests();
            AssignGiverIDToQuests();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }


        /// <summary>
        /// Resets quest giver's quest list to its original list.
        /// </summary>
        public override void ResetToOriginalState()
        {
            base.ResetToOriginalState();
            BackfillInfoFromEntityType();
            DeleteUnavailableQuests();
            AssignGiverIDToQuests();
        }

        /// <summary>
        /// If UI info is unassigned, get it from the quest giver's QuestEntity if present.
        /// </summary>
        protected void BackfillInfoFromEntityType()
        {
            //if (QuestEntity == null) return;
            //if (string.IsNullOrEmpty(id))
            //{
            //    id = QuestEntity.Id;
            //}
            //if (string.IsNullOrEmpty(displayName))
            //{
            //    displayName = QuestEntity.DisplayName;
            //}
            //if (image == null)
            //{
            //    image = QuestEntity.Image;
            //}
        }

        /// <summary>
        /// Deletes a quest from this quest giver's list.
        /// </summary>
        /// <param name="questID"></param>
        public override void DeleteQuest( string questID )
        {
            var quest = FindQuest(questID);
            if (quest)
            {
                quest.ClearQuestIndicatorStates();
                base.DeleteQuest(questID);
            }
        }

        /// <summary>
        /// Deletes quests whose maxTimes have been reached.
        /// </summary>
        protected void DeleteUnavailableQuests()
        {
            if (questList == null || questList.Count == 0)
            {
                return;
            }
            for (var i = questList.Count - 1; i >= 0; i--)
            {
                var quest = questList[i];
                if (quest != null && quest.TimesAccepted >= quest.MaxTimes)
                {
                    DeleteQuest(quest);
                }
            }
        }

        protected void AssignGiverIDToQuests()
        {
            for (var i = 0; i < questList.Count; i++)
            {
                var quest = questList[i];
                if (quest == null)
                {
                    continue;
                }
                quest.AssignQuestGiver(myQuestGiverTextInfo);
            }
        }

        /// <summary>
        /// Adds a quest to this quest giver's list.
        /// </summary>
        /// <param name="quest"></param>
        public override Quest AddQuest( Quest quest )
        {
            if (quest == null)
            {
                return null;
            }
            var instance = base.AddQuest(quest);
            instance.AssignQuestGiver(myQuestGiverTextInfo);
            return instance;
        }

        #endregion

        #region Record Quests By State

        /// <summary>
        /// Records the current offerable and player-assigned quests in the runtime lists.
        /// </summary>
        protected virtual void RecordQuestsByState()
        {
            RecordRelevantPlayerQuests();
            RecordOfferableQuests();
        }

        /// <summary>
        /// Records quests in the player's QuestList that were given by this quest giver
        /// or active quests for which this quest giver has dialogue content.
        /// </summary>
        protected virtual void RecordRelevantPlayerQuests()
        {
            ActiveQuests.Clear();
            CompletedQuests.Clear();
            if (PlayerQuestListContainer == null || PlayerQuestListContainer.questList == null)
            {
                return;
            }
            for (var i = 0; i < PlayerQuestListContainer.questList.Count; i++)
            {
                var quest = PlayerQuestListContainer.questList[i];
                if (quest == null)
                {
                    continue;
                }
                var questState = quest.GetState();
                if (string.Equals(quest.QuestGiverId, id))
                {
                    switch (questState)
                    {
                        case QuestState.Active:
                            ActiveQuests.Add(quest);
                            break;
                        case QuestState.Successful:
                        case QuestState.Failed:
                            CompletedQuests.Add(quest);
                            break;
                    }
                }
                else if (questState == QuestState.Active && quest.Speakers.Contains(id))
                {
                    ActiveQuests.Add(quest);
                }
            }
        }

        /// <summary>
        /// Removes completed quests that have no dialogue to offer.
        /// </summary>
        protected virtual void RemoveCompletedQuestsWithNoDialogue()
        {
            if (CompletedQuests == null || CompletedQuests.Count == 0)
            {
                return;
            }
            var info = new QuestParticipantTextInfo(id, displayName, image);
            for (var i = CompletedQuests.Count - 1; i >= 0; i--)
            {
                if (!CompletedQuests[i].HasContent(QuestContentCategory.Dialogue, info))
                {
                    CompletedQuests.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Records which quests are offerable or not in the runtime lists.
        /// </summary>
        protected virtual void RecordOfferableQuests()
        {
            NonOfferableQuests.Clear();
            OfferableQuests.Clear();
            if (questList == null)
            {
                return;
            }
            if (PlayerQuestListContainer == null || PlayerQuestListContainer.questList == null)
            {
                return;
            }
            for (var i = 0; i < questList.Count; i++)
            {
                var quest = questList[i];
                if (quest == null || quest.GetState() != QuestState.WaitingToStart)
                {
                    continue;
                }

                // Check if the player is already doing a copy of this quest:
                var playerCopy = PlayerQuestListContainer.FindQuest(quest.Id);
                var isPlayerCopyActive = playerCopy != null && playerCopy.GetState() == QuestState.Active;

                // And check if the player is already doing a similar procedurally-generated quest by this giver:
                if (IsDoingSimilarGeneratedQuest(quest, PlayerQuestListContainer))
                {
                    isPlayerCopyActive = true;
                }

                quest.UpdateCooldown();
                if (quest.CanOffer && !isPlayerCopyActive &&
                    (playerCopy == null || playerCopy.TimesAccepted < quest.MaxTimes))
                {
                    OfferableQuests.Add(quest);
                }
                else if (playerCopy == null)
                {
                    NonOfferableQuests.Add(quest);
                }
            }
        }

        protected virtual bool IsDoingSimilarGeneratedQuest( Quest quest, QuestListContainer playerQuestListContainer )
        {
            if (quest == null || playerQuestListContainer == null || !quest.IsProcedurallyGenerated ||
                ActiveQuests == null)
            {
                return false;
            }
            // Check active quests given by this giver:
            for (var i = 0; i < ActiveQuests.Count; i++)
            {
                var activeQuest = ActiveQuests[i];
                if (activeQuest == null)
                {
                    continue;
                }
                if (activeQuest.TagDictionary.ContainsTag(QuestMachineTags.ACTION) &&
                    quest.TagDictionary.ContainsTag(QuestMachineTags.ACTION) &&
                    string.Equals(activeQuest.TagDictionary.dict[QuestMachineTags.ACTION],
                        quest.TagDictionary.dict[QuestMachineTags.ACTION]) &&
                    activeQuest.TagDictionary.ContainsTag(QuestMachineTags.TARGET) &&
                    quest.TagDictionary.ContainsTag(QuestMachineTags.TARGET) &&
                    string.Equals(activeQuest.TagDictionary.dict[QuestMachineTags.TARGET],
                        quest.TagDictionary.dict[QuestMachineTags.TARGET]))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Dialogue

        /// <summary>
        /// Starts dialogue with the player. The content of the dialogue will depend on the quest giver's
        /// offerable quests and the player's quests.
        /// </summary>
        /// <param name="player">Player conversing with this QuestGiver. If null, searches the scene for a GameObject tagged Player.</param>
        public virtual void StartDialogue( GameObject player )
        {
            if (player == null)
            {
                Debug.LogWarning("StartDialogue player is null");
            }
            //if (QuestDialogueUi == null || player == null) return;

            // Record quests related to this player and me:
            Player = player;
            PlayerTextInfo =
                new QuestParticipantTextInfo(QuestUtility.GetID(player), QuestUtility.GetDisplayName(player), null);
            PlayerQuestListContainer = player.GetComponent<QuestListContainer>();
            if (PlayerQuestListContainer == null && Debug.isDebugBuild)
            {
                Debug.LogWarning(
                    "Quest Machine: Can't start dialogue with " + name + ". Player doesn't have a Quest Journal.",
                    this);
                return;
            }
            RecordQuestsByState();

            // Start the most appropriate dialogue based on the recorded quests:
            if (Engine.Debug)
            {
                SsitDebug.Debug(
                    "Quest Machine: " + name + ".StartDialogue: #offerable=" + OfferableQuests.Count + " #active=" +
                    ActiveQuests.Count + " #completed=" + CompletedQuests.Count, this);
            }
            QuestUtility.SendNotification((ushort) EnQuestEvent.GreetMessage, player, this, id);
            //if (ActiveQuests.Count + OfferableQuests.Count >= 2)
            //{
            //    QuestDialogueUi.ShowQuestList(myQuestGiverTextInfo, ActiveQuestsUiContents.contentList, ActiveQuests, OfferableQuestsUiContents.contentList, OfferableQuests, OnSelectQuest);
            //}
            //else if (ActiveQuests.Count == 1)
            //{
            //    ShowActiveQuest(ActiveQuests[0]);
            //}
            //else if (OfferableQuests.Count == 1)
            //{
            //    ShowOfferQuest(OfferableQuests[0]);
            //}
            //else if (NonOfferableQuests.Count >= 1)
            //{
            //    QuestDialogueUi.ShowOfferConditionsUnmet(myQuestGiverTextInfo, NoQuestsUiContents.contentList, NonOfferableQuests);
            //}
            //else
            //{
            //    RemoveCompletedQuestsWithNoDialogue();
            //    if (CompletedQuests.Count > 0 && CompletedQuestDialogueMode == CompletedQuestDialogueMode.ShowCompletedQuest)
            //    {

            //        QuestDialogueUi.ShowCompletedQuest(myQuestGiverTextInfo, CompletedQuests);
            //    }
            //    else
            //    {
            //        QuestDialogueUi.ShowContents(myQuestGiverTextInfo, NoQuestsUiContents.contentList);
            //    }
            //}
            QuestUtility.SendNotification((ushort) EnQuestEvent.GreetedMessage, player, this, id);
        }

        /// <summary>
        /// Stops dialogue with the player.
        /// </summary>
        public virtual void StopDialogue()
        {
            //todo：隐藏对话弹窗
        }

        protected virtual void ShowOfferQuest( Quest quest )
        {
            //if (QuestDialogueUi == null)
            //{
            //    if (Debug.isDebugBuild)
            //        Debug.LogWarning("Quest Machine: There is no Quest Dialogue UI.", this);
            //}
            //else if (quest == null)
            //{
            //    if (Debug.isDebugBuild)
            //        Debug.LogWarning("Quest Machine: The quest passed to ShowOfferQuest() is null.", this);
            //}
            //else if (PlayerQuestListContainer == null)
            //{
            //    if (Debug.isDebugBuild) Debug.LogWarning("Quest Machine: There is no Player Quest List Container. Can't offer quest '" + quest.Title + "'.", this);
            //}
            //else
            //{
            //    QuestMachineMessages.DiscussQuest(Player, this, id, quest.Id);
            //    PlayerQuestListContainer.DeleteQuest(quest.Id); // Clear any old instance of repeatable quests first.
            //    QuestDialogueUi.ShowOfferQuest(myQuestGiverTextInfo, quest, OnAcceptQuest, OnQuestBackButton);
            //    QuestMachineMessages.DiscussedQuest(Player, this, id, quest.Id);
            //}
        }

        protected virtual void ShowActiveQuest( Quest quest )
        {
            //QuestMachineMessages.DiscussQuest(Player, this, id, quest.Id);
            //QuestParameterDelegate backButtonDelegate = null;
            //if (ActiveQuests.Count + OfferableQuests.Count >= 2)
            //    backButtonDelegate = OnQuestBackButton;
            //QuestDialogueUi.ShowActiveQuest(myQuestGiverTextInfo, quest, OnContinueActiveQuest, backButtonDelegate);
            //QuestMachineMessages.DiscussedQuest(Player, this, id, quest.Id);
        }

        protected virtual void OnSelectQuest( Quest quest )
        {
            switch (quest.GetState())
            {
                case QuestState.WaitingToStart:
                    ShowOfferQuest(quest);
                    break;
                case QuestState.Active:
                    ShowActiveQuest(quest);
                    break;
            }
        }

        protected virtual void OnAcceptQuest( Quest quest )
        {
            GiveQuestToQuester(quest, PlayerTextInfo, PlayerQuestListContainer);
            RecordQuestsByState();
            //if (OfferableQuests.Count >= 1 && (ActiveQuests.Count + OfferableQuests.Count) >= 2)
            //{
            //    QuestDialogueUi.ShowQuestList(myQuestGiverTextInfo, ActiveQuestsUiContents.contentList, ActiveQuests, OfferableQuestsUiContents.contentList, OfferableQuests, OnSelectQuest);
            //}
            //else
            //{
            //    QuestDialogueUi.Hide();
            //}
        }

        protected virtual void OnQuestBackButton( Quest quest )
        {
            //if (ActiveQuests.Contains(quest) && quest.GetState() != QuestState.Active)
            //{
            //    ActiveQuests.Remove(quest);
            //}
            //if (ActiveQuests.Count + OfferableQuests.Count >= 2)
            //{
            //    QuestDialogueUi.ShowQuestList(myQuestGiverTextInfo, ActiveQuestsUiContents.contentList, ActiveQuests, OfferableQuestsUiContents.contentList, OfferableQuests, OnSelectQuest);
            //}
            //else
            //{
            //    QuestDialogueUi.Hide();
            //}
        }

        protected virtual void OnContinueActiveQuest( Quest quest )
        {
            //QuestDialogueUi.Hide();
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
        public virtual void GiveQuestToQuester( Quest quest, QuestParticipantTextInfo questerTextInfo,
            QuestListContainer questerQuestListContainer, QuestCompleteMode mode = QuestCompleteMode.SingleComplet )
        {
            if (quest == null)
            {
                Debug.LogWarning("Quest Machine: " + name + ".GiveQuestToQuester - quest is null.", this);
                return;
            }
            if (questerTextInfo == null)
            {
                Debug.LogWarning("Quest Machine: " + name + ".GiveQuestToQuester - questerTextInfo is null.", this);
                return;
            }
            if (questerQuestListContainer == null)
            {
                Debug.LogWarning("Quest Machine: " + name + ".GiveQuestToQuester - questerQuestListContainer is null.",
                    this);
                return;
            }

            // Make a copy of the quest for the quester:
            var questInstance = quest.Clone();

            // Update the version on this QuestGiver:
            quest.TimesAccepted++;
            if (quest.TimesAccepted >= quest.MaxTimes)
            {
                DeleteQuest(quest.Id);
            }
            else
            {
                quest.StartCooldown();
            }

            // Add the copy to the quester and activate it:
            questInstance.AssignQuestGiver(myQuestGiverTextInfo);
            questInstance.AssignQuester(questerTextInfo, mode);

            questInstance.TimesAccepted = 1;
            if (questerQuestListContainer.questList.Count > 0)
            {
                for (var i = questerQuestListContainer.questList.Count - 1; i >= 0; i--)
                {
                    var inJournal = questerQuestListContainer.questList[i];
                    if (inJournal == null)
                    {
                        continue;
                    }
                    if (string.Equals(inJournal.Id, quest.Id) && inJournal.GetState() != QuestState.Active)
                    {
                        questInstance.TimesAccepted++;
                        questerQuestListContainer.DeleteQuest(inJournal);
                    }
                }
            }
            questerQuestListContainer.deletedStaticQuests.Remove(questInstance.Id);
            questerQuestListContainer.AddQuest(questInstance);
            questInstance.SetState(QuestState.Active);
            QuestUtility.SendNotification((ushort) EnQuestEvent.RefreshIndicatorMessage, questInstance, null, null);
        }

        /// <summary>
        /// Adds an instance of a quest to a quester's list. If the quest's maxTimes are reached,
        /// deletes the quest from the giver. Otherwise starts cooldown timer until it can be
        /// given again.
        /// </summary>
        /// <param name="quest">Quest to give to quester.</param>
        /// <param name="questerTextInfo">Quester's text info.</param>
        /// <param name="questerQuestListContainer">Quester's quest list container.</param>
        public virtual void GiveQuestToQuester( Quest quest, QuestParticipantTextInfo questGiverTextInfo,
            QuestParticipantTextInfo questerTextInfo, QuestListContainer questerQuestListContainer,
            QuestCompleteMode mode = QuestCompleteMode.SingleComplet )
        {
            if (quest == null)
            {
                Debug.LogWarning("Quest Machine: " + name + ".GiveQuestToQuester - quest is null.", this);
                return;
            }
            if (questerTextInfo == null)
            {
                Debug.LogWarning("Quest Machine: " + name + ".GiveQuestToQuester - questerTextInfo is null.", this);
                return;
            }
            if (questerQuestListContainer == null)
            {
                Debug.LogWarning("Quest Machine: " + name + ".GiveQuestToQuester - questerQuestListContainer is null.",
                    this);
                return;
            }

            // Make a copy of the quest for the quester:
            var questInstance = quest.Clone();

            // Update the version on this QuestGiver:
            quest.TimesAccepted++;
            if (quest.TimesAccepted >= quest.MaxTimes)
            {
                DeleteQuest(quest.Id);
            }
            else
            {
                quest.StartCooldown();
            }

            // Add the copy to the quester and activate it:
            questInstance.AssignQuestGiver(questGiverTextInfo);
            questInstance.AssignQuester(questerTextInfo, mode);

            questInstance.TimesAccepted = 1;
            if (questerQuestListContainer.questList.Count > 0)
            {
                for (var i = questerQuestListContainer.questList.Count - 1; i >= 0; i--)
                {
                    var inJournal = questerQuestListContainer.questList[i];
                    if (inJournal == null)
                    {
                        continue;
                    }
                    if (string.Equals(inJournal.Id, quest.Id) && inJournal.GetState() != QuestState.Active)
                    {
                        questInstance.TimesAccepted++;
                        questerQuestListContainer.DeleteQuest(inJournal);
                    }
                }
            }
            questerQuestListContainer.deletedStaticQuests.Remove(questInstance.Id);
            questerQuestListContainer.AddQuest(questInstance);
            questInstance.SetState(QuestState.Active);
            QuestUtility.SendNotification((ushort) EnQuestEvent.RefreshIndicatorMessage, questInstance, null, null);
        }


        /// <summary>
        /// Adds an instance of a quest to a quester's list. If the quest's maxTimes are reached,
        /// deletes the quest from the giver. Otherwise starts cooldown timer until it can be
        /// given again.
        /// </summary>
        /// <param name="quest">Quest to give to quester.</param>
        /// <param name="questerQuestListContainer">Quester's quest list container.</param>
        public virtual void GiveQuestToQuester( Quest quest, QuestListContainer questerQuestListContainer )
        {
            if (quest == null)
            {
                return;
            }
            if (questerQuestListContainer == null)
            {
                Debug.LogWarning(
                    "Quest Machine: " + name + ".GiveQuestToQuester - quester (QuestListContainer) is null.", this);
                return;
            }
            //var questerTextInfo = new QuestParticipantTextInfo(QuestMachineMessages.GetID(questerQuestListContainer.gameObject), QuestMachineMessages.GetDisplayName(questerQuestListContainer.gameObject), null, null);
            var questerTextInfo = new QuestParticipantTextInfo(QuestUtility.GetID(questerQuestListContainer),
                QuestUtility.GetDisplayName(questerQuestListContainer), null);

            GiveQuestToQuester(quest, questerTextInfo, questerQuestListContainer);
        }

        public virtual void GiveQuestToQuester( Quest quest, QuestParticipantTextInfo questGiverTextInfo,
            QuestListContainer questerQuestListContainer )
        {
            if (quest == null)
            {
                return;
            }
            if (questerQuestListContainer == null)
            {
                Debug.LogWarning(
                    "Quest Machine: " + name + ".GiveQuestToQuester - quester (QuestListContainer) is null.", this);
                return;
            }
            //var questerTextInfo = new QuestParticipantTextInfo(QuestMachineMessages.GetID(questerQuestListContainer.gameObject), QuestMachineMessages.GetDisplayName(questerQuestListContainer.gameObject), null, null);
            var questerTextInfo = new QuestParticipantTextInfo(QuestUtility.GetID(questerQuestListContainer),
                QuestUtility.GetDisplayName(questerQuestListContainer), null);

            GiveQuestToQuester(quest, questGiverTextInfo, questerTextInfo, questerQuestListContainer);
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
            if (questerQuestListContainer == null || questList.Count == 0)
            {
                return;
            }
            for (var i = questList.Count - 1; i >= 0; i--)
            {
                var quest = questList[i];
                if (quest != null && !questerQuestListContainer.ContainsQuest(quest.Id))
                {
                    GiveQuestToQuester(questList[i], questerQuestListContainer);
                }
            }
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
            if (quester == null)
            {
                return;
            }
            GiveAllQuestsToQuester(quester.GetComponent<QuestListContainer>());
        }

        #endregion
    }
}