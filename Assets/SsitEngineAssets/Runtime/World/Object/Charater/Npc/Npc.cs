/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/30 12:02:50                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Framework.Data;
using Framework.SceneObject;
using Mirror;
using SsitEngine.DebugLog;
using SsitEngine.Fsm;
using SsitEngine.Unity;
using SsitEngine.Unity.Action;
using SsitEngine.Unity.Avatar;
using SsitEngine.Unity.HUD;
using SsitEngine.Unity.Resource;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace Framework.SceneObject
{
    public class Npc : Character
    {
        public Npc( string mGuid ) : base(mGuid)
        {
            //mUsingItems = new BaseObject[(int)InvUseNodeType.MaxValue];
            //mEquipmentList = new Item[(int)InvSlot.MaxValue];
        }

        #region HUD

        public override void InitHUD( Transform trans, string showName, string message )
        {
            if (mHud == null) mHud = trans.Find("HeadHost")?.GetComponent<HudElement>();

            if (mHud != null)
            {
                mHud.AttachTo(NavigationElementType.HUD, true);
                ChangeHUDText(showName, message);
            }
        }

        #endregion

        #region FSM

        protected override void InitFsm()
        {
            mFsm = FsmHelper.CreateFsm(Guid
                , this
                , new CharacterStateStay(this)
            );
            //access: 等待状态接入
            mFsm.OnStateCheckHandler = OnStateCheckCallback;
            mFsm.Start((int) EN_CharacterActionState.EN_CHA_Stay);
        }

        #endregion

        #region Property Sync Center

        //public override void ChangeProperty( object sender, EnPropertyId propertyId, string param, object data = null )
        //{
        //    base.ChangeProperty(sender, propertyId, param, data);
        //}

        //public override void OnChangeProperty( object sender, EnPropertyId propertyId, string param, object data = null )
        //{
        //    base.OnChangeProperty(sender, propertyId, param, data);
        //}

        #endregion

        #region 子类实现

        protected internal override void OnInit()
        {
            InitFsm();
        }

        public override void OnInitFinished()
        {
            //刷新使用回调
            //OnUseChanged?.Invoke(InvUseNodeType.InvNone,null,null);
        }

        public new PlayerAttribute GetAttribute()
        {
            return mAttribute as PlayerAttribute;
        }


        /// <summary>
        /// 帧轮询
        /// </summary>
        /// <param name="elapsed">逻辑流逝时间</param>

        #endregion

        #region 实体表现初始化

        protected override void Create( GameObject warpObject = null )
        {
            //SsitDebug.Debug(mRepresent + "Create" + warpObject);
            if (mRepresent != null)
            {
                //todo:统一处理销毁的问题（是用池管理，还是其他缓存方式）
                Object.Destroy(mRepresent);
                mRepresent = null;
            }


            if (m_loadStatu == EnLoadStatu.Loading)
            {
                if (warpObject == null)
                {
                    // //todo：资源管理器加载资源 异步话加入资源队列 回调执行后续，同步话继续
                    if (GlobalManager.Instance.IsSync && NetworkClient.active) return;
                    if (!string.IsNullOrEmpty(mResouceName))
                        //GameObject rep = ResourcesManager.Instance.LoadAsset(mResouceName, false);
                        ResourcesManager.Instance.LoadAsset<GameObject>(mResouceName, false,obj =>
                        {
                            var rep = Object.Instantiate(obj);
                            if (rep != null)
                                OnCreateComplete(rep);
                            else
                                SsitDebug.Error("对象创建异常" + mResouceName);
                        });
                    else
                        SsitDebug.Error("对象创建异常 mResouceName is null" + mResouceName);
                }
                else
                {
                    OnCreateComplete(warpObject);
                }
            }
        }

        protected override void OnCreateComplete( GameObject represent )
        {
            if (represent != null)
            {
                mRepresent = represent;

                m_loadStatu = EnLoadStatu.Loaded;


                OnInit();

                // todo:初始化父物体
                mRepresent.transform.SetParent(Engine.Instance.PlayerRoot);

                // todo：初始化表现对象
                if (GlobalManager.Instance.IsSync && NetworkServer.active)
                {
                    var synchronizer = mRepresent.GetComponent<ScriptSynchronizer>();
                    if (synchronizer) synchronizer.guid = mGuid;
                }

                // 注册和响应事件
                var actions = mRepresent.GetComponentsInChildren<ActionBase>(true);
                for (var i = 0; i < actions.Length; i++)
                    if (actions[i].DirectDescend)
                    {
                        if (m_actionMaps.ContainsKey(actions[i].ActionId))
                            m_actionMaps[actions[i].ActionId].Add(actions[i]);
                        else
                            m_actionMaps.Add(actions[i].ActionId, new List<ActionBase> {actions[i]});
                    }
                // todo :初始化物体触发装置
                var trigger = mRepresent.GetComponentsInChildren<Trigger.Trigger>();
                //if (Engine.Instance.PlatConfig.isSync)
                {
                    for (var i = 0; i < trigger.Length; i++)
                    {
                        if (trigger[i].isOnlyServer && NetworkClient.active)
                        {
                            trigger[i].enabled = false;
                        }
                        else if (trigger[i].isLocalClient)
                        {
                            //trigger[i].AddCondition
                        }

                        if (trigger[i].enabled)
                        {
                            if (Engine.Instance.PlatConfig.isSync)
                                trigger[i].m_onActionEvent.AddListener(ChangeProperty);
                            else
                                trigger[i].m_onActionEvent.AddListener(OnChangeProperty);
                        }
                    }
                }
                //instance.guid = mGuid;

                //old version
                sceneInstance = mRepresent.GetComponent<PlayerInstance>();
                if (sceneInstance)
                {
                    sceneInstance.Guid = mGuid;
                    sceneInstance.Init();
                    sceneInstance.SetLink(true, this);
                    m_onPropertyChange += sceneInstance.OnChangePropertyCallback;
                }
                // playerController
                m_playerController = mRepresent.GetComponent<ThirdPersonController>();
                if (m_playerController)
                {
                    m_playerController.Init();
                    m_playerController.SetLink(true, this);
                }


                // equipController
                m_equipController = mRepresent.GetComponent<EquipController>();
                if (m_equipController) m_equipController.Init(this);


                if (mOnCreated != null)
                    mOnCreated(this, mRepresent, mData);

                //mData = null;

                OnInitFinished();

                //todo：光照贴图变化
                /*if (mLightmapIndex != -1)
                {
                    MeshRenderer mr = go.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        mr.lightmapIndex = mLightmapIndex;
                        mr.lightmapScaleOffset = xx;
                    }
                }*/

                //todo: 骨骼节点

                //todo: 设置渲染、层级、光影

                /*Renderer[] list = mRepresent.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < list.Length; i++)
                {
                    var r = list[i];
                    r.gameObject.layer = mRepresent.layer;
                    r.shadowCastingMode = isCastShadows() ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                    r.receiveShadows = isReceiveShadows();
                }*/
            }
            m_loadStatu = EnLoadStatu.Inited;
        }

        #endregion

        #region 消息处理

        #endregion
    }
}