namespace SsitEngine
{
    /// <summary>
    ///     程序框架模块接口
    /// </summary>
    public interface IModule
    {
        /// <summary>
        ///     模块名称
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        ///     获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        int Priority { get; }

        /// <summary>
        ///     游戏框架模块轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        void OnUpdate( float elapseSeconds );

        /// <summary>
        ///     关闭并清理游戏框架模块。
        /// </summary>
        void Shutdown();
    }
}