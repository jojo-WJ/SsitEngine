/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/3 11:26:41                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    /// The base class for both mobile and standalone (keyboard/mouse and controller) input. This base class exists so UnityInput doesn't need to know if it
    /// is working with mobile controls or standalone controls.
    /// </summary>
    public abstract class InputBase
    {
        /// <summary>
        /// The type of button action to check against.
        /// </summary>
        public enum ButtonAction
        {
            GetButton,
            GetButtonDown,
            GetButtonUp
        }

        protected SsitInput ssitInput;

        /// <summary>
        /// Initializes the UnityInputBase.
        /// </summary>
        /// <param name="unityInput">A reference to the PlayerInput component.</param>
        public void Initialize( SsitInput ssitInput )
        {
            this.ssitInput = ssitInput;
        }

        /// <summary>
        /// Returns if the button is true with the specified ButtonAction.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <param name="action">The type of action to check.</param>
        /// <returns>The status of the action.</returns>
        public abstract bool GetButton( string name, ButtonAction action );

        /// <summary>
        /// Returns the axis of the specified button.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The axis value.</returns>
        public abstract float GetAxis( string name );

        /// <summary>
        /// Returns the raw axis of the specified button.
        /// </summary>
        /// <param name="axisName">The name of the axis.</param>
        /// <returns>The raw axis value.</returns>
        public abstract float GetAxisRaw( string axisName );
    }
}