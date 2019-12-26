using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SsitEngine.EzReplay;
using UnityEngine;

namespace Framework.Data
{
    /// <summary>
    /// 路线信息
    /// </summary>
    [Serializable]
    public class ArrowProxyInfo : InfoData, ISerializable
    {
        public List<PathVertexesInfo> arrowCache = new List<PathVertexesInfo>();
        public int cacheIndex;


        public void AddArrowInfo( PathVertexesInfo info )
        {
            if (arrowCache == null) arrowCache = new List<PathVertexesInfo>();
            arrowCache.Add(info);
            cacheIndex = arrowCache.Count;
            isChange = true;
        }


        #region 回放

        public override bool IsDifferentTo( SavedBase state, Object2PropertiesMapping o2m )
        {
            return isChange;
        }

        #endregion


        #region 序列化

        public ArrowProxyInfo()
        {
        }

        public ArrowProxyInfo( SerializationInfo info, StreamingContext context )
        {
            cacheIndex = info.GetInt32("1");
        }

        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue("1", cacheIndex);
        }

        #endregion
    }

    /// <summary>
    /// 路径点
    /// </summary>
    [Serializable]
    public class PathVertexesInfo : InfoData, ISerializable
    {
        public bool enable; //路线代理 
        private GameObject mAgentRoot; //路线代理
        public string PathName; // 路线名字
        public int PathType; // 路线种类
        public List<SerVector3> Vertexes; // 路线顶点

        public void SetAgent( GameObject profile )
        {
            mAgentRoot = profile;
        }

        public GameObject GetAgent()
        {
            return mAgentRoot;
        }
        //public override SavedBase Clone()
        //{
        //    return SerializationUtils.Clone<PathVertexesInfo>(this);
        //}

        #region 序列化

        public PathVertexesInfo()
        {
        }

        public PathVertexesInfo( SerializationInfo info, StreamingContext context )
        {
            PathType = info.GetInt32("1");
            PathName = info.GetString("2");
            Vertexes = (List<SerVector3>) info.GetValue("3", typeof(List<SerVector3>));

            //WindVelocity = info.GetInt64("WindVelocity");
        }

        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue("1", PathType);
            info.AddValue("2", PathName);
            info.AddValue("3", Vertexes);
        }

        #endregion
    }
}