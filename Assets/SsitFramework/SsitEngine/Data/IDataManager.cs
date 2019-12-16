/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：数据管理器接口                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/9 13:45:30                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Data
{
    /// <summary>
    ///     数据管理器的接口
    /// </summary>
    public interface IDataManager
    {
        /// <summary>
        ///     数据加载接口
        /// </summary>
        IDataLoader Loader { get; }

        /// <summary>
        ///     获取数据
        /// </summary>
        /// <param name="index">数据索引</param>
        /// <param name="id">数据id</param>
        /// <returns>数据</returns>
        T GetData<T>( int index, int id ) where T : class;

        /// <summary>
        ///     获取数据代理
        /// </summary>
        /// <param name="index">数据代理索引</param>
        /// <returns>数据代理</returns>
        T GetDataProxy<T>( int index ) where T : class, IDataProxy;
    }
}