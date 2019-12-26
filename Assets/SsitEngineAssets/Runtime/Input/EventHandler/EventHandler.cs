/*
*┌──────────────────────────────────────────────────────────────┐
*│  作    者：Shell Lee
*│  版    本：1.0.0
*│  创建时间：2019-07-23 11:29:25
*│  功能描述：
*│      本模块用于维护输入事件的处理
*│  注意事项:
*│  
*│  修改人员：
*│  修改时间：
*│  修改内容：
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using SsitEngine.PureMVC.Interfaces;
using SsitEngine.Unity.SsitInput;

namespace Framework.SSITInput.EventHandler
{
    public interface IEventData
    {
    }

    public interface IEventHandler
    {
        /// <summary>
        /// 种类
        /// </summary>
        EnEventHandlerType Type { get; }

        /// <summary>
        /// 是否启用
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// MVC消息处理
        /// </summary>
        /// <param name="notification"></param>
        void HandleNotification( INotification notification );

        /// <summary>
        /// 事件处理
        /// </summary>
        /// <param name="ed"></param>
        /// <returns></returns>
        bool Handle( IEventData ed );

        //IEventHandler Instance { set; get; }
    }

    /// <summary>
    /// EventHandler基类
    /// </summary>
    public class EventHandler : IEventHandler
    {
        #region static

        public static implicit operator bool( EventHandler self )
        {
            return null != self;
        }

        public static readonly List<EventHandler> AllHandlers = new List<EventHandler>();

        public static bool Enable( EnEventHandlerType type, bool enable )
        {
            var handlers = AllHandlers.FindAll(handler => handler?.Type == type);

            if (0 == handlers.Count)
            {
                return false;
            }

            foreach (var handler in handlers)
            {
                handler.Enabled = enable;
            }

            return true;
        }

        #endregion

        #region inherited

        public virtual EnEventHandlerType Type => EnEventHandlerType.Count;

        public virtual bool Enabled { get; set; }

        public virtual void HandleNotification( INotification notification )
        {
        }

        public virtual bool Handle( IEventData ed )
        {
            return true;
        }

        public virtual void OnInputEventCallBack( EnMouseEventType arg0 )
        {
        }

        #endregion
    }

    /// <summary>
    /// Handler类型
    /// </summary>
    public enum EnEventHandlerType
    {
        DrawArrowPath,
        SkillEditScene,

        Count
    }

}