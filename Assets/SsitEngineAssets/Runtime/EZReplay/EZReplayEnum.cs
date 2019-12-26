namespace SsitEngine.EzReplay
{
    /// <summary>
    /// 回放行为模式
    /// </summary>
    public enum ActionMode
    {
        /// <summary>
        /// 录制准备
        /// </summary>
        READY = -1,

        /// <summary>
        /// 录制中
        /// </summary>
        RECORD = 0,

        /// <summary>
        /// 回放中
        /// </summary>
        PLAY = 1,

        /// <summary>
        /// 回放暂停
        /// </summary>
        PAUSED = 2,

        /// <summary>
        /// 回放停止
        /// </summary>
        STOPPED = 3
    }

    /// <summary>
    /// 子物体标识模式
    /// </summary>
    public enum ChildIdentificationMode
    {
        /// <summary>
        /// 标识子物体索引
        /// </summary>
        IDENTIFY_BY_ORDER = 0,

        /// <summary>
        /// 标识子物体名称
        /// </summary>
        IDENTIFY_BY_NAME = 1,

        /// <summary>
        /// 标识子物体组件
        /// </summary>
        IDENTIFY_BY_COM = 2
    }

    /// <summary>
    /// 回放系统模式
    /// </summary>
    public enum ViewMode
    {
        /// <summary>
        /// 运行模式
        /// </summary>
        LIVE = 0,

        /// <summary>
        /// 录制模式
        /// </summary>
        REPLAY = 1
    }
}