using SsitEngine.Core;

namespace SsitEngine.Unity.WebRequest
{
    /// <summary>
    /// </summary>
    public enum ENRequestAssetType
    {
        EN_File = 1,
        EN_Audio = 10,
        EN_Audio_WAV = 11,

        EN_Audio_MP3 = 12,
        EN_Bundle = 20,
        EN_Texture = 30,

        EN_Text = 40
    }

    /// <summary>
    /// </summary>
    public enum EnWebRequestType
    {
        //TODO bendi 
        // gET
        EN_GET,
        EN_POST,

        EN_GETMEMORY
        /// yuancheng
        ///
        /// POST get
    }


    public interface IWebRequestInfo
    {
        /// <summary>
        ///     请求类型
        /// </summary>
        EnWebRequestType WebRequestType { get; set; }

        //string Url { get; set; }

        /// <summary>
        ///     是否断点续传
        /// </summary>
        bool IsBreakContinue { get; set; }

        /// <summary>
        ///     AssetBundle标识
        /// </summary>
        string BundleId { get; set; }


        string Url { get; set; }

        byte[] PostData { get; set; }

        string FileName { get; set; }

        string PostFilePath { get; set; }


        int Priority { get; set; }

        int Guid { get; set; }

        string Uuid { get; set; }

        /// <summary>
        ///     文件类型
        /// </summary>
        ENRequestAssetType FileType { get; set; }

        /// <summary>
        ///     进度回调
        /// </summary>
        SsitAction<float> RequestProcessAction { get; set; }

        /// <summary>
        ///     完成回调
        /// </summary>
        SsitAction<object> CompleteAction { get; set; }


        /// <summary>
        ///     取消回调
        /// </summary>
        SsitAction CancleAction { get; set; }

        /// <summary>
        ///     失败回调
        /// </summary>
        SsitAction<string> FailedAction { get; set; }
    }
}