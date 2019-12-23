using System;
using SsitEngine.Core.EventPool;
using UnityEngine;

namespace SsitEngine.QuestManager
{
    /// <summary>
    /// This struct is passed to listeners of the MessageSystem when a message is sent.
    /// </summary>
    [Serializable]
    public class QuestMessageArgs : BaseEventArgs
    {
        public ushort msgId;

        public string parameter;

        /// <summary>
        /// Reference to the message sender (e.g., GameObject or possibly custom-defined string ID).
        /// </summary>
        public object sender;

        /// <summary>
        /// Reference to the message target (e.g., GameObject or possibly custom-defined string ID).
        /// Typically null or blank string is interpreted as broadcast to all targets.
        /// </summary>
        public object target;

        public object[] values;

        public QuestMessageArgs()
        {
        }

        public QuestMessageArgs( ushort msgId, object sender, object target, string parameter, object[] values = null )
        {
            this.sender = sender;
            this.target = target;
            this.msgId = msgId;
            this.parameter = parameter;
            this.values = values;
        }

        public QuestMessageArgs( ushort msgId, object sender, string parameter, object[] values = null )
        {
            this.sender = sender;
            target = null;
            this.msgId = msgId;
            this.parameter = parameter;
            this.values = values;
        }

        /// <summary>
        /// If true, the message arguments specify a target.
        /// </summary>
        public bool HasTarget => !(target == null || string.IsNullOrEmpty(TargetString));

        /// <summary>
        /// True if the target value is a string or StringField.
        /// </summary>
        public bool IsTargetString
        {
            get
            {
                var type = target != null ? target.GetType() : null;
                return target != null && type == typeof(string);
            }
        }


        /// <summary>
        /// If the target is a string or StringField, its value.
        /// </summary>
        public string TargetString
        {
            get
            {
                if (target == null)
                {
                    return string.Empty;
                }
                return (string) target;
            }
        }

        /// <inheritdoc />
        public override ushort Id => msgId;

        /// <summary>
        /// 设置任务消息参数
        /// </summary>
        /// <param name="msgId">消息id</param>
        /// <param name="sender">发送者</param>
        /// <param name="target">目标者</param>
        /// <param name="parameter">参数</param>
        /// <param name="values">可变参数值</param>
        public void SetQuestMessageParm( ushort msgId, object sender, object target, string parameter,
            object[] values = null )
        {
            this.msgId = msgId;
            this.sender = sender;
            this.target = target;
            this.parameter = parameter;
            this.values = values;
        }

        /// <inheritdoc />
        public override void Clear()
        {
            msgId = 0;
            sender = null;
            target = null;
            parameter = null;
            values = null;
        }

        #region Internal Members

        public bool Matches( ushort message, ushort parameter )
        {
            return message == msgId;
        }

        public bool Matches( string message )
        {
            return message == msgId.ToString();
        }

        /// <summary>
        /// Returns true if the args' sender matches a required sender.
        /// </summary>
        public bool IsRequiredSender( string requiredSender )
        {
            return string.IsNullOrEmpty(requiredSender) || string.Equals(requiredSender, GetSenderString());
        }

        /// <summary>
        /// Returns true if the args' target matches a required target.
        /// </summary>
        public bool IsRequiredTarget( string requiredTarget )
        {
            return string.IsNullOrEmpty(requiredTarget) || string.Equals(requiredTarget, GetTargetString());
        }

        /// <summary>
        /// Returns the string name of the sender.
        /// </summary>
        public string GetSenderString()
        {
            return GetObjectString(sender);
        }

        /// <summary>
        /// Returns the string name of the target.
        /// </summary>
        public string GetTargetString()
        {
            return GetObjectString(target);
        }

        private string GetObjectString( object obj )
        {
            if (obj == null)
            {
                return string.Empty;
            }
            var type = obj.GetType();
            if (type == typeof(string))
            {
                return (string) obj;
            }
            if (type == typeof(GameObject))
            {
                return (obj as GameObject).name;
            }
            if (type == typeof(Component))
            {
                return (obj as Component).name;
            }
            return obj.ToString();
        }

        /// <summary>
        /// 可变参数数组下标为零的值
        /// </summary>
        public object FirstValue => values != null && values.Length > 0 ? values[0] : null;

        /// <summary>
        /// 可变参数数组下标为零的整形值
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
        /// 可变参数数组下标为零的字符串
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

        #endregion
    }
}