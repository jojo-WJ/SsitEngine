/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：Web 请求代理                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月10日                             
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Core.TaskPool;

namespace SsitEngine.Unity.WebRequest
{
    /// <summary>
    ///     Web 请求代理。
    /// </summary>
    public sealed class WebRequestAgent : ITaskAgent<WebRequestTask>
    {
        /// <summary>
        ///     初始化 Web 请求代理的新实例。
        /// </summary>
        /// <param name="webRequestAgentHelper">Web 请求代理辅助器。</param>
        public WebRequestAgent()
        {
            Task = null;
            WaitTime = 0f;
        }

        /// <summary>
        ///     获取已经等待时间。
        /// </summary>
        public float WaitTime { get; private set; }

        /// <summary>
        ///     获取 Web 请求任务。
        /// </summary>
        public WebRequestTask Task { get; private set; }

        /// <summary>
        ///     初始化 Web 请求代理。
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        ///     Web 请求代理轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update( float elapseSeconds, float realElapseSeconds )
        {
            if (Task.Status == WebRequestTaskStatus.Doing)
            {
                WaitTime += elapseSeconds;
                if (WaitTime >= Task.Timeout)
                {
                    //todo：超时处理
                }
            }
        }

        /// <summary>
        ///     关闭并清理 Web 请求代理。
        /// </summary>
        public void Shutdown()
        {
            Reset();
            Task = null;
        }

        /// <summary>
        ///     开始处理 Web 请求任务。
        /// </summary>
        /// <param name="task">要处理的 Web 请求任务。</param>
        public void Start( WebRequestTask task, float elapseSeconds )
        {
            if (task == null) throw new SsitEngineException();

            Task = task;
            Task.Status = WebRequestTaskStatus.Doing;

            // IEnumrator
            Task.Request();
            WaitTime = 0f;
        }

        /// <summary>
        ///     重置 Web 请求代理。
        /// </summary>
        public void Reset()
        {
            Task?.Shutdown();
            //m_Task = null;
            WaitTime = 0f;
        }
    }
}