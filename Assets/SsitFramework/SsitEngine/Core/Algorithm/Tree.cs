/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/10/21 16:54:21                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace SsitEngine.Core.Algorithm
{
    /// <summary>
    ///     树型结构泛型对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Tree<T>
    {
        /// <summary>
        ///     根节点
        /// </summary>
        protected TreeNode<T> m_Root;

        public TreeNode<T> Root
        {
            get => m_Root;
            set => m_Root = value;
        }
    }


    /// <summary>
    ///     树节点类
    /// </summary>
    public class TreeNode<T>
    {
        // 节点的子节点
        public List<TreeNode<T>> m_ChildNodes;

        // 节点当前深度
        public int m_CurrentDepth;

        // 节点包含对象列表
        public List<T> m_ObjectList;

        public bool Visibility { get; set; } = true; // 是否可视

        public bool Dirty { get; set; } = true; // 是否脏数据(Visibility变成!Visibility)

        public bool Enabled { get; } = true; // 是否启用，任意子节点启用则该节点启用，貌似还不完善，未考虑递归情况


        public virtual object Data { get; set; }
    }
}