using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Data;
using Framework.SsitInput;
using SsitEngine.DebugLog;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Unity.Avatar;
using SsitEngine.Unity.SceneObject;
using SsitEngine.Unity.SsitInput;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using InputManager = Framework.SsitInput.InputManager;
using MessageInfo = SSIT.proto.MessageInfo;

namespace Framework.SceneObject
{
    /// <summary>
    /// 车辆
    /// </summary>
    public class VehicleInstance : BaseSceneInstance, ITriggle
    {
        public enum VehicleState
        {
            VS_Move,
            VS_Stay
        }

        private const float c_sTime = 1f;

        [SerializeField] [Tooltip("出生移动目标点")] private Transform BornMovePoint;

        public float getoffTime = 1.5f;

        private bool m_DoorIsOpen = false;
        private bool m_IsGetOff;

        private NavAgentProxy mAgentProxy;

        private HashSet<BaseSceneInstance> mCachePlayer;

        private Vector3 mInitPos;
        private float mRealSpeed;

        private VehicleState mState = VehicleState.VS_Stay;
        //public CarWheel[] mWheels;


        public NavMeshAgent nav;
        private UnityAction OnEndMoveCallBack;
        public float startTime = 1.0f;


        private Vector3 targetPos;

        [field: SerializeField]
        [field: Tooltip("人物出生点")]
        public Transform BornPoint { get; }


        public bool PlayerISGetOff { get; set; }

        public new Vehicle LinkObject => m_baseObject as Vehicle;

        public bool IsLeave { get; set; }

        #region 同步

        public void SetAuthiroty( bool value )
        {
            if (SyncTrans != null)
            {
                SyncTrans.hasAuthority = value;
                SyncTrans.BaseObject = m_baseObject;
            }
        }

        #endregion

        public override void OnUpdate()
        {
            if (!HasAuthority)
                return;

            if (!nav.enabled || mState != VehicleState.VS_Move)
                return;

            if (!IsMoving())
            {
                StopMove();
            }
            else
            {
                var prohjectDir = Vector3.Project(nav.desiredVelocity, transform.forward);
                //Debug.Log(transform.position+"|"+nav.velocity);
                //距离比拟成速度
                mRealSpeed = prohjectDir.magnitude;
                //nav.speed = speed;
                //FindAngle();
                /*for (var i = 0; i < mWheels.Length; i++)
                {
                    mWheels[i].DoRotate(mRealSpeed * 10);
                }*/
            }

            //更新导航代理
            mAgentProxy.OnUpdate();
        }

        protected void LateUpdate()
        {
            if (m_baseObject == null || m_baseObject.LoadStatu != EnLoadStatu.Inited)
                return;

            LinkObject?.Hud?.UpdateHUDElement(LinkObject.Hud);
        }

        private void ClickMove( InputEventArgs msgR )
        {
            if (IsLeave) return;

            if (msgR != null && msgR.type == EnMouseEventType.RightDown)
            {
                if (msgR.isGroud)
                {
                    targetPos = msgR.groud;
                    targetPos.y = transform.position.y;
                }
                else
                {
                    targetPos = msgR.point;
                    targetPos.y = transform.position.y;
                }

                StopCoroutine("StartMove");
                StartCoroutine("StartMove");
            }
        }

        public bool IsMoving()
        {
            if (!nav.enabled)
                return false;
            var r = nav.pathPending || nav.remainingDistance > nav.stoppingDistance || nav.velocity != Vector3.zero;
            return r;
        }

        private void StopMove()
        {
            mState = VehicleState.VS_Stay;
            StopCoroutine("StartMove");
            nav.enabled = false;
            mAgentProxy.OnNavHandle(false);
            if (IsLeave)
            {
                //Debug.Log( "ambulance is arrive" );
                PlayerISGetOff = false;
                IsLeave = false;
            }

            mAgentProxy.ClearTarget();
            OnEndMoveCallBack = null;
        }


