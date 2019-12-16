namespace SsitEngine.Unity.NetSocket
{
    public interface IMessageRouter
    {
        void FireMessage( int messageId, object messageBody );
    }
}