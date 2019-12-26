/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：场景配置生成工具                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月25日 09点54分                             
*└──────────────────────────────────────────────────────────────┘
*/

using Framework.SceneObject;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 场景配置生成器、配置筛选器（GUID、EditorSceneConfig）
/// </summary>
public class SceneObjectGUISelectWindow : ScriptableWizard
{
    /// <summary>
    /// 配置导出格式
    /// </summary>
    private enum ExprotFormat
    {
        Json,
        Binary,
        XML,
    }

    /// <summary>
    /// 查询guid
    /// </summary>
    [Tooltip("查询guid")]
    public string guid = "";

    [Tooltip("查询guid")]
    public bool reset = false;

    /// <summary>
    /// 编辑时间
    /// </summary>
    [Tooltip("编辑时间")]
    public string time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

    [Tooltip("配置导出格式")]
    [SerializeField]
    private ExprotFormat mExprotFormat = ExprotFormat.Json;

    /// <summary>
    /// 场景更新列表
    /// </summary>
    [Tooltip("场景更新列表")]
    public GameObject[] UpdateList = new GameObject[] { };


    [MenuItem("Tools/SceneTools/场景配置检索工具1.0.0 %k",priority = 999)]
    static void CreateDeSer()
    {
        ScriptableWizard.DisplayWizard<SceneObjectGUISelectWindow>("场景配置检索工具1.0.0", "查询", "更新");
    }

    void OnEnable()
    {
        FindGameObject("999999999", true);
        helpString = "工具说明:(Ctrl + K)\n辅助场景编辑人员对场景交互物体物体进行统一Guid的构造和场景配置文件的导出\n" +
                     "      1、检索场景guid的唯一性,\n" +
                     "      2、对于场景中存在的交互对象进行guid的统一设定和更新\n" +
                     "      3、通过场景数据的guid查找场景中具体的GameObject对象(调试使用)\n" +
                     "      4、对于最新修改的数据进行原始数据的兼容和支持\n" +
                     "      5、[新增]Reset选项可强制进行guid的全部重新分配工作";

    }

    void OnWizardUpdate()
    {
        if (UpdateList.Length > 0)
        {
            errorString = "场景交互物体存在改变，请及时更新，否则影响程序运行";
            ShowNotification(new GUIContent(errorString));
        }
        else
        {
            errorString = "";
        }
        if (!string.IsNullOrEmpty(guid) || UpdateList.Length > 0)
        {
            isValid = true;
        }
        else
        {
            isValid = false;
        }

    }

    protected override bool DrawWizardGUI()
    {
        OnMenuGUI();
        return base.DrawWizardGUI();
    }

    void OnWizardCreate()
    {
        try
        {
            FindGameObject(guid);
        }
        catch (Exception)
        {
            Debug.LogError("日期格式有误");
        }

    }

    void OnWizardOtherButton()
    {
        GUIDGenerator();
    }

    private void OnFocus()
    {
        FindGameObject("999999999", true);
        OnWizardUpdate();
        this.Repaint();
    }

    #region 右键设置

    void OnMenuGUI()
    {
        if (!mouseOverWindow)
        {
            return;
        }
        Event aEvent = Event.current;
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
        Vector2 pos = Event.current.mousePosition;
        pos.x += focusedWindow.position.xMin;
        pos.y += focusedWindow.position.yMin;

        if ((focusedWindow == null) || focusedWindow.position.Contains(pos))
        {
            //Rect rc = new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 200, 50);
            GenerateContextMenu().ShowAsContext();
            Event.current.Use();
        }
    }

