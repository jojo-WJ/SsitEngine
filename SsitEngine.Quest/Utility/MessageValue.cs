/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：任务管理器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/1 12:00:33                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// 消息值类型
    /// </summary>
    public enum MessageValueType
    {
        /// <summary>
        /// 空
        /// </summary>
        None,

        /// <summary>
        /// 整型
        /// </summary>
        Int,

        /// <summary>
        /// 字符串
        /// </summary>
        String
    }

    /// <summary>
    /// 指定一个消息和参数传输的值
    /// </summary>
    [Serializable]
    public class MessageValue
    {
        [Tooltip("Optional int value to pass with message.")] [SerializeField]
        private int m_intValue;

        [Tooltip("Optional string value to pass with message.")] [SerializeField]
        private string m_stringValue;

        [Tooltip("Type of optional value to pass with message.")] [SerializeField]
        private MessageValueType m_valueType = MessageValueType.None;

        public MessageValue()
        {
        }

        public MessageValue( int i )
        {
            m_valueType = MessageValueType.Int;
            m_intValue = i;
        }

        public MessageValue( string s )
        {
            m_valueType = MessageValueType.String;
            m_stringValue = s;
        }

        /// <summary>
        /// Type of optional value to pass with message (int or string).
        /// </summary>
        public MessageValueType ValueType
        {
            get => m_valueType;
            set
            {
                m_valueType = value;
                if (value != MessageValueType.String)
                {
                    m_stringValue = null;
                }
            }
        }

        /// <summary>
        /// Optional int value to pass with message.
        /// </summary>
        public int IntValue
        {
            get => m_intValue;
            set
            {
                ValueType = MessageValueType.Int;
                m_intValue = value;
                m_stringValue = null;
            }
        }

        /// <summary>
        /// Optional string value to pass with message.
        /// </summary>
        public string StringValue
        {
            get => m_stringValue;
            set
            {
                ValueType = MessageValueType.String;
                m_stringValue = value;
            }
        }

        public override string ToString()
        {
            switch (ValueType)
            {
                case MessageValueType.Int:
                    return IntValue.ToString();
                case MessageValueType.String:
                    return StringValue;
                default:
                    return "MessageValue";
            }
        }

        public string EditorNameValue()
        {
            switch (ValueType)
            {
                case MessageValueType.Int:
                    return IntValue.ToString();
                case MessageValueType.String:
                    return StringValue;
                default:
                    return string.Empty;
            }
        }
    }
}