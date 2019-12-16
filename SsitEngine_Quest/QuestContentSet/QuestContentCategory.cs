

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// These categories of content may be shown in UIs.
    /// 任务内容类别
    /// </summary>
    public enum QuestContentCategory
    {
        /// <summary>
        /// Content shown in the dialogue UI.
        /// </summary>
        Dialogue,

        /// <summary>
        /// Content shown in the quest journal UI.
        /// </summary>
        Journal,

        /// <summary>
        /// Content shown in the quest tracker HUD.
        /// </summary>
        HUD,

        /// <summary>
        /// Content shown in the alert UI.
        /// </summary>
        Alert,

        /// <summary>
        /// Content shown in the dialogue UI when the quest giver offers the quest.
        /// </summary>
        Offer,

        /// <summary>
        /// Content shown in the dialogue UI when the offer conditions haven't been met yet.
        /// </summary>
        OfferConditionsUnmet
    }

}
