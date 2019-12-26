/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/5 18:12:58                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Core;
using UnityEngine;

namespace Framework.SsitInput
{
    /// <summary>
    /// 对象选择拖拽事件
    /// </summary>
    public class ObjectCreateEventArgs : SsitEventArgs
    {
        public readonly int Itemid;
        public readonly Vector3 Pos;
        public readonly ScenePrefabInfo ResourcePath;
        public readonly Quaternion Rot;

        public ObjectCreateEventArgs( int itemid, ScenePrefabInfo p_resourcePath, Vector3 pos, Quaternion rotate )
        {
            Itemid = itemid;
            ResourcePath = p_resourcePath;
            Pos = pos;
            Rot = rotate;
        }
    }
}