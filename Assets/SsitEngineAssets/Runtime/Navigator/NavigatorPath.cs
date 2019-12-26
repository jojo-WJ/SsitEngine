using UnityEngine;
using System.Collections.Generic;

namespace Framework.Navigator
{
    public class NavigatorPath
    {
        #region static method

        /*//all the full path, constructed in runtime
        private static List<NavigatorPath> individualPathList = new List<NavigatorPath>();

        //all the full path, constructed in runtime
        private static List<List<NavigatorPath>> completePathList = new List<List<NavigatorPath>>();

        public static List<List<NavigatorPath>> GetCompletePathList()
        {
            return completePathList;
        }


        public static void Init()
        {
            //setup all possible path combination in runtime
            individualPathList = new List<NavigatorPath>();
            completePathList = new List<List<NavigatorPath>>();
            NavigatorPath[] pathList = FindObjectsOfType(typeof(NavigatorPath)) as NavigatorPath[];
            for (int i = 0; i < pathList.Length; i++)
            {
                pathList[i]._Init();
                individualPathList.Add(pathList[i]);
            }

            for (int i = 0; i < pathList.Length; i++)
            {
                if (!pathList[i].IsStart()) continue;
                pathList[i].SetupLinkRecursively();
            }
        }

        public void SetupLinkRecursively(List<NavigatorPath> list = null)
        {
            if (list == null) list = new List<NavigatorPath>();
            list.Add(this);
            if (nextPath.Count > 0)
            {
                for (int i = 0; i < nextPath.Count; i++)
                {
                    if (nextPath[i] == this) continue;
                    nextPath[i].SetupLinkRecursively(new List<NavigatorPath>(list));
                }
            }
            else
            {
                completePathList.Add(list);

#if UNITY_EDITOR
                string text = "";
                for (int i = 0; i < list.Count; i++)
                {
                    text += list[i].gameObject.name + "   ";
                }

                Debug.Log(completePathList.Count + "  " + text);
#endif
            }
        }

        public static List<NavigatorPath> GetAllStartingPath()
        {
            //called by SpawnManager only, to retrive all starting path
            List<NavigatorPath> list = new List<NavigatorPath>();

            if (!Application.isPlaying)
            {
                //for SpawnGenerator to generate wave not in runtime
                NavigatorPath[] pathList = FindObjectsOfType(typeof(NavigatorPath)) as NavigatorPath[];
                for (int i = 0; i < pathList.Length; i++)
                {
                    bool hasParent = false;
                    for (int n = 0; n < pathList.Length; n++)
                    {
                        hasParent = (i != n & pathList[n].nextPath.Contains(pathList[i]));
                        if (hasParent) break;
                    }

                    if (!hasParent) list.Add(pathList[i]);
                }
            }
            else
                for (int i = 0; i < completePathList.Count; i++)
                    list.Add(completePathList[i][0]);

            return list;
        }


        public static List<NavigatorPath> GetAllStartingPathOfPath(NavigatorPath path, List<NavigatorPath> pList = null)
        {
            if (pList == null) pList = new List<NavigatorPath>();

            if (path.IsStart()) pList.Add(path);
            else
            {
                for (int i = 0; i < path.prevPath.Count; i++)
                {
                    pList = GetAllStartingPathOfPath(path.prevPath[i], pList);
                }
            }

            return pList;
        }

        //ignorelist is used when the function is used for determining the availability of the alternate path
        public static bool HasValidDestination(NavigatorPath path, List<NavigatorPath> ignoreList = null)
        {
            if (ignoreList == null) ignoreList = new List<NavigatorPath>();

            if (path.IsBlocked()) return false;

            if (path.IsEnd()) return true;

            for (int i = 0; i < path.nextPath.Count; i++)
            {
                if (ignoreList.Contains(path.nextPath[i])) continue;
                if (HasValidDestination(path.nextPath[i], ignoreList)) return true;
            }

            return false;
        }


        //check if a path has an alternate route, used to determine if a platform can/cannot be blocked entirely
        //ignore list is other path that uses the same platform, they are ignore in this check since they will be going through the same check
        public static bool HasAlternatePath(NavigatorPath path, List<NavigatorPath> ignoreList = null)
        {
            if (path.IsStart()) return false;

            for (int i = 0; i < path.prevPath.Count; i++)
            {
                NavigatorPath parentPath = path.prevPath[i];
                for (int n = 0; n < parentPath.nextPath.Count; n++)
                {
                    if (parentPath.nextPath[n] == path) continue;
                    if (ignoreList != null && ignoreList.Contains(parentPath.nextPath[n])) continue;
                    if (parentPath.nextPath[n].hasValidDestination &&
                        !parentPath.nextPath[n].IsEntryBlocked(parentPath))
                    {
                        if (HasValidDestination(parentPath.nextPath[n], ignoreList)) return true;
                    }
                }

                if (HasAlternatePath(parentPath, ignoreList)) return true;
            }

            return false;
        }
*/

  
        #endregion


        public int nextShortestIdx = 0;

        public NavigatorPath GetNextShortestPath()
        {
            return nextPath[nextShortestIdx];
        }

        //correspond to each prevPath
        private List<bool> blockedEntryPoint = new List<bool>();
        public List<int> blockedSec = new List<int>();
        public bool hasValidDestination = true;

        public float dynamicOffset = 0;

        public List<Vector3> waypointList = new List<Vector3>();

        public Vector3 GetFirstPoint()
        {
            return waypointList[0];
        }

        public Vector3 GetLastPoint()
        {
            return waypointList[waypointList.Count - 1];
        }


        public List<Vector3> GetAllWaypointList()
        {
            return waypointList;
        }

        private List<NavigatorPath> prevPath = new List<NavigatorPath>();
        public List<NavigatorPath> nextPath = new List<NavigatorPath>();

        public bool IsStart()
        {
            return prevPath.Count == 0 ? true : false;
        }

        public bool IsEnd()
        {
            return nextPath.Count == 0 ? true : false;
        }


        public void _Init()
        {
            dynamicOffset = Mathf.Min(dynamicOffset, 0.45f);

            for (int i = 0; i < nextPath.Count; i++)
            {
                if (nextPath[i] == null)
                {
                    nextPath.RemoveAt(i);
                    i -= 1;
                    continue;
                }

                nextPath[i].prevPath.Add(this);
                nextPath[i].blockedEntryPoint.Add(false);
            }
        }


        public void RemoveBlockedEntry(NavigatorPath prevP)
        {
            blockedEntryPoint[prevPath.IndexOf(prevP)] = false;
        }

        public void AddBlockedEntry(NavigatorPath prevP)
        {
            blockedEntryPoint[prevPath.IndexOf(prevP)] = true;
        }

        public bool IsEntryBlocked(NavigatorPath prevP)
        {
            return blockedEntryPoint[prevPath.IndexOf(prevP)];
        }

        public void AddBlockSec(int idx)
        {
            if (!blockedSec.Contains(idx)) blockedSec.Add(idx);
        }

        public void RemoveBlockSec(int idx)
        {
            blockedSec.Remove(idx);
        }

        public bool IsBlocked()
        {
            return blockedSec.Count > 0;
        }

        public bool IsSecBlocked(int idx)
        {
            return blockedSec.Contains(idx);
        }

  
    }
}