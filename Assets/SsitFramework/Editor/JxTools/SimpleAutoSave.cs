// Simple editor window that autosaves  the working scene  
//简单的编辑器窗口,可以自动保存工作场景
// Make sure to have this window opened to be able to execute the auto save. 
//确保这个窗口开着, 这样才能执行自动保存

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

class SimpleAutoSave : EditorWindow
{

    double saveTime = 20;
    double nextSave = 0;

    [MenuItem("Tools/MyWindow/Simple autoSave")]

    static void Init()
    {
        var window = EditorWindow.GetWindowWithRect(typeof(SimpleAutoSave), new Rect(0, 0, 200, 40));
        window.Show();
    }
    void OnGUI()
    {
        EditorGUILayout.LabelField("Save Each:", saveTime + " Secs");
        double timeToSave = nextSave - EditorApplication.timeSinceStartup;
        EditorGUILayout.LabelField("Next Save:", timeToSave.ToString() + " Sec");
        this.Repaint();

        if (EditorApplication.timeSinceStartup > nextSave)
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.isDirty && scene.IsValid())
            {
                string pre_scenePath = scene.path;
                string[] path = scene.path.Split(char.Parse("/"));
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