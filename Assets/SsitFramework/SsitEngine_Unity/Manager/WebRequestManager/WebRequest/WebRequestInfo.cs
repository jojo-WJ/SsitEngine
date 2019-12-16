/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：WebRequestInfo                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月10日                             
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Core;

namespace SsitEngine.Unity.WebRequest
{
    /// <summary>
    ///     网络请求信息
    /// </summary>
    public class WebRequestInfo : IWebRequestInfo
    {
        public string FileName { get; set; }

        public ENRequestAssetType FileType { get; set; }

        public SsitAction<object> CompleteAction { get; set; }
        public SsitAction<string> FailedAction { get; set; }

        public SsitAction CancleAction { get; set; }

        public SsitAction<float> RequestProcessAction { get; set; }

        public EnWebRequestType WebRequestType { get; set; }
        public bool IsBreakContinue { get; set; }
        public string BundleId { get; set; }
        public string Url { get; set; }
        public byte[] PostData { get; set; }
        public string PostFilePath { get; set; }
        public int Priority { get; set; }
        public int Guid { get; set; }
        public string Uuid { get; set; }
    }
}