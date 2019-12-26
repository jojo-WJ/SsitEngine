using System.Collections.Generic;
using DG.Tweening;
using Framework.Mirror;
using Framework.SceneObject;
using Invector;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// ReSharper disable All


namespace Framework
{
    public enum CameraStateType
    {
        Default,
        Strafing,
        Crouch,
        Pendulum
    }

    public enum ENCameraModeType
    {
        EmptyManipulator,
        ThreeViewManipulator,
        FocusModelManipulator,
        CrossPlatformManipulator,
        FollowScreenManipulator,
        ViewpointTranslationManipulator,
        PlayerManipulator,
    }

    public class CameraController : vThirdPersonCamera
    {
        private static CameraController _instance;

        private Vector3 m_homePosition;
        private Quaternion m_homeRotation;

        public float CurrentZoom
        {
            get { return currentZoom; }
            set { currentZoom = value; }
        }

        public Transform TargetLookAt
        {
            get { return targetLookAt; }
        }

        public static CameraController instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CameraController>();

                    //Tell unity not to destroy this object when loading a new scene!
                    //DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
            private set { _instance = value; }
        }

        private void Awake()
        {
            instance = this;
            m_homePosition = transform.position;
            m_homeRotation = transform.rotation;
        }

        protected override void Start()
        {
            base.Start();
            InitCameraMode();
        }

        //protected void FixedUpdate()
        //{
        //    if (m_currentCameraMode == null)
        //    {
        //        return;
        //    }

        //    if (m_currentCameraMode.Enable())
        //    {
        //        m_currentCameraMode.OnFixedUpdate();
        //    }
        //}

        protected void Update()
        {
            if (m_currentCameraMode == null)
            {
                return;
            }

            if (m_currentCameraMode.Enable())
            {
                m_currentCameraMode.OnUpdate();
            }
        }

        protected override void LateUpdate()
        {
            if (m_currentCameraMode == null)
            {
                return;
            }

            if (m_currentCameraMode.Enable())
            {
                m_currentCameraMode.OnLateUpdate();
                switch (m_currentCameraMode.CameraMode)
                {
                    case ENCameraModeType.PlayerManipulator:
                        base.LateUpdate();
                        break;
                    case ENCameraModeType.FollowScreenManipulator:
                    {
                        base.LateUpdate();
                        if (currentState.cameraMode == TPCameraMode.FreeDirectional)
                        {
                            //CameraFollowMovement();
                            CameraMovement();
                        }
                    }
                        break;
                }
            }
        }

        /// <summary>
        /// 视锥碰撞动态优化
        /// </summary>
        /// <param name="p_mode"></param>
        /// <returns></returns>
        protected float EstimateDistanceToLevel( EEstimateDistanceMode p_mode )
        {
            float camDist = 300f;
            if (_camera != null)
            {
                // raycast four rays to find out the approximate distance between camera and level
                float hitCount = 0;
                Vector3 hitValue = Vector3.zero;
                float nearestHit = float.MaxValue;
                for (float x = 0; x < 2; x++)
                {
                    for (float y = 0; y < 2; y++)
                    {
                        var temPos = new Vector3(_camera.rect.width * Screen.width * (0.25f + 0.5f * x),
                            _camera.rect.height * Screen.height * (0.25f + 0.5f * y), 0f);
                        Ray ray = _camera.ScreenPointToRay(temPos);
                        //if (Engine.Debug)
                        //{
                        //    Debug.DrawLine(cam.transform.position, temPos, Color.green, 0.3f);
                        //}
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit))
                        {
                            hitCount++;
                            if (p_mode == EEstimateDistanceMode.AVERAGE)
                            {
                                hitValue += hit.point;
                            }
                            else
                            {
                                float distToCam = (hit.point - _camera.transform.position).magnitude;
                                if (nearestHit > distToCam)
                                {
                                    nearestHit = distToCam;
                                    hitValue = hit.point;
                                }
                            }
                        }
                    }
                }

