using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace SsitEngine.Unity.WebRequest.Task
{
    /// <summary>
    ///     文本下载任务
    /// </summary>
    public class TextWebTask : WebRequestTask
    {
        public TextWebTask( string webRequestUri, byte[] postData, int priority, float timeout,
            IWebRequestInfo userData ) : base(webRequestUri, postData, priority, timeout, userData)
        {
        }

        protected override IEnumerator DoRequest()
        {
            switch (UserData.WebRequestType)
            {
                case EnWebRequestType.EN_GET:
                {
                    yield return Get();
                }
                    break;
                case EnWebRequestType.EN_GETMEMORY:
                {
                    yield return GetFromMemory();
                }
                    break;
                case EnWebRequestType.EN_POST:
                {
                    yield return Post();
                    break;
                }
            }


            m_coroutine = null;

            yield return null;
        }


        private IEnumerator Get()
        {
            m_uwr = UnityWebRequest.Get(UserData.Url);
            //存储文件的路径名downloaderhandle 会自动下载到存储路径like：Application.persistentDataPath+"/unity3d.html"。
            //m_uwr.downloadHandler = new DownloadHandlerFile(UserData.FileName);
            m_uwr.SendWebRequest();

            while (!m_uwr.downloadHandler.isDone)
            {
                if (m_uwr.isHttpError || m_uwr.isNetworkError ||
                    !string.IsNullOrEmpty(m_uwr.GetResponseHeader("errmsg")))
                {
                    var msg = m_uwr.error;
                    if (string.IsNullOrEmpty(msg)) msg = m_uwr.GetResponseHeader("errmsg");

                    UserData?.FailedAction?.Invoke(msg);
                    Done = true;
                    yield break;
                }

                UserData?.RequestProcessAction?.Invoke(m_uwr.downloadProgress);

                yield return 1;
            }

            if (m_uwr.downloadHandler.isDone)
            {
                if (UserData == null)
                {
                    Done = true;
                    yield break;
                }

                if (m_uwr.responseCode != 200)
                {
                    var msg = m_uwr.error;
                    UserData?.FailedAction?.Invoke(msg);
                    Done = true;
                    yield break;
                }

                UserData.RequestProcessAction?.Invoke(1);

                //DownloadHandler dht = m_uwr.downloadHandler as DownloadHandle;
                if (m_uwr.downloadHandler.data.Length == 0)
                {
                    UserData?.FailedAction?.Invoke("服务器下载器异常");
                }
                else
                {
                    var path = m_uwr.GetResponseHeader("path");
                    var str = FileUtils.CreateFile(path, m_uwr.downloadHandler.data, m_uwr.downloadHandler.data.Length,
                        Application.persistentDataPath);
                    if (string.IsNullOrEmpty(str))
                    {
                        UserData.FailedAction?.Invoke("本地文件存储失败：" + Application.persistentDataPath + "/" + path);
                    }
                    else
                    {
                        if (m_uwr.downloadHandler.data.Length == 0)
                        {
                            UserData.FailedAction?.Invoke("err data length is zero");
                            Done = true;
                        }
                        else
                        {
                            if (UserData.FileName == "byte")
                                UserData.CompleteAction?.Invoke(m_uwr.downloadHandler.data);
                            else
                                UserData.CompleteAction?.Invoke(m_uwr.downloadHandler.text);
                            Done = true;
                        }
                    }
                }
                Done = true;
            }
        }

        private IEnumerator Post()
        {
            var form = new WWWForm();
            form.AddBinaryData("file", UserData.PostData, UserData.FileName);
            form.AddField("receivePath", UserData.PostFilePath);

            using (m_uwr = UnityWebRequest.Post(UserData.Url, form))
            {
                m_uwr.SetRequestHeader("Content-Type", "multipart/form-data");
                m_uwr.chunkedTransfer = true;
                m_uwr.timeout = 30;

                m_uwr.SendWebRequest();

                if (m_uwr.isNetworkError || m_uwr.isHttpError)
                {
                    UserData.FailedAction(m_uwr.error + "Asset path :: " + UserData.FileName);
                    yield break;
                }
                if (UserData.RequestProcessAction != null)
                    while (!m_uwr.isDone)
                    {
                        UserData.RequestProcessAction.Invoke(m_uwr.uploadProgress);
                        yield return 1;
                    }

                if (m_uwr.isDone)
                {
                    if (UserData == null)
                    {
                        Done = true;
                        yield break;
                    }
                    UserData.RequestProcessAction?.Invoke(1f);
                    if (UserData.CompleteAction != null) UserData.CompleteAction.Invoke(null);
                }
            }
        }

        /// <summary>
        ///     加载本地文本文件
        /// </summary>
        /// <returns></returns>
        private IEnumerator GetFromMemory()
        {
            m_uwr = UnityWebRequest.Get($"{PathUtils.GetWWWFileHead()}{UserData.Url}");
            m_uwr.SendWebRequest();
            if (m_uwr.isNetworkError || m_uwr.isHttpError)
            {
                UserData.FailedAction?.Invoke(m_uwr.error);
                Done = true;
                yield break;
            }
            while (!m_uwr.isDone)
            {
                UserData.RequestProcessAction?.Invoke(m_uwr.downloadProgress);
                yield return 0;
            }
            //Debug.Log($"expression{m_uwr.url}");
            if (m_uwr.isDone)
            {
                if (UserData == null)
                {
                    Done = true;
                    yield break;
                }
                UserData.RequestProcessAction?.Invoke(1.0f);
                if (UserData.FileName == "byte")
                    UserData.CompleteAction?.Invoke(m_uwr.downloadHandler.data);
                else
                    UserData.CompleteAction?.Invoke(m_uwr.downloadHandler.text);
            }
            Done = true;
        }
    }
}