using Framework.Data;
using Framework.Logic;
using SsitEngine.EzReplay;
using SsitEngine.Unity.Resource;
using SsitEngine.Unity.Utility;
using System.Collections.Generic;
using SSIT.proto;
using SsitEngine;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.SceneObject
{
    public enum ArrowPathType
    {
        RescuePath,// 搜救
        EvacuatePath, // 疏散
        Cordon,
    }

    // 绘制状态
    public enum DrawingState
    {
        None,  //未开始绘制
        Start, // 开始绘制
        Doing, // 绘制中
        End    //绘制结束
    }

    public class PathNode
    {
        public Vector3 pos;
        public PathNode next;
    }

    public class DrawArrowProxy : SsitProxy
    {
        private GameObject Root = null;

        private GameObject m_SelectNode = null;
        private GameObject m_rescueRoot;
        private GameObject m_evacuateRoot;
        private GameObject m_cordonRoot;
        private string m_arrowPrefabPath;
        private GameObject m_cordonPrefab;

        private DrawPathTools mDrawPathTools;
        private DrawPathTools mCloneDrawPathTools;



        public void Init(ArrowPathType cType, DrawingState state)
        {
            if (mDrawPathTools.DrawState == DrawingState.Start)
            {
                return;
            }
            mDrawPathTools.goCurrents[cType] = null;
            mDrawPathTools.CurrentVertexes[cType].Clear();
            mDrawPathTools.CurrArrowPathType = cType;
            mDrawPathTools.DrawState = state;

            //干掉colider

            SetColiderActive(false);


        }

        private void CreateRootNode()
        {
            Root = new GameObject("DrawArrowProxyRoot");
            Root.transform.parent = m_present.transform;

            m_rescueRoot = new GameObject("rescueRoot");
            m_rescueRoot.transform.SetParent(Root.transform);
            m_rescueRoot.transform.localPosition = Vector3.zero;


            m_evacuateRoot = new GameObject("evacuateRoot");
            m_evacuateRoot.transform.SetParent(Root.transform);
            m_evacuateRoot.transform.localPosition = Vector3.zero;

            m_cordonRoot = new GameObject("cordonRoot");
            m_cordonRoot.transform.SetParent(Root.transform);
            m_cordonRoot.transform.localPosition = Vector3.zero;
        }

        public DrawPathTools GetNewDrawTools()
        {
            DrawPathTools mDrawPathTools = new DrawPathTools();
            mDrawPathTools.Init(this, m_rescueRoot, m_evacuateRoot, m_cordonRoot, m_arrowPrefabPath, m_cordonPrefab);
            return mDrawPathTools;
        }

        public DrawingState GetState()
        {
            return _DrawPathTools.DrawState;
        }

        public ArrowPathType CurrArrowPathType
        {
            get
            {
                return _DrawPathTools.CurrArrowPathType;
            }
        }

        public ArrowPathType CurrCloneArrowPathType
        {
            get
            {
                return _CloneDrawPathTools.CurrArrowPathType;
            }
        }

        public DrawPathTools _DrawPathTools
        {
            get
            {
                return mDrawPathTools;
            }

            set
            {
                mDrawPathTools = value;
            }
        }

        public DrawPathTools _CloneDrawPathTools
        {
            get
            {
                return mCloneDrawPathTools;
            }

            set
            {
                mCloneDrawPathTools = value;
            }
        }


        public SceneMoundInstance StopDrawing(UnityAction<SceneMoundInstance> action)
        {
            if (DrawingState.Doing == GetState())
            {
                if (mDrawPathTools.goCurrents[CurrArrowPathType])
                {
                    if (mDrawPathTools.goCurrents[CurrArrowPathType])
                    {
                        SceneMoundInstance instance = mDrawPathTools.goCurrents[CurrArrowPathType].GetComponent<SceneMoundInstance>();

                        switch (CurrArrowPathType)
                        {
                            case ArrowPathType.Cordon:
                            {
                                instance.ItemID = 2103;
                            }
                                break;
                            case ArrowPathType.EvacuatePath:
                            {
                                instance.ItemID = 2105;
                            }
                                break;
                            case ArrowPathType.RescuePath:
                            {
                                instance.ItemID = 2104;
                            }
                                break;

                        }

                        instance.OnPostRegister();

                        Transform parent = null;
                        switch (CurrArrowPathType)
                        {
                            case ArrowPathType.Cordon:
                                parent = m_cordonRoot.transform;
                                break;
                            case ArrowPathType.EvacuatePath:
                                parent = m_evacuateRoot.transform;
                                break;
                            case ArrowPathType.RescuePath:
                                parent = m_rescueRoot.transform;
                                break;
                            default:
                                break;
                        }

                        if (parent)
                            mDrawPathTools.goCurrents[CurrArrowPathType].transform.parent = parent;

                        mDrawPathTools.DrawState = DrawingState.None;
                        mDrawPathTools.LastPoint = Vector3.zero;
                        mDrawPathTools.OrigionPoint = Vector3.zero;

                        SetColiderActive(true);

                        action?.Invoke(instance);
                        return instance;
                    }

                }
            }

            return null;
        }

        public void ClearDrawTools()
        {
            mDrawPathTools.DrawState = DrawingState.None;
            mDrawPathTools.LastPoint = Vector3.zero;
            mDrawPathTools.OrigionPoint = Vector3.zero;
        }

        public void DelArrowNode()
        {
            if (m_SelectNode == null)
                return;

            Transform parent = m_SelectNode.transform.parent;
            Transform pParent = parent.parent;

            for (int i = 0, j = 0; i < pParent.childCount; i++)
            {

                if (pParent.GetChild(i).name == parent.name)
                {
                    continue;
                }
                else
                {
                    pParent.GetChild(i).name = j.ToString();
                    j++;
                }
            }
            Object.Destroy(parent.gameObject);
            m_SelectNode = null;
        }

        private void SetColiderActive(bool isActive)
        {
            for (int i = 0; i < Root.transform.childCount; i++)
            {
                Transform root = Root.transform.GetChild(i);
                for (int j = 0; j < root.childCount; j++)
                {
                    SceneMoundInstance tmp = root.GetChild(j).GetComponent<SceneMoundInstance>();
                    tmp?.SetColliderActive(isActive);
                }
            }
        }

        #region 去除Mono优化

        public new static string NAME = "DrawArrowProxy";

        private ArrowProxyInfo m_ArrowProxyInfo;

        public DrawArrowProxy(ArrowProxyInfo info) : base(NAME)
        {
            m_ArrowProxyInfo = info;
        }

        public override void OnRegister()
        {
            base.OnRegister();

            m_arrowPrefabPath = "SceneObject/Arrow";
            m_cordonPrefab = Resources.Load<GameObject>("SceneObject/TY_JJXDZ");
            CreateRootNode();

            mDrawPathTools = GetNewDrawTools();
            mCloneDrawPathTools = GetNewDrawTools();

            Facade.Instance.RegisterObservers(this, (ushort)EnDarwArrowEvent.SyncAddDrawnArrow, SyncAddDrawnArrow);
            Facade.Instance.RegisterObservers(this, (ushort)EnDarwArrowEvent.OnAddDrawnArrow, OnEzActiveDrawnArrow);

            InitEzReplayInfo();
        }

        public override void OnRemove()
        {
            Facade.Instance.RemoveObservers(this, (ushort)EnDarwArrowEvent.SyncAddDrawnArrow);
            Facade.Instance.RemoveObservers(this, (ushort)EnDarwArrowEvent.OnAddDrawnArrow);

            if (m_rescueRoot != null)
                Object.Destroy(m_rescueRoot);
            m_rescueRoot = null;

            if (m_evacuateRoot != null)
                Object.Destroy(m_evacuateRoot);
            m_evacuateRoot = null;

            if (m_cordonRoot != null)
                Object.Destroy(m_cordonRoot);
            m_cordonRoot = null;

            m_arrowPrefabPath = null;
            m_cordonPrefab = null;
            //note:not primited
            /*if (m_cordonPrefab != null)
                Object.Destroy(m_cordonPrefab);*/

            m_SelectNode = null;

            mDrawPathTools = null;
            mCloneDrawPathTools = null;
            m_ArrowProxyInfo = null;
            base.OnRemove();

        }

        public List<PathVertexesInfo> GetAllCacheInfo()
        {
            return m_ArrowProxyInfo.arrowCache;
        }

        #endregion

        #region Internal callback

        /// <summary>
        /// 网络同步
        /// </summary>
        /// <param name="notification"></param>
        private void SyncAddDrawnArrow(INotification notification)
        {
            CSRoutePlanResult result = notification.Body as CSRoutePlanResult;
            var pathVertexesInfo = result.routeInfo[0];
            if (pathVertexesInfo == null)
                return;

            //ConvertAll
            PathVertexesInfo info = new PathVertexesInfo()
            {
                PathName = pathVertexesInfo.PathName,
                PathType = pathVertexesInfo.PathType,
                Vertexes = pathVertexesInfo.Vertexes.ConvertAll((delegate (SSIT.proto.PBVector3 input)
                {
                    return new SerVector3(input.x, input.y, input.z);
                }))
            };

            OnAddDrawnArrow(info);
            m_ArrowProxyInfo.AddArrowInfo(info);
        }

        /// <summary>
        /// 划线回调
        /// </summary>
        private void OnAddDrawnArrow(PathVertexesInfo info)
        {
            //绘制结束消息
            if (GlobalManager.Instance.ReplayMode != ActionMode.PLAY)
            {
                if (ObjectManager.Instance.GetObject(info.PathName) != null)
                    return;
            }

            _CloneDrawPathTools.Init((ArrowPathType)info.PathType, DrawingState.Start);

            // 添加点
            foreach (var v in info.Vertexes)
            {
                _CloneDrawPathTools.AddPoint(v);
            }

            // 修改名字
            //_CloneDrawPathTools.goCurrents[(ArrowPathType)info.PathType].name = info.PathName;
            //m_proxy._CloneDrawPathTools.goCurrents[(ArrowPathType)pathVertexesInfo.PathType].GetComponentInChildren<Mound>().InitGuidRe(pathVertexesInfo.PathName);
            // 关闭绘制
            _CloneDrawPathTools.StopDrawing(info.PathName);

            //加入代理缓存
            info.SetAgent(_CloneDrawPathTools.goCurrents[CurrCloneArrowPathType]);
        }

        #endregion

        #region 回放写入

        public override string Guid
        {
            get { return NAME; }
            set { }
        }

        public override void InitEzReplay()
        {
            m_present = new GameObject(NAME);
            m_present.transform.SetParent(GlobalManager.Instance.transform);
        }


        private void InitEzReplayInfo()
        {
            //回放模式
            if (GlobalManager.Instance.ReplayMode == ActionMode.PLAY)
            {
                for (int i = 0; i < m_ArrowProxyInfo.arrowCache.Count; i++)
                {
                    var info = m_ArrowProxyInfo.arrowCache[i];
                    OnAddDrawnArrow(m_ArrowProxyInfo.arrowCache[i]);
                    info.GetAgent()?.SetActive(false);
                }
            }
        }


        public override SavedBase GeneralSaveData(bool isDeepClone = false)
        {
            var ret = m_ArrowProxyInfo;
            if (isDeepClone)
            {
                var temp = SerializationUtils.Clone(ret);
                ret.IsChange = false;
                return temp;
            }
            return ret;
        }

        public override void SynchronizeProperties(SavedBase savedState, bool isReset, bool isFristFrame)
        {
            ArrowProxyInfo info = savedState as ArrowProxyInfo;

            if (info == null || m_ArrowProxyInfo == null)
                return;

            if (m_ArrowProxyInfo.cacheIndex != info.cacheIndex)
            {
                m_ArrowProxyInfo.cacheIndex = info.cacheIndex;

                //同步数据
                for (int i = 0; i < m_ArrowProxyInfo.arrowCache.Count; i++)
                {
                    var temp = m_ArrowProxyInfo.arrowCache[i];

                    if (i < m_ArrowProxyInfo.cacheIndex)
                    {
                        if (temp != null && temp.GetAgent())
                        {
                            temp.GetAgent().SetActive(true);
                        }
                    }
                    else
                    {
                        if (temp != null && temp.GetAgent())
                        {
                            temp.GetAgent().SetActive(false);
                        }
                    }
                }
            }




            //todo:sync data
        }


        private void OnEzActiveDrawnArrow(INotification obj)
        {
            MvEventArgs atArgs = obj as MvEventArgs;
            var temp = m_ArrowProxyInfo.arrowCache.Find(x => x.PathName == (string) atArgs.Body);

            if (temp != null)
            {
                temp.GetAgent().SetActive(atArgs.BoolValue);
            }
        }


        #endregion

    }

    public class DrawPathTools
    {
        private ArrowPathType m_pathType = ArrowPathType.EvacuatePath;
        private DrawingState m_drawingState = DrawingState.None;

        //private Dictionary<int, PathNode> m_rescueTree;
        //private Dictionary<int, PathNode> m_evacuateTree;

        //public Dictionary<ArrowPathType, Dictionary<string, List<Vector3>>> PathsPoints = null;
        public Dictionary<ArrowPathType, List<Vector3>> CurrentVertexes = null;     // 当前路径的顶点
        public Dictionary<ArrowPathType, GameObject> goCurrents = new Dictionary<ArrowPathType, GameObject>();

        private Vector3 m_lastPoint = Vector3.zero;
        private Vector3 m_origionPoint = Vector3.zero;

        private GameObject m_rescueRoot;
        private GameObject m_evacuateRoot;
        private GameObject m_cordonRoot;

        private string m_arrowPrefab;
        private GameObject m_cordonPrefab;
        private DrawArrowProxy m_proxy;

        public DrawingState DrawState
        {
            get
            {
                return m_drawingState;

            }
            set
            {
                m_drawingState = value;
            }
        }

        public ArrowPathType CurrArrowPathType
        {
            get
            {
                return m_pathType;
            }
            set
            {
                m_pathType = value;
            }
        }

        public Vector3 LastPoint
        {
            get
            {
                return m_lastPoint;
            }

            set
            {
                m_lastPoint = value;
            }
        }

        public Vector3 OrigionPoint
        {
            get
            {
                return m_origionPoint;
            }

            set
            {
                m_origionPoint = value;
            }
        }

        public void Init(DrawArrowProxy proxy, GameObject rescueRoot, GameObject evacuateRoot, GameObject cordonRoot,
           string arrowPrefabPath, GameObject cordonPrefab)
        {
            m_proxy = proxy;
            this.m_rescueRoot = rescueRoot;
            this.m_evacuateRoot = evacuateRoot;
            this.m_cordonRoot = cordonRoot;
            this.m_arrowPrefab = arrowPrefabPath;
            this.m_cordonPrefab = cordonPrefab;

            CurrentVertexes = new Dictionary<ArrowPathType, List<Vector3>>();
            CurrentVertexes[ArrowPathType.RescuePath] = new List<Vector3>();
            CurrentVertexes[ArrowPathType.EvacuatePath] = new List<Vector3>();
            CurrentVertexes[ArrowPathType.Cordon] = new List<Vector3>();

            // 2018-08-11 18:10:19 Shell Lee
            //PathsPoints = new Dictionary<ArrowPathType, Dictionary<string, List<Vector3>>>();
            //PathsPoints[ArrowPathType.RescuePath] = new Dictionary<string, List<Vector3>>();
            //PathsPoints[ArrowPathType.EvacuatePath] = new Dictionary<string, List<Vector3>>();
            //PathsPoints[ArrowPathType.Cordon] = new Dictionary<string, List<Vector3>>();
        }

        public void Init(ArrowPathType cType, DrawingState state)
        {

            goCurrents[cType] = null;
            CurrentVertexes[cType].Clear();
            CurrArrowPathType = cType;
            DrawState = state;
            //干掉colider
        }

        public void AddPoint(Vector3 vec)
        {
            if (m_drawingState == DrawingState.Start)
            {
                m_drawingState = DrawingState.Doing;
            }
            else if (m_drawingState == DrawingState.End || m_drawingState == DrawingState.None)
            {
                return;
            }
            switch (m_pathType)
            {
                case ArrowPathType.EvacuatePath:
                    {

                        if (LastPoint == Vector3.zero)
                        {
                            // firstPoint
                            LastPoint = vec;
                            OrigionPoint = vec;

                            ////初始化当前路线节点
                            goCurrents[ArrowPathType.EvacuatePath] = new GameObject(m_evacuateRoot.transform.childCount.ToString());
                            SceneMoundInstance mm = goCurrents[ArrowPathType.EvacuatePath].AddComponent<SceneMoundInstance>();
                            mm.IsSmartMove = false;
                            //mm.Init();
                            //mm.InitDataRe(2105);

                            //goCurrents[ArrowPathType.EvacuatePath].name = mm.Guid;
                            //AppFacade.Instance.SendNotification( ConstNotification.c_sOnAddObjectInScene, goCurrents[ArrowPathType.EvacuatePath] );
                            goCurrents[ArrowPathType.EvacuatePath].transform.SetParent(m_evacuateRoot.transform);
                            goCurrents[ArrowPathType.EvacuatePath].transform.localPosition = vec;

                            //PathsPoints[ArrowPathType.EvacuatePath][goCurrents[ArrowPathType.EvacuatePath].name] = new List<Vector3>();
                            //CurrentVertexes[ArrowPathType.EvacuatePath].Clear();
                        }
                        else
                        {
                            CreateNode(LastPoint, vec);
                            //LastPoint = vec;
                        }

                        //PathsPoints[ArrowPathType.EvacuatePath][goCurrents[ArrowPathType.EvacuatePath].name].Add(vec);
                    }
                    break;
                case ArrowPathType.RescuePath:
                    {
                        if (LastPoint == Vector3.zero)
                        {
                            // firstPoint
                            LastPoint = vec;
                            OrigionPoint = vec;

                            goCurrents[ArrowPathType.RescuePath] = new GameObject(m_rescueRoot.transform.childCount.ToString());
                            SceneMoundInstance mm = goCurrents[ArrowPathType.RescuePath].AddComponent<SceneMoundInstance>();
                            mm.IsSmartMove = false;

                            //mm.Init();
                            //mm.InitDataRe(2104);
                            //goCurrents[ArrowPathType.RescuePath].name = mm.Guid;
                            //AppFacade.Instance.SendNotification( ConstNotification.c_sOnAddObjectInScene, goCurrents[ArrowPathType.RescuePath] );

                            goCurrents[ArrowPathType.RescuePath].transform.SetParent(m_rescueRoot.transform);
                            goCurrents[ArrowPathType.RescuePath].transform.localPosition = vec;

                            //PathsPoints[ArrowPathType.RescuePath][goCurrents[ArrowPathType.RescuePath].name] = new List<Vector3>();
                            //CurrentVertexes[ArrowPathType.RescuePath].Clear();
                        }
                        else
                        {
                            CreateNode(LastPoint, vec);
                            //LastPoint = vec;
                        }

                        //PathsPoints[ArrowPathType.RescuePath][goCurrents[ArrowPathType.RescuePath].name].Add(vec);
                    }
                    break;

                case ArrowPathType.Cordon:
                    {
                        if (LastPoint == Vector3.zero)
                        {
                            // firstPoint
                            LastPoint = vec;
                            OrigionPoint = vec;

                            goCurrents[ArrowPathType.Cordon] = new GameObject(m_cordonRoot.transform.childCount.ToString());
                            SceneMoundInstance mm = goCurrents[ArrowPathType.Cordon].AddComponent<SceneMoundInstance>();
                            mm.IsSmartMove = false;

                            //mm.Init();
                            //mm.InitDataRe(2103);
                            //goCurrents[ArrowPathType.Cordon].name = mm.Guid;
                            //AppFacade.Instance.SendNotification( ConstNotification.c_sOnAddObjectInScene, goCurrents[ArrowPathType.Cordon] );

                            goCurrents[ArrowPathType.Cordon].transform.SetParent(m_cordonRoot.transform);
                            goCurrents[ArrowPathType.Cordon].transform.localPosition = vec;

                            GameObject fNode = UnityEngine.Object.Instantiate(m_cordonPrefab) as GameObject;
                            fNode.transform.SetParent(goCurrents[ArrowPathType.Cordon].transform);
                            fNode.transform.localPosition = Vector3.zero;
                            fNode.transform.GetChild(0).gameObject.SetActive(false);
                            BaseSceneInstance script = fNode.GetComponent<BaseSceneInstance>();
                            if (script)
                                Object.Destroy(script);

                            //List<Collider> colList = new List<Collider>();
                            //Collider col1 = fNode.GetComponent<Collider>();
                            //Collider col2 = fNode.transform.GetChild(0).GetComponent<Collider>();
                            //colList.Add(col1);
                            //colList.Add(col2);
                            //col1.enabled = false;

                            //col2.enabled = false;
                            //goCurrents[ArrowPathType.Cordon].GetComponent<Mound>().AddColliderList(colList);

                            //PathsPoints[ArrowPathType.Cordon][goCurrents[ArrowPathType.Cordon].name] = new List<Vector3>();
                            //CurrentVertexes[ArrowPathType.Cordon].Clear();

                        }
                        else
                        {
                            CreateNode(vec, LastPoint);
                            LastPoint = vec;
                        }

                        //PathsPoints[ArrowPathType.Cordon][goCurrents[ArrowPathType.Cordon].name].Add(vec);
                    }
                    break;

                default:
                    {

                    }
                    break;
            }
            CurrentVertexes[CurrArrowPathType].Add(vec);
        }

        private void CreateNode(Vector3 start, Vector3 end)
        {
            Vector3 vec = end - start;
            Vector3 direction = Vector3.Normalize(vec);

            float angle = Vector3.Angle(-1 * Vector3.forward, direction); //求出两向量之间的夹角
            Vector3 normal = Vector3.Cross(-1 * Vector3.forward, direction);//叉乘求出法线向量
            angle *= Mathf.Sign(Vector3.Dot(normal, Vector3.up));  //求法线向量与物体上方向向量点乘，结果为1或-1，修正旋转方向

            //计算长度
            float length = Vector3.Distance(start, end);
            int number = Mathf.CeilToInt(length / 1.5f);
            if (CurrArrowPathType != ArrowPathType.Cordon)
            {
                LastPoint = start + direction * number * 1.5f;
            }

            List<Collider> colList = new List<Collider>(number);
            switch (m_pathType)
            {
                case ArrowPathType.RescuePath:
                    {
                        for (int i = 0; i < number; i++)
                        {
                            //access:资源加载接入
                            GameObject go = Resources.Load<GameObject>(m_arrowPrefab);
                            //GameObject go = ResourcesManager.Instance.LoadAsset<GameObject>(m_arrowPrefab);
                            go.name = "rescueNode";
                            BaseSceneInstance script = go.GetComponent<BaseSceneInstance>();
                            if (script)
                                Object.Destroy(script);
                            go.transform.SetParent(goCurrents[ArrowPathType.RescuePath].transform);
                            go.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.red;
                            go.GetComponent<Collider>().enabled = false;
                            //  go.transform.GetChild(1).gameObject.SetActive(true);
                            // colList.Add(go.GetComponent<Collider>());

                            // 高度
                            go.transform.localPosition = (start - OrigionPoint) + direction * 1.5f * i ;
                            //+Vector3.up * 0.2f;
                            Vector3 target = Vector3.zero;
                            if (Mathf.Abs(end.y - start.y) > 2f)
                            {
                                target = new Vector3(end.x, end.y, end.z);
                            }
                            else
                            {
                                target = new Vector3(end.x, go.transform.position.y, end.z);
                            }
                            go.transform.LookAt(target);

                            // 移动动画
                            //Tweener tween = go.transform.GetChild( 0 ).GetComponent<Renderer>().material.DOFade( 0.2f, 0.8f );
                            //tween.SetDelay( i * 0.05f );
                            //tween.SetEase( Ease.Linear );
                            //tween.SetLoops( -1, LoopType.Yoyo );
                        }

                        //Mound tmp = goCurrents[ArrowPathType.RescuePath].GetComponent<Mound>();
                        //tmp.AddColliderList(colList);
                    }
                    break;
                case ArrowPathType.EvacuatePath:
                    {
                        for (int i = 0; i < number; i++)
                        {
                            //access:资源加载接入
                            GameObject go = Resources.Load<GameObject>(m_arrowPrefab);
                            //GameObject go = ResourcesManager.Instance.LoadAsset<GameObject>(m_arrowPrefab);
                            go.name = "evacuateNode";
                            BaseSceneInstance script = go.GetComponent<BaseSceneInstance>();
                            if (script)
                                Object.Destroy(script);
                            go.transform.SetParent(goCurrents[ArrowPathType.EvacuatePath].transform);
                            go.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.green;
                            //go.GetComponent<Collider>().enabled = false;
                            //go.transform.GetChild(0).gameObject.SetActive(true);
                            //colList.Add(go.GetComponent<Collider>());

                            go.transform.localPosition = (start - OrigionPoint) + direction * 1.5f * i ;//+ Vector3.up * 0.2f
                            Vector3 target = Vector3.zero;
                            if (Mathf.Abs(end.y - start.y) > 2f)
                            {
                                target = new Vector3(end.x, end.y, end.z);
                            }
                            else
                            {
                                target = new Vector3(end.x, go.transform.position.y, end.z);
                            }
                            go.transform.LookAt(target);

                            // 移动动画
                            //Tweener tween = go.transform.GetChild(0).GetComponent<Renderer>().material.DOFade(0.2f, 0.8f);
                            //tween.SetDelay(i * 0.05f);
                            //tween.SetEase(Ease.Linear);
                            //tween.SetLoops(-1, LoopType.Yoyo);
                        }

                        //Mound tmp = goCurrents[ArrowPathType.EvacuatePath].GetComponent<Mound>();
                        //tmp.AddColliderList(colList);
                    }
                    break;
                case ArrowPathType.Cordon:
                    {
                        //
                        float ang = Vector3.Angle(Vector3.right, direction); //求出两向量之间的夹角
                        Vector3 nor = Vector3.Cross(Vector3.right, direction);//叉乘求出法线向量
                        ang *= Mathf.Sign(Vector3.Dot(nor, Vector3.up));  //求法线向量与物体上方向向量点乘，结果为1或-1，修正旋转方向


                        GameObject node = UnityEngine.Object.Instantiate(m_cordonPrefab) as GameObject;
                        node.name = "cordonNode";
                        BaseSceneInstance script = node.GetComponent<BaseSceneInstance>();
                        if (script)
                            Object.Destroy(script);

                        node.transform.SetParent(goCurrents[ArrowPathType.Cordon].transform);
                        node.transform.localPosition = start - OrigionPoint;
                        node.transform.localRotation = Quaternion.Euler(0, ang, 0);
                        node.transform.GetChild(0).gameObject.SetActive(true);

                        node.transform.GetChild(0).transform.localScale = new Vector3(length, 0.1f, 1f);

                        Mesh mesh = node.transform.GetChild(0).GetChild(0).GetComponent<MeshFilter>().mesh;

                        Vector2[] tmpUV = mesh.uv;
                        Vector2[] uvs = new Vector2[tmpUV.Length];
                        for (int i = 0; i < tmpUV.Length; i++)
                        {
                            uvs[i] = new Vector2(tmpUV[i].x * length, tmpUV[i].y);
                        }

                        mesh.uv = uvs;

                        //Collider col1 = node.GetComponent<Collider>();
                        //Collider col2 = node.transform.GetChild(0).GetComponent<Collider>();


                        //colList.Add(col1);
                        //colList.Add(col2);
                        //col1.enabled = false;

                        //col2.enabled = false;
                        //Mound tmp = goCurrents[ArrowPathType.Cordon].GetComponent<Mound>();
                        //tmp.AddColliderList(colList);
                    }
                    break;
                default:
                    break;
            }
        }

        public void StopDrawing(string uuid)
        {
            if (DrawState == DrawingState.Doing)
            {
                if (goCurrents[CurrArrowPathType])
                {
                    if (goCurrents[CurrArrowPathType])
                    {
                        SceneMoundInstance instance = goCurrents[CurrArrowPathType].GetComponent<SceneMoundInstance>();

                        switch (CurrArrowPathType)
                        {
                            case ArrowPathType.Cordon:
                                {
                                    instance.ItemID = 2103;
                                }
                                break;
                            case ArrowPathType.EvacuatePath:
                                {
                                    instance.ItemID = 2105;
                                }
                                break;
                            case ArrowPathType.RescuePath:
                                {
                                    instance.ItemID = 2104;
                                }
                                break;
                            default:
                                break;

                        }

                        instance.Guid = uuid;
                        instance.OnPostRegister();

                        Transform parent = null;
                        switch (CurrArrowPathType)
                        {
                            case ArrowPathType.Cordon:
                                parent = m_cordonRoot.transform;
                                break;
                            case ArrowPathType.EvacuatePath:
                                parent = m_evacuateRoot.transform;
                                break;
                            case ArrowPathType.RescuePath:
                                parent = m_rescueRoot.transform;
                                break;
                            default:
                                break;
                        }

                        if (parent)
                            goCurrents[CurrArrowPathType].transform.parent = parent;


                        DrawState = DrawingState.None;
                        LastPoint = Vector3.zero;
                        OrigionPoint = Vector3.zero;
                    }

                }
            }

        }
    }
}