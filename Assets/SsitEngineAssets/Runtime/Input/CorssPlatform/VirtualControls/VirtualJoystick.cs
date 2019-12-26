/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：虚拟摇杆                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年6月3日                             
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    /// 保持在指定半径范围内的虚拟操纵杆。当压力释放时，操纵手柄将回到起始位置
    /// </summary>
    public class VirtualJoystick : VirtualAxis, IDragHandler
    {
        private Transform m_CanvasScalarTransform;

        [Tooltip(
            "The joystick will return a zero value when the radius is within the specified deadzone radius of the center.")]
        [SerializeField]
        protected float m_DeadzoneRadius = 5;

        [Tooltip("A reference to the joystick that moves with the press position.")] [SerializeField]
        protected RectTransform m_Joystick;

        private Vector2 m_JoystickStartPosition;

        [Tooltip("The maximum number of pixels that the joystick can move.")] [SerializeField]
        protected float m_Radius = 100;

        /// <summary>
        /// Callback when a pointer has dragged the button.
        /// </summary>
        /// <param name="data">The pointer data.</param>
        public void OnDrag( PointerEventData data )
        {
            var canvasScale = m_CanvasScalarTransform == null ? Vector3.one : m_CanvasScalarTransform.localScale;
            m_DeltaPosition.x += data.delta.x / canvasScale.x;
            m_DeltaPosition.y += data.delta.y / canvasScale.y;
            m_DeltaPosition.x = Mathf.Clamp(m_DeltaPosition.x, -m_Radius, m_Radius);
            m_DeltaPosition.y = Mathf.Clamp(m_DeltaPosition.y, -m_Radius, m_Radius);
            if (m_DeltaPosition.magnitude > m_Radius) m_DeltaPosition = m_DeltaPosition.normalized * m_Radius;

            // Update the joystick position.
            m_Joystick.anchoredPosition = m_JoystickStartPosition + m_DeltaPosition;
        }

        protected override void Awake()
        {
            if (m_Joystick == null)
            {
                Debug.LogError("Error: A joystick transform must be specified.");
                enabled = false;
                return;
            }
            m_CanvasScalarTransform = GetComponentInParent<CanvasScaler>().transform;
            m_JoystickStartPosition = m_Joystick.anchoredPosition;

            base.Awake();
        }

        /// <summary>
        /// Callback when a finger has released the button.
        /// </summary>
        /// <param name="data">The pointer data.</param>
        public override void OnPointerUp( PointerEventData data )
        {
            if (!m_Pressed) return;

            base.OnPointerUp(data);

            m_Joystick.anchoredPosition = m_JoystickStartPosition;
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
            {
                if (Mathf.Abs(m_DeltaPosition.x) > m_DeadzoneRadius) return m_DeltaPosition.x / m_Radius;
            }
            else
            {
                if (Mathf.Abs(m_DeltaPosition.y) > m_DeadzoneRadius) return m_DeltaPosition.y / m_Radius;
            }
            return 0;
        }
    }
}