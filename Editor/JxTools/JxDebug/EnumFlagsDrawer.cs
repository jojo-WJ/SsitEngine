using UnityEditor;
using UnityEngine;

namespace JxDebug.test
{
    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI( Rect position, SerializedProperty property, GUIContent content )
        {
            EnumFlagsAttribute attribute = (EnumFlagsAttribute)base.attribute;
            property.intValue = EditorGUI.MaskField(position, content, property.intValue, attribute.displayOptions ?? property.enumDisplayNames);
        }
    }

}
