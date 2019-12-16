/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：抽象引用对象                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/28 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Core.TaskPool
{
    /// <summary>
    ///     任务代理接口。
    /// </summary>
    /// <typeparam name="T">任务类型。</typeparam>
    public interface ITaskAgent<T> where T : ITask
    {
        /// <summary>
        ///     获取任务。
        /// </summary>
        T Task { get; }

        /// <summary>
        ///     初始化任务代理。
        /// </summary>
        void Initialize();

        /// <summary>
        ///     任务代理轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        void Update( float elapseSeconds, float realElapseSeconds );

        /// <summary>
        ///     关闭并清理任务代理。
        /// </summary>
        void Shutdown();

        /// <summary>
        ///     开始处理任务。
        /// </summary>
        /// <param name="task">要处理的任务。</param>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        void Start( T task, float elapseSeconds );

        /// <summary>
        ///     停止正在处理的任务并重置任务代理。
        /// </summary>
        void Reset();
    }
}