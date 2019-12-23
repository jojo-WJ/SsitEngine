using System.Collections.Generic;

namespace SsitEngine.QuestManager
{
    public interface IQuestBuildHelper
    {
        Quest ConvertTemplateToQuest( string id, string title, string group, string desc, QuestTemplate questTemplate,
            List<QuestContent> rewardsUiContents, List<RewardSystem> rewardSystems );

        Step CreateStep( string name, string desc, string countName, string message, string param,
            string onActiveMsg = "", string onCompleteMsg = "", int score = 1, string acitveRequireValue = "" );
    }
}