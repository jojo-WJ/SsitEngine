/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：对象管理器接口                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月8日                             
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Core.ObjectPool
{
    /// <summary>
    ///     对象池管理器。
    /// </summary>
    public interface IObjectPoolManager
    {
        /// <summary>
        ///     获取对象池数量。
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     检查是否存在对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <returns>是否存在对象池。</returns>
        bool HasObjectPool<T>( string name ) where T : ObjectBase;

        /// <summary>
        ///     获取对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <returns>要获取的对象池。</returns>
        IObjectPool<T> GetObjectPool<T>( string name ) where T : ObjectBase;

        /// <summary>
        ///     获取所有对象池。
        /// </summary>
        /// <param name="sort">是否根据对象池的优先级排序。</param>
        /// <returns>所有对象池。</returns>
        ObjectPoolBase[] GetAllObjectPools( bool sort );

        /// <summary>
        ///     创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <param name="autoReleaseInterval">对象池自动释放可释放对象的间隔秒数。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <param name="loadFunction">对象池的对象加载回调。</param>
        /// <param name="spawncondition">对象池的过滤附加条件。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        IObjectPool<T> CreateSingleSpawnObjectPool<T>( string name, float autoReleaseInterval, int capacity,
            float expireTime, int priority, SsitFunction<T> loadFunction = null,
            SsitFunction<bool> spawncondition = null ) where T : ObjectBase;

        /// <summary>
        ///     创建允许多次获取的对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <param name="autoReleaseInterval">对象池自动释放可释放对象的间隔秒数。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <param name="loadFunction">对象池的对象加载回调。</param>
        /// <param name="spawncondition">对象池的过滤附加条件。</param>
        /// <returns>要创建的允许多次获取的对象池。</returns>
        IObjectPool<T> CreateMultiSpawnObjectPool<T>( string name, float autoReleaseInterval, int capacity,
            float expireTime, int priority, SsitFunction<T> loadFunction = null,
            SsitFunction<bool> spawncondition = null ) where T : ObjectBase;

        /// <summary>
        ///     销毁对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="name">要销毁的对象池名称。</param>
        /// <returns>是否销毁对象池成功。</returns>
        bool DestroyObjectPool<T>( string name ) where T : ObjectBase;

        /// <summary>
        ///     销毁对象池。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="objectPool">要销毁的对象池。</param>
        /// <returns>是否销毁对象池成功。</returns>
        bool DestroyObjectPool<T>( IObjectPool<T> objectPool ) where T : ObjectBase;

        /// <summary>
        ///     释放对象池中的可释放对象。
        /// </summary>
        void Release();

        /// <summary>
        ///     释放对象池中的所有未使用对象。
        /// </summary>
        void ReleaseAllUnused();
    }
}