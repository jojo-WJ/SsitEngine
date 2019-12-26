using UnityEngine;

namespace SSIT.proto
{
    public partial class PBVector3
    {
        public PBVector3( Vector3 source )
        {
            x = source.x;
            y = source.y;
            z = source.z;
        }

        public static implicit operator Vector3( PBVector3 i )
        {
            return new Vector3(i.x, i.y, i.z);
        }

        public static implicit operator SerVector3( PBVector3 i )
        {
            return new SerVector3(i.x, i.y, i.z);
        }

        public static implicit operator PBVector3( Vector3 i )
        {
            return new PBVector3(i);
        }
    }

    public partial class PBQuaternion
    {
        public PBQuaternion( Quaternion source )
        {
            x = source.x;
            y = source.y;
            z = source.z;
            w = source.w;
        }

        public static implicit operator Quaternion( PBQuaternion i )
        {
            return new Quaternion(i.x, i.y, i.z, i.w);
        }

        public static implicit operator SerQuaternion( PBQuaternion i )
        {
            return new SerQuaternion(i.x, i.y, i.z, i.w);
        }

        public static implicit operator PBQuaternion( Quaternion i )
        {
            return new PBQuaternion(i);
        }
    }
}