using Framework.SceneObject;
using Framework.SsitInput;
using Framework.Navigator;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Framework
{

    public enum NavType
    {
        NT_None,
        NT_Follow,
        NT_Move,
        NT_NavMove
    }

    public class NavAgentProxy : MonoBehaviour
    {
        private List<Vector3> _corners = new List<Vector3>();
        private int _navMeshPathIndex = 0;
        private bool _cursorPointUpdated = false;

        public bool isUseNavAgent = false;
        public bool showPath = false;

        public float stopDistance = 0.3f;
        private GameObject Green;

        //["Private"]
        private NavType navType = NavType.NT_None;
        private NavMeshPath _navMeshPath;
        private BaseSceneInstance mProxyObj;
        private Vector3 mFlowPoint;
        private Vector3 mTargetPos;
        private BaseSceneInstance mFollowPlayer;

        //["delegate"]
        public delegate void NavProxyHandle(bool isStop);
        public delegate void NavProxySteerHandle(Vector3 targetPos, Vector3 endPos, float angle, float distance, float rotatePerSecond, float accuracy, float stopDistance);

        public NavProxyHandle OnNavHandle;
        public NavProxySteerHandle OnNavProxySteerHandle;

        [Header("Component")]
        private NavMeshAgent agent;
        private NavMeshObstacle obstacle;

        [Header("Helper")]
        private bool isNav = false;


        [Range(0, 360)]
        public float angle = 160f;                       //检测前方角度范围
        [Range(0, 100)]
        public float distance = 1f;                    //检测距离
        public float rotatePerSecond = 30f;             //每秒旋转角度
        [Range(1, 50)]
        public float accuracy = 5f;                     //检测精度

        public BaseSceneInstance ProxyObj
        {
            get { return mProxyObj; }
            set { mProxyObj = value; }
        }

        public BaseSceneInstance FollowPlayer
        {
            get { return mFollowPlayer; }
            set
            {
                if (mFollowPlayer != value)
                {
                    if (InterruptMove != null)
                    {
                        InterruptMove();
                    }
                    mFollowPlayer = value;
                }
            }
        }

        private NavigatorPathIndicator _mPathIndicator;
        public List<Vector3> corners
        {
            set
            {
                _corners = value;
                _navMeshPathIndex = 1;
                _cursorPointUpdated = true;

                if (showPath && value != null)
                {
                    if (InputManager.Instance != null)
                    {
                        if (_mPathIndicator != null)
                        {
                            InputManager.Instance.ReleasePathIndicator(_mPathIndicator);
                        }
                        _mPathIndicator = InputManager.Instance.DrawPathIndicator(value.ToArray());
                    }
                }
            }
        }

        public UnityAction<Vector3> actMove { private set; get; }

        public UnityAction actOnMoveEnd { private set; get; }

        public UnityAction InterruptMove { get; private set; }

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent)
            {
                stopDistance = agent.stoppingDistance;
            }
            obstacle = GetComponent<NavMeshObstacle>();

            OnNavHandle = NavHandle;
            OnNavHandle(false);
        }

        private void OnDestroy()
        {
            if (_mPathIndicator != null)
            {
                InputManager.Instance?.ReleasePathIndicator(_mPathIndicator);
            }
        }


        private void NavHandle(bool value)
        {
            if (obstacle)
            {
                obstacle.enabled = !value;
            }
            if (showPath && !value && _mPathIndicator != null)
            {
                if (InputManager.Instance != null)
                {
                    InputManager.Instance.ReleasePathIndicator(_mPathIndicator);
                    _mPathIndicator = null;
                }
            }
        }

        public void OnUpdate()
        {
            switch (navType)
            {

                case NavType.NT_None:
                    ClearTarget();
                    break;
                case NavType.NT_Move:
                    Move();
                    break;
                case NavType.NT_Follow:
                    FollowMove();
                    break;
                case NavType.NT_NavMove:
                    NavMove();
                    break;

            }
        }
        NavMeshHit hit;

        private void Move()
        {
            if (_corners.Count <= 0)
            {
                return;
            }
            //TODO:停止距离判定
            if (NearPoint(mProxyObj.transform.position, mTargetPos))
            {
                _cursorPointUpdated = false;

                if (null != actOnMoveEnd)
                {
                    InterruptMove = null;
                    actOnMoveEnd.Invoke();
                }
                return;
            }
            //todo:规避问题
            if (isNav)
            {

                bool blocked = NavMesh.Raycast(transform.position, mTargetPos, out hit, NavMesh.AllAreas);
                Debug.DrawLine(transform.position, mTargetPos, blocked ? Color.red : Color.green);
                //如果（被阻止）
                if (blocked)
                {
                    Debug.DrawRay(hit.position, Vector3.up, Color.cyan);
                    //ResetCalcPath(mTargetPos);
                    //Debug.LogError("被阻止");

                }
            }

            //路径点寻路
            if (_cursorPointUpdated && _navMeshPathIndex < _corners.Count &&
                !NearPoint(mProxyObj.transform.position, _corners[_navMeshPathIndex],0.2f))
            {

                if (null != actMove)
                {
                    //todo：检测处理
                    //if (OnNavProxySteerHandle != null)
                    //{
                    //    OnNavProxySteerHandle(_corners[_navMeshPathIndex], mTargetPos, angle, distance, rotatePerSecond, accuracy, stopDistance);
                    //}
                    //else
                    {
                        actMove.Invoke(_corners[_navMeshPathIndex]);
                    }
                }

            }
            else if (_navMeshPathIndex < _corners.Count - 1)
            {
                ++_navMeshPathIndex;
            }
            else
            {
                _cursorPointUpdated = false;
                if (null != actOnMoveEnd)
                {
                    InterruptMove = null;
                    actOnMoveEnd.Invoke();
                }
                ClearTarget();
            }
        }

        private void NavMove()
        {
            if (_corners.Count <= 0)
            {
                return;
            }
            if (isUseNavAgent && agent.enabled)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (actOnMoveEnd != null)
                    {
                        InterruptMove = null;
                        actOnMoveEnd();
                        actOnMoveEnd = null;
                        ClearTarget();
                    }
                }

            }
        }
        private void FollowMove()
        {
            Vector3 cursorPoint = mFollowPlayer.transform.TransformPoint(mFlowPoint);

            ResetCalcPath(cursorPoint);
        }

        void ResetCalcPath(Vector3 targetPos)
        {
            if (_navMeshPath == null)
            {
                _navMeshPath = new NavMeshPath();
            }
            if (NavMesh.CalculatePath(transform.position, targetPos, NavMesh.AllAreas, _navMeshPath) &&
                _navMeshPath.corners.Length > 1)
            {
                _corners.Clear();
                _corners.AddRange(_navMeshPath.corners);
                for (int i = 1; i < _corners.Count - 2; i++)
                {
                    NavMeshHit hithit;
                    bool result = NavMesh.FindClosestEdge(_corners[i], out hithit, NavMesh.AllAreas);
                    if (result && hithit.distance < 1.0f)
                        _corners[i] = hithit.position + hithit.normal * 1.0f;
                }
                isNav = true;
            }
            else
            {
                isNav = false;
                _corners.Clear();
                _corners.Add(transform.position);
                _corners.Add(targetPos);
            }
            corners = _corners;
            mTargetPos = targetPos;
            Move();
        }

        //private void LateUpdate()
        //{
        //    //下一桢导航位置差调正
        //    if (!isUseNavAgent)
        //    {
        //        transform.localPosition = new Vector3( 0, transform.localPosition.y, 0 );
        //    }
        //}

        public bool CalcNavPath(Vector3 targetPos, ref NavMeshPath navMeshPath)
        {

            if (agent.enabled && agent.CalculatePath(targetPos, navMeshPath) && navMeshPath.corners.Length >= 2)
            {
                return true;
            }
            return false;
        }

        public void setStopDistance(float distance)
        {
            stopDistance = distance;
            if (isUseNavAgent)
            {
                agent.stoppingDistance = distance;
            }
        }
        /// <summary>
        /// 设置点位
        /// </summary>
        /// <param name="鼠标点击位置"></param>
        /// <param name="路径集合"></param>
        /// <param name="是否导航"></param>
        /// <param name="movingFunc"></param>
        /// <param name="interruptMovefunc"></param>
        /// <param name="actOnMoveEndFunc"></param>
        public void SetMovePostion(Vector3 targetPos, List<Vector3> path, bool isNav, bool isFlag, UnityAction<Vector3> movingFunc, UnityAction interruptMovefunc, UnityAction actOnMoveEndFunc)
        {
            if (isFlag)
            {
                if (Green == null)
                {
                    Green = new GameObject("GameObject");
                    GameObject.Instantiate(Resources.Load("Effect/Effect_Prefeb/Efx_Click_Green"), Green.transform);
                }
                Green.transform.position = targetPos;
                Green.SetActive(true);
            }

            this.actMove = movingFunc;
            this.InterruptMove = interruptMovefunc;
            this.actOnMoveEnd = actOnMoveEndFunc;
            mTargetPos = targetPos;

            if (isUseNavAgent)
            {
                agent.enabled = true;
                agent.SetDestination(targetPos);

                if (showPath)
                {
                    NavMeshPath navPath = new NavMeshPath();

                    if (agent.CalculatePath(targetPos, navPath))
                    {
                        Vector3[] mPathcorners = navPath.corners;
                        this.corners = mPathcorners.ToList();
                        navType = NavType.NT_NavMove;

                    }
                }
            }
            else
            {
                this.corners = path;
                navType = NavType.NT_Move;
            }
            this.isNav = isNav;

        }

        public void SetFolowTarget(BaseSceneInstance player, Vector3 followPoint
            , UnityAction<Vector3> movingFunc, UnityAction interruptMovefunc, UnityAction actOnMoveEndFunc)
        {
            FollowPlayer = player;
            mFlowPoint = followPoint;

            InterruptMove = interruptMovefunc;
            navType = NavType.NT_Follow;
        }
        /// <summary>
        /// 清楚点位
        /// </summary>
        public void ClearTarget()
        {
            if (Green != null)
            {
                //for (int i = 0; i < Green.transform.childCount; i++)
                //{
                //    Destroy(Green.transform.GetChild(i).gameObject);
                //}
                Green.SetActive(false);
            }
            if (_mPathIndicator != null)
            {
                InputManager.Instance.ReleasePathIndicator(_mPathIndicator);
            }
            if (InterruptMove != null)
            {
                InterruptMove();
                InterruptMove = null;
            }
            _corners.Clear();
            navType = NavType.NT_None;
        }
        protected bool NearPoint(Vector3 from, Vector3 tar, float offset = 0.1f)
        {
            //Vector3 _tar = new Vector3( tar.x, transform.position.y, tar.z );
            Vector3 to = from + ((PlayerInstance)ProxyObj).PlayerController.MoveDelta;
            //Vector3 to = from + ((FrameworkNetworkPlayer)ProxyObj).PlayerController._rigidbody.velocity;

            float xLOffset = Mathf.Abs(from.x - tar.x);
            float xROffset = Mathf.Abs(tar.x - to.x);

            float zLOffset = Mathf.Abs(from.z - tar.z);
            float zROffset = Mathf.Abs(tar.z - to.z);

            return (xLOffset <= offset || xROffset <= offset) && (zLOffset <= offset || zROffset <= offset);
            //return ((from.x <= tar.x && tar.x <= to.x) || (from.x >= tar.x && tar.x >= to.x))
            //    && ((from.z <= tar.z && tar.z <= to.z) || (from.z >= tar.z && tar.z >= to.z));
            //var _from = new Vector3( from.x, transform.position.y, from.z );
            //var _tar = new Vector3( tar.x, transform.position.y, tar.z );
            //return Vector3.Distance( _a, _b ) <= dis;
        }
        public static bool HasArrived(Vector3 from, Vector3 to, Vector3 tar)
        {
            // The path hasn't been computed yet if the path is pending.
            return ((from.x <= tar.x && tar.x <= to.x) || (from.x >= tar.x && tar.x >= to.x))
             && ((from.z <= tar.z && tar.z <= to.z) || (from.z >= tar.z && tar.z >= to.z));
        }



#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_corners != null && _corners.Count > 0)
            {
                UnityEditor.Handles.color = Color.red;
                for (int i = 0; i < _corners.Count; i++)
                {
                    UnityEditor.Handles.DrawWireCube(_corners[i], Vector3.one * 0.2f);
                    if (i > 0)
                    {
                        UnityEditor.Handles.DrawLine(_corners[i - 1], _corners[i]);
                    }
                }
                UnityEditor.Handles.color = Color.white;
            }
        }
#endif
    }

}