        /// <summary>
        /// 下车（函数名配表）
        /// </summary>
        public void GetOffVehicle()
        {
            //TODO:出现人物
            if (!PlayerISGetOff && !IsLeave)
            {
                PlayerISGetOff = true;
                StartCoroutine(GetOff());
            }
        }

        /// <summary>
        /// 上车
        /// </summary>
        public void GetOnVehicle()
        {
            //TODO:出现人物
            if (PlayerISGetOff && !m_IsGetOff)
            {
                PlayerISGetOff = false;
                StartCoroutine(GetOn());
            }
        }

        #region Main Method

        public VehicleAttribute UnitInfo => LinkObject.GetAttribute() as VehicleAttribute;

        private void Start()
        {
            mAgentProxy = GetComponent<NavAgentProxy>();
            mInitPos = transform.position;
        }


        public override void Init( string guid = null )
        {
            base.Init(guid);
            mCachePlayer = new HashSet<BaseSceneInstance>();
            SyncTrans = GetComponent<ScriptSyncTrans>();
        }

        public override bool HasAuthority => !GlobalManager.Instance.IsSync || SyncTrans.hasAuthority;

        public override void OnSelected( bool isIndicator = false )
        {
            base.OnSelected(isIndicator);
        }

        public override void OnUnSelected( bool isIndicator = false )
        {
            base.OnUnSelected(isIndicator);
        }


        public override void OnChangePropertyCallback( EnPropertyId propertyId, string property, object data = null )
        {
            base.OnChangePropertyCallback(propertyId, property, data);
            switch (propertyId)
            {
                case EnPropertyId.OnSwitch:
                {
                    var state = (En_SwitchState) property.ParseByDefault(0);
                    switch (state)
                    {
                        case En_SwitchState.Show:
                            GetOffVehicle();
                            break;
                        case En_SwitchState.Hide:
                            GetOnVehicle();

                            break;
                    }
                }
                    break;
            }
        }

        #endregion


        #region 触发

        public TriggleType mTriggleType = TriggleType.CharactorDis_Triggle;

        public TriggleType TriggleType => mTriggleType;

        public ScriptSyncTrans SyncTrans { get; set; }

        public bool Check( BaseSceneInstance baseObj )
        {
            //todo:判断是否是担架工
            var character = baseObj as PlayerInstance;
            //判断当前人是此车的固属成员
            if (character != null && character.HasAuthority && character.LinkObject.GetParent() == LinkObject)
                return true;

            return false;
        }

        public void Enter( BaseSceneInstance sceneObj )
        {
            if (mCachePlayer.Add(sceneObj))
            {
                var playerInstance = sceneObj as PlayerInstance;

                var player = playerInstance?.LinkObject;
                if (player != null)
                {
                    var state = playerInstance.LinkObject.State;
                    switch (state)
                    {
                        case EN_CharacterActionState.EN_CHA_Assign:
                        {
                            // change param
                            var param = StringUtils.JointStringByFormat(((int) InvUseNodeType.InvAssignNode).ToString(),
                                string.Empty);
                            sceneObj.LinkObject.ChangeProperty(this, EnPropertyId.Interaction, param);

                            param = StringUtils.JointStringByFormat(
                                ((int) InvUseNodeType.InvStrechPatientNode).ToString(), string.Empty);
                            sceneObj.LinkObject.ChangeProperty(this, EnPropertyId.Interaction, param);
                            // change state
                            player.ChangeProperty(this, EnPropertyId.State,
                                ((int) EN_CharacterActionState.EN_CHA_Stay).ToString());

                            // change param
                            param = StringUtils.JointStringByFormat(((int) InvUseNodeType.InvStrechNode).ToString(),
                                string.Empty);
                            sceneObj.LinkObject.ChangeProperty(this, EnPropertyId.Interaction, param);

                            // send message
                            var message0 = new MessageInfo
                            {
                                MessageType = (int) EnMessageType.ACTION,
                                //message0.UserName = "";
                                userDisplayName = player.GetAttribute().Name,
                                messageContent = "搀扶转移伤员成功"
                            };
                            //PopTipHelper.PopTipToMirror(message0);
                        }
                            break;
                        case EN_CharacterActionState.EN_CHA_Strecher:
                        {
                            // change param
                            var param = StringUtils.JointStringByFormat(((int) InvUseNodeType.InvAssignNode).ToString(),
                                string.Empty);
                            sceneObj.LinkObject.ChangeProperty(this, EnPropertyId.Interaction, param);

                            param = StringUtils.JointStringByFormat(
                                ((int) InvUseNodeType.InvStrechPatientNode).ToString(), string.Empty);
                            sceneObj.LinkObject.ChangeProperty(this, EnPropertyId.Interaction, param);

                            // change state
                            player.ChangeProperty(this, EnPropertyId.State,
                                ((int) EN_CharacterActionState.EN_CHA_Stay).ToString());

                            // change param
                            param = StringUtils.JointStringByFormat(((int) InvUseNodeType.InvStrechNode).ToString(),
                                string.Empty);
                            sceneObj.LinkObject.ChangeProperty(this, EnPropertyId.Interaction, param);

                            // send message
                            var message0 = new MessageInfo
                            {
                                MessageType = (int) EnMessageType.ACTION,
                                //message0.UserName = "";
                                userDisplayName = player.GetAttribute().Name,
                                messageContent = "使用担架转移伤员成功"
                            };
                            //PopTipHelper.PopTipToMirror(message0);
                        }

                            break;
                    }

                    if (IsLeave)
                    {
                        player.PlayerController.ClearTarget();
                        player.SetVisible(false);
                    }
                }
            }
        }

