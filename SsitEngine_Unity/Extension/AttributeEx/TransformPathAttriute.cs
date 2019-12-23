#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using Framework.Editor;
using UnityEngine;

#endif

/// <summary>
/// 根据子物体名递归查找子物体（若有重名，默认返回第一个）
/// </summary>
public class AddBindPathAttribute : PropertyAttribute
{
    public string ComponentName;
    public string Path;

    public AddBindPathAttribute( string path )
    {
        Path = path;
        var temp = path.LastIndexOf("/", StringComparison.Ordinal);
        if (temp != -1)
        {
            ComponentName = path.Substring(temp + 1);
        }
        else
        {
            ComponentName = path;
        }
    }

    public AddBindPathAttribute()
    {
        Path = string.Empty;
    }

    public AddBindPathAttribute( string path, string component )
    {
        Path = path;
        ComponentName = component;
    }
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AddBindPathAttribute))]
public class AddBindPathDrawer : PropertyDrawer
{
    private static readonly string CONFIGPATH = "Assets/SsitEngineAssets/Editor/Config/";
    private static BindSetting m_bindSetting;
    private float mHeight;

    private AddBindPathAttribute addBindPathAttribute => (AddBindPathAttribute) attribute;

    public static BindSetting GetBindSetting()
    {
        if (m_bindSetting == null)
        {
            m_bindSetting = AssetDatabase.LoadAssetAtPath<BindSetting>($"{CONFIGPATH}BindSetting.asset");
        }
        return m_bindSetting;
    }

    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        return EditorGUI.GetPropertyHeight(property, label, true) + mHeight;
    }

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        var height = EditorGUI.GetPropertyHeight(property, label, true);
        var labelRect = new Rect(position.x, position.y, position.width, height);
        mHeight = 0;
        if (property.propertyType == SerializedPropertyType.ObjectReference)
        {
            if (property.objectReferenceValue == null ||
                property.objectReferenceValue.name != addBindPathAttribute.ComponentName)
            {
                var type = GetPropertyType(property);

                var error = "绑定类型不支持";
                if (type != null)
                {
                    //获取节点,并且获取组件
                    var cc = FindObjectReferenceValue(property, type, ref error);
                    if (cc)
                    {
                        property.objectReferenceValue = cc;

                        property.serializedObject.ApplyModifiedProperties();
                        //Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                        property.objectReferenceValue = EditorGUI.ObjectField(labelRect, label,
                            property.objectReferenceValue, type, false);
                        return;
                    }
                }
                //Editor.Repaint();
                EditorGUI.PropertyField(labelRect, property, label);
                HelpBox(position, height, error);
                return;
            }
        }
        EditorGUI.PropertyField(labelRect, property, label);
    }

    private void HelpBox( Rect position, float preHeight, string error )
    {
        mHeight = EditorGUIUtility.singleLineHeight * 1.5f;
        var boxRect = new Rect(position.x, position.y + preHeight, position.width, mHeight);
        EditorGUI.HelpBox(boxRect, error, MessageType.Error);
        Debug.LogError($"AddBindPath Attribute is exception{error}");
    }

    private Type GetPropertyType( SerializedProperty property )
    {
        //string typeName = property.type.Replace( "PPtr<$" , "" ).Replace( ">" , "" );


        //test:通过序列化对象的类型字段去反射获取字段类型fullName
        var propertyScriptType = property.serializedObject.targetObject.GetType();

        var field = propertyScriptType.GetField(property.name,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (field == null || field.ReflectedType == null)
        {
            return null;
        }
        var typeFullName = field.FieldType.FullName;

        if (string.IsNullOrEmpty(typeFullName))
        {
            return null;
        }

        var refrenceSetting = GetBindSetting();

        //step01: sereach form refrence library 
        Type t;
        if (refrenceSetting != null)
        {
            for (var i = 0; i < refrenceSetting.m_builtInRefrence.Count; i++)
            {
                var ss = refrenceSetting.m_builtInRefrence[i];
                t = Assembly.Load(ss).GetType(typeFullName);
                if (t != null)
                {
                    return t;
                }
            }
        }

        //step02: sereach form AssemblyCSharp 
        {
            var assembly = ReflectionExtension.GetAssemblyCSharp();
            t = assembly.GetType(typeFullName);
        }
        return t;
    }

    public Transform FindDeepChild( Transform transform, string name )
    {
        if (transform.name == name)
        {
            return transform;
        }

        var ret = transform.Find(name);
        if (ret)
        {
            return ret;
        }

        foreach (Transform child in transform)
        {
            var result = FindDeepChild(child, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    private Component FindObjectReferenceValue( SerializedProperty property, Type type, ref string error )
    {
        var component = property.serializedObject.targetObject as Component;

        if (component == null)
        {
            throw new InvalidCastException("Couldn't cast targetObject");
        }

        var bindchild = FindDeepChild(component.transform, addBindPathAttribute.Path);
        if (bindchild == null)
        {
            error = $"自动设置节点失败：{type.FullName} - {addBindPathAttribute.Path}";
            Debug.Log(error);
            return null;
        }
        var comp = bindchild.GetComponent(type);
        if (comp == null)
        {
            error = $"节点没有对应组件：{type.FullName} - {addBindPathAttribute.Path}";

            Debug.Log(error);
            return null;
        }

        return comp;
    }
}

#endif