/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/29 12:15:08                     
*└──────────────────────────────────────────────────────────────┘
*/

using DG.Tweening;
using Framework;
using Framework.SceneObject;
using SsitEngine.Unity.Avatar;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace SsitEngine.Unity.Action
{
    public class ActionPick : ActionBase
    {
        private RaycastHit hitinfo;

        /// <summary>
        /// 场景对象实例
        /// </summary>
        private BaseSceneInstance instance;


        [SerializeField] private bool isNav;

        [Tooltip("拾取对象句柄")] [SerializeField] private GameObject mHandler;

        [Tooltip("挂接句柄根节点")] [SerializeField] private Transform mHandlerRoot;


        public EN_CharacterActionState mTransationState = EN_CharacterActionState.EN_CHA_None;

        [Tooltip("拾取对象节点")] [SerializeField] private InvUseNodeType mUseNodeType;

        [Tooltip("拾取对象节点类型")] [SerializeField] private InvUseSlot mUseType;

        private void Start()
        {
            if (instance == null) instance = gameObject.GetComponent<BaseSceneInstance>();

            if (mHandlerRoot == null) mHandlerRoot = Engine.Instance.SceneRoot;
        }

        public override void Execute( object sender, EnPropertyId actionId, string actionParam, object data = null )
        {
            var player = data as Player;

            var state = (En_SwitchState) actionParam.ParseByDefault(0);

            if (player != null)
                switch (state)
                {
                    case En_SwitchState.Off:
                        player.DetachInteractionObject(instance.LinkObject, mUseType, mUseNodeType, mHandler);
                        if (isNav)
                            if (mHandler)
                            {
                                var trans = mHandler.transform;
                                trans.SetParent(mHandlerRoot);

                                if (Physics.Raycast(trans.position + Vector3.down * 0.1f, Vector3.down, out hitinfo,
                                    1000.0f /*, 1 << 8*/))
                                {
                                    trans.DOMove(hitinfo.point, 0.5f);
                                    trans.DORotate(player.PlayerController.transform.rotation.eulerAngles, 0.5f);
                                }
                            }
                        break;
                    case En_SwitchState.On:
                        player.AttachInteractionObject(instance.LinkObject, mUseType, mUseNodeType, mHandler,
                            mTransationState);
                        break;
                }
        }

        private void OnDestroy()
        {
            instance = null;
        }
    }
}