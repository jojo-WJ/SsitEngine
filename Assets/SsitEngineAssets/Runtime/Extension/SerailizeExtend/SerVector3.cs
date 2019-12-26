using System;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class SerVector3 : ISerializable
{
    public float x;
    public float y;
    public float z;

    //serialization constructor
    protected SerVector3( SerializationInfo info, StreamingContext context )
    {
        x = (float) info.GetValue("x", typeof(float));
        y = (float) info.GetValue("y", typeof(float));
        z = (float) info.GetValue("z", typeof(float));
    }

    public SerVector3( Vector3 vec3 )
    {
        x = vec3.x;
        y = vec3.y;
        z = vec3.z;
    }

    public SerVector3( float x, float y, float z )
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    /*[SecurityPermissionAttribute(
                SecurityAction.Demand,
                SerializationFormatter = true)]		*/
    public void GetObjectData( SerializationInfo info, StreamingContext ctxt )
    {
        info.AddValue("x", x);
        info.AddValue("y", y);
        info.AddValue("z", z);
    }

    public bool isDifferentTo( SerVector3 other )
    {
        var changed = false;

        if (!changed && x != other.x)
            changed = true;

        if (!changed && y != other.y)
            changed = true;

        if (!changed && z != other.z)
            changed = true;

        return changed;
    }

    public bool isDifferentToByOffset( SerVector3 other, float offset )
    {
        var changed = false;

        if (!changed && Mathf.Abs(x - other.x) >= offset)
            changed = true;

        if (!changed && Mathf.Abs(y - other.y) >= offset)
            changed = true;

        if (!changed && Mathf.Abs(z - other.z) >= offset)
            changed = true;

        return changed;
    }

    public Vector3 getVector3()
    {
        return new Vector3(x, y, z);
    }

    public static implicit operator Vector3( SerVector3 i )
    {
        return i.getVector3();
    }

    public static implicit operator SerVector3( Vector3 i )
    {
        return new SerVector3(i);
    }
}