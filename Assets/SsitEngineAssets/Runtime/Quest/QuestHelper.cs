using System;
using System.Collections.Generic;
using Framework.JQuest;
using SsitEngine.PureMVC.Patterns;
using UnityEngine;
using Framework.SceneObject;
using SsitEngine.Core.ObjectPool;
using SsitEngine.QuestManager;
using SsitEngine.Unity.HUD;
using Action = SsitEngine.QuestManager.Action;

namespace Framework.Quest
{
    public class QuestHelper : IQuestHelper
    {
        private IObjectPool<QuestIndicatorHUD> m_QuestIndicatorPool;

        public QuestHelper( IObjectPool<QuestIndicatorHUD> questIndicatorPool )
        {
            m_QuestIndicatorPool = questIndicatorPool;
        }

        #region 任务资源转换

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
        protected virtual void AddQuestHeading( QuestBuilder questBuilder, QuestContentCategory category,
            bool addToSuccessfulList )
        {
            // Add to Active state and, if not HUD, to Successful state.
            questBuilder.AddContents(
                questBuilder.quest.StateInfoList[(int) QuestState.Active].categorizedContentList[(int) category],
                questBuilder.CreateTitleContent());
            if (addToSuccessfulList && category != QuestContentCategory.HUD)
            {
                questBuilder.AddContents(
                    questBuilder.quest.StateInfoList[(int) QuestState.Successful]
                        .categorizedContentList[(int) category], questBuilder.CreateTitleContent());
            }
        }

        private void AddSuccessfulText( QuestBuilder questBuilder, object mainTargetEntity, object mainTargetDescriptor,
            object domainName, object goal )
        {
        }

        public SsitEngine.QuestManager.Quest ConvertTemplateToQuest( string id, string title, string group, string desc,
            QuestTemplate questTemplate, List<QuestContent> rewardsUiContents = null,
            List<RewardSystem> rewardSystems = null )
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

            // reward
            if (rewardSystems == null)
            {
                rewardSystems = new List<RewardSystem>
                {
                    new SRewardSystem()
                };
            }

            // Successful text (shown in journal / when talking again about successful quest):
            // AddSuccessfulText( questBuilder, mainTargetEntity, mainTargetDescriptor, domainName, goal );

            // Add steps:
            var previousNode = AddSteps(questBuilder, string.Empty, questTemplate, rewardSystems);

            // Add "return to giver" node:
            //if (requireReturnToComplete)
            //{
            //previousNode = AddReturnNode( questBuilder, previousNode, entity, mainTargetEntity, mainTargetDescriptor, domainName, goal );
            //}

            // Success node:
            questBuilder.AddSuccessNode(previousNode);

            //all reward 
            //AddRewards(questBuilder, questTemplate.GetAllScore(), rewardsUiContents, rewardSystems);

            return questBuilder.ToQuest();
        }

        /// <summary>
        /// Adds the plan's steps.
        /// </summary>
        /// <param name="questBuilder">QuestBuilder.</param>
        /// <param name="domainName">Main target's domain.</param>
        /// <param name="plan">List of steps that end with goal step.</param>
        /// <returns>The last node added.</returns>
        protected virtual QuestNode AddSteps( QuestBuilder questBuilder, string domainName, QuestTemplate plan,
            List<RewardSystem> rewardSystems )
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
                var conditionNode =
                    questBuilder.AddConditionNode(previousNode, id, internalName, ConditionCountMode.All);

                // Variables for node text tag replacement:
                var counterName = string.Empty;
                int requiredCounterValue = 0;

                CreatSuccessStepNode(conditionNode, questBuilder, nodeCount, step.SuccesAction, ref targetEntity,
                    ref targetDescriptor, ref domainName, ref counterName, ref requiredCounterValue);
                // Create failtrue condition node
                if (step.FailureAction != null && step.FailureAction.Completion != null &&
                    step.FailureAction.Completion.Count > 0)
                {
                    targetEntity = step.FailureAction.DisplayName;
                    targetDescriptor = step.FailureAction.Description;
                    nodeCount++;
                    id = nodeCount.ToString();
                    internalName = step.FailureAction.Description + " " + targetDescriptor;
                    var failtureNode =
                        questBuilder.AddConditionNode(previousNode, id, internalName, ConditionCountMode.Any);
                    CreatFailtureStepNode(failtureNode, questBuilder, nodeCount, step.FailureAction, ref targetEntity,
                        ref targetDescriptor, ref domainName, ref counterName, ref requiredCounterValue);
                }

