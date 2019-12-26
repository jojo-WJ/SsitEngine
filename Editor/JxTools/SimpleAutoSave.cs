// Simple editor window that autosaves  the working scene  
//简单的编辑器窗口,可以自动保存工作场景
// Make sure to have this window opened to be able to execute the auto save. 
//确保这个窗口开着, 这样才能执行自动保存

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

internal class SimpleAutoSave : EditorWindow
{
    private double nextSave;

    private readonly double saveTime = 20;

    [MenuItem("Tools/Internal Tools/Simple autoSave")]
    private static void Init()
    {
        var window = GetWindowWithRect(typeof(SimpleAutoSave), new Rect(0, 0, 200, 40));
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Save Each:", saveTime + " Secs");
        var timeToSave = nextSave - EditorApplication.timeSinceStartup;
        EditorGUILayout.LabelField("Next Save:", timeToSave + " Sec");
        Repaint();

        if (EditorApplication.timeSinceStartup > nextSave)
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.isDirty && scene.IsValid())
            {
                var pre_scenePath = scene.path;
                var path = scene.path.Split(char.Parse("/"));
                path[path.Length - 1] = "AutoSave/AutoSave_" + scene.name;
                if (EditorSceneManager.SaveScene(scene, string.Join("/", path) + ".unity", true))
                {
                    Debug.LogWarning("场景自动备份完成");
                }
            }
            nextSave = EditorApplication.timeSinceStartup + saveTime;
        }
    }
}