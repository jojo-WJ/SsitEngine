using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    ///     按压手势
    /// </summary>
    public class TouchGesturePress : TouchGestureBase
    {
        /// <summary>
        ///     手指个数
        /// </summary>
        private readonly int m_fingerCount;

        /// <summary>
        ///     检测前的最小触摸时间
        /// </summary>
        private readonly float m_minTouchTimeBeforeDetection;

        /// <summary>
        ///     起始中心位置
        /// </summary>
        private Vector2 m_initialCenterPosition = Vector2.zero;

        /// <summary>
        ///     是否按下
        /// </summary>
        private bool m_isTapDown;

        /// <summary>
        ///     没有点击的等待时间
        /// </summary>
        private bool m_isWaitingForNoTap;

        /// <summary>
        ///     终止中心位置
        /// </summary>
        private Vector2 m_lastCenterPosition = Vector2.zero;

        /// <summary>
        ///     记录按下开始时间
        /// </summary>
        private float m_tapStartTime;

        /// <summary>
        ///     创建一个按压手势（测试版）
        /// </summary>
        /// <param name="p_type"></param>
        /// <param name="p_fingerCount"></param>
        /// <param name="p_minTouchTimeBeforeDetection"></param>
        public TouchGesturePress( EnTouchGestureType p_type, int p_fingerCount, float p_minTouchTimeBeforeDetection )
            : base(p_type)
        {
            m_fingerCount = p_fingerCount;
            m_minTouchTimeBeforeDetection = p_minTouchTimeBeforeDetection;
        }

        /// <inheritdoc />
        public override TouchGestureEventArgs Update()
        {
            if (m_isTapDown)
            {
#if UNITY_EDITOR
                if (m_fingerCount == 1 && Input.GetMouseButton(0))
                {
                    // simulated press
                    if (Time.realtimeSinceStartup - m_tapStartTime > m_minTouchTimeBeforeDetection)
                    {
                        m_isWaitingForNoTap = false;
                        var currCenter = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                        var delta = currCenter - m_lastCenterPosition;
                        m_lastCenterPosition = currCenter;
                        return new TouchGestureEventArgs(Type, m_initialCenterPosition, currCenter, delta);
                    }
                }
                else
#endif
                if (Input.touchCount != m_fingerCount)
                {
                    m_isWaitingForNoTap = true;
                    m_isTapDown = false;
                }
                else if (Time.realtimeSinceStartup - m_tapStartTime > m_minTouchTimeBeforeDetection)
                {
                    // press
                    m_isWaitingForNoTap = false;
                    var currCenter = GetTouchesCenterPosition();
                    var delta = currCenter - m_lastCenterPosition;
                    m_lastCenterPosition = currCenter;
                    return new TouchGestureEventArgs(Type, m_initialCenterPosition, currCenter, delta);
                }
            }
#if UNITY_EDITOR
            else if (Input.GetMouseButton(0) && m_fingerCount == 1)
            {
                // simulate touch with mouse
                if (!m_isWaitingForNoTap)
                {
                    m_isTapDown = true;
                    m_tapStartTime = Time.realtimeSinceStartup;
                    m_lastCenterPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                    m_initialCenterPosition = new Vector2(m_lastCenterPosition.x, m_lastCenterPosition.y);
                }
            }
#endif
            else if (Input.touchCount == m_fingerCount)
            {
                // m_fingerCount finger on the screen -> possible press
                if (!m_isWaitingForNoTap)
                {
                    m_isTapDown = true;
                    m_tapStartTime = Time.realtimeSinceStartup;
                    m_lastCenterPosition = GetTouchesCenterPosition();
                    m_initialCenterPosition = new Vector2(m_lastCenterPosition.x, m_lastCenterPosition.y);
                }
            }
            else
            {
                // more or less than m_fingerCount finger -> no press
                m_isTapDown = false;
                // if more fingers than needed first wait for no tap this way 2 fingers are not recognized when 3 finger gesture is ending
                if (Input.touchCount > m_fingerCount)
                {
                    m_isWaitingForNoTap = true;
                }
            }
            if (m_isWaitingForNoTap && Input.touchCount == 0)
            {
                m_isWaitingForNoTap = false;
            }
            return null;
        }
    }
}