using Framework;
using Framework.SceneObject;
using Framework.SsitInput;
using Framework.Utility;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.Unity;
using SsitEngine.Unity.Msic;
using SsitEngine.Unity.SsitInput;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ENMoveMode
{
    EN_None,
    EN_MiniMap,
    EN_Map
}

public class CrossPlatformManipulator : SsitMonoBase, IInputState
{
    private ENMoveMode mMoveMode = ENMoveMode.EN_None;
    private GameObject mainCameraObj;

    private Transform mainCameraTrans;
    // Use this for initialization

    /// <summary>
    /// 上次触摸点1(手指1)
    /// </summary>
    private Touch oldTouch1;

    /// <summary>
    /// 上次触摸点2(手指2)
    /// </summary>
    private Touch oldTouch2;

    /// <summary>
    /// 用于显示滑动距离
    /// </summary>
    private float oldDis;

    private float newDis;
    private float scaler = 0;


    private Vector3 lookPosition;
    private Vector3 lookDirection;

    private Vector3 move;
    private Vector3 rotate;
    private Vector3 forward;
    private Vector3 right;

    public bool update_one_axis_one_time = true;

    private bool m_bMouseOverUI;
    private LayerMask m_DetectedLayerMask;

    public GameObject Virtulobj { get; set; }

    public ENMoveMode MoveMode
    {
        get => mMoveMode;

        set
        {
            if (mMoveMode == ENMoveMode.EN_Map)
                mMoveMode = ENMoveMode.EN_None;
            else
                mMoveMode = value;
        }
    }

    public Vector3 FocusCenter { get; set; }

    public ENCameraModeType CameraMode => ENCameraModeType.CrossPlatformManipulator;

    public void OnUpdate()
    {
        if (Engine.Instance.PlatConfig)
        {
            return;
        }
        if (Utilitys.IsMousePlatform())
        {
            for (var i = 0; i < 2; ++i)
                if (Input.GetMouseButtonDown(i))
                    if (EventSystem.current)
                        m_bMouseOverUI |= InputHelper.IsPointerOverUI(Input.mousePosition);
        }
        else
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                m_bMouseOverUI =
                    InputHelper.IsPointerOverUI(Input.GetTouch(0).position);
        }

