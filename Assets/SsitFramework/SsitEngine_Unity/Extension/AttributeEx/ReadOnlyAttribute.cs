using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ReadOnlyAttribute : PropertyAttribute
{
    public Color textColor;
    public string toolTipName;

    public ReadOnlyAttribute()
    {
        textColor = Color.white;
    }

    public ReadOnlyAttribute( string toolTip )
    {
        toolTipName = toolTip;
        textColor = Color.white;
    }

    public ReadOnlyAttribute( string toolTip, float r, float g, float b, float a )
    {
        toolTipName = toolTip;
        textColor = new Color(r, g, b, a);
    }

    public ReadOnlyAttribute( string toolTip, float r, float g, float b )
    {
        toolTipName = toolTip;
        textColor = new Color(r, g, b, 1);
    }
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight( SerializedProperty property,
        GUIContent label )
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        EditorGUI.BeginDisabledGroup(true);
//        GUI.enabled = false;
        var att = (ReadOnlyAttribute) attribute;
        GUI.color = att.textColor;
        if (!string.IsNullOrEmpty(att.toolTipName))
        {
            label.text = string.Format("[{0}]:{1}", att.toolTipName, label.text);
        }
        EditorGUI.PropertyField(position, property, label, true);
        GUI.color = Color.white;
        EditorGUI.EndDisabledGroup();
//        GUI.enabled = true;
    }
}
#endif