/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/14 18:19:32                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using SSIT.proto;
using SsitEngine.Unity.Action;
using SsitEngine.Unity.SceneObject;
using UnityEngine;

namespace Framework.SceneObject
{
    //消息平滑同步方式
    public class ScriptSyncTrans : ActionBase
    {
        private float _dataStep;

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

        public float syncInterval = 0.1f;


        private SyncResults syncResults;
        public bool hasAuthority { get; set; }
        public BaseObject BaseObject { get; set; }


        public override void Execute( object sender, EnPropertyId m_actionId, string m_actionParam, object data = null )
        {
            RecieveResults((TransTempInfo) data);
        }

        //This struct would be used to collect results of Move and Rotate functions
        public struct Results
        {
            public Quaternion rotation;

            public Vector3 position;

            //public Vector2 input;
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

            //public Vector2 input;
            //public float jumpInput;
            //public bool rollInput;
            //public bool strafeInput;
            //public bool sprintInput;
            //public bool crouchInput;
            public float timeStamp;
        }


        #region Mirror角色同步

        public void FixedUpdate()
        {
            if (BaseObject == null || BaseObject.LoadStatu != EnLoadStatu.Inited)
                return;

            if (!GlobalManager.Instance.IsSync)
                return;

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
                    if (Vector3.Distance(_results.position, lastPosition) > 0 ||
                        Quaternion.Angle(_results.rotation, lastRotation) > 0)
                    {
                        //_results.timeStamp = _results.timeStamp;
                        //结构必须是全新的才能算为脏的
                        //转换一些值以减少流量
                        var info = new TransTempInfo
                        {
                            guid = BaseObject.Guid,
                            position = _results.position,
                            rotation = _results.rotation,
                            tamp = _results.timeStamp
                        };
                        SyncResult(info);
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
                    _dataStep += _step * Time.fixedDeltaTime;

                    _results.rotation = Quaternion.Lerp(_startRotation, _resultsList[0].rotation, _dataStep);
                    _results.position = Vector3.Lerp(_startPosition, _resultsList[0].position, _dataStep);
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

        public void EzFixedUpdate()
        {
            _results.timeStamp = Time.time;
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
        }

        private void SyncResult( TransTempInfo info )
        {
            /*CSSyncTransRequest request = new CSSyncTransRequest() { transInfo = info };
            Framework.NetSocket.MessagePackage message = new Framework.NetSocket.MessagePackage(ConstMessageID.CSSyncTransRequest, request);
            Facade.Instance.SendNotification((ushort)EnMirrorEvent.SendMessage, message);*/
        }

        public void RecieveResults( TransTempInfo info )
        {
            //将值转换回
            var results = new Results
            {
                position = info.position,
                rotation = info.rotation,
                //sprintInput = syncResults.sprintInput,
                //crouchInput = syncResults.crouchInput,
                //todo:
                timeStamp = info.tamp
            };

            //丢弃无序结果
            if (results.timeStamp <= _lastTimeStamp) return;
            _lastTimeStamp = results.timeStamp;
            //非业主客户
            if (!hasAuthority)
            {
                //将结果添加到结果列表中，以便在插值过程中使用。
                results.timeStamp = Time.time;
                _resultsList.Add(results);

                //搜索接收到的时间戳
                //删除在时间戳之前的所有输入
                //int targetCount = _resultsList.Count - foundIndex;
                while (_resultsList.Count > 10) _resultsList.RemoveAt(0);
            }
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

        #endregion


        #region Sync Result repair

        //下一个虚拟函数可以在继承类中为自定义移动和旋转力学进行更改
        public virtual void UpdatePosition( Vector3 delta )
        {
            transform.position = delta;
            //刚体驱动
            //m_playerController.FreeMove(delta);
        }

        public virtual void UpdateRotation( Quaternion newRotation )
        {
            transform.rotation = newRotation;
        }

        #endregion
    }
}