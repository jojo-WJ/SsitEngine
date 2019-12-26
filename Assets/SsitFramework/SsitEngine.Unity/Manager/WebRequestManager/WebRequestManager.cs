using System;
using SsitEngine.Core.TaskPool;
using SsitEngine.DebugLog;
using SsitEngine.Unity.WebRequest.Task;

namespace SsitEngine.Unity.WebRequest
{
    /// <summary>
    ///     Web 请求管理器。
    /// </summary>
    internal sealed class WebRequestManager : ManagerBase<WebRequestManager>, IWebRequestManager
    {
        private readonly TaskPool<WebRequestTask> m_TaskPool;
        private EventHandler<WebRequestFailureEventArgs> m_WebRequestFailureEventHandler;

        //事件备用
        private EventHandler<WebRequestStartEventArgs> m_WebRequestStartEventHandler;
        private EventHandler<WebRequestSuccessEventArgs> m_WebRequestSuccessEventHandler;

        /// <summary>
        ///     初始化 Web 请求管理器的新实例。
        /// </summary>
        public WebRequestManager()
        {
            m_TaskPool = new TaskPool<WebRequestTask>();
            Timeout = 30f;
        }

        /// <summary>
        ///     获取 Web 请求代理总数量。
        /// </summary>
        public int TotalAgentCount => m_TaskPool.TotalAgentCount;

        /// <summary>
        ///     获取可用 Web 请求代理数量。
        /// </summary>
        public int FreeAgentCount => m_TaskPool.FreeAgentCount;

        /// <summary>
        ///     获取工作中 Web 请求代理数量。
        /// </summary>
        public int WorkingAgentCount => m_TaskPool.WorkingAgentCount;

        /// <summary>
        ///     获取等待 Web 请求数量。
        /// </summary>
        public int WaitingTaskCount => m_TaskPool.WaitingTaskCount;

        /// <summary>
        ///     获取或设置 Web 请求超时时长，以秒为单位。
        /// </summary>
        public float Timeout { get; set; }

        /// <summary>
        ///     Web 请求开始事件。
        /// </summary>
        public event EventHandler<WebRequestStartEventArgs> WebRequestStart
        {
            add => m_WebRequestStartEventHandler += value;
            remove => m_WebRequestStartEventHandler -= value;
        }

        /// <summary>
        ///     Web 请求成功事件。
        /// </summary>
        public event EventHandler<WebRequestSuccessEventArgs> WebRequestSuccess
        {
            add => m_WebRequestSuccessEventHandler += value;
            remove => m_WebRequestSuccessEventHandler -= value;
        }

        /// <summary>
        ///     Web 请求失败事件。
        /// </summary>
        public event EventHandler<WebRequestFailureEventArgs> WebRequestFailure
        {
            add => m_WebRequestFailureEventHandler += value;
            remove => m_WebRequestFailureEventHandler -= value;
        }

        /// <summary>
        ///     增加 Web 请求代理辅助器。
        /// </summary>
        public void AddWebRequestAgent()
        {
            var agent = new WebRequestAgent();
            m_TaskPool.AddAgent(agent);
        }


        /// <summary>
        ///     增加 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="postData">要发送的数据流。</param>
        /// <returns>新增 Web 请求任务的序列编号。</returns>
        public ulong AddWebRequest( string webRequestUri, byte[] postData )
        {
            return AddWebRequest(webRequestUri, postData, SsitFrameUtils.DefaultPriority, null);
        }

        /// <summary>
        ///     增加 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="priority">Web 请求任务的优先级。</param>
        /// <returns>新增 Web 请求任务的序列编号。</returns>
        public ulong AddWebRequest( string webRequestUri, int priority )
        {
            return AddWebRequest(webRequestUri, null, priority, null);
        }

        /// <summary>
        ///     增加 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>新增 Web 请求任务的序列编号。</returns>
        public ulong AddWebRequest( string webRequestUri, IWebRequestInfo userData )
        {
            return AddWebRequest(webRequestUri, null, SsitFrameUtils.DefaultPriority, userData);
        }

        /// <summary>
        ///     增加 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="postData">要发送的数据流。</param>
        /// <param name="priority">Web 请求任务的优先级。</param>
        /// <returns>新增 Web 请求任务的序列编号。</returns>
        public ulong AddWebRequest( string webRequestUri, byte[] postData, int priority )
        {
            return AddWebRequest(webRequestUri, postData, priority, null);
        }

        /// <summary>
        ///     增加 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="postData">要发送的数据流。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>新增 Web 请求任务的序列编号。</returns>
        public ulong AddWebRequest( string webRequestUri, byte[] postData, IWebRequestInfo userData )
        {
            return AddWebRequest(webRequestUri, postData, SsitFrameUtils.DefaultPriority, userData);
        }

        /// <summary>
        ///     增加 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="priority">Web 请求任务的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>新增 Web 请求任务的序列编号。</returns>
        public ulong AddWebRequest( string webRequestUri, int priority, IWebRequestInfo userData )
        {
            return AddWebRequest(webRequestUri, null, priority, userData);
        }

