using Framework;
using Framework.SceneObject;
using Framework.SsitInput;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.DebugLog;
using SsitEngine.Unity.Msic;
using SsitEngine.Unity.SsitInput;
using UnityEngine;

public class VechileManipulator : SsitMonoBase, IInputState
{
    public Vector2 cameraInput = Vector2.zero;
    private ThirdPersonController cc;

    [HideInInspector] public bool changeCameraState; // generic bool to change the CameraState        

    [HideInInspector] public string customCameraState; // generic string to change the CameraState        

    [HideInInspector]
    public string customlookAtPoint; // generic string to change the CameraPoint of the Fixed Point Mode        

    private RaycastHit hit;

    [HideInInspector] public bool keepDirection; // keep the current direction in case you change the cameraState

    public bool lockCamera;
    public bool rotateToCameraWhileStrafe = true;

    [HideInInspector]
    public bool smoothCameraState; // generic bool to know if the state will change with or without lerp  


    [HideInInspector] public vThirdPersonCamera tpCamera; // acess camera info         

    private float zoomDelta;

    public void OnUpdate()
    {
    }

    public void OnLateUpdate()
    {
    }

    public void CameraUpdate()
    {
        CameraInput(); // update camera input           
        UpdateCameraStates(); // update camera states
    }

    public void OnFixedUpdate()
    {
    }

    public void SetRotateByWorld( bool value )
    {
        cc.rotateByWorld = value;
        keepDirection = value;
    }


    private void OnMouseClick( Vector3 arg0 )
    {
        if (Physics.Raycast(tpCamera.GetComponent<Camera>().ScreenPointToRay(arg0), out hit, Mathf.Infinity,
            LayerUtils.DragDetectedLayerMask))
            Facade.Instance.SendNotification((ushort) EnMouseEvent.RightDown, hit.point);
    }

    //建议后期尽可能分开相机模式控制（焦点模式）
    //public GenericInput rotateCameraXInput = new GenericInput("Mouse X", "RightAnalogHorizontal", "Mouse X");
    //public GenericInput rotateCameraYInput = new GenericInput("Mouse Y", "RightAnalogVertical", "Mouse Y");

    #region InputState

    public ENCameraModeType CameraMode => ENCameraModeType.PlayerManipulator;

    public bool Enable()
    {
        return cc && !cc.LockInput;
    }

    public bool CouldEnter()
    {
        if (CameraController.instance.target == null) return false;

        var player = CameraController.instance.target.GetComponent<PlayerInstance>();
        if (player != null) return !GlobalManager.Instance.IsSync || player.HasAuthority;
        SsitDebug.Debug("人物控制进入失败");

        return false;
    }

    public bool CouldLeave()
    {
        return true;
    }

    public void Enter()
    {
        if (CameraController.instance.target == null) return;
        tpCamera = CameraController.instance;
        cc = tpCamera.target.GetComponent<ThirdPersonController>();
        cc.OnLateUpdate.AddListener(CameraUpdate);
        SetRotateByWorld(false);
        m_msgList = new[]
        {
            (ushort) EnInputEvent.MoveCamera,
            (ushort) EnInputEvent.ZoomCamera
        };
        RegisterMsg(m_msgList);

        //注册右键事件
        //InputManager.Instance.EventOnRightClick.AddListener(OnMouseClick);
    }


    public void Leave()
    {
        SetRotateByWorld(true);
        cc.OnLateUpdate.RemoveListener(CameraUpdate);
        UnRegisterMsg(m_msgList);
        //        InputManager.Instance.EventOnRightClick.RemoveListener(OnMouseClick);

        cc = null;
    }

    #endregion

    #region Camera Methods

    public void CameraInput()
    {
        if (tpCamera == null || lockCamera)
            return;

        if (!keepDirection) cc.UpdateTargetDirection(tpCamera.transform);


        // 加入!cameraDrag判断，来实现从三维场景拖拽到ui界面之后，继续响应拖拽事件
        if (cc.IsCameraDragable) RotateWithCamera(tpCamera.transform);

        //Y = rotateCameraYInput.GetAxis();
        //X = rotateCameraXInput.GetAxis();
        var Y = -cameraInput.y;
        var X = -cameraInput.x;
        // 旋转移动摄像机
        tpCamera.RotateCamera(X, Y);

        tpCamera.Zoom(zoomDelta);

        // change keedDirection from input diference
        if (keepDirection && Vector2.Distance(cc.input, cc.oldInput) > 0.2f) keepDirection = false;
    }

