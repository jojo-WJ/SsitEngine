using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.Data;
using Framework.SceneObject;
using Invector.CharacterController;
using RootMotion.FinalIK;
using SsitEngine.Unity.Avatar;
using SsitEngine.Unity.HUD;
using Framework.SceneObject;
using SsitEngine.Unity.SceneObject;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class OnEnableCursor : UnityEvent<Vector3>
{
}


public class ThirdPersonController : vThirdPersonController
{
    public delegate void InputStateChangeHandle( ThirdPersonController playerController, EN_InputState preState,
        EN_InputState curState );

    private bool hasInputAuthority = false;
    public bool lockInput;


    private Vector2 m_input = Vector2.zero;
    public EN_InputState mInputState = EN_InputState.EPS_NoneInput;
    public MoveSyncAnimHandle OmMoveSyncAnimHandle = new MoveSyncAnimHandle();
    public UnityEvent onDisableCursor = new UnityEvent();
    public OnEnableCursor onEnableCursor = new OnEnableCursor();


    public UnityEvent OnLateUpdate = new UnityEvent();

    private int syncDataIndex = -1;

    //private float deadZone = 15;
    protected bool updateIK;

    public EN_InputState InputState
    {
        get => mInputState;

        set
        {
            if (mInputState != value)
                if (OnInputStateChange != null)
                    OnInputStateChange(this, mInputState, value);
            mInputState = value;
        }
    }

    public InteractionSystem IkSystem { get; set; }

    public Vector3 MoveDelta { get; set; } = Vector3.zero;

    public bool IsCameraDragable { get; set; } = true;

    public bool LockInput
    {
        get => lockInput;
        set => lockInput = value;
    }

    public event InputStateChangeHandle OnInputStateChange;


    public void OnInputAttach( bool state )
    {
        if (mOnInputAttach != null) mOnInputAttach(state);
    }

    private void OnInputStateChangeCallBack( ThirdPersonController pp, EN_InputState prestate, EN_InputState curstate )
    {
        switch (curstate)
        {
            case EN_InputState.EPS_NoneInput:
                if (prestate == EN_InputState.EPS_NavInput)
                {
                    //停止导航操作
                    StopInput();
                    mNavAgentProxy.OnNavHandle(false);
                }
                break;
            case EN_InputState.EPS_NavInput:
                SetRotateByWorld(true);
                mNavAgentProxy.OnNavHandle(true);
                break;
            case EN_InputState.EPS_KeyInput:
                SetRotateByWorld(false);
                if (prestate == EN_InputState.EPS_NavInput)
                {
                    //停止导航操作
                    StopInput(true);
                    mNavAgentProxy.OnNavHandle(false);
                }
                break;
        }
    }

    #region Internal 

    protected float FindAngle( Vector3 dir )
    {
        var normal = Vector3.Cross(transform.forward, dir);
        var angle = Vector3.Angle(transform.forward, dir);
        //if (normal.y < 0) {
        //    angle=(-1) * angle;
        //}
        //角度的转换（通过叉乘判断方向-/+）
        angle *= normal.y / Mathf.Abs(normal.y);
        //角度转弧度
        //angle *= Mathf.Deg2Rad;

        if (dir == Vector3.zero) angle = 0;
        return angle;
    }

    #endregion

    #region Component

    //玩家导航器
    public NavAgentProxy mNavAgentProxy;

    //老版实体表现体
    public PlayerInstance player;

    //操作同步辅助组件
    public ScriptSynchronizer syncCom;

    //Ik操作系统

    //底层对象
    private Character m_baseObject;

    public UnityAction<bool> mOnInputAttach;

    #endregion

    #region Main Method

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<PlayerInstance>();
        OnInputStateChange += OnInputStateChangeCallBack;
        InitNavAgent();
        syncCom = GetComponent<ScriptSynchronizer>();
        SetRotateByWorld(true);

