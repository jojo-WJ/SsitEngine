/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：引用管理池                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/20 14:37:18                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace SsitEngine.Core.ReferencePool
{
    /// <summary>
    ///     引用池。
    /// </summary>
    public static partial class ReferencePool
    {
        private static readonly IDictionary<string, ReferenceCollection> ReferenceCollectionMaps =
            new Dictionary<string, ReferenceCollection>();

        #region 属性

        /// <summary>
        ///     获取引用池的数量。
        /// </summary>
        public static int Count
        {
            get
            {
                lock (ReferenceCollectionMaps)
                {
                    return ReferenceCollectionMaps.Count;
                }
            }
        }

        #endregion

        #region 引用获取

        /// <summary>
        ///     从引用池获取引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        public static T Acquire<T>() where T : class, IReference, new()
        {
            return GetReferenceCollection(typeof(T)).Acquire<T>();
        }

        /// <summary>
        ///     从引用池获取引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        /// <returns></returns>
        public static IReference Acquire( Type referenceType )
        {
            InternalCheckReferenceType(referenceType);
            return GetReferenceCollection(referenceType).Acquire();
        }

        #endregion

        #region 引用释放

        /// <summary>
        ///     将引用归还引用池。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="reference">引用。</param>
        public static void Release<T>( T reference ) where T : class, IReference
        {
            if (reference == null)
            {
                throw new SsitEngineException("Reference is invalid.");
            }

            GetReferenceCollection(typeof(T)).Release(reference);
        }

        /// <summary>
        ///     将引用归还引用池。
        /// </summary>
        /// <param name="reference">引用。</param>
        public static void Release( IReference reference )
        {
            if (reference == null)
            {
                throw new SsitEngineException("Reference is invalid.");
            }

            var referenceType = reference.GetType();
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).Release(reference);
        }

        #endregion

        #region 引用添加

        /// <summary>
        ///     向引用池中追加指定数量的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="count">追加数量。</param>
        public static void Add<T>( int count ) where T : class, IReference, new()
        {
            GetReferenceCollection(typeof(T)).Add<T>(count);
        }

        /// <summary>
        ///     向引用池中追加指定数量的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        /// <param name="count">追加数量。</param>
        public static void Add( Type referenceType, int count )
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).Add(count);
        }

        #endregion

        #region 引用移除

        /// <summary>
        ///     从引用池中移除指定数量的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="count">移除数量。</param>
        public static void Remove<T>( int count ) where T : class, IReference
        {
            GetReferenceCollection(typeof(T)).Remove(count);
        }

        /// <summary>
        ///     从引用池中移除指定数量的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        /// <param name="count">移除数量。</param>
        public static void Remove( Type referenceType, int count )
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).Remove(count);
        }

        /// <summary>
        ///     从引用池中移除所有的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        public static void RemoveAll<T>() where T : class, IReference
        {
            GetReferenceCollection(typeof(T)).RemoveAll();
        }

        /// <summary>
        ///     从引用池中移除所有的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        public static void RemoveAll( Type referenceType )
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).RemoveAll();
        }

        #endregion

        #region 引用池辅助成员

        /// <summary>
        ///     获取所有引用池的信息。
        /// </summary>
        /// <returns>所有引用池的信息。</returns>
        public static ReferencePoolInfo[] GetAllReferencePoolInfos()
        {
            var index = 0;
            ReferencePoolInfo[] results = null;

            lock (ReferenceCollectionMaps)
            {
                results = new ReferencePoolInfo[ReferenceCollectionMaps.Count];
                foreach (var referenceCollection in ReferenceCollectionMaps)
                {
                    results[index++] = new ReferencePoolInfo(referenceCollection.Key,
                        referenceCollection.Value.UnusedReferenceCount, referenceCollection.Value.UsingReferenceCount,
                        referenceCollection.Value.AcquireReferenceCount,
                        referenceCollection.Value.ReleaseReferenceCount, referenceCollection.Value.AddReferenceCount,
                        referenceCollection.Value.RemoveReferenceCount);
                }
            }

            return results;
        }

        /// <summary>
        ///     检测引用类型
        /// </summary>
        /// <param name="referenceType"></param>
        private static void InternalCheckReferenceType( Type referenceType )
        {
            if (referenceType == null)
            {
                throw new SsitEngineException("Reference type is invalid.");
            }

            if (!referenceType.IsClass || referenceType.IsAbstract)
            {
                throw new SsitEngineException("Reference type is not a non-abstract class type.");
            }

            if (!typeof(IReference).IsAssignableFrom(referenceType))
            {
                throw new SsitEngineException(TextUtils.Format("Reference type '{0}' is invalid.",
                    referenceType.FullName));
            }
        }

        /// <summary>
        ///     获取对应类型的引用收集器
        /// </summary>
        /// <param name="referenceType"></param>
        /// <returns></returns>
        private static ReferenceCollection GetReferenceCollection( Type referenceType )
        {
            if (referenceType == null)
            {
                throw new SsitEngineException("ReferenceType is invalid.");
            }

            var fullName = referenceType.FullName;
            ReferenceCollection referenceCollection = null;
            lock (ReferenceCollectionMaps)
            {
                if (!ReferenceCollectionMaps.TryGetValue(fullName, out referenceCollection))
                {
                    referenceCollection = new ReferenceCollection(referenceType);
                    ReferenceCollectionMaps.Add(fullName, referenceCollection);
                }
            }

            return referenceCollection;
        }

        /// <summary>
        ///     清除所有引用池。
        /// </summary>
        public static void ClearAll()
        {
            lock (ReferenceCollectionMaps)
            {
                foreach (var referenceCollection in ReferenceCollectionMaps)
                {
                    referenceCollection.Value.RemoveAll();
                }

                ReferenceCollectionMaps.Clear();
            }
        }

        #endregion
    }
}