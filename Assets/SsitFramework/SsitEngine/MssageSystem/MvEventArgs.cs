/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/28 11:27:48                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using SsitEngine.Core.EventPool;
using SsitEngine.Core.ReferencePool;
using SsitEngine.PureMVC.Interfaces;

namespace SsitEngine
{
    /// <summary>
    ///     集成MVC的消息事件体
    /// </summary>
    public class MvEventArgs : BaseEventArgs, INotification
    {
        private ushort m_msgId;

        /// <summary>
        ///     构造方法
        /// </summary>
        public MvEventArgs()
        {
            m_msgId = 0;
            Body = null;
            Values = null;
        }

        /// <summary>
        ///     获取可变参数下标为零的值
        /// </summary>
        public object FirstValue => Values != null && Values.Length > 0 ? Values[0] : null;

        /// <summary>
        ///     获取可变参数下标为零的整型值
        /// </summary>
        public int IntValue
        {
            get
            {
                try
                {
                    return (int) FirstValue;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        /// <summary>
        ///     获取可变参数下标为零的字符值
        /// </summary>
        public string StringValue
        {
            get
            {
                try
                {
                    return (string) FirstValue;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>
        ///     获取可变参数下标为零的布尔值
        /// </summary>
        public bool BoolValue
        {
            get
            {
                try
                {
                    return (bool) FirstValue;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        ///     获取可变参数下标为零的单精度值
        /// </summary>
        public float FloatValue
        {
            get
            {
                try
                {
                    return (float) FirstValue;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }


        /// <summary>
        ///     获取可变参数下标为零的短整值
        /// </summary>
        public ushort UshortValue
        {
            get
            {
                try
                {
                    return (ushort) FirstValue;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        /// <summary>
        ///     可变消息体数据
        /// </summary>
        public object[] Values { get; private set; }

        /// <summary>
        ///     消息Id
        /// </summary>
        public override ushort Id => m_msgId;

        /// <summary>
        ///     消息体
        /// </summary>
        public object Body { get; private set; }

        /// <summary>
        ///     构造方法
        /// </summary>
        /// <param name="msgId">消息id</param>
        /// <param name="body">消息体</param>
        public void SetEventArgs( ushort msgId, object body = null )
        {
            m_msgId = msgId;
            Body = body;
        }

        /// <summary>
        ///     设置消息参数
        /// </summary>
        /// <param name="msgId">消息id</param>
        /// <param name="body">消息体</param>
        /// <param name="values">可变消息参数</param>
        public void SetEventArgs( ushort msgId, object body, params object[] values )
        {
            m_msgId = msgId;
            Body = body;
            Values = values;
        }

        /// <inheritdoc />
        public override void Clear()
        {
            m_msgId = 0;
            if (Body is BaseEventArgs)
                // 回收引用
                ReferencePool.Release((IReference) Body);
            Body = null;
            Values = null;
        }
    }
}