/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/5 9:56:40                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework.SceneObject;
using SsitEngine.Core;

namespace Framework.SsitInput
{
    /// <summary>
    /// 对象选择拖拽事件
    /// </summary>
    public class ObjectSelectDraggableEventArgs : SsitEventArgs
    {
        public readonly BaseSceneInstance ObjPrefab;
        public readonly string ResourcePath;

        public ObjectSelectDraggableEventArgs( BaseSceneInstance p_objPrefab, string p_resourcePath )
        {
            ObjPrefab = p_objPrefab;
            ResourcePath = p_resourcePath;
        }
    }
}