/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：对象池释放委托事件                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月8日                             
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace SsitEngine.Core.ObjectPool
{
    /// <summary>
    ///     释放对象筛选函数。
    /// </summary>
    /// <typeparam name="T">对象类型。</typeparam>
    /// <param name="candidateObjects">要筛选的对象集合。</param>
    /// <param name="toReleaseCount">需要释放的对象数量。</param>
    /// <param name="expireTime">对象过期参考时间。</param>
    /// <returns>经筛选需要释放的对象集合。</returns>
    public delegate LinkedList<T> ReleaseObjectFilterCallback<T>( LinkedList<T> candidateObjects, int toReleaseCount,
        DateTime expireTime );
}