        public void Stay( BaseSceneInstance sceneObj )
        {
        }

        public void Exit( BaseSceneInstance sceneObj )
        {
            mCachePlayer.Remove(sceneObj);
        }

        public BaseSceneInstance OnPostTriggle( Vector3 point )
        {
            return this;
        }

        #endregion

        #region Internal Members

        private IEnumerator StartMove()
        {
            mAgentProxy.OnNavHandle(true);
            if (!nav.enabled)
            {
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
                yield return new WaitForEndOfFrame();
            }

            mAgentProxy.SetMovePostion(targetPos, null, true, false, null, OnEndMoveCallBack, OnEndMoveCallBack);
            mState = VehicleState.VS_Move;
        }

        //下车
        private IEnumerator GetOff()
        {
            m_IsGetOff = true;

            //if (UnitInfo.m_state == En_SwitchState.Off)
            {
                LinkObject.ChangeProperty(this, EnPropertyId.OnSwitch, ((int) En_SwitchState.On).ToString());
                IsLeave = false;
                yield return new WaitForSeconds(c_sTime);
            }

            //Show Npc
            var playerList = LinkObject.GetPlayers();
            if (playerList.Count > BornMovePoint.childCount)
            {
                SsitDebug.Error("人员下车点与预设不匹配");
                yield return null;
            }

            var index = 0;
            for (var i = 0; i < playerList.Count; i++)
            {
                var player = playerList[i];
                if (player == null)
                    continue;

                var attach = player.SceneInstance;

                attach.transform.position = BornPoint.transform.position;
                var tarPos = BornMovePoint.GetChild(index).position;
                attach.transform.forward = (tarPos - BornPoint.transform.position).normalized;
                attach.transform.localEulerAngles = new Vector3(0, attach.transform.localEulerAngles.y, 0);
                player.ChangeProperty(this, EnPropertyId.Position, attach.transform.position.ToString());
                player.ChangeProperty(this, EnPropertyId.Rotate, attach.transform.rotation.ToString());
                yield return new WaitForSeconds(0.5f);

                player.SetVisible(true, true);
                if (attach is PlayerInstance)
                {
                    player.PlayerController.LockInput = false;
                    player.PlayerController.SetTargetPosition(tarPos, false);
                }
                else if (attach is VehicleInstance)
                {
                    var vehicleInstance = attach as VehicleInstance;
                    vehicleInstance.Move(tarPos, true);
                }

                index++;
                yield return new WaitForSeconds(getoffTime);
            }


            //显示UI面板成员列表信息
            RefreshUiInfo();
            m_IsGetOff = false;
        }

