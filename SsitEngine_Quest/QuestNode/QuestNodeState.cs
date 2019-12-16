namespace SsitEngine.QuestManager
{

    /// <summary>
    /// 任务节点可以处于的状态。注意任务是
    /// 总体状态由questState指定
    /// </summary>
    public enum QuestNodeState
    {
        /// <summary>
        /// 等待激活
        /// </summary>
        Inactive,

        /// <summary>
        /// 等待条件满足
        /// </summary>
        Active,

        /// <summary>
        /// 条件满足
        /// </summary>
        True

    }

}
