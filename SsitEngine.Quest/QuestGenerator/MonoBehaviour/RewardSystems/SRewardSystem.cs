namespace SsitEngine.QuestManager
{
    /// <summary>
    /// 发送分值奖励系统
    /// </summary>
    public class SRewardSystem : RewardSystem
    {
        public override int DetermineReward( int points, Quest quest )
        {
            var sp = points;

            var bodyText = BodyTextQuestContent.CreateInstance<BodyTextQuestContent>();
            bodyText.BodyText = sp + " Score points";
            quest.OfferContentList.Add(bodyText);

            var xpAction = MessageQuestAction.CreateInstance<MessageQuestAction>();
            xpAction.SenderId = QuestMachineTags.QUESTGIVERID; // Send from quest giver.
            xpAction.TargetId = QuestMachineTags.QUESTERID; // Send to quester (player).
            xpAction.Message = ((int) EnQuestEvent.RefreshQuestScore).ToString();
            xpAction.Parameter = sp.ToString();
            xpAction.Value = new MessageValue(points);
            var successInfo = quest.GetStateInfo(QuestState.Successful);
            successInfo.actionList.Add(xpAction);

            return points;
        }

        public override int DetermineReward( int points, QuestNode questNode )
        {
            var sp = points;
            var activeState = questNode.StateInfoList[(int) QuestNodeState.True];

            var bodyText = BodyTextQuestContent.CreateInstance<BodyTextQuestContent>();
            bodyText.BodyText = sp + " Score points";
            activeState.GetContentList(QuestContentCategory.Dialogue).Add(bodyText);

            var xpAction = MessageQuestAction.CreateInstance<MessageQuestAction>();
            xpAction.SenderId = QuestMachineTags.QUESTGIVERID; // Send from quest giver.
            xpAction.TargetId = QuestMachineTags.QUESTERID; // Send to quester (player).
            xpAction.Message = ((int) EnQuestEvent.RefreshQuestScore).ToString();
            xpAction.Parameter = sp.ToString();
            xpAction.Value = new MessageValue(points);
            activeState.actionList.Add(xpAction);
            return points;
        }
    }
}