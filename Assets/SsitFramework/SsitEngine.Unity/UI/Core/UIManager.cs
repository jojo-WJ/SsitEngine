/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：UI模块全局管理类                                                    
*│　作   者：Jusam                                       
*│　版   本：1.0.0                                                 
*│　创建时间：2019/04/29                             
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;
using SsitEngine.Core.Algorithm;
using SsitEngine.DebugLog;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.Unity.Data;
using SsitEngine.Unity.Resource;
using SsitEngine.Unity.Resource.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SsitEngine.Unity.UI
{
    /// <summary>
    ///     UI界面的加载参数
    /// </summary>
    public class UIParam
    {
        public int formId; //窗口id
        public bool isAsync; //异步加载标记
        public UnityAction<BaseUIForm> OnCloseCallBack;

        public UnityAction<BaseUIForm> OnOpenCallBack;
    }

    /// <summary>
    ///     UI模块管理类
    /// </summary>
    public class UIManager : ManagerBase<UIManager>
    {
        private const string c_sUIPrefabPath = "UI/";

        /// <summary>Info of layers and forms.</summary>
        private readonly Dictionary<UIFormType, Transform> layerForms = new Dictionary<UIFormType, Transform>();

        /// <summary>
        ///     当前显示的UI窗体集合
        /// </summary>
        private Dictionary<int, BaseUIForm> _DicCurrentShowUIForms;

        /// <summary>
        ///     当前反向切换窗体集合。
        /// </summary>
        private Stack<BaseUIForm> _StackCurrentUIForms;

        /// <summary>
        ///     UI窗体Canvas根节点
        /// </summary>
        private Transform _TraCanvasTransform;

        /// <summary>
        ///     缓存所有UI窗体集合
        /// </summary>
        private LRUCache<int, BaseUIForm> m_AllUIFormsCache;

        /// <summary>
        ///     UI管理脚本节点
        /// </summary>
        private BaseUIForm m_CurSingleUIForm;

        private List<int> m_OpenUIFormCahce;

        /// <summary>Canvas component.</summary>
        public Canvas Canvas { private set; get; }

        /// <summary>CanvasScaler component.</summary>
        public CanvasScaler CanvasScaler { private set; get; }

        /// <summary>GraphicRaycaster component.</summary>
        public GraphicRaycaster GraphicRaycaster { private set; get; }


        /// <summary>Layers for custom form.</summary>
        public string[] Layers { private set; get; }

        /// <summary>
        ///     显示界面
        /// </summary>
        /// <param name="loadParam">UI面板的ID标识，参考于UITable 配置表中的ID</param>
        /// <param name="nParam">参数</param>
        private void ShowForm( UIParam loadParam, params object[] nParam )
        {
            m_OpenUIFormCahce.Add(loadParam.formId);

            ShowUIForms(loadParam, tempUIForm =>
            {
                if (tempUIForm == null)
                {
                    return;
                }

                if (tempUIForm.CurrentUIType.UIForm_SHowMode == UIFormSHowMode.Single)
                {
                    if (m_CurSingleUIForm != null)
                    {
                        var uiName = GetUINameByForm(m_CurSingleUIForm);
                        //var sameTypeUiForms = GetUIFormsByType(tempUIForm);
                        if (uiName > 0)
                        {
                            CloseForm(uiName);
                        }
                        //foreach (var form in sameTypeUiForms)
                        //{
                        //    CloseForm(form);
                        //}
                    }
                    m_CurSingleUIForm = tempUIForm;
                }
            }, nParam);
        }

        /// <summary>
        ///     关闭界面
        /// </summary>
        /// <param name="formId">UI面板的ID标识，参考于UITable 配置表中的ID</param>
        private void CloseForm( int formId )
        {
            var keyList = _DicCurrentShowUIForms.Keys.ToList();
            IUIData data;
            for (var i = 0; i < keyList.Count; i++)
            {
                data = DataManager.Instance.GetData<IUIData>((int) EnDataType.DATA_UI, keyList[i]);
                if (data.Id != formId && data.GroupId == formId)
                {
                    if (IsShowForm(data.Id))
                    {
                        ForEachCloseSubUIForm(keyList, data.Id);

                        var temp = CloseUIForms(data.Id);
                        if (temp != null)
                        {
                            if (temp.CurrentUIType.UIForm_SHowMode == UIFormSHowMode.Single)
                            {
                                m_CurSingleUIForm = null;
                            }
                        }
                    }
                }
            }

            var tempUIForm = CloseUIForms(formId);
            if (tempUIForm != null)
            {
                if (tempUIForm.CurrentUIType.UIForm_SHowMode == UIFormSHowMode.Single)
                {
                    m_CurSingleUIForm = null;
                }
            }
        }

        private void ForEachCloseSubUIForm( List<int> keyList, int formId )
        {
            IUIData data;
            for (var i = 0; i < keyList.Count; i++)
            {
                data = DataManager.Instance.GetData<IUIData>((int) EnDataType.DATA_UI, keyList[i]);
                if (data.Id != formId && data.GroupId == formId)
                {
                    if (IsShowForm(data.Id))
                    {
                        ForEachCloseSubUIForm(keyList, data.Id);

                        var temp = CloseUIForms(data.Id);
                        if (temp != null)
                        {
                            if (temp.CurrentUIType.UIForm_SHowMode == UIFormSHowMode.Single)
                            {
                                m_CurSingleUIForm = null;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        internal Canvas GetCanvas()
        {
            return _TraCanvasTransform.GetComponent<Canvas>();
        }

        #region private func

        /// <summary>
        ///     初始化加载根UI窗体Canvas预设
        /// </summary>
        private void InitRootCanvasLoading( UnityAction complete )
        {
            //ResourcesManager.Instance.LoadUI(0, false, ( GameObject go ) =>
            // {
            //     GameObject goClone = Instantiate(go);
            //     goClone.name = goClone.name.Replace("(Clone)", "");
            //     complete.Invoke();
            // }
            //);

            Engine.Instance.Platform.InitRootCanvasLoading(complete);
        }

        /// <summary>
        ///     显示打开UI窗体
        ///     1.根据UI窗体名称，加载到_DicAllUIFOrms中
        ///     2.根据不同窗体的显示模式，分别做不同的加载处理
        /// </summary>
        /// <param name="loadParam">UI面板的ID标识，参考于UITable 配置表中的ID</param>
        /// <returns>成功打开的面板的BaseUIForm基类，打开失败返回null</returns>
        private void ShowUIForms( UIParam loadParam, UnityAction<BaseUIForm> complete, params object[] nParam )
        {
            // 参数的检查
            if (loadParam.formId < 1)
            {
                complete.Invoke(null);
            }
            // 根据UI窗体名称，获取窗体go

            var baseUIForms = m_AllUIFormsCache.Get(loadParam.formId);
            if (baseUIForms == null)
            {
                LoadUIForm(loadParam, uiForms => { ShowUIFormCallBack(loadParam, uiForms, complete, nParam); });
            }
            else
            {
                ShowUIFormCallBack(loadParam, baseUIForms, complete, nParam);
            }
        }

        private void ShowUIFormCallBack( UIParam param, BaseUIForm baseUIForms, UnityAction<BaseUIForm> complete,
            params object[] nParam )
        {
            if (baseUIForms == null)
            {
                SsitDebug.Error("The UIForm " + param.formId + " is not exist!!!");
                complete.Invoke(null);
            }
            else
            {
                //清空反向切换窗体栈集合
                if (baseUIForms.CurrentUIType.IsClearStack)
                {
                    ClearStackArray();
                }

                //   2.根据不同窗体的显示模式，分别做不同的加载处理
                switch (baseUIForms.CurrentUIType.UIForm_SHowMode)
                {
                    case UIFormSHowMode.Normal:
                        LoadUIFormsToCurrentCache(param, nParam);
                        break;
                    case UIFormSHowMode.HideOther:
                        EnterUIFormAndHiderOthers(param, nParam);
                        break;
                    case UIFormSHowMode.ReverseChange:
                        PushUIFormToStack(param, nParam);
                        break;
                    case UIFormSHowMode.Single:
                        LoadUIFormsToCurrentCache(param, nParam);
                        break;
                }
                m_OpenUIFormCahce.Remove(param.formId);
                complete.Invoke(baseUIForms);
            }
        }

        /// <summary>
        ///     关闭UI窗体
        /// </summary>
        /// <param name="uiFormId">UI面板的ID标识，参考于UITable 配置表中的ID</param>
        /// <returns>成功关闭的面板的BaseUIForm基类，关闭失败返回null</returns>
        private BaseUIForm CloseUIForms( int uiFormId )
        {
            BaseUIForm baseUIForm = null;
            if (uiFormId < 1)
            {
                return baseUIForm;
            }
            //   所有UI窗体集合中是否存在，不存在直接返回
            baseUIForm = m_AllUIFormsCache.Get(uiFormId);
            //_DicAllUIForms.TryGetValue(UIFormName, out baseUIForm);
            if (baseUIForm == null)
            {
                return baseUIForm;
            }

            //   2.根据不同窗体的显示模式，分别做不同的关闭处理
            switch (baseUIForm.CurrentUIType.UIForm_SHowMode)
            {
                case UIFormSHowMode.Normal:
                    ExitUIForms(uiFormId);
                    break;
                case UIFormSHowMode.HideOther:
                    ExitUIFormAndDiplayOthers(uiFormId);
                    break;
                case UIFormSHowMode.ReverseChange:
                    PopUIForms();
                    break;
                case UIFormSHowMode.Single:
                    ExitUIForms(uiFormId);
                    break;
            }
            return baseUIForm;
        }

        /// <summary>
        ///     加载UI窗体
        /// </summary>
        /// <param name="loadParam">UI面板的ID标识，参考于UITable 配置表中的ID</param>
        /// <returns>成功加载的面板的BaseUIForm基类，加载失败返回null</returns>
        private void LoadUIForm( UIParam loadParam, UnityAction<BaseUIForm> complete )
        {
            GameObject goCloneUIPrefabs = null; //创建的UI克隆体预设
            BaseUIForm baseUiForm = null; //窗体基类

            //根据“UI窗体名称”，加载“预设克隆体”

            if (loadParam.isAsync)
            {
                Engine.Instance.Platform.OpenLoadingForm();
            }

            //TODO 资源加载
            ResourcesManager.Instance.LoadAsset<GameObject>(loadParam.formId, loadParam.isAsync, go =>
            {
                goCloneUIPrefabs = Instantiate(go);
                goCloneUIPrefabs.name = goCloneUIPrefabs.name.Replace("(Clone)", "");

                //设置“UI克隆体”的父节点（根据克隆体中带的脚本中不同的“位置信息”）
                if (_TraCanvasTransform != null && goCloneUIPrefabs != null)
                {
                    baseUiForm = goCloneUIPrefabs.GetComponent<BaseUIForm>();
                    if (baseUiForm == null)
                    {
                        Debug.Log("baseUiForm==null! ,请先确认窗体预设对象上是否加载了baseUIForm的子类脚本！ 参数 uiFormId = " +
                                  loadParam.formId);
                        complete.Invoke(null);
                        return;
                    }

                    var data = DataManager.Instance.GetData<IUIData>((int) EnDataType.DATA_UI, loadParam.formId);
                    if (data != null)
                    {
                        baseUiForm.CurrentUIType.UIForm_SHowMode = (UIFormSHowMode) data.UIShowMode;
                        baseUiForm.CurrentUIType.UIForm_Type = (UIFormType) data.UIShowType;
                        //baseUiForm.CurrentUIType.UIForm_LucencyType = (UIFormLucencyType)data.UILucencyType;
                    }

                    baseUiForm.Init();

                    if (loadParam.isAsync)
                    {
                        Engine.Instance.Platform.CloseLoadingForm();
                    }

                    Transform node = null;
                    layerForms.TryGetValue(baseUiForm.CurrentUIType.UIForm_Type, out node);

                    if (node == null)
                    {
                        SsitDebug.Error($"UIForm node is exception{baseUiForm.CurrentUIType.UIForm_Type}");
                        return;
                    }

                    goCloneUIPrefabs.transform.SetParent(node, false);
                    goCloneUIPrefabs.transform.SetAsLastSibling();
                    //设置隐藏
                    goCloneUIPrefabs.SetActive(false);
                    //把克隆体，加入到“所有UI窗体”（缓存）集合中。
                    //TODO LRU缓存机制
                    m_AllUIFormsCache.Add(loadParam.formId, baseUiForm);
                    complete.Invoke(baseUiForm);
                }
                else
                {
                    Debug.Log("_TraCanvasTransfrom==null Or goCloneUIPrefabs==null!! ,Plese Check!, 参数uiFormId=" +
                              loadParam.formId);
                    complete.Invoke(null);
                }

                //Debug.Log("出现不可以预估的错误，请检查，参数 uiFormId=" + uiFormId);
                //complete.Invoke(null);
            });
        }

        /// <summary>
        ///     将UI面板添加入当前显示的UI面板集合
        /// </summary>
        /// <param name="loadParam">UI面板的ID标识，参考于UITable 配置表中的ID</param>
        private void LoadUIFormsToCurrentCache( UIParam loadParam, params object[] nParam )
        {
            //BaseUIForm baseUIForm = null;
            BaseUIForm baseUIFormFromAllCache; //从所有窗体集合中得到的窗体
            //如果正在显示的集合中，存在整个UI窗体，则直接返回
            //if (baseUIForm != null) return;
            //把当前窗体，加载到正在显示集合中
            baseUIFormFromAllCache = m_AllUIFormsCache.Get(loadParam.formId);
            if (baseUIFormFromAllCache != null)
            {
                _DicCurrentShowUIForms.Add(loadParam.formId, baseUIFormFromAllCache);
                baseUIFormFromAllCache.AddListener(loadParam);
                baseUIFormFromAllCache.Display(nParam);
            }
        }

        /// <summary>
        ///     UI窗体入栈
        /// </summary>
        /// <param name="loadParam">UI面板的ID标识，参考于UITable 配置表中的ID</param>
        private void PushUIFormToStack( UIParam loadParam, params object[] nParam )
        {
            BaseUIForm baseUiForm;
            //判断栈内是否存在窗体，若存在则将其他窗体冻结处理
            if (_StackCurrentUIForms.Count > 0)
            {
                var topUIForm = _StackCurrentUIForms.Peek();
                topUIForm.Freeze();
            }
            //判断所有UI窗体集合中是否有当前窗体，有则处理
            baseUiForm = m_AllUIFormsCache.Get(loadParam.formId);
            if (baseUiForm != null)
            {
                baseUiForm.AddListener(loadParam);
                baseUiForm.Display(nParam);
                //把指定的UI窗体加入栈中
                _StackCurrentUIForms.Push(baseUiForm);
            }
            else
            {
                Debug.Log("baseUIForm=null,请检查！");
            }
        }

        /// <summary>
        ///     将UI面板从当前显示的UI面板集合中移除
        /// </summary>
        /// <param name="uiFormId">UI面板的ID标识，参考于UITable 配置表中的ID</param>
        private void ExitUIForms( int uiFormId )
        {
            BaseUIForm baseUIForm;
            _DicCurrentShowUIForms.TryGetValue(uiFormId, out baseUIForm);
            if (baseUIForm == null)
            {
                return;
            }
            baseUIForm.Hiding();
            _DicCurrentShowUIForms.Remove(uiFormId);
        }

        /// <summary>
        ///     将面板从UI序列栈中移除
        /// </summary>
        private void PopUIForms()
        {
            BaseUIForm topUIForm;

            if (_StackCurrentUIForms.Count >= 2)
            {
                topUIForm = _StackCurrentUIForms.Pop();
                topUIForm.Hiding();

                var nextUIForm = _StackCurrentUIForms.Peek();
                nextUIForm.Redisplay();
            }
            else if (_StackCurrentUIForms.Count == 1)
            {
                topUIForm = _StackCurrentUIForms.Pop();
                topUIForm.Hiding();
            }
        }


        /// <summary>
        ///     显示目标面板并隐藏其他面板
        /// </summary>
        /// <param name="loadParam">目标UI面板的ID标识，参考于UITable 配置表中的ID</param>
        private void EnterUIFormAndHiderOthers( UIParam loadParam, params object[] nParam )
        {
            if (loadParam.formId < 1)
            {
                return;
            }
            _DicCurrentShowUIForms.TryGetValue(loadParam.formId, out var baseUIFormCurrent);
            if (baseUIFormCurrent != null)
            {
                return;
            }
            foreach (var baseUI in _DicCurrentShowUIForms.Values)
            {
                baseUI.Hiding();
            }
            foreach (var baseUI in _StackCurrentUIForms)
            {
                baseUI.Hiding();
            }

            var baseUIFormFromAllCache = m_AllUIFormsCache.Get(loadParam.formId);
            if (baseUIFormFromAllCache != null)
            {
                _DicCurrentShowUIForms.Add(loadParam.formId, baseUIFormFromAllCache);
                baseUIFormFromAllCache.AddListener(loadParam);
                baseUIFormFromAllCache.Display(nParam);
            }
        }


        /// <summary>
        ///     移除目标面板，并显示其他面板
        /// </summary>
        /// <param name="uiFormId">目标UI面板的ID标识，参考于UITable 配置表中的ID</param>
        private void ExitUIFormAndDiplayOthers( int uiFormId )
        {
            if (uiFormId < 1)
            {
                return;
            }
            _DicCurrentShowUIForms.TryGetValue(uiFormId, out var baseUIForm);
            if (baseUIForm == null)
            {
                return;
            }
            //当前窗体隐藏，并在正在显示窗体集合中移除
            baseUIForm.Hiding();
            _DicCurrentShowUIForms.Remove(uiFormId);

            //当前显示窗体集合和栈集合中窗体全部显示
            foreach (var baseUI in _DicCurrentShowUIForms.Values)
            {
                baseUI.Redisplay();
            }
            foreach (var baseUI in _StackCurrentUIForms)
            {
                baseUI.Redisplay();
            }
        }

        /// <summary>
        ///     清空反向切换窗体栈集合
        /// </summary>
        private void ClearStackArray()
        {
            if (_StackCurrentUIForms != null && _StackCurrentUIForms.Count >= 1)
            {
                _StackCurrentUIForms.Clear();
            }
        }

        /// <summary>
        ///     移除UI面板
        /// </summary>
        /// <param name="uiFormId">目标UI面板的ID标识，参考于UITable 配置表中的ID</param>
        /// <returns>移除面板是否成功</returns>
        private bool RemoveForm( int uiFormId )
        {
            //当前显示窗体集合和栈集合中窗体全部显示
            //参数的检查
            if (uiFormId < 1)
            {
                return false;
            }
            //   1.根据UI窗体名称，获取窗体go
            var baseUIForms = m_AllUIFormsCache.Get(uiFormId);
            if (baseUIForms == null)
            {
                return false;
            }
            if (baseUIForms.CurrentUIType.UIForm_Type == UIFormType.PopUp)
            {
                ClearStackArray();
            }
            //_DicAllUIForms.Remove(uiFormName);
            return true;
        }

        /// <summary>
        ///     移除所有面板
        /// </summary>
        private void RemoveAllForm()
        {
            //foreach (var item in m_UIFormMediatorDic)
            //{
            //    GameObject.Destroy(item.Value.ViewComponent as GameObject);
            //    //Facade.RemoveMediator( item.Value.MediatorName );
            //}
            //m_UIFormMediatorDic.Clear();
        }

        #endregion

        #region public func

        /// <summary>
        ///     UI面板是否显示
        /// </summary>
        /// <param name="formId">UI面板的ID标识，参考于UITable 配置表中的ID</param>
        /// <returns>如果返回true 则面板处于显示中</returns>
        public bool IsShowForm( int formId )
        {
            return _DicCurrentShowUIForms.ContainsKey(formId);
        }

        /// <summary>
        ///     获取面板ID
        /// </summary>
        /// <param name="uiform">派生自BaseUIForm的UI面板类</param>
        /// <returns>UI面板的ID标识，参考于UITable 配置表中的ID</returns>
        public int GetUINameByForm( BaseUIForm uiform )
        {
            return _DicCurrentShowUIForms.FirstOrDefault(q => q.Value.Equals(uiform)).Key;
        }

        /// <summary>
        ///     获取面板ID
        /// </summary>
        /// <param name="uiform">派生自BaseUIForm的UI面板类</param>
        /// <returns>UI面板的ID标识，参考于UITable 配置表中的ID</returns>
        public List<int> GetUIFormsByType( BaseUIForm uiform )
        {
            var ret = new List<int>();
            foreach (var item in _DicCurrentShowUIForms)
            {
                if (item.Value.CurrentUIType.UIForm_Type == uiform.CurrentUIType.UIForm_Type)
                {
                    ret.Add(item.Key);
                }
            }
            return ret;
        }

        #endregion

        #region 模块管理

        /// <summary>
        ///     模块初始化操作
        /// </summary>
        public override void OnSingletonInit()
        {
            m_AllUIFormsCache = new LRUCache<int, BaseUIForm>(50);
            //_DicAllUIForms = new Dictionary<string, BaseUIForm>();
            _DicCurrentShowUIForms = new Dictionary<int, BaseUIForm>();
            _StackCurrentUIForms = new Stack<BaseUIForm>();
            m_OpenUIFormCahce = new List<int>(50);

            Layers = ReadSettings().layers.ToArray();

            //初始化加载根UI窗体Canvas预设
            InitRootCanvasLoading(() =>
            {
                //得到UI根节点、全屏节点、固定节点、弹出节点
                _TraCanvasTransform = GameObject.FindGameObjectWithTag(SysDefine.SYS_CANVAS_TAG).transform;
                Canvas = _TraCanvasTransform.GetComponent<Canvas>();
                Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler = _TraCanvasTransform.GetComponent<CanvasScaler>();
                GraphicRaycaster = _TraCanvasTransform.GetComponent<GraphicRaycaster>();
                CreateLayerRoots(Layers, _TraCanvasTransform);

                /*_TraNormal = _TraCanvasTransform.gameObject.FindTheChildNode(SysDefine.SYS_NORMAL_NODE).transform;
                _TraFixed = _TraCanvasTransform.gameObject.FindTheChildNode(SysDefine.SYS_FIXED_NODE).transform;
                _TraPopUp = _TraCanvasTransform.gameObject.FindTheChildNode(SysDefine.SYS_POPOUP_NODE).transform;
                _TraUIScripts = UnityHelper.FindTheChildNode( _TraCanvasTransform.gameObject, SysDefine.SYS_SCRIPTMANAGER_NODE );
                把本脚本作为“根UI窗体”的子节点
                this.gameObject.transform.SetParent( _TraCanvasTransform, false );
                */
                //根UI窗体在场景转换时不允许销毁
                DontDestroyOnLoad(_TraCanvasTransform);
                RegisterUIMsg();
            });
        }

        /// <summary>Read settings from local file.</summary>
        /// <returns>Settings of UI form.</returns>
        private UIFormSettings ReadSettings()
        {
            var uiFormSettings = Resources.Load<UIFormSettings>("UI/Settings/UIFormSettings");
            if (uiFormSettings == null)
            {
                SsitDebug.Error(
                    "Read settings error: Can not load settings from file at path Assets/Resources/{0}.asset.",
                    "UI/Settings/UIFormSettings");
                uiFormSettings = ScriptableObject.CreateInstance<UIFormSettings>();
            }
            return uiFormSettings;
        }

        /// <summary>Create root for layers.</summary>
        /// <param name="layers">UI form layers.</param>
        /// <param name="canvas">UI form canvas.</param>
        private void CreateLayerRoots( string[] layers, Transform canvas )
        {
            for (var i = 0; i < layers.Length; i++)
            {
                var layer = layers[i];
                if (string.IsNullOrEmpty(layer))
                {
                    SsitDebug.Error("Create layer root error: The name of layer is null or empty.");
                }
                else
                {
                    var lay = canvas.Find(layer);

                    if (lay != null && lay.GetSiblingIndex() != i + 1)
                    {
                        lay.SetSiblingIndex(i);
                    }
                    else
                    {
                        var go = new GameObject(layer);
                        var rectTransform = go.AddComponent<RectTransform>();
                        go.layer = gameObject.layer;
                        rectTransform.SetParent(canvas, false);
                        rectTransform.anchorMin = Vector2.zero;
                        rectTransform.anchorMax = Vector2.one;
                        rectTransform.offsetMin = Vector2.zero;
                        rectTransform.offsetMax = Vector2.zero;
                        rectTransform.localScale = Vector3.one;
                        lay = go.transform;
                        lay.SetSiblingIndex(i);
                    }
                    layerForms.Add((UIFormType) i, lay);
                }
            }
        }

        /// <summary>
        ///     模块名字
        /// </summary>
        public override string ModuleName => typeof(UIManager).FullName;

        /// <summary>
        ///     模块的优先级
        /// </summary>
        public override int Priority => 7;

        /// <summary>
        ///     固定显示的节点
        /// </summary>
        public Transform GetCanvasNode( UIFormType formType )
        {
            Transform trans = null;
            layerForms.TryGetValue(formType, out trans);
            return trans;
        }


        /// <summary>
        ///     UI模块刷新操作
        /// </summary>
        /// <param name="elapseSeconds"></param>
        public override void OnUpdate( float elapseSeconds )
        {
            //todo:唤醒销毁机制
        }

        /// <summary>
        ///     退出模块回调
        /// </summary>
        public override void Shutdown()
        {
            if (isShutdown)
            {
                return;
            }
            UnRegisterMsg(m_msgList);

            RemoveAllForm();
            isShutdown = true;
        }

        #endregion


        #region monoBase

        /// <summary>
        ///     注册消息列表
        /// </summary>
        private void RegisterUIMsg()
        {
            m_msgList = new ushort[2]
            {
                (ushort) UIMsg.OpenForm,
                (ushort) UIMsg.CloseForm
            };
            RegisterMsg(m_msgList);
        }

        /// <summary>
        ///     执行消息事件
        /// </summary>
        /// <param name="notification"></param>
        public override void HandleNotification( INotification notification )
        {
            var _formId = -1;
            UIParam loadParam = null;
            var args = notification as MvEventArgs;
            switch (args.Id)
            {
                case (ushort) UIMsg.OpenForm:
                    if (args.Body is UIParam param)
                    {
                        loadParam = param;
                        _formId = param.formId;
                    }
                    else
                    {
                        _formId = (int) args.Body;

                        loadParam = new UIParam {formId = _formId, isAsync = true};
                    }

                    if (_formId < 1)
                    {
                        return;
                    }

                    if (m_OpenUIFormCahce.Contains(_formId) || IsShowForm(_formId))
                    {
                        return;
                    }
                    ShowForm(loadParam, args.Values);

                    break;
                case (ushort) UIMsg.CloseForm:
                    _formId = (int) notification.Body;

                    if (_formId < 1)
                    {
                        break;
                    }

                    if (!IsShowForm(_formId))
                    {
                        return;
                    }
                    CloseForm(_formId);

                    break;
            }
        }

        private void OpenLoadingForm( int _formId )
        {
            if (m_OpenUIFormCahce.Contains(_formId) || IsShowForm(_formId))
            {
                return;
            }
            ShowForm(new UIParam {formId = _formId, isAsync = false});
        }

        #endregion
    }
}