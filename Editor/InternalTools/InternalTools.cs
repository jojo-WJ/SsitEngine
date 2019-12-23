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

        [MenuItem("Tools/SsitEngine/Import SsitEngineAssets", false, 2051)]
        public static void ImportExamplesContentMenu()
        {
            ImportExtraContent();
        }

        /// <summary>
        /// 
        /// </summary>
        private static void ImportExtraContent()
        {
            var packageFullPath = GUIUtility.packageFullPath;
            AssetDatabase.ImportPackage(packageFullPath + "/Package Resources/SsitEngineAssets.unitypackage", true);
        }
    }
}