        IkSystem = GetComponent<InteractionSystem>();
    }

    protected override void Start()
    {
        // 不要删除这个方法 不能用父类的，父类是单玩家单例，会发生意外 base.Start()[x]
    }


    private void OnEnable()
    {
        if (m_baseObject?.Hud)
            m_baseObject.Hud.IsActive = true;
    }

    private void OnDisable()
    {
        if (m_baseObject?.Hud)
        {
            m_baseObject.Hud.IsActive = false;
            m_baseObject.Hud.SetHUDActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (m_baseObject == null)
            return;

        AirControl();
        updateIK = true;
        mNavAgentProxy?.OnUpdate();
        syncCom?.OnFixedUpdate();
    }

    public void OnUpdate( float elapsed )
    {
        if (lockInput || Time.timeScale == 0)
        {
            input = Vector2.zero;
            speed = 0f;
            animator.SetFloat("InputVertical", 0f, 0.2f, Time.deltaTime);
            animator.SetFloat("InputMagnitude", 0f, 0.2f, Time.deltaTime);
            return;
        }

        if (Math.Abs(input.x) <= float.Epsilon && Math.Abs(input.y) <= float.Epsilon)
        {
            input = Vector2.zero;
            isSprinting = false;
            animator.SetFloat("InputVertical", 0f, 0.2f, Time.deltaTime);
            animator.SetFloat("InputMagnitude", 0f, 0.2f, Time.deltaTime);
        }

        //if (syncCom != null)
        //{
        //    syncCom.OnUpdate();
        //}
        UpdateMotor(); // call ThirdPersonMotor methods
        UpdateAnimator(); // call ThirdPersonAnimator methods
    }

    protected virtual void LateUpdate()
    {
        if (m_baseObject == null || m_baseObject.LoadStatu != EnLoadStatu.Inited)
            return;
        //if (lockInput || Time.timeScale == 0) return;
        if (!updateIK && animator.updateMode == AnimatorUpdateMode.AnimatePhysics) return;

        OnLateUpdate?.Invoke();
        m_baseObject?.Hud?.UpdateHUDElement(m_baseObject.Hud);
        updateIK = false;
    }

    #endregion


    #region 子类重写

    public delegate bool MoveDeltaHandle( Vector3 movedelta );

    public MoveDeltaHandle OnMoveDeltaHandle;


    public void SetLink( bool b, Character obj )
    {
        m_baseObject = obj;
        m_baseObject.AddStateChagneCallBack(OnStateChange);
    }

    private void OnStateChange( Character sender, int prestate, int curstate )
    {
        switch ((EN_CharacterActionState) curstate)
        {
            case EN_CharacterActionState.EN_CHA_Assign:
            {
                var param = StringUtils.JointStringByFormat(((int) InvUseNodeType.InvStrechPatientNode).ToString(),
                    string.Empty);
                m_baseObject.OnChangeProperty(this, EnPropertyId.Interaction, param);

                param = StringUtils.JointStringByFormat(((int) InvUseNodeType.InvStrechNode).ToString(), string.Empty);
                m_baseObject.OnChangeProperty(this, EnPropertyId.Interaction, param);
            }
                break;
            case EN_CharacterActionState.EN_CHA_Strecher:
            {
                var param = StringUtils.JointStringByFormat(((int) InvUseNodeType.InvAssignNode).ToString(),
                    string.Empty);
                m_baseObject.OnChangeProperty(this, EnPropertyId.Interaction, param);
            }
                break;
        }
    }


    protected override void FreeVelocity( float velocity )
    {
        // speed是逐渐变化的
        //SsitDebug.Info( "velocity:" + velocity + "\tspeed:" + speed );

        var _targetVelocity = transform.forward * velocity * speed;
        _targetVelocity.y = _rigidbody.velocity.y;
        MoveDelta = transform.forward * velocity * speed * Time.fixedDeltaTime;
        var isMove = true;

        if (GlobalManager.Instance.IsSync && !syncCom.hasAuthority)
            return;

//        if (GlobalManager.Instance.ReplayMode == ActionMode.PLAY)
//            return;

        if (OnMoveDeltaHandle != null)
            isMove = OnMoveDeltaHandle(MoveDelta);
        if (isMove)
        {
            _rigidbody.velocity = _targetVelocity;
            _rigidbody.AddForce(MoveDelta, ForceMode.VelocityChange);
        }
        else
        {
            _rigidbody.velocity = new Vector3(0, _targetVelocity.y, 0);
        }
    }

    public void FreeMove( Vector3 delta )
    {
        var _targetVelocity = delta;
        _targetVelocity.y = _rigidbody.velocity.y;
        _rigidbody.velocity = _targetVelocity;
        _rigidbody.AddForce(delta, ForceMode.VelocityChange);
    }

    #endregion


    #region locamotionn aciton

    /// <summary>
    /// 操作器移动
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void Move( float x, float y )
    {
        if (mInputState != EN_InputState.EPS_NavInput) MoveCharacter(x, y);
    }

    /// <summary>
    /// 移动操作（仅非业主客户执行）
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void MoveInput( float x, float y )
    {
        input.x = x;
        input.y = y;
        OmMoveSyncAnimHandle.Invoke(x, y);
    }

    public void ClickMove( Vector3 hitPos )
    {
        StopInput();
        mNavAgentProxy.ClearTarget();
        if (onEnableCursor != null) onEnableCursor.Invoke(hitPos);
        navCoroutine = StartCoroutine(StartNavAgentProxy(hitPos, true));
    }

    public override void Jump()
    {
        if (customAction) return;

        // know if has enough stamina to make this action
        var staminaConditions = currentStamina > jumpStamina;
        // conditions to do this action
        var jumpConditions = !isCrouching && isGrounded && !actions && staminaConditions && !isJumping;
        // return if jumpCondigions is false
        if (!jumpConditions) return;
        // trigger jump behaviour
        jumpCounter = jumpTimer;
        isJumping = true;
        // trigger jump animations
        if (input.sqrMagnitude < 0.1f)
            animator.CrossFadeInFixedTime("Jump", 0.1f);
        else
            animator.CrossFadeInFixedTime("JumpMove", .2f);
        // reduce stamina
        ReduceStamina(jumpStamina, false);
        currentStaminaRecoveryDelay = 1f;
    }

    public override void Roll()
    {
        var staminaCondition = currentStamina > rollStamina;
        // can roll even if it's on a quickturn or quickstop animation
        var actionsRoll = !actions || actions && quickStop;
        // general conditions to roll
        var rollConditions = (input != Vector2.zero || speed > 0.25f) && actionsRoll && isGrounded &&
                             staminaCondition && !isJumping;

        if (!rollConditions || isRolling) return;

        animator.SetTrigger("ResetState");
        animator.CrossFadeInFixedTime("Roll", 0.1f);
        ReduceStamina(rollStamina, false);
        currentStaminaRecoveryDelay = 2f;
    }

    public override void Strafe()
    {
        isStrafing = !isStrafing;
    }

    public void Strafe( bool value )
    {
        isStrafing = value;
    }

    public override void Sprint( bool value )
    {
        if (value)
        {
            if (currentStamina > 0 && input.sqrMagnitude > 0.1f)
                if (isGrounded && !isCrouching)
                    isSprinting = !isSprinting;
        }
        else if (currentStamina <= 0 || input.sqrMagnitude < 0.1f || isCrouching || !isGrounded || actions ||
                 isStrafing && !strafeSpeed.walkByDefault && (direction >= 0.5 || direction <= -0.5 || speed <= 0))
        {
            isSprinting = false;
        }
    }

    public void Crouch( bool value )
    {
        isCrouching = value;
    }

    public override void Crouch()
    {
        if (isGrounded && !actions)
        {
            if (isCrouching && CanExitCrouch())
                isCrouching = false;
            else
                isCrouching = true;
        }
    }

    public bool AutoUpdateSprint()
    {
        if (!isSprinting) return true;
        if (currentStamina <= 0 || input.sqrMagnitude < 0.1f || isCrouching || !isGrounded || actions ||
            isStrafing && !strafeSpeed.walkByDefault && (direction >= 0.5 || direction <= -0.5 || speed <= 0))
        {
            isSprinting = false;
            return false;
        }
        return true;
    }

    public void MoveCharacter( float x, float y )
    {
        syncCom?.InternalMoveInput(x, y);
    }

    public void MoveCharacter( Vector3 position, bool rotateToDirection = true )
    {
        var dir = position - transform.position;
        var targetDir = isStrafing ? transform.InverseTransformDirection(dir).normalized : dir.normalized;
        if (rotateToDirection /* && isStrafing*/)
        {
            targetDir.y = 0;
            RotateToDirection(dir);
        }
        MoveCharacter(targetDir.x, targetDir.z);
    }

    public virtual void DirectMoveCharacter( Transform tarPos, bool ignoreLerp = false )
    {
        var rot = Quaternion.LookRotation(tarPos.forward);
        var newRot = new Vector3(transform.eulerAngles.x, rot.eulerAngles.y, transform.eulerAngles.z);

        var targetRot = Quaternion.Euler(newRot);
        var targetPos = new Vector3(tarPos.position.x, transform.position.y, tarPos.position.z);

        if (ignoreLerp)
        {
            transform.rotation = targetRot;
            transform.position = targetPos;
        }
        else
        {
            transform.rotation =
                Quaternion.Lerp(transform.rotation, targetRot, strafeSpeed.rotationSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, targetPos, strafeSpeed.sprintSpeed * Time.deltaTime);
        }
    }

    public void SetRotateByWorld( bool value )
    {
        rotateByWorld = value;
        keepDirection = value;
    }

    public void RotateToDir( Vector3 dir, UnityAction action )
    {
        var rotate = transform.rotation * Quaternion.FromToRotation(transform.forward, dir);

        var angle = new Vector3(0, rotate.eulerAngles.y, 0);

        var tarRot = Quaternion.Euler(angle);

        StartCoroutine(SmoothRotateToDir(tarRot, action));

        //角度的转换（通过叉乘判断方向-/+）
        //Vector3 normal = Vector3.Cross(transform.forward, dir);
        //float angle = Vector3.Angle(transform.forward, dir);
        //angle *= (normal.y / Mathf.Abs(normal.y));

        //var tarEuler = transform.localEulerAngles;
        //向左转
        //if (normal.y > 0)
        //{
        //    tarEuler  += new Vector3(0, angle, 0);
        //    StartCoroutine(SmoothRotateToDir(tarEuler, action));
        //}
        //else if (normal.y < 0)
        //{
        //    tarEuler += new Vector3(0, -angle, 0);
        //    StartCoroutine(SmoothRotateToDir(tarEuler, action));
        //}
        //else
        //{
        //    action.Invoke();
        //    return;
        //}
        //Debug.LogError($"normal {normal.y} ");
    }

    private IEnumerator SmoothRotateToDir( Vector3 dir, UnityAction action )
    {
        //yield return new WaitForEndOfFrame();
        //记录
        var pre = transform.localEulerAngles;

        float t = 0;
        while (t <= 1)
        {
            t += strafeSpeed.rotationSpeed * Time.deltaTime;
            transform.localEulerAngles = Vector3.Lerp(pre, dir, t);
            yield return new WaitForEndOfFrame();
        }
        if (action != null)
            action();
    }

    private IEnumerator SmoothRotateToDir( Quaternion dir, UnityAction action )
    {
        //yield return new WaitForEndOfFrame();
        //记录
        var pre = transform.rotation;

        float t = 0;
        while (t <= 1)
        {
            t += strafeSpeed.rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(pre, dir, t);
            yield return new WaitForEndOfFrame();
        }
        if (action != null)
            action();
    }

    //Self explanatory
    //Can be changed in inherited class
    public virtual void GetInputs( ref EzInputs ezInputs )
    {
        //Don't use one frame events in this part
        //It would be processed incorrectly 
        ezInputs.input = input;
        ezInputs.sprintInput = isSprinting;
        ezInputs.crouchInput = isCrouching;
        ezInputs.jumpInput = isJumping;
        ezInputs.rollInput = isRolling;
        ezInputs.strafeInput = isStrafing;
    }

    public virtual void ApplyInputs( PlayerAttribute attri )
    {
        //Don't use one frame events in this part
        //It would be processed incorrectly 
        var ezInputs = attri.mEzInputs;
        MoveInput(ezInputs.input.x, ezInputs.input.y);

        syncCom.RecieveResults(attri.position, attri.rotation, 0);

        isSprinting = ezInputs.sprintInput;
        isCrouching = ezInputs.crouchInput;
        isStrafing = ezInputs.strafeInput;

        if (isJumping != ezInputs.jumpInput && ezInputs.jumpInput)
            Jump();
        if (isRolling != ezInputs.rollInput && ezInputs.rollInput)
            Roll();
    }

    public bool CanMoveInput()
    {
        if (lockInput)
            return false;
        return true;
    }

    public bool CanJumpInput()
    {
        if (lockInput)
            return false;

        switch (m_baseObject.State)
        {
            case EN_CharacterActionState.EN_CHA_Stay:
            case EN_CharacterActionState.EN_CHA_MHQAttach:
            case EN_CharacterActionState.EN_CHA_MHQReady:
                break;
            default:
                return false;
        }

        return true;
    }

    public void ClearTarget()
    {
        mNavAgentProxy?.ClearTarget();
    }

    public void DisablePhysic( bool p0 )
    {
        if (p0)
        {
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            _rigidbody.isKinematic = true;
        }
        else
        {
            _rigidbody.isKinematic = false;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        //_rigidbody.useGravity = !_rigidbody.isKinematic;
    }

    #endregion

    #region Sync Action

    #endregion

    #region Nav

    private NavMeshPath _navMeshPath;
    private List<Vector3> _corners; // 单独保存一份寻路点数据，防止右键寻路失败而清除了寻路数据，导致人物位置状态错误（人物在[off mesh link]中，就不会走动了，比如爬楼梯过程中）
    private Coroutine navCoroutine;

    private void InitNavAgent()
    {
        _navMeshPath = new NavMeshPath();
        _corners = new List<Vector3>();
        mNavAgentProxy = gameObject.GetComponentInChildren<NavAgentProxy>();
        if (mNavAgentProxy)
        {
            mNavAgentProxy.ProxyObj = player;
            mNavAgentProxy.OnNavProxySteerHandle = OnNavProxySteerHandle;
        }
    }


    public void SetStopDistance( float distance )
    {
        if (mNavAgentProxy) mNavAgentProxy.setStopDistance(distance);
    }


    public void SetTargetPosition( Vector3 value, bool isShowGizmos, UnityAction onInterruptMove = null,
        UnityAction onArriveEnd = null )
    {
        mNavAgentProxy.ClearTarget();
        InputState = EN_InputState.EPS_NavInput;

        StopAllCoroutines();
        navCoroutine = StartCoroutine(StartNavAgentProxy(value, isShowGizmos, onInterruptMove, onArriveEnd));
    }

    public void SetFollowTarget( BaseSceneInstance player, Vector3 followPoint, UnityAction onInterruptFollow = null,
        UnityAction onArriveEnd = null )
    {
        if (mNavAgentProxy)
        {
            mNavAgentProxy.ClearTarget();

            InputState = EN_InputState.EPS_NavInput;

            void OnMoving( Vector3 position )
            {
                MoveCharacter(position);
            }

            void OnMoveEnd()
            {
                if (onDisableCursor != null) onDisableCursor.Invoke();
                InputState = EN_InputState.EPS_NoneInput;
                StopMove();
                if (onArriveEnd != null)
                    onArriveEnd();
            }

            mNavAgentProxy.SetFolowTarget(player, followPoint, OnMoving, onInterruptFollow, OnMoveEnd);
        }
    }

    private IEnumerator StartNavAgentProxy( Vector3 targetPos, bool isShowGizmos, UnityAction onInterruptMove = null,
        UnityAction onArriveEnd = null )
    {
        var distance = Vector3.Distance(targetPos, transform.position);
        if (distance < colliderRadius * 2) yield return null;
        //等待一桢(等待路径更新)
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        //yield return new WaitForEndOfFrame();

        var canNav = false;
        if (NavMesh.CalculatePath(transform.position, targetPos, NavMesh.AllAreas, _navMeshPath) &&
            _navMeshPath.corners.Length > 1)
        {
            _corners.Clear();
            _corners.AddRange(_navMeshPath.corners);
            for (var i = 1; i < _corners.Count - 2; i++)
            {
                NavMeshHit hithit;
                var result = NavMesh.FindClosestEdge(_corners[i], out hithit, NavMesh.AllAreas);
                if (result && hithit.distance < 1.0f)
                    _corners[i] = hithit.position + hithit.normal * 0.3f;
            }
            canNav = true;
        }
        else
        {
            _corners.Clear();
            _corners.Add(transform.position);
            _corners.Add(targetPos);
        }

        if (mNavAgentProxy)
        {
            void OnMoving( Vector3 position )
            {
                MoveCharacter(position);
            }

            void OnMoveEnd()
            {
                if (onDisableCursor != null) onDisableCursor.Invoke();
                StopInput(true);
                if (null != onArriveEnd) onArriveEnd.Invoke();
            }

            mNavAgentProxy.SetMovePostion(targetPos, _corners, canNav, isShowGizmos, OnMoving, onInterruptMove,
                OnMoveEnd);
        }
    }

    public void StopInput( bool instant = false )
    {
        if (InputState == EN_InputState.EPS_KeyInput || instant && InputState == EN_InputState.EPS_NavInput)
        {
            InputState = EN_InputState.EPS_NoneInput;
            MoveCharacter(0, 0);

            mNavAgentProxy.ClearTarget();
            _corners.Clear();
            if (navCoroutine != null)
            {
                StopCoroutine(navCoroutine);
                navCoroutine = null;
            }
        }
    }

    #endregion

    #region 附加检测

    private void OnNavProxySteerHandle( Vector3 targetPos, Vector3 endPos, float angle, float distance,
        float rotatePerSecond, float accuracy, float stopDis )
    {
        if (!isGrounded) return;
        var tartPos = targetPos;

        if (Vector3.SqrMagnitude(transform.position - endPos) <= Mathf.Pow(stopDis, 2))
            //distance = 0.1f;
            return;

        RaycastHit hitinfo;
        if (LookPerSecond(angle, _capsuleCollider.radius + distance, rotatePerSecond, accuracy, out hitinfo,
            Color.cyan))
        {
            //hitinfo.normal
            var hitDir = -hitinfo.normal;

            var dir = Vector3.Dot(transform.forward, hitDir);
            if (dir > 0)
                tartPos = transform.position - transform.right * freeSpeed.walkSpeed * Time.fixedDeltaTime;
            else
                tartPos = transform.position + transform.right * freeSpeed.walkSpeed * Time.fixedDeltaTime;

            transform.position = tartPos;
            return;

            //Debug.LogError($"OnNavProxySteerHandle is check {targetPos} tarPos{tartPos}");
        }
        //Debug.Log($"OnNavProxySteerHandle is check {targetPos} tarPos{tartPos}");


        MoveCharacter(tartPos);
    }


    //射出射线检测是否有Player
    public bool LookAround( Quaternion eulerAnger, float lookRange, out RaycastHit hitinfo, Color debugColor )
    {
        var mask = LayerMask.GetMask(LayerMask.LayerToName(LayerUtils.Default),
            LayerMask.LayerToName(LayerUtils.Character));
        var ray = new Ray(transform.position + new Vector3(0, stopMoveHeight, 0),
            eulerAnger * targetDirection.normalized);

        Debug.DrawLine(ray.origin, ray.origin + eulerAnger * transform.forward.normalized * lookRange, debugColor);

        if (Physics.Raycast(ray, out hitinfo, lookRange, mask)) return true;
        hitinfo = new RaycastHit();
        return false;
    }

    private bool LookPerSecond( float angle, float distance, float rotatePerSecond, float accuracy,
        out RaycastHit hitinfo, Color debugColor )
    {
        var subAngle = angle / accuracy; //每条射线需要检测的角度范围
        for (var i = 0; i < accuracy; i++)
            if (LookAround(
                Quaternion.Euler(0, -angle / 2 + i * subAngle + Mathf.Repeat(rotatePerSecond * Time.time, subAngle), 0),
                distance, out hitinfo, debugColor))
                return true;
        hitinfo = new RaycastHit();
        return false;
    }

    #endregion
}