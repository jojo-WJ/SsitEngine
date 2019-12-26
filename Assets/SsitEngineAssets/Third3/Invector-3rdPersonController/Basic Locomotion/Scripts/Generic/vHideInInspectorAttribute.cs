using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public class vHideInInspectorAttribute : PropertyAttribute
{
    public bool invertValue;
    public string refbooleanProperty;

    public vHideInInspectorAttribute( string refbooleanProperty, bool invertValue = false )
    {
        this.refbooleanProperty = refbooleanProperty;
        this.invertValue = invertValue;
    }
}