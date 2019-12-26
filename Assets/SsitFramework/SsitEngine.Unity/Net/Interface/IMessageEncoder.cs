namespace SsitEngine.Unity.NetSocket
{
    public interface IMessageEncoder
    {
        byte[] encode( IMessagePackage message );
    }
}