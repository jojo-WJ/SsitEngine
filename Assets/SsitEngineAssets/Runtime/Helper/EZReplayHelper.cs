using Framework.Data;
using Framework.Logic;
using Framework.SceneObject;
using SsitEngine.EzReplay;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Unity.Data;
using UnityEngine;

namespace Framework.Helper
{
    public static class EZReplayHelper
    {
        public static void GetOrCreateEZObject( Object2PropertiesMapping mapping, string prefabOrGuid, int itemId,
            OnCreated func )
        {
            // Debug.Log(" prefabOrGuid " + prefabOrGuid + " itemId " + itemId);
            // first find object in scene by guid
            var obj = ObjectManager.Instance.GetObject(prefabOrGuid);
            if (obj != null)
            {
                func?.Invoke(obj, obj.GetRepresent(), obj.SceneInstance);
                return;
            }
            // second to proxy

            if (itemId == -1)
            {
                var retProxy = Facade.Instance.RetrieveProxy(prefabOrGuid) as ISave;
                if (retProxy == null)
                {
                    Debug.LogError("retProxy info is null !! proxyName::" + prefabOrGuid);
                    return;
                }
                func?.Invoke(null, null, retProxy);
                return;
            }
            //生成相应物体
            var itemProxy = DataManager.Instance.GetDataProxy<DataItemProxy>((int) EnLocalDataType.DATA_ITEM);
            var define = itemProxy?.GetModelData(itemId);
            if (define != null)
                SsitApplication.Instance.CreateEzObject(prefabOrGuid, itemId, func);
            else
                Debug.LogError("loadInDataItemProxy info is null !! prefabOrGuid::" + prefabOrGuid);
        }
    }
}