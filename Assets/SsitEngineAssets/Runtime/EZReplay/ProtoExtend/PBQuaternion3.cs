using ProtoBuf;
using UnityEngine;

[ProtoContract]
public class PBQuaternion3
{
    [ProtoIgnore] public Quaternion quaternion;

    public PBQuaternion3()
    {
    }

    public PBQuaternion3( Quaternion source )
    {
        quaternion = source;
    }

    [ProtoMember(1, Name = "x")]
    public float x
    {
        get => quaternion.x;
        set => quaternion.x = value;
    }

    [ProtoMember(2, Name = "y")]
    public float y
    {
        get => quaternion.y;
        set => quaternion.y = value;
    }

    [ProtoMember(3, Name = "z")]
    public float z
    {
        get => quaternion.z;
        set => quaternion.z = value;
    }

    [ProtoMember(4, Name = "w")]
    public float w
    {
        get => quaternion.w;
        set => quaternion.w = value;
    }

    public static implicit operator Quaternion( PBQuaternion3 i )
    {
        return i.quaternion;
    }

    public static implicit operator PBQuaternion3( Quaternion i )
    {
        return new PBQuaternion3(i);
    }

    public override string ToString()
    {
        return quaternion.ToString();
    }

    public bool isDifferentTo( PBQuaternion3 i )
    {
        return x != i.x || y != i.y || z != i.z || w != i.w;
    }
}