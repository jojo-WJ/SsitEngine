using System.Collections.Generic;
using Framework.Data;
using SsitEngine.Unity.Utility;
using UnityEngine;

namespace Framework.Logic
{
    public class SceneMessageProxy : SsitProxy
    {
        public new static string NAME = "SceneMessageBoxProxy";
        private readonly MessageBoxProxyInfo mData;


        public SceneMessageProxy( MessageBoxProxyInfo data ) : base(NAME)
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
            var info = savedState as MessageBoxProxyInfo;

            if (info == null)
                return;
            if (isFristFrame)
//                Facade.Instance.SendNotification((ushort)UIManoeuvreChatFormEvent.RefreshMessageBox, 0);
                //todo:sync data
                SynchronizeMessage(info, isReset);
        }

        private void SynchronizeMessage( MessageBoxProxyInfo info, bool isReset )
        {
            if (mData.cacheIndex != info.cacheIndex)
            {
                var pre = mData.cacheIndex;
                mData.cacheIndex = info.cacheIndex;
                var cur = mData.cacheIndex;

                if (pre > cur || isReset)
                    return;

                var begin = pre < cur ? pre : cur;
                var end = pre > cur ? pre : cur;
                //Debug.Log($"expression{begin}{end}");
//                for (int i = begin; i < end; i++)
//                {
//                    var message = mData.messageCache[i];
//
//                    switch (message.MessageType)
//                    {
//                        case EnMessageType.ACTION:
//                        case EnMessageType.SYSTEM:
//                            PopTipHelper.PopTip(message,false);
//                            break;
//                    }
//                }

//                Facade.Instance.SendNotification((ushort)UIManoeuvreChatFormEvent.RefreshMessageBox, info.cacheIndex);
            }
        }

        #endregion
    }
}