using Framework.Data;
using SsitEngine.EzReplay;
using UnityEngine;

namespace Framework.EZReplay
{
    namespace SsitEngine.EzReplay
    {
        public class Object2Record : MonoBehaviour
        {
            [SerializeField] private string prefabName;

            // Use this for initialization
            private void Start()
            {
                EZReplayManager.Instance.Mark4Recording(gameObject, GetComponent<ISave>(), prefabName);
            }
        }
    }
}