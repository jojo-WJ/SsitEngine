using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AssetBindingAttribute : PropertyAttribute
{
    /// <summary>
    /// 检索方向枚举
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// 上升ascend
        /// </summary>
        ASC,
        /// <summary>
        /// 下降descend
        /// </summary>
        DESC
    }

    public int Limit = 10;

    /// <summary>
    /// <para>true 是否搜索</para>
    /// <para>false检索</para>
    /// </summary>
    public bool search = true;


    /// <summary>
    /// <para>检索的顺序</para>
    /// </summary>
    public Direction direction = Direction.ASC;

    /// <summary>
    /// 是否检索完成
    /// </summary>
    public bool cache = false;
    public bool error = false;

    public string[] subLableName;
    public string[] subAssetName;


    /// <summary>
    /// <para>检索的字典缓存</para>
    /// </summary>
    public static Dictionary<string, System.Type> assetTypes = new Dictionary<string, System.Type>();



    /// <summary>
    /// 绑定资源名的数组
    /// </summary>
    /// <param name="limit"></param>
    /// <param name="subAssetName"></param>
    public AssetBindingAttribute(int limit, string[] subLabelName, string[] subAssetName)
    {
        this.Limit = limit;
        if (Mathf.Sign(limit) == 1)
        {
            limit = Mathf.Clamp(limit, 0, 10);
        }
        this.subLableName = subLabelName;
        this.subAssetName = subAssetName;
    }
    public AssetBindingAttribute(int limit, string[] subLabelName)
    {
        this.Limit = limit;
        if (Mathf.Sign(limit) == 1)
        {
            limit = Mathf.Clamp(limit, 0, 10);
        }
        this.subLableName = subLabelName;
    }
    public AssetBindingAttribute(int limit)
    {
        this.Limit = limit;
        if (Mathf.Sign(limit) == 1)
        {
            limit = Mathf.Clamp(limit, 0, 10);
        }
    }

    void InitAttribute()
    {
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(AssetBindingAttribute))]
public class AssetBindingDrawer : PropertyDrawer
{
    /// <summary>
    /// GUI的高度
    /// </summary>
    private const int CONTENT_HEIGHT = 20;
    public SerializedProperty rootProperty;
    public int index = -1;
    public AssetBindingAttribute BindingAttribute
    {
        get { return (AssetBindingAttribute)attribute; }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (BindingAttribute.error)
        {
            if (rootProperty != null && GetProperTyIndex(property) == rootProperty.arraySize - 1)
            {
                return base.GetPropertyHeight(property, label) + CONTENT_HEIGHT;
            }
        }
        return base.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginDisabledGroup(true);
        if (rootProperty == null)
        {
            //获取属性类对像的类型
            //System.Type type = property.serializedObject.targetObject.GetType();
            //FieldInfo field = type.GetField(this.fieldInfo.Name);
            rootProperty = property.serializedObject.FindProperty(this.fieldInfo.Name);
        }
        else
        {
            if (rootProperty.isArray)
            {
                if (!CheckLimit())
                {
                    EditorGUI.EndDisabledGroup();
                    return;
                }
                index = GetProperTyIndex(property);
                if (index != -1)
                {
                    //判断绑定资源名
                    if (BindingAttribute.subAssetName != null)
                    {
                        //如果没有筛选标签就不做遍历
                        if (BindingAttribute.subAssetName.Length != rootProperty.arraySize)
                        {
                            DrawHelpBox(position, index, "资源列表Length不匹配{0}");
                        }
                        else if (string.IsNullOrEmpty(BindingAttribute.subAssetName[index]))
                        {
                            DrawHelpBox(position, index, "绑定资源{0}为空");
                        }
                        //如果搜寻
                        else if (BindingAttribute.search)
                        {
                            if (property.propertyType == SerializedPropertyType.ObjectReference)
                            {
                                if (property.objectReferenceValue != null &&
                                    property.objectReferenceValue.name == BindingAttribute.subAssetName[index])
                                {
                                    BindingAttribute.cache = true;
                                }
                                else
                                {
                                    BindingAttribute.cache = false;
                                }
                                if (!BindingAttribute.cache)
                                {
                                    property.objectReferenceValue = null;
                                    DrawSingleProperty(position, property, label, index);
                                }
                            }
                        }
                    }
                }

            }
        }
        DrawCachedSingleProperty(position, property, label, index);
        EditorGUI.EndDisabledGroup();
        GUI.color = Color.white;
    }

