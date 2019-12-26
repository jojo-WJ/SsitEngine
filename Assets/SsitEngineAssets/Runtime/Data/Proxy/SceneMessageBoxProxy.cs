using System.Collections.Generic;
using Framework.Data;
using SsitEngine.Unity.Utility;
using UnityEngine;

namespace Framework.Logic
{
    public class SceneMessageBoxProxy : SsitProxy
    {
        public new static string NAME = "SceneMessageBoxProxy";
        private readonly MessageBoxProxyInfo mData;


        public SceneMessageBoxProxy( MessageBoxProxyInfo data ) : base(NAME)
        {
            mData = data;
        }

        #region 子类实现

        public override void OnRegister()
        {
            base.OnRegister();
        }

        public override void OnRemove()
        {
            base.OnRemove();
        }

        #endregion

        #region 消息缓存

        public void AddMessage( MessageInfo info )
        {
            mData.AddMessageInfo(info);
        }

        public List<MessageInfo> GetAllMessageCache()
        {
            return mData.messageCache;
        }

        #endregion

        #region 回放写入

        public override string Guid
        {
            get => NAME;
            set { }
        }

        public override void InitEzReplay()
        {
            m_present = new GameObject(NAME);
            m_present.transform.SetParent(GlobalManager.Instance.transform);
        }

        public override SavedBase GeneralSaveData( bool isDeepClone = false )
        {
            var ret = mData;
            if (isDeepClone)
            {
                var temp = SerializationUtils.Clone(ret);
                ret.IsChange = false;
                return temp;
            }
            return ret;
        }

        public override void SynchronizeProperties( SavedBase savedState, bool isReset, bool isFristFrame )
        {
            var info = savedState as ArrowProxyInfo;

            if (info == null)
                return;

            //同步数据
            for (var i = 0; i < info.arrowCache.Count; i++)
            {
                var temp = info.arrowCache[i];

                if (i < info.cacheIndex)
                {
                    if (temp != null && temp.GetAgent()) temp.GetAgent().SetActive(true);
                }
                else
                {
                    if (temp != null && temp.GetAgent()) temp.GetAgent().SetActive(false);
                }
            }

            //todo:sync data
        }

        #endregion
    }
}