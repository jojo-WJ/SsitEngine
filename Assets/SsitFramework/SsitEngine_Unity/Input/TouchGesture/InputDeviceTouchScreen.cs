/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：触摸驱动器                                                   
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月16日09点38分                             
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using SsitEngine.DebugLog;
using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    ///     触摸驱动器
    /// </summary>
    public class InputDeviceTouchScreen : InputDeviceBase
    {
        /// <summary>
        ///     触摸驱动名称
        /// </summary>
        public const string DEVICETOUCHSCREEN = "DEVICETOUCHSCREEN";

        /// <summary>
        ///     触摸缩放灵敏度
        /// </summary>
        private const float ZOOM_SENSITIVITY = 1500f;

        private int m_lastCursorActivationFrame = -100;
        private readonly List<TouchGestureBase> m_touchGestures = new List<TouchGestureBase>();
        public EventHandler<TouchGestureEventArgs> OnGestureDetected;

        /// <summary>
        ///     创建触摸驱动器
        /// </summary>
        /// <param name="inputHandlerHelper"></param>
        public InputDeviceTouchScreen( IInputHandlerHelper inputHandlerHelper ) : base(inputHandlerHelper)
        {
            SsitDebug.Debug("InputDeviceTouchScreen IsDeviceSupport" + IsDeviceSupport);
            if (IsDeviceSupport)
            {
                OnGestureDetected += OnTouchGestureDetected;
                InitDevice();
            }
        }

        /// <summary>
        ///     驱动名称
        /// </summary>
        public override string DeviceName => DEVICETOUCHSCREEN;

        /// <summary>
        ///     驱动支持检测
        /// </summary>
        public bool IsDeviceSupport => Input.touchSupported;

        /// <summary>
        ///     初始化驱动
        /// </summary>
        public virtual void InitDevice()
        {
            Input.multiTouchEnabled = true;
            EnableGesture(EnTouchGestureType.PRESS_1_FINGER);
            EnableGesture(EnTouchGestureType.PRESS_2_FINGER);
            EnableGesture(EnTouchGestureType.PRESS_3_FINGER);
            EnableGesture(EnTouchGestureType.ZOOM);
        }

        /// <summary>
        ///     驱动轮询
        /// </summary>
        public override void Update()
        {
            // no need to update if no one is listening
            if (OnGestureDetected != null)
                foreach (var gesture in m_touchGestures)
                {
                    var detectedGestureEvent = gesture.Update();
                    if (detectedGestureEvent != null) OnGestureDetected(gesture, detectedGestureEvent);
                }
            // reset cursor action state if no cursor activating gestures were detected
            // in this frame it might happen that gestures will be detected in this frame
            // later in this case the cursor action state will be overwritten again
            if (m_lastCursorActivationFrame != Time.frameCount) inputHandlerHelper.EnableCursor(false);
        }

        /// <summary>
        ///     驱动销毁
        /// </summary>
        public override void Destroy()
        {
            if (OnGestureDetected != null) OnGestureDetected -= OnTouchGestureDetected;

            for (var i = 0; i < m_touchGestures.Count; i++) m_touchGestures[i].Destory();
        }

        /// <summary>
        ///     驱动监听检测
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private void OnTouchGestureDetected( object obj, TouchGestureEventArgs args )
        {
            switch (args.Type)
            {
                // cursor activation
                case EnTouchGestureType.PRESS_1_FINGER:
                {
                    var touch = args.Position;
                    inputHandlerHelper.SetCursorPosition(new Vector3(touch.x, touch.y, 0f));
                    inputHandlerHelper.EnableCursor(true);
                    m_lastCursorActivationFrame = Time.frameCount;
                    break;
                }
                // camera direction
                case EnTouchGestureType.PRESS_2_FINGER:
                {
                    var touch = args.Position;
                    var delta = args.Delta;
                    inputHandlerHelper.RotateCameraAroundPivot(touch + delta, touch);
                    //inputHandlerHelper.RotateCamera(touch, touch + delta);
                    break;
                }
                // camera movement
                case EnTouchGestureType.PRESS_3_FINGER:
                {
                    var touch = args.Position;
                    var delta = args.Delta;
                    inputHandlerHelper.MoveCamera(touch, touch - delta);
                    break;
                }
                // zoom
                case EnTouchGestureType.ZOOM:
                {
                    var zoomValue = args.Delta.x;
                    inputHandlerHelper.MoveCamera(Vector3.zero, Vector3.forward * zoomValue * ZOOM_SENSITIVITY);
                    break;
                }
            }
        }

        /// <summary>
        ///     激活触摸手势
        /// </summary>
        /// <param name="type"></param>
        private void EnableGesture( EnTouchGestureType type )
        {
            foreach (var gesture in m_touchGestures)
                if (gesture.Type == type)
                    return;
            switch (type)
            {
                case EnTouchGestureType.TAP:
                    m_touchGestures.Add(new TouchGestureTap());
                    break;
                case EnTouchGestureType.PRESS_1_FINGER:
                    m_touchGestures.Add(new TouchGesturePress(EnTouchGestureType.PRESS_1_FINGER, 1, 0.1f));
                    break;
                case EnTouchGestureType.PRESS_2_FINGER:
                    m_touchGestures.Add(new TouchGesturePress(EnTouchGestureType.PRESS_2_FINGER, 2, 0f));
                    break;
                case EnTouchGestureType.PRESS_3_FINGER:
                    m_touchGestures.Add(new TouchGesturePress(EnTouchGestureType.PRESS_3_FINGER, 3, 0f));
                    break;
                case EnTouchGestureType.PRESS_4_FINGER:
                    m_touchGestures.Add(new TouchGesturePress(EnTouchGestureType.PRESS_4_FINGER, 4, 0f));
                    break;
                case EnTouchGestureType.ZOOM:
                    m_touchGestures.Add(new TouchGestureZoom());
                    break;
                default:
                    Debug.LogError("TG_TouchGestures: EnableGesture: unknown gesture type '" + type + "'!");
                    break;
            }
        }
    }
}