using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    public class TouchGestureTap : TouchGestureBase
    {
        private const float MAX_TOUCH_TIME_FOR_TAP = 1f; // in seconds
        private const float MAX_DISTANCE_FOR_TAP = 0.1f; // in pixel/dpi

        private int m_framesNoTouch;
        private bool m_isTapDown;
        private Vector2 m_tapPos = Vector3.zero;
        private float m_tapStartTime;

        public TouchGestureTap()
            : base(EnTouchGestureType.TAP)
        {
        }

        public override TouchGestureEventArgs Update()
        {
            if (m_isTapDown)
            {
                if (Input.touchCount == 0)
                {
                    // screen was tapped
                    m_framesNoTouch++;
                    m_isTapDown = false;
                    return new TouchGestureEventArgs(Type, m_tapPos);
                }
                // no tap possible if
                if (
                    // there are more fingers than one or
                    Input.touchCount != 1 ||
                    // screen was touched too long for a tap
                    Time.realtimeSinceStartup - m_tapStartTime > MAX_TOUCH_TIME_FOR_TAP ||
                    // finger was moved too far for a tap
                    (Input.touches[0].position - m_tapPos).magnitude > MAX_DISTANCE_FOR_TAP)
                {
                    m_framesNoTouch = 0;
                    m_isTapDown = false;
                }
            }
            else if (Input.touchCount == 0)
            {
                // no finger -> no tap
                m_framesNoTouch++;
                m_isTapDown = false;
            }
            else if (Input.touchCount == 1)
            {
                // one finger -> possible tap if at least two frames passed after last touch
                if (m_framesNoTouch > 1)
                {
                    m_isTapDown = true;
                    m_tapStartTime = Time.realtimeSinceStartup;
                    m_tapPos = Input.touches[0].position;
                }
                m_framesNoTouch = 0;
            }
            else
            {
                // more than one finger -> no tap
                m_framesNoTouch = 0;
                m_isTapDown = false;
            }
            return null;
        }
    }
}