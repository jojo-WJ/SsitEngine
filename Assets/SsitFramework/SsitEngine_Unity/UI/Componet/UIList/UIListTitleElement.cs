using UnityEngine;

namespace SsitEngine.Unity.UI
{
    public class UIListTitleElement : MonoBehaviour
    {
        public virtual void Init( GameObject parent = null )
        {
            if (parent != null) transform.SetParent(parent.transform);
        }
    }
}