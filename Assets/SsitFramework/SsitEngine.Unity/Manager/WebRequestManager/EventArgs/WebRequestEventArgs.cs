using SsitEngine.Core;

namespace SsitEngine.Unity.WebRequest
{
    /// <summary>
    ///     Web 请求开始事件。
    /// </summary>
    public sealed class WebRequestStartEventArgs : SsitEventArgs
    {
        /// <summary>
        ///     初始化 Web 请求开始事件的新实例。
        /// </summary>
        /// <param name="serialId">Web 请求任务的序列编号。</param>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="userData">用户自定义数据。</param>
        public WebRequestStartEventArgs( ulong serialId, string webRequestUri, object userData )
        {
            SerialId = serialId;
            WebRequestUri = webRequestUri;
            UserData = userData;
        }

        /// <summary>
        ///     获取 Web 请求任务的序列编号。
        /// </summary>
        public ulong SerialId { get; }

        /// <summary>
        ///     获取 Web 请求地址。
        /// </summary>
        public string WebRequestUri { get; }

        /// <summary>
        ///     获取用户自定义数据。
        /// </summary>
        public object UserData { get; }
    }

    /// <summary>
    ///     Web 请求成功事件。
    /// </summary>
    public sealed class WebRequestSuccessEventArgs : SsitEventArgs
    {
        private readonly byte[] m_WebResponseBytes;

        /// <summary>
        ///     初始化 Web 请求成功事件的新实例。
        /// </summary>
        /// <param name="serialId">Web 请求任务的序列编号。</param>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="webResponseBytes">Web 响应的数据流。</param>
        /// <param name="userData">用户自定义数据。</param>
        public WebRequestSuccessEventArgs( ulong serialId, string webRequestUri, byte[] webResponseBytes,
            object userData )
        {
            SerialId = serialId;
            WebRequestUri = webRequestUri;
            m_WebResponseBytes = webResponseBytes;
            UserData = userData;
        }

        /// <summary>
        ///     获取 Web 请求任务的序列编号。
        /// </summary>
        public ulong SerialId { get; }

        /// <summary>
        ///     获取 Web 请求地址。
        /// </summary>
        public string WebRequestUri { get; }

        /// <summary>
        ///     获取用户自定义数据。
        /// </summary>
        public object UserData { get; }

        /// <summary>
        ///     获取 Web 响应的数据流。
        /// </summary>
        /// <returns>Web 响应的数据流。</returns>
        public byte[] GetWebResponseBytes()
        {
            return m_WebResponseBytes;
        }
    }

    /// <summary>
    ///     Web 请求失败事件。
    /// </summary>
    public sealed class WebRequestFailureEventArgs : SsitEventArgs
    {
        /// <summary>
        ///     初始化 Web 请求失败事件的新实例。
        /// </summary>
        /// <param name="serialId">Web 请求任务的序列编号。</param>
        /// <param name="webRequestUri">Web 请求地址。</param>
        /// <param name="errorMessage">错误信息。</param>
        /// <param name="userData">用户自定义数据。</param>
        public WebRequestFailureEventArgs( ulong serialId, string webRequestUri, string errorMessage, object userData )
        {
            SerialId = serialId;
            WebRequestUri = webRequestUri;
            ErrorMessage = errorMessage;
            UserData = userData;
        }

        /// <summary>
        ///     获取 Web 请求任务的序列编号。
        /// </summary>
        public ulong SerialId { get; }

        /// <summary>
        ///     获取 Web 请求地址。
        /// </summary>
        public string WebRequestUri { get; }

        /// <summary>
        ///     获取错误信息。
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        ///     获取用户自定义数据。
        /// </summary>
        public object UserData { get; }
    }
}