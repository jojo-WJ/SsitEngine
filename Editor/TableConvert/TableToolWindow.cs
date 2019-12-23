/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年11月5日 15点11分                             
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace SsitEngine.Editor
{
    public class TableToolWindow : EditorWindow
    {
        private static readonly string CONGFIGASSETNAME = "TableFile.asset";
        private int animIdx;


        //private float mButtonWidth = 200;
        //private float mButtonHeight = 60;


        private readonly string BgStyle = "PopupCurveSwatchBackground";


        private TableFileScrips mConfigInfo;
        private string mFilter;

        private string[] mFolderNames = { };
        private bool mIsHelper;
        private bool mIsScroll;

        private TableToModelBase mModelBase;

        public Vector2 mScrollPos;

        private readonly float offset = 10;

        private string search;


        public void OnFocus()
        {
            wantsMouseMove = true;
            FolderTree.SelectData = null;
            Repaint();
        }

        private void OnDestroy()
        {
            EditorApplication.isPlaying = false;
        }

        private void OnProjectWindowChange()
        {
            //TODO:重新加载目录文本
            //this.ReloadPreviousBehavior();
            FolderTree.SelectData = null;
            Repaint();
        }

        #region 主体

        [MenuItem("Tools/导出表格数据 #&o", false, 1)]
        private static void InitWindow()
        {
            GetWindow(typeof(TableToolWindow));
        }


        private void InitData()
        {
            if (!string.IsNullOrEmpty(mConfigInfo.WorkSpacePath))
            {
                if (!Directory.Exists(mConfigInfo.WorkSpacePath))
                {
                    return;
                }
                var tempArray = Directory.GetFiles(mConfigInfo.WorkSpacePath, "*.bat", SearchOption.AllDirectories);

                mConfigInfo.FileNames = new string[tempArray.Length];

                for (var i = 0; i < tempArray.Length; i++)
                {
                    var temp = tempArray[i].Replace("\\", "/");
                    var fileName = temp.Substring(temp.LastIndexOf("/") + 1);
                    mConfigInfo.FileNames[i] = fileName.Substring(0, fileName.IndexOf("."));
                }

                //Debug.Log("初始化工作目录  " + mConfigInfo.WorkSpacePath);

                TableTool.CheckFolder();

                FolderTree.OnInit(mConfigInfo.WorkSpacePath, OnFileClick, OnFolderClick);
                mFolderNames = FolderTree.mFileMaps.ConvertAll(delegate( FolderTree.Data data ) { return data.name; })
                    .ToArray();

                mModelBase = new TableToModelBase();
            }
        }

        private void OnDisable()
        {
            //Debug.Log("OnDisable");
            mModelBase = null;
        }

        private void OnEnable()
        {
            var tempPath = Path.Combine(EditorFileUtility.CONFIGPATH, CONGFIGASSETNAME);

            if (!File.Exists(tempPath))
            {
                mConfigInfo = CreateInstance<TableFileScrips>();

                if (!Directory.Exists(Path.GetDirectoryName(tempPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(tempPath) ?? throw new InvalidOperationException());
                    Debug.Log($"CreateDirectory::{Path.GetDirectoryName(tempPath)}");
                }
                //Directory.CreateDirectory( CONFIGPATH );
                AssetDatabase.CreateAsset(mConfigInfo, tempPath);
                //mConfigInfo.hideFlags = HideFlags.DontSaveInEditor;
                //AssetDatabase.Refresh();
            }

            mConfigInfo = AssetDatabase.LoadAssetAtPath<TableFileScrips>(tempPath);

            InitConfig();

            InitData();
        }

        private void InitConfig()
        {
            var temp = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/"));

            if (!string.IsNullOrEmpty(mConfigInfo.WorkSpaceRelationPath))
            {
                mConfigInfo.WorkSpacePath = temp + "/" + mConfigInfo.WorkSpaceRelationPath;
            }
            else
            {
                mConfigInfo.WorkSpacePath = temp;
            }

            //mConfigInfo.WorkSpacePath = temp + "/Table";

            mConfigInfo.ScriptExportPath = Application.dataPath + TableTool.DataScriptsFolderPath;

            if (mConfigInfo.mIsExportResources)
            {
                mConfigInfo.JsonExportPath = Application.dataPath + TableTool.JsonFolderResPath;
            }
            else
            {
                mConfigInfo.JsonExportPath = Application.dataPath + TableTool.JsonFolderDefaultPath;
            }
        }

        private void OnGUI()
        {
            if (mConfigInfo == null)
            {
                return;
            }
            GUILayout.BeginHorizontal("HelpBox");
            GUILayout.Label("快速查找相关表格", "label");
            GUILayout.FlexibleSpace();
            GUILayout.Label("查找:");

            mFilter = EditorGUILayout.TextField("筛选", mFilter);

            if (GUILayout.Button("x", GUILayout.Width(30)))
            {
                mFilter = "";
                GUI.FocusControl("");
            }

            var curAnims = mFolderNames;
            var curIndex = 0;
            if (!string.IsNullOrEmpty(mFilter))
            {
                var temp = mFolderNames.ToList().FindAll(x => { return Regex.IsMatch(x, mFilter); });
                curAnims = temp.ToArray();

                if (curAnims.Length > 0 && curAnims.Length < mFolderNames.Length)
                {
                    animIdx = Mathf.Clamp(animIdx, 0, mFolderNames.Length - 1);
                    animIdx = curAnims.ToList().FindIndex(x => { return x.Equals(mFolderNames[animIdx]); });
                    animIdx = animIdx >= 0 ? animIdx : 0;
                    animIdx = EditorGUILayout.Popup("名称", animIdx, curAnims);
                    curIndex = mFolderNames.ToList().FindIndex(x => { return x.Equals(curAnims[animIdx]); });
                }
                else
                {
                    animIdx = EditorGUILayout.Popup("名称", animIdx, mFolderNames);
                    curIndex = animIdx;
                }
            }
            else
            {
                animIdx = EditorGUILayout.Popup("名称", animIdx, mFolderNames);
                curIndex = animIdx;
            }

            if (curIndex < mFolderNames.Length)
            {
                search = EditorGUILayout.TextField(mFolderNames[curIndex]);
                if (GUILayout.Button("Export"))
                {
                    if (!string.IsNullOrEmpty(search) && curIndex != -1)
                    {
                        var tt = FolderTree.mFileMaps.Find(x => { return x.name.Equals(curAnims[curIndex]); });
                        if (tt != null)
                        {
                            if (tt.isFolder)
                            {
                                OnFolderClick(tt);
                            }
                            else
                            {
                                OnFileClick(tt);
                            }
                        }
                    }
                }
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical();
            {
                //绘制
                if (GUILayout.Button("Clear Bat"))
                {
                    ClearBat();
                }
            }
            EditorGUILayout.EndVertical();
            mScrollPos = GUILayout.BeginScrollView(mScrollPos);
            {
                EditorGUILayout.BeginVertical();
                {
                    //绘制
                    DrawWorkSoacePath();
                    DrawScripExprotPath();
                    DrawJsonExportPath();
                    DrawBinaryExportPath();
                    //DrawLuaExportPath();
                    DrawToggle();

                    //绘制根
                    DrawRoot();
                }
                EditorGUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
            OnMenuGUI();
        }

        private void ClearBat()
        {
            var files = Directory.GetFiles(mConfigInfo.WorkSpacePath, "*.bat");
            for (var i = files.Length - 1; i >= 0; i--)
            {
                File.Delete(files[i]);
            }
        }

        #endregion

        #region Draw Window

        private void DrawWorkSoacePath()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(BgStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            //EditorGUI.indentLevel = 1;
            GUI.color = Color.cyan;
            var rect = EditorGUILayout.GetControlRect();

            var width1 = EditorGUIUtility.fieldWidth + offset;
            ////GUILayout.Space(Screen.width / 2 - EditorGUIUtility.fieldWidth * 5);
            EditorGUI.LabelField(new Rect(rect.x, rect.y, width1, EditorGUIUtility.singleLineHeight), "工作路径:");
            var width2 = Screen.width - EditorGUIUtility.fieldWidth * 2;
            //GUI.color = Color.white;
            GUI.SetNextControlName("工作路径");
            EditorGUI.SelectableLabel(
                new Rect(rect.x + EditorGUIUtility.fieldWidth, rect.y, width2 - width1,
                    EditorGUIUtility.singleLineHeight), mConfigInfo.WorkSpacePath);

            if (GUI.GetNameOfFocusedControl() == "工作路径")
            {
                //把名字存储在剪粘板 
                EditorGUIUtility.systemCopyBuffer = mConfigInfo.WorkSpacePath; // "\"" + style.name + "\"";
            }

            if (GUI.Button(new Rect(width2, rect.y, Screen.width - width2, EditorGUIUtility.singleLineHeight),
                "Select"))
            {
                var temp = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/"));
                mConfigInfo.WorkSpacePath =
                    EditorUtility.SaveFolderPanel("工作路径", temp, mConfigInfo.WorkSpaceRelationPath);
                InitData();
            }

            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
        }

        private void DrawScripExprotPath()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(BgStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            //EditorGUI.indentLevel = 1;
            GUI.color = Color.cyan;
            var rect = EditorGUILayout.GetControlRect();

            var width1 = EditorGUIUtility.fieldWidth + 25;
            ////GUILayout.Space(Screen.width / 2 - EditorGUIUtility.fieldWidth * 5);
            EditorGUI.LabelField(new Rect(rect.x, rect.y, width1, EditorGUIUtility.singleLineHeight), "脚本导出路径:");
            var width2 = Screen.width - EditorGUIUtility.fieldWidth * 2;
            //GUI.color = Color.white;
            GUI.SetNextControlName("脚本导出路径");
            EditorGUI.SelectableLabel(
                new Rect(rect.x + width1, rect.y, width2 - width1, EditorGUIUtility.singleLineHeight),
                mConfigInfo.ScriptExportPath);
            if (GUI.GetNameOfFocusedControl() == "脚本导出路径")
            {
                //把名字存储在剪粘板 
                EditorGUIUtility.systemCopyBuffer = mConfigInfo.ScriptExportPath; // "\"" + style.name + "\"";
            }


            if (GUI.Button(new Rect(width2, rect.y, Screen.width - width2, EditorGUIUtility.singleLineHeight),
                "Select"))
            {
                var temp = Application.dataPath + TableTool.DataScriptsFolderPath;
                //string temp = Path.Combine(Application.dataPath,TableTool.DataScriptsFolderPath);

                var rf = temp.Substring(0, temp.LastIndexOf("/"));
                var f = temp.Substring(temp.LastIndexOf("/") + 1);

                mConfigInfo.ScriptExportPath = EditorUtility.SaveFolderPanel("脚本导出路径:", rf, f);

                InitData();
            }

            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
        }

        private void DrawBinaryExportPath()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(BgStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            //EditorGUI.indentLevel = 1;
            GUI.color = Color.cyan;
            var rect = EditorGUILayout.GetControlRect();

            var width1 = EditorGUIUtility.fieldWidth + 35;
            ////GUILayout.Space(Screen.width / 2 - EditorGUIUtility.fieldWidth * 5);
            GUI.Label(new Rect(rect.x, rect.y, width1, EditorGUIUtility.singleLineHeight), "二进制导出路径:");
            var width2 = Screen.width - EditorGUIUtility.fieldWidth * 2;
            //GUI.color = Color.white;
            GUI.SetNextControlName("二进制导出路径");
            EditorGUI.SelectableLabel(
                new Rect(rect.x + width1, rect.y, width2 - width1, EditorGUIUtility.singleLineHeight),
                mConfigInfo.BinaryExportPath);
            if (GUI.GetNameOfFocusedControl() == "二进制导出路径")
            {
                //把名字存储在剪粘板 
                EditorGUIUtility.systemCopyBuffer = mConfigInfo.BinaryExportPath; // "\"" + style.name + "\"";
            }

            if (GUI.Button(new Rect(width2, rect.y, Screen.width - width2, EditorGUIUtility.singleLineHeight),
                "Select"))
            {
                var temp = Application.dataPath + TableTool.BinaryFloderPath;

                var rf = temp.Substring(0, temp.LastIndexOf("/"));
                var f = temp.Substring(temp.LastIndexOf("/") + 1);
                mConfigInfo.BinaryExportPath = EditorUtility.SaveFolderPanel("二进制导出路径:", rf
                    , f);

                InitData();
            }

            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
        }

        private void DrawJsonExportPath()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(BgStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            //EditorGUI.indentLevel = 1;
            GUI.color = Color.cyan;
            var rect = EditorGUILayout.GetControlRect();

            var width1 = EditorGUIUtility.fieldWidth + 35;
            ////GUILayout.Space(Screen.width / 2 - EditorGUIUtility.fieldWidth * 5);
            GUI.Label(new Rect(rect.x, rect.y, width1, EditorGUIUtility.singleLineHeight), "Json导出路径:");
            var width2 = Screen.width - EditorGUIUtility.fieldWidth * 2;
            //GUI.color = Color.white;
            GUI.SetNextControlName("Json导出路径");
            EditorGUI.SelectableLabel(
                new Rect(rect.x + width1, rect.y, width2 - width1, EditorGUIUtility.singleLineHeight),
                mConfigInfo.JsonExportPath);
            if (GUI.GetNameOfFocusedControl() == "Json导出路径")
            {
                //把名字存储在剪粘板 
                EditorGUIUtility.systemCopyBuffer = mConfigInfo.LuaExportPath; // "\"" + style.name + "\"";
            }

            if (GUI.Button(new Rect(width2, rect.y, Screen.width - width2, EditorGUIUtility.singleLineHeight),
                "Select"))
            {
                var tempStr = mConfigInfo.mIsExportResources
                    ? TableTool.JsonFolderResPath
                    : TableTool.JsonFolderDefaultPath;
                var temp = Application.dataPath + tempStr;

                var rf = temp.Substring(0, temp.LastIndexOf("/"));
                var f = temp.Substring(temp.LastIndexOf("/") + 1);
                mConfigInfo.JsonExportPath = EditorUtility.SaveFolderPanel("Json导出路径:", rf, f);

                InitData();
            }

            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
        }

        private void DrawLuaExportPath()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(BgStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            //EditorGUI.indentLevel = 1;
            GUI.color = Color.cyan;
            var rect = EditorGUILayout.GetControlRect();

            var width1 = EditorGUIUtility.fieldWidth + 35;
            ////GUILayout.Space(Screen.width / 2 - EditorGUIUtility.fieldWidth * 5);
            GUI.Label(new Rect(rect.x, rect.y, width1, EditorGUIUtility.singleLineHeight), "Lua导出路径:");
            var width2 = Screen.width - EditorGUIUtility.fieldWidth * 2;
            //GUI.color = Color.white;
            GUI.SetNextControlName("Lua导出路径");
            EditorGUI.SelectableLabel(
                new Rect(rect.x + width1, rect.y, width2 - width1, EditorGUIUtility.singleLineHeight),
                mConfigInfo.LuaExportPath);
            if (GUI.GetNameOfFocusedControl() == "Lua导出路径")
            {
                //把名字存储在剪粘板 
                EditorGUIUtility.systemCopyBuffer = mConfigInfo.LuaExportPath; // "\"" + style.name + "\"";
            }

            if (GUI.Button(new Rect(width2, rect.y, Screen.width - width2, EditorGUIUtility.singleLineHeight),
                "Select"))
            {
                var temp = Application.dataPath + TableTool.LuasFolderPath;

                var rf = temp.Substring(0, temp.LastIndexOf("/"));
                var f = temp.Substring(temp.LastIndexOf("/") + 1);
                mConfigInfo.LuaExportPath = EditorUtility.SaveFolderPanel("Lua导出路径:", rf
                    , f);

                InitData();
            }

            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
        }

        private void DrawToggle()
        {
            //GUILayout.Space(10);
            //EditorGUILayout.BeginHorizontal(BgStyle
            //         , new GUILayoutOption[]
            //         {
            //             GUILayout.Height(EditorGUIUtility.singleLineHeight),
            //         });

            //mConfigInfo.mIsExportLua = EditorGUILayout.Toggle("是否导出Lua脚本", mConfigInfo.mIsExportLua);
            //HelpLable("针对用Lua解析,实现热更");
            //EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(BgStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight));

            mConfigInfo.mIsExportLuaForBit = EditorGUILayout.Toggle("是否导出LuaforBit脚本", mConfigInfo.mIsExportLuaForBit);
            HelpLable("针对用Lua解析二进制,实现热更(需要引入二进制读取工具类)");
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal(BgStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight));

            mConfigInfo.mIsExportJson = EditorGUILayout.Toggle("是否导出Json", mConfigInfo.mIsExportJson);
            HelpLable("默认导出Json");

            EditorGUI.BeginChangeCheck();
            mConfigInfo.mIsExportResources = EditorGUILayout.Toggle("是否导出Resources 目录", mConfigInfo.mIsExportResources);
            HelpLable("默认不导出Resources 目录");

            if (EditorGUI.EndChangeCheck())
            {
                ClearBat();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawRoot()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            if (!string.IsNullOrEmpty(mConfigInfo.WorkSpacePath) && Directory.Exists(mConfigInfo.WorkSpacePath))
            {
                FolderTree.OnGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region  Interval Tools

        private void Save()
        {
        }

        private string GetCurCommand( FolderTree.Data data )
        {
            var sb = new StringBuilder();

            sb.AppendFormat("set binaryOut={0}", mConfigInfo.BinaryExportPath);
            sb.AppendLine();
            sb.AppendFormat("set scriptOut={0}", mConfigInfo.ScriptExportPath);
            sb.AppendLine();
            sb.AppendFormat("set luaOut={0}", mConfigInfo.LuaExportPath);
            sb.AppendLine();
            sb.AppendFormat("set jsonOut={0}", mConfigInfo.JsonExportPath);
            sb.AppendLine();


            sb.AppendLine(@"bin\tabtoy.exe ^");
            sb.AppendLine(@"--mode=exportorv2 ^");

            var temp = "/" + data.name + ".cs";
            sb.AppendFormat("--csharp_out=%scriptOut%{0} ^", temp);
            sb.AppendLine();
            var combinename = string.Empty;
            if (mConfigInfo.mIsExportLuaForBit)
            {
                temp = "/" + data.name + ".bin";
                sb.AppendFormat("--binary_out=%binaryOut%{0} ^", temp);
                sb.AppendLine();
            }
            else if (mConfigInfo.mIsExportLua)
            {
                temp = "/" + data.name + ".lua";
                sb.AppendFormat("--lua_out=%luaOut%{0} ^", temp);
                sb.AppendLine();
            }
            else if (mConfigInfo.mIsExportJson)
            {
                if (string.IsNullOrEmpty(data.folderName) || data.folderName.Equals("Table"))
                {
                    temp = "/" + data.name + ".json";

                    //hack 加入子表匹配
                    var matchStr = data.name.Replace("Table", "");


                    var info = Directory.GetFiles(Path.GetDirectoryName(data.assetPath) ??
                                                  throw new InvalidOperationException(), "*.xlsx",
                        SearchOption.TopDirectoryOnly);

                    var regex = new Regex($"^{matchStr}_\\w");

                    foreach (var s in info)
                    {
                        var ss = Path.GetFileNameWithoutExtension(s);
                        if (regex.IsMatch(ss))
                        {
                            Debug.Log($"regex::{ss}");
                            combinename += $"+{ss}.xlsx";
                        }
                    }
                }
                else
                {
                    temp = "/" + data.folderName + "/" + data.name + ".json";
                }

                sb.AppendFormat("--json_out=%jsonOut%{0} ^", temp);

                sb.AppendLine();
            }


            sb.AppendLine(@"--mode=exportorv2 ^");
            temp = data.name + "Model";
            sb.AppendFormat("--combinename={0} ^", temp);


            sb.AppendLine();
            sb.AppendLine(@"--lan=zh_cn ^");

            Debug.Log($"combinename :: {combinename}");
            if (string.IsNullOrEmpty(combinename))
            {
                sb.AppendFormat(data.relationPath);
            }
            else
            {
                //combinename = combinename.Remove(combinename.Length - 1, 1);
                sb.AppendFormat($"{data.relationPath}{combinename}");
            }

            sb.AppendLine();
            sb.AppendLine(@"@IF %ERRORLEVEL% NEQ 0 pause");
            return sb.ToString();
        }

        /// <summary>
        /// 说明框
        /// </summary>
        /// <param name="message"></param>
        private void HelpLable( string message )
        {
            if (!mIsHelper)
            {
                return;
            }

            GUI.color = Color.yellow;
            GUILayout.Label(message, "label");
            GUI.color = Color.white;
        }

        private void ShowButton( Rect position )
        {
            mIsHelper = GUI.Toggle(position, mIsHelper, GUIContent.none, "IN LockButton");
            //Texture t = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Editor/TableConvert/FolderIcon.png");
            //GUIContent content = new GUIContent(AssetPreview.GetMiniThumbnail(t));
            //GUIContent content = new GUIContent(t);
            //mIsHelper = GUI.Toggle(position, mIsHelper, content);
        }

        public void AddItemsToMenu( GenericMenu menu )
        {
            menu.AddItem(new GUIContent("Lock"), mIsHelper, () => { mIsHelper = !mIsHelper; });
        }

        #region 右键设置

        private void OnMenuGUI()
        {
            var aEvent = Event.current;
            switch (aEvent.type)
            {
                case EventType.ContextClick:

                {
                    Event.current.Use();
                    break;
                }
                default:
                    return;
            }

            var pos = Event.current.mousePosition;
            if (mIsScroll)
            {
                pos -= mScrollPos;
            }

            pos.x += focusedWindow.position.xMin;
            pos.y += focusedWindow.position.yMin;

            if (focusedWindow == null || focusedWindow.position.Contains(pos))
            {
                //Rect rc = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 200, 50);
                GenerateContextMenu().ShowAsContext();
                Event.current.Use();
            }
        }

        private GenericMenu GenerateContextMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("导出ModelBase"), false, onModelBaseMenuFunc);
            menu.AddItem(new GUIContent("导出LuaBase"), false, onLuaBaseMenuFunc);
            return menu;
        }

        private void onModelBaseMenuFunc()
        {
            mModelBase.ExportModelBase();
        }

        private void onLuaBaseMenuFunc()
        {
        }

        #endregion

        #endregion

        #region CallBack

        private void OnFileClick( FolderTree.Data data )
        {
            var relationPath = data.relationPath;


            var command = GetCurCommand(data);

            var combineWorkFolder = mConfigInfo.WorkSpacePath;

            var batPath = mConfigInfo.WorkSpacePath + "/Export" + data.name;

            if (!string.IsNullOrEmpty(data.folderName) && !data.folderName.Equals("Table"))
            {
                batPath = mConfigInfo.WorkSpacePath + "/Export" + data.folderName + data.name;
            }

            //string scripPath = mConfigInfo.ScriptExportPath + "/" + data.name + ".cs";

            if (!File.Exists(batPath + ".bat"))
            {
                EditorFileUtility.WriteFile(batPath + ".bat", command, x =>
                {
                    Debug.Log(batPath + "----------------------" + x);
                    if (x.Equals("100.00%"))
                    {
                        TableTool.TableConvert(data, batPath, mConfigInfo);
                    }
                });
            }
            else
            {
                Debug.Log(batPath + "----------------------" + "100%");

                TableTool.TableConvert(data, batPath, mConfigInfo);
            }


            //TableTool.TableConvert(combineWorkFolder, command);
        }

        private void OnFolderClick( FolderTree.Data data )
        {
            if (!data.isFolder)
            {
                return;
            }

            foreach (var child in data.childs)
            {
                if (child.isFolder)
                {
                    OnFolderClick(child);
                    continue;
                }

                OnFileClick(child);
            }
            //string relationPath = data.relationPath;
            ////TODO:写入命令
            //string command = GetCurCommand( data );
            //string combineWorkFolder = mConfigInfo.WorkSpacePath;


            //string batPath = mConfigInfo.WorkSpacePath + "/Export" + data.name;

            ////string scripPath = mConfigInfo.ScriptExportPath + "/" + data.name + ".cs";

            //if (!File.Exists( batPath + ".bat" ))
            //{
            //    EditorFileUtility.WriteFile( batPath + ".bat", command, ( x ) =>
            //     {
            //         Debug.Log( "----------------------" + x );
            //         if (x.Equals( "100.00 %" ))
            //             TableTool.TableConvert( data, batPath, mConfigInfo );
            //     } );
            //}
            //else
            //{
            //    TableTool.TableConvert( data, batPath, mConfigInfo );
            //}

            //Debug.Log( File.Exists( batPath + ".bat" ) );

            //TableTool.TableConvert(combineWorkFolder, command);
        }

        #endregion
    }
}