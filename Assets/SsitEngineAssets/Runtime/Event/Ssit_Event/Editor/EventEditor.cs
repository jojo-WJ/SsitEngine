using Framework.SceneObject;
using SsitEngine.Unity;
using Framework.Event;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScriptScene))]
public class EventEditor : Editor
{
    [MenuItem("GameObject/CreateSceneConfig", priority = 49)]
    static void CreateSceneConfig()
    {
        //创建物体：在当前场景中创建一个GameObject
        GameObject emptyGo = GameObject.Find("SceneConfig");
        if (emptyGo == null)
        {
            emptyGo = new GameObject("SceneConfig");
        }
        emptyGo.GetOrAddComponent<ScriptScene>();
    }

    public ScriptScene _ScriptScene
    {
        get { return target as ScriptScene; }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();



        EditorGUILayout.BeginVertical();

        EditorGUILayout.HelpBox("场景配置管理辅助脚本" +
                                "1、统一处理场景中存在的交互组件，右键齿轮进行配置生成" +
                                "2、场景案列介绍动画编辑" +
                                "3、场景交互组件uuid生成", MessageType.Info);

        EditorGUILayout.BeginHorizontal("Box");

        if (GUILayout.Button("Open SceneAnimation Editor"))
        {
            EditorWindow.GetWindow<GUIEventWindow>().Show();
        }

        if (GUILayout.Button("Open SceneUUID Editor"))
        {
            //EditorWindow.GetWindow<swi>().Show();
            EditorApplication.ExecuteMenuItem("Tools/SceneTools/场景配置检索工具1.0.0");
        }


        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("根据需要可进行场景配置的筛选可支持两种配置" +
                                "1、场景中所有交互物体" +
                                "2、自定义场景交互物体（可选激活）", MessageType.Info);

        EditorGUILayout.BeginHorizontal("Box");

        if (GUILayout.Button("Create Scene Normal Config"))
        {
            //EditorWindow.GetWindow<swi>().Show();
            _ScriptScene.CreateSceneConfig();
        }

        if (GUILayout.Button("Create Scene Custom Config"))
        {
            //EditorWindow.GetWindow<swi>().Show();
            _ScriptScene.CreateSimpleSceneConfig();
        }

        EditorGUILayout.EndHorizontal();



        EditorGUILayout.EndVertical();
    }
}