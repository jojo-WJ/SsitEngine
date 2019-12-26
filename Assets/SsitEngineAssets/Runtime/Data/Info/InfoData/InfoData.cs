using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SsitEngine.Data;
using SsitEngine.EzReplay;
using Table;
using UnityEngine;

namespace Framework.Data
{
    /// <summary>
    /// 数据基类，记录信息ID
    /// </summary>
    [Serializable]
    public class InfoData : SaveProxyState, IInfoData
    {
        private string m_ID;
        //public bool isDestory = false;

        //禁止适用，系统序列化默认认为是个bool对象
        //public static implicit operator bool(InfoData self)
        //{
        //    return null != self;
        //}

        public string ID
        {
            get => m_ID;
            set => m_ID = value;
        }


        public virtual void OnDestroy()
        {
            //isDestory = true;
        }


        #region 序列化

#if EZRE_PROTO_INFO
#else
        public InfoData()
        {
        }

        public InfoData( SerializationInfo info, StreamingContext context )
        {
        }

        public override void GetObjectData( SerializationInfo info, StreamingContext context )
        {
        }
#endif

        #endregion

        #region 回放

        #endregion
    }

    [Serializable]
    public class ObjectData : SaveObjState, IInfoData
    {
        private string m_ID;

        public ObjectData()
        {
        }

        public ObjectData( GameObject go ) : base(go)
        {
        }

        #region 序列化

        public ObjectData( SerializationInfo info, StreamingContext context ) : base(info, context)
        {
        }

        #endregion

        public string ID
        {
            get => m_ID;
            set => m_ID = value;
        }
        //public bool isDestory = false;

        public static implicit operator bool( ObjectData self )
        {
            return null != self;
        }


        public virtual void ShutDown()
        {
            //isDestory = true;
        }

        #region 回放

        #endregion
    }


}