                if (hitCount > 0)
                {
                    if (p_mode == EEstimateDistanceMode.AVERAGE)
                    {
                        hitValue /= hitCount;
                    }

                    camDist = (hitValue - _camera.transform.position).magnitude;
                }
            }

            return camDist;
        }

        /// <summary>
        /// 估算最小距离
        /// </summary>
        protected enum EEstimateDistanceMode
        {
            AVERAGE,
            NEAREST
        }

        #region AutoRevert Camera Move：视角旋转结束后，自动还原到转动之前的视角

        float mouseXPrevious;
        float mouseYPrevious;

        public bool IsAutoRevertMoving { get; private set; } = false;


        /// <summary>
        /// 启动自动回退[视角旋转]
        /// </summary>
        /// <param name="data">输入数据，主要是想记录触摸点id，方便其它层判断</param>
        public void StartAutoRevertRotate( PointerEventData data = null )
        {
            IsAutoRevertMoving = true;

            mouseXPrevious = MouseX;
            mouseYPrevious = MouseY;

            PointerId = data?.pointerId ?? -1;
        }

        /// <summary>
        /// 停止自动回退[视角旋转]，并开启自动回退视角动画
        /// </summary>
        /// <param name="onStopRevertRotateFinished">视角回退完毕回调</param>
        /// <param name="duration">耗时</param>
        public void StopAutoRevertRotate( UnityAction onStopRevertRotateFinished, float duration = 1.0f )
        {
            DOTween.To(() => MouseX, x => mouseX = x, mouseXPrevious, duration).OnComplete(() =>
            {
                IsAutoRevertMoving = false;

                PointerId = -1;

                onStopRevertRotateFinished?.Invoke();
            });

            DOTween.To(() => MouseY, y => MouseY = y, mouseYPrevious, duration).OnComplete(() => { });
        }

        public void RotateToFirstPersonViewPoint( Transform tgt, UnityAction onRotateToFirstPersonViewPointFinished,
            float duration = 1.0f )
        {
            if (tgt)
            {
                IsAutoRevertMoving = true;

                DOTween.To(() => MouseX, x => mouseX = x, tgt.transform.eulerAngles.y, duration).OnComplete(() =>
                {
                    IsAutoRevertMoving = false;

                    onRotateToFirstPersonViewPointFinished?.Invoke();
                });
            }
        }

        #endregion

        #region CalculateCameraMovement

        private Vector3 m_resultPosition;
        private Quaternion m_resultRotation;

        //protected override void CameraMovement()
        //{
        //    if (CalculateCameraMovement(currentTarget, currentState, lerpState, useSmooth, out m_resultPosition,
        //        out m_resultRotation))
        //    {
        //        transform.position = m_resultPosition;
        //        transform.rotation = m_resultRotation;
        //    }
        //}

        public bool CalculateCameraMovement( Transform tgt, vThirdPersonCameraState tgtState,
            vThirdPersonCameraState tgtLerpState, bool tgtUseSmooth, out Vector3 resultPosition,
            out Quaternion resultRotation, bool calculateEndValue = false )
        {
            if (tgt == null || tgtState == null)
            {
                resultPosition = -Vector3.forward;
                resultRotation = Quaternion.identity;
                return false;
            }

            if (tgtUseSmooth)
            {
                tgtState.Slerp(tgtLerpState, smoothBetweenState * Time.deltaTime);
            }
            else
                tgtState.CopyState(tgtLerpState);

            if (tgtState.useZoom)
            {
                currentZoom = Mathf.Clamp(currentZoom, tgtState.minDistance, tgtState.maxDistance);
                distance = tgtUseSmooth
                    ? Mathf.Lerp(distance, currentZoom, tgtLerpState.smoothFollow * Time.deltaTime)
                    : currentZoom;
            }
            else
            {
                distance = tgtUseSmooth
                    ? Mathf.Lerp(distance, tgtState.defaultDistance, tgtLerpState.smoothFollow * Time.deltaTime)
                    : tgtState.defaultDistance;
                currentZoom = tgtState.defaultDistance;
            }

            _camera.fieldOfView = tgtState.fov;
            cullingDistance = Mathf.Lerp(cullingDistance, currentZoom, smoothBetweenState * Time.deltaTime);
            currentSwitchRight = Mathf.Lerp(currentSwitchRight, switchRight, smoothBetweenState * Time.deltaTime);

            //ThirdPersonManipulator thirdPersonManipulator = tgt.GetComponentInChildren<PlayerInstance>(true);

            //if (!tgtUseSmooth && thirdPersonManipulator && thirdPersonManipulator.FirstPersonManipulator)
            //{
            //    // mouseY = tgt.eulerAngles.x; // 屏蔽掉，保持俯仰角不变
            //    mouseX = tgt.eulerAngles.y;

            //    targetLookAt.rotation = Quaternion.Euler(targetLookAt.eulerAngles.x, mouseX, 0);
            //}

            var camDir = (tgtState.forward * targetLookAt.forward) +
                         ((tgtState.right * currentSwitchRight) * targetLookAt.right);

            camDir = camDir.normalized;

            var targetPos = new Vector3(tgt.position.x, tgt.position.y + offSetPlayerPivot, tgt.position.z);
            currentTargetPos = targetPos;
            desired_cPos = targetPos + new Vector3(0, tgtState.height, 0);
            current_cPos = targetPos + new Vector3(0, currentHeight, 0);
            RaycastHit hitInfo;

            ClipPlanePoints planePoints =
                _camera.NearClipPlanePoints(current_cPos + (camDir * (distance)), clipPlaneMargin);
            ClipPlanePoints oldPoints =
                _camera.NearClipPlanePoints(desired_cPos + (camDir * currentZoom), clipPlaneMargin);
            //Check if Height is not blocked 
            if (Physics.SphereCast(targetPos, checkHeightRadius, Vector3.up, out hitInfo, tgtState.cullingHeight + 0.2f,
                cullingLayer))
            {
                var t = hitInfo.distance - 0.2f;
                t -= tgtState.height;
                t /= (tgtState.cullingHeight - tgtState.height);
                cullingHeight = Mathf.Lerp(tgtState.height, tgtState.cullingHeight, Mathf.Clamp(t, 0.0f, 1.0f));
            }
            else
            {
                cullingHeight = tgtUseSmooth
                    ? Mathf.Lerp(cullingHeight, tgtState.cullingHeight, smoothBetweenState * Time.deltaTime)
                    : tgtState.cullingHeight;
            }

            //Check if desired target position is not blocked       
            if (CullingRayCast(desired_cPos, oldPoints, out hitInfo, currentZoom + 0.2f, cullingLayer, Color.blue))
            {
                distance = hitInfo.distance - 0.2f;
                if (distance < tgtState.defaultDistance)
                {
                    var t = hitInfo.distance;
                    t -= tgtState.cullingMinDist;
                    t /= (currentZoom - tgtState.cullingMinDist);
                    currentHeight = Mathf.Lerp(cullingHeight, tgtState.height, Mathf.Clamp(t, 0.0f, 1.0f));
                    current_cPos = targetPos + new Vector3(0, currentHeight, 0);
                }
            }
            else
            {
                currentHeight = tgtUseSmooth
                    ? Mathf.Lerp(currentHeight, tgtState.height, smoothBetweenState * Time.deltaTime)
                    : tgtState.height;
            }

            //Check if target position with culling height applied is not blocked
            if (CullingRayCast(current_cPos, planePoints, out hitInfo, distance, cullingLayer, Color.cyan))
                distance = Mathf.Clamp(cullingDistance, 0.0f, tgtState.defaultDistance);
            var lookPoint = current_cPos + targetLookAt.forward * 2f;
            lookPoint += (targetLookAt.right * Vector3.Dot(camDir * (distance), targetLookAt.right));
            targetLookAt.position = current_cPos;

            Quaternion newRot = Quaternion.Euler(MouseY, MouseX, 0);
            targetLookAt.rotation = tgtUseSmooth
                ? Quaternion.Lerp(targetLookAt.rotation, newRot, smoothCameraRotation * Time.deltaTime)
                : newRot;
            resultPosition = current_cPos + (camDir * (distance));
            var tempRotation = Quaternion.LookRotation((lookPoint) - resultPosition);

            if (lockTarget)
            {
                CalculeLockOnPoint();

                if (!(tgtState.cameraMode.Equals(TPCameraMode.FixedAngle)))
                {
                    var collider = lockTarget.GetComponent<Collider>();
                    if (collider != null)
                    {
                        var point = (collider.bounds.center + Vector3.up * heightOffset) - resultPosition;
                        var euler = Quaternion.LookRotation(point).eulerAngles - tempRotation.eulerAngles;
                        if (isNewTarget)
                        {
                            lookTargetAdjust.x = Mathf.LerpAngle(lookTargetAdjust.x, euler.x,
                                tgtState.smoothFollow * Time.deltaTime);
                            lookTargetAdjust.y = Mathf.LerpAngle(lookTargetAdjust.y, euler.y,
                                tgtState.smoothFollow * Time.deltaTime);
                            lookTargetAdjust.z = Mathf.LerpAngle(lookTargetAdjust.z, euler.z,
                                tgtState.smoothFollow * Time.deltaTime);
                            // Quaternion.LerpUnclamped(lookTargetAdjust, Quaternion.Euler(euler), tgtState.smoothFollow * Time.deltaTime);
                            if (Vector3.Distance(lookTargetAdjust, euler) < .5f)
                                isNewTarget = false;
                        }
                        else
                            lookTargetAdjust = euler;
                    }
                }
            }
            else
            {
                lookTargetAdjust.x = Mathf.LerpAngle(lookTargetAdjust.x, 0, tgtState.smoothFollow * Time.deltaTime);
                lookTargetAdjust.y = Mathf.LerpAngle(lookTargetAdjust.y, 0, tgtState.smoothFollow * Time.deltaTime);
                lookTargetAdjust.z = Mathf.LerpAngle(lookTargetAdjust.z, 0, tgtState.smoothFollow * Time.deltaTime);
                //lookTargetAdjust = Quaternion.LerpUnclamped(lookTargetAdjust, Quaternion.Euler(Vector3.zero), 1 * Time.deltaTime);
            }

            var tempEuler = tempRotation.eulerAngles + lookTargetAdjust;
            tempEuler.z = 0;
            resultRotation = Quaternion.Euler(tempEuler + tgtState.rotationOffSet);
            movementSpeed = Vector2.zero;

            return true;
        }

        public vThirdPersonCameraState GetThirdPersonCameraState( string name )
        {
            return CameraStateList != null
                ? CameraStateList.tpCameraStates.Find(obj => obj.Name.Equals(name))
                : new vThirdPersonCameraState("Default");
        }

        /// <summary>
        /// 计算相机位置
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public Vector3 CalculateTargetLookAt( Transform tgt, string targetStateName = "Default",
            bool isFirstPersonManipulator = false )
        {
            vThirdPersonCameraState targetState = GetThirdPersonCameraState(targetStateName);

            float targetRotationXAxis = Mathf.Clamp(Camera.main.transform.eulerAngles.x - targetState.rotationOffSet.x
                , targetState.yMinLimit
                , targetState.yMaxLimit);

            float targetRotationYAxis = Camera.main.transform.eulerAngles.y - transform.root.eulerAngles.y;

            // 修改关键变量，因为TargetLookAt的结果最终是由 mouseX mouseX 决定的
            if (isFirstPersonManipulator)
            {
                MouseY = tgt.eulerAngles.x;
                mouseX = tgt.eulerAngles.y;
            }
            else
            {
                MouseY = targetRotationXAxis;
                mouseX = targetRotationYAxis;
            }

            if (tgt.GetComponentInChildren<PlayerInstance>(true))
            {
                currentZoom = targetState.defaultDistance;
            }
            else
            {
                // 计算相机距离
                RaycastHit raycastHit = new RaycastHit();
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out raycastHit,
                    1000.0f))
                {
                    currentZoom = Mathf.Min(Vector3.Distance(Camera.main.transform.position, raycastHit.point),
                        currentZoom);
                }
            }

            if (targetState.useZoom)
            {
                currentZoom = Mathf.Clamp(currentZoom, targetState.minDistance, targetState.maxDistance);
            }

            targetLookAt.eulerAngles = new Vector3(MouseY, MouseX, 0) + Vector3.right * targetState.right;
            targetLookAt.position = tgt.position + Vector3.up * targetState.height;

            distance = currentZoom; // 赋值，否则定位人物后，视角会闪动

            // 2019-04-16 21:22:24 Shell Lee Todo
            // 需要考虑人物左右偏移
            // 返回相机位置
            return targetLookAt.position - targetLookAt.forward * distance;
        }


        protected virtual void CameraFollowMovement()
        {
            if (currentTarget == null)
                return;

            if (useSmooth)
            {
                currentState.Slerp(lerpState, smoothBetweenState * Time.deltaTime);
            }
            else
            {
                currentState.CopyState(lerpState);
            }

            if (currentState.useZoom)
            {
                currentZoom = Mathf.Clamp(currentZoom, currentState.minDistance, currentState.maxDistance);
                distance = useSmooth
                    ? Mathf.Lerp(distance, currentZoom, lerpState.smoothFollow * Time.deltaTime)
                    : currentZoom;
            }
            else
            {
                distance = useSmooth
                    ? Mathf.Lerp(distance, currentState.defaultDistance, lerpState.smoothFollow * Time.deltaTime)
                    : currentState.defaultDistance;
                currentZoom = currentState.defaultDistance;
            }

            _camera.fieldOfView = currentState.fov;
            cullingDistance = Mathf.Lerp(cullingDistance, currentZoom, smoothBetweenState * Time.deltaTime);
            currentSwitchRight = Mathf.Lerp(currentSwitchRight, switchRight, smoothBetweenState * Time.deltaTime);
            var camDir = (currentState.forward * targetLookAt.forward) +
                         ((currentState.right * currentSwitchRight) * targetLookAt.right);

            camDir = camDir.normalized;

            var targetPos = new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot,
                currentTarget.position.z);
            currentTargetPos = targetPos;
            desired_cPos = targetPos + new Vector3(0, currentState.height, 0);
            current_cPos = targetPos + new Vector3(0, currentHeight, 0);
            RaycastHit hitInfo;

            ClipPlanePoints planePoints =
                _camera.NearClipPlanePoints(current_cPos + (camDir * (distance)), clipPlaneMargin);
            ClipPlanePoints oldPoints =
                _camera.NearClipPlanePoints(desired_cPos + (camDir * currentZoom), clipPlaneMargin);
            //Check if Height is not blocked 
            if (Physics.SphereCast(targetPos, checkHeightRadius, Vector3.up, out hitInfo,
                currentState.cullingHeight + 0.2f, cullingLayer))
            {
                var t = hitInfo.distance - 0.2f;
                t -= currentState.height;
                t /= (currentState.cullingHeight - currentState.height);
                cullingHeight = Mathf.Lerp(currentState.height, currentState.cullingHeight, Mathf.Clamp(t, 0.0f, 1.0f));
            }
            else
            {
                cullingHeight = useSmooth
                    ? Mathf.Lerp(cullingHeight, currentState.cullingHeight, smoothBetweenState * Time.deltaTime)
                    : currentState.cullingHeight;
            }
            //Check if desired target position is not blocked       
            if (CullingRayCast(desired_cPos, oldPoints, out hitInfo, currentZoom + 0.2f, cullingLayer, Color.blue))
            {
                distance = hitInfo.distance - 0.2f;
                if (distance < currentState.defaultDistance)
                {
                    var t = hitInfo.distance;
                    t -= currentState.cullingMinDist;
                    t /= (currentZoom - currentState.cullingMinDist);
                    currentHeight = Mathf.Lerp(cullingHeight, currentState.height, Mathf.Clamp(t, 0.0f, 1.0f));
                    current_cPos = targetPos + new Vector3(0, currentHeight, 0);
                }
            }
            else
            {
                currentHeight = useSmooth
                    ? Mathf.Lerp(currentHeight, currentState.height, smoothBetweenState * Time.deltaTime)
                    : currentState.height;
            }
            //Check if target position with culling height applied is not blocked
            if (CullingRayCast(current_cPos, planePoints, out hitInfo, distance, cullingLayer, Color.cyan))
            {
                distance = Mathf.Clamp(cullingDistance, 0.0f, currentState.defaultDistance);
            }
            //var lookPoint = current_cPos + targetLookAt.forward * 2f;
            //lookPoint += (targetLookAt.right * Vector3.Dot(camDir * (distance), targetLookAt.right));
            targetLookAt.position = current_cPos;
            Quaternion newRot = transform.rotation;
            targetLookAt.rotation = useSmooth
                ? Quaternion.Lerp(targetLookAt.rotation, newRot, smoothCameraRotation * Time.deltaTime)
                : newRot;
            //targetLookAt.rotation = transform.rotation;
            transform.position = current_cPos + (camDir * (distance));
            movementSpeed = Vector2.zero;

            transform.rotation = targetRot;
        }

        public Quaternion targetRot { get; set; }

        public float MouseY
        {
            get { return mouseY; }
            set { mouseY = value; }
        }

        public float MouseX
        {
            get { return mouseX; }
            set { mouseX = value; }
        }

        #endregion

        #region 相机模式变化

        public Dictionary<ENCameraModeType, IInputState> mCameraModeMaps =
            new Dictionary<ENCameraModeType, IInputState>();

        private IInputState m_currentCameraMode;
        private IInputState m_defaultCameraMode;

        /// <summary>
        /// 摄像机的默认操作模式
        /// </summary>
        public IInputState DefaultCameraMode
        {
            get { return m_defaultCameraMode; }
            set { m_defaultCameraMode = value; }
        }

        /// <summary>
        /// 当前相机模式是否是默认模式
        /// </summary>
        public bool IsDefaultCameraMode
        {
            get { return m_defaultCameraMode == m_currentCameraMode; }
        }

        /// <summary>
        /// 当前的摄像机操作模式
        /// </summary>
        public IInputState CurrentCameraMode
        {
            get { return m_currentCameraMode; }
        }


        void InitCameraMode()
        {
            EmptyManipulator emptyManipulator = new EmptyManipulator();
            mCameraModeMaps.Add(ENCameraModeType.EmptyManipulator, emptyManipulator);
            // 三视图操作器
            // 目前只有俯视图
            ThreeViewManipulator threeViewManipulator = gameObject.GetComponentInChildren<ThreeViewManipulator>(true);
            mCameraModeMaps.Add(ENCameraModeType.ThreeViewManipulator, threeViewManipulator);

            FocusModelManipulator focusModelManipulator =
                gameObject.GetComponentInChildren<FocusModelManipulator>(true);
            mCameraModeMaps.Add(ENCameraModeType.FocusModelManipulator, focusModelManipulator);

            // 跨平台操作器（拖拽、旋转、缩放）
            CrossPlatformManipulator crossPlatformManipulator =
                gameObject.GetComponentInChildren<CrossPlatformManipulator>(true);
            mCameraModeMaps.Add(ENCameraModeType.CrossPlatformManipulator, crossPlatformManipulator);

            // 监控视角操作器（导调端）
            FollowScreenManipulator followScreenManipulator = gameObject.GetOrAddComponent<FollowScreenManipulator>();
            mCameraModeMaps.Add(ENCameraModeType.FollowScreenManipulator, followScreenManipulator);

            // 视角变换操作器（模型定位、视角定位）
            ViewpointTranslationManipulator viewpointTranslationManipulator =
                gameObject.GetOrAddComponent<ViewpointTranslationManipulator>();
            mCameraModeMaps.Add(ENCameraModeType.ViewpointTranslationManipulator, viewpointTranslationManipulator);

            // hack:人物控制器（后整理）
            PlayerManipulator playerManipulator = gameObject.GetOrAddComponent<PlayerManipulator>();
            mCameraModeMaps.Add(ENCameraModeType.PlayerManipulator, playerManipulator);

#if false
            // 模型编辑操作器
            //inputStateManager.add( SceneStateProxy.NAME, Facade.Instance.RetrieveProxy( SceneStateProxy.NAME ) as SceneStateProxy );

            // 2019-03-26 14:36:00 Shell Lee
            // 场景编辑扩展为按下平移、旋转按钮，禁用场景操作；松开按钮，启用场景操作
            SceneStateProxy sceneStateProxy = Facade.Instance.RetrieveProxy( SceneStateProxy.NAME ) as SceneStateProxy;
            if ( null != sceneStateProxy )
            {
                inputStateManager.add( sceneStateProxy, false );
            }
#endif

            DefaultCameraMode = crossPlatformManipulator;


            SwitchTo(DefaultCameraMode);
        }

        public void SwitchTo( ENCameraModeType mode, bool force = false )
        {
            if (mCameraModeMaps.ContainsKey(mode))
            {
                SwitchTo(mCameraModeMaps[mode], force);
            }
            else
            {
                Debug.Log("InputStateManager: Skipped! The InputState to switch to is not recorded");
            }
        }

        /// <summary>
        /// 切换摄像机状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cameraModeType"></param>
        /// <returns></returns>
        private IInputState SwitchTo( ENCameraModeType cameraModeType )
        {
            IInputState mode = GetInputState(cameraModeType);
            SwitchTo(mode);
            return mode;
        }


        public void SwitchTo( IInputState manipulator, bool force = false )
        {
            if (!force && (null == manipulator || m_currentCameraMode == manipulator))
            {
                return;
            }


            if (null == m_currentCameraMode)
            {
                manipulator.Enter();
                m_currentCameraMode = manipulator;

                return;
            }

            if (m_currentCameraMode.CouldLeave() && manipulator.CouldEnter())
            {
                if (manipulator.CameraMode != ENCameraModeType.PlayerManipulator)
                {
                    SetMainTarget(null);
                }

                //取消模式专属缓动动画/动态效果
                m_currentCameraMode.Leave();
                manipulator.Enter();
                m_currentCameraMode = manipulator;
            }
        }

        ///// <summary>
        ///// 获取某种类型的操作器，如果有多个，返回第一个查找到的。（大多操作器都仅有一个实例）
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //public T GetInputState<T>( ENCameraModeType type ) where T : class, IInputState
        //{
        //    if (mCameraModeMaps.TryGetValue(type, out var mode))
        //    {
        //        return mode as T;
        //    }

        //    return null;
        //}

        /// <summary>
        /// 获取某种类型的操作器，如果有多个，返回第一个查找到的。（大多操作器都仅有一个实例）
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IInputState GetInputState( ENCameraModeType type )
        {
            if (mCameraModeMaps.TryGetValue(type, out var mode))
            {
                return mode;
            }

            return null;
        }

        #endregion

        #region 消息注册

        //private void OnEnable()
        //{

        //}


        //private void OnDisable()
        //{

        //}

        #endregion

        #region Internal Members

        /// <summary>
        /// 切换摄像机模式
        /// </summary>
        /// <param name="camerMode"></param>
        public void SwitchCameraMode( ENCameraModeType camerMode )
        {
            SwitchTo(camerMode);
        }

        /// <summary>
        /// 默认相机的模式
        /// </summary>
        public void DefualtMode()
        {
            SwitchTo(DefaultCameraMode);
        }


        /// <summary>
        /// 聚焦模式
        /// </summary>
        /// <param name="target">聚焦对象</param>
        /// <param name="callback"></param>
        /// <param name="duration"></param>
        public void FocusMode( GameObject target, UnityAction callback, float duration = 2f )
        {
            Facade.Instance.SendNotification((ushort) EnGlobalEvent.ChangeInputMode, EnInputMode.Lock);
            FocusModelManipulator focusMode = SwitchTo(ENCameraModeType.FocusModelManipulator) as FocusModelManipulator;
            focusMode?.FocusModel(target, callback, duration);
        }

        public void ResetView()
        {
            FocusPosAndRotation(m_homePosition, m_homeRotation);
        }

        /// <summary>
        /// 聚焦到区域
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="quaternion"></param>
        public void FocusPosAndRotation( Vector3 pos, Quaternion quaternion )
        {
            if (m_currentCameraMode != DefaultCameraMode)
            {
                SwitchTo(m_defaultCameraMode);
            }

            ViewpointTranslationManipulator mode =
                SwitchTo(ENCameraModeType.ViewpointTranslationManipulator) as ViewpointTranslationManipulator;
            if (mode != null)
                mode.TranslateViewpoint(pos
                    , quaternion
                    , () => { SwitchTo(DefaultCameraMode); }
                    , () => { SwitchTo(DefaultCameraMode); });
        }


        public void FPVMode()
        {
            Facade.Instance.SendNotification((ushort) EnGlobalEvent.ChangeInputMode, EnInputMode.Free);
            //SwitchTo(ENCameraModeType.CrossPlatformManipulator);
        }


        /// <summary>
        /// 网络同步跟随屏幕，传null取消跟随
        /// </summary>
        /// <param name="followScreen"></param>
        public void FollowGamePlayer( IFollowScreen followScreen )
        {
            if (null == followScreen)
            {
                SwitchTo(m_defaultCameraMode);
                return;
            }

            FollowScreenManipulator mode =
                GetInputState(ENCameraModeType.FollowScreenManipulator) as FollowScreenManipulator;
            ;
            if (mode)
            {
                Facade.Instance.SendNotification((ushort) EnGlobalEvent.ChangeInputMode, EnInputMode.Lock);
                mode.Follow(followScreen);
                SwitchTo(mode);
            }
        }

        /// <summary>
        /// 摄像机绑定制定玩家
        /// </summary>
        /// <param name="player"></param>
        public void SetPlayer( Transform player )
        {
            if (target != player)
            {
                SetMainTarget(player);

                if (target != null)
                {
                    Facade.Instance.SendNotification((ushort) EnGlobalEvent.ChangeInputMode, EnInputMode.Control);
                    SwitchTo(ENCameraModeType.PlayerManipulator, true);
                    TranslationToPlayer(player);
                }
            }
        }

        public new void SetTarget( Transform player )
        {
            if (target != player)
            {
                SetMainTarget(player);

                if (target != null)
                {
                    TranslationToPlayer(player);
                }
            }
        }

        /// <summary>
        /// 重置摄像机跟随人物位置
        /// </summary>
        /// <param name="player"></param>
        public void TranslationToPlayer( Transform player )
        {
            //重置相机位置
            if (target == null)
                return;
            distance = Vector3.Distance(transform.position, currentTarget.position);

            ChangeState("Default", startSmooth);
            currentZoom = currentState.defaultDistance;
            currentHeight = currentState.height;
            currentTargetPos =
                new Vector3(currentTarget.position.x, currentTarget.position.y + offSetPlayerPivot,
                    currentTarget.position.z) + Vector3.up * lerpState.height;
            targetLookAt.position = currentTargetPos;
            targetLookAt.rotation = transform.rotation;
            //targetLookAt.hideFlags = HideFlags.HideInHierarchy;
            if (startUsingTargetRotation)
            {
                //mouseY = currentTarget.root.eulerAngles.x;
                //mouseX = currentTarget.root.eulerAngles.y;
                MouseY = currentTarget.eulerAngles.x;
                mouseX = currentTarget.eulerAngles.y;
            }
            else
            {
                //mouseY = transform.root.eulerAngles.x;
                //mouseX = transform.root.eulerAngles.y;

                MouseY = transform.eulerAngles.x;
                mouseX = transform.eulerAngles.y;
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// The object has been enabled.
        /// </summary>
        private void OnEnable()
        {
            // The object may have been enabled outside of the scene unloading.
            if (_instance == null)
            {
                _instance = this;
                SceneManager.sceneUnloaded -= SceneUnloaded;
            }

            Facade.Instance.RegisterObservers(this, (ushort) EnGlobalEvent.OnChangeInputMode, HandleNotification);
        }

        /// <summary>
        /// Reset the initialized variable when the scene is no longer loaded.
        /// </summary>
        /// <param name="scene">The scene that was unloaded.</param>
        private void SceneUnloaded( UnityEngine.SceneManagement.Scene scene )
        {
            _instance = null;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }

        public void HandleNotification( INotification notification )
        {
            switch (notification.Id)
            {
                case (ushort) EnGlobalEvent.OnChangeInputMode:
                    OnChangeInputMode(notification);
                    break;
            }
        }

        private void OnChangeInputMode( INotification notificationBody )
        {
            switch ((EnInputMode) notificationBody.Body)
            {
                case EnInputMode.Editor:
                    SwitchTo(DefaultCameraMode);
                    break;
            }
        }

        /// <summary>
        /// The object has been disabled.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.sceneUnloaded += SceneUnloaded;
            Facade.Instance?.RemoveObservers(this, (ushort) EnGlobalEvent.OnChangeInputMode);
        }


        private void OnDestroy()
        {
            _instance = null;
        }

        #endregion

        #region 移动实现

        public void MoveCamera( Vector3 fromScreenCoords, Vector3 toScreenCoords, bool convertDir = false,
            bool ignoreUI = false )
        {
            if (_camera != null)
            {
                float camDist = EstimateDistanceToLevel(EEstimateDistanceMode.AVERAGE);

                Vector3 camMove;
                Vector3 dir = toScreenCoords - fromScreenCoords;
                //本地坐标转世界坐标
                dir = transform.TransformDirection(dir);
                if (!convertDir)
                {
                    dir.y = 0;
                }

                if (_camera.orthographic)
                {
                    float forwardDir = Vector3.Dot(dir, _camera.transform.forward);
                    dir -= _camera.transform.forward * forwardDir;
                    _camera.orthographicSize =
                        Mathf.Clamp(_camera.orthographicSize - forwardDir * (_camera.orthographicSize / Screen.width),
                            5, 1000);
                    camMove = dir * (_camera.orthographicSize / Screen.width);
                }
                else
                {
                    camMove = dir * (camDist / Screen.width);
                }

                _camera.transform.position += camMove;
            }
        }

        public void RotateCamera( Vector3 fromScreenCoords, Vector3 toScreenCoords, bool ignoreUI = false )
        {
        }

        public void RotateCameraAroundPivot( Vector3 fromScreenCoords, Vector3 toScreenCoords, bool ignoreUI = false )
        {
        }

        #endregion
    }
}