/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：场景物体的基类                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年5月27日                             
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;
using Framework.Data;
using SSIT.proto;
using SsitEngine;
using SsitEngine.Data;
using SsitEngine.DebugLog;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.Unity.Action;
using SsitEngine.Unity.Resource;
using SsitEngine.Unity.SceneObject;
using SsitEngine.Unity.Timer;
using UnityEngine;

namespace Framework.SceneObject
{
    public class PropertyParam
    {
        public string param;
        public EnPropertyId property;
    }

    /// <summary>
    /// 物体加载状态
    /// </summary>
    public enum EnLoadStatu
    {
        None,
        Prepared,
        Loading,
        Loaded,
        Inited,
        Unload
    }

    public delegate void OnPropertyChange( EnPropertyId propertyId, string param, object data );

    public delegate void OnCreated( BaseObject obj, object render, object data );

    public class BaseObject : AllocatedObject
    {
        #region 消息处理

        /// <summary>
        /// 消息回调
        /// </summary>
        /// <param name="notification"></param>
        public virtual void HandleNotification( INotification notification )
        {
        }

        #endregion

        #region 组件行为处理

        public void ProcessAction( EnPropertyId propertyId, string param, object data = null )
        {
            if (isDirty)
                return;
            List<ActionBase> actionBase = null;
            if (m_actionMaps.TryGetValue(propertyId, out actionBase))
                foreach (var action in actionBase)
                    action.Execute(this, propertyId, param, data);
        }

        #endregion

        #region 动画

        public virtual bool PlayAnimation( string animName, int layer, bool isSmooth = false,
            float fixedOrNormalizeTime = 0, OnTimerEventHandler onEndPlayFunc = null, object data = null )
        {
            return SsitApplication.Instance.PlayAnimation(this, animName, layer, isSmooth, fixedOrNormalizeTime,
                onEndPlayFunc);
        }

        #endregion

        #region Variable

        protected string mGuid;
        protected string mResouceName;

        protected EnFactoryType m_factoryType = EnFactoryType.None;
        protected EnLoadStatu m_loadStatu = EnLoadStatu.None;

        protected GameObject mRepresent;
        protected BaseAtrribute mAttribute;

        protected OnCreated mOnCreated;
        protected OnPropertyChange m_onPropertyChange;

        // 上层实体对象【抽象对象管理实体】
        protected BaseSceneInstance sceneInstance;

        // 实体化组件行为
        protected Dictionary<EnPropertyId, List<ActionBase>> m_actionMaps;

        // 属性
        protected BaseObject mParent; //归属|权柄

        //protected List<SkillInfo> mSkillList;                   //技能列表
        protected Dictionary<int, Skill> m_skills;
        protected object mData;

        #endregion

        #region Property

        /// <summary>
        /// 物体的加载状态
        /// </summary>
        public EnLoadStatu LoadStatu
        {
            get => m_loadStatu;
            set => m_loadStatu = value;
        }

        /// <summary>
        /// 物体的guid
        /// </summary>
        public string Guid => mGuid;

        /// <summary>
        /// 物体的创建工厂类型
        /// </summary>
        public EnFactoryType FactoryType => m_factoryType;

        public BaseSceneInstance SceneInstance => sceneInstance;

        public void SetResourName( string resPath )
        {
            mResouceName = resPath;
        }

        public string GetResourceName()
        {
            return mResouceName;
        }

        public void SetRepresent( GameObject represent )
        {
            if (mRepresent == null) Create(represent);
        }

        public GameObject GetRepresent()
        {
            return mRepresent;
        }

        public void SetParent( BaseObject parent )
        {
            mParent = parent;
        }

        /// <summary>
        /// 设置归属
        /// </summary>
        /// <param name="parentId"></param>
        public void SetParent( string parentId )
        {
            if (string.IsNullOrEmpty(parentId))
            {
                mParent = null;
                return;
            }
            mParent = ObjectManager.Instance.GetObject<BaseObject>(parentId);
        }

        public BaseObject GetParent()
        {
            return mParent;
        }


        /// <summary>
        /// 设置属性
        /// </summary>
        /// <param name="propertyId">属性id</param>
        /// <param name="property">属性字符串</param>
        /// <param name="data">自定义数据</param>
        public void SetProperty( EnPropertyId propertyId, string property, object data = null )
        {
            OnChangeProperty(this, propertyId, property, data);
        }

        /// <summary>
        /// 获取属性
        /// </summary>
        /// <param name="propertyId"></param>
        /// <returns></returns>
        public string GetProperty( EnPropertyId propertyId )
        {
            return mAttribute?.GetProperty(propertyId);
        }

        /// <summary>
        /// 获取物体属性信息
        /// </summary>
        /// <typeparam name="T">指定的数据类型</typeparam>
        /// <returns>返回指定的数据模型</returns>
        public virtual T InfoData<T>() where T : IInfoData
        {
            return default;
        }

        #endregion

        #region 初始化

        public BaseObject( string mGuid )
        {
            this.mGuid = mGuid;
            m_loadStatu = EnLoadStatu.Prepared;
            m_actionMaps = new Dictionary<EnPropertyId, List<ActionBase>>();
        }

