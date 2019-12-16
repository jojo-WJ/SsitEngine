/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：抽象引用对象                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/18 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine
{
    /// <summary>
    ///     底层变量基类（对object的一次封装）。
    /// </summary>
    public abstract class AllocatedObject
    {
        /// <summary>
        ///     脏数据标识
        /// </summary>
        protected bool isDirty;

        /// <summary>
        ///     初始化变量的新实例。
        /// </summary>
        protected AllocatedObject()
        {
            isDirty = false;
        }

        /// <summary>
        ///     脏数据标识
        /// </summary>
        public bool IsDirty
        {
            get => isDirty;
            set => isDirty = value;
        }

        /// <summary>
        ///     重置变量值。
        /// </summary>
        public virtual void Reset()
        {
        }

        /// <summary>
        ///     销毁对象
        /// </summary>
        public virtual void Shutdown()
        {
            isDirty = true;
        }
    }
}