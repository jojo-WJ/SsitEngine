using System;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class SerQuaternion : ISerializable
{
    public float w;

    public float x;
    public float y;
    public float z;

    //serialization constructor
    protected SerQuaternion( SerializationInfo info, StreamingContext context )
    {
        x = (float) info.GetValue("x", typeof(float));
        y = (float) info.GetValue("y", typeof(float));
        z = (float) info.GetValue("z", typeof(float));
        w = (float) info.GetValue("w", typeof(float));
    }

    public SerQuaternion( Quaternion quat )
    {
        x = quat.x;
        y = quat.y;
        z = quat.z;
        w = quat.w;
    }

    public SerQuaternion( float x, float y, float z, float w )
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    /*[SecurityPermissionAttribute(
                SecurityAction.Demand,
                SerializationFormatter = true)]		*/
    public void GetObjectData( SerializationInfo info, StreamingContext ctxt )
    {
        info.AddValue("x", x);
        info.AddValue("y", y);
        info.AddValue("z", z);
        info.AddValue("w", w);
    }

    public bool isDifferentTo( SerQuaternion other )
    {
        var changed = false;

        if (!changed && x != other.x)
            changed = true;

        if (!changed && y != other.y)
            changed = true;

        if (!changed && z != other.z)
            changed = true;

        if (!changed && w != other.w)
            changed = true;


        return changed;
    }

    public bool isDifferentToByOffset( SerQuaternion other, float offset )
    {
        var changed = false;

        if (!changed && Mathf.Abs(x - other.x) >= offset)
            changed = true;

        if (!changed && Mathf.Abs(y - other.y) >= offset)
            changed = true;

        if (!changed && Mathf.Abs(z - other.z) >= offset)
            changed = true;

        if (!changed && Mathf.Abs(w - other.w) >= offset)
            changed = true;
        return changed;
    }

    public Quaternion getQuaternion()
    {
        return new Quaternion(x, y, z, w);
    }

    public static implicit operator Quaternion( SerQuaternion i )
    {
        return i.getQuaternion();
    }

    public static implicit operator SerQuaternion( Quaternion i )
    {
        return new SerQuaternion(i);
    }
}