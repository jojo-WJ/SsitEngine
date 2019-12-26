using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Navigator
{
    public class NavigatorPathIndicator : MonoBehaviour
    {
        public NavigatorPath path;
        private NavigatorPath nextPath;

        public LineRenderer rend;
        public GameObject endParticle;

        [HideInInspector]
        public float scrollSpeed = 0.5F;

        public int guid = 0;

        public bool enableUVAnimation = true;
        #region Mono Members

        void OnEnable()
        {
            //todo:外部刷新接口 Addlistener UpdatePath

        }

        void OnDisable()
        {
            //todo:外部刷新接口 Removelistener UpdatePath
            if (endParticle)
            {
                endParticle.SetActive(false);
            }
        }

        void Update()
        {
            if (!enableUVAnimation)
            {
                return;
            }
            if (rend != null && rend.positionCount > 1)
            {
                float offset = Time.time * -scrollSpeed;
                rend.material.mainTextureOffset = new Vector2(offset, 0);
            }
            //rend.material.mainTextureScale=new Vector2(dist*0.2f, 1);
        }

        #endregion

        #region Internal Members



        public void UpdatePath(Vector3[] point)
        {
            if (point != null && point.Length > 0)
            {
                rend.positionCount = point.Length;
                rend.SetPositions(point);
                if (endParticle)
                {
                    endParticle.SetActive(true);
                    endParticle.transform.position = point[point.Length - 1];
                }
            }
        }

        public void ResetPath()
        {
            rend.positionCount = 0;
            //rend.SetPositions(point);
        }

        public void SetPath(Vector3[] point, float width)
        {
            if (point != null && point.Length > 0)
            {
                rend.positionCount = point.Length;
                rend.SetPositions(point);
                //if (endParticle)
                //{
                //    endParticle.SetActive(true);
                //    endParticle.transform.position = point[point.Length - 1];
                //}
                rend.startWidth = width;
                rend.endWidth = width;
            }
        }

        public void AddPath(Vector3 point, float width)
        {
            var count = rend.positionCount;
            var vector3s = new List<Vector3>();

            for (int i = 0; i < count; i++)
            {
                vector3s.Add(rend.GetPosition(i));
            }
            vector3s.Add(point);


            rend.positionCount = vector3s.Count;
            rend.SetPositions(vector3s.ToArray());
            //if (endParticle)
            //{
            //    endParticle.SetActive(true);
            //    endParticle.transform.position = point[point.Length - 1];
            //}
            rend.startWidth = width;
            rend.endWidth = width;
        }

        public void SetWidth(float width)
        {
            rend.startWidth = width;
            rend.endWidth = width;
        }

        public void DrawBegin(Vector3 point)
        {
            rend.positionCount = 1;
            var vector3s = new List<Vector3>();
            vector3s.Add(point);
            rend.SetPositions(vector3s.ToArray());

        }

        public void DrawEnd(Vector3 point)
        {
            if (rend.positionCount == 0)
                return;
            rend.positionCount = 2;
            var vector3s = new List<Vector3>();
            vector3s.Add(rend.GetPosition(0));
            vector3s.Add(point);
            rend.SetPositions(vector3s.ToArray());
        }

        public void UpdatePath(Object tower = null)
        {
            StartCoroutine(_UpdatePath());
        }

        IEnumerator _UpdatePath()
        {
            //wait 2 frames to make sure the new path has been established
            yield return null;
            yield return null;

            //float dist=path.GetDistance();
            List<Vector3> allPosList = path.GetAllWaypointList();

            if (!path.IsEnd())
            {
                nextPath = path.GetNextShortestPath();
                while (nextPath != null)
                {
                    //dist+=nextPath.GetDistance();
                    List<Vector3> newList = nextPath.GetAllWaypointList();
                    for (int i = 0; i < newList.Count; i++)
                    {
                        allPosList.Add(newList[i]);
                    }

                    if (!nextPath.IsEnd())
                    {
                        nextPath = nextPath.GetNextShortestPath();
                    }
                    else
                    {
                        nextPath = null;
                    }
                }
            }

            rend.positionCount = allPosList.Count;
            rend.SetPositions(allPosList.ToArray());

            //yield return null;
        }

        #endregion

        //have path connect to path
        //end of path1 can be connect to path2.1, and path2.2, and path2.3 and so on to branch out
        //end of path2.n can be connected to start path3 (rejoin), or just goes on as independent path
        [Space(10)]
        public bool showGizmo = true;
        public Color gizmoColor = Color.blue;

        private float pointRadius = .1f;

    }
}