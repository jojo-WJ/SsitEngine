using System;
using System.Collections.Generic;
using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// 任务节点是任务中的任务或阶段
    /// </summary>
    [Serializable]
    public class QuestNode
    {
        #region Serialized Fields

        [SerializeField] private string m_id;

        [Tooltip("节点名称")] [SerializeField] private string m_internalName;

        //[SerializeField]
        //private string m_description;

        [Tooltip("节点类型，根据类型执行相应行为")] [SerializeField]
        private QuestNodeType m_nodeType;

        [Tooltip("可选执行项")] [SerializeField] private bool m_isOptional;

        [Tooltip("节点状态")] [SerializeField] private QuestNodeState m_state = QuestNodeState.Inactive;

        [Tooltip("任务的会话对象默认时发任务的人")] [SerializeField]
        private string m_speaker;

        [Tooltip("任务状态信息")] [SerializeField] private List<QuestStateInfo> m_stateInfoList = new List<QuestStateInfo>();

        [Tooltip("节点成功的需求条件")] [SerializeField]
        private QuestConditionSet m_conditionSet = new QuestConditionSet();

        [HideInInspector] [SerializeField]
        private List<int> m_childIndexList = new List<int>(); // Indices into Quest.nodes.

        #endregion

        #region Accessor Properties for Serialized Fields

        /// <summary>
        /// 任务节点Id
        /// </summary>
        public string Id
        {
            get => m_id;
            set => m_id = value;
        }

        /// <summary>
        /// 任务节点名称
        /// </summary>
        public string InternalName
        {
            get => m_internalName;
            set => m_internalName = value;
        }

        ///// <summary>
        ///// 任务节点描述
        ///// </summary>
        //public string Description
        //{
        //    get
        //    {
        //        return m_description;
        //    }

        //    set
        //    {
        //        m_description = value;
        //    }
        //}

        public QuestNodeType NodeType
        {
            get => m_nodeType;
            set => m_nodeType = value;
        }

        /// <summary>
        /// 是否是终止任务树的节点类型
        /// </summary>
        public bool IsEndNodeType => NodeType == QuestNodeType.Success || NodeType == QuestNodeType.Failure;

        /// <summary>
        /// 是否有连接其他节点.
        /// </summary>
        public bool IsConnectionNodeType => !IsEndNodeType;

        /// <summary>
        /// 此任务是可选的（比如一个任务隐藏的加分项）.
        /// </summary>
        public bool IsOptional
        {
            get => m_isOptional;
            set => m_isOptional = value;
        }

        /// <summary>
        /// 任务的会话对象默认时发任务的人
        /// </summary>
        public string Speaker
        {
            get => m_speaker;
            set => m_speaker = value;
        }

        /// <summary>
        /// 节点状态信息（以枚举长度作为列表）
        /// </summary>
        public List<QuestStateInfo> StateInfoList
        {
            get => m_stateInfoList;
            set => m_stateInfoList = value;
        }

        /// <summary>
        /// 节点条件
        /// </summary>
        public QuestConditionSet ConditionSet
        {
            get => m_conditionSet;
            set => m_conditionSet = value;
        }

        /// <summary>
        /// 任务节点的索引列表. Unity不能序列嵌套类型，比如QuestNode类中的QuestNode引用
        /// 在运行时利用索引来构造引用列表
        /// </summary>
        public List<int> ChildIndexList
        {
            get => m_childIndexList;
            set => m_childIndexList = value;
        }

        #endregion

        #region Runtime References

        [NonSerialized] private TagDictionary m_tagDictionary = new TagDictionary();

        [NonSerialized] private Quest m_quest;

        [NonSerialized] private List<QuestNode> m_childList;

        [NonSerialized] private List<QuestNode> m_parentList;

        [NonSerialized] private List<QuestNode> m_optionalParentList;

        [NonSerialized] private List<QuestNode> m_nonoptionalParentList;

        private bool m_isCheckingConditions;

        /// <summary>
        /// 此任务节点中定义的标记及其值的字典
        /// </summary>
        public TagDictionary TagDictionary
        {
            get => m_tagDictionary;
            set => m_tagDictionary = value;
        }

        /// <summary>
        /// 节点归属
        /// </summary>
        public Quest Quest
        {
            get => m_quest;
            set => m_quest = value;
        }

        /// <summary>
        /// 该节点链接的子节点列表
        /// </summary>
        public List<QuestNode> ChildList
        {
            get => m_childList;
            set => m_childList = value;
        }

        /// <summary>
        /// 该节点链接的父节点列表
        /// </summary>
        public List<QuestNode> ParentList
        {
            get => m_parentList;
            set => m_parentList = value;
        }

        /// <summary>
        /// 标记为可选的父级的子集.
        /// </summary>
        public List<QuestNode> OptionalParentList
        {
            get => m_optionalParentList;
            set => m_optionalParentList = value;
        }

        /// <summary>
        /// 未标记为可选的父级列表
        /// </summary>
        public List<QuestNode> NonoptionalParentList
        {
            get => m_nonoptionalParentList;
            set => m_nonoptionalParentList = value;
        }

        /// <summary>
        /// Invoked when the node changes state.
        /// </summary>
        public event QuestNodeParameterDelegate OnStateChanged = delegate { };

        #endregion

        #region Editor

        // Node sizes for editor:
        public const float DefaultNodeWidth = 120;
        public const float DefaultNodeHeight = 48;
        public const float ShortNodeHeight = 35;
        public const float DefaultStartNodeX = 200;
        public const float DefaultStartNodeY = 20;

        [HideInInspector] [SerializeField] private Rect m_canvasRect; // Position in editor canvas.

        /// <summary>
        /// Position of the quest node in the Quest Editor window.
        /// </summary>
        public Rect CanvasRect
        {
            get => m_canvasRect;
            set => m_canvasRect = value;
        }


        public string GetEditorName()
        {
            if (!string.IsNullOrEmpty(InternalName))
            {
                return InternalName;
            }
            if (!string.IsNullOrEmpty(Id))
            {
                return Id;
            }
            return "Node";
        }

        #endregion

        #region Initialization & DeConstructor

        public QuestNode()
        {
        }

        public QuestNode( string id, string internalName, QuestNodeType nodeType, bool isOptional = false )
        {
            m_id = id;
            m_internalName = internalName;
            m_nodeType = nodeType;
            m_isOptional = isOptional;
        }

        public void InitializeAsStartNode( string questID )
        {
            Id = questID + ".start";
            InternalName = "Start";
            NodeType = QuestNodeType.Start;
            m_state = QuestNodeState.Inactive;
            StateInfoList = new List<QuestStateInfo>();

            CanvasRect = new Rect(DefaultStartNodeX, DefaultStartNodeY, DefaultNodeWidth, DefaultNodeHeight);
        }

        public void CloneSubassetsInto( QuestNode copy )
        {
            // Assumes lists are identical except subassets haven't been copied.
            if (copy == null)
            {
                return;
            }
            ConditionSet.CloneSubassetsInto(copy.ConditionSet);
            QuestStateInfo.CloneSubassets(StateInfoList, copy.StateInfoList);
            TagDictionary.CopyInto(copy.TagDictionary);
        }

        public static void CloneSubassets( List<QuestNode> original, List<QuestNode> copy )
        {
            // Assumes lists are identical except subassets haven't been copied.
            if (original == null || copy == null || copy.Count != original.Count)
            {
                if (Debug.isDebugBuild)
                {
                    Debug.LogWarning(
                        "Quest Machine: QuestNode.CloneSubassets() failed because copy or original is invalid.");
                }
                return;
            }
            for (var i = 0; i < original.Count; i++)
            {
                if (original[i] != null)
                {
                    original[i].CloneSubassetsInto(copy[i]);
                }
            }
        }

        public void DestroySubassets()
        {
            if (ConditionSet != null)
            {
                ConditionSet.DestroySubassets();
            }
            QuestStateInfo.DestroyListSubassets(StateInfoList);
        }

        public static void DestroyListSubassets( List<QuestNode> nodes )
        {
            if (nodes == null)
            {
                return;
            }
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] != null)
                {
                    nodes[i].DestroySubassets();
                }
            }
        }

        public void InitializeRuntimeReferences( Quest quest )
        {
            Quest = quest;

            // Set references in condition set:
            if (ConditionSet != null)
            {
                ConditionSet.SetRuntimeReferences(quest, this);
            }

            // Build children list:
            if (quest.NodeList != null)
            {
                ChildList = new List<QuestNode>();
                for (var i = 0; i < ChildIndexList.Count; i++)
                {
                    var index = ChildIndexList[i];
                    if (0 <= index && index < quest.NodeList.Count)
                    {
                        ChildList.Add(quest.NodeList[index]);
                    }
                }
            }

            ParentList = new List<QuestNode>();
            OptionalParentList = new List<QuestNode>();
            NonoptionalParentList = new List<QuestNode>();
        }

        public void ConnectRuntimeNodeReferences()
        {
            if (ChildList == null)
            {
                return;
            }
            for (var i = 0; i < ChildList.Count; i++)
            {
                if (ChildList[i] != null)
                {
                    ChildList[i].SetParent(this);
                }
            }
        }

        private void SetParent( QuestNode parent )
        {
            if (parent == null)
            {
                return;
            }
            if (ParentList == null)
            {
                ParentList = new List<QuestNode>();
            }
            ParentList.Add(parent);
            if (parent.IsOptional)
            {
                if (OptionalParentList == null)
                {
                    OptionalParentList = new List<QuestNode>();
                }
                OptionalParentList.Add(parent);
            }
            else
            {
                if (NonoptionalParentList == null)
                {
                    NonoptionalParentList = new List<QuestNode>();
                }
                NonoptionalParentList.Add(parent);
            }
            parent.OnStateChanged -= OnParentOnStateChange;
            parent.OnStateChanged += OnParentOnStateChange;
        }

        public void SetRuntimeNodeReferences()
        {
            var stateCount = Enum.GetNames(typeof(QuestNodeState)).Length;
            for (var i = 0; i < stateCount; i++)
            {
                var stateInfo = QuestStateInfo.GetStateInfo(StateInfoList, (QuestNodeState) i);
                if (stateInfo != null)
                {
                    stateInfo.SetRuntimeReferences(Quest, this);
                }
            }
        }

        #endregion

        #region Quest Node State

        /// <summary>
        /// 当前节点的状态
        /// </summary>
        public QuestNodeState GetState()
        {
            return m_state;
        }

        /// <summary>
        /// 设置节点的状态
        /// </summary>
        /// <param name="newState">New state.</param>
        public void SetState( QuestNodeState newState, bool informListeners = true )
        {
            if (ConditionSet != null && ConditionSet.IsPassing)
            {
                return;
            }

            //if (Engine.Debug)
            //    SsitDebug.Debug("Quest Machine: " + ((Quest != null) ? Quest.GetEditorName() : "Quest") + "." + GetEditorName() + ".SetState(" + newState + ")", Quest);

            m_state = newState;

            SetConditionChecking(newState == QuestNodeState.Active);

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
                    if (stateInfo.actionList[i] == null)
                    {
                        continue;
                    }
                    stateInfo.actionList[i].Execute(this);
                }
            }

            // Notify that state changed:
            QuestUtility.SendNotification((ushort) EnQuestEvent.QuestStateChangedMessage, this, Quest.Id, Id, m_state);

            try
            {
                OnStateChanged(this);
            }
            catch (Exception e) // Don't let exceptions in user-added events break our code.
            {
                if (Debug.isDebugBuild)
                {
                    Debug.LogException(e);
                }
            }

            // Handle special node types:
            switch (m_state)
            {
                case QuestNodeState.Active:
                    if (NodeType != QuestNodeType.Condition)
                    {
                        // Automatically switch non-Condition nodes to True state:
                        SetState(QuestNodeState.True);
                    }
                    break;
                case QuestNodeState.True:
                    // If it's an endpoint, set the overall quest state:
                    switch (NodeType)
                    {
                        case QuestNodeType.Success:
                            if (Quest != null)
                            {
                                Quest.SetState(QuestState.Successful);
                            }
                            break;
                        case QuestNodeType.Failure:
                            if (Quest != null)
                            {
                                Quest.SetState(QuestState.Failed);
                            }
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// 在不执行任何状态更改处理的情况下设置内部状态值.
        /// </summary>
        public void SetStateRaw( QuestNodeState state )
        {
            m_state = state;
        }

        /// <summary>
        /// 返回与任务节点状态关联的状态信息
        /// </summary>
        public QuestStateInfo GetStateInfo( QuestNodeState state )
        {
            return StateInfoList != null ? QuestStateInfo.GetStateInfo(StateInfoList, state) : null;
        }

        /// <summary>
        /// 设置节点条件
        /// </summary>
        /// <param name="enable">Specifies whether to start (enable) or stop.</param>
        public void SetConditionChecking( bool enable )
        {
            if (!Application.isPlaying)
            {
                return;
            }
            if (enable && m_isCheckingConditions || !enable && !m_isCheckingConditions)
            {
                return;
            }
            if (!IsConnectionNodeType || ConditionSet == null)
            {
                return;
            }
            if (enable)
            {
                ConditionSet.StartChecking(OnConditionsTrue);
            }
            else
            {
                //ConditionSet.StopChecking();
                for (var i = 0; i < ParentList.Count; i++)
                {
                    var parentNode = ParentList[i];
                    for (var j = 0; j < parentNode.ChildList.Count; j++)
                    {
                        var parallelNode = parentNode.ChildList[j];
                        if (parallelNode.ConditionSet != null)
                        {
                            parallelNode.ConditionSet.StopChecking();
                        }
                    }
                }
            }
            m_isCheckingConditions = enable;
        }

        private void OnConditionsTrue()
        {
            SetState(QuestNodeState.True);
        }

        /// <summary>
        /// Invoked by parent when parent's state changes.
        /// </summary>
        /// <param name="parent">Parent node whose state changed.</param>
        private void OnParentOnStateChange( QuestNode parent )
        {
            if (parent != null && parent.GetState() == QuestNodeState.True)
            {
                SetState(QuestNodeState.Active);
            }
        }

        #endregion

        #region UI Content

        /// <summary>
        /// 检查特定类别是否有任何UI内容
        /// </summary>
        /// <param name="category">The content category (Dialogue, Journal, etc.).</param>
        /// <returns>True if GetContentList would return anything.</returns>
        public bool HasContent( QuestContentCategory category )
        {
            if (!IsContentValidForCurrentSpeaker(category))
            {
                return false;
            }
            var stateInfo = QuestStateInfo.GetStateInfo(StateInfoList, m_state);
            return stateInfo != null && stateInfo.GetContentList(category).Count > 0;
        }

        /// <summary>
        /// 查询设定文本
        /// </summary>
        /// <param name="category">The content category (Dialogue, Journal, etc.).</param>
        /// <returns>基于当前任务状态及其所有节点的UI内容项列表</returns>
        public List<QuestContent> GetContentList( QuestContentCategory category )
        {
            if (!IsContentValidForCurrentSpeaker(category))
            {
                return null;
            }
            var stateInfo = QuestStateInfo.GetStateInfo(StateInfoList, m_state);
            return stateInfo != null ? stateInfo.GetContentList(category) : null;
        }

        private bool IsContentValidForCurrentSpeaker( QuestContentCategory category )
        {
            // Non-dialogue content is always valid:
            if (category != QuestContentCategory.Dialogue)
            {
                return true;
            }
            if (Quest == null)
            {
                return true;
            }

            // Are quest's current speaker and this node's speaker both the quest giver?
            if (Quest.CurrentSpeaker == null)
            {
                return string.IsNullOrEmpty(Speaker) || string.Equals(Speaker, Quest.QuestGiverId);
            }

            // Otherwise is quest's current speaker same as this node's speaker?
            return string.Equals(Speaker, Quest.CurrentSpeaker.Id);
        }

        #endregion
    }
}