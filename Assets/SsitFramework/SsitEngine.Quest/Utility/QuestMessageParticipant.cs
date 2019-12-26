namespace SsitEngine.QuestManager
{
    /// <summary>
    /// 消息参与者
    /// </summary>
    public enum QuestMessageParticipant
    {
        /// <summary>
        /// Accepts any object.
        /// </summary>
        Any,

        /// <summary>
        /// Must be the quester assigned to this quest.
        /// </summary>
        Quester,

        /// <summary>
        /// Must be this quest's quest giver.
        /// </summary>
        QuestGiver,

        /// <summary>
        /// Specified by ID.
        /// </summary>
        Other
    }
}