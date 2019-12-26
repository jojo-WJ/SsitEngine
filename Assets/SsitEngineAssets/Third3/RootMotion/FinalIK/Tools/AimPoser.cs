using System;
using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// Aim Poser returns a reference by direction.
    /// </summary>
    public class AimPoser : MonoBehaviour
    {
        public float angleBuffer = 5f; // The angle buffer
        public Pose[] poses = new Pose[0]; // The array of poses.

        /// <summary>
        /// Gets the pose by direction. GetPose will go through the poses array and return the first pose that has the direction in range.
        /// </summary>
        public Pose GetPose( Vector3 localDirection )
        {
            if (poses.Length == 0) return null;

            for (var i = 0; i < poses.Length - 1; i++)
                if (poses[i].IsInDirection(localDirection))
                    return poses[i];
            return poses[poses.Length - 1];
        }

        /// <summary>
        /// Sets the pose active, increasing it's angle buffer.
        /// </summary>
        public void SetPoseActive( Pose pose )
        {
            for (var i = 0; i < poses.Length; i++) poses[i].SetAngleBuffer(poses[i] == pose ? angleBuffer : 0f);
        }

        /// <summary>
        /// the pose definition
        /// </summary>
        [Serializable]
        public class Pose
        {
            private float angleBuffer;
            public Vector3 direction; // the direction of the pose
            public string name; // the reference
            public float pitch = 45f; // the pitch range

            public bool visualize = true; // Show the direction and range of this pose in the scene view
            public float yaw = 75f; // the yaw range

            // Determines whether this Pose is in the specified direction.
            public bool IsInDirection( Vector3 d )
            {
                if (direction == Vector3.zero) return false;
                if (yaw <= 0 || pitch <= 0) return false;

                // Yaw
                if (yaw < 180f)
                {
                    var directionYaw = new Vector3(direction.x, 0f, direction.z);
                    if (directionYaw == Vector3.zero) directionYaw = Vector3.forward;

                    var dYaw = new Vector3(d.x, 0f, d.z);
                    var yawAngle = Vector3.Angle(dYaw, directionYaw);

                    if (yawAngle > yaw + angleBuffer) return false;
                }

                // Pitch
                if (pitch >= 180f) return true;

                var directionPitch = Vector3.Angle(Vector3.up, direction);
                var dPitch = Vector3.Angle(Vector3.up, d);
                return Mathf.Abs(dPitch - directionPitch) < pitch + angleBuffer;
            }

            // Sets the angle buffer to prevent immediatelly switching back to the last pose if the angle should change a bit.
            public void SetAngleBuffer( float value )
            {
                angleBuffer = value;
            }
        }
    }
}