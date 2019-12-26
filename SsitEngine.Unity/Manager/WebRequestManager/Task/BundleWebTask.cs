using System.Collections;
using System.IO;
using SsitEngine.DebugLog;
using UnityEngine;
using UnityEngine.Networking;

namespace SsitEngine.Unity.WebRequest.Task
{
    public class BundleWebTask : WebRequestTask
    {
        public BundleWebTask( string webRequestUri, byte[] postData, int priority, float timeout,
            IWebRequestInfo userData ) :
            base(webRequestUri, postData, priority, timeout, userData)
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
            yield return LoadBundleFromServer();
        }

        private IEnumerator Post()
        {
            yield return ValidNewVersionBundle();
        }

        private IEnumerator LoadBundleFromServer()
        {
            using (m_uwr = UnityWebRequest.Get(UserData.Url))
            {
                m_uwr.SendWebRequest();
                if (m_uwr.isHttpError || m_uwr.isNetworkError)
                {
                    Debug.LogError(m_uwr.error);
                    yield break;
                }

                var errmsg = m_uwr.GetResponseHeader("errmsg");
                if (!string.IsNullOrEmpty(errmsg))
                {
                    Debug.LogError(errmsg);
                    yield break;
                }

                while (!m_uwr.isDone)
                {
                    if (UserData == null)
                    {
                        Done = true;
                        yield break;
                    }
                    UserData.RequestProcessAction?.Invoke(m_uwr.downloadProgress);
                    yield return 1;
                }


                if (m_uwr.isDone)
                {
                    if (UserData == null)
                    {
                        Done = true;
                        yield break;
                    }
                    UserData.RequestProcessAction?.Invoke(1.0f);

                    var srvMd5 = m_uwr.GetResponseHeader("md5");
                    var disposition = m_uwr.GetResponseHeader("Content-disposition");
                    var tmpFileName = disposition.Split('=');
                    var fileName = tmpFileName[tmpFileName.Length - 1].Trim('"');
                    var path =
                        FileUtils.CreateFile(fileName, m_uwr.downloadHandler.data, m_uwr.downloadHandler.data.Length);

                    //    // 生成MD5码
                    var curMd5 = FileUtils.BuildMD5ToFile(path);
                    //    // 校验
                    if (srvMd5.ToLower() == curMd5.ToLower())
                        //        //成功
                        //        //通知服务器删除文件
                        //        //完成回调
                    {
                        UserData.CompleteAction?.Invoke(path);
                    }
                    //yield return LoadBundleFinish();
                    else
                        //        //失败
                    {
                        Debug.LogError("md5 failed,资源异常，请重新下载");
                    }
                    //        //下载数据错误
                }
            }
        }


        private IEnumerator ValidNewVersionBundle()
        {
            var filePath = Application.persistentDataPath + "/File.txt";
            if (!File.Exists(filePath))
            {
                yield break;
            }

            var strs = File.ReadAllLines(filePath);
            var ver = strs[0].Split('|')[0];

            var form = new WWWForm();
            form.AddField("version", ver);
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                {
                    form.AddField("terminal", "ANDROID");
                }
                    break;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                {
                    form.AddField("terminal", "PC");
                }
                    break;
            }

            form.AddBinaryData("file", File.ReadAllBytes(filePath), FileUtils.GetFileNameFromPath(filePath));

            using (m_uwr = UnityWebRequest.Post(UserData.Url, form))
            {
                m_uwr.SendWebRequest();
                if (m_uwr.isHttpError || m_uwr.isNetworkError)
                {
                    Debug.LogError(m_uwr.error);
                    yield break;
                }

                while (!m_uwr.isDone)
                {
                    if (UserData == null)
                    {
                        Done = true;
                        yield break;
                    }
                    if (UserData.RequestProcessAction != null)
                    {
                        UserData.RequestProcessAction.Invoke(m_uwr.downloadProgress);
                    }

                    yield return 1;
                }

                if (m_uwr.isDone)
                {
                    if (UserData == null)
                    {
                        Done = true;
                        yield break;
                    }
                    if (UserData.RequestProcessAction != null)
                    {
                        UserData.RequestProcessAction.Invoke(1.0f);
                    }

                    var result = JsonUtility.FromJson<ValidBundleResult>(m_uwr.downloadHandler.text);
                    if (result == null)
                    {
                        SsitDebug.Error("服务器校验返回异常");
                        yield break;
                    }

                    if (result.code == 200)
                    {
                        if (UserData.CompleteAction != null)
                        {
                            UserData.CompleteAction.Invoke(result);
                        }

                        // 通知服务器下载完成
                        if (!string.IsNullOrEmpty(result.data.bundleUuid))
                        {
                            Engine.Instance.Platform.StartPlatCoroutine(LoadBundleFinish(result.data.bundleUuid));
                        }
                    }
                    else
                    {
                        Debug.LogError(m_uwr.downloadHandler.text);

                        if (UserData.FailedAction != null)
                        {
                            UserData.FailedAction.Invoke(result.message);
                        }

                        //errorCallBack?.Invoke();
                    }
                }
            }
        }

        public static IEnumerator LoadBundleFinish( string uuid )
        {
            //using (UnityWebRequest m_uwr = UnityWebRequest.Get("http://" + LocalConfig.Instance.IP + ":8900/" +
            //                                                 HttpNetWorkAction.Download_Bundle_Finish + "?bundleUuid=" +
            //                                                 uuid))
            //{
            //    yield return m_uwr.SendWebRequest();
            //    if (m_uwr.isHttpError || m_uwr.isNetworkError)
            //    {
            //        yield break;
            //    }


            //}

            yield return 0;
        }
    }
}