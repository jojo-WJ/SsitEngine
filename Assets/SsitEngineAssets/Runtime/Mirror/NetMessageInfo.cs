/*
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：                                                    
*│　作    者：Xuxin                                              
*│　版    本：1.0.0                                                 
*│　创建时间：2019/12/24 10:46:57                             
*└──────────────────────────────────────────────────────────────┘
*/
using Mirror;
using SsitEngine.Unity;

namespace Framework.NetSocket
{
    public partial class MessagePackage : MessageBase
    {
        //public uint netId;

        public MessagePackage()
        {
            
        }

        #region Mirror Combine

        public override void Deserialize( NetworkReader reader )
        {
            MessageId = reader.ReadPackedInt32();
            MessageBody = MessageDeserializer.Deserialize(MessageId, reader.ReadBytesAndSize());
        }

        public override void Serialize( NetworkWriter writer )
        {
            writer.WritePackedInt32(MessageId);
            writer.WriteBytesAndSize(ProtoBufferUtils.Serialize(MessageBody));
            ;
        }

        #endregion
    }
}