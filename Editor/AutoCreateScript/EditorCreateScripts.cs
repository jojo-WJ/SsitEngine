using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace SsitEngine.Editor
{
    public class EditorCreateScripts
    {
        [MenuItem("Assets/Create/SsitScripts/Ssit UI Form", false, 70)]
        public static void CreateUIForm()
        {
            //Texture2D texture2D = EditorResources.Load<Texture2D>("Assets/Editor/AutoCreateScript/C#.png");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<CreateUIFormScriptAsset>(),
                GetSelectedPathOrFallback() + "/New Script.cs",
                null,
                "Assets/Editor/AutoCreateScript/ScriptsTemplate/UIFormClass.txt");
        }

        [MenuItem("Assets/Create/SsitScripts/Ssit DataBase", false, 80)]
        public static void CreateDataBase()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<CreateDataBaseScriptAsset>(),
                GetSelectedPathOrFallback() + "/New Script.cs",
                null,
                "Assets/Editor/AutoCreateScript/ScriptsTemplate/DataBaseClass.txt");
        }

        [MenuItem("Assets/Create/SsitScripts/Ssit DataProxy", false, 90)]
        public static void CreateDataProxy()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<CreateDataProxyScriptAsset>(),
                GetSelectedPathOrFallback() + "/New Script.cs",
                null,
                "Assets/Editor/AutoCreateScript/ScriptsTemplate/DataProxyClass.txt");
        }

        [MenuItem("Assets/Create/SsitScripts/Ssit Procedure", false, 90)]
        public static void CreateProcedure()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<CreateProcedureScriptAsset>(),
                GetSelectedPathOrFallback() + "/New Script.cs",
                null,
                "Assets/Editor/AutoCreateScript/ScriptsTemplate/ProcedureClass.txt");
        }

        public static string GetSelectedPathOrFallback()
        {
            string path = "Assets";
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }

    }


    public class CreateUIFormScriptAsset : EndNameEditAction
    {
        public override void Action( int instanceId, string pathName, string resourceFile )
        {
            UnityEngine.Object o = CreateScriptAssetFromTemplate(pathName, resourceFile);
            ProjectWindowUtil.ShowCreatedAsset(o);
        }

        internal static UnityEngine.Object CreateScriptAssetFromTemplate( string pathName, string resourceFile )
        {
            string fullPath = Path.GetFullPath(pathName);
            StreamReader streamReader = new StreamReader(resourceFile);
            string text = streamReader.ReadToEnd();
            streamReader.Close();
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);

            text = Regex.Replace(text, "UIFormClass", fileNameWithoutExtension);

            bool encoderShouldEmitUTF8Identifier = true;
            bool throwOnInvalidBytes = false;
            UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
            bool append = false;
            StreamWriter streamWriter = new StreamWriter(fullPath, append, encoding);
            streamWriter.Write(text);
            streamWriter.Close();
            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
        }
    }

    class CreateDataBaseScriptAsset : EndNameEditAction
    {
        public override void Action( int instanceId, string pathName, string resourceFile )
        {
            UnityEngine.Object o = CreateScriptAssetFromTemplate(pathName, resourceFile);
            ProjectWindowUtil.ShowCreatedAsset(o);
        }

        internal static UnityEngine.Object CreateScriptAssetFromTemplate( string pathName, string resourceFile )
        {
            string fullPath = Path.GetFullPath(pathName);
            StreamReader streamReader = new StreamReader(resourceFile);
            string text = streamReader.ReadToEnd();
            streamReader.Close();
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);

            text = Regex.Replace(text, "DataBaseClass", fileNameWithoutExtension);

            bool encoderShouldEmitUTF8Identifier = true;
            bool throwOnInvalidBytes = false;
            UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
            bool append = false;
            StreamWriter streamWriter = new StreamWriter(fullPath, append, encoding);
            streamWriter.Write(text);
            streamWriter.Close();
            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
        }
    }

    class CreateDataProxyScriptAsset : EndNameEditAction
    {
        public override void Action( int instanceId, string pathName, string resourceFile )
        {
            UnityEngine.Object o = CreateScriptAssetFromTemplate(pathName, resourceFile);
            ProjectWindowUtil.ShowCreatedAsset(o);
        }

        internal static UnityEngine.Object CreateScriptAssetFromTemplate( string pathName, string resourceFile )
        {
            string fullPath = Path.GetFullPath(pathName);
            StreamReader streamReader = new StreamReader(resourceFile);
            string text = streamReader.ReadToEnd();
            streamReader.Close();
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);
            text = Regex.Replace(text, "DataProxyClass", fileNameWithoutExtension);

            bool encoderShouldEmitUTF8Identifier = true;
            bool throwOnInvalidBytes = false;
            UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
            bool append = false;
            StreamWriter streamWriter = new StreamWriter(fullPath, append, encoding);
            streamWriter.Write(text);
            streamWriter.Close();
            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
        }
    }

    /// <summary>
    /// 创建进程脚本资源
    /// </summary>
    class CreateProcedureScriptAsset : EndNameEditAction
    {
        public override void Action( int instanceId, string pathName, string resourceFile )
        {
            UnityEngine.Object o = CreateScriptAssetFromTemplate(pathName, resourceFile);
            ProjectWindowUtil.ShowCreatedAsset(o);
        }

        internal static UnityEngine.Object CreateScriptAssetFromTemplate( string pathName, string resourceFile )
        {
            string fullPath = Path.GetFullPath(pathName);
            StreamReader streamReader = new StreamReader(resourceFile);
            string text = streamReader.ReadToEnd();
            streamReader.Close();
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);
            text = Regex.Replace(text, "ProcedureClass", fileNameWithoutExtension);

            bool encoderShouldEmitUTF8Identifier = true;
            bool throwOnInvalidBytes = false;
            UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
            bool append = false;
            StreamWriter streamWriter = new StreamWriter(fullPath, append, encoding);
            streamWriter.Write(text);
            streamWriter.Close();
            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
        }
    }
}