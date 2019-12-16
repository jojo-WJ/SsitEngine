/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/9 11:40:17                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Data
{
    /// <summary>
    ///     数据基类
    /// </summary>
    public abstract class DataBase : AllocatedObject
    {
        /// <summary>
        ///     数据id
        /// </summary>
        public abstract int Id { get; }

        /// <summary>
        ///     创建对象属性
        /// </summary>
        /// <param name="dataId"></param>
        /// <returns></returns>
        public abstract T Create<T>( int dataId ) where T : class;

        /// <summary>
        ///     应用对象（动态数据应用到对象）
        /// </summary>
        /// <param name="obj"></param>
        public abstract void Apply( object obj );
    }
}