/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：系统框架不带返回类型的委托                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月8日                             
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Core
{
    /// <summary>
    ///     封装一个方法，该方法不具有参数并且不返回值。
    /// </summary>
    public delegate void SsitAction();

    /// <summary>
    ///     封装一个方法，该方法只有一个参数并且不返回值。
    /// </summary>
    /// <typeparam name="T">此委托封装的方法的参数类型。</typeparam>
    /// <param name="obj">此委托封装的方法的参数。</param>
    public delegate void SsitAction<in T>( T obj );

    /// <summary>
    ///     封装一个方法，该方法具有两个参数并且不返回值。
    /// </summary>
    /// <typeparam name="T1">此委托封装的方法的第一个参数的类型。</typeparam>
    /// <typeparam name="T2">此委托封装的方法的第二个参数的类型。</typeparam>
    /// <param name="arg1">此委托封装的方法的第一个参数。</param>
    /// <param name="arg2">此委托封装的方法的第二个参数。</param>
    public delegate void SsitAction<in T1, in T2>( T1 arg1, T2 arg2 );

    /// <summary>
    ///     封装一个方法，该方法具有三个参数并且不返回值。
    /// </summary>
    /// <typeparam name="T1">此委托封装的方法的第一个参数的类型。</typeparam>
    /// <typeparam name="T2">此委托封装的方法的第二个参数的类型。</typeparam>
    /// <typeparam name="T3">此委托封装的方法的第三个参数的类型。</typeparam>
    /// <param name="arg1">此委托封装的方法的第一个参数。</param>
    /// <param name="arg2">此委托封装的方法的第二个参数。</param>
    /// <param name="arg3">此委托封装的方法的第三个参数。</param>
    public delegate void SsitAction<in T1, in T2, in T3>( T1 arg1, T2 arg2, T3 arg3 );

    /// <summary>
    ///     封装一个方法，该方法具有四个参数并且不返回值。
    /// </summary>
    /// <typeparam name="T1">此委托封装的方法的第一个参数的类型。</typeparam>
    /// <typeparam name="T2">此委托封装的方法的第二个参数的类型。</typeparam>
    /// <typeparam name="T3">此委托封装的方法的第三个参数的类型。</typeparam>
    /// <typeparam name="T4">此委托封装的方法的第四个参数的类型。</typeparam>
    /// <param name="arg1">此委托封装的方法的第一个参数。</param>
    /// <param name="arg2">此委托封装的方法的第二个参数。</param>
    /// <param name="arg3">此委托封装的方法的第三个参数。</param>
    /// <param name="arg4">此委托封装的方法的第四个参数。</param>
    public delegate void SsitAction<in T1, in T2, in T3, in T4>( T1 arg1, T2 arg2, T3 arg3, T4 arg4 );
}