using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class AddDescribeAttribute : PropertyAttribute
{
    public Color textColor;
    public string toolTipName;

    public AddDescribeAttribute()
    {
        textColor = Color.white;
    }

    public AddDescribeAttribute( string toolTip )
    {
        toolTipName = toolTip;
        textColor = Color.white;
    }

    public AddDescribeAttribute( string toolTip, float r, float g, float b, float a )
    {
        toolTipName = toolTip;
        textColor = new Color(r, g, b, a);
    }

    public AddDescribeAttribute( string toolTip, float r, float g, float b )
    {
        toolTipName = toolTip;
        textColor = new Color(r, g, b, 1);
    }
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AddDescribeAttribute))]
public class AddDescribeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        var att = (AddDescribeAttribute) attribute;
        GUI.color = att.textColor;
        if (!string.IsNullOrEmpty(att.toolTipName))
        {
            label.text = string.Format("[{0}]:{1}", att.toolTipName, label.text);
        }
        switch (property.propertyType)
        {
            case SerializedPropertyType.Integer:
                property.intValue = EditorGUI.IntField(position, label, property.intValue);
                break;
            case SerializedPropertyType.Float:
                property.floatValue = EditorGUI.FloatField(position, label, property.intValue);
                break;
            case SerializedPropertyType.Boolean:
                property.boolValue = EditorGUI.Toggle(position, label, property.boolValue);
                break;
            case SerializedPropertyType.String:
                property.stringValue = EditorGUI.TextField(position, label, property.stringValue);
                break;
            case SerializedPropertyType.Vector2:
                property.vector2Value = EditorGUI.Vector2Field(position, label, property.vector2Value);
                break;
            case SerializedPropertyType.Vector3:
                property.vector3Value = EditorGUI.Vector3Field(position, label, property.vector3Value);
                break;
            case SerializedPropertyType.Vector4:
                property.vector4Value = EditorGUI.Vector4Field(position, label, property.vector4Value);
                break;
            case SerializedPropertyType.Color:
                property.colorValue = EditorGUI.ColorField(position, label, property.colorValue);
                break;
            case SerializedPropertyType.Rect:
                property.rectValue = EditorGUI.RectField(position, label, property.rectValue);
                break;
            case SerializedPropertyType.Bounds:
                property.boundsValue = EditorGUI.BoundsField(position, label, property.boundsValue);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        GUI.color = Color.white;
    }
}
#endif