    protected void UpdateCameraStates()
    {
        // CAMERA STATE - you can change the CameraState here, the bool means if you want lerp of not, make sure to use the same CameraState String that you named on TPCameraListData

        if (changeCameraState && !cc.isStrafing)
            tpCamera.ChangeState(customCameraState, customlookAtPoint, smoothCameraState);
        else if (cc.isCrouching)
            tpCamera.ChangeState("Crouch", true);
        else if (cc.isStrafing)
            tpCamera.ChangeState("Strafing", true);
        else
            tpCamera.ChangeState("Default", true);
    }

    protected void RotateWithCamera( Transform cameraTransform )
    {
        if (rotateToCameraWhileStrafe && cc.isStrafing && !cc.actions && !cc.lockMovement)
        {
            // smooth align character with aim position               
            if (tpCamera != null && tpCamera.lockTarget)
                cc.RotateToTarget(tpCamera.lockTarget);
            // rotate the camera around the character and align with when the character move
            else //if (cc.input != Vector2.zero) 保持方向一直朝向相机方向
                cc.RotateWithAnotherTransform(cameraTransform);
        }
    }

    #endregion


    #region 消息处理

    public override void HandleNotification( INotification notification )
    {
        switch (notification.Id)
        {
            //[test:点击测试效果不是很好方案后期设计替换弃用]
            case (ushort) EnInputEvent.MoveCamera:
                OnRotateCamera(notification.Body as MouseEventArgs);
                break;
            case (ushort) EnInputEvent.ZoomCamera:
                OnZoomCamera(notification.Body as MouseEventArgs);
                break;
        }
    }

    private void OnZoomCamera( MouseEventArgs mouseEventArgs )
    {
        zoomDelta = mouseEventArgs.Delta.x;
    }


    private void OnRotateCamera( MouseEventArgs mouseEventArgs )
    {
        cameraInput = mouseEventArgs.Delta;
    }

    #endregion

