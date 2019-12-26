/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/8/19 16:50:30                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Framework.SceneObject;
using Mirror;
using SsitEngine.Unity;
using SsitEngine.Unity.Action;
using SsitEngine.Unity.HUD;
using UnityEngine;

namespace Framework.SceneObject
{
    /// <summary>
    /// 交通工具
    /// </summary>
    public class Vehicle : BaseObject
    {
        //hud操作
        public List<Player> mPlayers;

        public Vehicle( string mGuid ) : base(mGuid)
        {
            mPlayers = new List<Player>();
        }

        public HudElement Hud { get; set; }


        #region 实体表现初始化

        protected override void OnCreateComplete( GameObject represent )
        {
            if (represent != null)
            {
                mRepresent = represent;

                m_loadStatu = EnLoadStatu.Loaded;

                OnInit();


                // todo:初始化父物体
                mRepresent.transform.SetParent(Engine.Instance.SceneRoot);

                // todo：初始化表现对象
                sceneInstance = mRepresent.GetOrAddComponent<BaseSceneInstance>();
                if (sceneInstance)
                {
                    sceneInstance.SetLink(true, this);
                    sceneInstance.Guid = mGuid;
                    sceneInstance.Init();
                    m_onPropertyChange += sceneInstance.OnChangePropertyCallback;
                }

                // todo: 注册和响应事件
                var actions = mRepresent.GetComponentsInChildren<ActionBase>(true);
                for (var i = 0; i < actions.Length; i++)
                    if (actions[i].DirectDescend)
                    {
                        actions[i].Init(this);
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
                if (mOnCreated != null)
                    mOnCreated(this, mRepresent, mData);

                mData = null;
            }
            m_loadStatu = EnLoadStatu.Inited;
        }

        #endregion

        public void AddPlayers( Player player )
        {
            mPlayers.Add(player);
            player.SetParent(this);
        }

        public List<Player> GetPlayers()
        {
            return mPlayers;
        }

        #region 子类实现

        /// <summary>
        /// 帧轮询
        /// </summary>
        /// <param name="elapsed">逻辑流逝时间</param>
        protected internal override void OnUpdate( float elapsed )
        {
            if (mRepresent != null)
            {
                if (!mRepresent.activeSelf)
                    return;
                sceneInstance?.OnUpdate();
            }
        }

        public override void Shutdown()
        {
            base.Shutdown();

            mPlayers.Clear();
            mPlayers = null;
        }

        #endregion

        #region HUD

        public void InitHUD( Transform trans, string showName, string message )
        {
            if (Hud == null) Hud = trans.Find("HeadHost")?.GetComponent<HudElement>();

            if (Hud != null)
            {
                Hud.AttachTo(NavigationElementType.HUD, true);
                ChangeHUDText(showName, message);
            }
        }

        public void RemoveHUD()
        {
            if (Hud != null) Hud.AttachTo(NavigationElementType.HUD, false);
        }

        public void ChangeHUDText( string showName, string message )
        {
            if (Hud != null)
                if (Hud.Hud != null)
                {
                    Hud.Hud.ChangeNameText(showName);
                    Hud.Hud.ChangeStateText(message);
                }
        }

        #endregion
    }
}