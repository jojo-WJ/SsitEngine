namespace SsitEngine.QuestManager
{
    public enum QuestCompleteMode
    {
        /// <summary>
        /// 单独完成
        /// </summary>
        SingleComplet,

        /// <summary>
        /// 协作完成
        /// </summary>
        CooprationComplete
    }

    /// <summary>
    /// 会话的全局模式.
    /// </summary>
    public enum CompletedQuestGlobalDialogueMode
    {
        ShowCompletedQuest,
        ShowNoQuests
    }

    /// <summary>
    /// 特定任务发起者的会话模式（覆盖全局模式）
    /// </summary>
    public enum CompletedQuestDialogueMode
    {
        SameAsGlobal,
        ShowCompletedQuest,
        ShowNoQuests
    }
}