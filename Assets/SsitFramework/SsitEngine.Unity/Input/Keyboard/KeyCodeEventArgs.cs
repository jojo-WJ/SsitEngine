using SsitEngine.Core;
using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    ///     按键事件
    /// </summary>
    public class KeyCodeEventArgs : SsitEventArgs
    {
        /// <summary>
        ///     按下拖拽移动增量
        /// </summary>
        private readonly Vector2 m_delta;

        /// <summary>
        ///     初始位置
        /// </summary>
        private readonly Vector2 m_initialPosition;

        /// <summary>
        ///     触摸类型
        /// </summary>
        private readonly KeyCode m_key;

        /// <summary>
        ///     按键按下摄像机当前位置
        /// </summary>
        private readonly Vector2 m_position;

        /// <summary>
        ///     创建消息触摸的事件
        /// </summary>
        /// <param name="key">触摸类型</param>
        /// <param name="pos">触摸位置</param>
        public KeyCodeEventArgs( KeyCode key, Vector2 pos )
            : this(key, pos, Vector2.zero)
        {
        }

        /// <summary>
        ///     创建消息触摸的事件
        /// </summary>
        /// <param name="key">触摸类型</param>
        /// <param name="pos">触摸位置</param>
        /// <param name="delta">触摸移动增量</param>
        public KeyCodeEventArgs( KeyCode key, Vector2 pos, Vector2 delta )
            : this(key, pos, pos, delta)
        {
        }

        /// <summary>
        ///     创建触摸消息事件
        /// </summary>
        /// <param name="key">触摸类型</param>
        /// <param name="initialPosition">起始位置</param>
        /// <param name="position">触摸位置</param>
        /// <param name="delta">触摸移动增量</param>
        public KeyCodeEventArgs( KeyCode key, Vector2 initialPosition, Vector2 position, Vector2 delta )
        {
            m_key = key;
            m_initialPosition = initialPosition;
            m_position = position;
            m_delta = delta;
        }

        /// <summary>
        ///     触摸类型
        /// </summary>
        public KeyCode Key => m_key;

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