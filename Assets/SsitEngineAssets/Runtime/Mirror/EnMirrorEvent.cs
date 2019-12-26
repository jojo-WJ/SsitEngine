/*
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：                                                    
*│　作    者：Xuxin                                              
*│　版    本：1.0.0                                                 
*│　创建时间：2019/12/24 10:46:57                             
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Unity;

namespace Framework.Mirror
{
    public enum EnMirrorEvent
    {
        // Server
        CreateRoom = EnMsgCenter.NetEvent + 1,


        OnLobbyStartServer,
        OnLobbyServerConnect,
        OnLobbyServerDisconnect,

        // Client
        EnterRoom,
        OnLobbyClientEnter,
        OnLobbyClientReady,
        OnRoomClientExit,
        OnLobbyClientConnect,
        OnLobbyClientDisconnect,

        // Client And Server
        ExitRoom,
        SendMessage,
        StartGame,
        LoadScene,
        LoadSceneProcess,

        //...
        MaxValue
    }

    public enum EnRoomEvent
    {
        SwarpRoom = EnMirrorEvent.MaxValue + 1,

        OnLobbyMemberExchange,
        UpdateRoomState,

        SCMembresResult,
        SCSWarpMembresResult,

        //...
        MaxValue
    }
}