using UnityEngine;

namespace SsitEngine.Unity.NetSocket
{
    public class MessagePackage
    {
        public byte[] buffer;
        public int bufferSize;

        public Object messageBody;
        public int messageId;
    }
}