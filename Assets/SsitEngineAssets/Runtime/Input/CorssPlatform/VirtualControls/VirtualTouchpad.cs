using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    /// 一个虚拟触摸板，它将根据相对于起始压力位置的压力位置移动轴。
    /// </summary>
    public class VirtualTouchpad : VirtualAxis, IDragHandler
    {
        [Tooltip(
            "The value to dampen the drag value by when no longer dragging. The higher the value the quicker the drag value will decrease.")]
        [SerializeField]
        protected float m_ActiveDragDamping = 1f;

        private ScheduledEventBase m_ActiveDragScheduler;
        private Transform m_CanvasScalarTransform;
        private Vector2 m_LocalStartPosition;

        private RectTransform m_RectTransform;

        [Tooltip("Should the input value be stopped if there is no movement on the touch pad?")] [SerializeField]
        protected bool m_RequireActiveDrag;

        /// <summary>
        /// Callback when a pointer has dragged the button.
        /// </summary>
        /// <param name="data">The pointer data.</param>
        public void OnDrag( PointerEventData data )
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(m_RectTransform, data.position, null))
            {
                var canvasScale = m_CanvasScalarTransform == null ? Vector3.one : m_CanvasScalarTransform.localScale;
                m_DeltaPosition.x += data.delta.x / canvasScale.x;
                m_DeltaPosition.y += data.delta.y / canvasScale.y;
                Scheduler.Cancel(m_ActiveDragScheduler);
                if (m_RequireActiveDrag)
                    m_ActiveDragScheduler = Scheduler.Schedule(Time.fixedDeltaTime, DampenDeltaPosition);
            }
        }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_RectTransform = GetComponent<RectTransform>();
            m_CanvasScalarTransform = GetComponentInParent<CanvasScaler>().transform;
        }

        /// <summary>
        /// Callback when a pointer has pressed on the button.
        /// </summary>
        /// <param name="data">The pointer data.</param>
        public override void OnPointerDown( PointerEventData data )
        {
            base.OnPointerDown(data);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_RectTransform, data.pressPosition, null,
                out m_LocalStartPosition);
        }

        /// <summary>
        /// Clears the delta drag position.
        /// </summary>
        private void DampenDeltaPosition()
        {
            m_DeltaPosition /= 1 + m_ActiveDragDamping;
            if (m_DeltaPosition.sqrMagnitude > 0.1f)
                m_ActiveDragScheduler = Scheduler.Schedule(Time.fixedDeltaTime, DampenDeltaPosition);
        }

        /// <summary>
        /// Returns the value of the axis.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the axis.</returns>
        public override float GetAxis( string name )
        {
            if (!m_Pressed) return 0;

            if (name == m_HorizontalInputName)
                return m_DeltaPosition.x / (m_RectTransform.sizeDelta.x - m_LocalStartPosition.x);
            return m_DeltaPosition.y / (m_RectTransform.sizeDelta.y - m_LocalStartPosition.y);
        }
    }
}