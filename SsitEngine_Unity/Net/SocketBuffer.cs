namespace SsitEngine.Unity.NetSocket
{
    /// <summary>
    ///     处理沾包拆包
    /// </summary>
    public class SocketBuffer
    {
        public delegate void CallBackReceiveOver( byte[] allData );

        private int allDataLengh = -1; //这类数据总长度
        private byte[] allRecData; //数据拼接用

        private int curRecLength; //当前接收长度

        private readonly byte[] headBytes;
        private byte headLength = 4;

        private readonly CallBackReceiveOver m_callBackReceiveOver;

        public SocketBuffer( byte headLength, CallBackReceiveOver receiveOver )
        {
            m_callBackReceiveOver = receiveOver;
            headBytes = new byte[headLength];
            this.headLength = headLength;
        }

        public void RecevByte( byte[] recByte, int reallength )
        {
            if (reallength == 0)
                return;
            if (curRecLength < headBytes.Length)
            {
                RecvHead(recByte, reallength);
            }
            else
            {
                var tmpLength = curRecLength + reallength;
                if (tmpLength == allDataLengh)
                    RecvOneAll(recByte, reallength);
                else if (tmpLength > allDataLengh) //接收到的比要接的大
                    RecvLarger(recByte, reallength);
                else //实际接收到的　比要的小
                    RecvSmall(recByte, reallength);
            }
        }

        private void RecvHead( byte[] recByte, int reallength )
        {
            // if (curRecLength < headByte.Length) //小于头长度
            //    {
            var tmpReal = headBytes.Length - curRecLength;


            var tmpLength = curRecLength + reallength;

            if (tmpLength < headBytes.Length) //比头小
            {
                // Debug.Log("small  head");
                FileUtils.MemCpy(headBytes, recByte, curRecLength, reallength);
                curRecLength += tmpReal;
            }
            else
            {
                //Debug.Log("big head");

                FileUtils.MemCpy(headBytes, recByte, curRecLength, tmpReal);
                curRecLength += tmpReal;

                //第一次接收
                //if(allDataLengh == -1)
                //{
                allDataLengh = ByteArrayToInt(recByte, 0);
                //}

                //Debug.Log("=========allDataLengh == " + allDataLengh);
                allRecData = new byte[allDataLengh];


                //头考入
                FileUtils.MemCpy(allRecData, 0, headBytes, 0, headBytes.Length);


                // 剩下的　在放入
                var tmpRemain = reallength - tmpReal;
                //Debug.Log("=========tmpRemain == " + tmpRemain);

                if (tmpRemain > 0)
                {
                    var tmpBytes = new byte[tmpRemain];

                    FileUtils.MemCpy(tmpBytes, 0, recByte, tmpReal, tmpRemain);

                    //Debug.Log("=========Enter Again111111111111== ");
                    RecevByte(tmpBytes, tmpRemain);
                }
                else
                {
                    // Debug.Log("Rece   one over");
                    RecvOneMsgOver();
                }
            }

            //     }
            //else//　大于等于头长度
            //{
            //    //IFileTools.MemCpy(headByte, recByte, 0, headByte.Length);
            //    //curRecLength += headByte.Length;


            //    //allDataLengh = IFileTools.ByteToInt(headByte) + headLength;

            //    //Debug.Log("Big   allDataLengh =="+ allDataLengh);
            //    //allRecData = new byte[allDataLengh];


            //    ////头考入
            //    //IFileTools.MemCpy(allRecData, 0, headByte, 0, headByte.Length);


            //    //// 剩下的　在放入
            //    //int tmpRemain = reallength -  headByte.Length;
            //    //if (tmpRemain > 0)
            //    //{
            //    //    byte[] tmpBytes = new byte[tmpRemain];
            //    //    IFileTools.MemCpy(tmpBytes, 0, recByte, headByte.Length, tmpRemain);
            //    //    RecevByte(tmpBytes, tmpRemain);
            //    //}


            //}
        }

        private void RecvSmall( byte[] recvByte, int realLength )
        {
            FileUtils.MemCpy(allRecData, recvByte, curRecLength, realLength);
            curRecLength += realLength;
        }


        private void RecvLarger( byte[] recvByte, int realLength )
        {
            var tmpLength = allDataLengh - curRecLength;
            //Debug.Log(" =====tmpLength == " + tmpLength);
            if (tmpLength == 0) return;

            FileUtils.MemCpy(allRecData, recvByte, curRecLength, tmpLength);
            curRecLength += tmpLength;
            RecvOneMsgOver();

            var remainLenth = realLength - tmpLength;
            //Debug.Log(" =====remainLenth == " + remainLenth);
            var remainByte = new byte[remainLenth];

            //   Debug.Log("recvByte larger =="+ recvByte.Length);

            //Debug.Log("recvByte larger tmpLength ==" + tmpLength);


            //Debug.Log("recvByte larger tmpLength 2222==" + remainLenth);


            //Debug.Log("remainByte  444444==" + remainByte.Length);

            FileUtils.MemCpy(remainByte, 0, recvByte, tmpLength, remainLenth);
            //Debug.Log("=========Enter Again222222222222222== ");
            RecevByte(remainByte, remainLenth);
        }

        private void RecvOneAll( byte[] recvByte, int realLength )
        {
            FileUtils.MemCpy(allRecData, recvByte, curRecLength, realLength);
            curRecLength += realLength;

            RecvOneMsgOver();
        }


        private void RecvOneMsgOver()
        {
            // Debug.Log("RecvOneMsgOver over");

            if (m_callBackReceiveOver != null) m_callBackReceiveOver(GetOneMsg());
            curRecLength = 0;
            allDataLengh = 0;
            allRecData = null;
        }

        public byte[] GetOneMsg()
        {
            return allRecData;
        }


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