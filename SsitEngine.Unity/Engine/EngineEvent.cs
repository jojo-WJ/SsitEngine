/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/27 15:53:56                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity
{
    public enum EnEngineEvent
    {
        OnApplicationStart = EnMsgCenter.EngineEvent + 1,
        OnApplicationQuit,
        OnApplicationPauseChange,
        OnApplicationFocusChange
    }
}