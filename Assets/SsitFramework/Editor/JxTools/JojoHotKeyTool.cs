using UnityEditor;
using UnityEngine;

namespace SsitEngine.Editor
{
    public class JojoHotKeyTool : ScriptableObject
    {
        //  设置菜单Tool 下的 MyTool 下的 Enable\Disable Multi GameObj 快捷键为  command 加shift 加 d  <MAC上的>
        public const string MENU_DISABLE_SELECTED_GAMEOBJ =
                "Window/Enable\\Disable Multi GameObj %#z"; //%#d 即代表 command 加shift 加 d快捷键

        [MenuItem(MENU_DISABLE_SELECTED_GAMEOBJ, true)]
        static bool ValidateSelectEnableODisable()
        {
            GameObject[] gobj = GetSelectedGameObject() as GameObject[];
            if (gobj == null)
            {
                return false;
            }
            if (gobj.Length == 0)
            {
                return false;
            }
            return true;
        }

        [MenuItem(MENU_DISABLE_SELECTED_GAMEOBJ)]
        static void SelectEnableODisable()
        {
            GameObject[] gobj = GetSelectedGameObject() as GameObject[];
            bool enable = !gobj[0].activeSelf;
            foreach (GameObject go in gobj)
            {
                EnableODisableChildNote(go.transform, enable);
            }
        }

        //激活或者关闭选中的物体及其子物体
        public static void EnableODisableChildNote( Transform parent, bool enable )
        {
            parent.gameObject.SetActive(enable);
            /* for (int i = 0; i < parent.childCount; i++)
             {
                 Transform child = parent.GetChild(i);
                 if (child.childCount != 0)
                 {
                     EnableODisableChildNote(child, enable);
                 }
                 else
                 {
                     child.gameObject.active = enable;
                 }
             }*/
        }

        // 返回选中的物体
        static GameObject[] GetSelectedGameObject()
        {
            return Selection.gameObjects;
        }

        [MenuItem("Tools/Extension Tools/Apply Selected Prefabs")]
        static void ApplySelectedPrefabs()
        {
            //获取选中的gameobject对象
            GameObject[] selectedsGameobject = Selection.gameObjects;

            //GameObject prefab = PrefabUtility.FindPrefabRoot(selectedsGameobject[0]);

            for (int i = 0; i < selectedsGameobject.Length; i++)
            {
                GameObject obj = selectedsGameobject[i];

                //UnityEngine.Object newsPref = PrefabUtility.GetPrefabObject(obj);
                UnityEngine.Object newsPref = PrefabUtility.GetPrefabInstanceHandle(obj);

                //判断选择的物体，是否为预设
                if (PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.Connected)
                {
                    //UnityEngine.Object parentObject = PrefabUtility.GetPrefabParent(obj);
                    UnityEngine.Object parentObject = PrefabUtility.GetCorrespondingObjectFromSource(obj);

                    //获取路径
                    string path = AssetDatabase.GetAssetPath(parentObject);
                    //Debug.Log("path:"+path);
                    //替换预设
                    //PrefabUtility.ReplacePrefab(obj, parentObject, ReplacePrefabOptions.ConnectToPrefab);
                    PrefabUtility.SaveAsPrefabAssetAndConnect(obj, path, InteractionMode.UserAction);

                    //刷新
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}