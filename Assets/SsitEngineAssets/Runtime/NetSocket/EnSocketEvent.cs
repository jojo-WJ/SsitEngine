/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/26 11:59:50                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Unity;

namespace Framework.NetSocket
{
    /// <summary>
    /// Socket网络事件
    /// </summary>
    public enum EnSocketEvent
    {
        SendMessage = EnMsgCenter.SocketEvent,
        ReceiveMessage,

        MaxValue
    }
}