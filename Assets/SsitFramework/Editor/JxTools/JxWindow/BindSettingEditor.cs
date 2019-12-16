/*
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：平台主程序入口                                                    
*│　作    者：xuxin                                              
*│　版    本：1.0.0                                                 
*│　创建时间：2019/11/16 14:58:19                             
*└──────────────────────────────────────────────────────────────┘
*/

using Framework.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(BindSetting))]
public class BindSettingEditor : Editor
{
    private BindSetting mSetting;
    private ReorderableList mReorderableList;
    private Vector2 mScrollPosition;


    private float mElementHeight = 20;
    private float textLableWidth = 150;
    public void OnEnable()
    {
        SerializedProperty refrence = serializedObject.FindProperty("m_builtInRefrence");

        mReorderableList = new ReorderableList(serializedObject, refrence, true, true, true, true);

        //绘制元素回调
        mReorderableList.elementHeight = mElementHeight;

        mReorderableList.drawElementCallback = ( rect, index, isActive, isFocused ) =>
        {
            EditorGUI.DrawRect(rect, new Color32(93, 255, 226, 30));

            Rect rect0 = rect;
            rect0.x += 0.5f; rect0.y -= 0.5f; rect0.width -= 1; rect0.height -= 1;
            EditorGUI.DrawRect(rect0, new Color32(0, 0, 0, 150));
            //rect.y += EditorGUIUtility.singleLineHeight;
            //EditorGUI.indentLevel = 0;
            //SerializedProperty[] commands = new SerializedProperty[refrence.Copy().CountRemaining()];
            //EditorGUILayout.PropertyField(commands[index], true);
            //EditorGUI.LabelField(new Rect(rect.x, rect.y, textLableWidth, EditorGUIUtility.singleLineHeight), "refrece");

            Rect rect1 = new Rect(rect.x, rect.y, rect0.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect1, refrence.GetArrayElementAtIndex(index), new GUIContent($"refrece dll_{index}"), true);

        };

        //绘制元素背景回调
        var defaultColor = GUI.backgroundColor;
        mReorderableList.drawElementBackgroundCallback = ( rect, index, isActive, isFocused ) =>
        {
            GUI.backgroundColor = Color.yellow;
        };
        //绘制表头回调
        mReorderableList.drawHeaderCallback = ( rect ) =>
            EditorGUI.LabelField(rect, "builtInRefrence");

        /*reorderableList.onAddDropdownCallback = delegate(Rect buttonRect, ReorderableList list)
        {
            isFadeout = EditorGUI.dr(buttonRect, "", isFadeout);
        };*/
        //移除元素回调
        mReorderableList.onRemoveCallback = ( ReorderableList l ) =>
        {
            if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the wave?", "Yes", "No"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
            }
        };
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawRefrenceView();
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawRefrenceView()
    {
        mScrollPosition = EditorGUILayout.BeginScrollView(mScrollPosition);

        mReorderableList.DoLayoutList();

        EditorGUILayout.EndScrollView();
    }
}
