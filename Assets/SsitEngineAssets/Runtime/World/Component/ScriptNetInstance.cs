/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/30 13:56:16                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework;
using Framework.Data;
using Mirror;
using UnityEngine;

namespace Framework.SceneObject.Component
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class ScriptNetInstance : NetworkBehaviour
    {
        [Tooltip("装置唯一id")] [SyncVar] public string guid;

        public bool isAutoRegister = true;

        [Header("底层连接")] [Disable] public bool isLink;

        [Tooltip("装置的配置道具id")] [SyncVar] public int itemId;

        public EnObjectType mType;

        [Tooltip("装置编号")] public string resNumber;


        public override void OnStartClient()
        {
            if (isAutoRegister)
                OnPostRegister();
        }

        public void SetLink( bool b )
        {
            isLink = b;
        }

        public void OnPostRegister()
        {
            //注册对象实例
            if (itemId == 0)
                SsitApplication.Instance.CreatePlayer(guid, null, new PlayerAttribute(), null, null, gameObject);
            else
                SsitApplication.Instance.CreatePlayer(guid, itemId, null, null, gameObject);
        }
    }
}