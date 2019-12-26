/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：   网络数据路由转发                                                    
*│　作   者：   Jusam                                             
*│　版   本：   1.0.0                                                 
*│　创建时间：   2019/05/07                          
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Unity.NetSocket;

namespace Framework.NetSocket
{
    public class MessageRouter : IMessageRouter
    {
        public void FireMessage( int messageId, object messageBody )
        {
            /*var notify = ProtoMessageMap.Instance.GetValue((MessageID) messageId);
            Facade.Instance.SendNotification((ushort) EnSocketEvent.ReceiveMessage, messageBody, notify);
            */

        }
    }
}