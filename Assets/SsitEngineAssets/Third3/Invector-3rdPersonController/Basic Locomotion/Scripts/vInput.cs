using System.Collections;
using Framework.SsitInput;
using UnityEngine;
using UnityEngine.EventSystems;
using XInputDotNetPure;

namespace Invector
{
    public class vInput : MonoBehaviour
    {
        public delegate void OnChangeInputType( InputDevice type );

        private static vInput _instance;

        private InputDevice _inputType = InputDevice.MouseKeyboard;

        public vHUDController hud;

        public static vInput instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<vInput>();
                    if (_instance == null)
                    {
                        new GameObject("vInputType", typeof(vInput));
                        return instance;
                    }
                }
                return _instance;
            }
        }

        [HideInInspector]
        public InputDevice inputDevice
        {
            get => _inputType;
            set
            {
                _inputType = value;
                OnChangeInput();
            }
        }

        public event OnChangeInputType onChangeInputType;

        private void Start()
        {
            if (hud == null) hud = vHUDController.instance;
        }

        public void GamepadVibration( float vibTime )
        {
            if (inputDevice == InputDevice.Joystick) StartCoroutine(GamepadVibrationRotine(vibTime));
        }

        private IEnumerator GamepadVibrationRotine( float vibTime )
        {
            if (inputDevice == InputDevice.Joystick)
            {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

                GamePad.SetVibration(0, 1, 1);
                yield return new WaitForSeconds(vibTime);
                GamePad.SetVibration(0, 0, 0);

#else
	            yield return new WaitForSeconds(0f);
#endif
            }
        }

        private void OnGUI()
        {
            switch (inputDevice)
            {
                case InputDevice.MouseKeyboard:
                    if (isJoystickInput())
                    {
                        inputDevice = InputDevice.Joystick;

                        if (hud != null)
                        {
                            hud.controllerInput = true;
                            hud.FadeText("Control scheme changed to Controller", 2f, 0.5f);
                        }
                    }
                    else if (isMobileInput())
                    {
                        inputDevice = InputDevice.Mobile;
                        if (hud != null)
                        {
                            hud.controllerInput = true;
                            hud.FadeText("Control scheme changed to Mobile", 2f, 0.5f);
                        }
                    }
                    break;
                case InputDevice.Joystick:
                    if (isMouseKeyboard())
                    {
                        inputDevice = InputDevice.MouseKeyboard;
                        if (hud != null)
                        {
                            hud.controllerInput = false;
                            hud.FadeText("Control scheme changed to Keyboard/Mouse", 2f, 0.5f);
                        }
                    }
                    else if (isMobileInput())
                    {
                        inputDevice = InputDevice.Mobile;
                        if (hud != null)
                        {
                            hud.controllerInput = true;
                            hud.FadeText("Control scheme changed to Mobile", 2f, 0.5f);
                        }
                    }
                    break;
                case InputDevice.Mobile:
                    if (isMouseKeyboard())
                    {
                        inputDevice = InputDevice.MouseKeyboard;
                        if (hud != null)
                        {
                            hud.controllerInput = false;
                            hud.FadeText("Control scheme changed to Keyboard/Mouse", 2f, 0.5f);
                        }
                    }
                    else if (isJoystickInput())
                    {
                        inputDevice = InputDevice.Joystick;
                        if (hud != null)
                        {
                            hud.controllerInput = true;
                            hud.FadeText("Control scheme changed to Controller", 2f, 0.5f);
                        }
                    }
                    break;
            }
        }

        private bool isMobileInput()
        {
#if UNITY_EDITOR && UNITY_MOBILE
            if (EventSystem.current.IsPointerOverGameObject() && Input.GetMouseButtonDown(0))
            {
                return true;
            }

#elif MOBILE_INPUT
            if (EventSystem.current.IsPointerOverGameObject() || Input.touches.Length > 0)
                return true;
#endif
            return false;
        }

        private bool isMouseKeyboard()
        {
#if MOBILE_INPUT
            return false;
#else
            //mouse & keyboard buttons
            if (Event.current.isKey || Event.current.isMouse)
                return true;
            //mouse movement
            if (Input.GetAxis("Mouse X") != 0.0f || Input.GetAxis("Mouse Y") != 0.0f)
                return true;

            return false;
#endif
        }

        private bool isJoystickInput()
        {
            //joystick buttons
            if (Input.GetKey(KeyCode.Joystick1Button0) ||
                Input.GetKey(KeyCode.Joystick1Button1) ||
                Input.GetKey(KeyCode.Joystick1Button2) ||
                Input.GetKey(KeyCode.Joystick1Button3) ||
                Input.GetKey(KeyCode.Joystick1Button4) ||
                Input.GetKey(KeyCode.Joystick1Button5) ||
                Input.GetKey(KeyCode.Joystick1Button6) ||
                Input.GetKey(KeyCode.Joystick1Button7) ||
                Input.GetKey(KeyCode.Joystick1Button8) ||
                Input.GetKey(KeyCode.Joystick1Button9) ||
                Input.GetKey(KeyCode.Joystick1Button10) ||
                Input.GetKey(KeyCode.Joystick1Button11) ||
                Input.GetKey(KeyCode.Joystick1Button12) ||
                Input.GetKey(KeyCode.Joystick1Button13) ||
                Input.GetKey(KeyCode.Joystick1Button14) ||
                Input.GetKey(KeyCode.Joystick1Button15) ||
                Input.GetKey(KeyCode.Joystick1Button16) ||
                Input.GetKey(KeyCode.Joystick1Button17) ||
                Input.GetKey(KeyCode.Joystick1Button18) ||
                Input.GetKey(KeyCode.Joystick1Button19))
                return true;

            //joystick axis
            if (Input.GetAxis("LeftAnalogHorizontal") != 0.0f ||
                Input.GetAxis("LeftAnalogVertical") != 0.0f ||
                Input.GetAxis("RightAnalogHorizontal") != 0.0f ||
                Input.GetAxis("RightAnalogVertical") != 0.0f ||
                Input.GetAxis("LT") != 0.0f ||
                Input.GetAxis("RT") != 0.0f ||
                Input.GetAxis("D-Pad Horizontal") != 0.0f ||
                Input.GetAxis("D-Pad Vertical") != 0.0f)
                return true;
            return false;
        }

        private void OnChangeInput()
        {
            if (onChangeInputType != null) onChangeInputType(inputDevice);
        }
    }
}