        //上车/返回医院
        private IEnumerator GetOn()
        {
            IsLeave = true;
            //if (UnitInfo.m_state == En_SwitchState.Off)
            //{
            //    LinkObject.ChangeProperty(this, EnPropertyId.OnSwitch, ((int)En_SwitchState.On).ToString());
            //    IsLeave = false;
            //    yield return new WaitForSeconds(c_sTime);
            //}

            var players = LinkObject.GetPlayers();

            //检测任务执行状态
            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                var playerInstance = player?.SceneInstance as PlayerInstance;
                if (playerInstance && playerInstance.EnableObject)
                    if (players[i].State != EN_CharacterActionState.EN_CHA_Stay)
                    {
                        IsLeave = false;
                        var messageInfo = new Data.MessageInfo
                        {
                            MessageType = EnMessageType.SYSTEM,
                            //message0.UserName = "";
                            UserDisplayName = player.GetAttribute().Name,
                            MessageContent = "救护成员正在救护过程，请等待救护完成"
                        };
                        //      PopTipHelper.PopTip(messageInfo);
                        yield break;
                    }
            }

            //导航节点
            for (var i = 0; i < players.Count; i++)
                if (null != players[i])
                {
                    var i1 = i;
                    players[i].PlayerController.SetTargetPosition(BornPoint.position, false, null, () =>
                    {
                        players[i1].PlayerController.mNavAgentProxy.ClearTarget();
                        players[i1].SetVisible(false);
                    });
                }

            var isLeave = true;
            while (isLeave)
            {
                isLeave = false;
                for (var i = 0; i < players.Count; i++)
                    if (players[i].GetRepresent().activeSelf)
                        isLeave = true;
                yield return new WaitForEndOfFrame();
            }

            LinkObject.ChangeProperty(this, EnPropertyId.OnSwitch, ((int) En_SwitchState.Off).ToString());
            yield return new WaitForSeconds(c_sTime);

            //刷新右侧面板
            RefreshUiInfo(true);
            //走你
            Move(mInitPos, true, () => IsLeave = false);
        }

        private void Move( Vector3 pos, bool isForce, UnityAction OnEndMoveCallBack = null )
        {
            if (IsLeave && m_IsGetOff && !isForce) return;

            var tp = pos;
            targetPos.y = transform.position.y;
            targetPos = tp;
            this.OnEndMoveCallBack = OnEndMoveCallBack;
            //if (!nav.enabled)
            //{
            //    StopCoroutine( "StartMove" );
            //}
            StartCoroutine("StartMove");
        }

        private void RefreshUiInfo( bool isRemove = false )
        {
            /*var memberInfos = new List<SelectedMemberInfo>();

            var players = LinkObject.GetPlayers();

            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];

                var instance = player.SceneInstance;
                if (instance is PlayerInstance tt)
                    memberInfos.Add(new SelectedMemberInfo
                    {
                        ID = player.Guid,
                        iconId = player.GetAttribute().Icon,
                        name = player.GetAttribute().Name,
                        isReMove = isRemove
                    });
                else if (instance is VehicleInstance vehicleInstance)
                    memberInfos.Add(new SelectedMemberInfo
                    {
                        ID = vehicleInstance.Guid,
                        //npcInfo = vehicle.InfoData,
                        iconId = vehicleInstance.UnitInfo.Icon,
                        name = vehicleInstance.UnitInfo.Name,
                        isReMove = isRemove
                    });
            }*/

            //Facade.Instance.SendNotification((ushort)UIOperatorFormEvent.InitOperators, memberInfos);
        }

        #endregion

        #region 回放

        public override void SynchronizeProperties( SavedBase savedState, bool isReset, bool isFristFrame )
        {
            //base.SynchronizeProperties(savedState, isReset);
            gameObject.SetActive(savedState.isActive);

            var attribute = savedState as VehicleAttribute;
            if (attribute == null)
                return;

            if (isFristFrame)
                UnitInfo.ResetBaseTrack(attribute.mBaseTrack);

            SynchronizeState(UnitInfo, attribute, isReset, isFristFrame);

            SynchronizeMotor(attribute);
        }

