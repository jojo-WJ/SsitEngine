/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：可以按下的虚拟控件                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年6月3日                             
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;
using UnityEngine.EventSystems;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    /// 虚拟控件.
    /// </summary>
    public class VirtualButton : VirtualControl, IPointerDownHandler, IPointerUpHandler
    {
        [Tooltip("The name of the input.")] [SerializeField]
        protected string m_ButtonName;

        private int m_Frame;

        private bool m_Pressed;

        /// <summary>
        /// Callback when a pointer has pressed on the button.
        /// </summary>
        /// <param name="data">The pointer data.</param>
        public void OnPointerDown( PointerEventData data )
        {
            if (m_Pressed) return;

            m_Pressed = true;
            m_Frame = Time.frameCount;
        }

        /// <summary>
        /// Callback when a finger has released the button.
        /// </summary>
        /// <param name="data">The pointer data.</param>
        public void OnPointerUp( PointerEventData data )
        {
            if (!m_Pressed) return;

            m_Pressed = false;
            m_Frame = Time.frameCount;
        }

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        protected override void Awake()
        {
            if (string.IsNullOrEmpty(m_ButtonName))
            {
                Debug.LogError("Error: The virtual button " + gameObject.name + " cannot have an empty input name.");
                return;
            }

            base.Awake();

            if (m_VirtualControlsManager != null) m_VirtualControlsManager.RegisterVirtualControl(m_ButtonName, this);
        }

        /// <summary>
        /// Returns if the button is true with the specified ButtonAction.
        /// </summary>
        /// <param name="action">The type of action to check.</param>
        /// <returns>The status of the action.</returns>
        public override bool GetButton( InputBase.ButtonAction action )
        {
            if (action == InputBase.ButtonAction.GetButton) return m_Pressed;

            // GetButtonDown and GetButtonUp requires the button to be pressed or released within the same frame.
            if (Time.frameCount - m_Frame > 1) return false;

            return action == InputBase.ButtonAction.GetButtonDown ? m_Pressed : !m_Pressed;
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (m_VirtualControlsManager != null) m_VirtualControlsManager.UnregisterVirtualControl(m_ButtonName);
        }
    }
}