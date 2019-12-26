using SsitEngine.PureMVC.Patterns;
using UnityEngine;
using UnityEngine.Events;

namespace SsitEngine.Unity
{
    /// <summary>
    /// 框架入口
    /// </summary>
    public class SsitMain : MonoBehaviour
    {
        /// <summary>
        /// 程序初始化加载
        /// </summary>
        private static bool mIsInit;

        /// <summary>
        /// 程序加载完成回调
        /// </summary>
        private static UnityAction onInitFinished;

        /// <summary>
        /// 调试开关
        /// </summary>
        public bool mDebug;

        private void Awake()
        {
            if (mIsInit)
            {
                OnStart();
                onInitFinished = null;
            }
            else
            {
                onInitFinished = OnStart;
            }
        }

        /// <summary>
        /// Start is called before the first frame update
        /// </summary>
        protected virtual void OnStart()
        {
            Facade.Instance.SendNotification((ushort) EnEngineEvent.OnApplicationStart);
            onInitFinished = null;
        }

        [RuntimeInitializeOnLoadMethod]
        protected static void InitInternalAsset()
        {
            //加载内部资源配置
            var asset = Resources.Load<InternalAssetConfig>("InternalAssetConfig");
            if (asset != null)
            {
                FileUtils.CopyFolderFormStreamToPersitentPath(asset.mStreamingAsset, () =>
                {
                    mIsInit = true;
                    onInitFinished?.Invoke();
                    Debug.Log("CopyFolderFormStreamToPersitentPath is Finished");
                });
            }
        }
    }
}