        private void SynchronizeMotor( VehicleAttribute attribute )
        {
            if (!SyncTrans)
                return;
            SyncTrans.RecieveResults(attribute.position, attribute.rotation, 0);
        }

        protected virtual void SynchronizeState( VehicleAttribute curState, VehicleAttribute savedState, bool isReset,
            bool isFristFrame )
        {
            if (curState.m_state == savedState.m_state)
                return;

            curState.m_state = savedState.m_state;
            switch (savedState.m_state)
            {
                case En_SwitchState.On:
                {
                    LinkObject.ChangeProperty(this, EnPropertyId.OnSwitch, ((int) En_SwitchState.On).ToString());
                }
                    break;
                case En_SwitchState.Off:
                {
                    LinkObject.ChangeProperty(this, EnPropertyId.OnSwitch, ((int) En_SwitchState.Off).ToString());
                }
                    break;
                case En_SwitchState.Show:
                {
                    //LinkObject.OnChangeProperty(LinkObject, EnPropertyId.OnSwitch, ((int)En_SwitchState.Working).ToString(),LinkObject.GetParent());
                }
                    break;
                case En_SwitchState.Hide:
                {
                    //LinkObject.OnChangeProperty(LinkObject, EnPropertyId.OnSwitch, ((int)En_SwitchState.Hide).ToString(),LinkObject.GetParent());
                }
                    break;
            }
        }

        #endregion

        #region 弃用

        /// <summary>
        /// 自动导航离开医院（函数名配表）
        /// </summary>
        public void ReturnHospital()
        {
            //Facade.Instance.SendNotification( ConstNotification.c_sShowMemberList, false );
            if (IsLeave)
                return;

            Facade.Instance.SendNotification((ushort) EnGlobalEvent.ChangeInputMode, EnInputMode.Free);
            StartCoroutine(LeaveScene());
            InputManager.Instance.EventOnRightClick.RemoveListener(ClickMove);
        }

        private IEnumerator LeaveScene()
        {
            //SceneInfoProxy sceneProxy = (SceneInfoProxy)Facade.Instance.RetrieveProxy( SceneInfoProxy.NAME );

            //List<FrameworkNetworkPlayer> mNpcList = sceneProxy.GetNetworkPlayers( player => player.OwnerUid == ObjectGUID );

            /*if (!DoorIsOpen)
            {
                yield break;
            }*/
            IsLeave = true;

            var isLeave = true;
            //List<BaseSceneInstance> mNpcList = mAttachObject.ToList();
            while (isLeave)
            {
                isLeave = false;
                //for (int i = 0; i < mNpcList.Count; i++)
                //{
                //    if (mNpcList[i].EnableObject)
                //    {
                //        PlayerInstance player = mNpcList[i] as PlayerInstance;
                //        if (player)
                //        {
                //            PlayerAttribute attribute = player.PlayerAttribute;
                //            if (attribute.mProcessState == ENProcessState.EPS_Stay || attribute.mProcessState == ENProcessState.EPS_Finished)
                //            {
                //                attribute.mProcessState = ENProcessState.EPS_ForceProcessing;

                //                if (mCachePlayer.Contains(player))
                //                {
                //                    //player.thirdPersonManipulator.StopMove();
                //                    attribute.mProcessState = ENProcessState.EPS_Stay;
                //                    player.EnableObject = false;
                //                }
                //                else
                //                {
                //                    //player.thirdPersonManipulator.SetTargetPosition(bornPoint.position, null, null);
                //                }
                //            }
                //        }
                //        else
                //        {
                //            VehicleInstance vehicleInstance = mNpcList[i] as VehicleInstance;
                //            if (vehicleInstance)
                //            {
                //                if (mCachePlayer.Contains(vehicleInstance))
                //                {
                //                    vehicleInstance.EnableObject = false;
                //                }
                //                else
                //                {
                //                    vehicleInstance.Move(bornPoint.position, true);
                //                }
                //            }
                //        }
                //        isLeave = true;
                //    }
                //}
                yield return new WaitForEndOfFrame();
            }
            //if (DoorIsOpen)
            //{
            //    DoorIsOpen = false;
            //    yield return new WaitForSeconds(c_sTime);
            //}
            //MainProcess.inputStateManager.switchTo(MainProcess.inputStateManager.defaultInputState);

            //刷新右侧面板
            RefreshUiInfo(true);
            //走你

            Move(mInitPos, true, () => IsLeave = false);

            //Debug.Log( "Reach the target" );
        }

