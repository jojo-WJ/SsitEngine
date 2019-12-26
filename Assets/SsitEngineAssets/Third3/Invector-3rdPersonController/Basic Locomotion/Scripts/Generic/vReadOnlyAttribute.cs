using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class vReadOnlyAttribute : PropertyAttribute
{
    public readonly bool justInPlayMode;

    public vReadOnlyAttribute( bool justInPlayMode = true )
    {
        this.justInPlayMode = justInPlayMode;
    }
}