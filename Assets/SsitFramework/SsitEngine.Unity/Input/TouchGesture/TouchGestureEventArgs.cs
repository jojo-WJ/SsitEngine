using SsitEngine.Core;
using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    public class TouchGestureEventArgs : SsitEventArgs
    {
        /// <summary>
        ///     触摸移动增量
        /// </summary>
        private readonly Vector2 m_delta;

        /// <summary>
        ///     初始位置
        /// </summary>
        private readonly Vector2 m_initialPosition;

        /// <summary>
        ///     当前位置
        /// </summary>
        private readonly Vector2 m_position;

        /// <summary>
        ///     触摸类型
        /// </summary>
        private readonly EnTouchGestureType m_type;

        /// <summary>
        ///     创建消息触摸的事件
        /// </summary>
        /// <param name="type">触摸类型</param>
        /// <param name="pos">触摸位置</param>
        public TouchGestureEventArgs( EnTouchGestureType type, Vector2 pos )
            : this(type, pos, Vector2.zero)
        {
        }

        /// <summary>
        ///     创建消息触摸的事件
        /// </summary>
        /// <param name="type">触摸类型</param>
        /// <param name="pos">触摸位置</param>
        /// <param name="delta">触摸移动增量</param>
        public TouchGestureEventArgs( EnTouchGestureType type, Vector2 pos, Vector2 delta )
            : this(type, pos, pos, delta)
        {
        }

        /// <summary>
        ///     创建触摸消息事件
        /// </summary>
        /// <param name="type">触摸类型</param>
        /// <param name="initialPosition">起始位置</param>
        /// <param name="position">触摸位置</param>
        /// <param name="delta">触摸移动增量</param>
        public TouchGestureEventArgs( EnTouchGestureType type, Vector2 initialPosition, Vector2 position,
            Vector2 delta )
        {
            m_type = type;
            m_initialPosition = initialPosition;
            m_position = position;
            m_delta = delta;
        }

        /// <summary>
        ///     触摸类型
        /// </summary>
        public EnTouchGestureType Type => m_type;

        /// <summary>
        ///     初始位置
        /// </summary>
        public Vector2 InitialPosition => m_initialPosition;

        /// <summary>
        ///     当前位置
        /// </summary>
        public Vector2 Position => m_position;

        /// <summary>
        ///     触摸移动增量
        /// </summary>
        public Vector2 Delta => m_delta;
    }
}