    private GenericMenu GenerateContextMenu()
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("导出场景配置"), false, OnExportSceneConfig);
        menu.AddItem(new GUIContent("待定"), false, OnTest);
        return menu;
    }
    private void OnExportSceneConfig()
    {
        //todo:导出场景配置文件
        Debug.Log(ColorFormat("待开发，敬请期待")); ;

        switch (mExprotFormat)
        {
            case ExprotFormat.Json:
                break;
            case ExprotFormat.Binary:
                break;
            case ExprotFormat.XML:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    private void OnTest()
    {
        return;
    }


    #endregion
    /// <summary>
    /// 附加标志颜色
    /// </summary>
    /// <param name="tex"></param>
    /// <returns></returns>
    string ColorFormat(string tex)
    {
        return "<color='#FFD700'>" + tex + "</color>";
    }

    private string GetTimestamp(string t)
    {
        string[] tmp = t.Split('-');
        int year = int.Parse(tmp[0]);
        int month = int.Parse(tmp[1]);
        int day = int.Parse(tmp[2]);
        int hour = int.Parse(tmp[3]);
        int minute = int.Parse(tmp[4]);
        int second = int.Parse(tmp[5]);
        DateTime DateNow = new DateTime(year, month, day, hour, minute, second);
        DateTime DateStart = new DateTime(1970, 1, 1, 8, 0, 0);
        return (DateNow - DateStart).TotalSeconds.ToString();
    }

    GameObject FindGameObject(string id, bool isRepaint = false)
    {
        if (!string.IsNullOrEmpty(id))
        {
            Boo.Lang.List<GameObject> sceneObjects = new Boo.Lang.List<GameObject>();
            BaseSceneInstance[] baseSceneInstances = Resources.FindObjectsOfTypeAll<BaseSceneInstance>();
            for (int i = 0; i < baseSceneInstances.Length; i++)
            {

                BaseSceneInstance sceneInstance = baseSceneInstances[i];
                // 过滤不可编辑的对象
                if (sceneInstance.hideFlags == HideFlags.NotEditable || sceneInstance.hideFlags == HideFlags.HideAndDontSave)
                    continue;
                // 过滤存在磁盘上的对象
                if (EditorUtility.IsPersistent(sceneInstance.gameObject))
                    continue;

                if (string.IsNullOrEmpty(sceneInstance.Guid))
                {
                    sceneObjects.Add(sceneInstance.gameObject);
                }
                if (sceneInstance.Guid == id)
                {
                    Selection.SetActiveObjectWithContext(sceneInstance, null);
                    Debug.Log("GUID: " + id + " GameObject: " + sceneInstance.gameObject.name, sceneInstance.gameObject);
                    return sceneInstance.gameObject;
                }
            }

            UpdateList = sceneObjects.ToArray();
            if (!isRepaint)
                Debug.Log(ColorFormat("查询guid不存在"));
        }
        return null;
    }

    void GUIDGenerator()
    {
        List<BaseSceneInstance> baseSceneObjects = GetAllObjectsInScene<BaseSceneInstance>();

        // 首先在场景中查找没有生成GUID的交互对象并确定当前最大guid的值
        List<BaseSceneInstance> updateList = new List<BaseSceneInstance>();

        // 通过字典检测已有的guid是否存在重复现象
        Dictionary<string, BaseSceneInstance> checkRepeatDic = new Dictionary<string, BaseSceneInstance>();
        //ulong maxUid = 10000;
        //ulong uid = maxUid;
        for (int i = 0; i < baseSceneObjects.Count; i++)
        {
            BaseSceneInstance sceneObj = baseSceneObjects[i];

            if (reset)
            {
                updateList.Add(sceneObj);
                continue;
            }

            if (string.IsNullOrEmpty(sceneObj.Guid))
            {
                updateList.Add(sceneObj);
            }
            else
            {
                checkRepeatDic.Add(sceneObj.Guid, sceneObj);
                //UInt64.TryParse(sceneObj.guid, out uid);
                //if (uid > maxUid)
                //{
                //    maxUid = uid;
                //}
            }
        }

        if (updateList.Count == 0)
        {
            Debug.Log("更新场景交互物体的GUID 总数==" + updateList.Count);
            return;
        }

        EditorUtility.DisplayProgressBar("正在搜集交互物体", "请稍候", 0);
        //var guid = new UlGuid(maxUid + 1);
        for (int i = 0; i < updateList.Count; i++)
        {
            BaseSceneInstance sceneInstance = updateList[i];
            Undo.RecordObject(sceneInstance, sceneInstance.name);

            //var id = guid.GenerateNewId();
            sceneInstance.Guid = sceneInstance.name;
            //var tt = sceneInstance.GetComponent<UnityEngine.Networking.NetworkIdentity>();
            //if (tt)
            //{
            //    Destroy(tt);
            //}

            EditorUtility.DisplayProgressBar("正在构造场景交互物体的GUID", string.Format("正在生成{0}/{1}", sceneInstance.gameObject.name, sceneInstance.Guid), i / updateList.Count);
        }
        Undo.ClearAll();
        Debug.Log("更新场景交互物体的GUID 总数==" + updateList.Count);
        EditorUtility.ClearProgressBar();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
    }

    static List<T> GetAllObjectsInScene<T>() where T : MonoBehaviour
    {
        List<T> objectsInScene = new List<T>();

        foreach (T go in Resources.FindObjectsOfTypeAll<T>())
        {
            if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                continue;

            //过滤存在磁盘上的对象
            if (EditorUtility.IsPersistent(go.gameObject))
                continue;

            objectsInScene.Add(go);
        }

        return objectsInScene;
    }


}