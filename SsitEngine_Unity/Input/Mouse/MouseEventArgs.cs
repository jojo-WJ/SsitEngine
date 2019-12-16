using SsitEngine.Core;
using SsitEngine.Core.ReferencePool;
using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    ///     鼠标事件
    /// </summary>
    public enum EnMouseEventType
    {
        /// <summary>
        ///     左键按下
        /// </summary>
        LeftDown,

        /// <summary>
        ///     左键按下抬起（不拖拽）
        /// </summary>
        LeftSingleDown,

        /// <summary>
        ///     左键抬起
        /// </summary>
        LeftUp,

        /// <summary>
        ///     左键长按
        /// </summary>
        LeftHover,

        /// <summary>
        ///     左键双击
        /// </summary>
        LeftDouble,

        /// <summary>
        ///     右键按下
        /// </summary>
        RightDown,

        /// <summary>
        ///     右键抬起
        /// </summary>
        RightUp,

        /// <summary>
        ///     右键长按
        /// </summary>
        RightHover,

        /// <summary>
        ///     右键双击
        /// </summary>
        RightDouble,

        /// <summary>
        ///     鼠标滚轮滑动
        /// </summary>
        ScrollWheel,

        /// <summary>
        ///     鼠标滚轮按下事件
        /// </summary>
        ScrollWheelDown,


        Custom1,
        Custom2,
        Custom3,
        Custom4
    }

    /// <summary>
    ///     按键事件
    /// </summary>
    public class MouseEventArgs : SsitEventArgs, IReference
    {
        /// <summary>
        ///     按下拖拽移动增量
        /// </summary>
        private Vector2 m_delta;

        /// <summary>
        ///     初始位置
        /// </summary>
        private Vector2 m_initialPosition;

        /// <summary>
        ///     按键按下摄像机当前位置
        /// </summary>
        private Vector2 m_position;

        /// <summary>
        ///     鼠标事件类型
        /// </summary>
        private EnMouseEventType m_type;

        /// <summary>
        ///     创建消息触摸的事件
        /// </summary>
        public MouseEventArgs()
        {
        }

        /// <summary>
        ///     创建消息触摸的事件
        /// </summary>
        /// <param name="type">触摸类型</param>
        /// <param name="pos">触摸位置</param>
        public MouseEventArgs( EnMouseEventType type, Vector2 pos )
            : this(type, pos, Vector2.zero)
        {
        }

        /// <summary>
        ///     创建消息触摸的事件
        /// </summary>
        /// <param name="type">触摸类型</param>
        /// <param name="pos">触摸位置</param>
        /// <param name="delta">触摸移动增量</param>
        public MouseEventArgs( EnMouseEventType type, Vector2 pos, Vector2 delta )
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
        public MouseEventArgs( EnMouseEventType type, Vector2 initialPosition, Vector2 position, Vector2 delta )
        {
            m_type = type;
            m_initialPosition = initialPosition;
            m_position = position;
            m_delta = delta;
        }

        /// <summary>
        ///     鼠标事件类型
        /// </summary>
        public EnMouseEventType Type => m_type;

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


        /// <summary>
        ///     清除
        /// </summary>
        public void Clear()
        {
            m_initialPosition = Vector2.zero;
            m_position = Vector2.zero;
            m_delta = Vector2.zero;
        }

        /// <summary>
        ///     设置触摸消息事件参数
        /// </summary>
        /// <param name="type">触摸类型</param>
        /// <param name="initialPosition">起始位置</param>
        /// <param name="position">触摸位置</param>
        /// <param name="delta">触摸移动增量</param>
        public void SetArgs( EnMouseEventType type, Vector2 initialPosition, Vector2 position, Vector2 delta )
        {
            m_type = type;
            m_initialPosition = initialPosition;
            m_position = position;
            m_delta = delta;
        }
    }
}