/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/7/15 12:03:13                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Xml.Serialization;
using DG.Tweening;
using SsitEngine.Unity.Sound;
using UnityEngine;

namespace Framework.Event
{
    [XmlRoot("AnimInfo")]
    public class AnimInfo
    {
        public List<EventDataInfo> eventList;
        public string id;
        public float length;
        public string name;


        public float CalcSubEventCost()
        {
            float le = 0;
            if (eventList != null)
                for (var i = 0; i < eventList.Count; i++)
                    le += eventList[i].length;
            return le;
        }
    }
    //[XmlInclude(typeof(EventTweenComplexInfo))]

    [XmlInclude(typeof(EventTweenInfo))]
    [XmlInclude(typeof(EventSoundInfo))]
    [XmlInclude(typeof(EventUIContentInfo))]
    [XmlInclude(typeof(EventUITagInfo))]
    [XmlInclude(typeof(EventFlashInfo))]
    [XmlInclude(typeof(EventMessageInfo))]
    [XmlInclude(typeof(EventFlashAdvanceInfo))]
    public abstract class EventDataInfo
    {
        public string binderId;
        public List<EventDataInfo> endEvents;
        public int id;

        [XmlIgnore] public float length;

        public string name;

        [XmlIgnore] public int parentId;

        public List<EventDataInfo> startEvents;
        public string targetId;
        public float timer;
        public EventType type;


        /// <summary>
        /// 获取节点最大运行花费
        /// </summary>
        /// <returns></returns>
        public virtual float CalcSubEventCost()
        {
            //float maxDelay = 0;
            //if (startEvents != null)
            //{

            //}
            //for (int i = 0; i < UPPER; i++)
            //{
            //    return 
            //}
            return 0;
        }

        /// <summary>
        /// 获取节点长度
        /// </summary>
        /// <returns></returns>
        public virtual float GetNodeLength()
        {
            return length;
        }
    }

    [XmlRoot("EventTweenInfo")]
    public class EventTweenInfo : EventDataInfo
    {
        public float duration;
        public List<TrasInfo> path;
        public Ease tweenType;
    }

    [XmlRoot("EventTweenComplexInfo")]
    public class EventTweenComplexInfo : EventDataInfo
    {
        public List<TrasInfo> path;
    }

    [XmlRoot("EventTweenComplexInfo")]
    public class EventSoundInfo : EventDataInfo
    {
        public string clipName;
        public SoundDuckingSetting setting;
        public float volume;
    }

    [XmlRoot("EventUIContentInfo")]
    public class EventUIContentInfo : EventDataInfo
    {
        public string content;
        public bool state;
    }

    [XmlRoot("EventUITagInfo")]
    public class EventUITagInfo : EventDataInfo
    {
        public string BindName;
        public string content;
        public float duration;
        public Vector3 offset;
    }

    [XmlRoot("EventFlashInfo")]
    public class EventFlashInfo : EventDataInfo
    {
        public string BindName;
        public float duration;
        public float frequency;
        public Color lightColor;
        public Color nomalColor;
    }

    [XmlRoot("EventFlashAdvanceInfo")]
    public class EventFlashAdvanceInfo : EventDataInfo
    {
        public string BindName;

        public string content;
        public float duration;
        public float frequency;
        public Color lightColor;
        public Color nomalColor;
        public Vector3 offset;
    }

    [XmlRoot("EventMessageInfo")]
    public class EventMessageInfo : EventDataInfo
    {
        public EnEventMessage messageName;
        public string value;
    }

    [XmlRoot("TrasInfo")]
    public class TrasInfo
    {
        public float duration;
        public Vector3 postion;
        public Vector3 rotate;
    }
}