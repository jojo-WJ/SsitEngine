using UnityEngine;
using System.Collections;
using UnityEditor;
[CustomPropertyDrawer(typeof(vReadOnlyAttribute))]
public class vReadOnlyAttributeDrawer : PropertyDrawer
{

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
       
        var att = attribute as vReadOnlyAttribute;
        if (att.justInPlayMode && !Application.isPlaying) return;
        string value =string.Empty;

        switch (property.propertyType)
        {
            case SerializedPropertyType.Integer:
                value = property.intValue.ToString();
                break;
            case SerializedPropertyType.Boolean:
                value = property.boolValue.ToString();
                break;
            case SerializedPropertyType.Float:
                value = property.floatValue.ToString("0.00000");
                break;
            case SerializedPropertyType.String:
                value = property.stringValue;
                break;
            case SerializedPropertyType.ObjectReference:
                value = property.objectReferenceValue ? property.objectReferenceValue.name : "Null";
                break;
            case SerializedPropertyType.Quaternion:
                value = property.quaternionValue.eulerAngles.ToString();
                break;
            case SerializedPropertyType.Vector2:
                value = property.vector2Value.ToString();
                break;
            case SerializedPropertyType.Vector3:
                value = property.vector3Value.ToString();
                break;
            case SerializedPropertyType.Enum:
                value = property.enumDisplayNames[property.enumValueIndex];
                break;
            default:
                value = "(not supported)";
                break;
        }
              
        var fontStyle = GUI.skin.label.fontStyle;
        GUI.skin.label.fontStyle = FontStyle.BoldAndItalic;
        GUIStyle style =new GUIStyle( EditorStyles.miniBoldLabel);
        style.fontStyle = FontStyle.BoldAndItalic;
      
        style.normal.textColor = Color.yellow;
        style.alignment = TextAnchor.MiddleLeft;
        var rect = position;
        rect.width = position.width*0.6f;
        EditorGUI.LabelField(rect, "", label.text, style);
        style.normal.textColor = Color.red;
        style.alignment = TextAnchor.MiddleLeft;
        position.x += rect.width + 0.05f;
        position.width = position.width*0.35f;
        EditorGUI.LabelField(position,"",value, style);
        GUI.skin.label.fontStyle = fontStyle;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var att = attribute as vReadOnlyAttribute;
        if (att.justInPlayMode && !Application.isPlaying) return 0;
        return base.GetPropertyHeight(property, label);
    }
}
