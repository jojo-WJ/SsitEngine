/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：操作管理器                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月15日                             
*└──────────────────────────────────────────────────────────────┘
*/

using System.Linq;
using SsitEngine.DebugLog;
using SsitEngine.PureMVC.Patterns;
using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    ///     操作管理器
    /// </summary>
    public class InputManager : ManagerBase<InputManager>, IInputManager
    {
        /// <summary>
        ///     摄像机
        /// </summary>
        public Camera Cam
        {
            get
            {
                if (m_cam == null)
                {
                    m_cam = Camera.main;
                }
                return m_cam;
            }
        }

        #region Init InputManager

        /// <inheritdoc />
        public override void OnSingletonInit()
        {
            // todo:initialize
        }

        #endregion

        #region Variable

        /// <summary>
        ///     驱动辅助器
        /// </summary>
        private IInputHandlerHelper m_inputHandlerHelper;

        /// <summary>
        ///     驱动列表
        /// </summary>
        private InputDeviceBase[] m_inputDeviceMaps;

        /// <summary>
        ///     操作摄像机
        /// </summary>
        private Camera m_cam;

        #endregion

        #region IModule

        /// <summary>
        ///     模块优先级
        /// </summary>
        public override int Priority => (int) EnModuleType.ENMODULEDEFAULT;

        /// <summary>
        ///     驱动轮询
        /// </summary>
        /// <param name="elapsed">流逝时间</param>
        public override void OnUpdate( float elapsed )
        {
            if (m_inputHandlerHelper == null)
            {
                return;
            }
            m_inputHandlerHelper.Update();

            if (m_inputDeviceMaps == null)
            {
                return;
            }

            for (var i = 0; i < m_inputDeviceMaps.Length; i++)
            {
                m_inputDeviceMaps[i].Update();
            }
        }

        /// <summary>
        ///     关闭驱动
        /// </summary>
        public override void Shutdown()
        {
            if (isShutdown || m_inputDeviceMaps == null)
            {
                return;
            }
            isShutdown = true;

            for (var i = 0; i < m_inputDeviceMaps.Length; i++)
            {
                m_inputDeviceMaps[i].Destroy();
            }

            m_inputDeviceMaps = null;
        }

        #endregion

        #region Public Members

        /// <summary>
        ///     设置驱动助手
        /// </summary>
        /// <param name="inputHandlerHelper"></param>
        public void SetInputHander( IInputHandlerHelper inputHandlerHelper )
        {
            m_inputHandlerHelper = inputHandlerHelper;
            m_inputDeviceMaps = inputHandlerHelper.InitInputDevice(this);
            m_inputHandlerHelper.InitHelper();
        }

        /// <summary>
        ///     获取驱动助手
        /// </summary>
        /// <returns></returns>
        public IInputHandlerHelper GetInputHander()
        {
            return m_inputHandlerHelper;
        }

        /// <summary>
        ///     获取驱动助手
        /// </summary>
        /// <typeparam name="T">辅助器类型</typeparam>
        /// <returns></returns>
        public T GetInputHander<T>() where T : class, IInputHandlerHelper
        {
            return m_inputHandlerHelper as T;
        }

        /// <summary>
        ///     驱动是否受理
        /// </summary>
        /// <param name="deviceName">驱动名称</param>
        /// <returns></returns>
        public bool IsDeviceSet( string deviceName )
        {
            if (m_inputDeviceMaps != null)
            {
                return m_inputDeviceMaps.First(x => x.DeviceName == deviceName) != null;
            }

            if (Engine.Debug)
            {
                SsitDebug.Debug("当前系统无此名称的驱动");
            }
            return false;
        }

        /// <summary>
        ///     激活或禁用相应名称的操作驱动器
        /// </summary>
        /// <param name="deviceName">驱动名称</param>
        /// <param name="enable">参数值</param>
        public void EnableDevice( string deviceName, bool enable )
        {
            var device = GetDeviceByName(deviceName);
            if (device != null)
            {
                device.Enable = enable;
            }
        }

        /// <summary>
        ///     获取驱动管理器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DeviceName"></param>
        /// <returns></returns>
        public T GetDeviceByName<T>( string DeviceName ) where T : InputDeviceBase
        {
            if (m_inputDeviceMaps != null)
            {
                return m_inputDeviceMaps.First(x => x.DeviceName == DeviceName) as T;
            }

            if (Engine.Debug)
            {
                SsitDebug.Debug("当前系统无此名称的驱动");
            }
            return null;
        }

        /// <summary>
        ///     获取驱动管理器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DeviceName"></param>
        /// <returns></returns>
        public InputDeviceBase GetDeviceByName( string DeviceName )
        {
            if (m_inputDeviceMaps != null)
            {
                return m_inputDeviceMaps.First(x => x.DeviceName == DeviceName);
            }

            if (Engine.Debug)
            {
                SsitDebug.Debug("当前系统无此名称的驱动");
            }
            return null;
        }

        #endregion

        #region EventCallback

        /// <summary>
        ///     注册驱动事件
        /// </summary>
        /// <param name="device"></param>
        /// <param name="msgList"></param>
        public void RegisterDeviceMsg( InputDeviceBase device, params ushort[] msgList )
        {
            for (var i = 0; i < msgList.Length; i++)
            {
                Facade.Instance.RegisterObservers(this, msgList[i], device.HandleNotification);
            }
        }

        /// <summary>
        ///     移除驱动事件
        /// </summary>
        /// <param name="device"></param>
        /// <param name="msgList"></param>
        public void UnRegisterDeviceMsg( InputDeviceBase device, params ushort[] msgList )
        {
            for (var i = 0; i < msgList.Length; i++)
            {
                Facade.Instance.RemoveObservers(this, msgList[i]);
            }
        }

        #endregion
    }
}