#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ReadOnlyAttribute : PropertyAttribute
{
    public string toolTipName;
    public Color textColor;
    public ReadOnlyAttribute()
    {
        textColor = Color.white;
    }
    public ReadOnlyAttribute(string toolTip)
    {
        this.toolTipName = toolTip;
        textColor = Color.white;
    }
    public ReadOnlyAttribute(string toolTip, float r, float g, float b, float a)
    {
        this.toolTipName = toolTip;
        textColor = new Color(r, g, b, a);
    }
    public ReadOnlyAttribute(string toolTip, float r, float g, float b)
    {
        this.toolTipName = toolTip;
        textColor = new Color(r, g, b, 1);
    }
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginDisabledGroup(true);
//        GUI.enabled = false;
        ReadOnlyAttribute att = (ReadOnlyAttribute)attribute;
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
