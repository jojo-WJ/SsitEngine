using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
[CustomPropertyDrawer(typeof(vHideInInspectorAttribute),true)]
public class vHideInInspectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect _Position, SerializedProperty _Property, GUIContent _Label)
    {
        vHideInInspectorAttribute _attribute = attribute as vHideInInspectorAttribute;
        if (_attribute != null && _Property.serializedObject.targetObject)
        {
           
            var propertyName = _Property.propertyPath.Replace(_Property.name, "");
            var booleamProperty = _Property.serializedObject.FindProperty(propertyName+_attribute.refbooleanProperty);                     
            if (booleamProperty != null )
            {
               
                var valid =(bool) _attribute.invertValue ? !booleamProperty.boolValue :booleamProperty.boolValue;
                if (valid)
                {
                    EditorGUI.PropertyField(_Position, _Property, true);
                   
                }               
            }
            else
            {
                EditorGUI.PropertyField(_Position, _Property, true);
            }
        }
        else
            EditorGUI.PropertyField(_Position, _Property, true);
    }
       
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        vHideInInspectorAttribute _attribute = attribute as vHideInInspectorAttribute;
        if (_attribute != null)
        {
            var propertyName = property.propertyPath.Replace(property.name, "");
            var booleamProperty = property.serializedObject.FindProperty(propertyName + _attribute.refbooleanProperty);
            if (booleamProperty != null)
            {
                var valid = _attribute.invertValue ? !booleamProperty.boolValue : booleamProperty.boolValue;
                if (valid) return base.GetPropertyHeight(property, label);
                else return 0;
            }
            else
                return base.GetPropertyHeight(property, label);
        }
        return base.GetPropertyHeight(property, label);
    }
}