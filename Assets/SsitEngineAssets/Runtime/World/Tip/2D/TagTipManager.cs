/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/7/2 14:08:02                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using SsitEngine.Unity;
using SsitEngine.Unity.Resource;
using UnityEngine;

namespace Framework.Tip
{
    public class TagTipManager : SingletonMono<TagTipManager>
    {
        private const string TIP_PATH = "UI/TagTip/UITagTip";
        private Transform canvasRoot;
        private List<TagTip> tips = new List<TagTip>();

        public override void OnSingletonInit()
        {
            tips = new List<TagTip>();
        }

        public void Awake()
        {
            if (Instance != this) Debug.LogError("Tag tip instance is exception");
        }

        public void LateUpdate()
        {
            for (var i = 0; i < tips.Count; i++)
            {
                var tt = tips[i];

                if (tt.isInUse)
                {
                    if (tt.root == null)
                    {
                        tt.isInUse = false;
                        gameObject.SetActive(false); //也可以移除画布外
                        continue;
                    }
                    if (tt.IsInView(tt.root.transform.position))
                    {
                        tt.gameObject.SetActive(true);
                        tt.OnShow();
                    }
                    else
                    {
                        tt.gameObject.SetActive(false);
                    }
                }
            }
        }

        public TagTip GetTip()
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

        private TagTip CreateTip()
        {
            TagTip tip = null;
            //access:资源加载
            ResourcesManager.Instance.LoadAsset<GameObject>(TIP_PATH, false, prefab =>
            {
                if (prefab != null)
                {
                    //prefab = Instantiate(prefab);

                    if (canvasRoot == null) canvasRoot = transform;
                    if (canvasRoot == null)
                    {
                        Debug.LogError("Can Not Find Canvas!!!");
                    }
                    else
                    {
                        prefab.transform.SetParent(canvasRoot);
                        //prefab.transform.localPosition = Vector3.one;
                        prefab.transform.localScale = Vector3.one;
                    }
                    tip = prefab.GetComponent<TagTip>();
                }
            });

            return tip;
        }

        private void ClearTip()
        {
            if (tips != null)
            {
                foreach (var tagTip in tips) Destroy(tagTip);
                tips.Clear();
            }
        }

        public void Destory()
        {
            tips.Clear();
            tips = null;
        }

        #region 消息处理

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        #endregion
    }
}