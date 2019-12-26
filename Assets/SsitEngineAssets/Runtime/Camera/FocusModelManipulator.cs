using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Framework;
using SsitEngine.PureMVC.Patterns;
using UnityEngine;
using UnityEngine.Events;

public class FocusModelManipulator : MonoBehaviour, IInputState
{
    private Tweener ani;

    public ENCameraModeType CameraMode => ENCameraModeType.FocusModelManipulator;

    public void OnUpdate()
    {
    }

    public void OnLateUpdate()
    {
    }

    public void OnFixedUpdate()
    {
    }

    /// <summary>
    /// 定位模型操作器
    /// </summary>
    /// <param name="target"></param>
    /// <param name="callback"></param>
    /// <param name="duration"></param>
    public void FocusModel( GameObject target, UnityAction callback, float duration = 2f )
    {
        if (null != target && null != Camera.main)
        {
            Stop(true);

            var focusOpInfo = CameraFocus(Camera.main, target);

            if (Vector3.Distance(focusOpInfo, Camera.main.transform.position) < 0.5f)
            {
                if (null != callback) 
                    callback.Invoke();
                Facade.Instance.SendNotification((ushort) EnGlobalEvent.ChangeInputMode, EnInputMode.Free);
                return;
            }
            ani = Camera.main.transform.DOMove(focusOpInfo, duration);

            ani.OnComplete(() =>
            {
                ani = null;

                if (null != callback) 
                    callback.Invoke();
                Facade.Instance.SendNotification((ushort) EnGlobalEvent.ChangeInputMode, EnInputMode.Free);
            });
        }

        //if ( null != target && null != Camera.main )
        //{
        //    StopCoroutine( "focusModelCoroutine" );

        //    StartCoroutine( "focusModelCoroutine", new FocusModelCoroutineParameter( Camera.main.transform.position, target, callback ) );
        //}
    }

    private void Stop( bool complete )
    {
        if (null != ani && ani.IsPlaying()) ani.Kill(true);
    }

    #region focus model coroutine

    private class FocusModelCoroutineParameter
    {
        public readonly UnityAction callback;
        public readonly Vector3 startPosition;
        public readonly GameObject target;

        public FocusModelCoroutineParameter( Vector3 startPosition, GameObject target, UnityAction callback )
        {
            this.startPosition = startPosition;
            this.target = target;
            this.callback = callback;
        }
    }

    private IEnumerator focusModelCoroutine( FocusModelCoroutineParameter parameter )
    {
        if (null == Camera.main) yield break;

        // Store needed data
        var focusOpInfo = cameraFocusInfo(Camera.main, parameter.target);

        var cameraDestinationPoint = focusOpInfo.position;
        var cameraTransform = Camera.main.transform;

        // If the distance to travel is small enough, we can exit
        //if ( ( cameraDestinationPoint - cameraTransform.position ).magnitude < 1e-4f ) yield break;

        // We will need this to modify the position using 'Vector3.SmoothDamp'
        var velocity = Vector3.zero;

        while (true)
        {
            // Calculate the new position
            cameraTransform.position =
                Vector3.SmoothDamp(parameter.startPosition, cameraDestinationPoint, ref velocity, 0.9f);

            // If the position is close enough to the target position and the camera ortho size
            // is close enough to the target size, we can exit the loop.
            if ((cameraTransform.position - cameraDestinationPoint).magnitude < 1e-1f)
            {
                // Clamp to make sure we got the correct values and then exit the loop
                cameraTransform.position = cameraDestinationPoint;

                break;
            }

            yield return null;
        }

        if (null != parameter.callback) parameter.callback.Invoke();
    }

    #endregion

    #region focus info

    private struct CameraFocusInfo
    {
        public Vector3 position;
        public Vector3 target;
    }

    private Bounds ComputeBoundingBox( GameObject go, ref float v_distance )
    {
        var parent = go.transform;
        List<Vector3> targetPositons = new List<Vector3>();
        var renders = parent.GetComponentsInChildren<Renderer>();
        if (renders.Length > 1)
        {
            foreach (var render in renders)
            {
                targetPositons.Add(render.bounds.center);
            }
        }
        else
        {
            targetPositons.Add(parent.position);
        }

        var targetCenter = new Vector3(
            (targetPositons.Max(x => x.x) + targetPositons.Min(x => x.x)) / 2,
            (targetPositons.Max(x => x.y) + targetPositons.Min(x => x.y)) / 2,
            (targetPositons.Max(x => x.z) + targetPositons.Min(x => x.z)) / 2
        );

        /*if (2 <= targetPositons.Count)
        {
            // 相机->坐标中心
            var kizyunsen = new Vector3(0, 0, v_distance);

            // 相机->各对象的中心
            var hyoukasen = new List<Vector3>();
            foreach (var tp in targetPositons)
            {
                var line = kizyunsen + (tp - targetCenter);
                // Debug.Log(string.Format("kizyun={0} line={1} angle={2}", kizyunsen, line, Vector3.Angle(kizyunsen, line)));
                hyoukasen.Add(line);
            }

            // 获得基准线和评估线的角度
            var angles = hyoukasen.Select(x => Vector3.Angle(kizyunsen, x)).ToList();

            // 在虚拟坐标上确定距离
            v_distance = v_distance / camera.fieldOfView * (angles.Max() * 2.0f);
        }*/

        var bounds = new Bounds(targetCenter, Vector3.zero);
        foreach (var render in renders)
        {
            bounds.Encapsulate(render.bounds);
        }

        var vLimited_distance = bounds.size.x;
        if (vLimited_distance < bounds.size.y)
            vLimited_distance = bounds.size.y;
        if (vLimited_distance < bounds.size.z)
            vLimited_distance = bounds.size.z;
        //Debug.LogError("v_distance " + v_distance +"vLimited_distance"+ vLimited_distance);

        v_distance = Mathf.Max(v_distance, vLimited_distance);

        return bounds;
    }

