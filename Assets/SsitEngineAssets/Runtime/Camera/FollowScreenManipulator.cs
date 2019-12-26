using Framework.SceneObject;
using SsitEngine.PureMVC.Patterns;
using UnityEngine;

namespace Framework.Mirror
{
    public interface IFollowScreen
    {
        bool IsActive { get; }

        SyncFollowResult FollowResult { get; }

        Transform GetFollowTarget();
    }

    public class FollowScreenManipulator : MonoBehaviour, IInputState
    {
        private IFollowScreen followScreen;

//        private float _dataStep = 0;
//        private bool _playData = false;
//        private Vector3 _startPosition;
//        private Quaternion _startRotation;

//        private float syncInterval = 0.1f;//同步阈值
//        private float _step = 0;

        private string lastFollowPlayer;
        private EnInputMode lastInputode;

        public ENCameraModeType CameraMode => ENCameraModeType.FollowScreenManipulator;

        public void OnUpdate()
        {
            var tt = followScreen.GetFollowTarget();
            var cma = CameraController.instance;
            if (followScreen == null || !tt /*|| !followScreen.IsActive*/)
            {
                //转到默认状态
                Facade.Instance.SendNotification((ushort) EnGlobalEvent.ChangeInputMode, EnInputMode.Free);
                return;
            }

            var result = followScreen.FollowResult;

            if (result.mode != EnInputMode.Control)
            {
                if (result.mode == EnInputMode.None)
                    return;

                if (lastInputode == EnInputMode.Control) CameraController.instance.SetTarget(null);
                transform.rotation = tt.rotation;
                transform.position = tt.position;
            }
            else
            {
                //设置相机跟随目标
                if (!string.IsNullOrEmpty(result.player))
                {
                    if (lastFollowPlayer != result.player)
                    {
                        var player =
                            ObjectManager.Instance.GetObject<Player>(result.player, EnFactoryType.PlayerFactory);
                        if (player != null && !player.IsDirty)
                            //CameraController.instance.startSmooth = true;
                            cma.SetTarget(player.GetRepresent().transform);
                        //transform.position = tt.position;
                    }
                    else if (cma.target == null)
                    {
                        var player =
                            ObjectManager.Instance.GetObject<Player>(result.player, EnFactoryType.PlayerFactory);
                        //上一次监听的还是此人、中间过程没有更换对象
                        //CameraController.instance.startSmooth = false;
                        transform.rotation = tt.rotation;
                        transform.position = tt.position;
                        cma.SetTarget(player.GetRepresent().transform);
                    }


                    //transform.position  = new Vector3(transform.position.x,transform.position.y);
                }
                cma.CurrentZoom = result.zoom;
                //transform.rotation = tt.rotation;
                //cma.targetRot = result.rot;
                cma.MouseX = result.input.x;
                cma.MouseY = result.input.y;
            }
            lastInputode = result.mode;
            lastFollowPlayer = result.player;
        }

        public void OnLateUpdate()
        {
            //if (followScreen == null)
            {
                //MainProcess.inputStateManager.switchTo(MainProcess.inputStateManager.defaultInputState);
            }
            // 2018-07-28 19:58:14 Shell Lee
            // 插值 平滑 过渡

            //Vector3 lastPosition = transform.position;
            //Quaternion lastRotation = transform.rotation;

            //if (Camera.main && null != followScreen)
            //{

            //if (_dataStep == 0)
            //{
            //    _startPosition = lastPosition;
            //    _startRotation = lastRotation;
            //    //_dataStep = Time.time - _results.timeStamp;
            //}
            //_step = 1 / syncInterval;
            ////_results.rotation = _resultsList[0].rotation;
            //Quaternion LerpRot = Quaternion.Lerp(_startRotation, followScreen.CameraRotation(), _dataStep);
            //Vector3 LerpPos = Vector3.Lerp(_startPosition,followScreen.CameraPosition(), _dataStep);

            //_dataStep += _step * Time.fixedDeltaTime;

            ////当前：位置缓动lerp
            ////优化方向：根据位置补偿计算刚体力，通过UpdatePosition（）进行刚体同步（未实现）
            ////var delta = _resultsList[0].position - _startPosition;
            ////_results.position = delta / syncInterval;
            //if (_dataStep >= 1)
            //{
            //    _dataStep = 0;
            //}

            //transform.rotation = followScreen.CameraRotation();
            //transform.position = followScreen.CameraPosition();

            //}
            //else
            //{
            //MainProcess.FollowGamePlayer(null);
            //}
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
            //if ( null != followScreen )
            //{
            //    followScreen.FollowScreen( true );
            //}
            enabled = true;
//            _dataStep = 0;
        }

        public bool Enable()
        {
            return enabled;
        }

        public void Leave()
        {
            //if ( null != followScreen )
            //{
            //    followScreen.FollowScreen( false );
            //}

            enabled = false;
        }


        public void Follow( IFollowScreen followScreen )
        {
            Leave();

            this.followScreen = followScreen;
        }
    }
}