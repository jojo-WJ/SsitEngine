/*
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：                                                    
*│　作    者：Xuxin                                              
*│　版    本：1.0.0                                                 
*│　创建时间：2019/12/24 10:46:57                             
*└──────────────────────────────────────────────────────────────┘
*/

using Framework.Logic;
using Mirror;
using SsitEngine.PureMVC.Patterns;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.Mirror
{
    public class RoomPlayer : NetworkRoomPlayer
    {
        /// <summary>
        /// Current UserId of the player, e.g. Player1, Player2, etc.
        /// </summary>
        [SyncVar] 
        public string userId;

        public override void OnStartClient()
        {
            if (LogFilter.Debug)
            {
                Debug.LogFormat("OnStartClient {0}", SceneManager.GetActiveScene().name);
            }

            base.OnStartClient();
        }

        public override void OnClientEnterRoom()
        {
            if (LogFilter.Debug)
                Debug.LogFormat("OnClientEnterRoom {0}", SceneManager.GetActiveScene().name);

            //安排座位
            //SsitDebug.Error("Index:" + Index);

            // 服务端和客户端同时执行
        }

        public override void OnClientExitRoom()
        {
        }

        public override void OnClientReady( bool readyState )
        {
            Debug.Log($"OnClientReady {userId}");
            //NetworkLobbyManager lobby = NetworkManager.singleton as NetworkLobbyManager;
            //lobby?.ReadyStatusChanged();

            //todo:update roomInfo and server and client
            Facade.Instance.SendNotification((ushort) EnMirrorEvent.OnLobbyClientReady, userId, readyState, netId);

        }

        public override void OnStartLocalPlayer()
        {
            if (Facade.Instance.RetrieveProxy(UserProxy.NAME) is UserProxy proxy)
            {
                //请求分配座位
                Facade.Instance.SendNotification((ushort) EnMirrorEvent.OnLobbyClientEnter, (int) netId);
                //CmdChangeLobbyGuid(proxy.GetUserID());
                userId = proxy.GetUserID();
            }
            // host mode test present has error 
            // 在host端客户端往服务器发送的消息会被local client 截下来 按照client的消息指令运行这与存粹的服务器是存在差异的
            /*if (NetworkServer.active && NetworkClient.isLocalClient)
            {
                CmdChangeReadyState(true);
            }*/
        }

        private void OnDestroy()
        {
            if (NetworkManager.singleton != null)
            {
                if (NetworkClient.active && !NetworkServer.active)
                {
                    Facade.Instance.SendNotification((ushort) EnMirrorEvent.OnRoomClientExit, new ClientExitParam()
                    {
                        userId = userId,
                        isClient = true,
                        isLobby = true,
                        isExit = true,
                        isEnd = false,
                    });
                }
                else if (NetworkServer.active)
                {
                    //update memberInfo
                    //Facade.Instance.SendNotification((ushort)EnMirrorEvent.OnLobbyClientExit, UserId);
                    Facade.Instance.SendNotification((ushort) EnMirrorEvent.OnRoomClientExit, new ClientExitParam()
                    {
                        userId = userId,
                        isClient = false,
                        isLobby = true,
                        isExit = true,
                        isEnd = false,
                    });
                }
            }
        }

        #region Commands

        /*[Command]
        public void CmdChangeReadyState( bool ReadyState )
        {
            readyToBegin = ReadyState;
        }*/

        /*[Command]
        public void CmdChangeLobbyGuid( string guid )
        {
            userId = guid;
        }*/

        #endregion
    }
}