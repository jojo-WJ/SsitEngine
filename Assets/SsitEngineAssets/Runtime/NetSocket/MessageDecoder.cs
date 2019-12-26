/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：   网络数据包解码器                                                    
*│　作   者：   Xuxin                                             
*│　版   本：   1.0.0                                                 
*│　创建时间：  2019/11/07                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Framework.Logic;
using SsitEngine.Unity.NetSocket;

namespace Framework.NetSocket
{
    public class MessageDecoder : IMessageDecoder
    {
        public const int BodyHeadLen = 16;

        public IMessagePackage decode( byte[] data )
        {
            var curPos = 0;

            //access：局域网内暂不牵扯拆包粘包的过程，后续如有外网机制需要进行扩展处理
            //消息包 = 包头 + 消息体
            //包头 = 总体长度 + 服务器错误码 + 消息id + 消息长度
            curPos += sizeof(int);
            //byte error = data[4];

            //服务器错误码
            var errorCode = ByteArrayToInt(data, 4);
            curPos += sizeof(int);

            //hack:针对服务器的错误码机制过滤
            /*if (errorCode != (int) SysErrorCode.Success)
            {
                NetHelper.PopSysErrorLog((SysErrorCode) errorCode);
                return null;
            }*/

            //消息Id
            var messageId = ByteArrayToInt(data, 8);
            curPos += sizeof(int);

            //包体长度
            var bodyLength = ByteArrayToInt(data, 12);
            curPos += sizeof(int);

            /*if (curPos != BodyHeadLen)
                SsitDebug.Error("服务器消息长度异常 MessageDecoder.decode[41]");*/

            var body = new List<byte>(bodyLength);
            var totalLength = BodyHeadLen + bodyLength;

            for (var i = BodyHeadLen; i < totalLength; i++) body.Add(data[i]);
            var result = new MessagePackage(messageId, body.ToArray(), errorCode);
            return result;
        }

        /// <summary>
        /// Int32类型的大端小端处理
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private int ByteArrayToInt( byte[] data, int startIndex )
        {
            var value = 0;
            for (var i = 0; i < 4; i++)
            {
                var shift = (4 - 1 - i) * 8;
                value += (data[i + startIndex] & 0x000000FF) << shift;
            }
            return value;
        }
    }
}