using SsitEngine.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SsitEngine.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Event = UnityEngine.Event;
using EventType = UnityEngine.EventType;
using Object = UnityEngine.Object;
using Rect = UnityEngine.Rect;

[AddComponentMenu("GameObject/jojo")]
public class SerializeBindWindow : EditorWindow, IHasCustomMenu
{
    [Serializable]
    public class SeriableItem
    {
        [NonSerialized]
        public GameObject go;
        public string fieldName;
        public string typeName;
        public string previewStr;

        [NonSerialized]
        public int selectIndex;
        [NonSerialized]
        public string filterStr;
        [NonSerialized]
        public bool isVaild;

    }

    private List<SeriableItem> mSeriableData = new List<SeriableItem>();

    private ReorderableList reorderableList;
    //private bool isFadeout = false;
    private Texture2D mTexture;
    private static float textLableWidth = 150;
    private static float enumWidth = 300;
    private static float elementHeight = 40;

    /// <summary>
    /// Keep local copy of lock button style for efficiency.
    /// </summary>
    [NonSerialized] private GUIStyle lockButtonStyle;
    /// <summary>
    /// Indicates whether lock is toggled on/off.
    /// </summary>
    [NonSerialized] private bool locked = false;

    private static EditorWindow mWindow;
    private bool mIsScroll;
    private Vector2 mScrollPosition;

    [MenuItem("GameObject/序列化绑定生成工具 #&i", priority = 40)]
    private static void Init()
    {
        mWindow = GetWindow(typeof(SerializeBindWindow));
        mWindow.Show();
    }

    private void InitData()
    {
        mGo = Selection.activeGameObject;
        InitBindForm();
    }

    private void OnEnable()
    {
        InitData();
        mTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/JxTools/JxWindow/Line.png");

        reorderableList = new ReorderableList(mSeriableData, typeof(SeriableItem));


        //绘制元素回调
        reorderableList.elementHeight = elementHeight;
        reorderableList.drawElementCallback =
            ( rect, index, isActive, isFocused ) =>
            {
                var bgRect = rect;
                EditorGUI.DrawRect(rect, new Color32(0, 0, 0, 60));
                bgRect.x += 0.5f; bgRect.y -= .5f; bgRect.width -= 1; bgRect.height -= 1;
                EditorGUI.DrawRect(bgRect, new Color32(166, 255, 255, 100));
                var element = mSeriableData[index];
                if (element == null)
                    element = new SeriableItem();
                var rectControl = rect;
                {
                    EditorGUI.BeginChangeCheck();
                    element.go = EditorGUI.ObjectField(
                        new Rect(rectControl.x, rectControl.y, textLableWidth * 2, EditorGUIUtility.singleLineHeight),
                        new GUIContent("序列化对象: "), element.go, typeof(GameObject), true) as GameObject;
                    if (EditorGUI.EndChangeCheck())
                    {
                        element.selectIndex = 0;
                        element.typeName = element.fieldName = "";
                    }

                    rectControl.x += textLableWidth * 2;
                    //获取序列化对象上的组件
                    if (element.go != null)
                    {
                        var components = element.go.GetComponents<Component>();

                        var compStrings = components.ToList().ConvertAll(( x ) => { return x.GetType().Name; }).Where(component =>
                        {
                            if (!string.IsNullOrEmpty(element.filterStr) && !Regex.IsMatch(component, element.filterStr, RegexOptions.IgnoreCase)) return false;
                            return true;
                        }).ToArray();

                        if (!string.IsNullOrEmpty(element.typeName))
                        {
                            element.selectIndex = compStrings.ToList().FindIndex((s => s == element.typeName));
                        }

                        element.selectIndex = element.selectIndex == -1 ? 0 : element.selectIndex;
                        element.selectIndex = EditorGUI.Popup(
                            new Rect(rectControl.x, rectControl.y, enumWidth, EditorGUIUtility.singleLineHeight),
                            element.selectIndex,
                            compStrings);
                        if (compStrings.Length > 0) element.typeName = compStrings[element.selectIndex];
                    }
                    rectControl.x += enumWidth;

                    element.filterStr = EditorGUI.TextField(new Rect(rectControl.x, rectControl.y, textLableWidth * 2, EditorGUIUtility.singleLineHeight),
                         new GUIContent("快速定位"), element.filterStr);

                    EditorGUI.DrawTextureTransparent(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, 0.5f)
                        , mTexture, ScaleMode.ScaleAndCrop);

                    //setting
                    rectControl = rect;
                    rectControl.y += EditorGUIUtility.singleLineHeight + .5f;

                    element.fieldName = EditorGUI.TextField(new Rect(rectControl.x, rectControl.y, textLableWidth * 2, EditorGUIUtility.singleLineHeight),
                      new GUIContent("字段序列化显示名称"), element.fieldName);

                    if (!string.IsNullOrEmpty(element.typeName) && !string.IsNullOrEmpty(element.fieldName))
                    {
                        element.isVaild = true;
                        element.previewStr = $"private {element.typeName} {element.fieldName};";
                    }
                    else
                    {
                        element.isVaild = false;
                        element.previewStr = string.Empty;
                    }
                    rectControl.x += textLableWidth * 2 + 5;
                    EditorGUI.LabelField(new Rect(rectControl.x, rectControl.y, textLableWidth, EditorGUIUtility.singleLineHeight),
                     new GUIContent("预览"), new GUIStyle("LargeButton"));
                    rectControl.x += textLableWidth + 5;

                    EditorGUI.LabelField(new Rect(rectControl.x, rectControl.y, textLableWidth * 2, EditorGUIUtility.singleLineHeight),
                    new GUIContent(element.previewStr), new GUIStyle("AssetLabel Partia"));
                }
            };

