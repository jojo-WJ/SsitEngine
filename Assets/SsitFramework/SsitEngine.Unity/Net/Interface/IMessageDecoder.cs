namespace SsitEngine.Unity.NetSocket
{
    public interface IMessageDecoder
    {
        IMessagePackage decode( byte[] data );
    }
}