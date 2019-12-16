/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/1 12:17:54                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Unity;

namespace SsitEngine.QuestManager
{
    public enum EnQuestEvent
    {

        /// <summary>
        /// 任务状态改变事件
        /// </summary>
        QuestStateChangedMessage = EnMsgCenter.QuestEvent + 1,

        /// <summary>
        /// 设置任务计数器
        /// </summary>
        SetQuestCounter,
        /// <summary>
        /// 递增任务计数器
        /// </summary>
        IncrementQuestCounter,

        /// <summary>
        /// 计数器改变事件
        /// </summary>
        QuestCounterChangedMessage,

        /// <summary>
        /// 设置任务指示状态事件
        /// </summary>
        SetIndicatorStateMessage,
        /// <summary>
        /// 刷新任务指示
        /// </summary>
        RefreshIndicatorMessage,
        /// <summary>
        /// 任务追踪事件
        /// </summary>
        QuestTrackToggleChangedMessage,

        /// <summary>
        /// 任务拒绝
        /// </summary>
        QuestAbandonedMessage,

        /// <summary>
        /// 在开始与提问者对话之前发送
        /// </summary>
        GreetMessage,

        /// <summary>
        /// 在开始与发问者对话后发送。
        /// </summary>
        GreetedMessage,

        /// <summary>
        /// 会话消息
        /// </summary>
        DiscussedQuestMessage,

        /// <summary>
        /// 数据同步
        /// </summary>
        DataSourceValueChangedMessage,

        /// <summary>
        /// 刷新任务奖励
        /// </summary>
        RefreshQuestScore,

        //...
        MaxValue
    }


}