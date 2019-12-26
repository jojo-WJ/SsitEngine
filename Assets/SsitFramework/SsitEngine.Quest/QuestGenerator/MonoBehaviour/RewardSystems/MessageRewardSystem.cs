using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// This reward system sends a message with number of user-definable "things".
    /// </summary>
    [AddComponentMenu("")] // Use wrapper instead.
    public class MessageRewardSystem : RewardSystem
    {
        [Tooltip("Consume reward points when determining reward.")]
        public bool consumePoints = true;

        public string message = "Get";
        public string parameter = "Score";

        [Tooltip("奖励点的缩放曲线")]
        public AnimationCurve pointsCurve = new AnimationCurve(new Keyframe(1, 1), new Keyframe(100, 100));

        public string target = QuestMachineTags.QUESTERID;

        public string thing = "Score points";

        public override int DetermineReward( int points, Quest quest )
        {
            var val = (int) pointsCurve.Evaluate(points);

            if (!string.IsNullOrEmpty(thing))
            {
                var bodyText = BodyTextQuestContent.CreateInstance<BodyTextQuestContent>();
                bodyText.BodyText = val + " " + thing;
                quest.OfferContentList.Add(bodyText);
            }

            var action = MessageQuestAction.CreateInstance<MessageQuestAction>();
            action.SenderId = QuestMachineTags.QUESTGIVERID;
            action.TargetId = target;
            action.Message = message;
            action.Parameter = parameter;
            action.Value.ValueType = MessageValueType.Int;
            action.Value.IntValue = val;
            var successInfo = quest.GetStateInfo(QuestState.Successful);
            successInfo.actionList.Add(action);

            return consumePoints ? points - val : points;
        }

        public override int DetermineReward( int points, QuestNode questNode )
        {
            var sp = points;
            return points;
        }
    }
}