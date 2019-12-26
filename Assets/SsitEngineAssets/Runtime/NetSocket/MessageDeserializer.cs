/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：   网络数据包的反序列化器                                                    
*│　作   者：   Xuxin                                              
*│　版   本：   1.0.0                                                 
*│　创建时间：  2019/11/07                           
*└──────────────────────────────────────────────────────────────┘
*/

namespace Framework.NetSocket
{
    public class MessageDeserializer
    {
        public static object Deserialize( int messageId, byte[] bodyArray )
        {
            /*switch ((MessageID) messageId)
            {
                default: return default;
            }*/
            return default;
        }
    }
}