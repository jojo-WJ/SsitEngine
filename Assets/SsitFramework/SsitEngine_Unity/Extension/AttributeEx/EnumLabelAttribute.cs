using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#endif 

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
public class EnumLabelAttribute : PropertyAttribute
{
    public string label;
    public EnumLabelAttribute(string label)
    {
        this.label = label;
    }
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EnumLabelAttribute))]
public class EnumLabelDrawer : PropertyDrawer
{
    private Dictionary<string, string> customEnumNames = new Dictionary<string, string>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SetUpCustomEnumNames(property, property.enumNames);

        if (property.propertyType == SerializedPropertyType.Enum)
        {
            EditorGUI.BeginChangeCheck();
            string[] displayedOptions = property.enumNames
                    .Where(enumName => customEnumNames.ContainsKey(enumName))
                    .Select<string, string>(enumName => customEnumNames[enumName])
                    .ToArray();

            int selectedIndex = EditorGUI.Popup(position, enumLabelAttribute.label, property.enumValueIndex, displayedOptions);
            if (EditorGUI.EndChangeCheck())
            {
                property.enumValueIndex = selectedIndex;
            }
        }
    }

    private EnumLabelAttribute enumLabelAttribute
    {
        get
        {
            return (EnumLabelAttribute)attribute;
        }
    }

    /// <summary>
    /// 设置序列化属性的名称
    /// </summary>
    /// <param name="property"></param>
    /// <param name="enumNames"></param>
    public void SetUpCustomEnumNames(SerializedProperty property, string[] enumNames)
    {
        //获取属性类对像的类型
        Type type = property.serializedObject.targetObject.GetType();
        //遍历类型中的字段
        foreach (FieldInfo fieldInfo in type.GetFields())
        {
            //获取所有字段enumLabeAttribute相关联（EnumLabelAttribute）的属性
            object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumLabelAttribute), false);
            foreach (EnumLabelAttribute customAttribute in customAttributes)
            {
                Type enumType = fieldInfo.FieldType;

                foreach (string enumName in enumNames)
                {
                    FieldInfo field = enumType.GetField(enumName);
                    if (field == null) continue;
                    EnumLabelAttribute[] attrs = (EnumLabelAttribute[])field.GetCustomAttributes(customAttribute.GetType(), false);

                    if (!customEnumNames.ContainsKey(enumName))
                    {
                        foreach (EnumLabelAttribute labelAttribute in attrs)
                        {
                            customEnumNames.Add(enumName, labelAttribute.label);
                        }
                    }
                }
            }
        }
    }
}

#endif