        // 必要：检测是否取消
        if (Utilitys.IsMousePlatform())
        {
            for (var i = 0; i < 2; ++i)
                if (Input.GetMouseButtonUp(i))
                    m_bMouseOverUI = false;
        }
        else
        {
            if (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended ||
                                         Input.GetTouch(0).phase == TouchPhase.Canceled)) m_bMouseOverUI = false;
        }

        // Touch one
        if (1 == Input.touchCount && TouchPhase.Moved == Input.GetTouch(0).phase && !m_bMouseOverUI)
        {
            var move_right = -Input.GetTouch(0).deltaPosition.x * 0.02F;
            var move_forward = -Input.GetTouch(0).deltaPosition.y * 0.02F;

            forward = mainCameraTrans.transform.forward;
            forward.y = 0;
            forward.Normalize();

            right = mainCameraTrans.transform.right;
            right.y = 0;
            right.Normalize();

            move = forward * move_forward + right * move_right;
            FocusCenter += move * transform.position.y * 0.05f;
            mainCameraTrans.transform.Translate(move * transform.position.y * 0.05f, Space.World);

            // 注释：Transform的Translate方法是一个逐步更新的方法，是在原来的数值上，再加上一个数值。相当于+=
            //mainCameraTrans.Translate(move);
        }

        // Touch two
        if (2 == Input.touchCount)
        {
            var newTouch1 = Input.GetTouch(0);
            var newTouch2 = Input.GetTouch(1);

            //第2点刚开始接触屏幕, 只记录，不做处理
            if (newTouch2.phase == TouchPhase.Began)
            {
                oldTouch2 = newTouch2;
                oldTouch1 = newTouch1;
            }
            else if (newTouch1.phase == TouchPhase.Moved && newTouch2.phase == TouchPhase.Moved)
            {
                //计算老的两点距离和新的两点间距离，变大要放大模型，变小要缩放模型
                var oldDistance = Vector2.Distance(oldTouch1.position, oldTouch2.position);
                var newDistance = Vector2.Distance(newTouch1.position, newTouch2.position);
                oldDis = oldDistance;
                newDis = newDistance;

                //两个距离之差，为正表示放大手势， 为负表示缩小手势
                var offset = newDistance - oldDistance;

                //放大因子， 一个像素按 0.001倍来算(1000可调整)
                var scaleFactor = offset / 1000f;

                if (newDistance > 10 && oldDistance > 10 && Mathf.Abs(newDistance - oldDistance) < 10)
                {
                    forward = mainCameraTrans.transform.forward;
                    forward.Normalize();
                    mainCameraTrans.transform.Translate(forward * scaleFactor * transform.position.y, Space.World);
                }
                else
                {
                    var dirTouch1 = newTouch1.position - oldTouch1.position;
                    var dirTouch2 = newTouch2.position - oldTouch2.position;

                    var curDir = (newTouch1.position - newTouch2.position).normalized;
                    var refDir = (oldTouch1.position - oldTouch2.position).normalized;

                    var dir1To2 = newTouch2.position - newTouch1.position;
                    var dir2To1 = newTouch1.position - newTouch2.position;

                    var dot = Vector2.Dot(dirTouch1.normalized, dirTouch2.normalized);
                    var ret = 0.0f;
                    if (dot < -0.7f) ret = Mathf.Rad2Deg * SignedAngle(refDir, curDir);


                    var angle_1 = Vector2.Angle(dirTouch1, dir1To2);
                    var angle_2 = Vector2.Angle(dirTouch2, dir2To1);
                    var angle_12 = Vector2.Angle(dirTouch1, dirTouch2);
                    var rotate_around_up = 0.0f;
                    var rotate_around_right = 0.0f;
                    if (angle_1 > 45.0f && angle_1 < 135.0f && angle_2 > 45.0f && angle_2 < 135.0f && angle_12 > 120.0f)
                    {
                        rotate_around_up = (newTouch1.position.x - oldTouch1.position.x) / 20;
                        rotate_around_up = ret;
                    }
                    else if (angle_12 < 30.0f && dirTouch1.y * dirTouch2.y > 0.0f)
                    {
                        rotate_around_right = -(newTouch1.position.y - oldTouch1.position.y) / 20;
                        rotate_around_up = ret;
                    }

                    right = mainCameraTrans.transform.right;
                    right.y = 0;
                    right.Normalize();

                    if (update_one_axis_one_time)
                    {
                        if (Mathf.Abs(rotate_around_right) > Mathf.Abs(rotate_around_up))
                            rotate_around_up = 0;
                        else
                            rotate_around_right = 0;
                    }

                    ProcCameraRotate(rotate_around_up, rotate_around_right);
                }
                if (newDistance < 300 && oldDistance < 300 && Mathf.Abs(newDistance - oldDistance) < 10)
                {
                    var rotate_around_up = (newTouch1.position.x - oldTouch1.position.x) / 20;
                    var rotate_around_right = -(newTouch1.position.y - oldTouch1.position.y) / 20;

                    right = mainCameraTrans.transform.right;
                    right.y = 0;
                    right.Normalize();

                    if (update_one_axis_one_time)
                    {
                        if (Mathf.Abs(rotate_around_right) > Mathf.Abs(rotate_around_up))
                            rotate_around_up = 0;
                        else
                            rotate_around_right = 0;
                    }

                    ProcCameraRotate(rotate_around_up, rotate_around_right);
                }

                oldTouch1 = newTouch1;
                oldTouch2 = newTouch2;
            }
        }

        if (Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.OSXEditor ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            if (Input.GetMouseButton(0) && !m_bMouseOverUI)
            {
                //停止当前Tweenr
                //if (null != MainProcess.inputStateManager)
                //{
                //    MainProcess.inputStateManager.viewpointTranslationManipulator.Stop(false);
                //}

                var move_right = -Input.GetAxis("Mouse X");
                var move_forward = -Input.GetAxis("Mouse Y");

                forward = mainCameraTrans.transform.forward;
                forward.y = 0;
                forward.Normalize();

                right = mainCameraTrans.transform.right;
                right.y = 0;
                right.Normalize();

                //             if (update_one_axis_one_time)
                //             {
                //                 if (Mathf.Abs(move_right) > Mathf.Abs(move_forward))
                //                 {
                //                     move_forward = 0;
                //                 }
                //                 else
                //                 {
                //                     move_right = 0;
                //                 }
                //             }

                move = forward * move_forward + right * move_right;
                FocusCenter += move * transform.position.y * 0.05f;
                //mainCameraTrans.transform.position += move;
                mainCameraTrans.transform.Translate(move * transform.position.y * 0.05f, Space.World);

                // 注释：Transform的Translate方法是一个逐步更新的方法，是在原来的数值上，再加上一个数值。相当于+=
                //mainCameraTrans.Translate(move);


                //PlayerInstance curPlayer = CharactorProxy.CurCacheNetworkPlayer;
                //if (curPlayer != null)
                {
                    //if (!UnityUtils.IsPointerOverUI( Input.mousePosition, ConstValue.c_sIgnoreUI ))
                    //{
                    //    Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );

                    //    RaycastHit hit = new RaycastHit();
                    //    LayerMask mask = 1 << LayerMask.NameToLayer( ConstValue.c_sGround ) | 1 << LayerMask.NameToLayer( ConstValue.c_sDefault );

                    //    if (Physics.Raycast( ray, out hit, Mathf.Infinity, mask.value ))

                    //    {

                    //        //   Debug.Log(hit.collider.gameObject.layer);

                    //        if (virtulobj == null)
                    //        {
                    //            virtulobj = Resouce.ResourcesManager.Instance.LoadAsset( ConstValue.c_virtulobj, false, false );
                    //            virtulobj.transform.position = new Vector3( hit.point.x, 0.5f, hit.point.z );
                    //        }
                    //        else
                    //        {

                    //            virtulobj.transform.position = new Vector3( hit.point.x, 0.5f, hit.point.z );
                    //            virtulobj.SetActive( true );
                    //        }
                    //    }
                    //}
                }
            }

            // 右键拖拽旋转
            if (Input.GetMouseButton(1) && !m_bMouseOverUI)
            {
                //停止当前Tweenr
                //if (null != MainProcess.inputStateManager)
                //{
                //    MainProcess.inputStateManager.viewpointTranslationManipulator.Stop(false);
                //}

                var rotate_around_up = Input.GetAxis("Mouse X");
                var rotate_around_right = -Input.GetAxis("Mouse Y");

                right = mainCameraTrans.transform.right;
                right.y = 0;
                right.Normalize();

                if (update_one_axis_one_time)
                {
                    if (Mathf.Abs(rotate_around_right) > Mathf.Abs(rotate_around_up))
                        rotate_around_up = 0;
                    else
                        rotate_around_right = 0;
                }

                ProcCameraRotate(rotate_around_up * 1.5f, rotate_around_right * 1.5f * 16.0f / 9.0f);
            }


            if (Input.GetAxis("Mouse ScrollWheel") > 0 && !m_bMouseOverUI)
            {
                forward = mainCameraTrans.transform.forward;
                forward.Normalize();
                mainCameraTrans.transform.Translate(forward * transform.position.y * 0.2f, Space.World);
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0 && !m_bMouseOverUI)
            {
                forward = mainCameraTrans.transform.forward;
                forward.Normalize();
                mainCameraTrans.transform.Translate(-forward * transform.position.y * 0.2f, Space.World);
            }
        }

        if (isMoverPayerTeleporting) PlayerTeleporting();

        Debug.DrawLine(lookPosition, FocusCenter, Color.red, 1);
    }

    public void OnLateUpdate()
    {
        if (Input.GetMouseButtonUp(1)) m_bIsRotating = false;
    }

    public void OnFixedUpdate()
    {
    }

    public static float SignedAngle( Vector2 from, Vector2 to )
    {
        // perpendicular dot product
        var perpDot = from.x * to.y - from.y * to.x;
        return Mathf.Atan2(perpDot, Vector2.Dot(from, to));
    }

    private void Awake()
    {
        //virtulobj = ResourcesManager.Instance.LoadAsset(ConstValue.c_virtulobj, false, false);
        //if (Virtulobj)
        //{
        //    virtulobj.SetActive(false);
        //}
        mainCameraTrans = Camera.main.transform;
    }

    private void Start()
    {
        if (mainCameraTrans == null)
        {
            mainCameraObj = GameObject.FindGameObjectWithTag("MainCamera");
            mainCameraTrans = mainCameraObj.transform;
        }

        move = new Vector3();
        FocusCenter = new Vector3();
        lookPosition = new Vector3();
        lookDirection = new Vector3();

        lookPosition = mainCameraTrans.position;

        m_DetectedLayerMask = LayerUtils.DragDetectedLayerMask;

        UpdateCenterAndPosition();
    }

    public Vector3 UpdateCenterAndPosition()
    {
        lookPosition = mainCameraTrans.position;
        lookDirection = mainCameraTrans.forward;
        if (Physics.Raycast(lookPosition, lookDirection, out hitinfo, 1000.0f, m_DetectedLayerMask /*, 1 << 8*/))
            FocusCenter = hitinfo.point;
        else
            //focusCenter.Set(0, 0, 0);
            FocusCenter = lookPosition + lookDirection * 100;

        if (lookPosition.y != 0.0f && lookDirection.y != 0.0f)
            FocusCenter = lookPosition + lookDirection / Mathf.Abs(lookDirection.y) * lookPosition.y;

        mainCameraTrans.LookAt(FocusCenter);

        return FocusCenter;
        //Debug.DrawLine( lookPosition, focusCenter, Color.red, 1 );
    }


    private RaycastHit hitinfo;
    private RaycastHit moveHitInfo;
    private bool isMoverPayerTeleporting;
    private float currentTime;
    private float gameTime;

    public void OnMovePalyerByMap()
    {
        isMoverPayerTeleporting = true;
    }

    private void PlayerTeleporting()
    {
        if (mMoveMode != ENMoveMode.EN_Map)
        {
            Virtulobj.SetActive(false);
            return;
        }

        var curPlayer = (PlayerInstance) GlobalManager.Instance.CachePlayer.SceneInstance;


        //        if (curPlayer == null || curPlayer.State != EN_CharacterActionState.EN_CHA_Stay)
        //        {
        //            return;
        //        }
        //LayerMask mask = 1 << LayerMask.NameToLayer(ConstValue.c_sGround) | 1 << LayerMask.NameToLayer(ConstValue.c_sVehicleRoad) | 1 << LayerMask.NameToLayer(ConstValue.c_sDefault);
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out moveHitInfo, m_DetectedLayerMask))
        {
            //Debug.Log("curPlayer");
            if (Virtulobj)
            {
                Virtulobj.transform.position = moveHitInfo.point;
                Virtulobj.SetActive(true);
            }
            if (Input.GetMouseButtonDown(0))
            {
                //todo:断开瞬移
                //curPlayer.InputState = EN_InputState.EPS_NoneInput;
                gameTime = Time.realtimeSinceStartup;
                if (gameTime - currentTime < 0.2f)
                {
                    curPlayer.transform.position = moveHitInfo.point;
                    //vp_Timer.In(0.3f, () => virtulobj.SetActive(false));
                    mMoveMode = ENMoveMode.EN_None;
                    isMoverPayerTeleporting = false;
                }
                currentTime = gameTime;
            }
        }
        else
        {
            Virtulobj.SetActive(false);
            isMoverPayerTeleporting = false;
        }

        //lookPosition = mainCameraTrans.position;
        //lookDirection = mainCameraTrans.forward;
        //LayerMask mask = 1 << LayerMask.NameToLayer(ConstValue.c_sGround) | 1 << LayerMask.NameToLayer(ConstValue.c_sVehicleRoad);
        //if (Physics.Raycast(lookPosition, lookDirection, out moveHitInfo, 1000.0f, mask))
        //{

        //    if (virtulobj)
        //    {
        //        virtulobj.transform.position = moveHitInfo.point;
        //        virtulobj.SetActive(true);
        //    }
        //    if (Input.GetKeyDown(KeyCode.Space))
        //    {
        //        curPlayer.transform.position = moveHitInfo.point;
        //        vp_Timer.In(0.3f, () => virtulobj.SetActive(false));
        //        mMoveMode = ENMoveMode.EN_None;
        //        isMoverPayerTeleporting = false;
        //    }
        //}
        //else
        //{
        //    virtulobj.SetActive(false);
        //    isMoverPayerTeleporting = false;
        //}
    }

