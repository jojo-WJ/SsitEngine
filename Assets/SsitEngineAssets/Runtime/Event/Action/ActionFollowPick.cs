/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：跟随捡起                                                   
*│　作   者：Jusam                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/09/03                        
*└──────────────────────────────────────────────────────────────┘
*/

using DG.Tweening;
using Framework;
using Framework.SceneObject;
using SsitEngine.Fsm;
using SsitEngine.Unity.Avatar;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace SsitEngine.Unity.Action
{
    public class ActionFollowPick : ActionBase
    {
        private RaycastHit hitinfo;

        [SerializeField] private bool isNav;

        [Tooltip("拾取对象句柄")] [SerializeField] private GameObject mHandler;

        [Tooltip("挂接句柄根节点")] [SerializeField] private Transform mHandlerRoot;

        private BaseSceneInstance mInstance;

        // 被跟随挂点
        private InvAttachmentPoint mPoint;
        public EN_CharacterActionState mTransationState = EN_CharacterActionState.EN_CHA_None;

        [Tooltip("拾取对象节点")] [SerializeField] private InvUseNodeType mUseNodeType;

        [Tooltip("拾取对象节点类型")] [SerializeField] private InvUseSlot mUseType;

        public Vector3 offset = Vector3.zero;

        private bool Sync;

        public GameObject MHandler
        {
            get => mHandler;
            set => mHandler = value;
        }

        public InvUseSlot MUseType
        {
            get => mUseType;
            set => mUseType = value;
        }

        public InvUseNodeType MUseNodeType
        {
            get => mUseNodeType;
            set => mUseNodeType = value;
        }


        private void Start()
        {
            if (mInstance == null) mInstance = GetComponent<BaseSceneInstance>();


            if (mHandlerRoot == null) mHandlerRoot = Engine.Instance.SceneRoot;
        }

        public override void Execute( object sender, EnPropertyId m_actionId, string m_actionParam, object data = null )
        {
            var player = data as Player;

            var state = (En_SwitchState) m_actionParam.ParseByDefault(0);

            if (player != null)
                switch (state)
                {
                    case En_SwitchState.Off:

                        // 取消跟随

                        // player.ChangeState(mTransationState);
                        // mHandler.GetComponent<Rigidbody>().isKinematic = false;
                        Sync = false;

                        player.DeFollowInteractionObject(mInstance.LinkObject, mUseType, mUseNodeType, mHandler);

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
                        // 开始跟随
                        //player.SubStateEvent(EN_CharacterActionState.EN_CHA_WPipeReady,(int )EnCharacterStateEvent.WaterPipeReady,OnAnimCallBack );
                        player.ChangeState(EN_CharacterActionState.EN_CHA_WPipeReady);
                        break;
                }
        }

        private void FixedUpdate()
        {
            if (Sync && mPoint)
            {
                mHandler.transform.position = mPoint.transform.position;
                mHandler.transform.rotation = mPoint.transform.rotation;
                //mHandler.transform.localScale = Vector3.one;
            }
        }

        private void OnAnimCallBack( IFsm<Player> fsm, object sender, object userData )
        {
            mPoint = fsm.Owner.EquipController.GetUseHandAttachPoint(InvUseNodeType.InvWaterPipeNode);
            //mHandler.GetComponent<Rigidbody>().isKinematic = true;
            fsm.Owner.FollowInteractionObject(mInstance.LinkObject, mUseType, mUseNodeType, mHandler, mTransationState);
            Sync = true;

            //fsm.Owner.UnSubStateEvent(EN_CharacterActionState.EN_CHA_WPipeReady,(int )EnCharacterStateEvent.WaterPipeReady,OnAnimCallBack);
        }
    }
}