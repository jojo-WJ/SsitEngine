/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：编辑器内置工具                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/6 15:40:09                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.IO;
using UnityEditor;
using UnityEngine;

namespace SsitEngine.Editor.InternalTools
{
    public class InternalTools
    {
        [MenuItem("Tools/Beta/Clear PersistentPath")]
        public static void ClearPersistentPath()
        {
            var persistentPath = Application.persistentDataPath;
            if (Directory.Exists(persistentPath))
            {
                FileUtil.DeleteFileOrDirectory(persistentPath);
            }
            //todo:清除数据库
        }

        [MenuItem("Tools/SsitEngine/1-Import SsitEngineAssets Tools", false, 2051)]
        public static void ImportSsitEngineAssetsToolsContentMenu()
        {
            ImportExtraToolsContent();
        }
        
        [MenuItem("Tools/SsitEngine/2-UnZip SsitEngineAssets Tools And Run it", false, 2051)]
        public static void UnZipSsitEngineAssetsToolsContentMenu()
        {
            UnZipExtraToolsContent();
        }

        [MenuItem("Tools/SsitEngine/3-Import SsitEngineAssets General Package", false, 2051)]
        public static void ImportExamplesContentMenu()
        {
            ImportExtraGeneralPackageContent();
        }
        
        [MenuItem("Tools/SsitEngine/4-Copy Streaming Assets", false, 2051)]
        public static void CopyStreamingContentMenu()
        {
            CopyStreamingContent();
        }

        /// <summary>
        /// 
        /// </summary>
        private static void ImportExtraToolsContent()
        {
            var packageFullPath = GUIUtility.packageFullPath;
            AssetDatabase.ImportPackage(packageFullPath + "/Package Resources/SsitEngineAssets.unitypackage", true);
        }

        private static void UnZipExtraToolsContent()
        {
            string resourcePath = EditorFileUtility.ASSETOOLSFILTPATH;
            if (!File.Exists(resourcePath))
            {
                Debug.Log("请先导入资源 - SsitEngineAssets Tools");
                return;
            }
            string targetPath = EditorFileUtility.GetAseetSiblingPath(EditorFileUtility.TOOLSPATH);
            ZipUtil.Unzip(resourcePath, targetPath);
        }
        
        private static void ImportExtraGeneralPackageContent()
        {
            var packageFullPath = GUIUtility.packageFullPath;
            AssetDatabase.ImportPackage(packageFullPath + "/Package Resources/SsitEngineGeneralAssets.unitypackage", true);
        }
        
        
        private static void CopyStreamingContent()
        {
            EditorFileUtility.CopyAssetsFolder(GUIUtility.ToStreamingAssets,GUIUtility.StreamingAssets);
        }
    }
}