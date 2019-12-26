using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace U3DExtends
{
    public class ReopenLayoutOnExitGame : MonoBehaviour
    {
#if UNITY_EDITOR
        // class ReopenInfo
        // {
        //     string path;
        //     Vector3 pos;
        // }
        private static ReopenLayoutOnExitGame Instance;

        private static readonly Dictionary<string, Vector3> layout_open_in_playmode = new Dictionary<string, Vector3>();
        private bool isRunningGame;

        public static void RecordOpenLayout( string path, Vector3 pos )
        {
            Debug.Log("record : " + path + " pos:" + pos);
            if (Instance != null && Instance.isRunningGame && path != "") layout_open_in_playmode.Add(path, pos);
        }

        private void Start()
        {
            Instance = this;
            // hadSaveOnRunTime = false;
            Debug.Log("Start");
            isRunningGame = true;
        }

        private void OnDisable()
        {
            // Debug.Log("disable");
            Instance = null;
        }

        private void OnTransformChildrenChanged()
        {
            Debug.Log("OnTransformChildrenChanged");
            var wait_delete_key = new List<string>();
            foreach (var item in layout_open_in_playmode)
            {
                var had_find = false;
                for (var i = 0; i < transform.childCount; i++)
                {
                    var info = transform.GetChild(i).GetComponent<LayoutInfo>();
                    if (info && info.LayoutPath == item.Key)
                    {
                        had_find = true;
                        break;
                    }
                }
                if (!had_find) wait_delete_key.Add(item.Key);
            }
            foreach (var item in wait_delete_key) layout_open_in_playmode.Remove(item);
        }

        private void OnApplicationQuit()
        {
            Debug.Log("OnApplicationQuit");
            isRunningGame = false;
            if (layout_open_in_playmode.Count > 0 && Configure.ReloadLayoutOnExitGame)
            {
                Action<PlayModeStateChange> p = null;
                p = c =>
                {
                    foreach (var item in layout_open_in_playmode)
                    {
                        // Debug.Log("item.Key : "+item.Key);
                        var layout = UIEditorHelper.LoadLayoutByPath(item.Key);
                        if (layout != null) layout.localPosition = item.Value;
                    }
                    layout_open_in_playmode.Clear();
                    EditorApplication.playModeStateChanged -= p;
                };
                EditorApplication.playModeStateChanged += p;
            }
        }
#endif
    }
}