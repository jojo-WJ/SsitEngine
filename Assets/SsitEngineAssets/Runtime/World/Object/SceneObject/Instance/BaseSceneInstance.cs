using System;
using System.Collections.Generic;
using Framework.Data;
using Framework.Logic;
using Framework.Tip;
using Framework.Utility;
using HighlightingSystem;
using SsitEngine;
using SsitEngine.DebugLog;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Unity;
using SsitEngine.Unity.SceneObject;
using SsitEngine.Unity.Utility;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Framework.SceneObject
{
    public abstract class BaseSceneInstance : MonoBase, ISave
    {
        private Highlighter highlighter;

        [Header("底层连接")] [Disable] public bool isLink;

        [SerializeField] protected int itemId;

        protected BaseObject m_baseObject;
        protected List<Collider> m_Colliders;

        [Disable] [SerializeField] protected string m_guid = string.Empty;

        protected List<Material> m_Materials;
        //protected List<Texture> m_Textures;  //未使用、屏蔽

        protected Rigidbody m_Rigibody;

        [SerializeField] protected EnObjectType mType;

        public UnityAction<BaseSceneInstance> OnDisVisiableEvent;

        /// <summary>
        /// 物体的类型（细类划分【相同类存在不同类型】）
        /// [remark]
        /// </summary>
        public virtual EnObjectType Type => mType;

        /// <summary>
        /// 物体显示隐藏
        /// </summary>
        public bool EnableObject
        {
            get => gameObject.activeSelf;
            set
            {
                if (GlobalManager.Instance.IsSync)
                    LinkObject.ChangeProperty(this, EnPropertyId.Active, value ? "1" : "0");
                else
                    LinkObject.OnChangeProperty(this, EnPropertyId.Active, value ? "1" : "0");
            }
        }

        /// <summary>
        /// 物体唯一id（必须保证其唯一性）
        /// </summary>
        public string Guid
        {
            set => m_guid = value;
            get => m_guid;
        }

        /// <summary>
        /// 物体的数据id（必须关联本地表格）
        /// </summary>
        public int ItemID
        {
            get => itemId;
            set => itemId = value;
        }

        #region Mono

        protected virtual void OnDestroy()
        {
            if (m_Materials != null)
                for (var i = 0; i < m_Materials.Count; i++)
                    Destroy(m_Materials[i]);
            m_Materials = null;
            m_Colliders = null;
            OnDisVisiableEvent?.Invoke(this);
            OnDisVisiableEvent = null;
            if (m_baseObject != null && !m_baseObject.IsDirty)
            {
                ObjectManager.Instance.DestroyObject(Guid);
                m_baseObject = null;
            }
        }

        #endregion


        #region 虚方法（子类可进行重写）

        /// <summary>
        /// 权限
        /// </summary>
        public virtual bool HasAuthority
        {
            get
            {
                var hasAuthiroty = Utilitys.CheckAuthiroty(this);
                return hasAuthiroty;
            }
        }

        /// <summary>
        /// 是否本段持有
        /// </summary>
        public virtual bool IslocalClient
        {
            get
            {
                var auth = LinkObject.GetParent();
                if (auth == null || GlobalManager.Instance.CachePlayer == auth) return true;

                return false;
            }
        }

        /// <summary>
        /// 实体表现初始化
        /// </summary>
        /// <param name="guid"></param>
        public virtual void Init( string guid = null )
        {
            m_Materials = new List<Material>();
            //m_Textures = new List<Texture>();
            /*Renderer[] renderer = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderer.Length; i++)
            {
                m_Materials.AddRange(renderer[i].materials);
                for (int j = 0; j < renderer[i].materials.Length; j++)
                {
                    m_Textures.Add(renderer[i].materials[j].mainTexture);
                }
            }*/
            m_Colliders = new List<Collider>();
            var colliders = GetComponentsInChildren<Collider>(true);

            for (var i = 0; i < colliders.Length; i++)
            {
                var cc = colliders[i];
                if (cc.gameObject.layer == LayerUtils.IgnoreRaycast) continue;
                m_Colliders.Add(cc);
            }

            m_Rigibody = GetComponent<Rigidbody>();
            highlighter = GetComponentInChildren<Highlighter>(true);
            if (m_isHightLight && null == highlighter) highlighter = gameObject.AddComponent<Highlighter>();
            //gameObject.transform.parent = GameObjectRootManager.GetGameObjectRoot(this);
            //LastFramePos = transform.position;
        }

        /// <summary>
        /// 底层初始化完成后
        /// </summary>
        public virtual void OnInitFinished()
        {
        }

        /// <summary>
        /// 实例轮询
        /// </summary>
        public virtual void OnUpdate()
        {
            //Facade.Instance.SendNotification((ushort)UIMiniMapFormEvent.UpdateTransfromPos, Guid, transform);
        }


        #region 上层实体的消息回调

        /// <summary>
        /// 消息处理回调
        /// </summary>
        /// <param name="notification">消息体</param>
        public override void HandleNotification( INotification notification )
        {
            var eventArgs = notification as MvEventArgs;
            if (eventArgs == null)
                return;
            switch (eventArgs.Id)
            {
                case (ushort) EnPropertyId.Selected:
                {
                    var state = (bool) notification.Body;
                    OnSelect(state, eventArgs?.FirstValue);
                    //m_baseObject?.OnChangeProperty(this, EnPropertyId.Selected, state.DeParseByDefault(), m_baseObject);
                }
                    break;
                case (ushort) EnPropertyId.Wind:
                {
                    var weatherInfo = notification.Body as WeatherInfo;
                    if (weatherInfo != null)
                    {
                        OnWind(weatherInfo.WindDirection, weatherInfo.WindLevel);
                    }
                }
                    break;
            }
        }

        /// <summary>
        /// 属性改变回调
        /// </summary>
        /// <param name="propertyId">属性id</param>
        /// <param name="property">属性参数(字符串化)</param>
        /// <param name="data">自定义属性数据</param>
        public virtual void OnChangePropertyCallback( EnPropertyId propertyId, string property, object data = null )
        {
            switch (propertyId)
            {
                case EnPropertyId.Active:
                    var isVisiable = property.ParseByDefault(true);
                    gameObject.SetActive(isVisiable);
                    if (!isVisiable)
                    {
                        OnDisVisiableEvent?.Invoke(this);
                        OnDisVisiableEvent = null;
                    }
                    break;
                case EnPropertyId.Show3DTag:
                    Set3DTag();
                    break;
                case EnPropertyId.Show2DTag:
                    Set2DTag(property.ParseByDefault(false));
                    break;
                case EnPropertyId.Position:
                    OnMove(property.ParseByDefault(transform.position));
                    break;
                case EnPropertyId.Rotate:
                    OnRotate(property.ParseByDefault(transform.rotation));
                    break;
            }
        }

        #endregion

        #region Internal Members

        public virtual void OnSelect( bool state, object preSelect )
        {
            IsSelected = state;
            if (state)
                OnSelected();
            else
                OnUnSelected();
        }

        public virtual void OnSelected( bool isIndicator = false )
        {
            if (highlighter)
            {
                highlighter.enabled = true;
                highlighter.ConstantOn(Color.cyan);
            }
        }

        public virtual void OnUnSelected( bool isIndicator = false )
        {
            if (highlighter)
            {
                highlighter.Off();
                highlighter.enabled = false;
            }
        }

        /// <summary>
        /// 风向变化
        /// </summary>
        /// <param name="windDirection"></param>
        /// <param name="level"></param>
        public virtual void OnWind( EnWindDirection windDirection, int level )
        {
        }

        #endregion

        #endregion

        #region Base Memberes

        public virtual void ChangeColor( Color color )
        {
            if (m_Materials == null) return;
            for (var i = 0; i < m_Materials.Count; i++) m_Materials[i].SetColor(name, color);
        }

        public virtual void ClearTexture( string name = null )
        {
            if (name == null) name = "_MainTex";
            for (var i = 0; i < m_Materials.Count; i++) m_Materials[i].SetTexture(name, null);
        }

        public virtual void ResetTexture( string name = null )
        {
            /*if (name == null)
            {
                return;
            }
            for (int i = 0; i < m_Materials.Count; i++)
            {
                m_Materials[i].SetTexture(name, m_Textures[i]);
            }*/
        }

        public virtual void SetColliderActive( bool enable )
        {
            if (null == m_Colliders)
                //Debug.LogError("SetColliderActive error, ensure this base scene object had inited already please");
                return;

            for (var i = 0; i < m_Colliders.Count; i++)
            {
                if (m_Colliders[i] == null) continue;
                m_Colliders[i].enabled = enable;
            }

            if (m_Rigibody)
            {
                if (enable)
                {
                    m_Rigibody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    m_Rigibody.isKinematic = true;
                }
                else
                {
                    m_Rigibody.isKinematic = false;
                    m_Rigibody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                }
            }
        }

        public virtual void SetColliderTriggler( bool enable )
        {
            if (null == m_Colliders)
                Debug.LogError("SetColliderActive error, ensure this base scene object had inited already please");

            for (var i = 0; i < m_Colliders.Count; i++)
            {
                if (m_Colliders[i] == null) continue;
                m_Colliders[i].isTrigger = enable;
            }
        }

        #endregion

        #region 放置检测

        public bool EnableLimitMove { get; internal set; }

        public virtual bool OnDragMoveStart()
        {
            return true;
        }

        public virtual void OnDragMove( Vector3 position )
        {
            transform.position = position;
        }

        public virtual bool OnDragMoveEnd()
        {
            return true;
        }

        #endregion

        #region 回放

        //记录/回放初始化标记
        protected bool isInitRecord;

        public virtual void SaveRecord()
        {
            Facade.Instance.SendNotification((ushort) EnEzReplayEvent.Mark, this);
        }

        public SavedBase GeneralSaveData( bool isDeepClone )
        {
            var ret = LinkObject.GetAttribute();
            if (null == LinkObject) SsitDebug.Error($"LinkObject is null by instance {gameObject.name}");
            ret.SetSaveObjState(gameObject);
            if (isDeepClone)
            {
                if (!isInitRecord)
                {
                    ret.InitBaseTrack();
                    isInitRecord = true;
                }

                try
                {
                    var temp = SerializationUtils.Clone(ret);
                    ret.IsChange = false;
                    return temp;
                }
                catch (Exception e)
                {
                    SsitDebug.Fatal($"{name}exception{e.Message}");
                    throw;
                }
            }

            return ret;
        }

        public GameObject GetRepresent()
        {
            return gameObject;
        }

        public virtual void SynchronizeProperties( SavedBase savedState, bool isReset, bool isFristFrame )
        {
            var ret = LinkObject.GetAttribute();


            gameObject.SetActive(savedState.isActive);

            var state = savedState as SaveObjState;
            if (state == null)
                return;

            if (isFristFrame)
                ret.ResetBaseTrack(state.mBaseTrack);

            OnMove(state.position);
            OnRotate(state.rotation);
        }

        public virtual void SynchronizeProperties( InfoData infoData, ref bool isContinueSyn )
        {
        }

        #endregion


        /*
         * 新版1.0重构后 by xx
         * 
         */


        #region 底层实体实现

        public BaseObject LinkObject => m_baseObject;

        public void SetLink( bool b, BaseObject baseObj )
        {
            isLink = b;
            m_baseObject = baseObj;

            if (m_baseObject != null)
            {
                itemId = baseObj.GetAttribute().DataId;
                m_guid = baseObj.Guid;
            }
        }

        public void OnPostRegister()
        {
            //注册对象实例
            if (itemId == 0)
                SsitApplication.Instance.CreateObject(m_guid, null, new BaseAtrribute(), null, null, gameObject);
            else
                SsitApplication.Instance.CreateObject(m_guid, itemId, null, null, gameObject);
        }

        #endregion

        #region Editor Variable

        public bool isCanEditorable;

        /*
        [SerializeField]
        private bool m_isMovable = true;
        [SerializeField]
        private bool m_isMovableOnX = true;
        [SerializeField]
        private bool m_isMovableOnY = true;
        [SerializeField]
        private bool m_isMovableOnZ = true;

        [SerializeField]
        private bool m_isRotatable = true;
        [SerializeField]
        private bool m_isRotatableAroundX = true;
        [SerializeField]
        private bool m_isRotatableAroundY = true;
        [SerializeField]
        private bool m_isRotatableAroundZ = true;

        [SerializeField]
        private bool m_isScaleable = true;
        [SerializeField]
        private bool m_isScaleableOnX = true;
        [SerializeField]
        private bool m_isScaleableOnY = true;
        [SerializeField]
        private bool m_isScaleableOnZ = true;

        private LE_ObjectEditHandle m_editHandle = null;

        /// <summary>
        /// 编辑句柄
        /// </summary>
        private InputManager.EnObjectEditSpace m_editSpace = InputManager.EnObjectEditSpace.SELF;
        private InputManager.EnObjectEditMode m_editMode = InputManager.EnObjectEditMode.NO_EDIT;
*/

        [SerializeField] private bool m_isHightLight = true;

        /// <summary>
        /// 是否可编辑
        /// </summary>
        public bool CanEdit
        {
            get => isCanEditorable;
            set => isCanEditorable = value;
        }

        /// <summary>
        /// 是否可拖拽移动.
        /// </summary>
        [field: SerializeField]
        public bool IsSmartMove { get; set; } = true;

        /// <summary>
        /// 如果为真，放置时将旋转对象以适合曲面法向(切面放置)
        /// </summary>
        [field: SerializeField]
        public bool IsPlacementRotationByNormal { get; set; } = true;

        [field: Header("Test")]
        [field: Disable]
        [field: SerializeField]
        public bool IsSelected { get; set; }

        /*
        /// <summary>
        /// Move handle will be displayed when this object is selected in the level editor if this property is true.
        /// </summary>
        public bool IsMovable
        {
            get { return m_isMovable; }
            set { m_isMovable = value; }
        }

        /// <summary>
        /// Move handle will have x axis if this property is true.
        /// </summary>
        public bool IsMovableOnX
        {
            get { return m_isMovableOnX; }
            set { m_isMovableOnX = value; }
        }

        /// <summary>
        /// Move handle will have y axis if this property is true.
        /// </summary>
        public bool IsMovableOnY
        {
            get { return m_isMovableOnY; }
            set { m_isMovableOnY = value; }
        }

        /// <summary>
        /// Move handle will have z axis if this property is true.
        /// </summary>
        public bool IsMovableOnZ
        {
            get { return m_isMovableOnZ; }
            set { m_isMovableOnZ = value; }
        }

        /// <summary>
        /// Rotate handle will be displayed when this object is selected in the level editor if this property is true.
        /// </summary>
        public bool IsRotatable
        {
            get { return m_isRotatable; }
            set { m_isRotatable = value; }
        }

        /// <summary>
        /// Rotate handle will have x axis if this property is true.
        /// </summary>
        public bool IsRotatableAroundX
        {
            get { return m_isRotatableAroundX; }
            set { m_isRotatableAroundX = value; }
        }

        /// <summary>
        /// Rotate handle will have y axis if this property is true.
        /// </summary>
        public bool IsRotatableAroundY
        {
            get { return m_isRotatableAroundY; }
            set { m_isRotatableAroundY = value; }
        }


        /// <summary>
        /// Rotate handle will have z axis if this property is true.
        /// </summary>
        public bool IsRotatableAroundZ
        {
            get { return m_isRotatableAroundZ; }
            set { m_isRotatableAroundZ = value; }
        }

        /// <summary>
        /// Scale handle will be displayed when this object is selected in the level editor if this property is true.
        /// </summary>
        public bool IsScaleable
        {
            get { return m_isScaleable; }
            set { m_isScaleable = value; }
        }

        /// <summary>
        /// Scale handle will have x axis if this property is true.
        /// </summary>
        public bool IsScaleableOnX
        {
            get { return m_isScaleableOnX; }
            set { m_isScaleableOnX = value; }
        }


        /// <summary>
        /// Scale handle will have y axis if this property is true.
        /// </summary>
        public bool IsScaleableOnY
        {
            get { return m_isScaleableOnY; }
            set { m_isScaleableOnY = value; }
        }

        /// <summary>
        /// Scale handle will have z axis if this property is true.
        /// </summary>
        public bool IsScaleableOnZ
        {
            get { return m_isScaleableOnZ; }
            set { m_isScaleableOnZ = value; }
        }
        

        public LE_ObjectEditHandle EditHandle
        {
            get { return m_editHandle; }
        }
        public InputManager.EnObjectEditSpace EditSpace
        {
            set { m_editSpace = value; }
            get { return m_editSpace; }
        }


        public InputManager.EnObjectEditMode EditMode
        {
            set { m_editMode = value; }
            get { return m_editMode; }
        }

        public void UpdateEditorHandle(){
         // remove edit handle if it is not needed any more
            if (m_editMode == InputManager.EnObjectEditMode.NO_EDIT)
            {
                if (m_editHandle != null)
                {
                    Destroy(m_editHandle.gameObject);
                }
            }
            // create edit handle if needed
            else if (m_editHandle == null || m_editHandle.EditMode != m_editMode)
            {
                if (m_editHandle != null)
                {
                    Destroy(m_editHandle.gameObject);
                }
                // check if this edit mode is suported
                if ((m_editMode == InputManager.EnObjectEditMode.SMART && m_isSmartMove) ||
                    (m_editMode == InputManager.EnObjectEditMode.MOVE && m_isMovable && (m_isMovableOnX || m_isMovableOnY || m_isMovableOnZ)) ||
                    (m_editMode == InputManager.EnObjectEditMode.ROTATE && m_isRotatable && (m_isRotatableAroundX || m_isRotatableAroundY || m_isRotatableAroundZ)) ||
                    (m_editMode == InputManager.EnObjectEditMode.SCALE && m_isScaleable && (m_isScaleableOnX || m_isScaleableOnY || m_isScaleableOnZ)))
                {
                    // create edit handle
                    string handlePostfix = m_editMode.ToString();

                    GameObject editHandleGO = (GameObject)Instantiate(Resources.Load("ObjectEditHandle" + handlePostfix), transform.position, transform.rotation);
                    m_editHandle = editHandleGO.GetComponent<LE_ObjectEditHandle>();
                    m_editHandle.Target = transform;
                    switch (m_editMode)
                    {
                        case InputManager.EnObjectEditMode.MOVE:
                            if (!m_isMovableOnX) { m_editHandle.DisableAxisX(); }
                            if (!m_isMovableOnY) { m_editHandle.DisableAxisY(); }
                            if (!m_isMovableOnZ) { m_editHandle.DisableAxisZ(); }
                            break;
                        case InputManager.EnObjectEditMode.ROTATE:
                            if (!m_isRotatableAroundX) { m_editHandle.DisableAxisX(); }
                            if (!m_isRotatableAroundY) { m_editHandle.DisableAxisY(); }
                            if (!m_isRotatableAroundZ) { m_editHandle.DisableAxisZ(); }
                            break;
                        case InputManager.EnObjectEditMode.SCALE:
                            if (!m_isScaleableOnX) { m_editHandle.DisableAxisX(); }
                            if (!m_isScaleableOnY) { m_editHandle.DisableAxisY(); }
                            if (!m_isScaleableOnZ) { m_editHandle.DisableAxisZ(); }
                            break;
                    }
                }
            }
            // set handle edit space
            if (m_editHandle != null)
            {
                m_editHandle.EditSpace = m_editSpace;
            }
        }
        */

        #endregion

        #region 标签

        private TagTip m_tagTip;

        /// <summary>
        /// 设置3D标签
        /// </summary>
        private void Set3DTag()
        {
            SsitApplication.Instance.CreateObject(null, DataItemProxy.c_sTagItemId, OnCreateTag, this);
        }

        /// <summary>
        /// 设置2D标签
        /// </summary>
        /// <param name="value"></param>
        private void Set2DTag( bool value )
        {
            if (value && m_tagTip == null)
            {
                m_tagTip = TagTipManager.Instance.GetTip();
                var paramList = m_baseObject.GetAttribute()?.ExtendParamList;
                if (paramList != null)
                    m_tagTip.SetTagInfo(gameObject, paramList[En_SceneObjectExParam.En_TagLogo],
                        paramList[En_SceneObjectExParam.En_TagName], paramList[En_SceneObjectExParam.En_TagDetail],
                        Vector3.zero);
            }
            else
            {
                m_tagTip.SetTagInfo(null);
            }
        }

        /// <summary>
        /// 标签创建回调
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="render"></param>
        /// <param name="data"></param>
        private void OnCreateTag( BaseObject obj, object render, object data )
        {
            var represent = obj.GetRepresent();
            represent.transform.SetParent(transform.Find("TagPos"), false);
            represent.SetActive(true);
            represent.transform.localPosition = Vector3.zero;
            var paramList = m_baseObject.GetAttribute()?.ExtendParamList;
            if (paramList != null && obj.SceneInstance is SceneTipInstance tipInstance)
            {
                tipInstance.SetValue(paramList[En_SceneObjectExParam.En_TagLogo],
                    paramList[En_SceneObjectExParam.En_TagName], paramList[En_SceneObjectExParam.En_TagDetail]);
            }
            SetColliderActive(false);
        }

        #endregion

        #region Trans

        public virtual void UpdateTransformInfo( Vector3 position, Vector3 eulerAngles, Vector3 scale )
        {
            transform.position = position;
            transform.eulerAngles = eulerAngles;
            transform.localScale = scale;
        }

        /// <summary>
        /// 移动改变回调
        /// </summary>
        /// <param name="lasPos">上个位置</param>
        /// <param name="tarPos">目标位置</param>
        public void DoMove( Vector3 lasPos, Vector3 tarPos )
        {
            // 产生变化再去同步
            if (Vector3.Distance(lasPos, tarPos) > 0)
            {
                if (GlobalManager.Instance.IsSync)
                    m_baseObject?.ChangeProperty(this, EnPropertyId.Position, tarPos.ToString());
                else
                    m_baseObject?.OnChangeProperty(this, EnPropertyId.Position, tarPos.ToString());
            }
        }

        public virtual void DoRotate( Quaternion rotate )
        {
            if (Quaternion.Angle(transform.rotation, rotate) > 0)
            {
                if (GlobalManager.Instance.IsSync)
                    m_baseObject?.ChangeProperty(this, EnPropertyId.Rotate, rotate.ToString());
                else
                    m_baseObject?.OnChangeProperty(this, EnPropertyId.Rotate, rotate.ToString());
            }
        }

        public virtual void DoScale( Vector3 scale )
        {
            if (transform.localScale == scale)
                return;

            if (GlobalManager.Instance.IsSync)
                m_baseObject?.ChangeProperty(this, EnPropertyId.Scale, scale.ToString());
            else
                m_baseObject?.OnChangeProperty(this, EnPropertyId.Scale, scale.ToString());
        }

        /// <summary>
        /// 具体移动方式//hack:优化方向：增加插值补偿运动，优化网络同步的走动现象
        /// </summary>
        /// <param name="pos"></param>
        public virtual void OnMove( Vector3 pos )
        {
            transform.position = pos;
        }

        public virtual void OnRotate( Quaternion rotate )
        {
            transform.rotation = rotate;
        }

        public virtual void OnScale( Vector3 scale )
        {
            transform.localScale = scale;
        }

        public Vector3 GetDirection()
        {
            return transform.forward;
        }

        public virtual Vector3 GetPosition()
        {
            return transform.position;
        }

        public virtual Vector3 GetPositionAroud()
        {
            return transform.position.Vec3ToAound();
        }

        public virtual Vector3 GetAngles()
        {
            return transform.eulerAngles;
        }

        public Quaternion GetOrientation()
        {
            return transform.rotation;
        }

        public virtual Vector3 GetScale()
        {
            return transform.localScale;
        }

        #endregion


        #region Extension

        public virtual void SetItemMaterial( GameObject item, Material material, Color color )
        {
            var itemRenderers = item.GetComponentsInChildren<Renderer>();
            for (var i = 0; i < itemRenderers.Length; i++)
                if (material != null)
                    for (var j = 0; j < itemRenderers[i].materials.Length; j++)
                    {
                        itemRenderers[i].materials[j] = material;
                        SetMaterial(itemRenderers[i].materials[j], color);
                    }
        }

        public virtual void SetMaterial( Material material, Color color )
        {
            if (material != null)
            {
                material.EnableKeyword("_EMISSION");

                if (material.HasProperty("_Color")) material.color = color;

                if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", color);
            }
        }

        #endregion
    }
}