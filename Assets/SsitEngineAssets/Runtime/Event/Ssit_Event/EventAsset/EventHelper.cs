/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/7/15 14:18:55                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Unity.Animation;

namespace Framework.Event
{
    public class EventHelper
    {
        public enum EnBindType
        {
            En_None,
            EN_Start,
            EN_End
        }

        public static SS_Animation ReadAnimation( AnimInfo data )
        {
            //Ss_Event eve = null;
            //MessagePack
            var animation = new SS_Animation();
            for (var i = 0; i < data.eventList.Count; ++i)
            {
                var child = data.eventList[i];
                var subEvent = ReadEvent(child, EnBindType.En_None, null);


                if (subEvent != null)
                {
                    animation.AddEvent(subEvent);

                    if (child.startEvents != null)
                        for (var j = 0; j < child.startEvents.Count; j++)
                        {
                            var temp = child.startEvents[j];
                            ReadEvent(temp, EnBindType.EN_Start, subEvent);
                        }

                    if (child.endEvents != null)
                        for (var j = 0; j < child.endEvents.Count; j++)
                        {
                            var temp = child.endEvents[j];
                            ReadEvent(temp, EnBindType.EN_End, subEvent);
                        }
                }
            }

            animation.length = data.length;
            return animation;
        }


        public static Ss_Event ReadEvent( EventDataInfo info, EnBindType bindType, Ss_Event eve )
        {
            if (info == null)
                return null;
            var retEvent = EventManager.Instance.CreateEvent(info.type, info.timer, info);

            if (retEvent != null && eve != null)
            {
                retEvent.Parent = eve;

                switch (bindType)
                {
                    case EnBindType.EN_Start:
                        eve.AddStartEvent(retEvent);
                        break;
                    case EnBindType.EN_End:
                        eve.AddEndEvent(retEvent);
                        break;
                }

                if (info.startEvents != null)
                    for (var j = 0; j < info.startEvents.Count; j++)
                    {
                        var child = info.startEvents[j];
                        ReadEvent(child, EnBindType.EN_Start, retEvent);
                    }

                if (info.endEvents != null)
                    for (var j = 0; j < info.endEvents.Count; j++)
                    {
                        var child = info.endEvents[j];
                        ReadEvent(child, EnBindType.EN_End, retEvent);
                    }
            }
            return retEvent;
        }


        public static void GetAnimationLastNode( AnimInfo data, ref int index )
        {
            for (var i = 0; i < data.eventList.Count; ++i)
            {
                var child = data.eventList[i];
                if (child.id > index) index = child.id;

                GetEventLastNode(child, ref index);
            }
        }

        public static void GetEventLastNode( EventDataInfo info, ref int index )
        {
            if (info == null)
                return;

            if (info.startEvents != null)
                for (var j = 0; j < info.startEvents.Count; j++)
                {
                    var child = info.startEvents[j];
                    if (child.id > index) index = child.id;
                    GetEventLastNode(child, ref index);
                }

            if (info.endEvents != null)
                for (var j = 0; j < info.endEvents.Count; j++)
                {
                    var child = info.endEvents[j];
                    if (child.id > index) index = child.id;
                    GetEventLastNode(child, ref index);
                }
        }
    }
}