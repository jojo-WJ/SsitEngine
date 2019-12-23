using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SsitEngine.Editor
{
    public class FolderTree
    {
        public delegate void FileOnClick( Data data );

        //Excell Icon Path
        private static readonly string excellpath = "ExcelIcon.png";

        //Folder Icon Path
        private static readonly string folderpath = "FolderIcon.png";

        private static Texture2D mExcellIcon;
        private static Texture2D mFolderlIcon;


        private static string target;

        private static float offset = 7;

        private static Data data;
        private static Data selectData;

        public static List<Data> mFileMaps;

        private static FileOnClick OnFileOnClick;
        private static FileOnClick OnFolderOnClick;

        public static Data SelectData
        {
            get => selectData;
            set => selectData = value;
        }

        public static void OnInit( string target, FileOnClick func1, FileOnClick func2 = null )
        {
            //AssetDatabase.GetAssetPath(target)
            FolderTree.target = target;
            mExcellIcon = LoadTexture(excellpath);
            mFolderlIcon = LoadTexture(folderpath);

            if (Directory.Exists(target))
            {
                mFileMaps = new List<Data>();
                data = new Data();
                LoadFiles(data, target);
            }

            OnFileOnClick = func1;
            OnFolderOnClick = func2;
        }

        private static void LoadFiles( Data data, string curPath, int indent = 0 )
        {
            var content = GetGUIContent(curPath, true);

            if (content != null)
            {
                data.isFolder = true;
                data.indent = indent;
                data.content = content;
                data.assetPath = curPath;
                data.name = GetShowName(curPath);
                data.relationPath = GetRelationPath(curPath);
            }

            foreach (var path in Directory.GetDirectories(curPath))
            {
                if (path.EndsWith("bin"))
                {
                    continue;
                }

                var childDir = new Data();
                data.childs.Add(childDir);

                LoadFiles(childDir, path, indent + 1);
            }

            foreach (var path in Directory.GetFiles(curPath))
            {
                if (!path.EndsWith(".xlsx") || Path.GetFileNameWithoutExtension(path).Contains("_"))
                {
                    continue;
                }

                if (path.Contains("$"))
                {
                    continue;
                }

                content = GetGUIContent(path, false);

                if (content != null)
                {
                    var child = new Data();
                    child.indent = indent + 1;
                    child.content = content;
                    child.name = GetShowName(path);
                    child.folderName = data.name;
                    child.assetPath = path;
                    child.relationPath = GetRelationPath(path);
                    data.childs.Add(child);
                    mFileMaps.Add(child);
                }
            }
        }


        private static void DrawData( Data data )
        {
            GUILayout.Space(7);
            EditorGUILayout.BeginVertical("HelpBox");
            if (data.isFolder)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(mFolderlIcon));
                data.isSelected = EditorGUILayout.Foldout(data.isSelected, data.content, new GUIStyle("IN Foldout"));
                GUILayout.FlexibleSpace();
                //GUILayout.Label( "Export Folder" );
                //Rect rt = EditorGUILayout.GetControlRect();

                //Rect btnRect = new Rect( Screen.width - EditorGUIUtility.labelWidth, rt.y, EditorGUIUtility.labelWidth, rt.height );

                if (GUILayout.Button("Export Folder", GUILayout.MaxWidth(200)))
                {
                    if (selectData != data)
                    {
                        if (OnFolderOnClick != null)
                        {
                            OnFolderOnClick(data);
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            if (data.isSelected || !data.isFolder)
            {
                if (!data.isFolder)
                {
                    DrawGUIData(data);
                }

                for (var i = 0; i < data.childs.Count; i++)
                {
                    var child = data.childs[i];
                    if (child.content != null)
                    {
                        EditorGUI.indentLevel = child.indent;
                        //if (child.childs.Count > 0)
                        DrawData(child);
                        //else
                        //    DrawGUIData(child);
                    }
                }
            }


            EditorGUILayout.EndVertical();
        }


        public static void OnGUI()
        {
            if (Directory.Exists(target))
            {
                GUI.enabled = true;
                //EditorGUIUtility.SetIconSize(Vector2.one * 16);
                DrawData(data);
            }
        }


        private static void DrawGUIData( Data data )
        {
            GUIStyle style = "Label";
            GUI.color = Color.white;

            //Rect rt = GUILayoutUtility.GetRect(data.content, style);
            var rt = EditorGUILayout.GetControlRect();
            //GUI.color = Color.white;
            //EditorGUILayout.LabelField(data.content);

            if (selectData != null && selectData.Equals(data) || data.isExport)
            {
                style = "BoldLabel";
                GUI.color = Color.cyan;
            }

            rt.x += 16 * EditorGUI.indentLevel;
            if (GUI.Button(rt, data.content, style))
            {
                //if (selectData != null)
                //{
                //    selectData.isSelected = false;
                //}
                //bool preSelect = data.isSelected;
                data.isExport = true;
                if (selectData != data)
                {
                    selectData = data;
                    if (OnFileOnClick != null)
                    {
                        OnFileOnClick(data);
                    }
                }
            }


            GUI.color = Color.white;
        }

        private static GUIContent GetGUIContent( string path, bool isFolder )
        {
            //Object asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));


            if (!string.IsNullOrEmpty(path))
            {
                path = path.Replace("\\", "/");

                if (isFolder)
                {
                    if (!Directory.Exists(path))
                    {
                        return null;
                    }
                }
                else
                {
                    if (!File.Exists(path))
                    {
                        return null;
                    }
                }

                Texture tempTexture = isFolder ? mFolderlIcon : mExcellIcon;
                var name = GetShowName(path);
                return new GUIContent(name, tempTexture);
            }

            return null;
        }

        private static Texture2D LoadTexture( string path )
        {
            var curTexture =
                (Texture) AssetDatabase.LoadAssetAtPath($"{EditorFileUtility.RESPATH}{path}", typeof(Texture));
            //Texture2D curTexture = GUIUtils.LoadTexture(path, false);
            return curTexture as Texture2D;
        }

        private static string GetShowName( string path )
        {
            var temp = path.Replace("\\", "/");
            var fileName = temp.Substring(temp.LastIndexOf("/") + 1);

            var index = fileName.IndexOf(".");
            if (index == -1)
            {
                return fileName;
            }

            return fileName.Substring(0, fileName.IndexOf("."));
        }

        private static string GetRelationPath( string path )
        {
            var temp = path.Replace("\\", "/");
            temp = temp.Replace(target, "");
            if (string.IsNullOrEmpty(temp))
            {
                return path;
            }
            return temp.Substring(1);
        }

        private static string GetSuffix( string path )
        {
            //int index = path.LastIndexOf(target);
            //if (index == -1)
            //{
            //    Debug.LogError("获取相对路径失败::" + path);
            //    return null;
            //}
            return path.Substring(path.LastIndexOf("."));
        }

        /// <summary>
        /// 文件夹对象
        /// </summary>
        public class Data
        {
            public string assetPath;

            public List<Data> childs = new List<Data>();
            public GUIContent content;
            public string folderName;
            public int indent;
            public bool isExport;

            public bool isFolder;
            public bool isSelected = true;

            public string name;
            public string relationPath;
        }
    }
}