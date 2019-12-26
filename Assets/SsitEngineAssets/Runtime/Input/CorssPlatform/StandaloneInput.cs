/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/3 11:35:36                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using SsitEngine.DebugLog;
using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    /// Uses Unity's input system to detect input related actions.
    /// </summary>
    public class StandaloneInput : InputBase
    {
        /// <summary>
        /// Returns if the button is true with the specified ButtonAction.
        /// </summary>
        /// <param name="name">The name of the button.</param>
        /// <param name="action">The type of action to check.</param>
        /// <returns>The status of the action.</returns>
        public override bool GetButton( string name, ButtonAction action )
        {
            if (string.IsNullOrEmpty(name) || !IsButtonAvailable(name)) return false;
//#if UNITY_EDITOR
            try
            {
                switch (action)
                {
                    case ButtonAction.GetButton:
                        if (IsKey(name))
                            return Input.GetKey(GetKey(name));
                        else
                            return Input.GetButton(name);
                    case ButtonAction.GetButtonDown:
                        if (IsKey(name))
                            return Input.GetKeyDown(GetKey(name));
                        else
                            return Input.GetButtonDown(name);
                    case ButtonAction.GetButtonUp:
                        if (IsKey(name))
                            return Input.GetKeyUp(GetKey(name));
                        else
                            return Input.GetButtonUp(name);
                }
            }
            catch (Exception /*e*/)
            {
                Debug.LogError("Button \"" + name +
                               "\" is not setup. Please create a button mapping within the Unity Input Manager.");
            }
//#else
//            switch (action) {
//                case ButtonAction.GetButton:
//                    return UnityEngine.Input.GetButton(name);
//                case ButtonAction.GetButtonDown:
//                    return UnityEngine.Input.GetButtonDown(name);
//                case ButtonAction.GetButtonUp:
//                    return UnityEngine.Input.GetButtonUp(name);
//            }
//#endif
            return false;
        }

        /// <summary>
        /// Return the value of the axis with the specified name.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the axis.</returns>
        public override float GetAxis( string name )
        {
            if (string.IsNullOrEmpty(name) || !IsButtonAvailable(name) || IsKey(name)) return 0;

#if UNITY_EDITOR
            try
            {
                return Input.GetAxis(name);
            }
            catch (UnityException /*e*/)
            {
                Debug.LogError("Axis \"" + name +
                               "\" is not setup. Please create an axis mapping within the Unity Input Manager.");
            }
            return 0;
#else
            return UnityEngine.Input.GetAxis(name);
#endif
        }

        /// <summary>
        /// Return the value of theraw  axis with the specified name.
        /// </summary>
        /// <param name="name">The name of the axis.</param>
        /// <returns>The value of the raw axis.</returns>
        public override float GetAxisRaw( string name )
        {
            if (string.IsNullOrEmpty(name) || !IsButtonAvailable(name) || IsKey(name)) return 0;

#if UNITY_EDITOR
            try
            {
                return Input.GetAxisRaw(name);
            }
            catch (UnityException /*e*/)
            {
                Debug.LogError("Axis \"" + name +
                               "\" is not setup. Please create an axis mapping within the Unity Input Manager.");
            }
            return 0;
#else
            return UnityEngine.Input.GetAxisRaw(name);
#endif
        }


        /// <summary>
        /// Check if button is a Key
        /// </summary>
        public bool IsKey( string btnName )
        {
            if (Enum.IsDefined(typeof(KeyCode), btnName))
                return true;
            return false;
        }

        /// <summary>
        /// Get <see cref="KeyCode"/> value
        /// </summary>
        public KeyCode GetKey( string btnName )
        {
            return (KeyCode) Enum.Parse(typeof(KeyCode), btnName);
        }

        private bool IsButtonAvailable( string btnName )
        {
            try
            {
                if (IsKey(btnName)) return true;
                Input.GetButton(btnName);
                return true;
            }
            catch (Exception exc)
            {
                SsitDebug.Warning(" Failure to try access button :" + btnName + "\n" + exc.Message);
                return false;
            }
        }
    }
}