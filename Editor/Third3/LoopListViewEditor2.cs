using SuperScrollView;
using UnityEditor;
using UnityEngine;

namespace SsitEngine.Editor
{
    [CustomEditor(typeof(LoopListView2))]
    public class LoopListViewEditor2 : UnityEditor.Editor
    {
        private SerializedProperty mArrangeType;
        private readonly GUIContent mArrangeTypeGuiContent = new GUIContent("ArrangeType");
        private SerializedProperty mItemPrefabDataList;
        private readonly GUIContent mItemPrefabListContent = new GUIContent("ItemPrefabList");
        private SerializedProperty mItemSnapEnable;
        private readonly GUIContent mItemSnapEnableContent = new GUIContent("ItemSnapEnable");
        private SerializedProperty mItemSnapPivot;
        private readonly GUIContent mItemSnapPivotContent = new GUIContent("ItemSnapPivot");

        private SerializedProperty mSupportScrollBar;

        private readonly GUIContent mSupportScrollBarContent = new GUIContent("SupportScrollBar");
        private SerializedProperty mViewPortSnapPivot;
        private readonly GUIContent mViewPortSnapPivotContent = new GUIContent("ViewPortSnapPivot");

        protected virtual void OnEnable()
        {
            mSupportScrollBar = serializedObject.FindProperty("mSupportScrollBar");
            mItemSnapEnable = serializedObject.FindProperty("mItemSnapEnable");
            mArrangeType = serializedObject.FindProperty("mArrangeType");
            mItemPrefabDataList = serializedObject.FindProperty("mItemPrefabDataList");
            mItemSnapPivot = serializedObject.FindProperty("mItemSnapPivot");
            mViewPortSnapPivot = serializedObject.FindProperty("mViewPortSnapPivot");
        }


        private void ShowItemPrefabDataList( LoopListView2 listView )
        {
            EditorGUILayout.PropertyField(mItemPrefabDataList, mItemPrefabListContent);
            if (mItemPrefabDataList.isExpanded == false)
            {
                return;
            }
            EditorGUI.indentLevel += 1;
            if (GUILayout.Button("Add New"))
            {
                mItemPrefabDataList.InsertArrayElementAtIndex(mItemPrefabDataList.arraySize);
                if (mItemPrefabDataList.arraySize > 0)
                {
                    var itemData = mItemPrefabDataList.GetArrayElementAtIndex(mItemPrefabDataList.arraySize - 1);
                    var mItemPrefab = itemData.FindPropertyRelative("mItemPrefab");
                    mItemPrefab.objectReferenceValue = null;
                }
            }
            var removeIndex = -1;
            EditorGUILayout.PropertyField(mItemPrefabDataList.FindPropertyRelative("Array.size"));
            for (var i = 0; i < mItemPrefabDataList.arraySize; i++)
            {
                var itemData = mItemPrefabDataList.GetArrayElementAtIndex(i);
                var mInitCreateCount = itemData.FindPropertyRelative("mInitCreateCount");
                var mItemPrefab = itemData.FindPropertyRelative("mItemPrefab");
                var mItemPrefabPadding = itemData.FindPropertyRelative("mPadding");
                var mItemStartPosOffset = itemData.FindPropertyRelative("mStartPosOffset");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(itemData);
                if (GUILayout.Button("Remove"))
                {
                    removeIndex = i;
                }
                EditorGUILayout.EndHorizontal();
                if (itemData.isExpanded == false)
                {
                    continue;
                }
                mItemPrefab.objectReferenceValue = EditorGUILayout.ObjectField("ItemPrefab",
                    mItemPrefab.objectReferenceValue, typeof(GameObject), true);
                mItemPrefabPadding.floatValue =
                    EditorGUILayout.FloatField("ItemPadding", mItemPrefabPadding.floatValue);
                if (listView.ArrangeType == ListItemArrangeType.TopToBottom ||
                    listView.ArrangeType == ListItemArrangeType.BottomToTop)
                {
                    mItemStartPosOffset.floatValue =
                        EditorGUILayout.FloatField("XPosOffset", mItemStartPosOffset.floatValue);
                }
                else
                {
                    mItemStartPosOffset.floatValue =
                        EditorGUILayout.FloatField("YPosOffset", mItemStartPosOffset.floatValue);
                }
                mInitCreateCount.intValue = EditorGUILayout.IntField("InitCreateCount", mInitCreateCount.intValue);
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            if (removeIndex >= 0)
            {
                mItemPrefabDataList.DeleteArrayElementAtIndex(removeIndex);
            }
            EditorGUI.indentLevel -= 1;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var tListView = serializedObject.targetObject as LoopListView2;
            if (tListView == null)
            {
                return;
            }
            ShowItemPrefabDataList(tListView);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(mSupportScrollBar, mSupportScrollBarContent);
            EditorGUILayout.PropertyField(mItemSnapEnable, mItemSnapEnableContent);
            if (mItemSnapEnable.boolValue)
            {
                EditorGUILayout.PropertyField(mItemSnapPivot, mItemSnapPivotContent);
                EditorGUILayout.PropertyField(mViewPortSnapPivot, mViewPortSnapPivotContent);
            }
            EditorGUILayout.PropertyField(mArrangeType, mArrangeTypeGuiContent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}