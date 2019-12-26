/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/7/23 10:33:26                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Mirror;
using SsitEngine.DebugLog;
using SsitEngine.Unity;
using SsitEngine.Unity.Action;
using SsitEngine.Unity.Avatar;
using SsitEngine.Unity.Resource;
using UnityEngine;

namespace Framework.SceneObject
{
    public enum EnItemState
    {
        IS_Normal,
        IS_Using,
        IS_Lock,
        Is_Destroy
    }

    /// <summary>
    /// 物体道具
    /// </summary>
    public class Item : BaseObject
    {
        /// <summary>
        /// 我的归属
        /// </summary>
        protected Player mOwner;

        /// <summary>
        /// 挂点
        /// </summary>
        protected InvAttachmentPoint mParnetAttach;

        /// <summary>
        /// 道具状态（使用冷却用的暂时仅用于记录道具句柄）
        /// </summary>
        protected EnItemState mState;

        public Item( string mGuid ) : base(mGuid)
        {
            mOwner = null;
            mState = EnItemState.IS_Normal;
        }

        /// <summary>
        /// 挂点
        /// </summary>
        public InvAttachmentPoint AttachPoint => mParnetAttach;

        public new ItemAtrribute GetAttribute()
        {
            return mAttribute as ItemAtrribute;
        }

        public Player GetOwner()
        {
            return mOwner;
        }

        public void SetOwner( Player player )
        {
            mOwner = player;
        }

        #region 子类实现

        /// <summary>
        /// 销毁
        /// </summary>
        public override void Shutdown()
        {
            base.Shutdown();
            mOwner = null;
            Detach();

            Unload();
        }

        #endregion

        public void Use()
        {
            //todo:set state and begin timer change
            mState = EnItemState.IS_Using;
        }

        public void UnUse()
        {
            mState = EnItemState.IS_Normal;
        }

        public EnItemState GetState()
        {
            return mState;
        }

        public void SetState( EnItemState state )
        {
            mState = state;
        }


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
                    if (!string.IsNullOrEmpty(mResouceName))
                        //GameObject rep = ResourcesManager.Instance.LoadAsset(mResouceName, false);
                        ResourcesManager.Instance.LoadAsset<GameObject>(mResouceName, true, obj =>
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

                if (mOnCreated != null)
                    mOnCreated(this, mRepresent, mData);

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

        #region 挂点

        public void Attach( InvAttachmentPoint equipPoint, Vector3 offset )
        {
            if (mParnetAttach != null)
                mParnetAttach.Detach(false);
            mParnetAttach = equipPoint;
            var attr = GetAttribute();
            equipPoint.Attach(mRepresent, attr.CombineIndex, offset);
        }


        public void Detach()
        {
            mParnetAttach?.Detach();
            mParnetAttach = null;
        }

        #endregion

        #region Clone

        public GameObject CloneRepresent()
        {
            return Object.Instantiate(mRepresent);
        }

        public void AttachClone( InvAttachmentPoint equipPoint, Vector3 offset )
        {
            mParnetAttach = equipPoint;
            var attr = GetAttribute();
            var euipClone = CloneRepresent();
            euipClone.SetActive(true);
            equipPoint.Attach(euipClone, attr.CombineIndex, offset);
        }

        #endregion
    }
}