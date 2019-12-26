/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：  Socket网络模块                                                  
*│　作   者：  Jusam                                              
*│　版   本：  1.0.0                                                 
*│　创建时间：  2019/05/07                           
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections;
using SsitEngine;
using SsitEngine.DebugLog;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Unity;
using SsitEngine.Unity.NetSocket;
using SsitEngine.Unity.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.NetSocket
{
    /// <summary>
    /// Socket网络管理器
    /// </summary>
    public class NetSocketManager : ManagerBase<NetSocketManager>
    {
        /// <summary>
        /// Socket 网络连接状态
        /// </summary>
        private EnNetSocketStatu asyncConnectState = EnNetSocketStatu.Disconnect;

        private EnNetSocketStatu connectState = EnNetSocketStatu.Disconnect;
        private bool isShowLoading;

        /// <summary>
        /// Socket 操作对象
        /// </summary>
        private NetSocketToServer netSocket;

        /// <summary>
        /// 网络连接状态回调
        /// </summary>
        public UnityAction<object, EnNetSocketStatu, EnNetSocketStatu> OnStatuChange;

        public EnNetSocketStatu ConnectState
        {
            get => connectState;
            set
            {
                if (connectState != value && OnStatuChange != null)
                {
                    var temp = connectState;
                    connectState = value;
                    OnStatuChange(this, temp, connectState);
                }
            }
        }

        /// <summary>
        /// 网络是否处于连接状态
        /// </summary>
        public bool IsConnected => netSocket.IsDisconnected;

        /// <summary>
        /// 网络连接回调
        /// </summary>
        /// <param name="isSuccess">网络连接状态标记，如果为 True 则网络连接成功</param>
        /// <param name="error">网络连接状态码</param>
        /// <param name="exception">网络连接状态内容</param>
        private void OnConnectCallBack( bool isSuccess, ErrorSockets error, string exception )
        {
            if (error != ErrorSockets.Success && !string.IsNullOrEmpty(exception))
                Debug.LogError("ConnectCallBack :  " + exception);
            asyncConnectState = isSuccess ? EnNetSocketStatu.Connect : EnNetSocketStatu.ConnectDisconnect;
        }

        /// <summary>
        /// 消息发送回调
        /// </summary>
        /// <param name="isSuccess">消息发送状态标记，如果为 True 则消息发送成功</param>
        /// <param name="error">消息发送状态码</param>
        /// <param name="exception">消息发送状态内容</param>
        private void OnSendCallBack( bool isSuccess, ErrorSockets error, string exception )
        {
            if (error != ErrorSockets.Success && !string.IsNullOrEmpty(exception))
                Debug.LogError("SendCallBack:  " + exception);
        }

        /// <summary>
        /// 数据接收回调
        /// 一般的 业务层不需要关心该回调
        /// </summary>
        /// <param name="isSuccess">数据接收状态标记，如果为 True 则数据接收成功</param>
        /// <param name="error">数据接收状态码</param>
        /// <param name="exception">数据接收状态内容</param>
        /// <param name="byteData">接收数据体</param>
        /// <param name="strMsg">数据字符串</param>
        private void OnReceiveCallBack( bool isSuccess, ErrorSockets error, string exception, byte[] byteData,
            string strMsg )
        {
            if (error != ErrorSockets.Success)
                if (!string.IsNullOrEmpty(exception))
                    //SsitDebug.Error("ReceiveCallBack:  " + exception);
                    asyncConnectState = EnNetSocketStatu.RecvClientDisconnect;
        }

        /// <summary>
        /// 网络断开连接回调
        /// </summary>
        /// <param name="isSuccess">网络断开连接状态标记，如果为 True 则网络断开连接成功</param>
        /// <param name="error">网络断开连接状态码</param>
        /// <param name="exception">网络断开连接状态内容</param>
        private void OnDisConnectCallBack( bool isSuccess, ErrorSockets error, string exception )
        {
            if (error != ErrorSockets.Success)
                if (!string.IsNullOrEmpty(exception))
                    SsitDebug.Error("DisConnectCallBack:  " + exception);
        }

        /// <summary>
        /// 发送数据包
        /// </summary>
        /// <param name="pack"></param>
        private void SendMessage( MessagePackage pack )
        {
            netSocket.PutSendMessageToPool(pack);
        }

        #region 模块管理

        public override void OnSingletonInit()
        {
        }

        public override string ModuleName => typeof(NetSocketManager).FullName;

        public override int Priority => 11;

        public override void OnUpdate( float elapseSeconds )
        {
            if (asyncConnectState != connectState) ConnectState = asyncConnectState;

            if (netSocket != null) netSocket.Update();
        }

        public override void Shutdown()
        {
            netSocket?.Disconnect();
        }

        #endregion


        #region 网络连接

        public void InitNet()
        {
            SsitDebug.Info(
                $"UserNet::ip:{SsitEngine.Unity.Config.ConfigManager.Instance.Helper.SocketIp} socketPort{SsitEngine.Unity.Config.ConfigManager.Instance.Helper.SocketPort} webPort{SsitEngine.Unity.Config.ConfigManager.Instance.Helper.HttpPort}");
            netSocket = new NetSocketToServer(SsitEngine.Unity.Config.ConfigManager.Instance.Helper.SocketIp
                , SsitEngine.Unity.Config.ConfigManager.Instance.Helper.SocketPort
                , new MessageRouter()
                , new MessageEncoder()
                , new MessageDecoder()
                , OnConnectCallBack
                , OnSendCallBack
                , OnReceiveCallBack
                , OnDisConnectCallBack);
        }

        public void ReConnect()
        {
            StartCoroutine(IEReConnect());
        }

        private IEnumerator IEReConnect()
        {
            //Facade.Instance.SendNotification((ushort) UIMsg.OpenForm, En_UIForm.LoadingForm);
            yield return new WaitForSeconds(1.5f);
            InitNet();
        }

        #endregion


        #region 消息处理

        private void OnEnable()
        {
            m_msgList = new ushort[2]
            {
                (ushort) EnSocketEvent.SendMessage,
                (ushort) EnSocketEvent.ReceiveMessage
            };
            RegisterMsg(m_msgList);
        }

        private void OnDisable()
        {
            UnRegisterMsg(m_msgList);
        }


        public override void HandleNotification( INotification notification )
        {
            var args = notification as MvEventArgs;

            /*switch (args.Id)
            {
                case (ushort) EnSocketEvent.SendMessage:
                {
                    isShowLoading = args.BoolValue;
                    if (isShowLoading)
                        Facade.Instance.SendNotification((ushort) UIMsg.OpenForm, En_UIForm.LoadingForm);
                    var packet = notification.Body as MessagePackage;
                    SendMessage(packet);
                }
                    break;
                case (ushort) EnSocketEvent.ReceiveMessage:
                {
                    var notify = args.UshortValue;
                    Facade.Instance.SendNotification((ushort) UIMsg.CloseForm, En_UIForm.LoadingForm);
                    Facade.Instance.SendNotification(notify, args.Body);
                }
                    break;
            }*/
        }

        #endregion
    }
}