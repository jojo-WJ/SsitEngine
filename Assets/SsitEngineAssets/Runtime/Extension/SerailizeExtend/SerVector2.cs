using System;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class SerVector2 : ISerializable
{
    public float x;
    public float y;

    //serialization constructor
    protected SerVector2( SerializationInfo info, StreamingContext context )
    {
        x = (float) info.GetValue("x", typeof(float));
        y = (float) info.GetValue("y", typeof(float));
    }

    public SerVector2( Vector2 vec3 )
    {
        x = vec3.x;
        y = vec3.y;
    }

    public SerVector2( float x, float y )
    {
        this.x = x;
        this.y = y;
    }

    public void GetObjectData( SerializationInfo info, StreamingContext ctxt )
    {
        info.AddValue("x", x);
        info.AddValue("y", y);
    }

    public bool isDifferentTo( SerVector2 other )
    {
        var changed = false;

        if (!changed && x != other.x)
            changed = true;

        if (!changed && y != other.y)
            changed = true;

        return changed;
    }

    public bool isDifferentToByOffset( SerVector2 other, float offset )
    {
        var changed = false;

        if (!changed && Mathf.Abs(x - other.x) >= offset)
            changed = true;

        if (!changed && Mathf.Abs(y - other.y) >= offset)
            changed = true;

        return changed;
    }

    public Vector2 getVector2()
    {
        return new Vector2(x, y);
    }

    public static implicit operator Vector2( SerVector2 i )
    {
        return i.getVector2();
    }

    public static implicit operator SerVector2( Vector2 i )
    {
        return new SerVector2(i);
    }
}