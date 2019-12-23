namespace SsitEngine.QuestManager
{
    /// <summary>
    /// 【注意】任务的序列化是间接进行的，需要处理子对象的引用关系
    /// 必须将其数据复制到可序列化的代理对象
    /// </summary>
    public interface IProxySerializationCallbackReceiver
    {
        /// <summary>
        /// 序列化之前调用（处理临时的数据转换）
        /// </summary>
        void OnBeforeProxySerialization();

        /// <summary>
        /// 在对象反序列化后调用，处理从临时的序列化变量重新构造不可序列化的数据
        /// </summary>
        void OnAfterProxyDeserialization();
    }
}