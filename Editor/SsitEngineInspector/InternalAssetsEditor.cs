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
using SsitEngine.Unity;
using UnityEditor;
using UnityEngine;

namespace SsitEngine.Editor.SsitEngineInspector
{
    [CustomEditor(typeof(InternalAssetConfig), true)]
    public class InternalAssetsEditor : UnityEditor.Editor
    {
        protected InternalAssetConfig Target => target as InternalAssetConfig;

        [MenuItem("Tools/Beta/Generate SteamingCopyAsset")]
        protected static void FocusSettings()
        {
            var str = "Assets/Resources/InternalAssetConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath(str, typeof(InternalAssetConfig)) as InternalAssetConfig;
            if (config == null)
            {
                RequireDirectory($"{Application.dataPath}/Resources/InternalAssetConfig.asset");
                config = CreateInstance<InternalAssetConfig>();
                AssetDatabase.CreateAsset(config, str);
            }
            Selection.activeObject = config;
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


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!GUILayout.Button("Auto calc streaming Assets"))
            {
                return;
            }
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                return;
            }
            var directoryInfo = new DirectoryInfo(Application.streamingAssetsPath);

            Target.mStreamingAsset.Clear();

            var files = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; i++)
            {
                var info = files[i];

                if (info.Name.Contains(".meta"))
                {
                    continue;
                }

                Target.mStreamingAsset.Add(EditorFileUtility.GetStreamingAssetPath(info.FullName));
            }
            AssetDatabase.Refresh();
        }
    }
}