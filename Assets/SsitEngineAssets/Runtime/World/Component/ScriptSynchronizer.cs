//#define EzReplay
/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/14 18:19:32                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using Framework.SsitInput;
using Mirror;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.DebugLog;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace Framework.SceneObject
{
    //[NetworkSettings(sendInterval=0.05f)]
    public class ScriptSynchronizer : NetworkBehaviour
    {
        private float _dataStep;

        //This struct would be used to collect player inputs
        //[SyncVar(hook = "RecieveInputs")]
        //private SyncInputs syncinputs;
        private Inputs _inputs;

        private float _lastTimeStamp;
        private bool _playData;

        private Results _results;

        //Owner client and server would store it's inputs in this list [效果不好屏蔽 by jojo]
        //private List<Inputs> _inputsList = new List<Inputs>();
        //This list stores results of movement and rotation. Needed for non-owner client interpolation
        private readonly List<Results> _resultsList = new List<Results>();
        private Vector3 _startPosition;
        private Quaternion _startRotation;

        private float _step;

        //Synced from server to all clients
        [SyncVar] public string guid;

        private ThirdPersonController m_playerController;

        [SyncVar(hook = "RecieveResults")] private SyncResults syncResults;

        private void Awake()
        {
            m_playerController = GetComponent<ThirdPersonController>();
            if (!m_playerController) SsitDebug.Error("角色脚本绑定异常");
        }


        private void OnEnable()
        {
            m_playerController.mOnInputAttach += OnInputAttach;
        }

        private void OnDisable()
        {
            m_playerController.mOnInputAttach -= OnInputAttach;
            OnInputAttach(false);
        }

        public void OnInputAttach( bool state )
        {
            if (state)
            {
                //注册移动监听
                Facade.Instance.RegisterObservers(this, (ushort) EnKeyEventType.moveInput, OnMoveInput);
                Facade.Instance.RegisterObservers(this, (ushort) EnKeyEventType.jumpInput, OnJumpInput);
                Facade.Instance.RegisterObservers(this, (ushort) EnKeyEventType.rollInput, OnRollInput);
                Facade.Instance.RegisterObservers(this, (ushort) EnKeyEventType.strafeInput, OnStrafeInput);
                Facade.Instance.RegisterObservers(this, (ushort) EnKeyEventType.sprintInput, OnSprintInput);
                Facade.Instance.RegisterObservers(this, (ushort) EnKeyEventType.crouchInput, OnCrouchInput);
                Facade.Instance.RegisterObservers(this, (ushort) EnMouseEvent.RightDown, OnMouseClickInput);
                //Debug.Log("OnInputAttach true");
            }
            else
            {
                //注销监听
                Facade.Instance.RemoveObservers(this, (ushort) EnKeyEventType.moveInput);
                Facade.Instance.RemoveObservers(this, (ushort) EnKeyEventType.jumpInput);
                Facade.Instance.RemoveObservers(this, (ushort) EnKeyEventType.rollInput);
                Facade.Instance.RemoveObservers(this, (ushort) EnKeyEventType.strafeInput);
                Facade.Instance.RemoveObservers(this, (ushort) EnKeyEventType.sprintInput);
                Facade.Instance.RemoveObservers(this, (ushort) EnKeyEventType.crouchInput);
                Facade.Instance.RemoveObservers(this, (ushort) EnMouseEvent.RightDown);
                //Debug.Log("OnInputAttach false");
            }
        }

        public override void OnStartClient()
        {
            //Debug.Log("ScriptSync NetworkServer.active" + NetworkServer.active + "guid " + guid);
            Facade.Instance.SendNotification((ushort) EnObjectEvent.SpawnPlayer, gameObject, guid);
        }


        public struct SyncInputs
        {
            public Vector2 input;
            public float jumpInput;
            public bool rollInput;
            public bool strafeInput;
            public bool sprintInput;
            public bool crouchInput;

            public float timeStamp;
        }

        //This struct would be used to collect results of Move and Rotate functions
        public struct Results
        {
            public Quaternion rotation;
            public Vector3 position;

            public Vector2 input;

            //public float jumpInput;
            //public bool rollInput;
            //public bool strafeInput;
            //public bool sprintInput;
            //public bool crouchInput;
            public float timeStamp;
        }

        public struct SyncResults
        {
            public Vector3 position;
            public Quaternion rotation;

            public Vector2 input;

            //public float jumpInput;
            //public bool rollInput;
            //public bool strafeInput;
            //public bool sprintInput;
            //public bool crouchInput;
            public float timeStamp;
        }

        public struct Inputs
        {
            public Vector2 input;
            public bool jumpInput;
            public bool rollInput;
            public bool strafeInput;
            public bool sprintInput;
            public bool crouchInput;

            public float timeStamp;
        }


        #region 绑定事件行为

        /// <summary>
        /// 玩家移动
        /// </summary>
        /// <param name="notification"></param>
        public void OnMoveInput( INotification notification )
        {
            var moveDelta = (Vector2) notification.Body;

            if (!m_playerController.CanMoveInput())
                return;

            if (Mathf.Abs(moveDelta.x) >= float.Epsilon || Mathf.Abs(moveDelta.y) >= float.Epsilon)
                m_playerController.InputState = EN_InputState.EPS_KeyInput;

            Move(moveDelta);
        }

        public void OnSprintInput( INotification notification )
        {
            if (!m_playerController.CanMoveInput())
                return;
            if (GlobalManager.Instance.IsSync && NetworkClient.active)
                Cmd_Sprint((bool) notification.Body);
            else
                Sprint((bool) notification.Body);
        }

        /// <summary>
        /// 玩家跳起
        /// </summary>
        public void OnJumpInput( INotification notification )
        {
            if (!m_playerController.CanJumpInput())
                return;

            m_playerController.InputState = EN_InputState.EPS_KeyInput;

            if (GlobalManager.Instance.IsSync && NetworkClient.active)
                Cmd_Jump();
            else
                Jump();
        }

        /// <summary>
        /// 玩家滚动
        /// </summary>
        public void OnRollInput( INotification notification )
        {
            if (!m_playerController.CanJumpInput())
                return;

            m_playerController.InputState = EN_InputState.EPS_KeyInput;

            if (GlobalManager.Instance.IsSync && NetworkClient.active)
                Cmd_Roll();
            else
                Roll();
        }

        /// <summary>
        /// 玩家拉近（扫射）
        /// </summary>
        /// <param name="notification"></param>
        public void OnStrafeInput( INotification notification )
        {
            if (!m_playerController.CanJumpInput())
                return;

            m_playerController.InputState = EN_InputState.EPS_KeyInput;

            if (GlobalManager.Instance.IsSync && NetworkClient.active)
                Cmd_Strafe();
            else
                Strafe();
        }


        public void OnCrouchInput( INotification notification )
        {
            if (!m_playerController.CanJumpInput())
                return;

            m_playerController.InputState = EN_InputState.EPS_KeyInput;

            if (GlobalManager.Instance.IsSync && NetworkClient.active)
                Cmd_Crouch();
            else
                Crouch();
        }

        private void OnMouseClickInput( INotification obj )
        {
            if (!m_playerController.CanJumpInput())
                return;
            m_playerController.InputState = EN_InputState.EPS_NavInput;

            if (m_playerController) m_playerController.ClickMove((Vector3) obj.Body);
        }

        public void InternalMoveInput( float x, float y )
        {
            if (!m_playerController.CanMoveInput())
                return;

            if (GlobalManager.Instance.IsSync && NetworkClient.active)
                Cmd_MoveInput(x, y);
            else
                MoveInput(x, y);
        }

        #endregion

        #region Mirror角色同步

        public void OnUpdate()
        {
            if (GlobalManager.Instance.IsSync)
                if (!isLocalPlayer && hasAuthority)
                    GetInputs(ref _inputs);
        }

        public void OnFixedUpdate()
        {
            if (!m_playerController)
                return;

            /*if (GlobalManager.Instance.ReplayMode == ActionMode.PLAY)
            {
                OnRePlayFixedUpdate();
                return;
            }*/

            if (!GlobalManager.Instance.IsSync)
                return;

            if (isLocalPlayer)
            {
                _results.timeStamp = Time.time;
                //侦听服务器/主机的非权威客户端或平面移动和旋转的客户端预测
                var lastPosition = _results.position;
                var lastRotation = _results.rotation;
                _results.rotation = transform.rotation;
                _results.position = transform.position;
                if (hasAuthority)
                {
                    //将结果发送到其他客户端（状态同步）
                    if (_dataStep >= syncInterval)
                    {
                        if (Math.Abs(_results.position.sqrMagnitude - lastPosition.sqrMagnitude) > 0 ||
                            Quaternion.Angle(_results.rotation, lastRotation) > 0)
                        {
                            //_results.timeStamp = _results.timeStamp;
                            //结构必须是全新的才能算为脏的
                            //转换一些值以减少流量
                            var tempResults = new SyncResults
                            {
                                position = _results.position,
                                rotation = _results.rotation,
                                //sprintInput = _results.sprintInput,
                                //crouchInput = _results.crouchInput,
                                timeStamp = _results.timeStamp
                            };
                            CmdSyncResult(tempResults);
                        }
                        _dataStep = 0;
                    }
                    _dataStep += Time.fixedDeltaTime;
                }
                else
                {
                    //非业主客户A.K.A.虚拟客户
                    //结果列表中应该至少有两个记录，这样就可以在它们之间进行插值，以防出现某些丢失的压缩或延迟峰值。
                    //是的，这个愚蠢的结构应该在这里，因为它应该在至少有两个记录的情况下开始播放数据，即使只剩下一个记录，也要继续播放
                    if (_resultsList.Count == 0) _playData = false;
                    if (_resultsList.Count >= 2) _playData = true;
                    if (_playData)
                    {
                        if (_dataStep == 0)
                        {
                            _startPosition = _results.position;
                            _startRotation = _results.rotation;
                            //_dataStep = Time.time - _results.timeStamp;
                        }
                        _step = 1 / syncInterval;
                        //_results.rotation = _resultsList[0].rotation;
                        _results.rotation = Quaternion.Lerp(_startRotation, _resultsList[0].rotation, _dataStep);
                        _results.position = Vector3.Lerp(_startPosition, _resultsList[0].position, _dataStep);

                        _dataStep += _step * Time.fixedDeltaTime;

                        //当前：位置缓动lerp
                        //优化方向：根据位置补偿计算刚体力，通过UpdatePosition（）进行刚体同步（未实现）
                        //var delta = _resultsList[0].position - _startPosition;
                        //_results.position = delta / syncInterval;
                        if (_dataStep >= 1)
                        {
                            _dataStep = 0;
                            //_results.position = Vector3.zero;
                            _resultsList.RemoveAt(0);
                        }
                    }
                    UpdateRotation(_results.rotation);
                    UpdatePosition(_results.position);
                    //UpdateCrouch(_results.crouchInput);
                    //UpdateSprinting(_results.sprintInput);
                }
            }
            else
            {
                //Server
                _results.timeStamp = Time.time;
                var lastInput = _results.input;
                var lastPosition = _results.position;
                var lastRotation = _results.rotation;

                //Sending results to other clients(state sync)
                _results.rotation = transform.rotation;
                _results.position = transform.position;
                if (hasAuthority)
                {
                    _results.input = m_playerController.input;

                    if (_dataStep >= syncInterval)
                    {
                        if (Math.Abs(_results.position.sqrMagnitude - lastPosition.sqrMagnitude) >
                            0 || Quaternion.Angle(_results.rotation, lastRotation) > 0
                              || Math.Abs(lastInput.sqrMagnitude - _results.input.sqrMagnitude) > 0)
                        {
                            //Struct need to be fully new to count as dirty 
                            //Convering some of the values to get less traffic
                            var tempResults = new SyncResults
                            {
                                input = _results.input,
                                rotation = _results.rotation,
                                position = _results.position,
                                timeStamp = _results.timeStamp
                                //tempResults.sprinting = _results.sprinting;
                                //tempResults.crouching = _results.crouching;
                            };
                            syncResults = tempResults;
                        }
                        _dataStep = 0;
                    }
                    _dataStep += Time.fixedDeltaTime;
                }
                else
                {
                    //Non-owner client a.k.a. dummy client
                    //there should be at least two records in the results list so it would be possible to interpolate between them in case if there would be some dropped packed or latency spike
                    //And yes this stupid structure should be here because it should start playing data when there are at least two records and continue playing even if there is only one record left 
                    if (_resultsList.Count == 0)
                        _playData = false;
                    else
                        _results.input = _resultsList[0].input;

                    if (_resultsList.Count >= 2) _playData = true;

                    if (_playData)
                    {
                        if (_dataStep == 0)
                        {
                            _startPosition = _results.position;
                            _startRotation = _results.rotation;
                            _results.input = _resultsList[0].input;
                        }
                        _step = 1 / syncInterval;
                        _results.rotation = Quaternion.Slerp(_startRotation, _resultsList[0].rotation, _dataStep);
                        _results.position = Vector3.Lerp(_startPosition, _resultsList[0].position, _dataStep);
                        _dataStep += _step * Time.fixedDeltaTime;
                        if (_dataStep >= 1)
                        {
                            _dataStep = 0;
                            _resultsList.RemoveAt(0);
                        }
                    }
                    UpdateInput(_results.input);
                    UpdateRotation(_results.rotation);
                    UpdatePosition(_results.position);
                }
            }
        }

        public void OnRePlayFixedUpdate()
        {
#if EzReplay
            
            //侦听服务器/主机的非权威客户端或平面移动和旋转的客户端预测
            _results.rotation = transform.rotation;
            _results.position = transform.position;
            //非业主客户A.K.A.虚拟客户
            //结果列表中应该至少有两个记录，这样就可以在它们之间进行插值，以防出现某些丢失的压缩或延迟峰值。
            //是的，这个愚蠢的结构应该在这里，因为它应该在至少有两个记录的情况下开始播放数据，即使只剩下一个记录，也要继续播放
            if (_resultsList.Count == 0) _playData = false;
            if (_resultsList.Count >= 2) _playData = true;
            if (_playData)
            {
                if (_dataStep == 0)
                {
                    _startPosition = _results.position;
                    _startRotation = _results.rotation;
                    _step = 1 / EZReplayManager.Instance.recordingInterval;
                }
                //_results.rotation = _resultsList[0].rotation;
                _results.rotation = Quaternion.Lerp(_startRotation, _resultsList[0].rotation, _dataStep);
                _results.position = Vector3.Lerp(_startPosition, _resultsList[0].position, _dataStep);

                _dataStep += _step * Time.fixedDeltaTime;

                //当前：位置缓动lerp
                //优化方向：根据位置补偿计算刚体力，通过UpdatePosition（）进行刚体同步（未实现）
                //var delta = _resultsList[0].position - _startPosition;
                //_results.position = delta / syncInterval;
                if (_dataStep >= 1)
                {
                    _dataStep = 0;
                    //_results.position = Vector3.zero;
                    _resultsList.RemoveAt(0);
                }
            }
            UpdateRotation(_results.rotation);
            UpdatePosition(_results.position);
            //UpdateCrouch(_results.crouchInput);
            //UpdateSprinting(_results.sprintInput);
            
#endif
        }


        /// <summary>
        /// 位置、旋转同步信息调正
        /// </summary>
        /// <param name="tempResults"></param>
        [Command]
        private void CmdSyncResult( SyncResults tempResults )
        {
            syncResults = tempResults;
            ServerRecieveResults(syncResults);
        }


        //Updating Clients with server states
        [ClientCallback]
        private void ClientRecieveResults( SyncResults syncResults )
        {
            RecieveResults(syncResults);
        }

        [ServerCallback]
        private void ServerRecieveResults( SyncResults syncResults )
        {
            RecieveResults(syncResults);
        }

        private void RecieveResults( SyncResults syncResults )
        {
            //将值转换回
            var results = new Results
            {
                position = syncResults.position,
                rotation = syncResults.rotation,
                //sprintInput = syncResults.sprintInput,
                //crouchInput = syncResults.crouchInput,
                input = syncResults.input,
                //todo:
                timeStamp = syncResults.timeStamp
            };

            //丢弃无序结果
            if (results.timeStamp <= _lastTimeStamp) return;
            _lastTimeStamp = results.timeStamp;
            //非业主客户
            if (isLocalPlayer && !hasAuthority)
            {
                //将结果添加到结果列表中，以便在插值过程中使用。
                results.timeStamp = Time.time;
                _resultsList.Add(results);

                //搜索接收到的时间戳

                //删除在时间戳之前的所有输入
                //int targetCount = _resultsList.Count - foundIndex;
                while (_resultsList.Count > 3) _resultsList.RemoveAt(0);
            }
            else if (!hasAuthority)
            {
                results.timeStamp = Time.time;
                //Debug.Log("results" + results.position);
                _resultsList.Add(results);

                //删除在时间戳之前的所有输入
                //int targetCount = _resultsList.Count - foundIndex;
                while (_resultsList.Count > 3) _resultsList.RemoveAt(0);
            }

            //Owner client if input sync away
            //应执行服务器-客户机协调过程，以便客户机与服务器值的轮换和定位，但不要抖动。
            //if (localPlayerAuthority && !hasAuthority)
            //{
            //    //Update client's position and rotation with ones from server 
            //    _results.rotation = results.rotation;
            //    _results.position = results.position;
            //    int foundIndex = -1;
            //    //Search recieved time stamp in client's inputs list
            //    for (int index = 0; index < _resultsList.Count; index++)
            //    {
            //        //If time stamp found run through all inputs starting from needed time stamp 
            //        if (_resultsList[index].timeStamp > results.timeStamp)
            //        {
            //            foundIndex = index;
            //            break;
            //        }
            //    }
            //    if (foundIndex == -1)
            //    {
            //        //如果找不到需要的记录，则清除输入列表
            //        while (_resultsList.Count != 0)
            //        {
            //            _resultsList.RemoveAt(0);
            //        }
            //        return;
            //    }
            //    //重播录制的输入
            //    for (int subIndex = foundIndex; subIndex < _resultsList.Count; subIndex++)
            //    {
            //        _results.rotation = Rotate(_resultsList[subIndex], _results);
            //        _results.crouchInput = Crouch(_resultsList[subIndex], _results);
            //        _results.sprintInput = Sprint(_resultsList[subIndex], _results);

            //        _results.position = Move(_inputsList[subIndex], _results);
            //    }
            //    //删除在时间戳之前的所有输入
            //    int targetCount = _resultsList.Count - foundIndex;
            //    while (_resultsList.Count > targetCount)
            //    {
            //        _resultsList.RemoveAt(0);
            //    }
            //}
        }

        public void RecieveResults( Vector3 pos, Quaternion qua, float timeStamp )
        {
            //if (Vector3.Distance(transform.position, pos) > 0 || Quaternion.Angle(transform.rotation, qua) > 0)
            {
                //将值转换回
                var results = new Results
                {
                    position = pos,
                    rotation = qua,
                    //todo:
                    timeStamp = Time.time - _lastTimeStamp
                };
                if (_lastTimeStamp == 0)
                    results.timeStamp = 0.1f;
                //Debug.Log($"_resultsList{_resultsList.Count}results.timeStamp{results.timeStamp}");
                _resultsList.Add(results);
            }
            _lastTimeStamp = Time.time;

            //删除在时间戳之前的所有输入
            //int targetCount = _resultsList.Count - foundIndex;
            while (_resultsList.Count > 3) _resultsList.RemoveAt(0);
        }

        public virtual void GetInputs( ref Inputs inputs )
        {
            //Don't use one frame events in this part
            //It would be processed incorrectly 
            inputs.input = m_playerController.input;
            //inputs.sprintInput = isSprinting;
            //inputs.crouchInput = isCrouching;
            //inputs.jumpInput = isJumping;
            //inputs.rollInput = isRolling;
            //inputs.strafeInput = isStrafing;
            inputs.timeStamp = Time.time;
        }

        public void RecieveInputs( SyncInputs syncInputs )
        {
            //将值转换回
            if (isLocalPlayer && !hasAuthority)
            {
            }
            else if (!hasAuthority)
            {
                UpdateInput(syncInputs.input);
            }
        }

        #endregion

        #region Sync Server Input

        [Command]
        private void Cmd_Jump()
        {
            Rpc_Jump();
            Jump();
        }

        [Command]
        private void Cmd_Sprint( bool isSprint )
        {
            Rpc_Sprint(isSprint);
            Sprint(isSprint);
        }

        [Command]
        private void Cmd_Crouch()
        {
            Crouch();
            Rpc_Crouch(m_playerController.isCrouching);
        }


        [Command]
        private void Cmd_Roll()
        {
            Rpc_Roll();
            Roll();
        }

        [Command]
        private void Cmd_Strafe()
        {
            Strafe();
            Rpc_Strafe(m_playerController.isStrafing);
        }

        [Command]
        private void Cmd_MoveInput( float x, float y )
        {
            Rpc_Click(x, y);
            MoveInput(x, y);
        }

        #endregion

        #region Sync Client Input

        [ClientRpc]
        private void Rpc_Jump()
        {
            Jump();
        }

        [ClientRpc]
        private void Rpc_Sprint( bool isSprint )
        {
            Sprint(isSprint);
        }

        [ClientRpc]
        private void Rpc_Crouch( bool isCrouch )
        {
            Crouch(isCrouch);
        }

        [ClientRpc]
        private void Rpc_Roll()
        {
            Roll();
        }

        [ClientRpc]
        private void Rpc_Strafe( bool isStrafe )
        {
            Strafe(isStrafe);
        }

        [ClientRpc]
        private void Rpc_Click( float x, float y )
        {
            MoveInput(x, y);
        }

        #endregion

        #region On Sync Input

        private void Move( Vector2 moveDelta )
        {
            if (m_playerController) m_playerController.Move(moveDelta.x, moveDelta.y);
        }

        private void Jump()
        {
            if (m_playerController) m_playerController.Jump();
        }

        private void Sprint( bool isSprint )
        {
            if (m_playerController) m_playerController.Sprint(isSprint);
        }

        private void Crouch( bool isCrouch )
        {
            if (m_playerController) m_playerController.Crouch(isCrouch);
        }

        private void Crouch()
        {
            if (m_playerController) m_playerController.Crouch();
        }

        private void Roll()
        {
            if (m_playerController) m_playerController.Roll();
        }

        private void Strafe( bool isStrafe )
        {
            if (m_playerController) m_playerController.Strafe(isStrafe);
        }

        private void Strafe()
        {
            if (m_playerController) m_playerController.Strafe();
        }

        private void MoveInput( float x, float y )
        {
            if (m_playerController) m_playerController.MoveInput(x, y);
        }

        #endregion

        #region Sync Result repair

        public virtual void UpdateInput( Vector2 input )
        {
            m_playerController.MoveInput(input.x, input.y);
        }

        //下一个虚拟函数可以在继承类中为自定义移动和旋转力学进行更改
        public virtual void UpdatePosition( Vector3 delta )
        {
            transform.position = delta;
            //刚体驱动
            //m_playerController.FreeMove(delta);
            //Debug.LogError($"expression{transform.position}");
        }

        public virtual void UpdateRotation( Quaternion newRotation )
        {
            transform.rotation = newRotation;
        }

        public virtual void UpdateCrouch( bool crouch )
        {
        }

        public virtual void UpdateSprinting( bool sprinting )
        {
        }

        #endregion
    }
}