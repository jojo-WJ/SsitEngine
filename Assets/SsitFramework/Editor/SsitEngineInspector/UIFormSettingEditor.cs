/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 19:35:00                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.IO;
using SsitEngine.Unity.UI;
using UnityEditor;
using UnityEngine;

namespace SsitEngine.Editor.SsitEngineInspector
{
    [CustomEditor(typeof(UIFormSettings), true)]
    public class UIFormSettingsEditor : UnityEditor.Editor
    {
        protected UIFormSettings Target => target as UIFormSettings;

        [MenuItem("Tools/Beta/Generate UI Form Settings &F ")]
        protected static void FocusSettings()
        {
            var str = string.Format("Assets/Resources/{0}.asset", "UI/Settings/UIFormSettings");
            var uiFormSettings = AssetDatabase.LoadAssetAtPath(str, typeof(UIFormSettings)) as UIFormSettings;
            if (uiFormSettings == null)
            {
                RequireDirectory($"{Application.dataPath}/Resources/UI/Settings/UIFormSettings.asset");
                uiFormSettings = CreateInstance<UIFormSettings>();
                AssetDatabase.CreateAsset(uiFormSettings, str);
            }
            Selection.activeObject = uiFormSettings;
        }

        private static void RequireDirectory( string path )
        {
            var directoryName = Path.GetDirectoryName(path);
            if (Directory.Exists(directoryName))
            {
                return;
            }
            Directory.CreateDirectory(directoryName);
        }

        protected bool CheckRepeated<T>( ICollection<T> collections )
        {
            var objSet = new HashSet<T>(collections);
            return collections.Count != objSet.Count;
        }

        protected bool CheckNullOrEmpty( IEnumerable<string> enums )
        {
            foreach (var str in enums)
            {
                if (string.IsNullOrEmpty(str))
                {
                    return true;
                }
            }
            return false;
        }

        protected void CreateFolderForPrefab( List<string> layers )
        {
            foreach (object layer in layers)
            {
                RequireDirectory($"{Application.dataPath}/Resources/UIForm/Prefabs/{layer}/{string.Empty}");
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Target.layers.Count == 0)
            {
                return;
            }
            if (CheckNullOrEmpty(Target.layers))
            {
                EditorGUILayout.HelpBox("The elements in the Layers can not be empty.", MessageType.Error, true);
            }
            else if (CheckRepeated(Target.layers))
            {
                EditorGUILayout.HelpBox("The elements in the Layers can not be repeated.", MessageType.Error, true);
            }
            else
            {
                if (!GUILayout.Button("Create Folder For Prefab dont attempt"))
                {
                    return;
                }
                CreateFolderForPrefab(Target.layers);
                AssetDatabase.Refresh();
            }
        }
    }
}