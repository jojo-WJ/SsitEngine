/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/17 15:25:13                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Unity;
using UnityEditor;
using UnityEngine;

namespace SsitEngine.Editor.SsitEngineInspector
{
    [CustomEditor(typeof(PlatformConfig))]
    public class PlatConfig : UnityEditor.Editor
    {
        private SerializedProperty selectedTab;

        private readonly GUIContent[] tabsContents =
        {
            new GUIContent("系统设置"),
            new GUIContent("操作设置"),
            new GUIContent("待定")
        };

        private void OnEnable()
        {
            selectedTab = serializedObject.FindProperty("selectedTab");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawTabs();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawTabs()
        {
            selectedTab.intValue = GUILayout.Toolbar(selectedTab.intValue, tabsContents);
            switch (selectedTab.intValue)
            {
                case 0:
                    DrawSystemSet();
                    break;
                case 1:
                    DrawInputSet();
                    break;
                case 2:
                    break;
                case 3:
                    break;
            }
        }

        private void DrawSystemSet()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetFrameRate"), new GUIContent("系统帧率"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sleepTimeout"), new GUIContent("屏幕睡眠时间"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isSync"), new GUIContent("网络同步"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("timeTaskAgentMaxCount"),
                new GUIContent("计时器代理最大个数"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("webTaskAgentMaxCount"),
                new GUIContent("webRequest代理最大个数"));
        }

        private void DrawInputSet()
        {
            GUI.color = Color.cyan;
            EditorGUILayout.HelpBox("场景操作", MessageType.Info);
            GUI.color = Color.white;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("zoomSpeed"), new GUIContent("鼠标滑动速度"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("zoomMin"), new GUIContent("场景操作器的缩放下限高度"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("zoomMax"), new GUIContent("场景操作器的缩放上限高度"));

            GUI.color = Color.cyan;
            EditorGUILayout.HelpBox("场景移动操作", MessageType.Info);
            GUI.color = Color.white;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("movSpeed"), new GUIContent("移动速度"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("movAttachSpeed"),
                new GUIContent("移动速度shift加成"));


            GUI.color = Color.cyan;
            EditorGUILayout.HelpBox("场景旋转操作", MessageType.Info);
            GUI.color = Color.white;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rotSpeed"), new GUIContent("旋转速度"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rotPermissiDelta"), new GUIContent("旋转误差允许"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rotXAxisMin"), new GUIContent("场景操作器绕x轴下限高度"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rotXAxisMax"), new GUIContent("场景操作器绕x轴上限高度"));
        }
    }
}