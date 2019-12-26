/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：任务管理器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/1 12:00:33                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using SsitEngine.Unity;
using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// 任务资产
    /// </summary>
    public class Quest : ScriptableObject
    {
        #region Editor

        public string GetEditorName()
        {
#if Localization_Text
            if (!StringField.IsNullOrEmpty( title )) return title.value;
            if (!StringField.IsNullOrEmpty( id )) return id.value;
#else
            if (!string.IsNullOrEmpty(Title))
            {
                return Title;
            }
            if (!string.IsNullOrEmpty(Id))
            {
                return Id;
            }
#endif
            return "Unnamed Quest";
        }

        #endregion

        #region Nodes

        /// <summary>
        /// Looks up a node by its ID.
        /// </summary>
        public QuestNode GetNode( string questNodeID )
        {
            if (string.IsNullOrEmpty(questNodeID) || NodeList == null)
            {
                return null;
            }
            return NodeList.Find(x => string.Equals(x.Id, questNodeID));
        }

        #endregion

        #region Serialized Fields

        [Tooltip("是否是资源实例体")] [SerializeField] private bool m_isInstance;

        [Tooltip("如果是资源实例，是否是创建实例的原始资产")] [SerializeField]
        private Quest m_originalAsset;

        [SerializeField] private int m_fileVersion;

        [Tooltip("任务的唯一标识符")] [SerializeField] private string m_id;

        [Tooltip("在ui中显示的标题")] [SerializeField]
        private string m_title;

        [Tooltip("可选组进行任务分类")] [SerializeField]
        private string m_group;

        [Tooltip("分配给此任务的可选标签，用于排序和筛选")] [SerializeField]
        private List<string> m_labels;


        [Tooltip("显示在用户界面任务图标")] [SerializeField]
        private Sprite m_icon;

        [Tooltip("提供此任务的任务给予者的ID。通常在发问者接受任务时在发问者的运行时实例上设置")] [SerializeField]
        private string m_questGiverId;

        [Tooltip("允许玩家打开和关闭追踪")] [SerializeField]
        private bool m_isTrackable;

        [Tooltip("在任务HUD中显示")] [SerializeField]
        private bool m_showInTrackHud;

        [Tooltip("是否允许玩家放弃任务")] [SerializeField]
        private bool m_isAbandonable;

        [Tooltip("任务完成模式")] [SerializeField] private QuestCompleteMode m_completeMode;

        [Tooltip("如果被放弃，是否保存在任务日志中.")] [SerializeField]
        private bool m_rememberIfAbandoned;

        [Tooltip("如果指定，则条件满足时自动启动任务")] [SerializeField]
        private QuestConditionSet m_autostartConditionSet;

        [Tooltip("在提供任务之前必须满足的条件")] [SerializeField]
        private QuestConditionSet m_offerConditionSet;

        [Tooltip("提供条件不满足时显示的对话")] [SerializeField]
        private List<QuestContent> m_offerConditionsUnmetContentList;

        [Tooltip("显示任务对话的内容")] [SerializeField]
        private List<QuestContent> m_offerContentList;

        [Tooltip("此任务可接受的最大次数")] [SerializeField]
        private int m_maxTimes;

        [Tooltip("任务被接受的次数")] [SerializeField] private int m_timesAccepted;

        [Tooltip("任务冷却时间")] [SerializeField] private float m_cooldownSeconds;

        [Tooltip("任务冷却剩余时间")] [SerializeField] private float m_cooldownSecondsRemaining;

        [Tooltip("任务的当前状态")] [SerializeField] private QuestState m_state;

        [Tooltip("状态信息，根据任务状态枚举的int值索引")] [SerializeField]
        private List<QuestStateInfo> m_stateInfoList;

        [Tooltip("任务计数器")] [SerializeField] private List<QuestCounter> m_counterList;

        [Tooltip("任务节点（步骤）")] [SerializeField] private List<QuestNode> m_nodeList;

        [HideInInspector] [SerializeField] private string m_goalEntityTypeName; //目标实体类型名称

        #endregion

        #region Property Accessors to Serialized Fields

        /// <summary>
        /// Quest is a runtime instance, not an asset file.
        /// </summary>
        public bool IsInstance
        {
            get => m_isInstance;
            set => m_isInstance = value;
        }

        /// <summary>
        /// Quest is an asset, not a runtime instance.
        /// </summary>
        public bool IsAsset
        {
            get => !IsInstance;
            set => IsInstance = !value;
        }

        /// <summary>
        /// If a runtime instance, this is the original asset from which the instance was created.
        /// </summary>
        public Quest OriginalAsset
        {
            get => IsInstance ? m_originalAsset : this;
            set => m_originalAsset = value;
        }

        /// <summary>
        /// Quest was procedurally generated.
        /// </summary>
        public bool IsProcedurallyGenerated => IsInstance && OriginalAsset == null;

        public int FileVersion
        {
            get => m_fileVersion;
            set => m_fileVersion = value;
        }

        /// <summary>
        /// Unique identifier for this quest.
        /// </summary>
        public string Id
        {
            get => m_id;
            set => m_id = value;
        }

        /// <summary>
        /// Title shown in UIs.
        /// </summary>
        public string Title
        {
            get => m_title;
            set => m_title = value;
        }

        /// <summary>
        /// Optional quest icon shown in UIs.
        /// </summary>
        public Sprite Icon
        {
            get => m_icon;
            set => m_icon = value;
        }

        /// <summary>
        /// Optional group under which to categorize this quest.
        /// </summary>
        public string Group
        {
            get => m_group;
            set => m_group = value;
        }

        public List<string> Labels
        {
            get => m_labels;
            set => m_labels = value;
        }

        public string QuestGiverId
        {
            get => m_questGiverId;
            set => m_questGiverId = value;
        }

        /// <summary>
        /// ID of the quester assigned to this quest. If this is an asset or it hasn't been
        /// accepted yet, questerID will be empty;
        /// </summary>
        public string QuesterId => TagDictionary.GetTagValue(QuestMachineTags.QUESTERID, string.Empty);

        /// <summary>
        /// Specifies whether the player is allowed to toggle tracking on and off.
        /// </summary>
        public bool IsTrackable
        {
            get => m_isTrackable;
            set => m_isTrackable = value;
        }

        /// <summary>
        /// Specifies whether to show in the quest tracking HUD.
        /// </summary>
        public bool ShowInTrackHud
        {
            get => m_showInTrackHud;
            set
            {
                m_showInTrackHud = value;
                QuestUtility.SendNotification((ushort) EnQuestEvent.QuestTrackToggleChangedMessage, this, null, Id,
                    value);
            }
        }

        /// <summary>
        /// Specifies whether the player is allowed to abandon the quest.
        /// </summary>
        public bool IsAbandonable
        {
            get => m_isAbandonable;
            set => m_isAbandonable = value;
        }

        /// <summary>
        /// Specifies whether to keep in quest journal if abandoned.
        /// </summary>
        public bool RememberIfAbandoned
        {
            get => m_rememberIfAbandoned;
            set => m_rememberIfAbandoned = value;
        }

        /// <summary>
        /// If specified, conditions that autostart the quest when true.
        /// </summary>
        public QuestConditionSet AutostartConditionSet
        {
            get => m_autostartConditionSet;
            set => m_autostartConditionSet = value;
        }

        /// <summary>
        /// If true, the quest has autostart conditions. The quest will start 
        /// automatically when the conditions are met.
        /// </summary>
        public bool HasAutostartConditions => QuestConditionSet.ConditionCount(AutostartConditionSet) > 0;

        /// <summary>
        /// Conditions that must be true before the quest can be offered.
        /// </summary>
        public QuestConditionSet OfferConditionSet
        {
            get => m_offerConditionSet;
            set => m_offerConditionSet = value;
        }

        /// <summary>
        /// If true, the quest has offer conditions. The giver should not offer the 
        /// quest until the conditions are met.
        /// </summary>
        public bool HasOfferConditions => QuestConditionSet.ConditionCount(OfferConditionSet) > 0;

        /// <summary>
        /// If true, the giver can offer the quest.
        /// </summary>
        public bool CanOffer => (!HasOfferConditions || OfferConditionSet.AreConditionsMet) &&
                                TimesAccepted < MaxTimes && CooldownSecondsRemaining <= 0;

        /// <summary>
        /// Dialogue text to show when the offer conditions are unmet.
        /// </summary>
        public List<QuestContent> OfferConditionsUnmetContentList
        {
            get => m_offerConditionsUnmetContentList;
            set => m_offerConditionsUnmetContentList = value;
        }

        /// <summary>
        /// Dialogue text to show when offering the quest.
        /// </summary>
        public List<QuestContent> OfferContentList
        {
            get => m_offerContentList;
            set => m_offerContentList = value;
        }

        /// <summary>
        /// Max number of times this quest can be accepted.
        /// </summary>
        public int MaxTimes
        {
            get => m_maxTimes;
            set => m_maxTimes = value;
        }

        /// <summary>
        /// The number of times the quest has been accepted.
        /// </summary>
        public int TimesAccepted
        {
            get => m_timesAccepted;
            set => m_timesAccepted = value;
        }

        /// <summary>
        /// Minimum duration in seconds that must pass after quest acceptance to 
        /// offer it again.
        /// </summary>
        public float CooldownSeconds
        {
            get => m_cooldownSeconds;
            set => m_cooldownSeconds = value;
        }

        /// <summary>
        /// Seconds remaining until cooldown period is over.
        /// </summary>
        public float CooldownSecondsRemaining
        {
            get => m_cooldownSecondsRemaining;
            set => m_cooldownSecondsRemaining = value;
        }

        /// <summary>
        /// Info for each state, indexed by the int value of the QuestState enum.
        /// </summary>
        public List<QuestStateInfo> StateInfoList
        {
            get => m_stateInfoList;
            set => m_stateInfoList = value;
        }

        /// <summary>
        /// Counters defined for this quest.
        /// </summary>
        public List<QuestCounter> CounterList
        {
            get => m_counterList;
            set => m_counterList = value;
        }

        /// <summary>
        /// All nodes in this quest.
        /// </summary>
        public List<QuestNode> NodeList
        {
            get => m_nodeList;
            set => m_nodeList = value;
        }

        /// <summary>
        /// The quest's start node.
        /// </summary>
        public QuestNode StartNode => m_nodeList != null && m_nodeList.Count > 0 ? m_nodeList[0] : null;

        /// <summary>
        /// If this quest was procedurally generated, the goal EntityType's name.
        /// </summary>
        public string GoalEntityTypeName
        {
            get => m_goalEntityTypeName;
            set => m_goalEntityTypeName = value;
        }

        #endregion

        #region Runtime References

        [NonSerialized] private float m_timeCooldownLastChecked;

        //任务标签字典
        [NonSerialized] private TagDictionary m_tagDictionary = new TagDictionary();

        //任务指示器状态
        [NonSerialized]
        private Dictionary<string, QuestIndicatorState> m_questIndicatorStates =
            new Dictionary<string, QuestIndicatorState>();

        //任务播放文本
        [NonSerialized] private HashSet<string> m_speakers = new HashSet<string>();

        private QuestParticipantTextInfo m_currentSpeaker;


        /// <summary>
        /// 本任务中定义的标签及其值字典.
        /// </summary>
        public TagDictionary TagDictionary
        {
            get => m_tagDictionary;
            set => m_tagDictionary = value;
        }

        /// <summary>
        /// Current quest state indicator states by entity ID.
        /// </summary>
        public Dictionary<string, QuestIndicatorState> IndicatorStates
        {
            get => m_questIndicatorStates;
            set => m_questIndicatorStates = value;
        }

        /// <summary>
        /// 所有任务节点扬声器列表.
        /// </summary>
        public HashSet<string> Speakers
        {
            get => m_speakers;
            set => m_speakers = value;
        }

        /// <summary>
        /// The current speaker's info, if the speaker is different
        /// from the quest giver. If the quest giver is speaking, this
        /// property will be null.
        /// </summary>
        public QuestParticipantTextInfo CurrentSpeaker
        {
            get => m_currentSpeaker;
            private set => m_currentSpeaker = value;
        }

        public QuestCompleteMode CompleteMode
        {
            get => m_completeMode;

            set => m_completeMode = value;
        }

        /// <summary>
        /// 任务被提供时调用
        /// </summary>
        public event QuestParameterDelegate OnQuestOfferable = delegate { };

        /// <summary>
        /// 任务状态改变时调用
        /// </summary>
        public event QuestParameterDelegate OnQueststateChanged = delegate { };

        #endregion

        #region Initialization & Destruction

        /// <summary>
        /// Initializes a quest to empty starting values. Invoked when object is 
        /// created by ScriptableObjectUtility.CreateInstance.
        /// </summary>
        public void Initialize()
        {
            // (isInstance & originalAsset are not set here.)
            var instanceID = GetInstanceID();
            Id = "Quest" + instanceID;
            Title = "Quest " + instanceID;
            Icon = null;
            Group = string.Empty;
            Labels = new List<string>();
            QuestGiverId = string.Empty;
            ;
            IsTrackable = true;
            ShowInTrackHud = true;
            IsAbandonable = false;
            RememberIfAbandoned = false;
            AutostartConditionSet = new QuestConditionSet();
            OfferConditionSet = new QuestConditionSet();
            OfferConditionsUnmetContentList = new List<QuestContent>();
            OfferContentList = new List<QuestContent>();
            MaxTimes = 1;
            TimesAccepted = 0;
            CooldownSeconds = -1;
            CooldownSecondsRemaining = 0;
            m_state = QuestState.WaitingToStart;
            var numStates = Enum.GetNames(typeof(QuestState)).Length;
            StateInfoList = new List<QuestStateInfo>();
            for (var i = 0; i < numStates; i++)
            {
                StateInfoList.Add(new QuestStateInfo());
            }
            CounterList = new List<QuestCounter>();
            var startNode = new QuestNode();
            startNode.InitializeAsStartNode(Id);
            NodeList = new List<QuestNode>();
            NodeList.Add(startNode);
        }

        //private void OnEnable()
        //{
        //    SetRuntimeReferences();
        //}

        /// <summary>
        /// Returns a new instance of the quest, including new instances of all subassets
        /// such as QuestAction, QuestCondition, and QuestContent subassets.
        /// </summary>
        public Quest Clone()
        {
            var clone = Instantiate(this);
            SetRuntimeReferences(); // Fix original's references since Instantiate calls OnEnable > SetRuntimeReferences while clone's fields still point to original.
            clone.IsInstance = true;
            clone.OriginalAsset = OriginalAsset;
            clone.CompleteMode = CompleteMode;
            AutostartConditionSet.CloneSubassetsInto(clone.AutostartConditionSet);
            OfferConditionSet.CloneSubassetsInto(clone.OfferConditionSet);
            clone.OfferConditionsUnmetContentList = QuestSubasset.CloneList(OfferConditionsUnmetContentList);
            clone.OfferContentList = QuestSubasset.CloneList(OfferContentList);
            QuestStateInfo.CloneSubassets(StateInfoList, clone.StateInfoList);
            QuestNode.CloneSubassets(NodeList, clone.NodeList);
            TagDictionary.CopyInto(clone.TagDictionary);
            clone.SetRuntimeReferences();
            return clone;
        }

        private void OnDestroy()
        {
            if (IsInstance && Application.isPlaying)
            {
                QuestUtility.UnregisterQuestInstance(this);
                if (GetState() != QuestState.Disabled)
                {
                    SetState(QuestState.Disabled);
                }
                if (AutostartConditionSet != null)
                {
                    AutostartConditionSet.DestroySubassets();
                }
                if (OfferConditionSet != null)
                {
                    OfferConditionSet.DestroySubassets();
                }
                QuestSubasset.DestroyList(OfferConditionsUnmetContentList);
                QuestSubasset.DestroyList(OfferContentList);
                QuestStateInfo.DestroyListSubassets(StateInfoList);
                QuestNode.DestroyListSubassets(NodeList);
            }
        }

        public static void DestroyInstance( Quest quest )
        {
            if (quest != null && quest.IsInstance)
            {
                if (quest.GetState() != QuestState.Disabled)
                {
                    quest.SetState(QuestState.Disabled);
                }
                Destroy(quest);
            }
        }


        /// <summary>
        /// Sets sub-objects' runtime references to this quest.
        /// </summary>
        public void SetRuntimeReferences()
        {
            // Set references in start info:
            if (Application.isPlaying)
            {
                m_timeCooldownLastChecked = GameTime.Time;
            }
            if (AutostartConditionSet != null)
            {
                AutostartConditionSet.SetRuntimeReferences(this, null);
            }
            if (OfferConditionSet != null)
            {
                OfferConditionSet.SetRuntimeReferences(this, null);
            }
            QuestContent.SetRuntimeReferences(OfferConditionsUnmetContentList, this, null);
            QuestContent.SetRuntimeReferences(OfferContentList, this, null);

            // Set references in counters:
            if (CounterList != null)
            {
                for (var i = 0; i < CounterList.Count; i++)
                {
                    CounterList[i].SetRuntimeReferences(this);
                }
            }

            // Set references in state info:
            if (StateInfoList != null)
            {
                for (var i = 0; i < StateInfoList.Count; i++)
                {
                    var stateInfo = QuestStateInfo.GetStateInfo(StateInfoList, (QuestState) i);
                    if (stateInfo != null)
                    {
                        stateInfo.SetRuntimeReferences(this, null);
                    }
                }
            }

            // Set references in nodes:
            if (NodeList != null)
            {
                for (var i = 0; i < NodeList.Count; i++)
                {
                    if (NodeList[i] != null)
                    {
                        NodeList[i].InitializeRuntimeReferences(this);
                    }
                }
                for (var i = 0; i < NodeList.Count; i++)
                {
                    if (NodeList[i] != null)
                    {
                        NodeList[i].ConnectRuntimeNodeReferences();
                    }
                }
                for (var i = 0; i < NodeList.Count; i++)
                {
                    if (NodeList[i] != null)
                    {
                        NodeList[i].SetRuntimeNodeReferences();
                    }
                }
            }

            // Record list of any nodes' speakers who aren't the quest giver:
            RecordSpeakersUsedInQuestAndAnyNodes();

            // Add tags to dictionary:
            // Debug.Log( "AddTagsToDictionary" + (TagDictionary == null) + "Title" + (m_title == null) + "Group" + (Group == null) );
            QuestMachineTags.AddTagsToDictionary(TagDictionary, Title);
            QuestMachineTags.AddTagsToDictionary(TagDictionary, Group);
            if (!string.IsNullOrEmpty(QuestGiverId))
            {
                TagDictionary.SetTag(QuestMachineTags.QUESTGIVERID, QuestGiverId);
            }
        }

        /// <summary>
        /// 用任务中使用的交互对象填充演交互对象列表
        /// </summary>
        private void RecordSpeakersUsedInQuestAndAnyNodes()
        {
            if (Speakers == null)
            {
                Speakers = new HashSet<string>();
            }
            Speakers.Clear();
            if (!string.IsNullOrEmpty(QuestGiverId))
            {
                Speakers.Add(QuestGiverId);
            }
            if (NodeList == null)
            {
                return;
            }
            for (var i = 0; i < NodeList.Count; i++)
            {
                if (NodeList[i] == null)
                {
                    continue;
                }
                var speaker = NodeList[i].Speaker;
                if (string.IsNullOrEmpty(speaker) || Speakers.Contains(speaker))
                {
                    continue;
                }
                Speakers.Add(speaker);
            }
        }

        /// <summary>
        /// 设定任务发起者
        /// </summary>
        /// <param name="questGiverTextInfo">Identifying information about the quest giver.</param>
        public void AssignQuestGiver( QuestParticipantTextInfo questGiverTextInfo )
        {
            if (questGiverTextInfo == null)
            {
                return;
            }
            QuestGiverId = questGiverTextInfo.Id;
            if (!string.IsNullOrEmpty(questGiverTextInfo.Id))
            {
                Speakers.Add(questGiverTextInfo.Id);
            }
            QuestMachineTags.AddTagValuesToDictionary(TagDictionary);
            TagDictionary.SetTag(QuestMachineTags.QUESTGIVERID, questGiverTextInfo.Id);
            TagDictionary.SetTag(QuestMachineTags.QUESTGIVER, questGiverTextInfo.DisplayName);
        }

        /// <summary>
        /// 设定任务接受者
        /// </summary>
        /// <param name="questerTextInfo">Idenntifying information about the quester.</param>
        public void AssignQuester( QuestParticipantTextInfo questerTextInfo )
        {
            if (questerTextInfo == null || string.IsNullOrEmpty(questerTextInfo.Id))
            {
                return;
            }
            TagDictionary.SetTag(QuestMachineTags.QUESTERID, questerTextInfo.Id);
            TagDictionary.SetTag(QuestMachineTags.QUESTER, questerTextInfo.DisplayName);
        }

        public void AssignQuester( QuestParticipantTextInfo questerTextInfo,
            QuestCompleteMode completeMode = QuestCompleteMode.SingleComplet )
        {
            if (questerTextInfo == null || string.IsNullOrEmpty(questerTextInfo.Id))
            {
                return;
            }
            TagDictionary.SetTag(QuestMachineTags.QUESTERID, questerTextInfo.Id);
            TagDictionary.SetTag(QuestMachineTags.QUESTER, questerTextInfo.DisplayName);
            m_completeMode = completeMode;
        }

        #endregion

        #region Startup

        /// <summary>
        /// Invoke to tell the quest to perform its runtime startup actions.
        /// </summary>
        public void RuntimeStartup()
        {
            if (Application.isPlaying)
            {
                SetState(m_state, !QuestUtility.IsLoadingGame);
            }
        }

        /// <summary>
        /// Begins checking autostart and offer conditions.
        /// </summary>
        public void SetStartChecking( bool enable )
        {
            if (!Application.isPlaying)
            {
                return;
            }
            if (enable)
            {
                SetRandomCounterValues();
                if (HasAutostartConditions)
                {
                    AutostartConditionSet.StartChecking(Autostart);
                }
                if (HasOfferConditions)
                {
                    OfferConditionSet.StartChecking(BecomeOfferable);
                }
                else
                {
                    BecomeOfferable();
                }
            }
            else
            {
                if (HasAutostartConditions)
                {
                    AutostartConditionSet.StopChecking();
                }
                if (HasOfferConditions)
                {
                    OfferConditionSet.StopChecking();
                }
            }
        }

        private void Autostart()
        {
            SetState(QuestState.Active);
        }

        public void BecomeOfferable()
        {
            try
            {
                OnQuestOfferable(this);
                SetQuestIndicatorState(QuestGiverId, QuestIndicatorState.Offer);
            }
            catch (Exception e) // Don't let exceptions in user-added events break our code.
            {
                if (Debug.isDebugBuild)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// Starts the cooldown period for this quest.
        /// </summary>
        public void StartCooldown()
        {
            if (CooldownSeconds <= 0)
            {
                return;
            }
            CooldownSecondsRemaining = CooldownSeconds;
            m_timeCooldownLastChecked = GameTime.Time;
        }

        /// <summary>
        /// Checks the current game time and updates the cooldown period.
        /// </summary>
        public void UpdateCooldown()
        {
            if (CooldownSecondsRemaining <= 0)
            {
                return;
            }
            var elapsed = GameTime.Time - m_timeCooldownLastChecked;
            m_timeCooldownLastChecked = GameTime.Time;
            CooldownSecondsRemaining = Mathf.Max(0, CooldownSecondsRemaining - elapsed);
            if (CooldownSecondsRemaining <= 0)
            {
                BecomeOfferable();
            }
        }

        #endregion

        #region Quest State

        /// <summary>
        /// Gets the quest state.
        /// </summary>
        /// <returns>The current quest state. Each quest node also has its own state.</returns>
        public QuestState GetState()
        {
            return m_state;
        }

        /// <summary>
        /// Sets the quest state. This may also affect the states of the quest's nodes.
        /// </summary>
        /// <param name="newState">The new quest state.</param>
        public void SetState( QuestState newState, bool informListeners = true )
        {
            //if (Engine.Debug)
            //    SsitDebug.Debug("Quest Machine: " + GetEditorName() + ".SetState(" + newState + ", informListeners=" + informListeners + ")", this);
            m_state = newState;

            SetStartChecking(newState == QuestState.WaitingToStart);
            SetCounterListeners(newState == QuestState.Active ||
                                newState == QuestState.WaitingToStart &&
                                (HasAutostartConditions || HasOfferConditions));
            if (newState != QuestState.Active)
            {
                StopNodeListeners();
            }

            if (!informListeners)
            {
                return;
            }

            // Execute state actions:
            var stateInfo = GetStateInfo(m_state);
            if (stateInfo != null && stateInfo.actionList != null)
            {
                for (var i = 0; i < stateInfo.actionList.Count; i++)
                {
                    if (stateInfo.actionList[i] != null)
                    {
                        stateInfo.actionList[i].Execute(null);
                    }
                }
            }

            // Notify that state changed:
            QuestUtility.SendNotification((ushort) EnQuestEvent.QuestStateChangedMessage, this, Id, null, m_state);

            try
            {
                OnQueststateChanged(this);
            }
            catch (Exception e) // Don't let exceptions in user-added events break our code.
            {
                if (Debug.isDebugBuild)
                {
                    Debug.LogException(e);
                }
            }

            // If going active, activate the start node:
            if (m_state == QuestState.Active && StartNode != null)
            {
                StartNode.SetState(QuestNodeState.Active);
            }

            // If inactive, clear the indicators:
            if (m_state != QuestState.Active)
            {
                ClearQuestIndicatorStates();
            }
        }

        /// <summary>
        /// 设置内部状态值，而不执行任何状态更改处理
        /// </summary>
        public void SetStateRaw( QuestState state )
        {
            m_state = state;
        }

        /// <summary>
        /// Returns the state info associated with a quest state.
        /// </summary>
        public QuestStateInfo GetStateInfo( QuestState state )
        {
            return StateInfoList != null ? QuestStateInfo.GetStateInfo(StateInfoList, state) : null;
        }

        private void StopNodeListeners()
        {
            if (NodeList == null)
            {
                return;
            }
            for (var i = 0; i < NodeList.Count; i++)
            {
                if (NodeList[i] != null)
                {
                    NodeList[i].SetConditionChecking(false);
                }
            }
        }

        #endregion

        #region Counters

        private void SetRandomCounterValues()
        {
            if (CounterList == null)
            {
                return;
            }
            for (var i = 0; i < CounterList.Count; i++)
            {
                var counter = CounterList[i];
                if (counter != null && counter.randomizeInitialValue)
                {
                    counter.InitializeToRandomValue();
                }
            }
        }

        private void SetCounterListeners( bool enable )
        {
            if (CounterList == null)
            {
                return;
            }
            for (var i = 0; i < CounterList.Count; i++)
            {
                if (CounterList[i] != null)
                {
                    CounterList[i].SetListeners(enable);
                }
            }
        }

        /// <summary>
        /// Gets a counter defined in this quest.
        /// </summary>
        /// <param name="index">The index of the counter defined in the quest.</param>
        /// <returns>The counter, or null if there is no counter with the specified name.</returns>
        public QuestCounter GetCounter( int index )
        {
            return CounterList != null && 0 <= index && index < CounterList.Count ? CounterList[index] : null;
        }

        /// <summary>
        /// Gets a counter defined in this quest.
        /// </summary>
        /// <param name="counterName">The name of the counter defined in the quest.</param>
        /// <returns>The counter, or null if there is no counter with the specified name.</returns>
        public QuestCounter GetCounter( string counterName )
        {
            if (CounterList == null)
            {
                return null;
            }
            for (var i = 0; i < CounterList.Count; i++)
            {
                var counter = CounterList[i];
                if (counter != null && string.Equals(counter.name, counterName))
                {
                    return counter;
                }
            }
            return null;
        }

        public int GetCounterIndex( string counterName )
        {
            if (CounterList == null)
            {
                return -1;
            }
            for (var i = 0; i < CounterList.Count; i++)
            {
                var counter = CounterList[i];
                if (counter != null && string.Equals(counter.name, counterName))
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion

        #region Quest Indicator States

        public void SetQuestIndicatorState( string entityID, QuestIndicatorState questIndicatorState )
        {
            if (string.IsNullOrEmpty(entityID))
            {
                return;
            }
            if (!IndicatorStates.ContainsKey(entityID))
            {
                IndicatorStates.Add(entityID, QuestIndicatorState.None);
            }
            IndicatorStates[entityID] = questIndicatorState;

            QuestUtility.SendNotification((ushort) EnQuestEvent.SetIndicatorStateMessage, this, entityID, Id,
                questIndicatorState);
        }

        public QuestIndicatorState GetQuestIndicatorState( string entityID )
        {
            return string.IsNullOrEmpty(entityID) || !IndicatorStates.ContainsKey(entityID)
                ? QuestIndicatorState.None
                : IndicatorStates[entityID];
        }

        public void ClearQuestIndicatorStates()
        {
            if (IndicatorStates == null)
            {
                return;
            }
            foreach (var kvp in IndicatorStates)
            {
                if (kvp.Value == QuestIndicatorState.None)
                {
                    continue;
                }
                QuestUtility.SendNotification((ushort) EnQuestEvent.SetIndicatorStateMessage, this, kvp.Key, Id,
                    QuestIndicatorState.None);
            }
            IndicatorStates.Clear();
            QuestUtility.SendNotification((ushort) EnQuestEvent.RefreshIndicatorMessage, QuestGiverId, null, null);
        }

        #endregion

        #region UI Content

        public bool IsSpeakerQuestGiver( QuestParticipantTextInfo speaker )
        {
            return speaker == null || string.Equals(speaker.Id, QuestGiverId);
        }

        /// <summary>
        /// Checks if there is any UI content for a specific category.
        /// </summary>
        /// <param name="category">The content category (Dialogue, Journal, etc.).</param>
        /// <param name="speaker">The speaker whose content to check, or blank for the quest giver.</param>
        /// <returns>True if GetContentList would return anything.</returns>
        public bool HasContent( QuestContentCategory category, QuestParticipantTextInfo speaker = null )
        {
            CurrentSpeaker = IsSpeakerQuestGiver(speaker) ? null : speaker;
            var stateInfo = GetStateInfo(GetState());
            if (stateInfo.HasContent(category))
            {
                return true;
            }
            if (NodeList == null)
            {
                return false;
            }
            for (var i = 0; i < NodeList.Count; i++)
            {
                var node = NodeList[i];
                if (node != null && node.HasContent(category))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the UI content for a specific category.
        /// </summary>
        /// <param name="category">The content category (Dialogue, Journal, etc.).</param>
        /// <param name="speaker">The speaker whose content to get, or blank for the quest giver.</param>
        /// <returns>A list of content items based on the current state of the quest and all of its nodes.</returns>
        public List<QuestContent> GetContentList( QuestContentCategory category,
            QuestParticipantTextInfo speaker = null )
        {
            var contentList = new List<QuestContent>();
            CurrentSpeaker = IsSpeakerQuestGiver(speaker) ? null : speaker;
            var stateInfo = GetStateInfo(GetState());
            if (stateInfo != null)
            {
                contentList.AddRange(stateInfo.GetContentList(category));
            }
            if (NodeList != null)
            {
                for (var i = 0; i < NodeList.Count; i++)
                {
                    var node = NodeList[i];
                    var nodeContentList = node != null ? node.GetContentList(category) : null;
                    if (nodeContentList != null)
                    {
                        contentList.AddRange(nodeContentList);
                    }
                }
            }
            return contentList;
        }

        #endregion
    }
}