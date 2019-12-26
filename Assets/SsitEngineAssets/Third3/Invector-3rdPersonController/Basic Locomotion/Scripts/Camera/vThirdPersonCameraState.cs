using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class vThirdPersonCameraState
{
    public TPCameraMode cameraMode;
    public float cullingHeight;
    public float cullingMinDist;
    public float defaultDistance;
    public Vector2 fixedAngle;
    public float forward;
    public float fov;
    public float height;
    public List<LookPoint> lookPoints;
    public float maxDistance;
    public float minDistance;
    public string Name;
    public float right;

    public Vector3 rotationOffSet;
    public float smoothFollow;
    public bool useZoom;
    public float xMaxLimit;
    public float xMinLimit;
    public float xMouseSensitivity;
    public float yMaxLimit;
    public float yMinLimit;
    public float yMouseSensitivity;

    public vThirdPersonCameraState( string name )
    {
        Name = name;
        forward = -1f;
        right = 0f;
        defaultDistance = 1.5f;
        maxDistance = 3f;
        minDistance = 0.5f;
        height = 0f;
        smoothFollow = 10f;
        xMouseSensitivity = 3f;
        yMouseSensitivity = 3f;
        yMinLimit = -40f;
        yMaxLimit = 80f;
        xMinLimit = -360f;
        xMaxLimit = 360f;
        cullingHeight = 0.2f;
        cullingMinDist = 0.1f;
        useZoom = false;
        forward = 60;
        fixedAngle = Vector2.zero;
        cameraMode = TPCameraMode.FreeDirectional;
    }
}

[Serializable]
public class LookPoint
{
    public Vector3 eulerAngle;
    public bool freeRotation;
    public string pointName;
    public Vector3 positionPoint;
}

public enum TPCameraMode
{
    FreeDirectional,
    FixedAngle,
    FixedPoint
}