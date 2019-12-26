/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：虚拟轴                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年6月3日                             
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;
using UnityEngine.EventSystems;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    /// 用于处理轴输入的抽象类
    /// </summary>
    public abstract class VirtualAxis : VirtualControl, IPointerDownHandler, IPointerUpHandler
    {
        protected Vector2 m_DeltaPosition;

        [Tooltip("The name of the horizontal input axis.")] [SerializeField]
        protected string m_HorizontalInputName = "Horizontal";

        protected bool m_Pressed;

        [Tooltip("The name of the vertical input axis.")] [SerializeField]
        protected string m_VerticalInputName = "Vertical";

        /// <summary>
        /// Callback when a pointer has pressed on the button.
        /// </summary>
        /// <param name="data">The pointer data.</param>
        public virtual void OnPointerDown( PointerEventData data )
        {
            if (m_Pressed) return;

            m_Pressed = true;
            m_DeltaPosition = Vector2.zero;
        }

        /// <summary>
        /// Callback when a pointer has released the button.
        /// </summary>
        /// <param name="data">The pointer data.</param>
        public virtual void OnPointerUp( PointerEventData data )
        {
            if (!m_Pressed) return;

            m_Pressed = false;
            m_DeltaPosition = Vector2.zero;
        }

        protected override void Awake()
        {
            base.Awake();

            if (m_VirtualControlsManager != null)
            {
                m_VirtualControlsManager.RegisterVirtualControl(m_HorizontalInputName, this);
                m_VirtualControlsManager.RegisterVirtualControl(m_VerticalInputName, this);
            }
        }
    }
}