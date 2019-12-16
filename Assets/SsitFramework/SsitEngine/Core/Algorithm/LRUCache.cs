/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：LruCache（Least Recently Used）算法                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/27 9:16:33  
*│  核心思想：
*│        维护一个缓存对象列表，其中对象列表的排列方式按照访问顺序实现。      
*│        一直没访问的对象，将放在队尾，即将被淘汰。而最近访问的对象将放在队头，最后被淘汰。
*│        通过put数据的时候判断是否内存已经满了，如果满了，则将最近最少使用的数据给剔除掉
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;

namespace SsitEngine.Core.Algorithm
{
    /// <summary>
    ///     缓存实体对象
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    internal class LRUEntity<K, V>
    {
        public K Key { get; set; }
        public V Value { get; set; }
    }

    /// <summary>
    ///     最近最少使用（Least Recently Used）
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class LRUCache<K, V>
    {
        private readonly int m_cacheCount = 50;
        private readonly Dictionary<K, LinkedListNode<LRUEntity<K, V>>> m_linkedDic;
        private readonly LinkedList<LRUEntity<K, V>> m_linkedList;

        /// <summary>
        ///     创建一个缓存器对象
        /// </summary>
        public LRUCache()
        {
            m_linkedList = new LinkedList<LRUEntity<K, V>>();
            m_linkedDic = new Dictionary<K, LinkedListNode<LRUEntity<K, V>>>(m_cacheCount);
        }

        /// <summary>
        ///     创建一个指定容量的缓存器对象
        /// </summary>
        public LRUCache( int capacity )
        {
            m_cacheCount = capacity;
            m_linkedList = new LinkedList<LRUEntity<K, V>>();
            m_linkedDic = new Dictionary<K, LinkedListNode<LRUEntity<K, V>>>(m_cacheCount);
        }

        /// <summary>
        /// </summary>
        public int Count => m_linkedList.Count;

        /// <summary>
        ///     添加缓存对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public V Add( K key, V value )
        {
            V obj = default;
            if (m_linkedDic.ContainsKey(key))
            {
                var node = m_linkedDic[key];
                m_linkedList.Remove(node); //O(1)
                m_linkedList.AddFirst(node);

                node.Value.Value = value;

                obj = node.Value.Value;
            }
            else
            {
                LinkedListNode<LRUEntity<K, V>> newNode = null;

                if (m_linkedList.Count >= m_cacheCount)
                {
                    newNode = m_linkedList.Last; //无需创建新对象
                    m_linkedList.RemoveLast();
                    m_linkedDic.Remove(newNode.Value.Key);
                    obj = newNode.Value.Value;
                }
                else
                {
                    newNode = new LinkedListNode<LRUEntity<K, V>>(new LRUEntity<K, V>());
                }

                newNode.Value.Key = key;
                newNode.Value.Value = value;
                m_linkedList.AddFirst(newNode);
                m_linkedDic.Add(key, newNode);
            }

            return obj;
        }

        /// <summary>
        ///     获取对应key值得缓存对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public V Get( K key )
        {
            if (m_linkedDic.ContainsKey(key))
            {
                var node = m_linkedDic[key];
                m_linkedList.Remove(node); //O(1)
                m_linkedList.AddFirst(node);

                return node.Value.Value;
            }

            return default;
        }

        /// <summary>
        ///     清理缓存器
        /// </summary>
        public void Clear()
        {
            m_linkedList.Clear();
            m_linkedDic.Clear();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public List<K> GetKeyList()
        {
            return m_linkedDic.Keys.ToList();
        }
    }
}