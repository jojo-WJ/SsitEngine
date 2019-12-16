/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述： NetSocket  网络回调底层                                                    
*│　作   者：Jusam                                              
*│　版   本：1.0.0                                                 
*│　创建时间: 2019/4/19                        
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SsitEngine.Unity.NetSocket
{
    /// <summary>
    ///     网络模块错误代码标识
    /// </summary>
    public enum ErrorSockets
    {
        Success = 0,

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

    /// <summary>
    /// </summary>
    public class NetSocket
    {
        public delegate void NormalNetCallBack( bool isSuccess, ErrorSockets error, string expection );

        public delegate void ReceiveCallBack( bool isSuccess, ErrorSockets error, string expection, byte[] byteData,
            string strMsg );


        private Socket clientSocket;

        private readonly byte[] dataBuffer; //1kb

        private NormalNetCallBack m_callBackConnect;
        private NormalNetCallBack m_callBackDisConnect;
        private ReceiveCallBack m_callBackReceive;
        private NormalNetCallBack m_callBackSend;

        private readonly SocketBuffer receiveBuffer;


        public NetSocket()
        {
            receiveBuffer = new SocketBuffer(4, HandlePacketComplete);
            dataBuffer = new byte[1024];
        }

        #region 网络请求

        public void AsyncConnect( string ip, ushort port, NormalNetCallBack connectBack, NormalNetCallBack sendBack,
            ReceiveCallBack callBackRecv, NormalNetCallBack disConnect )
        {
            m_callBackConnect = connectBack;
            if (clientSocket != null && clientSocket.Connected)
            {
                m_callBackConnect(false, ErrorSockets.MutiConnect, "重复连接");

                return;
            }

            m_callBackSend = sendBack;
            m_callBackReceive = callBackRecv;
            m_callBackDisConnect = disConnect;

            if (clientSocket == null || !clientSocket.Connected)
            {
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var address = IPAddress.Parse(ip);
                var point = new IPEndPoint(address, port);
                var connectAr = clientSocket.BeginConnect(point, ConnectCallBack, clientSocket);
                if (!WriteDot(connectAr)) connectBack(false, ErrorSockets.ConnectTimeOut, "连接超时");
            }
        }

        private void ConnectCallBack( IAsyncResult ar )
        {
            try
            {
                clientSocket.EndConnect(ar);
            }
            catch (Exception e)
            {
                m_callBackConnect(false, ErrorSockets.ConnectUnknowError, e.ToString());
            }


            if (!clientSocket.Connected)
            {
                m_callBackConnect(false, ErrorSockets.ConnectDisconnect, "连接服务器失败");
            }
            else
            {
                m_callBackConnect(true, ErrorSockets.Success, "连接成功");
                AsyncReceive();
            }
        }

        private void AsyncReceive()
        {
            if ((clientSocket != null) & clientSocket.Connected)
            {
                var rec = clientSocket.BeginReceive(dataBuffer, 0, dataBuffer.Length, SocketFlags.None,
                    RecceiveCallBack, clientSocket);
            }
            else
            {
                m_callBackReceive(false, ErrorSockets.RecvClientDisconnect, "Disconnect", null, null);
            }
        }

        private void RecceiveCallBack( IAsyncResult ar )
        {
            try
            {
                if (clientSocket == null || !clientSocket.Connected)
                {
                    m_callBackReceive(false, ErrorSockets.RecvClientDisconnect, "接收时连接断开", null, null);
                    return;
                }

                var length = clientSocket.EndReceive(ar);
                if (length == 0)
                    //Debug.LogError(" RecceiveCallBack   数据长度为0");
                    return;
                receiveBuffer.RecevByte(dataBuffer, length);
            }
            catch (Exception e)
            {
                m_callBackReceive(false, ErrorSockets.RecvUnkownError, e.ToString(), null, null);
            }
            //循环接收
            AsyncReceive();
        }

        public void AsyncSend( byte[] data )
        {
            if (data == null || data.Length == 0 || clientSocket == null)
                m_callBackSend(false, ErrorSockets.SendError, "error");
            if (clientSocket.Connected != true)
            {
                m_callBackSend(false, ErrorSockets.SendError, "未连接");
            }
            else
            {
                var send = clientSocket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallBack, clientSocket);


                // Debug.Log(" send.Length==" + data.Length);
                if (!WriteDot(send)) m_callBackSend(false, ErrorSockets.SendTimeOut, "发送超时");
            }
        }

        private void SendCallBack( IAsyncResult ar )
        {
            try
            {
                var byteSend = clientSocket.EndSend(ar);
                if (byteSend > 0)
                    m_callBackSend(true, ErrorSockets.Success, "发送成功");
                else
                    m_callBackSend(false, ErrorSockets.SendZeroByte, "发送异常");
            }
            catch (Exception e)
            {
                m_callBackSend(false, ErrorSockets.SendUnkownError, e.ToString());
            }
        }

        public void AsyncDisconnect()
        {
            try
            {
                if (clientSocket == null)
                {
                    m_callBackDisConnect(false, ErrorSockets.DisconnectSocketIsNull, "null");
                    return;
                }
                if (clientSocket.Connected != true)
                {
                    m_callBackDisConnect(false, ErrorSockets.DisconnectSocketHasDisconnect, "已经断开连接了");
                }
                else
                {
                    var dis = clientSocket.BeginDisconnect(false, DisConnectCallBack, clientSocket);
                    if (!WriteDot(dis)) m_callBackDisConnect(false, ErrorSockets.DisConnectionTimeOut, "超时");
                }
            }
            catch (Exception e)
            {
                m_callBackDisConnect(false, ErrorSockets.DisConnectionUnknow, e.ToString());
            }
        }

        private void DisConnectCallBack( IAsyncResult ar )
        {
            try
            {
                clientSocket.EndDisconnect(ar);
                clientSocket.Close();
                clientSocket = null;
                m_callBackDisConnect(true, ErrorSockets.Success, "成功断开");
            }
            catch (Exception e)
            {
                m_callBackDisConnect(false, ErrorSockets.DisConnectionUnknow, e.ToString());
            }
        }

        #endregion


        #region Helper

        /// <summary>
        ///     TimeOutCheck,true 代表不超时
        /// </summary>
        /// <param name="ar"></param>
        /// <returns></returns>
        private bool WriteDot( IAsyncResult ar )
        {
            var i = 0;
            while (ar.IsCompleted == false)
            {
                i++;
                if (i > 30) return false;
                Thread.Sleep(100);
            }

            return true;
        }

        private void HandlePacketComplete( byte[] allData )
        {
            m_callBackReceive(true, ErrorSockets.Success, null, allData, "成功收到数据 长度：" + allData.Length);
        }

        public bool isConnect()
        {
            if (clientSocket != null) return clientSocket.Connected;
            return false;
        }

        #endregion
    }
}