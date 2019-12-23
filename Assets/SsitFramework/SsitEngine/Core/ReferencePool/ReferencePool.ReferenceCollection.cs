/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：引用集合器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/20 14:35:45                     
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
        /// <summary>
        ///     引用集合收集器
        /// </summary>
        private sealed class ReferenceCollection
        {
            #region 变量

            /// <summary>
            ///     引用队列
            /// </summary>
            private readonly Queue<IReference> m_References;

            #endregion

            #region 构造及初始化

            /// <summary>
            ///     创建一个引用收集器
            /// </summary>
            /// <param name="referenceType">引用类型</param>
            public ReferenceCollection( Type referenceType )
            {
                m_References = new Queue<IReference>();
                ReferenceType = referenceType;
                UsingReferenceCount = 0;
                AcquireReferenceCount = 0;
                ReleaseReferenceCount = 0;
                AddReferenceCount = 0;
                RemoveReferenceCount = 0;
            }

            #endregion

            #region 释放

            /// <summary>
            ///     释放引用
            /// </summary>
            /// <param name="reference"></param>
            public void Release( IReference reference )
            {
                reference.Clear();
                lock (m_References)
                {
                    if (m_References.Contains(reference))
                    {
                        return;
                    }

                    m_References.Enqueue(reference);
                }

                ReleaseReferenceCount++;
                UsingReferenceCount--;
            }

            #endregion

            #region 属性

            public Type ReferenceType { get; }

            public int UnusedReferenceCount => m_References.Count;

            public int UsingReferenceCount { get; private set; }

            public int AcquireReferenceCount { get; private set; }

            public int ReleaseReferenceCount { get; private set; }

            public int AddReferenceCount { get; private set; }

            public int RemoveReferenceCount { get; private set; }

            #endregion

            #region 获取

            /// <summary>
            ///     泛型获取引用
            /// </summary>
            /// <typeparam name="T">指定引用的类型</typeparam>
            /// <returns>返回指定类型</returns>
            public T Acquire<T>() where T : class, IReference, new()
            {
                if (typeof(T) != ReferenceType)
                {
                    throw new SsitEngineException("Type is invalid.");
                }

                UsingReferenceCount++;
                AcquireReferenceCount++;
                lock (m_References)
                {
                    if (m_References.Count > 0)
                    {
                        return (T) m_References.Dequeue();
                    }
                }
                //没有话就new()
                AddReferenceCount++;
                return new T();
            }

            /// <summary>
            ///     获取引用
            /// </summary>
            /// <returns></returns>
            public IReference Acquire()
            {
                UsingReferenceCount++;
                AcquireReferenceCount++;
                lock (m_References)
                {
                    if (m_References.Count > 0)
                    {
                        return m_References.Dequeue();
                    }
                }

                AddReferenceCount++;
                return (IReference) Activator.CreateInstance(ReferenceType);
            }

            #endregion

            #region 添加

            /// <summary>
            ///     泛型添加指定个数的引用到引用收集器
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="count"></param>
            public void Add<T>( int count ) where T : class, IReference, new()
            {
                if (typeof(T) != ReferenceType)
                {
                    throw new SsitEngineException("Type is invalid.");
                }

                lock (m_References)
                {
                    AddReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Enqueue(new T());
                    }
                }
            }

            /// <summary>
            ///     添加指定个数的引用到引用收集器
            /// </summary>
            /// <param name="count">指定添加的引用个数</param>
            public void Add( int count )
            {
                lock (m_References)
                {
                    AddReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Enqueue((IReference) Activator.CreateInstance(ReferenceType));
                    }
                }
            }

            #endregion

            #region 移除

            /// <summary>
            ///     添加指定个数的引用到引用收集器
            /// </summary>
            /// <param name="count">指定添加的引用个数</param>
            public void Remove( int count )
            {
                lock (m_References)
                {
                    if (count > m_References.Count)
                    {
                        count = m_References.Count;
                    }

                    RemoveReferenceCount += count;
                    while (count-- > 0)
                    {
                        m_References.Dequeue();
                    }
                }
            }

            /// <summary>
            ///     移除引用收集器的所有引用
            /// </summary>
            public void RemoveAll()
            {
                lock (m_References)
                {
                    RemoveReferenceCount += m_References.Count;
                    m_References.Clear();
                }
            }

            #endregion
        }
    }
}