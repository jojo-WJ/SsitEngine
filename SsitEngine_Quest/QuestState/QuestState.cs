

namespace SsitEngine.QuestManager
{

    /// <summary>
    /// 任务可以进入的状态.
    /// [注意]任务节点的状态归属任务节点自己
    /// </summary>
    public enum QuestState
    {
        /// <summary>
        /// Not active yet.
        /// </summary>
        WaitingToStart,

        /// <summary>
        /// Waiting for the quester to complete objectives.
        /// </summary>
        Active,

        /// <summary>
        /// Quester completed the objectives successfully.
        /// </summary>
        Successful,

        /// <summary>
        /// Quester failed to complete the objectives.
        /// </summary>
        Failed,

        /// <summary>
        /// Quester abandoned the quest.
        /// </summary>
        Abandoned,

        /// <summary>
        /// Quest is disabled.
        /// </summary>
        Disabled
    }

}
