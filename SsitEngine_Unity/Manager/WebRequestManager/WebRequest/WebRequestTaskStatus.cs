/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：Web 请求任务的状态                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月10日                             
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.WebRequest
{
    /// <summary>
    ///     Web 请求任务的状态。
    /// </summary>
    public enum WebRequestTaskStatus
    {
        /// <summary>
        ///     准备请求。
        /// </summary>
        Todo,

        /// <summary>
        ///     请求中。
        /// </summary>
        Doing,

        /// <summary>
        ///     请求完成。
        /// </summary>
        Done,

        /// <summary>
        ///     请求错误。
        /// </summary>
        Error
    }
}