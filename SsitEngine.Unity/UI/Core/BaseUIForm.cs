/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：UI窗体面板Form基类                                                    
*│　作   者：Jusam                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/04/29                             
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;
using UnityEngine.Events;

namespace SsitEngine.Unity.UI
{
    /// <summary>
    ///     UI面板基类
    /// </summary>
    public class BaseUIForm : UIMonoBase
    {
        /// <summary>
        ///     当前UI面板的显示状态
        /// </summary>
        private UIType _CurrentUIType = new UIType();

        public UnityAction<BaseUIForm> OnCloseCallBack;

        //callback 
        public UnityAction<BaseUIForm> OnOpenCallBack;

        /// <summary>
        ///     当前UI窗体类型
        /// </summary>
        public UIType CurrentUIType
        {
            get => _CurrentUIType;
            set => _CurrentUIType = value;
        }


        public void AddListener( UIParam loadParam )
        {
            OnOpenCallBack = loadParam.OnOpenCallBack;
            OnCloseCallBack = loadParam.OnCloseCallBack;
        }

        //public string ViewName { get; set; }

        //public IMediator Mediator { get; set; }


        //public T GetOverLay<T>() where T : IBaseView
        //{
        //    return this.GetComponent<T>();
        //}

        #region Interval func

        /// <summary>
        ///     面板加载时的初始化工作
        /// </summary>
        private void Awake()
        {
            Initial();
            InitUIWidght(this);
        }


        /// <summary>
        ///     隐藏后再显示
        /// </summary>
        public virtual void Redisplay()
        {
            gameObject.SetActive(true);
            RegisterMsg(m_msgList);

            if (_CurrentUIType.UIForm_Type == UIFormType.PopUp)
            {
                UIMaskMgr.GetInstance().SetMaskWindow(gameObject, CurrentUIType.UIForm_LucencyType);
            }
        }

        /// <summary>
        ///     冻结状态
        /// </summary>
        internal void Freeze()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        ///     界面销毁时的回调
        /// </summary>
        private void OnDestroy()
        {
            OnOpenCallBack = null;
            OnCloseCallBack = null;
            Destroy();
            base.Destroy();
        }

        #endregion


        #region Public func

        /// <summary>
        ///     界面加载时初始化工作
        /// </summary>
        public virtual void Init()
        {
        }

        /// <summary>
        ///     界面显示回调
        /// </summary>
        public virtual void Display( params object[] nParam )
        {
            gameObject.SetActive(true);

            RegisterMsg(m_msgList);
            if (_CurrentUIType.UIForm_Type == UIFormType.PopUp)
            {
                UIMaskMgr.GetInstance().SetMaskWindow(gameObject, CurrentUIType.UIForm_LucencyType);
            }

            OnOpenCallBack?.Invoke(this);
        }

        /// <summary>
        ///     界面隐藏回调
        /// </summary>
        public virtual void Hiding()
        {
            UnRegisterMsg(m_msgList);
            if (_CurrentUIType.UIForm_Type == UIFormType.PopUp)
            {
                UIMaskMgr.GetInstance().CancelMaskWindow();
            }
            OnCloseCallBack?.Invoke(this);
        }

        /// <summary>
        ///     程序关闭时或界面销毁时的回调函数
        /// </summary>
        public virtual void Destroy()
        {
        }

        public GameObject GetGameObject( string name )
        {
            return base.GetGameObject(name);
        }

        public T GetUIComponet<T>( string name ) where T : Component
        {
            return base.GetUIComponet<T>(name);
        }

        public RectTransform GetUITransform( string name )
        {
            return base.GetUITransform(name);
        }

        #endregion
    }
}