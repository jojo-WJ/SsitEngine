/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：对象池信息                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月8日                             
*└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace SsitEngine.Core.ObjectPool
{
    /// <summary>
    ///     对象信息。
    /// </summary>
    public struct ObjectInfo
    {
        /// <summary>
        ///     初始化对象信息的新实例。
        /// </summary>
        /// <param name="name">对象名称。</param>
        /// <param name="locked">对象是否被加锁。</param>
        /// <param name="priority">对象的优先级。</param>
        /// <param name="lastUseTime">对象上次使用时间。</param>
        /// <param name="spawnCount">对象的获取计数。</param>
        public ObjectInfo( string name, bool locked, int priority, DateTime lastUseTime, int spawnCount )
        {
            Name = name;
            Locked = locked;
            Priority = priority;
            LastUseTime = lastUseTime;
            SpawnCount = spawnCount;
        }

        /// <summary>
        ///     获取对象名称。
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     获取对象是否被加锁。
        /// </summary>
        public bool Locked { get; }

        /// <summary>
        ///     获取对象的优先级。
        /// </summary>
        public int Priority { get; }

        /// <summary>
        ///     获取对象上次使用时间。
        /// </summary>
        public DateTime LastUseTime { get; }

        /// <summary>
        ///     获取对象是否正在使用。
        /// </summary>
        public bool IsInUse => SpawnCount > 0;

        /// <summary>
        ///     获取对象的获取计数。
        /// </summary>
        public int SpawnCount { get; }
    }
}