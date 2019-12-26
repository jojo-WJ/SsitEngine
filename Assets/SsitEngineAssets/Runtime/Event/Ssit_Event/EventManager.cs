/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/7/15 11:36:10                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using DG.Tweening;
using SsitEngine;
using SsitEngine.Core;
using UnityEngine;

namespace Framework.Event
{
    using EventMap = Dictionary<int, Ss_Event>;

    public class EventManager : Singleton<EventManager>, ISingleton
    {
        protected EventMap mEvents;
        protected UlGuid mIdGenerator; // Id生成器

        public void OnSingletonInit()
        {
            mEvents = new EventMap();
            mIdGenerator = new UlGuid();
        }

        public Ss_Event CreateEvent( EventType type, float time, EventDataInfo info )
        {
            Ss_Event eve = null;
            var id = (int) mIdGenerator.GenerateNewId();

            switch (type)
            {
                case EventType.EVE_TWEEN:
                {
                    eve = new EventTween(id, time);
                    break;
                }
                case EventType.EVE_UITIP:
                {
                    eve = new EventUITag(id, time);
                    break;
                }
                case EventType.EVE_UIHINT:
                {
                    eve = new EventUIContent(id, time);
                    break;
                }
                case EventType.EVE_SOUND:
                {
                    eve = new EventSound(id, time);
                    break;
                }
                case EventType.EVE_FLASH:
                {
                    eve = new EventFlash(id, time);
                    break;
                }
                case EventType.EVE_FLASHAdvance:
                {
                    eve = new EventFlashAdvance(id, time);
                    break;
                }
                case EventType.EVE_MESSAGE:
                {
                    eve = new EventMessage(id, time);
                    break;
                }
            }


            if (eve != null)
            {
                if (mEvents.ContainsKey(id))
                    Debug.Log("A event of id '" + id + "' already exists EventManager::createEvent");
                eve.Init(info);

                mEvents.Add(id, eve);
            }

            return eve;
        }

        public void DestroyEvent( Ss_Event eve )
        {
            DestroyEvent(eve.Id);
        }

        public void DestroyEvent( int id )
        {
            if (mEvents.ContainsKey(id))
            {
                mEvents[id].Shutdown();
                mEvents.Remove(id);
            }
        }

        public void Destroy()
        {
            var itor = mEvents.GetEnumerator();
            while (itor.MoveNext())
                itor.Current.Value.Shutdown();
            itor.Dispose();
            mEvents.Clear();

            mIdGenerator = null;
        }

        public EventDataInfo CreateEventInfo( EventType type, int time, int nodeIndex )
        {
            EventDataInfo eve = null;
            var id = nodeIndex;

            switch (type)
            {
                case EventType.EVE_TWEEN:
                {
                    eve = new EventTweenInfo {id = id, timer = time, tweenType = Ease.Linear};
                    break;
                }
                case EventType.EVE_UITIP:
                {
                    eve = new EventUITagInfo {id = id, timer = time};
                    break;
                }
                case EventType.EVE_UIHINT:
                {
                    eve = new EventUIContentInfo {id = id, timer = time};
                    break;
                }
                case EventType.EVE_SOUND:
                {
                    eve = new EventSoundInfo {id = id, timer = time};
                    break;
                }
                case EventType.EVE_FLASH:
                {
                    eve = new EventFlashInfo {id = id, timer = time};
                    break;
                }
                case EventType.EVE_FLASHAdvance:
                {
                    eve = new EventFlashAdvanceInfo {id = id, timer = time};
                    break;
                }
                case EventType.EVE_MESSAGE:
                {
                    eve = new EventMessageInfo {id = id, timer = time};
                    break;
                }
            }

            eve.type = type;
            Debug.Log($"create eve id {id} eve type {eve.type}");
            return eve;
        }
    }
}