/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：任务系统辅助器（主要为上层提供实现接口，以便底层调用上层接口实现业务逻辑）                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年5月13日 12点31分                             
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// 任务系统辅助器
    /// </summary>
    public interface IQuestHelper
    {
        Quest ConvertTemplateToQuest( string id, string title, string group, string desc, QuestTemplate questTemplate,
            List<QuestContent> rewardsUiContents, List<RewardSystem> rewardSystems );

        Step CreateStep( string name, string desc, string countName, string message, string param,
            string onActiveMsg = "", string onCompleteMsg = "", int score = 1, string acitveRequireValue = "" );

        void CreateQuestIndicator( QuestMessageArgs msgArgs );

        void RefreshQuestIndicator( QuestMessageArgs msgArgs );
    }
}