using System.Collections.Generic;
using SsitEngine;
using SsitEngine.Unity.Resource;
using UnityEngine;

namespace Framework.Hud
{
    public class HUDManager : Singleton<HUDManager>, ISingleton
    {
        private const string TIP_PATH = "UI/HUDCanvas";
        private Transform canvasRoot;
        private readonly List<HUD> tips = new List<HUD>();

        public HUDManager()
        {
            tips = new List<HUD>();
        }

        public void OnSingletonInit()
        {
        }

        public HUD GetTip()
        {
            tips.RemoveAll(tip => { return tip == null; });
            foreach (var hud in tips)
            {
                if (hud.isInUse) continue;
                return hud;
            }
            var tmp = CreateTip();
            if (tmp != null) tips.Add(tmp);
            return tmp;
        }

        private HUD CreateTip()
        {
            //access:资源加载
            //GameObject prefab = ResourcesManager.Instance.LoadAsset<GameObject>(TIP_PATH);
            HUD hud = null;
            ResourcesManager.Instance.LoadAsset<GameObject>(TIP_PATH, false, prefab =>
            {
                if (prefab != null)
                {
                    if (canvasRoot == null)
                    {
                        canvasRoot = new GameObject("HUDManagerRoot").transform;
                        Object.DontDestroyOnLoad(canvasRoot);
                    }
                    if (canvasRoot == null)
                        Debug.LogError("Can Not Find Canvas!!!");
                    else
                        prefab.transform.SetParent(canvasRoot);
                    hud = prefab.GetComponent<HUD>();
                }
            });

            return hud;
        }

        public void Destory()
        {
            if (canvasRoot) Object.Destroy(canvasRoot.gameObject);
        }
    }
}