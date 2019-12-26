using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class PBVector3
{
    [ProtoIgnore] public Vector3 vector3;

    public PBVector3()
    {
    }

    public PBVector3( Vector3 source )
    {
        vector3 = source;
    }

    [ProtoMember(1, Name = "x")]
    public float x
    {
        get => vector3.x;
        set => vector3.x = value;
    }

    [ProtoMember(2, Name = "y")]
    public float y
    {
        get => vector3.y;
        set => vector3.y = value;
    }

    [ProtoMember(3, Name = "z")]
    public float z
    {
        get => vector3.z;
        set => vector3.z = value;
    }

    public static implicit operator Vector3( PBVector3 i )
    {
        return i.vector3;
    }

    public static implicit operator PBVector3( Vector3 i )
    {
        return new PBVector3(i);
    }

    public override string ToString()
    {
        return vector3.ToString();
    }

    public bool isDifferentTo( PBVector3 i )
    {
        return x != i.x || y != i.y || z != i.z;
    }
}