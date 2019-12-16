using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    ///     触摸手势基类
    /// </summary>
    public abstract class TouchGestureBase
    {
        /// <summary>
        ///     创建手势类型
        /// </summary>
        /// <param name="p_type"></param>
        public TouchGestureBase( EnTouchGestureType p_type )
        {
            Type = p_type;
        }

        public EnTouchGestureType Type { get; }

        /// <summary>
        ///     触摸手势的轮询检测
        /// </summary>
        /// <returns></returns>
        public abstract TouchGestureEventArgs Update();

        /// <summary>
        ///     获取触摸中心点的位置
        /// </summary>
        /// <returns></returns>
        protected Vector2 GetTouchesCenterPosition()
        {
            var center = Vector2.zero;
            var touches = Input.touches;
            for (var i = 0; i < Input.touchCount; i++) center += touches[i].position;
            return center / Input.touchCount;
        }

        /// <summary>
        ///     是否触摸移动
        /// </summary>
        /// <returns></returns>
        protected bool IsMovedTouch()
        {
            var touches = Input.touches;
            for (var i = 0; i < Input.touchCount; i++)
                if (touches[i].phase == TouchPhase.Moved)
                    return true;
            return false;
        }

        public void Destory()
        {
        }
    }
}