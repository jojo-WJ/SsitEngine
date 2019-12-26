/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/3 15:18:57                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using SsitEngine.DebugLog;
using UnityEngine;

namespace Framework.SsitInput
{
    /// <summary>
    /// INPUT TYPE - check in real time if you are using a joystick, mobile or mouse/keyboard
    /// </summary>
    [HideInInspector]
    public enum InputDevice
    {
        MouseKeyboard,
        Joystick,
        Mobile
    }

    [Serializable]
    public class GenericInput
    {
        private float buttomTimer;
        private bool inButtomTimer;

        [SerializeField] private bool isAxisInUse;

        [SerializeField] private string joystick;

        [SerializeField] private bool joystickAxis;

        [SerializeField] private bool joystickAxisInvert;

        [SerializeField] private string keyboard;

        [SerializeField] private bool keyboardAxis;

        [SerializeField] private bool keyboardAxisInvert;

        [SerializeField] private string mobile;

        [SerializeField] private bool mobileAxis;

        [SerializeField] private bool mobileAxisInvert;

        private int multTapCounter;
        private float multTapTimer;
        public bool useInput = true;

        /// <summary>
        /// Initialise a new GenericInput
        /// </summary>
        /// <param name="keyboard"></param>
        /// <param name="joystick"></param>
        /// <param name="mobile"></param>
        public GenericInput( string keyboard, string joystick, string mobile )
        {
            this.keyboard = keyboard;
            this.joystick = joystick;
            this.mobile = mobile;
        }

        /// <summary>
        /// Initialise a new GenericInput
        /// </summary>
        /// <param name="keyboard"></param>
        /// <param name="joystick"></param>
        /// <param name="mobile"></param>
        public GenericInput( string keyboard, bool keyboardAxis, string joystick, bool joystickAxis, string mobile,
            bool mobileAxis )
        {
            this.keyboard = keyboard;
            this.keyboardAxis = keyboardAxis;
            this.joystick = joystick;
            this.joystickAxis = joystickAxis;
            this.mobile = mobile;
            this.mobileAxis = mobileAxis;
        }

        /// <summary>
        /// Initialise a new GenericInput
        /// </summary>
        /// <param name="keyboard"></param>
        /// <param name="joystick"></param>
        /// <param name="mobile"></param>
        public GenericInput( string keyboard, bool keyboardAxis, bool keyboardInvert, string joystick,
            bool joystickAxis, bool joystickInvert, string mobile, bool mobileAxis, bool mobileInvert )
        {
            this.keyboard = keyboard;
            this.keyboardAxis = keyboardAxis;
            keyboardAxisInvert = keyboardInvert;
            this.joystick = joystick;
            this.joystickAxis = joystickAxis;
            joystickAxisInvert = joystickInvert;
            this.mobile = mobile;
            this.mobileAxis = mobileAxis;
            mobileAxisInvert = mobileInvert;
        }

        protected InputDevice inputDevice => InputManager.Instance.InputDevice;

        public bool isAxis
        {
            get
            {
                var value = false;
                switch (inputDevice)
                {
                    case InputDevice.Joystick:
                        value = joystickAxis;
                        break;
                    case InputDevice.MouseKeyboard:
                        value = keyboardAxis;
                        break;
                    case InputDevice.Mobile:
                        value = mobileAxis;
                        break;
                }
                return value;
            }
        }

        public bool isAxisInvert
        {
            get
            {
                var value = false;
                switch (inputDevice)
                {
                    case InputDevice.Joystick:
                        value = joystickAxisInvert;
                        break;
                    case InputDevice.MouseKeyboard:
                        value = keyboardAxisInvert;
                        break;
                    case InputDevice.Mobile:
                        value = mobileAxisInvert;
                        break;
                }
                return value;
            }
        }

