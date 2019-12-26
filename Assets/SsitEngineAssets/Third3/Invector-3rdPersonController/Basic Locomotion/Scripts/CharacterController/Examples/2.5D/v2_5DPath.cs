using System;
using Framework;
using Invector;
using UnityEngine;

[vClassHeader("2.5D PathNode", false)]
public class v2_5DPath : vMonoBehaviour
{
    public bool autoUpdateCameraAngle = true;
    public v2_5DPathPoint currentPoint;
    public bool loopPath = true;
    public Transform[] points;
    public Transform reference;

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying && points.Length != transform.childCount)
            points = new Transform[transform.childCount];

        if (transform.childCount > 1)
        {
            var point = transform.GetChild(0);
            if (!Application.isPlaying)
                points[0] = transform.GetChild(0);
            for (var i = 1; i < transform.childCount; i++)
            {
                if (!Application.isPlaying)
                    points[i] = transform.GetChild(i);
                Gizmos.DrawLine(point.position, transform.GetChild(i).position);
                point = transform.GetChild(i);
            }
            if (loopPath) Gizmos.DrawLine(points[0].position, points[points.Length - 1].position);
        }

        if (currentPoint != null)
        {
            Gizmos.color = Color.red;
            if (currentPoint.center) Gizmos.DrawSphere(currentPoint.center.position, 0.2f);
            Gizmos.color = Color.yellow;
            if (currentPoint.forward) Gizmos.DrawSphere(currentPoint.forward.position, 0.2f);
            Gizmos.color = Color.yellow;
            if (currentPoint.backward) Gizmos.DrawSphere(currentPoint.backward.position, 0.2f);
        }
    }

    private v2_5DPathPoint GetStartPoint( Vector3 position )
    {
        var distance = Mathf.Infinity;
        var point = new v2_5DPathPoint();
        for (var i = 0; i < points.Length; i++)
        {
            var _distance = Vector3.Distance(points[i].position, position);
            if (_distance < distance)
            {
                distance = _distance;
                point.center = points[i];
                if (i + 1 < points.Length) point.forward = points[i + 1];
                else if (i == points.Length - 1 && loopPath) point.forward = points[0];
                if (i - 1 > -1) point.backward = points[i - 1];
                else if (i == 0 && loopPath) point.backward = points[points.Length - 1];
            }
        }
        return point;
    }

    public bool isNearForward( Vector3 position )
    {
        if (currentPoint == null || !currentPoint.forward) return false;
        return Vector3.Distance(currentPoint.forward.position, position) < 0.1f;
    }

    public bool isNearBackward( Vector3 position )
    {
        if (!currentPoint.backward) return false;
        return Vector3.Distance(currentPoint.backward.position, position) < 0.1f;
    }

    private v2_5DPathPoint GetNextPoint( Transform center )
    {
        var point = new v2_5DPathPoint();
        point.center = center;
        var pointIndex = Array.IndexOf(points, center);
        if (pointIndex + 1 < points.Length) point.forward = points[pointIndex + 1];
        else if (pointIndex == points.Length - 1 && loopPath) point.forward = points[0];
        if (pointIndex - 1 > -1) point.backward = points[pointIndex - 1];
        else if (pointIndex == 0 && loopPath) point.backward = points[points.Length - 1];
        return point;
    }

    public Vector3 ConstraintPosition( Vector3 pos, bool checkChangePoint = true )
    {
        var position = pos;
        if (currentPoint == null) currentPoint = GetStartPoint(pos);
        if (currentPoint.center)
        {
            if (!reference)
            {
                var obj = new GameObject("Reference");
                reference = obj.transform;
            }
            position.y = currentPoint.center.position.y;
            if (checkChangePoint)
            {
                if (isNearBackward(position)) currentPoint = GetNextPoint(currentPoint.backward);
                if (isNearForward(position)) currentPoint = GetNextPoint(currentPoint.forward);
            }

            if (currentPoint.forward != null)
            {
                var dirA = currentPoint.backward
                    ? currentPoint.backward.position - currentPoint.center.position
                    : -reference.right;
                var pA = currentPoint.center.position + dirA;
                var dirB = currentPoint.forward
                    ? currentPoint.forward.position - currentPoint.center.position
                    : reference.right;
                var pB = currentPoint.center.position + dirB;

                reference.position = currentPoint.center.position;
                var distanceA = currentPoint.backward
                    ? Vector3.Distance(currentPoint.center.position, currentPoint.backward.position)
                    : Mathf.Infinity;
                var distanceB = currentPoint.forward
                    ? Vector3.Distance(currentPoint.center.position, currentPoint.forward.position)
                    : Mathf.Infinity;
                if (Vector3.Distance(pA, position) > distanceA + 0.1f) reference.right = dirB;
                else if (Vector3.Distance(pB, position) > distanceB + 0.1f) reference.right = -dirA;
            }

            if (autoUpdateCameraAngle && CameraController.instance)
            {
                var rot = Quaternion.LookRotation(reference.forward, Vector3.up);
                var angle = rot.eulerAngles.NormalizeAngle().y;
                CameraController.instance.lerpState.fixedAngle.x = angle;
            }
            var localPosition = reference.InverseTransformPoint(pos);
            localPosition.z = 0;

            return reference.TransformPoint(localPosition);
        }
        return position;
    }

    public class v2_5DPathPoint
    {
        public Transform backward;
        public Transform center;
        public Transform forward;
    }
}