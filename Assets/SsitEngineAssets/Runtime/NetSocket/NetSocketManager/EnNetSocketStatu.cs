/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/10 12:01:30                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace Framework.NetSocket
{
    public enum EnNetSocketStatu
    {
        Disconnect = -1,
        Success = 0,
        Connect = 1,

        ConnectTimeOut,
        ConnectDisconnect,
        MutiConnect,
        ConnectUnknowError,


        RecvTimeOut,
        RecvClientDisconnect,
        RecvUnkownError,

        SendTimeOut,
        SendZeroByte,
        SendError,
        SendUnkownError,

        DisconnectSocketIsNull,
        DisconnectSocketHasDisconnect,
        DisConnectionTimeOut,
        DisConnectionUnknow
    }
}