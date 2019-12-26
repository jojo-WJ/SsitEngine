using System;
using System.Collections.Generic;
using SsitEngine.Data;
using SsitEngine.UI;
using SsitEngine.Unity.Pool;
using UnityEngine;

namespace SsitEngine.Unity.UI
{
    public class RecycleList : MonoBehaviour, IUIWidgit
    {
        protected string ElemetnGOName = "UIListElement";

        [SerializeField] private bool HasTitle;

        protected bool m_IsInit;

        protected UIListTitleElement m_TitleElement;
        protected RecycleListElement m_UIListElement;
        protected List<RecycleListElement> mElements;
        protected List<UIListTitleElement> mTitleElements;
        protected string RootGOName = "UIListRootElement";
        private BaseUIForm RootPanel;
        protected string TitleGOName = "UIListTitleElement";

        /// <summary>
        ///     对象池中元素的根节点
        /// </summary>
        public Transform PoolRoot { get; private set; } //对象池对象的根节点

        //private List<GameObject> mChildList;
        /// <summary>
        ///     对象池容量（使用自动扩容时为当前容量的2倍，最好一次设置到位）
        /// </summary>
        public int PoolCapacity { get; set; } = 4;


        /// <summary>
        ///     元素根节点
        /// </summary>
        public Transform UIListRoot { get; private set; }

        /// <summary>
        ///     对象池
        /// </summary>
        protected ObjectPool<RecycleListElement> ElementPool { get; private set; }

        public virtual void Init( BaseUIForm root )
        {
            RootPanel = root;
            if (m_IsInit)
            {
                return;
            }
            if (HasTitle)
            {
                m_TitleElement = gameObject.GetChildNodeComponentScripts<UIListTitleElement>(TitleGOName);
                if (m_TitleElement != null)
                {
                    HasTitle = true;
                    m_TitleElement.Init();
                }
            }

            if (mElements == null)
            {
                mElements = new List<RecycleListElement>();
            }

            if (mTitleElements == null)
            {
                mTitleElements = new List<UIListTitleElement>();
            }

            UIListRoot = transform.Find<Transform>(RootGOName);

            m_UIListElement = transform.Find<RecycleListElement>(ElemetnGOName);
            m_UIListElement.SetActive(false)
                .Init();

            if (PoolRoot == null)
            {
                PoolRoot = new GameObject("PoolRoot").transform;
                PoolRoot.Parent(UIListRoot);
                PoolRoot.SetActive(false);
                //PoolRoot.SetLocalScale(Vector3.one);
            }

            ElementPool = new ObjectPool<RecycleListElement>(PoolCapacity,
                () =>
                {
                    var obj = Instantiate(m_UIListElement.gameObject, PoolRoot);
                    var element = obj.GetComponent<RecycleListElement>();
                    element.SetActive(true);
                    element.Init(UIListRoot.gameObject);
                    element.transform.localScale = Vector3.one;
                    OnCreateElementEvent(element);
                    return element;
                },
                element =>
                {
                    element.Parent(PoolRoot);
                    /*.SetActive(false);*/
                    OnReleaseElementEvent(element);
                });

            m_IsInit = true;
        }


        /// <summary>
        ///     清理所有元素
        /// </summary>
        public virtual void ClearElement()
        {
            for (var i = 0; i < mElements.Count; i++)
            {
                mElements[i].OnRelease();
                ElementPool.Release(mElements[i]);
            }

            for (var i = 0; i < mTitleElements.Count; i++)
                //TODO 对象池
            {
                Destroy(mElements[i].gameObject);
            }
            mElements.Clear();
            mTitleElements.Clear();
            AddTitleElement();
        }

        /// <summary>
        ///     生成element时响应
        /// </summary>
        /// <param name="element"></param>
        public virtual void OnCreateElementEvent( RecycleListElement element )
        {
        }

        /// <summary>
        ///     获取一个element时的响应
        /// </summary>
        /// <param name="element"></param>
        public virtual RecycleListElement AddElement( IInfoData infoData )
        {
            var element = ElementPool.Acquire();
            //element.SetActive(true);
            element.Parent(UIListRoot, true);
            element.SetLocalScale(Vector3.one);
            element.SetInfo(infoData);

            mElements.Add(element);

            return element;
        }

        /// <summary>
        ///     释放element时的响应方法
        /// </summary>
        /// <param name="element"></param>
        public virtual void OnReleaseElementEvent( RecycleListElement element )
        {
        }


        public virtual void DeleteElement( IInfoData infoData )
        {
            for (var i = 0; i < mElements.Count; i++)
            {
                var element = mElements[i];
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
                    ElementPool.Release(element);
                    mElements.Remove(element);
                    break;
                }
            }
        }

        /// <summary>
        ///     刷新Element元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataList"></param>
        public virtual void UpdateElement<T>( IList<T> dataList ) where T : IInfoData
        {
            ClearElement();
            for (var i = 0; i < dataList.Count; i++)
            {
                AddElement(dataList[i]);
            }
        }

        /// <summary>
        ///     获取某个Elment
        /// </summary>
        /// <param name="num">Root下的索引值</param>
        /// <returns></returns>
        public RecycleListElement GetElement( int num )
        {
            return mElements[num];
        }

        public T GetElement<T>( Predicate<T> condition ) where T : RecycleListElement
        {
            return GetElements<T>().Find(condition);
        }

        public List<T> GetElements<T>() where T : RecycleListElement
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
            for (var i = 0; i < UIListRoot.childCount; i++)
            {
                if (HasTitle && i == 0)
                {
                    continue;
                }
                var element = UIListRoot.GetChild(i).GetComponent<RecycleListElement>();
                if (element)
                {
                    element.gameObject.SetActive(enable);
                }
            }
        }

        private void Destroy()
        {
            ClearElement();

            for (var i = 0; i < mTitleElements.Count; i++)
            {
                Destroy(mTitleElements[i]);
            }
            mElements.Clear();
            mTitleElements.Clear();
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
                titleElement.Init(UIListRoot.gameObject);
                titleElement.transform.localScale = Vector3.one;
                mTitleElements.Add(titleElement);
                return titleElement;
            }
            return null;
        }
    }
}