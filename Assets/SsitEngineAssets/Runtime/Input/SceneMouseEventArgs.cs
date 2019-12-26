/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/4 15:22:22                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework.SceneObject;
using SsitEngine.Core.EventPool;
using SsitEngine.Unity.SsitInput;
using UnityEngine;

namespace Framework.SsitInput
{
    public class SceneMouseEventArgs : BaseEventArgs
    {
        public object data;
        public Vector3 normal;

        public Vector3 point;

        public object sender;
        public BaseSceneInstance target;
        public EnMouseEventType type;

        public override ushort Id => (ushort) type;

        public override void Clear()
        {
            sender = null;
            target = null;
            data = null;
        }
    }
}