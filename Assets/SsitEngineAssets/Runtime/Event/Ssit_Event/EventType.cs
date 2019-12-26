/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/7/15 9:14:47                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace Framework.Event
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public enum EventType
    {
        EVE_NONE,

        EVE_TWEEN, // 缓动
        EVE_UIHINT,
        EVE_UITIP,
        EVE_SOUND,

        EVE_FLASH,
        EVE_MESSAGE,
        EVE_FLASHAdvance
    }
}