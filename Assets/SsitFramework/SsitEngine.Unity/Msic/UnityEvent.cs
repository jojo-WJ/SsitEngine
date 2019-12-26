/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/19 15:35:46                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SsitEngine.Unity.Unity.Msic
{
    /// <summary>
    ///     (float) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityFloatEvent : UnityEvent<float>
    {
    }

    /// <summary>
    ///     (bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityBoolEvent : UnityEvent<bool>
    {
    }

    /// <summary>
    ///     (Transform) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityTransformEvent : UnityEvent<Transform>
    {
    }

    /// <summary>
    ///     (MovementType, bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityMovementTypeBoolEvent : UnityEvent<ScrollRect.MovementType, bool>
    {
    }

    /// <summary>
    ///     (Vector3, Vector3, GameObject) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityVector3Vector3GameObjectEvent : UnityEvent<Vector3, Vector3, GameObject>
    {
    }

    /// <summary>
    ///     (float, Vector3, Vector3, GameObject) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [Serializable]
    public class UnityFloatVector3Vector3GameObjectEvent : UnityEvent<float, Vector3, Vector3, GameObject>
    {
    }

    /*
    /// <summary>
    /// (ViewType, bool) UnityEvent subclass so the event will appear in the inspector.
    /// </summary>
    [System.Serializable]
    public class UnityViewTypeBoolEvent : UnityEvent<ViewType, bool> { }
    */
}