namespace SsitEngine.QuestManager
{

    /// <summary>
    /// 条件模式（串并连）
    /// </summary>
    public enum ConditionCountMode
    {
        /// <summary>
        /// The condition becomes true if any watched condition becomes true.
        /// </summary>
        Any,

        /// <summary>
        /// The condition becomes true if all watched conditions become true.
        /// </summary>
        All,

        /// <summary>
        /// The condition becomes true if a specified minimum number of watched conditions become true.
        /// </summary>
        Min
    }

}