        /// <summary>
        ///     增加 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="postData">要发送的数据流。</param>
        /// <param name="priority">Web 请求任务的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>新增 Web 请求任务的序列编号。</returns>
        public ulong AddWebRequest( string webRequestUri, byte[] postData, int priority, IWebRequestInfo userData )
        {
            if (string.IsNullOrEmpty(webRequestUri))
            {
                throw new SsitEngineException("Web request uri is invalid.");
            }

            WebRequestTask webRequestTask = null;
            switch (userData.FileType)
            {
                case ENRequestAssetType.EN_Text:
                {
                    webRequestTask = new TextWebTask(webRequestUri, postData, priority, Timeout, userData);
                }
                    break;
                case ENRequestAssetType.EN_Audio:
                case ENRequestAssetType.EN_Audio_MP3:
                case ENRequestAssetType.EN_Audio_WAV:
                {
                    webRequestTask = new AudioWebTask(webRequestUri, postData, priority, Timeout, userData);
                }
                    break;
                case ENRequestAssetType.EN_Texture:
                {
                    webRequestTask = new TextureWebTask(webRequestUri, postData, priority, Timeout, userData);
                }
                    break;
                case ENRequestAssetType.EN_File:
                {
                    webRequestTask = new FileWebTask(webRequestUri, postData, priority, Timeout, userData);
                }
                    break;
                case ENRequestAssetType.EN_Bundle:
                {
                    webRequestTask = new BundleWebTask(webRequestUri, postData, priority, Timeout, userData);
                }
                    break;
            }

            if (webRequestTask == null)
            {
                throw new Exception("找不到对应的资源类型任务代理");
            }
            m_TaskPool.AddTask(webRequestTask);

            // 计时器不等同于可以等待的机制处理，他需要即时性的一些概念
            if (WaitingTaskCount > FreeAgentCount)
            {
                if (TotalAgentCount > Engine.Instance.PlatConfig.webTaskAgentMaxCount)
                {
                    SsitDebug.Warning("计时器代理超出平台配置的最大个数。。。");
                }
                else
                {
                    AddWebRequestAgent();
                }
            }

            return webRequestTask.Id;
        }

        /// <summary>
        ///     移除 Web 请求任务。
        /// </summary>
        /// <param name="serialId">要移除 Web 请求任务的序列编号。</param>
        /// <returns>是否移除 Web 请求任务成功。</returns>
        public bool RemoveWebRequest( ulong serialId )
        {
            return m_TaskPool.RemoveTask(serialId) != null;
        }

        /// <summary>
        ///     移除所有 Web 请求任务。
        /// </summary>
        public void RemoveAllWebRequests()
        {
            m_TaskPool.RemoveAllTasks();
        }


        public ulong AddWebRequest( IWebRequestInfo webRequestInfo )
        {
            return AddWebRequest(webRequestInfo.Url, webRequestInfo.PostData, webRequestInfo.Priority, webRequestInfo);
        }

        /// <summary>
        ///     增加 Web 请求任务。
        /// </summary>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="postData">要发送的数据流。</param>
        /// <param name="priority">Web 请求任务的优先级。</param>
        /// <param name="task">任务。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>新增 Web 请求任务的序列编号。</returns>
        public ulong AddWebRequest( string webRequestUri, byte[] postData, int priority, WebRequestTask task,
            IWebRequestInfo userData )
        {
            if (string.IsNullOrEmpty(webRequestUri))
            {
                throw new SsitEngineException("Web request uri is invalid.");
            }

            m_TaskPool.AddTask(task);

            // 计时器不等同于可以等待的机制处理，他需要即时性的一些概念
            if (WaitingTaskCount > FreeAgentCount)
            {
                if (TotalAgentCount > Engine.Instance.PlatConfig.webTaskAgentMaxCount)
                {
                    SsitDebug.Warning("计时器代理超出平台配置的最大个数。。。");
                }
                else
                {
                    AddWebRequestAgent();
                }
            }

            return task.Id;
        }

        private void OnWebRequestAgentStart( WebRequestAgent sender )
        {
            if (m_WebRequestStartEventHandler != null)
            {
                m_WebRequestStartEventHandler(this,
                    new WebRequestStartEventArgs(sender.Task.Id, sender.Task.WebRequestUri, sender.Task.UserData));
            }
        }

        private void OnWebRequestAgentSuccess( WebRequestAgent sender, byte[] webResponseBytes )
        {
            if (m_WebRequestSuccessEventHandler != null)
            {
                m_WebRequestSuccessEventHandler(this,
                    new WebRequestSuccessEventArgs(sender.Task.Id, sender.Task.WebRequestUri, webResponseBytes,
                        sender.Task.UserData));
            }
        }

        private void OnWebRequestAgentFailure( WebRequestAgent sender, string errorMessage )
        {
            if (m_WebRequestFailureEventHandler != null)
            {
                m_WebRequestFailureEventHandler(this,
                    new WebRequestFailureEventArgs(sender.Task.Id, sender.Task.WebRequestUri, errorMessage,
                        sender.Task.UserData));
            }
        }

        #region Moudle

        public override string ModuleName => typeof(WebRequestManager).FullName;

        public override int Priority => (int) EnModuleType.ENMODULEDEFAULT;

        public override void OnUpdate( float elapseSeconds )
        {
            m_TaskPool.Update(elapseSeconds, 0);
        }

        /// <summary>
        ///     关闭并清理 Web 请求管理器。
        /// </summary>
        public override void Shutdown()
        {
            m_TaskPool.Shutdown();
        }

        #endregion
    }
}