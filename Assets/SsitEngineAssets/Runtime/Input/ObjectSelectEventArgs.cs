/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/5 10:31:17                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using Framework.SceneObject;
using UnityEngine.Events;

namespace Framework.SsitInput
{
    [Serializable]
    public class OnInputEvent : UnityEvent<InputEventArgs>
    {
    }

    public class ObjectSelectedEvent : EventArgs
    {
        public ObjectSelectedEvent( BaseSceneInstance pSelectedInstance, BaseSceneInstance pPriorSelectedInstance )
        {
            SelectedInstance = pSelectedInstance;
            PriorSelectedInstance = pPriorSelectedInstance;
        }

        public BaseSceneInstance SelectedInstance { get; }

        public BaseSceneInstance PriorSelectedInstance { get; }
    }
}