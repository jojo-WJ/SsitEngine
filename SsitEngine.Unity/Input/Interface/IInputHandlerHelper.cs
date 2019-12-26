/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：操作器接口                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月15日                             
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    ///     操作器接口
    /// </summary>
    public interface IInputHandlerHelper
    {
        /// <summary>
        ///     是否可交互
        /// </summary>
        bool IsInteractable { get; }

        /// <summary>
        ///     是否聚焦
        /// </summary>
        bool HasFoucs { get; set; }

        /// <summary>
        ///     是否检测到对象
        /// </summary>
        bool IsCursonSomething { get; set; }

        /// <summary>
        ///     碰撞检测信息
        /// </summary>
        RaycastHit HitInfo { get; }

        /// <summary>
        ///     初始化辅助器
        /// </summary>
        void InitHelper();

        /// <summary>
        ///     初始化操作驱动
        /// </summary>
        /// <returns></returns>
        InputDeviceBase[] InitInputDevice( IInputManager camera );

        /// <summary>
        ///     设置鼠标位置
        /// </summary>
        /// <param name="cursorScreenCoords">鼠标位置</param>
        void SetCursorPosition( Vector3 cursorScreenCoords );

        /// <summary>
        ///     禁用激活鼠标
        /// </summary>
        /// <param name="enableCursor"></param>
        void EnableCursor( bool enableCursor );

        /// <summary>
        ///     移动摄像机
        /// </summary>
        /// <param name="fromScreenCoords">起始位置</param>
        /// <param name="toScreenCoords">目标位置</param>
        /// <param name="convertDir">本地坐标调正</param>
        /// <param name="ignoreUI">不理睬UI</param>
        void MoveCamera( Vector3 fromScreenCoords, Vector3 toScreenCoords, bool convertDir = false,
            bool ignoreUI = true );

        /// <summary>
        ///     旋转摄像机围绕轴
        /// </summary>
        /// <param name="fromScreenCoords">起始位置</param>
        /// <param name="toScreenCoords">目标位置</param>
        /// <param name="ignoreUI">不理睬UI</param>
        void RotateCamera( Vector3 fromScreenCoords, Vector3 toScreenCoords, bool ignoreUI = true );

        /// <summary>
        ///     旋转摄像机围轴点
        /// </summary>
        /// <param name="fromScreenCoords">起始位置</param>
        /// <param name="toScreenCoords">目标位置</param>
        /// <param name="ignoreUI">不理睬UI</param>
        void RotateCameraAroundPivot( Vector3 fromScreenCoords, Vector3 toScreenCoords, bool ignoreUI = true );

        /// <summary>
        ///     辅助器更新
        /// </summary>
        void Update();

        /// <summary>
        ///     鼠标事件回调
        /// </summary>
        /// <param name="mouseEventType"></param>
        void ProcessMouseEvent( EnMouseEventType mouseEventType );

        /// <summary>
        ///     销毁辅助器
        /// </summary>
        void Shutdown();
    }
}