        //绘制元素背景回调
        var defaultColor = GUI.backgroundColor;
        reorderableList.drawElementBackgroundCallback = ( rect, index, isActive, isFocused ) =>
        {
            GUI.backgroundColor = Color.yellow;
        };
        //绘制表头回调
        reorderableList.drawHeaderCallback = ( rect ) =>
            EditorGUI.LabelField(rect, "elment");

        /*reorderableList.onAddDropdownCallback = delegate(Rect buttonRect, ReorderableList list)
        {
            isFadeout = EditorGUI.dr(buttonRect, "", isFadeout);
        };*/
        //移除元素回调
        reorderableList.onRemoveCallback = ( ReorderableList l ) =>
        {
            if (EditorUtility.DisplayDialog("Warning!",
                "Are you sure you want to delete the wave?", "Yes", "No"))
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
        };
        reorderableList.onSelectCallback = ( ReorderableList l ) =>
        {
            //预设高亮
            /*var prefab = l.serializedProperty.GetArrayElementAtIndex(l.index).
                FindPropertyRelative("Prefab").objectReferenceValue as GameObject;
            if (prefab)
                EditorGUIUtility.PingObject(prefab.gameObject);*/


        };
    }

    private void OnGUI()
    {
        DrawToolbar();

        mScrollPosition = GUILayout.BeginScrollView(mScrollPosition);
        reorderableList.DoLayoutList();


        var tempObj = GetOjbect();

        if (tempObj != null) Debug.LogError(tempObj.name);

        //Draw Check
        DrawCheck();
        //Draw Preview
        DrawPreview();



        GUILayout.EndScrollView();

        showRightMenu();
    }



    private GameObject mGo;
    private string mScripPath;
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal("HelpBox");
        //GUILayout.Label("快速查找相关子物体", "label");
        EditorGUILayout.LabelField("检索:", GUILayout.MaxWidth(80));

        /*mFilter = EditorGUILayout.TextField("筛选", mFilter);

        if (GUILayout.Button("x", GUILayout.Width(30)))
        {
            mFilter = "";
            GUI.FocusControl("");
        }
        */
        EditorGUI.BeginChangeCheck();

        mGo = EditorGUILayout.ObjectField("UI Form: ", mGo, typeof(GameObject), true, GUILayout.MinWidth(200)) as GameObject;
        if (EditorGUI.EndChangeCheck())
        {
            InitBindForm();
        }


        EditorGUILayout.EndHorizontal();
    }

    private void InitBindForm()
    {
        if (mGo != null)
        {
            //var xx = mGo.GetComponent(mGo.name);

            mScripPath = FindScript(mGo.name);

            if (!string.IsNullOrEmpty(mScripPath))
            {
                ParseCodeRes(mScripPath);
            }
        }
        else
        {
            mScripPath = null;
        }
    }

    public static Object GetOjbect( string meg = null )
    {
        Event aEvent;
        aEvent = Event.current;
        GUI.contentColor = Color.white;
        Object temp = null;

        var dragArea = GUILayoutUtility.GetRect(elementHeight, 35f, GUILayout.ExpandWidth(true));

        var title = new GUIContent(meg);
        if (string.IsNullOrEmpty(meg)) title = new GUIContent("Drag Object here from Project view to get the object");

        GUI.Box(dragArea, title);
        switch (aEvent.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dragArea.Contains(aEvent.mousePosition)) break;

                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                if (aEvent.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    for (var i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                    {
                        temp = DragAndDrop.objectReferences[i];

                        if (temp == null) break;
                    }
                }

                Event.current.Use();
                break;
            default:
                break;
        }

        return temp;
    }

    private void DrawPreview()
    {
        if (mSeriableData == null)
            return;

        EditorGUILayout.BeginVertical("HelpBox");

        EditorGUILayout.LabelField(new GUIContent("Preview"), new GUIStyle("AssetLabel Partia"));

        var height = GeneratorCode() * EditorGUIUtility.singleLineHeight;

        EditorGUILayout.LabelField(new GUIContent(m_stringBuilder.ToString()), new GUIStyle("en_text"), GUILayout.MinHeight(height));


        EditorGUILayout.EndVertical();

    }

    private void DrawCheck()
    {
        if (mSeriableData == null)
        {
            return;
        }

        //检测绑定对象名称唯一性
        var bindNames = mSeriableData.Where((item => item.go)).ToList().ConvertAll((x =>
        {
            return x.go.name;
        }));

        if (bindNames.Distinct().Count() != bindNames.Count)
        {
            EditorGUILayout.HelpBox("<size=18>绑定对象名重复，深度查询异常</size>", MessageType.Error, true);

        }

        //检测字段名称唯一性

        var fieldNames = mSeriableData.Where((item => item.go)).ToList().ConvertAll((x => x.fieldName));

        if (fieldNames.Distinct().Count() != fieldNames.Count)
        {
            EditorGUILayout.HelpBox("字段名重复，脚本编译异常", MessageType.Error);
        }
    }

    #region 右键

    /// <summary>
    /// Magic method which Unity detects automatically.
    /// </summary>
    /// <param name="position">Position of button.</param>
    private void ShowButton( Rect position )
    {
        if (lockButtonStyle == null)
            lockButtonStyle = "IN LockButton";
        locked = GUI.Toggle(position, locked, GUIContent.none, lockButtonStyle);
    }

    /// <summary>
    /// Adds custom items to editor window context menu.
    /// </summary>
    /// <remarks>
    /// <para>This will only work for Unity 4.x+</para>
    /// </remarks>
    /// <param name="menu">Context menu.</param>
    void IHasCustomMenu.AddItemsToMenu( GenericMenu menu )
    {
        menu.AddItem(new GUIContent("Lock"), locked, () =>
        {
            locked = !locked;
        });
    }

    public void showRightMenu()
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
            pos -= mScrollPosition;

        pos.x += focusedWindow.position.xMin;
        pos.y += focusedWindow.position.yMin;

        if (focusedWindow == null || focusedWindow.position.Contains(pos))
        {
            var rc = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 200, 50);
            GenerateContextMenu().ShowAsContext();

            Event.current.Use();
        }
    }

    private GenericMenu GenerateContextMenu()
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("保存"), false, OnMenuFunc, mSeriableData);

        return menu;
    }

    private void OnMenuFunc( object userData )
    {
        Debug.Log("右键保存");

        ExportCode();
    }

    #endregion

    #region Code Parse


    private void ParseCodeRes( string scripPath )
    {
        var fileContent = File.ReadAllText(scripPath, Encoding.UTF8);

        var beginIndex = fileContent.IndexOf(mGeneratorCodeBegin);

        if (beginIndex != -1)
        {
            var endIndex = fileContent.Substring(beginIndex).IndexOf(mGeneratorCodeEnd);

            var scripMoudle = fileContent.Substring(beginIndex, endIndex + mGeneratorCodeEnd.Length);
            //Debug.Log("scripMoudle" + scripMoudle);

            //fileContent = fileContent.Replace(scripMoudle, m_stringBuilder.ToString());
            if (mSeriableData == null)
                mSeriableData = new List<SeriableItem>();
            else
                mSeriableData.Clear();
            var splitStr = scripMoudle.Split('\n');

            for (var i = 0; i < splitStr.Length; i++)
            {
                //解析bind对象
                SeriableItem item = null;

                if (splitStr[i].Contains("[AddBindPath(") && splitStr[i].Contains("SerializeField"))
                {
                    var tt = splitStr[i].Split(new string[] { "\"", "\"" }, StringSplitOptions.RemoveEmptyEntries);
                    //Debug.Log("splitStr[i]" + splitStr[i] + tt.Length);

                    item = new SeriableItem();
                    item.go = mGo.transform.FindDeepChild(tt[1])?.gameObject;

                    //解析类型及字段显示名称
                    if (splitStr[i + 1].Contains("private"))
                    {
                        tt = splitStr[i + 1].Split(new string[] { " ", ";" }, StringSplitOptions.RemoveEmptyEntries);
                        item.typeName = tt[1];
                        item.fieldName = tt[2];
                    }

                    mSeriableData.Add(item);

                }

            }
        }
        else
        {
            //todo:没有响应代码块，需要生成
            var strings = fileContent.Split('\n');


        }
    }

    #endregion

    #region Code Generator

    private StringBuilder m_stringBuilder;

    private readonly string mAtrributefileter = "\t\t[AddBindPath(\"{0}\"), SerializeField]";
    private readonly string mGeneratorCodeBegin = "#region Serialize Variable Generator";
    private readonly string mGeneratorCodeEnd = "#endregion";

    private int GeneratorCode()
    {
        var height = 0;
        DrawCodeMoudleBegin();
        height++;
        //todo:
        for (var i = 0; i < mSeriableData.Count; i++)
            if (mSeriableData[i].isVaild)
            {
                height++;
                m_stringBuilder.AppendLine(string.Format(mAtrributefileter, mSeriableData[i].go.name));

                height++;
                m_stringBuilder.AppendLine($"\t\t{mSeriableData[i].previewStr}");
            }


        DrawCodeMoudleEnd();
        height++;

        return height;
    }

    private void DrawCodeMoudleBegin()
    {
        m_stringBuilder = new StringBuilder();

        m_stringBuilder.AppendLine($"{mGeneratorCodeBegin}");
    }

    private void DrawCodeMoudleEnd()
    {
        m_stringBuilder.AppendLine($"{mGeneratorCodeEnd}");
    }

    private void ExportCode()
    {
        if (mGo == null)
        {
            return;
        }
        if (!string.IsNullOrEmpty(mScripPath))
        {
            var fileContent = File.ReadAllText(mScripPath, Encoding.UTF8);


            var beginIndex = fileContent.IndexOf(mGeneratorCodeBegin);

            if (beginIndex != -1)
            {
                var endIndex = fileContent.Substring(beginIndex).IndexOf(mGeneratorCodeEnd);

                var scripMoudle = fileContent.Substring(beginIndex, endIndex + mGeneratorCodeEnd.Length);


                fileContent = fileContent.Replace(scripMoudle, m_stringBuilder.ToString());

            }
            File.WriteAllText(mScripPath, fileContent, Encoding.UTF8);
        }
        else
        {
            mScripPath = EditorUtility.SaveFilePanel("保存路径", Application.dataPath + scriptsPath, mGo.name, "cs");
            if (!string.IsNullOrEmpty(mScripPath))
            {
                string assetPath = mScripPath.Replace(Application.dataPath, "");//.Replace(".cs","");

                ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
                   CreateInstance<CreateUIFormScriptAsset>(),
                   EditorCreateScripts.GetSelectedPathOrFallback() + assetPath,
                   null,
                   "Assets/Editor/AutoCreateScript/ScriptsTemplate/UIFormClass.txt");

                Selection.activeGameObject = mGo;

                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                InitBindForm();
            }
        }
    }

    #endregion

    #region 资源检索

    private MonoScript scriptObj = null;

    private string scriptsPath = "/Scripts";

    private string FindScript( string scriptName )
    {
        var prefabs_names = new List<string>();
        var tempStr = Application.dataPath + scriptsPath;
        if (Directory.Exists(tempStr))
        {
            var direction = new DirectoryInfo(tempStr);
            var files = direction.GetFiles("*", SearchOption.AllDirectories);
            for (var i = 0; i < files.Length; i++)
                //Debug.Log("FindScript" + Path.GetFileNameWithoutExtension(files[i].Name));
                if (files[i].Name.EndsWith(".cs") && Path.GetFileNameWithoutExtension(files[i].Name) == scriptName) return files[i].FullName;
        }
        return string.Empty;
    }

    #endregion
}
