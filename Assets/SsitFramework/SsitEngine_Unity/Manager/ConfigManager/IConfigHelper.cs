/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/28 20:36:11                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.Config
{
    public interface IConfigHelper
    {
        string SocketIp { get; }
        ushort SocketPort { get; }

        string HttpIp { get; }
        ushort HttpPort { get; }

        string HttpIpPort { get; }

        void ReadConfig();
    }
}