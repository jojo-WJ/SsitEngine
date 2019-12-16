using System;
using System.Collections.Generic;
using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    ///     按键驱动器
    /// </summary>
    public class InputDeviceKeyboard : InputDeviceBase
    {
        /// <summary>
        ///     触摸驱动名称
        /// </summary>
        public const string DEVICEKEYBOARD = "DEVICEKEYBOARD";

        /// <summary>
        ///     移动速度
        /// </summary>
        private const float MOVE_SPEED = 6f;

        /// <summary>
        ///     shift移动增量值
        /// </summary>
        private const float MOVE_SPEED_SHIFT_FACTOR = 5f;

        /// <summary>
        ///     按键监听列表
        /// </summary>
        private List<KeyCode> m_keyCodeListenerList = new List<KeyCode>();

        public EventHandler<KeyCodeEventArgs> OnKeyCodeDetected;

        /// <summary>
        ///     创建按键驱动器
        /// </summary>
        /// <param name="inputHandlerHelper"></param>
        public InputDeviceKeyboard( IInputHandlerHelper inputHandlerHelper ) : base(inputHandlerHelper)
        {
            if (IsDeviceSupport)
                InitKeyCodeListener();
        }

        /// <summary>
        ///     驱动名称
        /// </summary>
        public override string DeviceName => DEVICEKEYBOARD;

        /// <summary>
        ///     驱动支持检测
        /// </summary>
        public bool IsDeviceSupport => true;

        /// <summary>
        ///     初始化按键监听列表
        /// </summary>
        protected virtual void InitKeyCodeListener()
        {
            m_keyCodeListenerList.Add(KeyCode.W);
            m_keyCodeListenerList.Add(KeyCode.A);
            m_keyCodeListenerList.Add(KeyCode.S);
            m_keyCodeListenerList.Add(KeyCode.D);
            m_keyCodeListenerList.Add(KeyCode.Q);
            m_keyCodeListenerList.Add(KeyCode.E);
            m_keyCodeListenerList.Add(KeyCode.F);
        }

        /// <summary>
        ///     驱动轮询
        /// </summary>
        public override void Update()
        {
            if (m_keyCodeListenerList.Count == 0) return;

            var key = KeyCode.None;

            var moveDirection = Vector3.zero;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveDirection += Vector3.left;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveDirection += Vector3.right;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveDirection += Vector3.back;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveDirection += Vector3.forward;
            if (Input.GetKey(KeyCode.Q))
            {
                moveDirection += Vector3.down;
                key = KeyCode.Q;
            }
            if (Input.GetKey(KeyCode.E))
            {
                moveDirection += Vector3.up;
                key = KeyCode.E;
            }
            // 矫正平台配置限定
            var config = Engine.Instance.Platform.PlatformConfig;
            moveDirection *= config.movSpeed;
            if (!Mathf.Approximately(moveDirection.sqrMagnitude, 0))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    moveDirection *= config.movAttachSpeed;

                inputHandlerHelper.MoveCamera(Vector3.zero, moveDirection);
                if (OnKeyCodeDetected != null)
                    OnKeyCodeDetected(this, new KeyCodeEventArgs(key, Vector2.zero, moveDirection));
            }
        }


        /*/// <summary>
        /// 按键监听检测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnKeyCodeListenerDetected( object sender, KeyCodeEventArgs e )
        {

        }*/

        /// <summary>
        ///     驱动关闭
        /// </summary>
        public override void Destroy()
        {
            m_keyCodeListenerList.Clear();
            OnKeyCodeDetected = null;
            m_keyCodeListenerList = null;
        }
    }
}