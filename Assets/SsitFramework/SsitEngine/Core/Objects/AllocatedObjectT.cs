/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：可分配的泛型对象                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/18 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

using System;

namespace SsitEngine
{
    /// <summary>
    ///     分配的对象（用于记录资源对象的引用）
    /// </summary>
    /// <typeparam name="T">变量类型。</typeparam>
    public class AllocatedObject<T> : AllocatedObject
    {
        /// <summary>
        ///     分配对象
        /// </summary>
        protected T data;

        /// <summary>
        ///     初始化变量的新实例。
        /// </summary>
        public AllocatedObject()
        {
            data = default;
        }

        /// <summary>
        ///     初始化变量的新实例。
        /// </summary>
        /// <param name="data">初始值。</param>
        public AllocatedObject( T data )
        {
            this.data = data;
        }

        /// <summary>
        ///     获取变量类型。
        /// </summary>
        public virtual Type Type => typeof(T);

        /// <summary>
        ///     获取或设置变量值。
        /// </summary>
        public T Data
        {
            get => data;
            set => data = value;
        }

        /// <summary>
        ///     获取脏数据标识
        /// </summary>
        /// <returns></returns>
        public bool GetDirty()
        {
            return IsDirty;
        }

        /// <summary>
        ///     设置脏数据标识
        /// </summary>
        /// <param name="value"></param>
        public void SetDirty( bool value )
        {
            IsDirty = value;
        }

        /// <summary>
        ///     获取变量值。
        /// </summary>
        /// <returns>变量值。</returns>
        public virtual object GetValue()
        {
            return data;
        }

        /// <summary>
        ///     设置变量值。
        /// </summary>
        /// <param name="value">变量值。</param>
        public virtual void SetValue( object value )
        {
            data = (T) value;
        }

        /// <summary>
        ///     重置变量值。
        /// </summary>
        public override void Reset()
        {
            data = default;
            IsDirty = false;
        }

        /// <summary>
        ///     关闭并销毁对象
        /// </summary>
        public override void Shutdown()
        {
        }

        /// <summary>
        ///     获取变量字符串。
        /// </summary>
        /// <returns>变量字符串。</returns>
        public override string ToString()
        {
            return data != null ? data.ToString() : "<Null>";
        }
    }
}