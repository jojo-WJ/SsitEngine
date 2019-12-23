using UnityEditor;
using UnityEngine;

namespace JxDebug.test
{
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI( Rect position, SerializedProperty property, GUIContent content )
        {
            var attribute = (EnumFlagsAttribute) this.attribute;
            property.intValue = EditorGUI.MaskField(position, content, property.intValue,
                attribute.displayOptions ?? property.enumDisplayNames);
        }
    }
}