        public void SetAttribute( BaseAtrribute info )
        {
            mAttribute = info;
            mAttribute.SetParent(this);
            //todo：技能初始化
            m_onPropertyChange += mAttribute.NotifyPropertyChange;
        }

        public BaseAtrribute GetAttribute()
        {
            return mAttribute;
        }

        public void AddChangePropertyCallBack( OnPropertyChange func )
        {
            m_onPropertyChange -= func;
            m_onPropertyChange += func;
        }

        public void RemoveChangePropertyCallBack( OnPropertyChange func )
        {
            m_onPropertyChange -= func;
        }

        #endregion

        #region 对象生命周期

        /// <summary>
        /// 场景物体的初始化(先于实体进行的初始化)
        /// </summary>
        protected internal virtual void OnInit()
        {
            if (string.IsNullOrEmpty(mGuid)) throw new SsitEngineException("baseObject's guid is unvaild");
            m_onPropertyChange += ProcessAction;
        }

        public virtual void OnInitFinished()
        {
            sceneInstance?.OnInitFinished();
        }

        /// <summary>
        /// 帧轮询
        /// </summary>
        /// <param name="elapsed">逻辑流逝时间</param>
        protected internal virtual void OnUpdate( float elapsed )
        {
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public override void Shutdown()
        {
            base.Shutdown();
            if (m_actionMaps != null) m_actionMaps.Clear();
            m_actionMaps = null;
            if (m_skills != null)
            {
                var enu = m_skills.GetEnumerator();
                while (enu.MoveNext()) SsitApplication.Instance.DestorySkill(enu.Current.Value.Guid);
                enu.Dispose();
                m_skills.Clear();
            }

            m_skills = null;
            mData = null;
            Unload();
        }

        #endregion

        #region Mono表现

        public void SetOnCreated( OnCreated func, object data = null )
        {
            mOnCreated = func;
            mData = data;
        }

        protected virtual void Create( GameObject warpObject = null )
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
                    //todo:调用资源加载加载
                    // //todo：资源管理器加载资源 异步话加入资源队列 回调执行后续，同步话继续
                    //ResourcesManager.Instance.load
                    if (!string.IsNullOrEmpty(mResouceName))
                        //GameObject rep = ResourcesManager.Instance.LoadAsset(mResouceName, false);
                        ResourcesManager.Instance.LoadAsset<GameObject>(mResouceName,true, obj =>
                        {
                            var rep = Object.Instantiate(obj);
                            if (rep != null)
                                OnCreateComplete(rep);
                            else
                                SsitDebug.Error("对象创建异常" + mResouceName);
                        });
                    else
                        OnCreateComplete(warpObject);
                }
                else
                {
                    OnCreateComplete(warpObject);
                }
            }
        }

        public virtual void Load( bool sync, GameObject represent = null )
        {
            if (m_loadStatu == EnLoadStatu.Loading)
                return;

            m_loadStatu = EnLoadStatu.Loading;

            if (false /*资源配置加载*/)
            {
                //todo: 加载物体配置
                // //todo：资源管理器加载资源 异步话加入资源队列 回调执行后续，同步话继续
                //ResourcesManager.Instance.load
                /*if (string.IsNullOrEmpty(mResouceName))
                {
                    //GameObject rep = ResourcesManager.Instance.LoadAsset(mResouceName, false);
                }
                else
                {

                }*/
            }

            Create(represent);
        }

        public virtual void Unload()
        {
            //todo：检测1、资源配置及2、表现资源放回池子，还是直接释放

            if (mRepresent != null)
            {
                Object.Destroy(mRepresent);
                mRepresent = null;
            }

            m_loadStatu = EnLoadStatu.Unload;
        }


        protected virtual void OnCreateComplete( GameObject represent )
        {
            if (represent != null)
            {
                mRepresent = represent;

                m_loadStatu = EnLoadStatu.Loaded;

                OnInit();

                if (mOnCreated != null)
                    mOnCreated(this, mRepresent, mData);

                // todo：初始化表现对象
                /*ScriptInstance instance = mRepresent.GetOrAddComponent<ScriptInstance>();
                if (instance)
                {
                    instance.SetLink(true);
                    instance.guid = mGuid;
                }*/


                // todo: 注册触发装置和响应事件

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

        #region Internal Members

        public void SetVisible( bool isVisible, bool direct = false )
        {
            if (sceneInstance)
            {
                if (direct)
                    mRepresent?.SetActive(isVisible);
                sceneInstance.EnableObject = isVisible;
            }
            else
            {
                mRepresent?.SetActive(isVisible);
            }
        }

        public bool GetVisible()
        {
            if (sceneInstance) return sceneInstance.isActiveAndEnabled;

            return false;
        }

        public void AttachObject( BaseObject p )
        {
        }

        #endregion

        #region 属性处理

        public virtual void ChangeProperty( object sender, List<PropertyParam> propertys, object data = null )
        {
            if (GlobalManager.Instance.IsSync)
            {
                var request = new CSSyncSceneObjInfoRequest();

                var syncInfo = new SyncSceneObjInfo();
                syncInfo.guid = mGuid;
                for (var i = 0; i < propertys.Count; i++)
                {
                    var info = propertys[i];
                    syncInfo.vars.Add(new SyncVar {id = (int) info.property, param = info.param});
                }

                request.sceneObjInfo.Add(syncInfo);
                //Facade.Instance.SendNotification((ushort) EnMirrorEvent.SendMessage,new MessagePackage(ConstMessageID.CSSyncSceneObjInfoRequest, request), true);
            }
            else
            {
                OnChangeProperty(sender, propertys, data);
            }
        }

        public virtual void ChangeProperty( object sender, EnPropertyId propertyId, string p, object data = null )
        {
            if (GlobalManager.Instance.IsSync)
            {
                var request = new CSSyncSceneObjInfoRequest();
                var syncInfo = new SyncSceneObjInfo();
                syncInfo.guid = mGuid;
                syncInfo.vars.Add(new SyncVar {id = (int) propertyId, param = p});

                request.sceneObjInfo.Add(syncInfo);
                //Facade.Instance.SendNotification((ushort)EnMirrorEvent.SendMessage, new MessagePackage(ConstMessageID.CSSyncSceneObjInfoRequest, request), true);
            }
            else
            {
                OnChangeProperty(sender, new List<PropertyParam> {new PropertyParam {property = propertyId, param = p}},
                    data);
            }
        }

        public virtual void OnChangeProperty( object sender, List<PropertyParam> propertys, object data = null )
        {
            if (m_onPropertyChange != null)
                for (var i = 0; i < propertys.Count; i++)
                {
                    var info = propertys[i];
                    m_onPropertyChange(info.property, info.param, data);
                }
        }

        public virtual void OnChangeProperty( object sender, EnPropertyId propertyId, string p, object data = null )
        {
            if (m_onPropertyChange != null) m_onPropertyChange(propertyId, p, data);
        }

        #endregion

        #region 技能

        public void AddSkill( Skill skill )
        {
            if (m_skills == null) m_skills = new Dictionary<int, Skill>();

            var skillid = skill.GetSkillInfo().skillId;
            if (m_skills.ContainsKey(skillid))
            {
                SsitDebug.Error("角色拥有相同技能： " + skillid);
                return;
            }
            skill.SetOwner(this);
            m_skills.Add(skillid, skill);

            //Debug.Log($"{GetAttribute().Name} ++ {m_skills.Count} ++{mAttribute.GetHashCode()}");
        }

        public List<SkillAttribute> GetSkillInfos()
        {
            if (m_skills == null) return null;
            return m_skills.Values.ToList().ConvertAll(x => { return x.GetSkillInfo(); });
        }

        public List<Skill> GetSkills()
        {
            if (m_skills == null) return null;
            return m_skills.Values.ToList();
        }

        public void SetSkillState( int skillId, EnSkillState state )
        {
            if (m_skills.TryGetValue(skillId, out var skill)) skill.SetState(state);
        }

        /// <summary>
        /// 施放技能
        /// </summary>
        /// <param name="skillId">技能ID</param>
        /// <param name="isOn">是否开启技能 默认为false</param>
        /// <returns></returns>
        public bool ExcuteSkill( int skillId, bool isOn = false )
        {
            if (m_skills.TryGetValue(skillId, out var skill))
            {
                skill.ExcuteSkill(isOn);
                return true;
            }
            return false;

            //            if ((En_SkillTriigleType)skillInfo.SkillTriggerType == En_SkillTriigleType.EN_Buttton)
            //            {
            //
            //
            //                if (Enum.TryParse(skillInfo.MsgName, out ConstNotification mesId))
            //                {
            //
            //                    if ((EN_SkillMsgType)skillInfo.SkillMsgType == EN_SkillMsgType.EN_CreateObject ||
            //                        (EN_SkillMsgType)skillInfo.SkillMsgType == EN_SkillMsgType.EN_OpenForm ||
            //                        (EN_SkillMsgType)skillInfo.SkillMsgType == EN_SkillMsgType.EN_EventPost)
            //                    {
            //
            //                        skillInfo.IsOn = isOn;
            //                        Facade.Instance.SendNotification((ushort)mesId, skillInfo.MsgParm);
            //                    }
            //
            //                }
            //
            //            }
            //            else
            //            {
            //                skillInfo.IsOn = isOn;
            //
            //                if (isOn)
            //                {
            //
            //                    if (Enum.TryParse(skillInfo.MsgName, out ConstNotification mesId))
            //                    {
            //                        Facade.Instance.SendNotification((ushort)mesId, skillInfo.MsgParm);
            //                    }
            //                }
            //                else
            //                {
            //                    if (Enum.TryParse(skillInfo.ExtraMsgName, out ConstNotification mesId))
            //                    {
            //                        Facade.Instance.SendNotification((ushort)mesId, skillInfo.ExtraMsgParm);
            //                    }
            //                }
            //
            //            }
            //            return true;
        }

        #endregion
    }
}