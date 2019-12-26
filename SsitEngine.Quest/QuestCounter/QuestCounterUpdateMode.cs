namespace SsitEngine.QuestManager
{
    /// <summary>
    /// 计数器的更新方式
    /// </summary>
    public enum QuestCounterUpdateMode
    {
        /// <summary>
        /// Counter uses a DataSynchronizer to synchronize its value with an external source.
        /// Synchronizes using the counter's name.
        /// </summary>
        DataSync,

        /// <summary>
        /// Counter listens for messages and adjusts its value accordingly.
        /// </summary>
        Messages
    }
}