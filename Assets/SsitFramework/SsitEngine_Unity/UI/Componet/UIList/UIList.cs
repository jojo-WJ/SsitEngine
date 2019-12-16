using System;
using System.Collections.Generic;
using SsitEngine.Data;
using SsitEngine.UI;
using UnityEngine;

namespace SsitEngine.Unity.UI
{
    [Obsolete("UIList is obsolete,using RecycleList Instead")]
    public class UIList : MonoBehaviour, IUIWidgit
    {
        protected string ElemetnGOName = "UIListElement";

        [SerializeField] private bool HasTitle;

        protected bool m_IsInit;

        protected UIListTitleElement m_TitleElement;
        protected UIListElement m_UIListElement;
        protected Transform m_UIListRoot;
        protected List<UIListElement> mElements;

        protected List<UIListTitleElement> mTitleElements;
        protected string RootGOName = "UIListRootElement";
        private BaseUIForm RootPanel;
        protected string TitleGOName = "UIListTitleElement";

        //private List<GameObject> mChildList;

        public virtual void Init( BaseUIForm root )
        {
            RootPanel = root;
            if (m_IsInit) return;
            if (HasTitle)
            {
                m_TitleElement = gameObject.GetChildNodeComponentScripts<UIListTitleElement>(TitleGOName);
                if (m_TitleElement != null)
                {
                    HasTitle = true;
                    m_TitleElement.Init();
                }
            }
            //mChildList = new List<GameObject>(10);
            if (mElements == null) mElements = new List<UIListElement>();

            if (mTitleElements == null) mTitleElements = new List<UIListTitleElement>();

            var elementObj = gameObject.Child(ElemetnGOName);
            if (elementObj != null)
            {
                elementObj.SetActive(false);
                m_UIListElement = gameObject.GetChildNodeComponentScripts<UIListElement>(ElemetnGOName);
            }

            m_UIListElement.Init();
            m_UIListRoot = gameObject.FindTheChildNode(RootGOName).transform;

            m_IsInit = true;
        }


        public virtual void ClearElement()
        {
            for (var i = 0; i < mElements.Count; i++)
                //TODO 对象池
                Destroy(mElements[i].gameObject);
            //mElements[i].gameObject.SetActive(false);

            //if (m_UIListRoot && m_UIListRoot.gameObject)
            //{
            //    m_UIListRoot.gameObject.ClearAllChild();
            //}

            for (var i = 0; i < mTitleElements.Count; i++)
                //TODO 对象池
                Destroy(mElements[i].gameObject);
            //mTitleElements[i].gameObject.SetActive(false);


            mElements.Clear();
            mTitleElements.Clear();
            AddTitleElement();
        }

        public virtual UIListElement CreateElement()
        {
            var listElementGO = Instantiate(m_UIListElement.gameObject);
            return listElementGO.GetComponent<UIListElement>();
        }

        public virtual UIListElement AddElement( IInfoData infoData )
        {
            var listElement = CreateElement();
            listElement.Init(m_UIListRoot.gameObject);
            listElement.SetInfo(infoData);
            listElement.gameObject.SetActive(true);

            listElement.transform.localScale = Vector3.one;

            mElements.Add(listElement);
            //mChildList.Add(listElementGO);
            return listElement;
        }

        public virtual UIListTitleElement CreateTitlElement()
        {
            var titleElementGO = Instantiate(m_TitleElement.gameObject);
            return titleElementGO.GetComponent<UIListTitleElement>();
        }

        public virtual UIListTitleElement AddTitleElement()
        {
            if (HasTitle)
            {
                var titleElement = CreateTitlElement();
                titleElement.gameObject.SetActive(true);
                titleElement.Init(m_UIListRoot.gameObject);
                titleElement.transform.localScale = Vector3.one;
                mTitleElements.Add(titleElement);
                return titleElement;
            }
            return null;
        }

        public virtual void DeleteElement( IInfoData infoData )
        {
            for (var i = 0; i < m_UIListRoot.childCount; i++)
            {
                var element = m_UIListRoot.GetChild(i).GetComponent<UIListElement>();
#if !MOBILE_INPUT
                // 2018-10-15 11:59:42 Shell Lee Todo
                // 有的表格有表头，但表头没有Element脚本
                // 表头或许该单独设计，不然获取数量跟清除子组件都有问题。
                if (element && element.InfoData.ID.Equals(infoData.ID))
#else
                if ( element &&
                     null != element.InfoData &&
                     !string.IsNullOrEmpty( element.InfoData.ID ) &&
                     !string.IsNullOrEmpty( infoData.ID ) &&
                     element.InfoData.ID.Equals( infoData.ID ) )
#endif
                {
                    //Destroy(element.gameObject);
                    element.gameObject.SetActive(false);
                    mElements.Remove(element);
                    break;
                }
            }
        }

        public virtual void UpdateElement<T>( IList<T> dataList ) where T : IInfoData
        {
            ClearElement();
            for (var i = 0; i < dataList.Count; i++) AddElement(dataList[i]);
        }

        public UIListElement GetElement( int num )
        {
            return m_UIListRoot.GetChild(num).GetComponent<UIListElement>();
        }

        public T GetElement<T>( Predicate<T> condition ) where T : UIListElement
        {
            return GetElements<T>().Find(condition);
        }

        public List<T> GetElements<T>() where T : UIListElement
        {
            return mElements.ConvertAll(x => { return x as T; });
        }

        public void ShowAllElement()
        {
            DisplayAllElement(true);
        }

        public void HideAllElement()
        {
            DisplayAllElement(false);
        }

        private void DisplayAllElement( bool enable )
        {
            for (var i = 0; i < m_UIListRoot.childCount; i++)
            {
                if (HasTitle && i == 0) continue;
                var element = m_UIListRoot.GetChild(i).GetComponent<UIListElement>();
                if (element) element.gameObject.SetActive(enable);
            }
        }

        private void Destroy()
        {
            for (var i = 0; i < mElements.Count; i++) Destroy(mElements[i]);

            for (var i = 0; i < mTitleElements.Count; i++) Destroy(mTitleElements[i]);
            mElements.Clear();
            mTitleElements.Clear();
        }
    }
}