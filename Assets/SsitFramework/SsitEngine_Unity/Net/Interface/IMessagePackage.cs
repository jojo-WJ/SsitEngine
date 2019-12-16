namespace SsitEngine.Unity.NetSocket
{
    public interface IMessagePackage
    {
        int MessageId { get; set; }

        object MessageBody { get; set; }
    }
}