/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：   网络数据包                                                 
*│　作   者：   Jusam                                              
*│　版   本：   1.0.0                                                 
*│　创建时间：  2019/05/07                           
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Unity.NetSocket;

namespace Framework.NetSocket
{
    public partial class MessagePackage : IMessagePackage
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public int ErrorCode { get; set; }
        
        /// <summary>
        /// 消息id
        /// </summary>
        public int MessageId { get; set; }

        /// <summary>
        /// 消息体
        /// </summary>
        public object MessageBody { get; set; }
        
        /*public MessagePackage( MessageID msgId, object body )
        {
            MessageId = (int) msgId;
            MessageBody = body;
        }*/

        public MessagePackage( int msgId, byte[] bodyArray, int errorCode )
        {
            MessageId = msgId;
            ErrorCode = errorCode;
            MessageBody = MessageDeserializer.Deserialize(msgId, bodyArray);
        }

    }
}