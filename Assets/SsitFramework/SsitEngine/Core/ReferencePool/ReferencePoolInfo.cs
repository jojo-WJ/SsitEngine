/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：引用池信息                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/20 14:35:45                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Core.ReferencePool
{
    /// <summary>
    ///     引用池信息。
    /// </summary>
    public sealed class ReferencePoolInfo
    {
        /// <summary>
        ///     初始化引用池信息的新实例。
        /// </summary>
        /// <param name="typeName">引用池类型名称。</param>
        /// <param name="unUsedReferenceCount">未使用引用数量。</param>
        /// <param name="usingReferenceCount">正在使用引用数量。</param>
        /// <param name="acquireReferenceCount">获取引用数量。</param>
        /// <param name="releaseReferenceCount">归还引用数量。</param>
        /// <param name="addReferenceCount">增加引用数量。</param>
        /// <param name="removeReferenceCount">移除引用数量。</param>
        public ReferencePoolInfo( string typeName, int unUsedReferenceCount, int usingReferenceCount,
            int acquireReferenceCount, int releaseReferenceCount, int addReferenceCount, int removeReferenceCount )
        {
            TypeName = typeName;
            UnUsedReferenceCount = unUsedReferenceCount;
            UsingReferenceCount = usingReferenceCount;
            AcquireReferenceCount = acquireReferenceCount;
            ReleaseReferenceCount = releaseReferenceCount;
            AddReferenceCount = addReferenceCount;
            RemoveReferenceCount = removeReferenceCount;
        }

        /// <summary>
        ///     获取引用池类型名称。
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        ///     获取未使用引用数量。
        /// </summary>
        public int UnUsedReferenceCount { get; }

        /// <summary>
        ///     获取正在使用引用数量。
        /// </summary>
        public int UsingReferenceCount { get; }

        /// <summary>
        ///     获取获取引用数量。
        /// </summary>
        public int AcquireReferenceCount { get; }

        /// <summary>
        ///     获取归还引用数量。
        /// </summary>
        public int ReleaseReferenceCount { get; }

        /// <summary>
        ///     获取增加引用数量。
        /// </summary>
        public int AddReferenceCount { get; }

        /// <summary>
        ///     获取移除引用数量。
        /// </summary>
        public int RemoveReferenceCount { get; }
    }
}