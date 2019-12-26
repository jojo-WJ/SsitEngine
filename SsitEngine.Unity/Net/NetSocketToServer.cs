/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：Jusam                                              
*│　版   本：1.0.0                                                 
*│　创建时间：                             
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Threading;

namespace SsitEngine.Unity.NetSocket
{
    /// <summary>
    ///     服务端
    /// </summary>
    public class NetSocketToServer
    {
        private readonly NetSocket clientSocket;
        private readonly NetSocket.NormalNetCallBack m_callBackConnect;
        private readonly NetSocket.NormalNetCallBack m_callBackDisConnect;
        private readonly NetSocket.ReceiveCallBack m_callBackReceive;
        private readonly NetSocket.NormalNetCallBack m_callBackSend;
        private readonly IMessageDecoder m_messageDecoder;
        private readonly IMessageEncoder m_messageEncoder;

        private readonly IMessageRouter m_messageRouter;
        private readonly Queue<IMessagePackage> receiveMessagePool = new Queue<IMessagePackage>();
        private readonly Queue<IMessagePackage> sendMessagePool = new Queue<IMessagePackage>();

        private NetSocket.NormalNetCallBack m_callBackNormal;

        private Thread sendThread;

        /// <summary>
        ///     创建一个Socket网络长连接
        /// </summary>
        /// <param name="ip">Ip地址</param>
        /// <param name="port">网络接口</param>
        /// <param name="messageRouter">？</param>
        /// <param name="messageEncoder">？</param>
        /// <param name="messageDecoder">？</param>
        /// <param name="connectFaled">？</param>
        /// <param name="sendFailed">？</param>
        /// <param name="recvFailed">？</param>
        /// <param name="disConnect">？</param>
        public NetSocketToServer( string ip, ushort port, IMessageRouter messageRouter, IMessageEncoder messageEncoder,
            IMessageDecoder messageDecoder, NetSocket.NormalNetCallBack connectFaled,
            NetSocket.NormalNetCallBack sendFailed, NetSocket.ReceiveCallBack recvFailed,
            NetSocket.NormalNetCallBack disConnect )
        {
            m_callBackConnect = connectFaled;

            m_callBackSend = sendFailed;

            m_callBackReceive = recvFailed;

            m_callBackDisConnect = disConnect;

            m_messageRouter = messageRouter;
            m_messageEncoder = messageEncoder;
            m_messageDecoder = messageDecoder;


            clientSocket = new NetSocket();
            clientSocket.AsyncConnect(ip, port, ConnectCallBack, SendCallBack, ReceiveCallBack, DisConnectCallBack);
        }

        /// <summary>
        ///     是否是连接状态
        /// </summary>
        public bool IsDisconnected => clientSocket.isConnect();

        /// <summary>
        ///     断开连接
        /// </summary>
        public void Disconnect()
        {
            clientSocket.AsyncDisconnect();
        }

        #region 网络方法回调

        private void ConnectCallBack( bool isSuccess, ErrorSockets error, string expection )
        {
            if (isSuccess)
            {
                sendThread = new Thread(LoopSending);
                sendThread.Start();
            }

            m_callBackConnect(isSuccess, error, expection);
        }

        private void ReceiveCallBack( bool isSuccess, ErrorSockets error, string expection, byte[] byteData,
            string strMsg )
        {
            if (isSuccess)
            {
                ReceiveMsgToNetMsg(byteData);
            }
            m_callBackReceive(isSuccess, error, expection, byteData, strMsg);
        }

        private void SendCallBack( bool isSuccess, ErrorSockets error, string expection )
        {
            m_callBackSend(isSuccess, error, expection);
        }

        private void DisConnectCallBack( bool isSuccess, ErrorSockets error, string expection )
        {
            if (isSuccess)
            {
                sendThread.Abort();
            }
            m_callBackDisConnect(isSuccess, error, expection);
        }

        #endregion


        #region 上层API

        public void PutSendMessageToPool( IMessagePackage msg )
        {
            lock (sendMessagePool)
            {
                sendMessagePool.Enqueue(msg);
            }
        }

        public void Update()
        {
            if (receiveMessagePool != null && receiveMessagePool.Count > 0)
            {
                while (receiveMessagePool.Count > 0)
                {
                    AnalyseData(receiveMessagePool.Dequeue());
                }
            }
        }

        /// TODO :ADD MORE
        /// <summary>
        ///     发送到上层
        /// </summary>
        /// <param name="msg"></param>
        private void AnalyseData( IMessagePackage msg )
        {
            //处理消息
            m_messageRouter.FireMessage(msg.MessageId, msg.MessageBody);
        }

        /// <summary>
        ///     转换为网络数据,并加入队列
        /// </summary>
        /// <param name="data"></param>
        private void ReceiveMsgToNetMsg( byte[] data )
        {
            //反序列化
            var pack = m_messageDecoder.decode(data);
            if (pack == null)
            {
                return;
            }
            receiveMessagePool.Enqueue(pack);
        }

        private void LoopSending()
        {
            while (clientSocket != null && clientSocket.isConnect())
            {
                lock (sendMessagePool)
                {
                    while (sendMessagePool.Count > 0)
                    {
                        var msg = sendMessagePool.Dequeue();
                        var data = m_messageEncoder.encode(msg);
                        clientSocket.AsyncSend(data);
                    }
                }
                Thread.Sleep(100);
            }
        }

        #endregion
    }
}