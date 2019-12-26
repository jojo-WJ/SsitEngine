#define OldVersion
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Framework.SceneObject
{
    [ExecuteInEditMode]
    [AddComponentMenu("SceneConfig")]
    public class ScriptScene : MonoBehaviour
    {
        //场景原始配置
        public SceneAsset sceneConfig;

        //场景简化配置
        public SceneAsset sceneSimpleConfig;


#if UNITY_EDITOR
        [ContextMenu("CreateSceneConfig")]
        public void CreateSceneConfig()
        {
            sceneConfig = SceneAsset.CreateInstance();
#if OldVersion
            sceneConfig.interactionMaps = GetAllObjectsInScene<BaseSceneInstance>().ToArray();
            CreateBackSceneConfig(true);
#else
            sceneConfig.interactionMaps = GetAllObjectsInScene<ScriptInstance>().ToArray();
#endif
        }


        [ContextMenu("CreateSimpleSceneConfig")]
        public void CreateSimpleSceneConfig()
        {
            sceneSimpleConfig = SceneAsset.CreateInstance();
#if OldVersion
            sceneSimpleConfig.interactionMaps = FindObjectsOfType<BaseSceneInstance>();
            CreateBackSceneConfig(false);
#else
            sceneSimpleConfig.interactionMaps = FindObjectsOfType<ScriptInstance>();
#endif
        }


        private void CreateBackSceneConfig( bool isNormal )
        {
            //获取老版本补丁
            /*var patch = gameObject.GetOrAddComponent<ScriptBackScene>();

            var configMaps = isNormal ? sceneConfig.interactionMaps : sceneSimpleConfig.interactionMaps;

            if (isNormal)
            {
                patch.sceneConfig = SceneBackAsset.CreateInstance();
            }
            else
            {
                patch.sceneSimpleConfig = SceneBackAsset.CreateInstance();
            }
            var backConfig = isNormal ? patch.sceneConfig : patch.sceneSimpleConfig;

            for (int i = 0; i < configMaps.Length; i++)
            {
                var tt = configMaps[i];

                var type = tt.GetType();

                if (type == typeof(Valve))
                {
                    backConfig.ValueMaps.Add(tt.gameObject);
                }
                else if (type == typeof(PumpSwitch))
                {
                    backConfig.PumpMaps.Add(tt.gameObject);
                }
                else if (type == typeof(Annihilator))
                {
                    backConfig.MHQMaps.Add(tt.gameObject);
                }
                else if (type == typeof(XFP))
                {
                    backConfig.XFPMaps.Add(tt.gameObject);
                }
                else if (type == typeof(StreamRope))
                {
                    backConfig.RopeMaps.Add(tt.gameObject);
                }
            }
            */
        }

        [ContextMenu("EnableSceneConfig")]
        public void EnableSceneConfig()
        {
            foreach (var ss in sceneConfig.interactionMaps)
                if (ss)
                    ss.gameObject.SetActive(true);
        }

        [ContextMenu("EnableSceneSimpleConfig")]
        public void EnableSceneSimpleConfig()
        {
            var allSceneObject = FindObjectsOfType<BaseSceneInstance>();

            foreach (var s in allSceneObject) s.gameObject.SetActive(false);
            foreach (var ss in sceneSimpleConfig.interactionMaps)
                if (ss)
                    ss.gameObject.SetActive(true);
        }


        private List<T> GetAllObjectsInScene<T>() where T : MonoBehaviour
        {
            var objectsInScene = new List<T>();

            foreach (var go in Resources.FindObjectsOfTypeAll<T>())
            {
                if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                    continue;

                //过滤存在磁盘上的对象
                if (EditorUtility.IsPersistent(go.gameObject))
                    continue;

                objectsInScene.Add(go);
            }

            return objectsInScene;
        }
#endif
    }
}