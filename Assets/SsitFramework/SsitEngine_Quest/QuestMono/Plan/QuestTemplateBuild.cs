/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/11 15:07:48                     
*└──────────────────────────────────────────────────────────────┘
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SsitEngine.QuestManager
{
    public class QuestTemplateBuild
    {
        public virtual Quest ConvertTemplateToQuest( string id, string title, string group, string desc, QuestTemplate questTemplate, List<QuestContent> rewardsUiContents, List<RewardSystem> rewardSystems )
        {
            // Build id:
            //var questID = title + " " + System.Guid.NewGuid();

            // Start QuestBuilder:
            var questBuilder = new QuestBuilder(title, id, title);
            SetMainInfo(questBuilder, group);
            AddTagsToDictionary(questBuilder.quest.TagDictionary);

            // Offer:

            AddOfferText(questBuilder, desc);

            // Quest headings:
            AddQuestHeadings(questBuilder);

            // Successful text (shown in journal / when talking again about successful quest):
            // AddSuccessfulText( questBuilder, mainTargetEntity, mainTargetDescriptor, domainName, goal );

            // Add steps:
            var previousNode = AddSteps(questBuilder, String.Empty, questTemplate);

            // Add "return to giver" node:
            //if (requireReturnToComplete)
            //{
            //previousNode = AddReturnNode( questBuilder, previousNode, entity, mainTargetEntity, mainTargetDescriptor, domainName, goal );
            //}

            // Success node:
            questBuilder.AddSuccessNode(previousNode);

            AddRewards(questBuilder, questTemplate.GetAllScore(), rewardsUiContents, rewardSystems);

            return questBuilder.ToQuest();
        }

        private void SetMainInfo( QuestBuilder questBuilder, string group )
        {
            questBuilder.quest.IsTrackable = true;
            questBuilder.quest.ShowInTrackHud = true;
            //questBuilder.quest.Icon = null;
            questBuilder.quest.Group = group;
        }

        private void AddTagsToDictionary( TagDictionary tagDictionary, object goal = null )
        {
            //todo:设置任务目标类型
            //tagDictionary.dict.Add( QuestMachineTags.TARGET, goal.fact.entityType.displayName );
            //tagDictionary.dict.Add( QuestMachineTags.TARGETS, goal.fact.entityType.pluralDisplayName );
            //todo:设置计数模式时任务计数器的值
            //var completion = goal.action.completion;
            //if (completion.mode == ActionCompletion.Mode.Counter)
            //{
            //    tagDictionary.dict.Add( QuestMachineTags.COUNTERGOAL, goal.requiredCounterValue.ToString() );
            //}
        }

        private void AddOfferText( QuestBuilder questBuilder, string questDesc )
        {
            // Offer motive:
            questBuilder.AddOfferContents(questBuilder.CreateTitleContent(), questBuilder.CreateBodyContent(questDesc));
        }

        /// <summary>
        /// Adds quest heading text to the main quest's Dialogue, Journal, and HUD categories.
        /// </summary>
        /// <param name="questBuilder">QuestBuilder.</param>
        /// <param name="goal">Goal step, which may contain completion text.</param>
        protected virtual void AddQuestHeadings( QuestBuilder questBuilder )
        {
            //AddQuestHeading( questBuilder, QuestContentCategory.Dialogue, false );
            AddQuestHeading(questBuilder, QuestContentCategory.Journal, true);
            AddQuestHeading(questBuilder, QuestContentCategory.HUD, false);
        }

        /// <summary>
        /// Adds quest heading text to a specific UI category's active state (and possibly 
        /// also its successful state).
        /// </summary>
        protected virtual void AddQuestHeading( QuestBuilder questBuilder, QuestContentCategory category, bool addToSuccessfulList )
        {
            // Add to Active state and, if not HUD, to Successful state.
            questBuilder.AddContents(questBuilder.quest.StateInfoList[(int)QuestState.Active].categorizedContentList[(int)category], questBuilder.CreateTitleContent());
            if (addToSuccessfulList && category != QuestContentCategory.HUD)
            {
                questBuilder.AddContents(questBuilder.quest.StateInfoList[(int)QuestState.Successful].categorizedContentList[(int)category], questBuilder.CreateTitleContent());
            }
        }

        private void AddSuccessfulText( QuestBuilder questBuilder, object mainTargetEntity, object mainTargetDescriptor, object domainName, object goal )
        {

        }

        #region 节点构造
        /// <summary>
        /// Adds the plan's steps.
        /// </summary>
        /// <param name="questBuilder">QuestBuilder.</param>
        /// <param name="domainName">Main target's domain.</param>
        /// <param name="goal">Goal step.</param>
        /// <param name="plan">List of steps that end with goal step.</param>
        /// <returns>The last node added.</returns>
        protected virtual QuestNode AddSteps( QuestBuilder questBuilder, string domainName, QuestTemplate plan )
        {
            var previousNode = questBuilder.GetStartNode();
            //var counterNames = new HashSet<string>();

            int nodeCount = 0;

            for (int i = 0; i < plan.Steps.Count; i++)
            {
                var step = plan.Steps[i];


                // Create next condition node:
                var targetEntity = step.SuccesAction.DisplayName;
                var targetDescriptor = step.SuccesAction.Description;
                nodeCount++;
                var id = nodeCount.ToString();
                var internalName = step.SuccesAction.Description + " " + targetDescriptor;
                var conditionNode = questBuilder.AddConditionNode(previousNode, id, internalName, ConditionCountMode.All);

                // Variables for node text tag replacement:
                var counterName = string.Empty;
                int requiredCounterValue = 0;

                CreatSuccessStepNode(conditionNode, questBuilder, nodeCount, step.SuccesAction, ref targetEntity, ref targetDescriptor,
                                    ref domainName, ref counterName, ref requiredCounterValue);
                // Create failtrue condition node
                if (step.FailureAction != null && step.FailureAction.Completion != null && step.FailureAction.Completion.Count > 0)
                {
                    targetEntity = step.FailureAction.DisplayName;
                    targetDescriptor = step.FailureAction.Description;
                    nodeCount++;
                    id = nodeCount.ToString();
                    internalName = step.FailureAction.Description + " " + targetDescriptor;
                    var failtureNode = questBuilder.AddConditionNode(previousNode, id, internalName,
                        ConditionCountMode.Any);
                    CreatFailtureStepNode(failtureNode, questBuilder, nodeCount, step.FailureAction, ref targetEntity, ref targetDescriptor,
                                   ref domainName, ref counterName, ref requiredCounterValue);
                }

                previousNode = conditionNode;

            }
            return previousNode;
        }

        private QuestNode CreatSuccessStepNode( QuestNode conditionNode, QuestBuilder questBuilder, int nodeIndex, Action action, ref string targetEntity, ref string targetDescriptor, ref string domainName, ref string counterName, ref int requiredCounterValue )
        {

            var completion = action.Completion;
            var activeState = conditionNode.StateInfoList[(int)QuestNodeState.Active];

            foreach (var cc in completion)
            {
                switch (cc.mode)
                {
                    case ActionCompletion.Mode.Counter:
                        // Setup counter condition:
                        counterName = cc.baseCounterName + nodeIndex.ToString();
                        //if (!counterNames.Contains( counterName ))
                        {
                            var counter = questBuilder.AddCounter(counterName, cc.initialValue, cc.minValue, cc.initialValue, false, cc.updateMode);
                            foreach (var messageEvent in cc.messageEventList)
                            {
                                var counterMessageEvent = new QuestCounterMessageEvent(messageEvent.targetID, messageEvent.message,
                                    messageEvent.parameter.Replace("{TARGETENTITY}", targetEntity),
                                    messageEvent.operation, messageEvent.literalValue);
                                counter.messageEventList.Add(counterMessageEvent);
                            }
                        }
                        requiredCounterValue = cc.requiredValue;
                        switch (cc.conditionType)
                        {
                            case ActionCompletion.ConditionType.Default:
                                questBuilder.AddCounterCondition(conditionNode, counterName, CounterValueConditionMode.AtLeast, requiredCounterValue);
                                break;
                            case ActionCompletion.ConditionType.Timer:
                                questBuilder.AddTimerCounterCondition(conditionNode, counterName, CounterValueConditionMode.AtLeast, requiredCounterValue);
                                break;
                        }
                        // Consider: Add action to reset counter to zero in case future nodes repeat the same counter?
                        break;
                    case ActionCompletion.Mode.Message:

                        // Setup message condition:
                        switch (cc.conditionType)
                        {
                            case ActionCompletion.ConditionType.Default:
                                questBuilder.AddMessageCondition(conditionNode, QuestMessageParticipant.Any, cc.senderID, QuestMessageParticipant.Quester, cc.targetID,
                                    cc.message, cc.parameter.Replace("{TARGETENTITY}", targetEntity));
                           
                                break;
                        }
                        break;
                }
            }



            AddStepNodeText(questBuilder, conditionNode, activeState, targetEntity, targetDescriptor, domainName, counterName, requiredCounterValue, action);

            // Send message action:
            if (!string.IsNullOrEmpty(action.SendMessageOnActive))
            {
                // var messageAction = questBuilder.CreateMessageAction( ReplaceStepTags( step.action.SendMessageOnActive, targetEntity, targetDescriptor, domainName, counterName, requiredCounterValue ) );
                var messageAction = questBuilder.CreateMessageAction(action.SendMessageOnActive, action.ActiveRequiredValue);

                activeState.actionList.Add(messageAction);
            }

            // Actions when completed:
            if (!string.IsNullOrEmpty(action.SendMessageOnCompletion))
            {
                var trueState = conditionNode.StateInfoList[(int)QuestNodeState.True];
                // var messageAction = questBuilder.CreateMessageAction( ReplaceStepTags( step.action.SendMessageOnCompletion, targetEntity, targetDescriptor, domainName, counterName, requiredCounterValue ) );
                var messageAction = questBuilder.CreateMessageAction(ReplaceStepTags(action.SendMessageOnCompletion, targetEntity, targetDescriptor, domainName, counterName, requiredCounterValue));

                trueState.actionList.Add(messageAction);
            }
            return conditionNode;
        }

        private void CreatFailtureStepNode( QuestNode conditionNode, QuestBuilder questBuilder, int nodeIndex, Action action, ref string targetEntity, ref string targetDescriptor, ref string domainName, ref string counterName, ref int requiredCounterValue )
        {
            CreatSuccessStepNode(conditionNode, questBuilder, nodeIndex, action, ref targetEntity, ref targetDescriptor,
                ref domainName, ref counterName, ref requiredCounterValue);
            questBuilder.AddFailureNode(conditionNode);
        }


        /// <summary>
        /// Replaces special tags that are specific to generated quests.
        /// </summary>
        /// <param name="s">Text to modify</param>
        /// <param name="targetEntity">Target entity name.</param>
        /// <param name="targetDescriptor">Target descriptor (including count).</param>
        /// <param name="domainName">Domain name.</param>
        /// <param name="counterName">Counter name.</param>
        /// <param name="counterValue">Counter value.</param>
        /// <returns></returns>
        protected virtual string ReplaceStepTags( string s, string targetEntity, string targetDescriptor, string domainName, string counterName, int counterValue )
        {
            return s.Replace("{#COUNTERNAME}", "{#" + counterName + "}").
                    Replace("{#COUNTERGOAL}", counterValue.ToString()).
                    Replace("{TARGETENTITY}", targetEntity).
                    Replace("{TARGETDESCRIPTOR}", targetDescriptor).
                    Replace("{DOMAIN}", domainName);
        }

        /// <summary>
        /// Adds the text for a step.
        /// </summary>
        protected virtual void AddStepNodeText( QuestBuilder questBuilder, QuestNode conditionNode, QuestStateInfo activeState, string targetEntity, string targetDescriptor, string domainName, string counterName, int requiredCounterValue, Action action )
        {
            // Text for condition node's Active state:
            var taskText = action.ActionText.activeText.dialogueText;//ReplaceStepTags( step.action.ActionText.activeText.dialogueText, targetEntity, targetDescriptor, domainName, counterName, requiredCounterValue );
            var bodyText = questBuilder.CreateBodyContent(taskText);
            var dialogueList = activeState.categorizedContentList[(int)QuestContentCategory.Dialogue];
            dialogueList.contentList.Add(bodyText);

            var jrlText = action.ActionText.activeText.journalText;//ReplaceStepTags( step.action.ActionText.activeText.journalText, targetEntity, targetDescriptor, domainName, counterName, requiredCounterValue );
            var jrlbodyText = questBuilder.CreateBodyContent(jrlText);
            var journalList = activeState.categorizedContentList[(int)QuestContentCategory.Journal];
            journalList.contentList.Add(jrlbodyText);

            var hudText = action.ActionText.activeText.hudText;//ReplaceStepTags( step.action.ActionText.activeText.hudText, targetEntity, targetDescriptor, domainName, counterName, requiredCounterValue );
            var hudbodyText = questBuilder.CreateBodyContent(hudText);
            var hudList = activeState.categorizedContentList[(int)QuestContentCategory.HUD];
            hudList.contentList.Add(hudbodyText);
        }

        #endregion

        #region 奖励构造
        /// <summary>
        /// 添加任务完成得奖励
        /// </summary>
        /// <param name="questBuilder"></param>
        /// <param name="rewardPoint"></param>
        /// <param name="rewardsUIContents"></param>
        /// <param name="rewardSystems"></param>
        private void AddRewards( QuestBuilder questBuilder, int rewardPoint, List<QuestContent> rewardsUIContents, List<RewardSystem> rewardSystems )
        {
            questBuilder.AddOfferContents(QuestContent.CloneList<QuestContent>(rewardsUIContents).ToArray());
            foreach (var rewardSystem in rewardSystems)
            {
                if (rewardSystem == null)
                    continue;
                rewardSystem.DetermineReward(rewardPoint, questBuilder.quest);
            }
        }
        #endregion

        #region  模板
        /// <summary>
        /// 任务模板创建（示例）
        /// </summary>
        /// <param name="name">步骤名称</param>
        /// <param name="desc">步骤描述</param>
        /// <param name="countName">计时事件名称</param>
        /// <param name="message">消息名称</param>
        /// <param name="param">消息参数</param>
        /// <param name="onActiveMsg">步骤激活触发消息名称</param>
        /// <param name="onCompleteMsg">步骤完成触发消息名称</param>
        /// <param name="score">分值</param>
        /// <param name="acitveRequireValue">步骤需要的参数</param>
        /// <returns></returns>
        public virtual Step CreateStep( string name, string desc, string countName, string message, string param, string onActiveMsg = "", string onCompleteMsg = "", int score = 1, string acitveRequireValue = "" )
        {
            //每个步骤必须有successAction
            Action action = new Action();
            action.DisplayName = name;
            action.Description = desc;
            //set text
            action.ActionText.activeText.dialogueText = "";
            action.ActionText.activeText.hudText = "";
            action.ActionText.activeText.journalText = desc;
            //set 完成条件列表
            action.Completion = new List<ActionCompletion>();
            ActionCompletion cc = new ActionCompletion();
            cc.baseCounterName = countName;
            cc.mode = ActionCompletion.Mode.Message;
            //定义消息名称（全局定制（2个对应消息名称）
            //              步骤激活：1、访问全局监听代理，是否已经在执行步骤）
            //                        2、目标物体发送事件
            cc.message = message;
            //参数
            cc.parameter = param;
            action.Completion.Add(cc);

            action.SendMessageOnActive = onActiveMsg;//节点触发时发的消息事件
            action.ActiveRequiredValue = acitveRequireValue;//节点触发时发的消息参数一般与任务行为参数保持一致
            action.SendMessageOnCompletion = onCompleteMsg;//节点完成时发送的消息事件
            Step step = new Step(action);

            return step;
        }

        #endregion

    }
}