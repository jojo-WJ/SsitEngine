namespace SsitEngine.Data
{
    public interface IDataProxy
    {
        /// <summary>
        ///     代理名称
        /// </summary>
        string ProxyName { get; }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T GetData<T>( int id ) where T : class;
    }
}