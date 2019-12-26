using SsitEngine.Data;
using UnityEngine;

namespace SsitEngine.Unity.UI
{
    public abstract class RecycleListElement : MonoBehaviour
    {
        private IInfoData m_InfoData;
        protected bool m_IsInit = false;

        public virtual IInfoData InfoData => m_InfoData;

        public virtual void Init( GameObject parent = null )
        {
            if (parent != null)
            {
                transform.SetParent(parent.transform);
            }
        }

        public virtual void SetInfo( IInfoData info )
        {
            m_InfoData = info;
        }

        /// <summary>
        ///     向对象池释放元素前执行的方法
        /// </summary>
        /// <returns></returns>
        public virtual RecycleListElement OnRelease()
        {
            return this;
        }
    }
}