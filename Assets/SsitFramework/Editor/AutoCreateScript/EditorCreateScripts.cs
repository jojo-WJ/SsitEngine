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
                $"{EditorFileUtility.AutoScriptPATH}UIFormClass.txt");
        }

        [MenuItem("Assets/Create/SsitScripts/Ssit DataBase", false, 80)]
        public static void CreateDataBase()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<CreateDataBaseScriptAsset>(),
                GetSelectedPathOrFallback() + "/New Script.cs",
                null,
                $"{EditorFileUtility.AutoScriptPATH}DataBaseClass.txt");
        }

        [MenuItem("Assets/Create/SsitScripts/Ssit DataProxy", false, 90)]
        public static void CreateDataProxy()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<CreateDataProxyScriptAsset>(),
                GetSelectedPathOrFallback() + "/New Script.cs",
                null,
                $"{EditorFileUtility.AutoScriptPATH}DataProxyClass.txt");
        }

        [MenuItem("Assets/Create/SsitScripts/Ssit Procedure", false, 90)]
        public static void CreateProcedure()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                ScriptableObject.CreateInstance<CreateProcedureScriptAsset>(),
                GetSelectedPathOrFallback() + "/New Script.cs",
                null,
                $"{EditorFileUtility.AutoScriptPATH}ProcedureClass.txt");
        }

        public static string GetSelectedPathOrFallback()
        {
            var path = "Assets";
            foreach (var obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
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
            var o = CreateScriptAssetFromTemplate(pathName, resourceFile);
            ProjectWindowUtil.ShowCreatedAsset(o);
        }

        internal static Object CreateScriptAssetFromTemplate( string pathName, string resourceFile )
        {
            var fullPath = Path.GetFullPath(pathName);
            var streamReader = new StreamReader(resourceFile);
            var text = streamReader.ReadToEnd();
            streamReader.Close();
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);

            text = Regex.Replace(text, "UIFormClass", fileNameWithoutExtension);

            var encoderShouldEmitUTF8Identifier = true;
            var throwOnInvalidBytes = false;
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
            var append = false;
            var streamWriter = new StreamWriter(fullPath, append, encoding);
            streamWriter.Write(text);
            streamWriter.Close();
            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
        }
    }

    internal class CreateDataBaseScriptAsset : EndNameEditAction
    {
        public override void Action( int instanceId, string pathName, string resourceFile )
        {
            var o = CreateScriptAssetFromTemplate(pathName, resourceFile);
            ProjectWindowUtil.ShowCreatedAsset(o);
        }

        internal static Object CreateScriptAssetFromTemplate( string pathName, string resourceFile )
        {
            var fullPath = Path.GetFullPath(pathName);
            var streamReader = new StreamReader(resourceFile);
            var text = streamReader.ReadToEnd();
            streamReader.Close();
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);

            text = Regex.Replace(text, "DataBaseClass", fileNameWithoutExtension);

            var encoderShouldEmitUTF8Identifier = true;
            var throwOnInvalidBytes = false;
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
            var append = false;
            var streamWriter = new StreamWriter(fullPath, append, encoding);
            streamWriter.Write(text);
            streamWriter.Close();
            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
        }
    }

    internal class CreateDataProxyScriptAsset : EndNameEditAction
    {
        public override void Action( int instanceId, string pathName, string resourceFile )
        {
            var o = CreateScriptAssetFromTemplate(pathName, resourceFile);
            ProjectWindowUtil.ShowCreatedAsset(o);
        }

        internal static Object CreateScriptAssetFromTemplate( string pathName, string resourceFile )
        {
            var fullPath = Path.GetFullPath(pathName);
            var streamReader = new StreamReader(resourceFile);
            var text = streamReader.ReadToEnd();
            streamReader.Close();
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);
            text = Regex.Replace(text, "DataProxyClass", fileNameWithoutExtension);

            var encoderShouldEmitUTF8Identifier = true;
            var throwOnInvalidBytes = false;
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
            var append = false;
            var streamWriter = new StreamWriter(fullPath, append, encoding);
            streamWriter.Write(text);
            streamWriter.Close();
            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
        }
    }

    /// <summary>
    /// 创建进程脚本资源
    /// </summary>
    internal class CreateProcedureScriptAsset : EndNameEditAction
    {
        public override void Action( int instanceId, string pathName, string resourceFile )
        {
            var o = CreateScriptAssetFromTemplate(pathName, resourceFile);
            ProjectWindowUtil.ShowCreatedAsset(o);
        }

        internal static Object CreateScriptAssetFromTemplate( string pathName, string resourceFile )
        {
            var fullPath = Path.GetFullPath(pathName);
            var streamReader = new StreamReader(resourceFile);
            var text = streamReader.ReadToEnd();
            streamReader.Close();
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);
            text = Regex.Replace(text, "ProcedureClass", fileNameWithoutExtension);

            var encoderShouldEmitUTF8Identifier = true;
            var throwOnInvalidBytes = false;
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
            var append = false;
            var streamWriter = new StreamWriter(fullPath, append, encoding);
            streamWriter.Write(text);
            streamWriter.Close();
            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
        }
    }
}