        /// <summary>
        /// 弃用
        /// </summary>
        [Obsolete]
        public void UnfoldStretcher()
        {
            //m_FoldStretcher.SetActive( false );
            //m_UnfoldStretcher.SetActive( true );
        }

        /// <summary>
        /// 弃用
        /// </summary>
        [Obsolete]
        public void FoldStretcher()
        {
            //m_FoldStretcher.SetActive( true );
            //m_UnfoldStretcher.SetActive( false );
        }

        /// <summary>
        /// 角速度设定
        /// </summary>
        /// <returns></returns>
        private float FindAngle()
        {
            var dir = nav.desiredVelocity;
            Debug.DrawRay(transform.position, dir, Color.green);
            var normal = Vector3.Cross(transform.forward, dir);
            var angle = Vector3.Angle(transform.forward, dir);
            //if (normal.y < 0) {
            //    angle=(-1) * angle;
            //}
            //角度的转换（通过叉乘判断方向-/+）
            angle *= normal.y / Mathf.Abs(normal.y);
            //角度转弧度
            angle *= Mathf.Deg2Rad;

            if (dir == Vector3.zero)
            {
                angle = 0;
                nav.acceleration = 8F;
            }
            else
            {
                nav.acceleration = -8F;
            }
            //if (player != null)
            //{
            //    EN_PlayerState state = player.State;
            //    switch (state)
            //    {
            //        case EN_PlayerState.EPS_Assign:
            //            player.AssignPatientFinished();
            //            MessageInfo message0 = new MessageInfo();
            //            message0.MessageType = EnMessageType.ACTION;
            //            //message0.UserName = "";
            //            message0.UserDisplayName = player.BaseData.ItemName;
            //            message0.MessageContent = "搀扶转移伤员成功";
            //            Facade.Instance.SendNotification( ConstNotification.c_sSyncPopTips, message0 );
            //            break;
            //        case EN_PlayerState.EPS_Strechering:
            //            player.StrecherPatientFinished();
            //            MessageInfo message1 = new MessageInfo();
            //            message1.MessageType = EnMessageType.ACTION;
            //            //message1.UserName = "";
            //            message1.UserDisplayName = player.BaseData.ItemName;
            //            message1.MessageContent = "使用担架转移伤员成功";
            //            Facade.Instance.SendNotification( ConstNotification.c_sSyncPopTips, message1 );

            //            break;
            //    }
            //    if (IsLeave)
            //    {
            //        player.thirdPersonManipulator.StopMove();
            //        ((NpcInfo)player.InfoData).mProcessState = ENProcessState.EPS_Stay;
            //        player.EnableObject = false;
            //    }
            return 1;
        }

        //public virtual void OnDetect() {}

        //public virtual void OffDetect() { }

        //private GameObject m_UnfoldStretcher;
        //private GameObject m_FoldStretcher;
        //private Transform m_UnfoldStretcherTransform;

        //[HideInInspector] public EventMouseOnClickPosition OnClickMove = null;
        //[HideInInspector] public InfoEventToggle EventOnSkillSelected = null;
        //[HideInInspector] public InfoEvent EventOnItemSelected = null;

        #endregion

        #region Test

        [ContextMenu("TestGetOn")]
        private void TestGetOn()
        {
            GetOnVehicle();
        }

        [ContextMenu("TestGetOff")]
        private void TestGetOff()
        {
            GetOffVehicle();
        }

        #endregion
    }
}