        /// <summary>
        /// Button Name
        /// </summary>
        public string buttonName
        {
            get
            {
                if (InputManager.Instance != null)
                {
                    if (InputManager.Instance.InputDevice == InputDevice.MouseKeyboard) return keyboard;
                    if (InputManager.Instance.InputDevice == InputDevice.Joystick) return joystick;
                    return mobile;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Check if button is a Key
        /// </summary>
        public bool isKey
        {
            get
            {
                if (InputManager.Instance != null)
                    if (Enum.IsDefined(typeof(KeyCode), buttonName))
                        return true;
                return false;
            }
        }

        /// <summary>
        /// Get <see cref="KeyCode"/> value
        /// </summary>
        public KeyCode key => (KeyCode) Enum.Parse(typeof(KeyCode), buttonName);

        /// <summary>
        /// Get Button
        /// </summary>
        /// <returns></returns>
        public bool GetButton()
        {
            return InputManager.Instance.PlatInput.GetButton(buttonName);
        }

        /// <summary>
        /// Get ButtonDown
        /// </summary>
        /// <returns></returns>
        public bool GetButtonDown()
        {
            return InputManager.Instance.PlatInput.GetButtonDown(buttonName);
        }

        /// <summary>
        /// Get Button Up
        /// </summary>
        /// <returns></returns>
        public bool GetButtonUp()
        {
            return InputManager.Instance.PlatInput.GetButtonUp(buttonName);
        }

        /// <summary>
        /// Get Axis
        /// </summary>
        /// <returns></returns>
        public float GetAxis()
        {
            return InputManager.Instance.PlatInput.GetAxis(buttonName);
        }

        /// <summary>
        /// Get Axis Raw
        /// </summary>
        /// <returns></returns>
        public float GetAxisRaw()
        {
            return InputManager.Instance.PlatInput.GetAxisRaw(buttonName);
        }

        /// <summary>
        /// Get Double Button Down Check if button is pressed Within the defined time
        /// </summary>
        /// <param name="inputTime"></param>
        /// <returns></returns>
        public bool GetDoubleButtonDown( float inputTime = 1 )
        {
            if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName)) return false;

            if (multTapCounter == 0 && GetButtonDown())
            {
                multTapTimer = Time.time;
                multTapCounter = 1;
                return false;
            }

            if (multTapCounter == 1 && GetButtonDown())
            {
                var time = multTapTimer + inputTime;
                var valid = Time.time < time;
                multTapTimer = 0;
                multTapCounter = 0;
                return valid;
            }
            return false;
        }

        /// <summary>
        /// Get Buttom Timer Check if button is pressed for defined time
        /// </summary>
        /// <param name="inputTime"> time to check button press</param>
        /// <returns></returns>
        public bool GetButtonTimer( float inputTime = 2 )
        {
            if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName)) return false;
            if (GetButtonDown() && !inButtomTimer)
            {
                buttomTimer = Time.time;
                inButtomTimer = true;
            }
            if (inButtomTimer)
            {
                var time = buttomTimer + inputTime;
                var valid = time - Time.time <= 0;
                if (GetButtonUp())
                {
                    inButtomTimer = false;
                    return valid;
                }
                if (valid) inButtomTimer = false;
                return valid;
            }
            return false;
        }

        /// <summary>
        /// Get Axis like a button        
        /// </summary>
        /// <param name="value">Value to check need to be diferent 0</param>
        /// <returns></returns>
        public bool GetAxisButton( float value = 0.5f )
        {
            if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName)) return false;
            if (isAxisInvert) value *= -1f;
            if (value > 0)
                return GetAxisRaw() >= value;
            if (value < 0) return GetAxisRaw() <= value;
            return false;
        }

        /// <summary>
        /// Get Axis like a buttonDown        
        /// </summary>
        /// <param name="value">Value to check need to be diferent 0</param>
        /// <returns></returns>
        public bool GetAxisButtonDown( float value = 0.5f )
        {
            if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName)) return false;
            if (isAxisInvert) value *= -1f;
            if (value > 0)
            {
                if (!isAxisInUse && GetAxisRaw() >= value)
                {
                    isAxisInUse = true;
                    return true;
                }
                if (isAxisInUse && Mathf.Approximately(GetAxisRaw(), 0)) isAxisInUse = false;
            }
            else if (value < 0)
            {
                if (!isAxisInUse && GetAxisRaw() <= value)
                {
                    isAxisInUse = true;
                    return true;
                }
                if (isAxisInUse && Mathf.Approximately(GetAxisRaw(), 0)) isAxisInUse = false;
            }
            return false;
        }

        /// <summary>
        /// Get Axis like a buttonUp
        /// Check if Axis is zero after press       
        /// <returns></returns>
        public bool GetAxisButtonUp()
        {
            if (string.IsNullOrEmpty(buttonName) || !IsButtonAvailable(buttonName)) return false;
            if (isAxisInUse && GetAxisRaw() == 0)
            {
                isAxisInUse = false;
                return true;
            }
            if (!isAxisInUse && GetAxisRaw() != 0)
            {
                isAxisInUse = true;
            }
            return false;
        }

        private bool IsButtonAvailable( string btnName )
        {
            if (!useInput) return false;
            try
            {
                if (isKey) return true;
                //Input.GetButton(buttonName);
                return true;
            }
            catch (Exception exc)
            {
                SsitDebug.Warning(" Failure to try access button :" + buttonName + "\n" + exc.Message);
                return false;
            }
        }
    }
}