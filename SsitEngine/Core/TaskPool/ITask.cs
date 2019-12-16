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
    ///     任务接口。
    /// </summary>
    public interface ITask
    {
        /// <summary>
        ///     获取任务的序列编号。
        /// </summary>
        ulong Id { get; }

        /// <summary>
        ///     获取任务的优先级。
        /// </summary>
        int Priority { get; }

        /// <summary>
        ///     获取任务是否完成。
        /// </summary>
        bool Done { get; }

        /// <summary>
        ///     释放任务
        /// </summary>
        void Shutdown();
    }
}