using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Framework.Data;

namespace SsitEngine.EzReplay
{
    /// <summary>
    /// 属性映射列表包装器
    /// </summary>
    [Serializable]
    public class Object2PropertiesMappingListWrapper : ISerializable

    {
        //回放开始时间
        public long begineTime;

        //回放结束时间
        public long endTime;

        // desc: 回放系统的版本
        public string EZR_VERSION = EZReplayManager.EZR_VERSION;

        //最大位置
        public int maxPosition;


        // 预案聊天信息
        public List<MessageInfo> messageInfos;

        // desc: 对象和属性的关联器
        public List<Object2PropertiesMapping> object2PropertiesMappings = new List<Object2PropertiesMapping>();

        // 线路绘制信息
        public List<PathVertexesInfo> pathInfos;

        // desc: 记录间隔
        public float recordingInterval;

        // serialization constructor
        protected Object2PropertiesMappingListWrapper( SerializationInfo info, StreamingContext context )
        {
            EZR_VERSION = info.GetString("1");
            recordingInterval = (float) info.GetValue("2", typeof(float));

            maxPosition = info.GetInt32("3");
            begineTime = info.GetInt64("4");
            endTime = info.GetInt64("5");

            object2PropertiesMappings =
                (List<Object2PropertiesMapping>) info.GetValue("6", typeof(List<Object2PropertiesMapping>));
            messageInfos = (List<MessageInfo>) info.GetValue("8", typeof(List<MessageInfo>));
            pathInfos = (List<PathVertexesInfo>) info.GetValue("9", typeof(List<PathVertexesInfo>));
        }

        public Object2PropertiesMappingListWrapper( List<Object2PropertiesMapping> mappings )
        {
            object2PropertiesMappings = mappings;
        }

        public Object2PropertiesMappingListWrapper()
        {
        }

        public void GetObjectData( SerializationInfo info, StreamingContext ctxt )
        {
            info.AddValue("1", EZR_VERSION);
            info.AddValue("2", recordingInterval);

            info.AddValue("3", maxPosition);
            info.AddValue("4", begineTime);
            info.AddValue("5", endTime);


            info.AddValue("6", object2PropertiesMappings);
            info.AddValue("8", messageInfos);
            info.AddValue("9", pathInfos);

            //base.GetObjectData(info, context);
        }

        public void AddMapping( Object2PropertiesMapping mapping )
        {
            lock (object2PropertiesMappings)
            {
                object2PropertiesMappings.Add(mapping);
            }
        }
    }
}