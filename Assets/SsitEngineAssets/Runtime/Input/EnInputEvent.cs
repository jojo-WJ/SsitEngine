/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/5 14:26:20                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework.SceneObject;
using SsitEngine.Unity;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.SsitInput
{
    public enum EnInputEvent
    {
        SetDraggedObject = EnMsgCenter.InputEvent + 1,

        /// <summary>
        /// 拖拽预览状态改变
        /// </summary>
        EDraggedObjectState,

        /// <summary>
        /// 拖拽状态改变
        /// </summary>
        UDraggedObjectstate,

        OnEditorMove,
        OnForceEditorMove,

        MoveCamera,
        RotateCamera,
        ZoomCamera,

        //...
        SelectObject,
        FinishAddObject,
        FinishAddObjects,
        FinishDeleteObject,
        FinishDeleteObjects,
        TriggerObject,
        FinishChangeSceneState,

        UpdateEditPanelPosition,

        SelectDestoryObject,
        UpdateOperators,

        MaxValue,
    }

    public enum EnKeyEventType
    {
        escInput = EnInputEvent.MaxValue,
        moveInput,
        moveArrowInput,
        jumpInput,
        rollInput,
        strafeInput,
        sprintInput,
        crouchInput,

        resetInput,

        //...
        MaxValue
    }

    public enum EnMouseEvent
    {
        LeftDown = EnKeyEventType.MaxValue,
        LeftUp,
        LeftHover,

        RightDown,
        RightUp,
        RightHover,

        LeftDoubleClick,
        RightDoubleClick
    }


    public class EventOnClickSceneObject : UnityEvent<BaseSceneInstance>
    {
    }

    //public class EventOnRightClick : UnityEvent<InputEventArgs> { };
    public class EventOnDetectPosition : UnityEvent<Vector3>
    {
    }

    public class EventOnClickCollider : UnityEvent<Collider>
    {
    }

    public class EventOnClickScreen : UnityEvent<MouseClickEventVo>
    {
    }

    public class EventMouseOnClickPosition : UnityEvent<MouseClickEventVo>
    {
    }
}