                // action reward
                if (step.scoreValue != 0)
                {
                    AddRewards(conditionNode, step.scoreValue, rewardSystems);
                }

                previousNode = conditionNode;
            }
            return previousNode;
        }

        QuestNode CreatSuccessStepNode( QuestNode conditionNode, QuestBuilder questBuilder, int nodeIndex,
            Action action, ref string targetEntity, ref string targetDescriptor, ref string domainName,
            ref string counterName, ref int requiredCounterValue )
        {
            var completion = action.Completion;
            var activeState = conditionNode.StateInfoList[(int) QuestNodeState.Active];
            var completeState = conditionNode.StateInfoList[(int) QuestNodeState.True];
            foreach (var cc in completion)
            {
                switch (cc.mode)
                {
                    case ActionCompletion.Mode.Counter:
                        // Setup counter condition:
                        counterName = cc.baseCounterName + nodeIndex.ToString();
                        //if (!counterNames.Contains( counterName ))
                    {
                        var counter = questBuilder.AddCounter(counterName, cc.initialValue, cc.minValue,
                            cc.initialValue, false, cc.updateMode);
                        foreach (var messageEvent in cc.messageEventList)
                        {
                            var counterMessageEvent = new QuestCounterMessageEvent(messageEvent.targetID,
                                messageEvent.message,
                                messageEvent.parameter.Replace("{TARGETENTITY}", targetEntity),
                                messageEvent.operation, messageEvent.literalValue);
                            counter.messageEventList.Add(counterMessageEvent);
                        }
                    }
                        requiredCounterValue = cc.requiredValue;
                        switch (cc.conditionType)
                        {
                            case ActionCompletion.ConditionType.Default:
                                questBuilder.AddCounterCondition(conditionNode, counterName,
                                    CounterValueConditionMode.AtLeast, requiredCounterValue);
                                break;
                            case ActionCompletion.ConditionType.Timer:
                                questBuilder.AddTimerCounterCondition(conditionNode, counterName,
                                    CounterValueConditionMode.AtLeast, requiredCounterValue);
                                break;
                        }
                        // Consider: Add action to reset counter to zero in case future nodes repeat the same counter?
                        break;
                    case ActionCompletion.Mode.Message:

                        // Setup message condition:
                        switch (cc.conditionType)
                        {
                            case ActionCompletion.ConditionType.Default:
                                questBuilder.AddMessageCondition(conditionNode, QuestMessageParticipant.Any,
                                    cc.senderID, QuestMessageParticipant.Quester, cc.targetID, cc.message,
                                    cc.parameter.Replace("{TARGETENTITY}", targetEntity));

                                // 指示器设置
                                if (cc.message == ((int) En_QuestsMsg.En_Interactive).ToString())
                                {
                                    var indicatorAction = questBuilder.CreateSetIndicatorAction(questBuilder.quest.Id,
                                        DeGeneratorParam2Target(En_QuestsMsg.En_Interactive, cc.parameter),
                                        QuestIndicatorState.Custom0);
                                    activeState.actionList.Add(indicatorAction);

                                    indicatorAction = questBuilder.CreateSetIndicatorAction(questBuilder.quest.Id,
                                        DeGeneratorParam2Target(En_QuestsMsg.En_Interactive, cc.parameter),
                                        QuestIndicatorState.None);
                                    completeState.actionList.Add(indicatorAction);
                                }
                                else if (cc.message == ((int) En_QuestsMsg.En_ArrivedTo).ToString())
                                {
                                    var indicatorAction = questBuilder.CreateSetIndicatorAction(questBuilder.quest.Id,
                                        DeGeneratorParam2Target(En_QuestsMsg.En_ArrivedTo, cc.parameter),
                                        QuestIndicatorState.Custom0);
                                    activeState.actionList.Add(indicatorAction);

                                    indicatorAction = questBuilder.CreateSetIndicatorAction(questBuilder.quest.Id,
                                        DeGeneratorParam2Target(En_QuestsMsg.En_ArrivedTo, cc.parameter),
                                        QuestIndicatorState.None);
                                    completeState.actionList.Add(indicatorAction);
                                }
                                break;
                        }

                        break;
                }
            }


            AddStepNodeText(questBuilder, conditionNode, activeState, targetEntity, targetDescriptor, domainName,
                counterName, requiredCounterValue, action);

            // Actions when active:(此系统版本不启用)
            //if (!string.IsNullOrEmpty( action.ActionText.activeText.alertText ))
            //{
            //    // Alert action:
            //    // var alertAction = questBuilder.CreateAlertAction( ReplaceStepTags( step.action.ActionText.activeText.alertText, targetEntity, targetDescriptor, domainName, counterName, requiredCounterValue ) );
            //    var alertAction = questBuilder.CreateAlertAction( ReplaceStepTags( action.ActionText.activeText.alertText, targetEntity, targetDescriptor, domainName, counterName, requiredCounterValue ) );

            //    activeState.actionList.Add( alertAction );
            //}


            // Send message action:
            if (!string.IsNullOrEmpty(action.SendMessageOnActive))
            {
                int msgid = action.SendMessageOnActive.ParseByDefault(0);

                if (msgid != 0)
                {
                    var messageAction = ScriptableObject.CreateInstance<MessageTypeQuestAction>();
                    messageAction.Message = action.SendMessageOnActive;
                    messageAction.Parameter = action.ActiveRequiredValue;
                    messageAction.TargetSpecifier = QuestMessageParticipant.Quester;
                    activeState.actionList.Add(messageAction);
                }
            }

            // Actions when completed:(此系统版本不启用)
            //if (!string.IsNullOrEmpty(action.SendMessageOnCompletion))
            //{
            //    var trueState = conditionNode.StateInfoList[(int)QuestNodeState.True];
            //    // var messageAction = questBuilder.CreateMessageAction( ReplaceStepTags( step.action.SendMessageOnCompletion, targetEntity, targetDescriptor, domainName, counterName, requiredCounterValue ) );
            //    var messageAction = questBuilder.CreateMessageAction(ReplaceStepTags(action.SendMessageOnCompletion, targetEntity, targetDescriptor, domainName, counterName, requiredCounterValue));

            //    trueState.actionList.Add(messageAction);
            //}


            return conditionNode;
        }

        void CreatFailtureStepNode( QuestNode conditionNode, QuestBuilder questBuilder, int nodeIndex, Action action,
            ref string targetEntity, ref string targetDescriptor, ref string domainName, ref string counterName,
            ref int requiredCounterValue )
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
        protected virtual string ReplaceStepTags( string s, string targetEntity, string targetDescriptor,
            string domainName, string counterName, int counterValue )
        {
            return s.Replace("{#COUNTERNAME}", "{#" + counterName + "}")
                .Replace("{#COUNTERGOAL}", counterValue.ToString()).Replace("{TARGETENTITY}", targetEntity)
                .Replace("{TARGETDESCRIPTOR}", targetDescriptor).Replace("{DOMAIN}", domainName);
        }

        /// <summary>
        /// Adds the text for a step.
        /// </summary>
        protected virtual void AddStepNodeText( QuestBuilder questBuilder, QuestNode conditionNode,
            QuestStateInfo activeState, string targetEntity, string targetDescriptor, string domainName,
            string counterName, int requiredCounterValue, Action action )
        {
            // Text for condition node's Active state:
            var taskText =
                action.ActionText.activeText
                    .dialogueText; //ReplaceStepTags( step.action.ActionText.activeText.dialogueText, targetEntity, targetDescriptor, domainName, counterName, requiredCounterValue );
            var bodyText = questBuilder.CreateBodyContent(taskText);
            var dialogueList = activeState.categorizedContentList[(int) QuestContentCategory.Dialogue];
            dialogueList.contentList.Add(bodyText);

            var jrlText =
                action.ActionText.activeText
                    .journalText; //ReplaceStepTags( step.action.ActionText.activeText.journalText, targetEntity, targetDescriptor, domainName, counterName, requiredCounterValue );
            var jrlbodyText = questBuilder.CreateBodyContent(jrlText);
            var journalList = activeState.categorizedContentList[(int) QuestContentCategory.Journal];
            journalList.contentList.Add(jrlbodyText);

            var hudText =
                action.ActionText.activeText
                    .hudText; //ReplaceStepTags( step.action.ActionText.activeText.hudText, targetEntity, targetDescriptor, domainName, counterName, requiredCounterValue );
            var hudbodyText = questBuilder.CreateBodyContent(hudText);
            var hudList = activeState.categorizedContentList[(int) QuestContentCategory.HUD];
            hudList.contentList.Add(hudbodyText);
        }


        /// <summary>
        /// 添加任务完成得奖励
        /// </summary>
        /// <param name="questBuilder"></param>
        /// <param name="rewardPoint"></param>
        /// <param name="rewardsUIContents"></param>
        /// <param name="rewardSystems"></param>
        private void AddRewards( QuestBuilder questBuilder, int rewardPoint, List<QuestContent> rewardsUIContents,
            List<RewardSystem> rewardSystems )
        {
            questBuilder.AddOfferContents(QuestSubasset.CloneList<QuestContent>(rewardsUIContents).ToArray());


            foreach (var rewardSystem in rewardSystems)
            {
                if (rewardSystem == null)
                    continue;
                rewardSystem.DetermineReward(rewardPoint, questBuilder.quest);
            }
        }


        /// <summary>
        /// 添加任务完成得奖励
        /// </summary>
        /// <param name="node"></param>
        /// <param name="rewardPoint"></param>
        /// <param name="rewardSystems"></param>
        private void AddRewards( QuestNode node, int rewardPoint, List<RewardSystem> rewardSystems )
        {
            //questBuilder.AddOfferContents(QuestSubasset.CloneList<QuestContent>(rewardsUIContents).ToArray());
            foreach (var rewardSystem in rewardSystems)
            {
                if (rewardSystem == null)
                    continue;
                rewardSystem.DetermineReward(rewardPoint, node);
            }
        }

        #endregion

        #region 任务步骤

        /// <summary>
        /// 创建任务步骤
        /// </summary>
        public Step CreateStep( string name, string desc, string countName, string message, string param,
            string onActiveMsg = "", string SendMessageOnActive = "", int score = 1, string acitveRequireValue = "" )
        {
            Action action = new Action();
            action.DisplayName = name;
            action.Description = desc;
            //set text
            action.ActionText.activeText.dialogueText = "";
            action.ActionText.activeText.hudText = "";
            action.ActionText.activeText.journalText = desc;
            //set main
            action.Completion = new List<ActionCompletion>();

            ActionCompletion cc = new ActionCompletion();
            cc.baseCounterName = countName;

            cc.mode = ActionCompletion.Mode.Message;
            cc.conditionType = ActionCompletion.ConditionType.Default;

            cc.message = message;
            cc.parameter = param;

            action.Completion.Add(cc);

            action.ActiveRequiredValue = acitveRequireValue;
            action.SendMessageOnActive = SendMessageOnActive;
            Step step = new Step(action, null, score);
            return step;
        }

        /// <summary>
        /// 创建多参数条件的任务步骤（举例：穿戴个体防护）
        /// </summary>
        public Step CreateStep( En_QuestsMsg questType, string name, string desc, string countName, string message,
            List<string> paramList, int score )
        {
            Action action = new Action();
            action.DisplayName = name;
            action.Description = desc;
            //set text
            action.ActionText.activeText.dialogueText = "";
            action.ActionText.activeText.hudText = "";
            action.ActionText.activeText.journalText = desc;
            //set main
            action.Completion = new List<ActionCompletion>();

            for (int i = 0; i < paramList.Count; i++)
            {
                ActionCompletion cc = new ActionCompletion();
                cc.baseCounterName = countName;

                cc.mode = ActionCompletion.Mode.Message;
                cc.conditionType = ActionCompletion.ConditionType.Default;

                cc.message = message;
                //cc.parameter = paramList[i];
                cc.parameter = QuestHelper.GeneratorParam(questType, paramList[i]);

                action.Completion.Add(cc);
            }

            action.SendMessageOnActive = ((int) questType + 1).ToString();
            action.ActiveRequiredValue = string.Join(QuestMachineTags.SINGLESPLIT, paramList.ToArray());

            Step step = new Step(action, null, score);
            return step;
        }

        /// <summary>
        /// 创建通用单条件任务步骤
        /// </summary>
        public Step CreateStep( En_QuestsMsg questType, string name, string desc, string countName, string message,
            string param, int score, int requireValue = 0, bool hasFailtureNode = false )
        {
            Action action = new Action();
            action.DisplayName = name;
            action.Description = desc;
            //set text
            action.ActionText.activeText.dialogueText = "";
            action.ActionText.activeText.hudText = "";
            action.ActionText.activeText.journalText = desc;
            //set main
            action.Completion = new List<ActionCompletion>();

            ActionCompletion cc = new ActionCompletion();
            cc.baseCounterName = countName;

            cc.mode = ActionCompletion.Mode.Message;
            cc.conditionType = ActionCompletion.ConditionType.Default;

            cc.message = message;
            cc.parameter = param;

            action.Completion.Add(cc);

            action.ActiveRequiredValue = param;
            action.SendMessageOnActive = ((int) questType + 1).ToString();
            Action failtureAction = null;
            if (hasFailtureNode)
            {
                failtureAction = new Action();
                failtureAction.DisplayName = name + "failtrue";
                failtureAction.Description = desc + "failtrue";
                //set text
                failtureAction.ActionText.activeText.dialogueText = "";
                failtureAction.ActionText.activeText.hudText = "";
                failtureAction.ActionText.activeText.journalText = desc + "failtrue";
                //set main
                failtureAction.Completion = new List<ActionCompletion>();

                ActionCompletion c1 = new ActionCompletion
                {
                    baseCounterName = countName,
                    mode = ActionCompletion.Mode.Counter,
                    conditionType = ActionCompletion.ConditionType.Timer,
                    message = message,
                    parameter = param,
                    initialValue = requireValue,
                    requiredValue = requireValue
                };

                failtureAction.Completion.Add(c1);
            }
            Step step = new Step(action, failtureAction, score);
            return step;
        }

        /// <summary>
        /// 创建多参数条件的任务步骤（举例:创建公议规则任务步骤)
        /// </summary>
        public Step CreateStep( En_QuestsMsg questType, string name, string desc, string countName, string message,
            List<string> openList, List<string> closemList, int score )
        {
            Action action = new Action();
            action.DisplayName = name;
            action.Description = desc;
            //set text
            action.ActionText.activeText.dialogueText = "";
            action.ActionText.activeText.hudText = "";
            action.ActionText.activeText.journalText = desc;
            //set main
            action.Completion = new List<ActionCompletion>();

            List<string> activeValue = new List<string>();

            for (int i = 0; i < openList.Count; i++)
            {
                ActionCompletion cc = new ActionCompletion();
                cc.baseCounterName = countName;

                cc.mode = ActionCompletion.Mode.Message;
                cc.conditionType = ActionCompletion.ConditionType.Default;

                cc.message = message;
                cc.parameter = GeneratorParam(questType, true, openList[i]);
                activeValue.Add(cc.parameter);
                action.Completion.Add(cc);
            }

            for (int i = 0; i < closemList.Count; i++)
            {
                ActionCompletion cc = new ActionCompletion();
                cc.baseCounterName = countName;

                cc.mode = ActionCompletion.Mode.Message;
                cc.conditionType = ActionCompletion.ConditionType.Default;

                cc.message = message;
                cc.parameter = GeneratorParam(questType, false, closemList[i]);
                activeValue.Add(cc.parameter);

                action.Completion.Add(cc);
            }

            action.ActiveRequiredValue = string.Join(QuestMachineTags.SINGLESPLIT, activeValue.ToArray());
            ;
            action.SendMessageOnActive = ((int) questType + 1).ToString();

            Step step = new Step(action, null, score);
            return step;
        }

        #endregion

        #region 任务指示器

        public void CreateQuestIndicator( QuestMessageArgs msgArgs )
        {
            QuestIndicatorState indicatorState = (QuestIndicatorState) msgArgs.FirstValue;
            switch (indicatorState)
            {
                case QuestIndicatorState.Custom0:
                {
                    var sceneInstance = ObjectManager.Instance.GetObject((string) msgArgs.target);
                    if (sceneInstance != null)
                    {
                        CreateQuestIndicatorObject(msgArgs, sceneInstance.SceneInstance);
                    }
                }
                    break;
                case QuestIndicatorState.None:
                {
                    var sceneInstance = ObjectManager.Instance.GetObject((string) msgArgs.target);
                    if (sceneInstance != null)
                    {
                        RemoveQuestIndicatorObject(msgArgs, sceneInstance.SceneInstance);
                    }
                }
                    break;
            }
        }

        void CreateQuestIndicatorObject( QuestMessageArgs msgArgs, BaseSceneInstance sceneInstance )
        {
            //QuestIndicatorHUD temp = m_QuestIndicatorPool.Spawn();
            //if (temp == null)
            //{
            //    var target = new QuestIndicatorHUD(ResourcesManager.Instance.LoadAsset<GameObject>("Effect/UIEffect/UIQuest_hud", true));
            //    m_QuestIndicatorPool.Register(target, false);
            //    temp = m_QuestIndicatorPool.Spawn();
            //}
            //temp.Owner = sceneInstance;

            if (sceneInstance)
            {
                sceneInstance.GetComponentInChildren<HudElement>()?.AttachTo(NavigationElementType.Indicator, true);
            }
        }

        void RemoveQuestIndicatorObject( QuestMessageArgs msgArgs, BaseSceneInstance sceneInstance )
        {
            //m_QuestIndicatorPool.Unspawn(sceneInstance.IndicatorHud);
            //sceneInstance.IndicatorHud = null;
            if (sceneInstance)
            {
                sceneInstance.GetComponentInChildren<HudElement>()?.AttachTo(NavigationElementType.Indicator, false);
            }
        }

        public void RefreshQuestIndicator( QuestMessageArgs msgArgs )
        {
        }

        #endregion

        #region 任务参数构造

        public static string GeneratorMessage( En_QuestsMsg msg )
        {
            return ((int) msg).ToString();
        }

        public static string GeneratorParam( En_QuestsMsg msg, params object[] parms )
        {
            switch (msg)
            {
                case En_QuestsMsg.En_Interactive:
                case En_QuestsMsg.En_Technology:
                    bool isOn = (bool) parms[0];
                    var temp = isOn ? "0" : "1";
                    string[] param = new string[2] {temp, (string) parms[1]};
                    return string.Join("|", param);
                case En_QuestsMsg.En_ArrivedTo:
                    bool isDefault = (bool) parms[1];
                    var defuat = isDefault ? "1" : "0";
                    string[] param02 = new string[3] {(string) parms[0], defuat, (string) parms[2]};
                    return string.Join("|", param02);
                case En_QuestsMsg.En_WearEquip:
                case En_QuestsMsg.En_AdoptedMeasure:
                    return parms[0].ToString();
            }

            return null;
        }

        public static string[] DeGeneratorParam( En_QuestsMsg msg, string parms )
        {
            return parms.Split(new string[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string DeGeneratorParam2Target( En_QuestsMsg msg, string parms )
        {
            switch (msg)
            {
                case En_QuestsMsg.En_Interactive:
                    string[] param = parms.Split(new string[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
                    return param[1];
                case En_QuestsMsg.En_WearEquip:
                case En_QuestsMsg.En_ArrivedTo:
                case En_QuestsMsg.En_AdoptedMeasure:
                    return parms;
            }

            return null;
        }

        public static string[] ParseParam( En_QuestsMsg msg, string parms )
        {
            switch (msg)
            {
                case En_QuestsMsg.En_Interactive:
                case En_QuestsMsg.En_Technology:
                case En_QuestsMsg.En_AdoptedMeasure:
                    string[] param = parms.Split(new string[] {"|"}, StringSplitOptions.RemoveEmptyEntries);
                    return param;
                case En_QuestsMsg.En_ArrivedTo:
                case En_QuestsMsg.En_WearEquip:
                    string[] param2 = new string[] {parms};
                    return param2;
            }

            return null;
        }

        #endregion
    }
}