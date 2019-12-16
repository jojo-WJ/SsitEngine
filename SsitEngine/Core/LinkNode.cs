/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/27 9:16:33                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Core
{
    /// <summary>
    ///     泛型单链表数据结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LinkNode<T>
    {
        /// <summary>
        ///     创建一个链表节点
        /// </summary>
        public LinkNode()
        {
            Data = default;
            Next = null;
        }

        /// <summary>
        ///     创建一个链表节点
        /// </summary>
        /// <param name="obj">节点对象</param>
        public LinkNode( T obj )
        {
            Data = obj;
            Next = null;
        }

        /// <summary>
        ///     创建一个链表节点
        /// </summary>
        /// <param name="obj">节点对象</param>
        /// <param name="pNext">下一个节点对象</param>
        public LinkNode( T obj, LinkNode<T> pNext )
        {
            Data = obj;
            Next = pNext;
        }

        /// <summary>
        ///     单链表的下个节点
        /// </summary>
        public LinkNode<T> Next { get; set; }

        /// <summary>
        ///     节点数据
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        ///     节点释放
        /// </summary>
        public void Dispose()
        {
            Data = default;
            Next = null;
        }

        /// <summary>
        ///     节点是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsNull()
        {
            return Data != null && Next == null;
        }
    }
}