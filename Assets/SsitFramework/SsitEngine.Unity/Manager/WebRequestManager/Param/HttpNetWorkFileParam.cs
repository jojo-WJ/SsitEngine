using System;
using System.Collections.Generic;

namespace SsitEngine.Unity.WebRequest
{
    public class HttpNetWorkAction
    {
        public static readonly string Update_Valid = "update/valid"; //检查新版本的bundle
        public static readonly string Download_Bundle = "update/download/bundle"; //下载bundle
        public static readonly string Upload_Bundle = "update/upload/bundle"; //上传bundle
        public static readonly string Download_Bundle_Finish = "update/download/bundle/finish"; //下载bundle完成回调
        public static readonly string Upload_File = "file/upload"; //上传文件
        public static readonly string Download_File = "file/download/"; //下载文件
    }

    [Serializable]
    public class HttpResponseResult
    {
        public int code;
        public string message;

        public HttpResponseResult()
        {
        }

        public HttpResponseResult( int cc, string msg )
        {
            code = cc;
            message = msg;
        }
    }

    [Serializable]
    public class ValidBundleResult : HttpResponseResult
    {
        public ValidBundleData data;

        public ValidBundleResult()
        {
        }

        public ValidBundleResult( int code, string message, ValidBundleData data ) : base(code, message)
        {
            this.data = data;
        }
    }

    [Serializable]
    public class ValidBundleData
    {
        public string bundleUuid;
        public List<string> delBundleList;
        public string version;

        public ValidBundleData()
        {
        }

        public ValidBundleData( string bundle, List<string> list, string version )
        {
            bundleUuid = bundle;
            delBundleList = list;
            this.version = version;
        }
    }

    /// <summary>
    ///     有效上传返回数据
    /// </summary>
    [Serializable]
    public class ValidUploadResult : HttpResponseResult
    {
        public string data;

        public ValidUploadResult()
        {
        }

        public ValidUploadResult( int code, string message, string data ) : base(code, message)
        {
            this.data = data;
        }
    }
}