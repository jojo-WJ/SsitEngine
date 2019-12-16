using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    public class TouchGestureZoom : TouchGestureBase
    {
        private const float MIN_RELATIVE_ZOOM_THRESHOLD = 0.1f;
        private bool m_isZooming;

        private float m_lastZoomDist = -1;

        public TouchGestureZoom()
            : base(EnTouchGestureType.ZOOM)
        {
        }

        public override TouchGestureEventArgs Update()
        {
            if (Input.touchCount == 2) // two fingers for zooming
            {
                var touches = Input.touches;
                var currZoomDist = (touches[1].position - touches[0].position).magnitude;
                if (m_lastZoomDist == -1)
                {
                    m_lastZoomDist = currZoomDist;
                }
                else
                {
                    float screenSize = Mathf.Min(Screen.width, Screen.height);
                    if (Mathf.Abs(currZoomDist - m_lastZoomDist) / screenSize > MIN_RELATIVE_ZOOM_THRESHOLD ||
                        m_isZooming)
                    {
                        if (!m_isZooming)
                        {
                            m_isZooming = true;
                            m_lastZoomDist = currZoomDist;
                        }
                        else
                        {
                            var zoomValue = (currZoomDist - m_lastZoomDist) / screenSize;
                            m_lastZoomDist = currZoomDist;
                            return new TouchGestureEventArgs(Type, GetTouchesCenterPosition(), Vector2.one * zoomValue);
                        }
                    }
                }
            }
            else
            {
                m_lastZoomDist = -1;
                m_isZooming = false;
            }
            return null;
        }
    }
}