#if false
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawSphere( focusCenter, 1.0f );
    }
#endif

    //private void OnEnable()
    //{
    //    CalculateFocusCenter();
    //}
    public void CalculateFocusCenter()
    {
        lookPosition = mainCameraTrans.position;
        lookDirection = mainCameraTrans.forward;

        if (Physics.Raycast(lookPosition, lookDirection, out hitinfo, m_DetectedLayerMask /*, 1 << 8*/))
            FocusCenter = hitinfo.point;

        // 高度与俯仰角关联，高度越高，俯仰角越大
        var limitAngle = Mathf.Abs(lookPosition.y) * 1.414f;
        if (Vector3.Distance(lookPosition, FocusCenter) > limitAngle) // 45°
            FocusCenter = lookPosition + lookDirection * limitAngle;

        mainCameraTrans.LookAt(FocusCenter);
    }

    // 2018-09-18 18:11:11 Shell Lee Todo
    // 修改为鼠标状态
    private bool m_bIsRotating;

    private void ProcCameraRotate( float rotate_around_up, float rotate_around_right )
    {
        if (!m_bIsRotating)
            // 2018-09-18 18:11:31 Shell Lee Todo
            // 移动位置到按键判断逻辑
            CalculateFocusCenter();

        m_bIsRotating = true;

        lookPosition = mainCameraTrans.transform.position;

        var oldLookPosition = lookPosition;

        // 2018-09-18 15:21:31 Shell Lee Todo
        // 距离计算策略
        var oldDistance = Vector3.Distance(oldLookPosition, FocusCenter);

        var eulerAngles = Quaternion.FromToRotation(Vector3.forward, FocusCenter - lookPosition).eulerAngles;
        eulerAngles.y += Quaternion.AngleAxis(rotate_around_up, Vector3.up).eulerAngles.y;

        eulerAngles.x += Quaternion.AngleAxis(rotate_around_right, Vector3.right).eulerAngles.x;
        eulerAngles.x %= 360.0f;
        eulerAngles.x = eulerAngles.x > 180.0f ? eulerAngles.x - 360.0f : eulerAngles.x;
        eulerAngles.x = Mathf.Clamp(eulerAngles.x, 5.0f, 85.0f);

        var tmplookDir = Matrix4x4.Rotate(Quaternion.Euler(eulerAngles)).MultiplyPoint(Vector3.forward);
        tmplookDir.Normalize();

        lookPosition = FocusCenter - tmplookDir * oldDistance;

        // 2018-09-18 19:07:00 Shell Lee Todo
        // 最低高度需要维护起来
        if (lookPosition.y < 0.5f) lookPosition.y = oldLookPosition.y;
        mainCameraTrans.transform.position = lookPosition;
        mainCameraTrans.LookAt(FocusCenter);
    }

    public bool Enable()
    {
        return enabled;
    }

    public bool CouldEnter()
    {
        return true;
    }

    public bool CouldLeave()
    {
        return true;
    }

    public void Enter()
    {
        enabled = true;
    }

    public void Leave()
    {
        enabled = false;
    }


    #region 消息处理

    private void OnEnable()
    {
        m_msgList = new[]
        {
            (ushort) EnInputEvent.MoveCamera,
            (ushort) EnInputEvent.RotateCamera,
            (ushort) EnInputEvent.ZoomCamera
        };
        RegisterMsg(m_msgList);
    }

    private void OnDisable()
    {
        UnRegisterMsg(m_msgList);
    }

    public override void HandleNotification( INotification notification )
    {
        switch (notification.Id)
        {
            case (ushort) EnInputEvent.MoveCamera:
                if (notification.Body is MouseEventArgs eventArgs) OnMoveCamera(eventArgs);

                break;
            case (ushort) EnInputEvent.RotateCamera:
                OnRotateCamera(notification.Body as MouseEventArgs);
                break;
            case (ushort) EnInputEvent.ZoomCamera:
                OnZoomCamera(notification.Body as MouseEventArgs);
                break;
        }
    }


    private void OnRotateCamera( MouseEventArgs eventArgs )
    {
        // 右键拖拽旋转
        //停止当前Tweenr
        //if (null != MainProcess.inputStateManager)
        //{
        //    MainProcess.inputStateManager.viewpointTranslationManipulator.Stop(false);
        //}

        var rotate_around_up = eventArgs.Delta.x;
        ;
        var rotate_around_right = -eventArgs.Delta.y;
        ;

        right = mainCameraTrans.transform.right;
        right.y = 0;
        right.Normalize();

        if (update_one_axis_one_time)
        {
            if (Mathf.Abs(rotate_around_right) > Mathf.Abs(rotate_around_up))
                rotate_around_up = 0;
            else
                rotate_around_right = 0;
        }

        ProcCameraRotate(rotate_around_up * 1.5f, rotate_around_right * 1.5f * 16.0f / 9.0f);
    }

    private void OnMoveCamera( MouseEventArgs eventArgs )
    {
        //停止当前Tweenr
        //if (null != MainProcess.inputStateManager)
        //{
        //    MainProcess.inputStateManager.viewpointTranslationManipulator.Stop(false);
        //}


        var move_right = eventArgs.Delta.x;
        var move_forward = eventArgs.Delta.y;
        //float move_right = -Input.GetAxis("Mouse X");
        //float move_forward = -Input.GetAxis("Mouse Y");

        forward = mainCameraTrans.transform.forward;
        forward.y = 0;
        forward.Normalize();

        right = mainCameraTrans.transform.right;
        right.y = 0;
        right.Normalize();
        move = forward * move_forward + right * move_right;
        //todo:焦点是否重新检测后进行获取
        var moveDelta = move * Mathf.Abs(transform.position.y) * 0.05f;

        FocusCenter += moveDelta;
        mainCameraTrans.transform.Translate(moveDelta, Space.World);
    }

    private void OnMoveCamera( Vector2 moveDelta )
    {
        if (Mathf.Abs(moveDelta.x) <= float.Epsilon && Mathf.Abs(moveDelta.y) <= float.Epsilon) return;
        //停止当前Tweenr
        //if (null != MainProcess.inputStateManager)
        //{
        //    MainProcess.inputStateManager.viewpointTranslationManipulator.Stop(false);
        //}
        var move_right = moveDelta.x;
        var move_forward = moveDelta.y;

        forward = mainCameraTrans.transform.forward;
        forward.y = 0;
        forward.Normalize();

        right = mainCameraTrans.transform.right;
        right.y = 0;
        right.Normalize();
        move = forward * move_forward + right * move_right;
        //todo:焦点是否重新检测后进行获取
        FocusCenter += move * transform.position.y * 0.05f;

        mainCameraTrans.transform.Translate(move * transform.position.y * 0.05f, Space.World);
    }

    private void OnZoomCamera( MouseEventArgs zoom )
    {
        if (zoom.Delta.x > 0)
        {
            forward = mainCameraTrans.transform.forward;
            forward.Normalize();
            var moveDelta = forward * Mathf.Abs(transform.position.y) * 0.2f;
            mainCameraTrans.transform.Translate(moveDelta, Space.World);
        }

        if (zoom.Delta.x < 0)
        {
            forward = mainCameraTrans.transform.forward;
            forward.Normalize();
            var moveDelta = -forward * Mathf.Abs(transform.position.y) * 0.2f;
            mainCameraTrans.transform.Translate(moveDelta, Space.World);
        }
    }

    #endregion
}