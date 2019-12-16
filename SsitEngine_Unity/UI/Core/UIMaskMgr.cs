/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：UI Mask遮罩管理器                                                    
*│　作   者：Jusam                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/04/29                             
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;
using UnityEngine.UI;

namespace SsitEngine.Unity.UI
{
    /// <summary>
    ///     UI Mask遮罩管理类
    /// </summary>
    public class UIMaskMgr : MonoBehaviour
    {
        /// <summary>
        ///     本脚本私有单例
        /// </summary>
        private static UIMaskMgr _Instance;

        /// <summary>
        ///     UI根节点对象
        /// </summary>
        private GameObject _GoCanvasRoot;

        /// <summary>
        ///     遮罩面板
        /// </summary>
        private GameObject _GoMaskPanel;

        /// <summary>
        ///     顶层面板
        /// </summary>
        private GameObject _GoTopPanel;

        /// <summary>
        ///     UI摄像机原始的层深
        /// </summary>
        private float _OriginalUICameraDepth;

        /// <summary>
        ///     UI脚本节点对象
        /// </summary>
        private Transform _TraUIScriptNode;

        /// <summary>
        ///     UI摄像机
        /// </summary>
        private Camera _UICamera;

        /// <summary>
        ///     得到实例方法
        /// </summary>
        /// <returns></returns>
        public static UIMaskMgr GetInstance()
        {
            if (_Instance == null) _Instance = new GameObject("_UIMaskMgr").AddComponent<UIMaskMgr>();
            return _Instance;
        }

        /// <summary>
        ///     初始化
        /// </summary>
        public void Awake()
        {
            //得到根节点对象，脚本节点对象
            _GoCanvasRoot = GameObject.FindGameObjectWithTag(SysDefine.SYS_CANVAS_TAG);
            _TraUIScriptNode = _GoCanvasRoot.FindTheChildNode(SysDefine.SYS_SCRIPTMANAGER_NODE).transform;
            ////把本脚本实例，作为脚本节点对象的子节点
            transform.AddChildNodeToParentNode(_TraUIScriptNode);
            //UnityHelper.AddChildNodeToParentNode(_TraUIScriptNode, this.gameObject.transform);
            ////得到顶层面板，遮罩面板
            _GoTopPanel = _GoCanvasRoot;
            _GoMaskPanel = _GoCanvasRoot.FindTheChildNode("_UIMaskPanel").gameObject;
            ////得到UI相机原始的层深
            _UICamera = _GoCanvasRoot.GetChildNodeComponentScripts<Camera>("UICamera");
            if (_UICamera != null)
                //得到UICamer原始层深
                _OriginalUICameraDepth = _UICamera.depth;
            else
                Debug.Log(GetType() + "Start()/UI_Camera is NUll,Please Check!");
        }

        /// <summary>
        ///     设置遮罩状态
        /// </summary>
        public void SetMaskWindow( GameObject goDisplayUIForms,
            UIFormLucencyType lucenyType = UIFormLucencyType.Lucency )
        {
            //顶层遮罩下移
            _GoTopPanel.transform.SetAsLastSibling();
            //启用遮罩窗体及设置透明度
            switch (lucenyType)
            {
                //完全透明，不能穿透
                case UIFormLucencyType.Lucency:
                    //LogManager.Info("完全透明");
                    _GoMaskPanel.SetActive(true);
                    var newColor1 = new Color(0, 0, 0, 0);
                    _GoMaskPanel.GetComponent<Image>().color = newColor1;
                    break;
                //半透明，不能穿透
                case UIFormLucencyType.Translucence:
                    //LogManager.Info("半透明");
                    _GoMaskPanel.SetActive(true);
                    var newColor2 = new Color(220 / 255F, 220 / 255F, 220 / 255F, 50 / 255f);
                    _GoMaskPanel.GetComponent<Image>().color = newColor2;
                    break;
                //低透明，不能穿透
                case UIFormLucencyType.Impenetrable:
                    //LogManager.Info("低透明");
                    _GoMaskPanel.SetActive(true);
                    var newColor3 = new Color(50 / 255F, 50 / 255F, 50 / 255F, 200 / 255f);
                    _GoMaskPanel.GetComponent<Image>().color = newColor3;
                    break;
                case UIFormLucencyType.Penetrate:
                    //LogManager.Info("允许穿透");
                    if (_GoMaskPanel.activeInHierarchy) _GoMaskPanel.SetActive(false);
                    break;
            }
            //遮罩体下移
            _GoMaskPanel.transform.SetAsLastSibling();
            //显示窗体下移
            goDisplayUIForms.transform.SetAsLastSibling();
            if (_UICamera != null) _UICamera.depth = _UICamera.depth + 100; //增加层深
        }

        /// <summary>
        ///     取消遮罩状态
        /// </summary>
        public void CancelMaskWindow()
        {
            //顶层窗体上移
            _GoTopPanel.transform.SetAsFirstSibling();
            //禁用遮罩窗体
            if (_GoMaskPanel.activeInHierarchy) _GoMaskPanel.SetActive(false);
            if (_UICamera != null) _UICamera.depth = _OriginalUICameraDepth;
        }
    }
}