    private CameraFocusInfo cameraFocusInfo( Camera camera, GameObject go, float distance = 5f )
    {
        if (null == camera || null == go)
            return default;

        var selectionWorldAABB = ComputeBoundingBox(go, ref distance);

        // We will establish the camera destination position by moving the camera along the reverse of its look vector
        // starting from the center of the world AABB by a distance equal to the maximum AABB size component.
        var maxAABBComponent = selectionWorldAABB.size.x;
        if (maxAABBComponent < selectionWorldAABB.size.y)
            maxAABBComponent = selectionWorldAABB.size.y;
        if (maxAABBComponent < selectionWorldAABB.size.z)
            maxAABBComponent = selectionWorldAABB.size.z;

        // Construct the focus operation info and return it to the caller
        var cameraFocusInfo = new CameraFocusInfo();
        cameraFocusInfo.position = selectionWorldAABB.center - camera.transform.forward * maxAABBComponent * 1.65f;
        cameraFocusInfo.target = selectionWorldAABB.center;

        return cameraFocusInfo;
    }

    private Vector3 CameraFocus( Camera camera, GameObject targets, float minDistance = 5.0f )
    {
        Bounds bounds = ComputeBoundingBox(targets, ref minDistance);
        var targetCenter = bounds.center;

        //v_distance = Mathf.Clamp(v_distance, minDistance, float.MaxValue);
        // Debug.Log(string.Format("v_distance={0}", v_distance));

        Transform cameTran = camera.transform;
        Vector3 relative = cameTran.TransformPoint(new Vector3(0, 0, minDistance));
        Vector3 tarPos = targetCenter - (relative - cameTran.position);
        // Vector3 dir = tarPos - targetCenter;
        // Debug.Log(string.Format("{0} {1}", camera.transform.position, relative));

        return tarPos;
    }

    public void FocusMultiObject( Camera camera, List<GameObject> targets, float distance = 5.0f )
    {
        // 物体が１つの場合は対象の点を１つにする
        List<Vector3> targetPositons = new List<Vector3>();
        if (1 == targets.Count)
            targetPositons.Add(targets[0].transform.position);
        else
        {
            foreach (var t in new Vector3[]
                {Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back})
            {
                targetPositons.AddRange(targets.Select(x => x.transform.position + t));
            }
        }

        // すべての物体の中心位置を取得
        var targetCenter = new Vector3(
            (targetPositons.Max(x => x.x) + targetPositons.Min(x => x.x)) / 2,
            (targetPositons.Max(x => x.y) + targetPositons.Min(x => x.y)) / 2,
            (targetPositons.Max(x => x.z) + targetPositons.Min(x => x.z)) / 2
        );
        // Debug.Log(string.Format("targetCenter={0}", targetCenter));

        // 计算离物体中心的距离
        var v_distance = distance;
        // Debug.Log(string.Format("v_distance={0}", v_distance));
        if (2 <= targetPositons.Count)
        {
            // 相机->坐标中心
            var kizyunsen = new Vector3(0, 0, v_distance);

            // 相机->各对象的中心
            var hyoukasen = new List<Vector3>();
            foreach (var tp in targetPositons)
            {
                var line = kizyunsen + (tp - targetCenter);
                // Debug.Log(string.Format("kizyun={0} line={1} angle={2}", kizyunsen, line, Vector3.Angle(kizyunsen, line)));
                hyoukasen.Add(line);
            }

            // 基準線と評価線の角度を取得
            var angles = hyoukasen.Select(x => Vector3.Angle(kizyunsen, x)).ToList();

            // 仮想座標上で距離を決定
            v_distance = v_distance / camera.fieldOfView * (angles.Max() * 2.0f);
            // 仮想座標上で距離を決定（補正してみる）
            // var cen_distance = (targetCenter - camera.transform.position).magnitude;
            // var min_distance = targetPositons.Select(x => (x - camera.transform.position).magnitude).Min();
            // v_distance = v_distance + (cen_distance - min_distance);
            // Debug.Log(string.Format("v_distance={0}", v_distance));
        }
        Transform cameTran = camera.transform;
        // カメラの移動先を設定
        var relative = cameTran.TransformPoint(new Vector3(0, 0, v_distance));
        // Debug.Log(string.Format("{0} {1}", camera.transform.position, relative));
        var cameraFocusInfo = new CameraFocusInfo();

        cameraFocusInfo.position = targetCenter - (relative - cameTran.position);
        cameraFocusInfo.target = targetCenter;
    }

    #endregion

    #region IState

    public bool Enable()
    {
        return enabled;
    }

    public bool CouldEnter()
    {
        return true;
    } // 不能切换到该操作器，连续定位模型，会导致系统操作器状态错乱

    public bool CouldLeave()
    {
        return true;
    } // 不能切换到该操作器，连续定位模型，会导致系统操作器状态错乱

    public void Enter()
    {
        enabled = true;
    }

    public void Leave()
    {
        enabled = false;
        Stop(enabled);
    }

    #endregion
}