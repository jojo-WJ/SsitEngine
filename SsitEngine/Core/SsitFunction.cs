/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：系统框架带返回类型的委托                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/8 18:21:02                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Core
{
    /// <summary>
    ///     封装一个方法，该方法不具有参数并且有返回值。
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <returns>返回对象</returns>
    public delegate T SsitFunction<out T>();

    /// <summary>
    ///     封装一个方法，该方法具有一个参数并且有返回值。
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <typeparam name="T1">此委托封装的方法的第一个参数的类型</typeparam>
    /// <returns>返回对象</returns>
    public delegate T SsitFunction<in T1, out T>( T1 arg1 );

    /// <summary>
    ///     封装一个方法，该方法具有两个参数并且有返回值。
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <typeparam name="T1">此委托封装的方法的第一个参数的类型</typeparam>
    /// <typeparam name="T2">此委托封装的方法的第二个参数的类型</typeparam>
    /// <returns>返回对象</returns>
    public delegate T SsitFunction<in T1, in T2, out T>( T1 arg1, T2 arg2 );

    /// <summary>
    ///     封装一个方法，该方法具有两个参数并且有返回值。
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <typeparam name="T1">此委托封装的方法的第一个参数的类型</typeparam>
    /// <typeparam name="T2">此委托封装的方法的第二个参数的类型</typeparam>
    /// <typeparam name="T3">此委托封装的方法的第三个参数的类型</typeparam>
    /// <returns>返回对象</returns>
    public delegate T SsitFunction<in T1, in T2, in T3, out T>( T1 arg1, T2 arg2, T3 arg3 );


    /// <summary>
    ///     封装一个方法，该方法具有两个参数并且有返回值。
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <typeparam name="T1">此委托封装的方法的第一个参数的类型</typeparam>
    /// <typeparam name="T2">此委托封装的方法的第二个参数的类型</typeparam>
    /// <typeparam name="T3">此委托封装的方法的第三个参数的类型</typeparam>
    /// <typeparam name="T4">此委托封装的方法的第四个参数的类型</typeparam>
    /// <returns>返回对象</returns>
    public delegate T SsitFunction<in T1, in T2, in T3, in T4, out T>( T1 arg1, T2 arg2, T3 arg3, T4 arg4 );
}