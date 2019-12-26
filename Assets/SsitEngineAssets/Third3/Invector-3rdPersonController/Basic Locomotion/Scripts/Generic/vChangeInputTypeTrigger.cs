using Framework.SsitInput;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.ItemManager
{
    public class vChangeInputTypeTrigger : MonoBehaviour
    {
        public UnityEvent OnChangeToJoystick;

        [Header("Events called when InputType changed")]
        public UnityEvent OnChangeToKeyboard;

        public UnityEvent OnChangeToMobile;

        private void Start()
        {
            vInput.instance.onChangeInputType -= OnChangeInput;
            vInput.instance.onChangeInputType += OnChangeInput;
            OnChangeInput(vInput.instance.inputDevice);
        }

        public void OnChangeInput( InputDevice type )
        {
            switch (type)
            {
                case InputDevice.MouseKeyboard:
                    OnChangeToKeyboard.Invoke();
                    break;
                case InputDevice.Mobile:
                    OnChangeToMobile.Invoke();
                    break;
                case InputDevice.Joystick:
                    OnChangeToJoystick.Invoke();
                    break;
            }
        }
    }
}