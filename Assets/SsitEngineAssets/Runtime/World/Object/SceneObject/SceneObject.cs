/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/27 16:15:49                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Framework.SceneObject;
using Mirror;
using SsitEngine.Unity;
using SsitEngine.Unity.Action;
using UnityEngine;

namespace Framework.SceneObject
{
    public class SceneObject : BaseObject
    {
        public SceneObject( string mGuid ) : base(mGuid)
        {
        }

        /// <summary>
        /// 帧轮询
        /// </summary>
        /// <param name="elapsed">逻辑流逝时间</param>
        protected internal override void OnUpdate( float elapsed )
        {
            if (mRepresent != null && sceneInstance != null)
            {
                if (!mRepresent.activeSelf)
                    return;
                sceneInstance.OnUpdate();
            }
        }


        /// <summary>
        /// 销毁
        /// </summary>
        public override void Shutdown()
        {
            base.Shutdown();
            if (sceneInstance) sceneInstance.SetLink(false, null);
            sceneInstance = null;
            mData = null;
            Unload();
        }

        #region Property

        //public override void ChangeProperty( object sender, EnPropertyId propertyId, string param, object data = null )
        //{
        //    base.ChangeProperty(sender, propertyId, param, data);
        //}

        //public override void OnChangeProperty( object sender, EnPropertyId propertyId, string param, object data = null )
        //{
        //    base.OnChangeProperty(sender, propertyId, param, data);
        //}

        #endregion

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
                            trigger[i].Init(this);
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

                //mData = null;

                OnInitFinished();
            }
            m_loadStatu = EnLoadStatu.Inited;
        }

        public override void OnInitFinished()
        {
            sceneInstance?.OnInitFinished();
        }

        #endregion
    }
}