    /*

        [Tooltip("The offset between the anchor and the camera.")]
        [SerializeField]
        protected Vector3 m_LookOffset = new Vector3(0.5f, 0, -2.5f);
        public Vector3 m_CurrentLookOffset;
        public Vector3 m_CollisionAnchorOffset;
        protected float m_CollisionRadius = 0.05f;
        private Vector3 m_SmoothPositionVelocity;
        private Vector3 m_ObstructionSmoothPositionVelocity;
        private Vector3 m_SmoothLookOffsetVelocity;
        [Tooltip("The amount of smoothing to apply to the position. Can be zero.")]
        [SerializeField] protected float m_PositionSmoothing = 0.1f;
        [Tooltip("The amount of smoothing to apply to the look offset. Can be zero.")]
        [SerializeField] protected float m_LookOffsetSmoothing = 0.05f;
        [Tooltip("The amount of smoothing to apply to the position when an object is obstructing the target position. Can be zero.")]
        [SerializeField] protected float m_ObstructionPositionSmoothing = 0.04f;
        [Tooltip("The positional spring used for regular movement.")]
        [SerializeField] protected Spring m_PositionSpring;
        [Tooltip("The rotational spring used for regular movement.")]
        [SerializeField] protected Spring m_RotationSpring;
        [Tooltip("The positional spring which returns to equilibrium after a small amount of time (for recoil).")]
        [SerializeField] protected Spring m_SecondaryPositionSpring;
        [Tooltip("The rotational spring which returns to equilibrium after a small amount of time (for recoil).")]
        [SerializeField] protected Spring m_SecondaryRotationSpring;


        private const int DefaultLayer = 0;
        private const int TransparentFXLayer = 1;
        private const int IgnoreRaycastLayer = 2;
        private const int WaterLayer = 4;
        private const int UILayer = 5;

        public static int Default { get { return DefaultLayer; } }
        public static int TransparentFX { get { return TransparentFXLayer; } }
        public static int IgnoreRaycast { get { return IgnoreRaycastLayer; } }
        public static int Water { get { return WaterLayer; } }
        public static int UI { get { return UILayer; } }

        [SerializeField] protected LayerMask m_InvisibleLayers = (1 << TransparentFX) | (1 << IgnoreRaycast) | (1 << UI);

        private LayerMask m_CharacterLayer = 1 << 9;/*LayerManager.Character;#1#
        public int IgnoreInvisibleCharacterWaterLayers { get { return ~(m_InvisibleLayers | m_CharacterLayer | (1 << 4)); } }

        /// <summary>
        /// 根据当前的俯仰和偏航值移动摄像机（第三视角新版摄像机跟随移动）
        /// </summary>
        public void CameraMovementNew( bool immediateUpdate )
        {
            var characterRotation = cc.transform.rotation;
            // 防止其他物体阻塞。根据角色扮演者的位置而不是外观位置检查障碍物，因为角色应始终可见。如果看的位置不是直接可见的，那就没什么关系了。
            var anchorPosition = tpCamera.GetAnchorPosition();
            m_CurrentLookOffset = immediateUpdate ? m_LookOffset : Vector3.SmoothDamp(m_CurrentLookOffset, m_LookOffset, ref m_SmoothLookOffsetVelocity, m_LookOffsetSmoothing);
            var lookPosition = anchorPosition + (m_CurrentLookOffset.x * transform.right) + (m_CurrentLookOffset.y * cc.transform.up) + ((m_CurrentLookOffset.z + tpCamera.GetZoomStep()) * transform.forward);

            // The position spring is already smoothed so it doesn't need to be included in SmoothDamp.
            lookPosition += transform.TransformDirection(m_PositionSpring.Value + m_SecondaryPositionSpring.Value);
            // Keep the look position above water.
            if (Physics.Linecast(cc.transform.position, cc.transform.position + cc.transform.up * cc.colliderHeight, out m_RaycastHit, 1 << 4))
            {
                if (lookPosition.y < m_RaycastHit.point.y)
                {
                    lookPosition.y = m_RaycastHit.point.y;
                }
            }

            // Smoothly move into position.
            Vector3 targetPosition;
            if (immediateUpdate)
            {
                targetPosition = lookPosition;
            }
            else
            {
                targetPosition = Vector3.SmoothDamp(transform.position, lookPosition, ref m_SmoothPositionVelocity, m_PositionSmoothing);
            }

            // todo:需要将人物身上附属的子碰撞框层级调整到不理睬摄像机检测
            //var collisionLayerEnabled = m_CharacterLocomotion.CollisionLayerEnabled;
            //m_CharacterLocomotion.EnableColliderCollisionLayer(false);
            var direction = lookPosition - (anchorPosition + m_CollisionAnchorOffset);
            // Fire a sphere to prevent the camera from colliding with other objects.
            if (Physics.SphereCast(anchorPosition + m_CollisionAnchorOffset - direction.normalized * m_CollisionRadius, m_CollisionRadius, direction.normalized, out m_RaycastHit, direction.magnitude,
                                IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore))
            {
                // Move the camera in if the character isn't in view.
                targetPosition = m_RaycastHit.point + m_RaycastHit.normal * m_CollisionRadius;
                if (!immediateUpdate)
                {
                    targetPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref m_ObstructionSmoothPositionVelocity, m_ObstructionPositionSmoothing);
                }

                // Keep a constant height if there is nothing getting in the way of that position.
                var localDirection = cc.transform.TransformDirection(direction);
                if (localDirection.y > 0)
                {
                    // Account for local y values.
                    var constantHeightPosition = MathUtils.InverseTransformPoint(cc.transform.position, cc.transform.rotation, targetPosition);
                    constantHeightPosition.y = MathUtils.InverseTransformPoint(cc.transform.position, characterRotation, lookPosition).y;
                    constantHeightPosition = MathUtils.TransformPoint(cc.transform.position, characterRotation, constantHeightPosition);
                    direction = constantHeightPosition - (anchorPosition + m_CollisionAnchorOffset);
                    if (!Physics.SphereCast(anchorPosition + m_CollisionAnchorOffset - direction.normalized * m_CollisionRadius, m_CollisionRadius, direction.normalized,
                            out m_RaycastHit, direction.magnitude - m_CollisionRadius, IgnoreInvisibleCharacterWaterLayers, QueryTriggerInteraction.Ignore))
                    {
                        targetPosition = constantHeightPosition;
                    }
                }
            }
            //m_CharacterLocomotion.EnableColliderCollisionLayer(collisionLayerEnabled);

            // 防止相机对角色进行裁剪。
            Collider containsCollider;
            if ((containsCollider = cc.BoundsCountains(targetPosition)) != null)
            {
                targetPosition = containsCollider.ClosestPointOnBounds(targetPosition);
            }

            // 目标位置不得低于角色的位置
            var localTargetPosition = cc.transform.InverseTransformPoint(targetPosition);
            if (localTargetPosition.y < 0)
            {
                localTargetPosition.y = 0;
                targetPosition = cc.transform.TransformPoint(localTargetPosition);
            }

            transform.position = targetPosition;
        }
    */
}