using System;
using System.Collections;
using SsitEngine.Core.TaskPool;
using UnityEngine;
using UnityEngine.Networking;

/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：Web 请求任务                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月10日                             
*└──────────────────────────────────────────────────────────────┘
*/
namespace SsitEngine.Unity.WebRequest
{
    /// <summary>
    ///     Web 请求任务。
    /// </summary>
    public class WebRequestTask : ITask
    {
        private static ulong s_Serial = 100;
        private readonly byte[] m_PostData;

        protected Coroutine m_coroutine;

        protected UnityWebRequest m_uwr;

        /// <summary>
        ///     初始化 Web 请求任务的新实例。
        /// </summary>
        /// <param name="webRequestUri">要发送的远程地址。</param>
        /// <param name="postData">要发送的数据流。</param>
        /// <param name="priority">Web 请求任务的优先级。</param>
        /// <param name="timeout">下载超时时长，以秒为单位。</param>
        /// <param name="userData">用户自定义数据。</param>
        public WebRequestTask( string webRequestUri, byte[] postData, int priority, float timeout,
            IWebRequestInfo userData )
        {
            Id = s_Serial++;
            Priority = priority;
            Done = false;
            Status = WebRequestTaskStatus.Todo;
            WebRequestUri = webRequestUri;
            m_PostData = postData;
            Timeout = timeout;
            UserData = userData;
        }


        public WebRequestTask( WebRequestInfo requestInfo )
        {
            Id = s_Serial++;
            UserData = requestInfo;
        }

        /// <summary>
        ///     获取或设置 Web 请求任务的状态。
        /// </summary>
        public WebRequestTaskStatus Status { get; set; }

        /// <summary>
        ///     获取要发送的远程地址。
        /// </summary>
        public string WebRequestUri { get; }

        /// <summary>
        ///     获取 Web 请求超时时长，以秒为单位。
        /// </summary>
        public float Timeout { get; }

        /// <summary>
        ///     获取用户自定义数据。
        /// </summary>
        public IWebRequestInfo UserData { get; private set; }

        /// <summary>
        ///     获取要发送的数据流。
        /// </summary>
        public byte[] GetPostData()
        {
            return m_PostData;
        }

        /// <summary>
        ///     请求处理
        /// </summary>
        public void Request()
        {
            m_coroutine = Engine.Instance.Platform.StartPlatCoroutine(DoRequest());
        }

        protected virtual IEnumerator DoRequest()
        {
            m_coroutine = null;
            yield return null;

            //            UnityWebRequest uwr = null;
            //
            //            switch (m_UserData.WebRequestType)
            //            {
            //                case EnWebRequestType.EN_GET:
            //                {
            //                    uwr = UnityWebRequest.Get();
            //                }
            //                    break;
            //                case EnWebRequestType.EN_POST:
            //                {
            //
            //                }
            //                    break;
            //            }
        }


        #region ITask

        public ulong Id { get; }

        /// <summary>
        ///     获取 Web 请求任务的优先级。
        /// </summary>
        public int Priority { get; }

        /// <summary>
        ///     获取或设置 Web 请求任务是否完成。
        /// </summary>
        public bool Done { get; set; }

        /// <summary>
        /// </summary>
        public virtual void Shutdown()
        {
            if (null != m_uwr)
            {
                try
                {
                    if (!m_uwr.isDone)
                    {
                        m_uwr.Abort();
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }

                m_uwr.Dispose();
            }
            if (m_coroutine != null)
            {
                Engine.Instance.Platform.StopPlatCoroutine(m_coroutine);
            }
            UserData = null;
        }

        #endregion
    }
}