    void DrawSingleProperty(Rect position, SerializedProperty property, GUIContent label, int index)
    {
        System.Type type = GetPropertyType(property);
        foreach (string path in GetAllAssetPath())
        {
            System.Type assetType = null;
            UnityEngine.Object asset = null;

            if (BindLabelSearchAttribute.assetTypes.TryGetValue(path, out assetType) == false)
            {
                asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset == null)
                {
                    continue;
                }
                assetType = asset.GetType();
                BindLabelSearchAttribute.assetTypes.Add(path, assetType);
            }
            if (type != assetType)
            {
                continue;
            }
            if (asset == null)
            {
                asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset == null)
                {
                    continue;
                }
            }
            if (asset.name.Equals(BindingAttribute.subAssetName[index]))
            {
                property.objectReferenceValue = asset;
                EditorUtility.SetDirty(asset);
                break;
            }
        }
    }
    void DrawCachedSingleProperty(Rect position, SerializedProperty property, GUIContent label, int index)
    {
        if (BindingAttribute.subLableName != null && index != -1)
        {
            if (BindingAttribute.subLableName.Length > index)
            {
                label.text = string.Format("{0}[{1}]", label.text, BindingAttribute.subLableName[index]);
            }
        }
        Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        if (property.propertyType == SerializedPropertyType.ObjectReference)
        {
            property.objectReferenceValue = EditorGUI.ObjectField(labelRect, label, property.objectReferenceValue,
           GetPropertyType(property), false);
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
    bool CheckLimit()
    {
        if (rootProperty.arraySize > BindingAttribute.Limit)
        {
            rootProperty.DeleteArrayElementAtIndex(rootProperty.arraySize - 1);
            return false;
        }
        else if (rootProperty.arraySize < BindingAttribute.Limit)
        {
            int count = BindingAttribute.Limit - rootProperty.arraySize;
            for (int i = 0; i < count; i++)
            {
                rootProperty.InsertArrayElementAtIndex(rootProperty.arraySize);
            }
        }
        return true;
    }

    int GetProperTyIndex(SerializedProperty property)
    {
        string path = property.propertyPath;
        int index = -1;
        if (path.EndsWith("]"))
        {
            int.TryParse(path.Substring(path.Length - 2).Replace("]", ""), out index);
            return index;
        }
        return -1;
    }
    System.Type GetPropertyType(SerializedProperty property)
    {
        return
            Assembly.Load("UnityEngine.dll")
                .GetType("UnityEngine." + property.type.Replace("PPtr<$", "").Replace(">", ""));
    }
    /// <summary>
    /// 取得所有资源的路径
    /// </summary>
    /// <returns>
    /// The all asset path.
    /// </returns>
    string[] GetAllAssetPath()
    {
        string[] allAssetPath = AssetDatabase.GetAllAssetPaths();

        System.Array.Sort(allAssetPath);

        if (BindingAttribute.direction.Equals(BindLabelSearchAttribute.Direction.DESC))
        {
            System.Array.Reverse(allAssetPath);
        }
        return allAssetPath;
    }
    void DrawHelpBox(Rect position, int index, string message)
    {
        if (index != rootProperty.arraySize - 1)
        {
            return;
        }
        BindingAttribute.error = true;
        position.x += 10;
        position.y += 20;
        position.width -= 10;
        position.height = CONTENT_HEIGHT;
        EditorGUI.HelpBox(position, string.Format(message, BindingAttribute.subAssetName[index]), MessageType.Error);
    }
}

#endif