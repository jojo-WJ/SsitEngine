using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace SsitEngine.Unity.WebRequest.Task
{
    public class AudioWebTask : WebRequestTask
    {
        public AudioWebTask( string webRequestUri, byte[] postData, int priority, float timeout,
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
            m_uwr = UnityWebRequestMultimedia.GetAudioClip(UserData.Url, GetAudioType());
            m_uwr.SendWebRequest();

            while (!m_uwr.isDone)
            {
                if (m_uwr.isHttpError || m_uwr.isNetworkError ||
                    !string.IsNullOrEmpty(m_uwr.GetResponseHeader("errmsg")))
                {
                    var msg = m_uwr.error;
                    if (string.IsNullOrEmpty(msg)) msg = m_uwr.GetResponseHeader("errmsg");

                    UserData.FailedAction?.Invoke(msg);
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
                if (m_uwr.responseCode != 200)
                {
                    var msg = m_uwr.error;
                    UserData.FailedAction?.Invoke(msg);
                    Done = true;
                    yield break;
                }

                UserData.RequestProcessAction?.Invoke(1);
                //Texture texture = (m_uwr.downloadHandler as DownloadHandlerTexture).texture;
                //if (texture == null)
                //{
                //    UserData.FailedAction?.Invoke("err");
                //}
                //else
                //{
                UserData.CompleteAction?.Invoke(DownloadHandlerAudioClip.GetContent(m_uwr));
                Done = true;
                //}
            }
        }


        private IEnumerator Post()
        {
            var form = new WWWForm();
            form.AddBinaryData("file", UserData.PostData, UserData.FileName);
            form.AddField("messageId", UserData.PostFilePath);
            form.AddField("uuid", UserData.Guid);

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
                        if (UserData == null)
                        {
                            Done = true;
                            yield break;
                        }
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

        private AudioType GetAudioType()
        {
            switch (UserData.FileType)
            {
                case ENRequestAssetType.EN_Audio_MP3:
                {
                    return AudioType.MPEG;
                }
                case ENRequestAssetType.EN_Audio_WAV:
                {
                    return AudioType.WAV;
                }
                default:
                {
                    return AudioType.MPEG;
                }
            }
        }
    }
}