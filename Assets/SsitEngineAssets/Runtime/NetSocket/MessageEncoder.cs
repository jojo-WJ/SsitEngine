/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：   网络数据包编码器                                                    
*│　作   者：   Jusam                                             
*│　版   本：   1.0.0                                                 
*│　创建时间：   2019/05/07                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using SsitEngine.Unity;
using SsitEngine.Unity.NetSocket;

namespace Framework.NetSocket
{
    public class MessageEncoder : IMessageEncoder
    {
        public byte[] encode( IMessagePackage message )
        {
            var packet = message as MessagePackage;

            var bufferList = new List<byte>();

            //bufferList.Add(packet.Flag);

            var id = Int2Bytes(packet.MessageId);
            bufferList.AddRange(id);

            var body = ProtoBufferUtils.Serialize(packet.MessageBody);
            var bodyLength = Int2Bytes(body.Length);
            bufferList.AddRange(bodyLength);
            if (body.Length > 0) bufferList.AddRange(body);

            var result = bufferList.ToArray();
            return result;
        }


        private byte[] Int2Bytes( int n )
        {
            var result = new byte[4];
            result[0] = (byte) ((n & 0xFF000000) >> 24);
            result[1] = (byte) ((n & 0x00FF0000) >> 16);
            result[2] = (byte) ((n & 0x0000FF00) >> 8);
            result[3] = (byte) (n & 0x000000FF);
            return result;
        }
    }
}