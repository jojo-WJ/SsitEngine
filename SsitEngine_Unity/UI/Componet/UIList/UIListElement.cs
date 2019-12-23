using System;
using SsitEngine.Data;
using UnityEngine;

namespace SsitEngine.Unity.UI
{
    [Obsolete("UIListElement is obsolete, using RecycleListElement Instead")]
    public abstract class UIListElement : MonoBehaviour
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
    }
}