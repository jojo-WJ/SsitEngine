/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/22 9:12:04                     
*└──────────────────────────────────────────────────────────────┘
*/
using SsitEngine.DebugLog;
using SsitEngine.Unity.Utility;
using System;
using System.Collections.Generic;
using Framework.Data;
using Framework.Logic;
using Framework.SceneObject;
using Mirror;
using SSIT.proto;
using SsitEngine.EzReplay;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace Framework.Mirror
{
    /// <summary>
    /// 虚拟玩家代理（由于系统未使用第一人称游戏模式【客户端作为网络端的玩家代理存在】）
    /// 辅助mirror 的内部方法同步的进行（客户端对象权限、卵生系统、视角关联）
    /// </summary>
    public class NetPlayerAgent : NetworkBehaviour, IFollowScreen, ISave
    {
        #region Sync screen

        public Transform CamTra
        {
            get
            {
                if (Camera.main)
                {
                    return Camera.main.transform;
                }
                return null;
            }
        }


        public bool IsActive
        {
            get { return isActive; }
        }

        public SyncFollowResult FollowResult
        {
            get
            {
                return syncResult;
            }
        }

        public Transform GetFollowTarget()
        {
            if (m_baseObject == null || m_baseObject.IsDirty)
            {
                return null;
            }
            return transform;
        }

        #endregion

        #region Old Version
        [Command]
        public void CmdRequestAuthority(NetworkIdentity networkIdentity)
        {
            if (networkIdentity == null)
            {
                return;
            }
            var clientAuthorityOwner = networkIdentity.clientAuthorityOwner;
            if (clientAuthorityOwner == connectionToClient)
            {
                return;
            }

            if (null != clientAuthorityOwner)
            {
                //networkIdentity.RemoveClientAuthority( clientAuthorityOwner );
            }
            bool result = networkIdentity.AssignClientAuthority(connectionToClient);
            SsitDebug.Info("AssignClientAuthority: " + result);
        }


        #endregion

        #region Mirror
        
        [Disable]
        [SyncVar]
        public string guid;

        [Disable]
        [SyncVar]
        public int index;
        
        protected BaseObject m_baseObject;

        public override void OnStartServer()
        {
            Debug.Log("netplayer OnStartServer" + guid + "  " + NetworkServer.connections.Count);
            SsitApplication.Instance.CreatePlayer(guid, DataItemProxy.c_sClientTerminal, OnInternalCreatedCallBack, null, gameObject);
        }


        public override void OnStartClient()
        {
            Debug.Log("netplayer onstartlocalplayer" + guid + " [] " + NetworkClient.connection.playerController.name);

            SsitApplication.Instance.CreatePlayer(guid, DataItemProxy.c_sClientTerminal, OnInternalCreatedCallBack, null, gameObject);
        }

        public void SetLink(bool b, Player player)
        {
            m_baseObject = player;
            ((NetPlayerAttribute)m_baseObject.GetAttribute()).SetAgent(this);
            m_baseObject.AddChangePropertyCallBack(Func);
        }

        private void OnDestroy()
        {
            if (m_baseObject != null && !m_baseObject.IsDirty)
            {
                ObjectManager.Instance.DestroyObject(guid);
                m_baseObject = null;
            }

            if (NetworkManager.singleton != null)
            {
                if (NetworkClient.active && !NetworkServer.active)
                {
                    Facade.Instance.SendNotification((ushort)EnMirrorEvent.OnRoomClientExit, new ClientExitParam()
                    {
                        userId = guid,
                        isClient = true,
                        isLobby = false,
                        isExit = true,
                        isEnd = false,
                    });
                }
                else if (NetworkServer.active)
                {
                    //update memberInfo
                    Facade.Instance.SendNotification((ushort)EnMirrorEvent.OnRoomClientExit, new ClientExitParam()
                    {
                        userId = guid,
                        isClient = false,
                        isLobby = false,
                        isExit = true,
                        isEnd = false,
                    });
                }
            }
        }

        #endregion

        #region 回放
        public string Guid
        {
            get { return guid; }
            set { }
        }

        public int ItemID
        {
            get { return DataItemProxy.c_sClientTerminal; }
            set { }
        }

        public bool InitSave { get; set; }

        public SavedBase GeneralSaveData(bool isDeepClone = false)
        {
            var ret = m_baseObject?.GetAttribute();
            if (ret == null)
            {
                return null;
            }
            ret.SetSaveObjState(gameObject);
            if (isDeepClone)
            {
                //Debug.Log("ret" + ret);
                try
                {
                    var temp = SerializationUtils.Clone(ret);
                    ret.IsChange = false;
                    return temp;
                }
                catch (Exception e)
                {
                    SsitDebug.Fatal($"{name}exception{e.Message}");
                    throw;
                }

            }

            return ret;
        }

        public GameObject GetRepresent()
        {
            return gameObject;
        }

        public void SaveRecord()
        {
            Facade.Instance.SendNotification((ushort)EnEzReplayEvent.Mark, this);
        }

        public void SynchronizeProperties(SavedBase savedState, bool isReset, bool isFristFrame)
        {
            gameObject.SetActive(savedState.isActive);

            var state = savedState as NetPlayerAttribute;
            if (state == null)
                return;

            EzRecieveResults(state);
        }
        #endregion

        private void OnInternalCreatedCallBack(BaseObject obj, object render, object data)
        {
            //m_baseObject = obj;
            //m_baseObject.AddChangePropertyCallBack(Func);
            // 同步回放组件
            if (GlobalManager.Instance.ReplayMode == ActionMode.READY)
            {
                //添加回放组件
                SaveRecord();
            }
        }

        private void Func(EnPropertyId propertyid, string param, object data)
        {
            switch (propertyid)
            {
                case EnPropertyId.Follow:
                    {
                        Follow((SCFollowClientResult)data);
                    }
                    break;
            }
        }

        void Follow(SCFollowClientResult result)
        {
            if (hasAuthority)
            {
                //开始同步位置、相机模式、操作人员
                isActive = result.isFollow == 1;
            }

        }


        private FollowResult _results;
        [SyncVar(hook = "RecieveResults")]
        private SyncFollowResult syncResult;
        private List<FollowResult> _resultsList = new List<FollowResult>();
        public bool isActive = false;

        private float _lastTimeStamp = 0;
        private float _dataStep = 0;
        private bool _playData = false;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private float _step = 0;

        #region 跟随同步

        public void FixedUpdate()
        {
            //非录制模式下，在不检测情况下不对位置进行实时保存，提高运行效率（但预计正式情况下，可能每次演练都会进行回放记录
            //所以只能说一句fuck you,所以选择不管回放不回放都进行同步了，mmp）
            //if (GlobalManager.Instance.ReplayMode == ActionMode.READY)
            //{

            //}
            //else
            //{

            //    if (!isActive)
            //        return;
            //}


            if (GlobalManager.Instance.ReplayMode == ActionMode.PLAY)
            {
                EzFixedUpdate();
                return;
            }

            if (!GlobalManager.Instance.IsSync)
                return;

            if (isLocalPlayer)
            {
                _results.timeStamp = Time.time;
                //侦听服务器/主机的非权威客户端或平面移动和旋转的客户端预测
                //Vector3 lastPosition = _results.pos;
                //Quaternion lastRotation = _results.rot;

                _results.pos = transform.position;
                _results.rot = transform.rotation;

                if (hasAuthority)
                {
                    GetInput(ref _results);
                    //将结果发送到其他客户端（状态同步）
                    if (_dataStep >= syncInterval)
                    {
                        //if (Vector3.Distance(_results.pos, lastPosition) > 0 || Quaternion.Angle(_results.rot, lastRotation) > 0)
                        {
                            //_results.timeStamp = _results.timeStamp;
                            //结构必须是全新的才能算为脏的
                            //转换一些值以减少流量
                            SyncFollowResult tempResults = new SyncFollowResult
                            {
                                pos = _results.pos,
                                rot = _results.rot,
                                mode = _results.mode,
                                player = _results.player,
                                input = _results.input,
                                zoom = _results.zoom,
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
                    if (_resultsList.Count == 0)
                    {
                        _playData = false;
                    }
                    if (_resultsList.Count >= 2)
                    {
                        _playData = true;
                    }
                    if (_playData)
                    {
                        if (_dataStep == 0)
                        {
                            _startPosition = _results.pos;
                            _startRotation = _results.rot;
                            //_dataStep = Time.time - _results.timeStamp;
                        }
                        _step = 1 / syncInterval;
                        //_results.rotation = _resultsList[0].rotation;
                        _results.rot = Quaternion.Lerp(_startRotation, _resultsList[0].rot, _dataStep);
                        _results.pos = Vector3.Lerp(_startPosition, _resultsList[0].pos, _dataStep);

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
                    UpdateRotation(_results.rot);
                    UpdatePosition(_results.pos);
                    //UpdateCrouch(_results.crouchInput);
                    //UpdateSprinting(_results.sprintInput);
                }
            }
        }

        private void EzFixedUpdate()
        {

            _results.pos = transform.position;
            _results.rot = transform.rotation;
            //非业主客户A.K.A.虚拟客户
            //结果列表中应该至少有两个记录，这样就可以在它们之间进行插值，以防出现某些丢失的压缩或延迟峰值。
            //是的，这个愚蠢的结构应该在这里，因为它应该在至少有两个记录的情况下开始播放数据，即使只剩下一个记录，也要继续播放
            if (_resultsList.Count == 0)
            {
                _playData = false;
            }
            if (_resultsList.Count >= 2)
            {
                _playData = true;
            }
            if (_playData)
            {
                if (_dataStep == 0)
                {
                    _startPosition = _results.pos;
                    _startRotation = _results.rot;
                    //_dataStep = Time.time - _results.timeStamp;
                }
                _step = 1 / syncInterval;
                //_results.rotation = _resultsList[0].rotation;
                _results.rot = Quaternion.Lerp(_startRotation, _resultsList[0].rot, _dataStep);
                _results.pos = Vector3.Lerp(_startPosition, _resultsList[0].pos, _dataStep);

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
            UpdateRotation(_results.rot);
            UpdatePosition(_results.pos);
        }

        //下一个虚拟函数可以在继承类中为自定义移动和旋转力学进行更改
        public virtual void UpdatePosition(Vector3 delta)
        {
            transform.position = delta;
        }

        public virtual void UpdateRotation(Quaternion newRotation)
        {
            transform.rotation = newRotation;
        }

        /// <summary>
        /// 位置、旋转同步信息调正
        /// </summary>
        /// <param name="tempResults"></param>
        [Command]
        void CmdSyncResult(SyncFollowResult tempResults)
        {
            syncResult = tempResults;
            ServerRecieveResults(tempResults);
        }

        [ClientCallback]
        void ClientRecieveResults(SyncFollowResult syncResults)
        {
            RecieveResults(syncResults);
        }

        [ServerCallback]
        void ServerRecieveResults(SyncFollowResult syncResults)
        {
            RecieveResults(syncResults);
            if (!isActive)
            {
                transform.position = syncResults.pos;
                transform.rotation = syncResults.rot;
                isActive = true;
            }
        }

        void RecieveResults(SyncFollowResult syncResults)
        {
            //syncResult = syncResults;

            //将值转换回
            FollowResult results = new FollowResult
            {
                pos = syncResults.pos,
                rot = syncResults.rot,
                mode = syncResults.mode,
                player = syncResults.player,
                input = syncResults.input,
                zoom = syncResult.zoom,

                timeStamp = syncResults.timeStamp
            };

            //丢弃无序结果
            if (results.timeStamp <= _lastTimeStamp)
            {
                return;
            }

            _lastTimeStamp = results.timeStamp;
            //非业主客户
            if (!hasAuthority)
            {
                //将结果添加到结果列表中，以便在插值过程中使用。
                results.timeStamp = Time.time;
                _resultsList.Add(results);

                //删除在时间戳之前的所有输入
                //int targetCount = _resultsList.Count - foundIndex;
                while (_resultsList.Count > 3)
                {
                    _resultsList.RemoveAt(0);
                }
            }
        }

        void EzRecieveResults(NetPlayerAttribute syncResults)
        {
            syncResult = new SyncFollowResult()
            {
                pos = syncResults.position,
                rot = syncResults.rotation,
                mode = syncResults.mode,
                player = syncResults.player,
                input = syncResults.input,
                zoom = syncResult.zoom,
            };

            //将值转换回
            FollowResult results = new FollowResult
            {
                pos = syncResults.position,
                rot = syncResults.rotation,
                mode = syncResults.mode,
                player = syncResults.player,
                input = syncResults.input,
                zoom = syncResult.zoom,
            };
            //非业主客户
            if (!hasAuthority)
            {
                //将结果添加到结果列表中，以便在插值过程中使用。
                results.timeStamp = Time.time;
                _resultsList.Add(results);

                //删除在时间戳之前的所有输入
                //int targetCount = _resultsList.Count - foundIndex;
                while (_resultsList.Count > 3)
                {
                    _resultsList.RemoveAt(0);
                }
            }
        }

        void GetInput(ref FollowResult result)
        {
            if (null == CamTra)
                return;

            var cam = CameraController.instance;

            transform.position = CamTra.position;
            transform.rotation = CamTra.rotation;

            result.pos = transform.position;
            result.rot = transform.rotation;

            result.zoom = cam.CurrentZoom;
            result.mode = GlobalManager.Instance.InputMode;
            result.input = new Vector2(cam.MouseX, cam.MouseY);
            var player = GlobalManager.Instance.Player;
            result.player = player != null ? player.Guid : string.Empty;

        }
        #endregion


        public void ApplyInput(NetPlayerAttribute netPlayerAttribute)
        {
            netPlayerAttribute.mode = syncResult.mode;
            netPlayerAttribute.player = syncResult.player;
            netPlayerAttribute.input = syncResult.input;
            netPlayerAttribute.zoom = syncResult.zoom;
        }
    }

    public struct FollowResult
    {
        public Vector3 pos;
        public Quaternion rot;
        public EnInputMode mode;
        public string player;
        public Vector2 input;
        public float zoom;
        public float timeStamp;
    }

    [Serializable]
    public struct SyncFollowResult
    {
        public Vector3 pos;
        public Quaternion rot;
        public EnInputMode mode;
        public string player;
        public Vector2 input;
        public float zoom;
        public float timeStamp;
    }
}
