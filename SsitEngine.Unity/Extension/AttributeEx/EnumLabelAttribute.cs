using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field)]
public class EnumLabelAttribute : PropertyAttribute
{
    public string label;

    public EnumLabelAttribute( string label )
    {
        this.label = label;
    }
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EnumLabelAttribute))]
public class EnumLabelDrawer : PropertyDrawer
{
    private readonly Dictionary<string, string> customEnumNames = new Dictionary<string, string>();

    private EnumLabelAttribute enumLabelAttribute => (EnumLabelAttribute) attribute;

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        SetUpCustomEnumNames(property, property.enumNames);

        if (property.propertyType == SerializedPropertyType.Enum)
        {
            EditorGUI.BeginChangeCheck();
            var displayedOptions = property.enumNames
                .Where(enumName => customEnumNames.ContainsKey(enumName))
                .Select(enumName => customEnumNames[enumName])
                .ToArray();

            var selectedIndex = EditorGUI.Popup(position, enumLabelAttribute.label, property.enumValueIndex,
                displayedOptions);
            if (EditorGUI.EndChangeCheck())
            {
                property.enumValueIndex = selectedIndex;
            }
        }
    }

    /// <summary>
    /// 设置序列化属性的名称
    /// </summary>
    /// <param name="property"></param>
    /// <param name="enumNames"></param>
    public void SetUpCustomEnumNames( SerializedProperty property, string[] enumNames )
    {
        //获取属性类对像的类型
        var type = property.serializedObject.targetObject.GetType();
        //遍历类型中的字段
        foreach (var fieldInfo in type.GetFields())
        {
            //获取所有字段enumLabeAttribute相关联（EnumLabelAttribute）的属性
            var customAttributes = fieldInfo.GetCustomAttributes(typeof(EnumLabelAttribute), false);
            foreach (EnumLabelAttribute customAttribute in customAttributes)
            {
                var enumType = fieldInfo.FieldType;

                foreach (var enumName in enumNames)
                {
                    var field = enumType.GetField(enumName);
                    if (field == null)
                    {
                        continue;
                    }
                    var attrs = (EnumLabelAttribute[]) field.GetCustomAttributes(customAttribute.GetType(), false);

                    if (!customEnumNames.ContainsKey(enumName))
                    {
                        foreach (var labelAttribute in attrs)
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