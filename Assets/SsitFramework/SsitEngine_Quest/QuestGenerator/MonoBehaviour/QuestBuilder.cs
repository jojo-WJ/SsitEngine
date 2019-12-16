using UnityEngine;
using System;
using System.Collections.Generic;

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// Utility class for procedurally building quests.
    /// </summary>
    public class QuestBuilder
    {

        public Quest quest { get; set; }

        #region Constructors

        /// <summary>
        /// Creates a new quest.
        /// </summary>
        /// <param name="name">Name to use for quest name, ID, and title.</param>
        public QuestBuilder( string name )
        {
            CreateQuest(  name , name, name );
        }

        /// <summary>
        /// Creates a new quest.
        /// </summary>
        /// <param name="name">Quest name.</param>
        /// <param name="id">Quest ID.</param>
        /// <param name="title">Quest title (visible in UIs).</param>
        public QuestBuilder( string name, string id, string title )
        {
            CreateQuest( name, id, title );
        }


        private void CreateQuest( string name, string id, string title )
        {
            quest = Quest.CreateInstance<Quest>();
            quest.Initialize();
            quest.IsAsset = false;
            quest.IsInstance = true;
            quest.name = name;
            quest.Id = id;
            quest.Title = title;
            GetStartNode().Id =  id + ".start" ;
        }


        public Quest ToQuest()
        {
            if (quest == null)
                return null;
            ValidateListSizes();
            quest.SetRuntimeReferences();
            return quest;
        }

        private void ValidateListSizes()
        {
            var numStates = Enum.GetNames( typeof( QuestState ) ).Length;
            QuestStateInfo.ValidateStateInfoListCount( quest.StateInfoList, numStates );
            for (int i = 0; i < numStates; i++)
            {
                QuestStateInfo.ValidateCategorizedContentListCount( quest.StateInfoList[i].categorizedContentList );
            }
        }

        #endregion

        #region Counters

        public QuestCounter AddCounter( string counterName, int initialValue, int minValue, int maxValue, bool randomizeInitialValue, QuestCounterUpdateMode updateMode )
        {
            if (quest.CounterList.Find( x => string.Equals( x.name, counterName ) ) != null)
            {
                if (Debug.isDebugBuild) Debug.LogWarning( "Quest Machine: Counter '" + counterName + "' already exists in QuestBuilder." );
                return null;
            }
            var counter = new QuestCounter( counterName, initialValue, minValue, maxValue, updateMode );
            quest.CounterList.Add( counter );
            return counter;
        }


        public void AddCounterMessageEvent( string counterName, string targetID, string message, string parameter,
            QuestCounterMessageEvent.Operation operation, int literalValue = 0 )
        {
            var counter = quest.GetCounter( counterName );
            if (counter == null)
            {
                if (Debug.isDebugBuild) Debug.LogWarning( "Quest Machine: Counter '" + counterName + "' isn't present in QuestBuilder." );
                return;
            }
            counter.updateMode = QuestCounterUpdateMode.Messages;
            counter.messageEventList.Add( new QuestCounterMessageEvent( targetID, message, parameter, operation, literalValue ) );
        }



        #endregion

        #region Rewards

        public void AssignRewards( RewardSystem[] rewardSystems, int points )
        {
            if (rewardSystems == null)
                return;
            var pointsRemaining = points;
            for (int i = 0; i < rewardSystems.Length; i++)
            {
                var rewardSystem = rewardSystems[i];
                if (rewardSystem == null)
                    continue;
                pointsRemaining = rewardSystem.DetermineReward( pointsRemaining, quest );
                if (pointsRemaining <= 0)
                    break;
            }
        }

        #endregion

        #region Offer Content

        public void AddOfferContents( params QuestContent[] contents )
        {
            AddContents( quest.OfferContentList, contents );
        }

        public void AddOfferUnmetContents( params QuestContent[] contents )
        {
            AddContents( quest.OfferConditionsUnmetContentList, contents );
        }

        #endregion

        #region Create Content 

        //[TODO] Add error reporting to all null checks in QuestBuilder.

        public void AddContents( QuestContentSet contentSet, params QuestContent[] contents )
        {
            if (contentSet == null) return;
            AddContents( contentSet.contentList, contents );
        }

        public void AddContents( List<QuestContent> contentList, params QuestContent[] contents )
        {
            if (contentList == null || contents == null) return;
            contentList.AddRange( contents );
        }

        public QuestContent CreateTitleContent()
        {
            var content = HeadingTextQuestContent.CreateInstance<HeadingTextQuestContent>();
            content.name = "title";
            content.useQuestTitle = true;
            content.headingLevel = 1;
            return content;
        }

        public QuestContent CreateHeadingContent( string text, int level )
        {
            var content = HeadingTextQuestContent.CreateInstance<HeadingTextQuestContent>();
            content.name = "heading";
            content.useQuestTitle = false;
            content.headingText = text;
            content.headingLevel = level;
            return content;
        }

        public QuestContent CreateBodyContent( string text )
        {
            var content = BodyTextQuestContent.CreateInstance<BodyTextQuestContent>();
            content.name = "text";
            content.BodyText = text.Replace( "{DOMAIN}", "Forest" ); //[TODO] Example. Replace text.
            return content;
        }


        //[TODO] Other content types in QuestBuilder.

        #endregion

        #region Nodes

        public QuestNode GetStartNode()
        {
            return quest.NodeList[0];
        }


        public QuestNode AddNode( QuestNode parent, string id, string internalName, QuestNodeType nodeType, bool isOptional = false )
        {

            if (parent == null)
            {
                if (Debug.isDebugBuild) Debug.LogWarning( "Quest Machine: QuestBuilder.AddNode must be provided a valid parent node." );
                return null;
            }
            if (parent.ChildIndexList == null) return null;
            parent.ChildIndexList.Add( quest.NodeList.Count );
            var node = new QuestNode( id, internalName, nodeType, isOptional );
            node.CanvasRect = new Rect( parent.CanvasRect.x, parent.CanvasRect.y + 20 + QuestNode.DefaultNodeHeight, QuestNode.DefaultNodeWidth, QuestNode.DefaultNodeHeight );
            quest.NodeList.Add( node );
            QuestStateInfo.ValidateStateInfoListCount( node.StateInfoList );
            QuestStateInfo.ValidateCategorizedContentListCount( node.StateInfoList[(int)QuestNodeState.Active].categorizedContentList );
            QuestStateInfo.ValidateCategorizedContentListCount( node.StateInfoList[(int)QuestNodeState.Inactive].categorizedContentList );
            QuestStateInfo.ValidateCategorizedContentListCount( node.StateInfoList[(int)QuestNodeState.True].categorizedContentList );
            return node;
        }

        public QuestNode AddSuccessNode( QuestNode parent )
        {
            return AddNode( parent, "success", "Success", QuestNodeType.Success );
        }

        public QuestNode AddFailureNode( QuestNode parent )
        {
            return AddNode( parent, "failure", "Failure", QuestNodeType.Failure );
        }

        public QuestNode AddPassthroughNode( QuestNode parent, string id, string internalName )
        {
            return AddNode( parent, id, internalName, QuestNodeType.Passthrough );
        }


        public QuestNode AddConditionNode( QuestNode parent, string id, string internalName, ConditionCountMode conditionCountMode = ConditionCountMode.All, bool isOptional = false )
        {
            var node = AddNode( parent, id, internalName, QuestNodeType.Condition, isOptional );
            if (node == null)
                return null;
            node.ConditionSet.ConditionCountMode = conditionCountMode;
            return node;
        }


        public QuestNode AddDiscussQuestNode( QuestNode parent, QuestMessageParticipant targetSpecifier, string targetID, bool isOptional = false )
        {
            var node = AddConditionNode( parent, "talkTo" + targetID, "Talk to " + targetID, ConditionCountMode.All, isOptional );
            AddMessageCondition( node, QuestMessageParticipant.Quester, String.Empty, targetSpecifier, targetID, EnQuestEvent.DiscussedQuestMessage.ToString(), quest.Id );
            return node;
        }

        #endregion

        #region Conditions

        public CounterQuestCondition AddCounterCondition( QuestNode node, string counterName, CounterValueConditionMode conditionMode, QuestNumber requiredValue )
        {
            var condition = CounterQuestCondition.CreateInstance<CounterQuestCondition>();
            condition.name = "counterCondition";
            condition.counterIndex = quest.GetCounterIndex( counterName );
            condition.counterValueMode = conditionMode;
            condition.requiredCounterValue = requiredValue;
            node.ConditionSet.ConditionList.Add( condition );
            return condition;
        }

        public TimerQuestConditionPro AddTimerCounterCondition( QuestNode node, string counterName, CounterValueConditionMode conditionMode, QuestNumber requiredValue )
        {
            var condition = TimerQuestConditionPro.CreateInstance<TimerQuestConditionPro>();
            condition.name = "timerCountCondition";
            condition.counterIndex = quest.GetCounterIndex( counterName );
            condition.requiredCounterValue = requiredValue;
            node.ConditionSet.ConditionList.Add( condition );
            return condition;
        }

        public CounterQuestCondition AddCounterCondition( QuestNode node, string counterName, CounterValueConditionMode conditionMode, int requiredValue )
        {
            return AddCounterCondition( node, counterName, conditionMode, new QuestNumber( requiredValue ) );
        }
        public TimerQuestConditionPro AddTimerCounterCondition( QuestNode node, string counterName, CounterValueConditionMode conditionMode, int requiredValue )
        {
            return AddTimerCounterCondition( node, counterName, conditionMode, new QuestNumber( requiredValue ) );
        }

        public MessageQuestCondition AddMessageCondition( QuestNode node,
            QuestMessageParticipant senderSpecifier, string senderID,
            QuestMessageParticipant targetSpecifier, string targetID,
            string message, string parameter, MessageValue value = null )
        {
            var condition = MessageQuestCondition.CreateInstance<MessageQuestCondition>();
            condition.name = "messageCondition";
            condition.senderSpecifier = senderSpecifier;
            condition.SenderId = senderID;
            condition.TargetSpecifier = targetSpecifier;
            condition.TargetId = targetID;
            condition.Message = message;
            condition.Parameter = parameter;
            condition.Value = (value != null) ? value : new MessageValue();
            node.ConditionSet.ConditionList.Add( condition );
            return condition;
        }
        public MessageQuestCondition AddTimerCondition( QuestNode node,QuestMessageParticipant senderSpecifier, string senderID,QuestMessageParticipant targetSpecifier, string targetID,
         string message, string parameter, MessageValue value = null )
        {
            var condition = MessageQuestCondition.CreateInstance<MessageQuestCondition>();
            condition.name = "messageCondition";
            condition.senderSpecifier = senderSpecifier;
            condition.SenderId = senderID;
            condition.TargetSpecifier = targetSpecifier;
            condition.TargetId = targetID;
            condition.Message = message;
            condition.Parameter = parameter;
            condition.Value = (value != null) ? value : new MessageValue();
            node.ConditionSet.ConditionList.Add( condition );
            return condition;
        }
        #endregion

        #region Actions

        /// <summary>
        /// 创建消息事件行为
        /// </summary>
        /// <param name="message"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public QuestAction CreateMessageAction( string message, string parameter )
        {
            var messageAction = MessageQuestAction.CreateInstance<MessageQuestAction>();
            messageAction.Message = message;
            messageAction.Parameter = parameter;
            messageAction.TargetSpecifier = QuestMessageParticipant.Quester;
            return messageAction;
        }


        public QuestAction CreateMessageAction( string text )
        {
            if (text.Contains( ":" )) // Parameter?
            {
                var colonPos = text.IndexOf( ':' );
                return CreateMessageAction( text.Substring( colonPos + 1 ), text.Substring( 0, colonPos ) );
            }
            else
            {
                return CreateMessageAction( text, string.Empty );
            }
        }

        public QuestAction CreateSetIndicatorAction( string questID, string entityID, QuestIndicatorState indicatorState )
        {
            var indicatorAction = SetIndicatorQuestAction.CreateInstance<SetIndicatorQuestAction>();
            indicatorAction.questID = questID;
            indicatorAction.entityID = entityID;
            indicatorAction.questIndicatorState = indicatorState;
            return indicatorAction;
        }
        //[TODO] Other action types in QuestBuilder.

        #endregion

    }

}
