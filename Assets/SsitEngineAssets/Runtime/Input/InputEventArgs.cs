/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/4 15:22:22                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework.SceneObject;
using Framework.SSITInput.EventHandler;
using SsitEngine.Core.EventPool;
using SsitEngine.Unity.SsitInput;
using UnityEngine;

namespace Framework.SsitInput
{
    /// <summary>
    /// 物体拖拽检测状态
    /// </summary>
    public enum EDraggedObjectState
    {
        NONE, /*2DUI*/
        IN_3D_PREVIEW, /*3D显示*/
        NOT_PLACEABLE /*放置失败*/
    }

    /// <summary>
    /// UI拖拽状态
    /// </summary>
    public enum UDraggedObjectState
    {
        NONE,
        BEGINDRAG, /*拖拽开始*/
        ONDRAG, /*拖拽中*/
        ENDRAG, /*拖拽结束*/
        Cancel /*取消*/
    }

    /// <summary>
    /// 场景内操作物体事件类型
    /// </summary>
    public enum EnSceneState
    {
        NEWOBJECT, /*新建物体*/
        EDITOBJECT, /*编辑物体坐标信息*/
        NEWLINE, /*新建画线*/
        EDITLINE, /*编辑线段*/
        NORMAL, /*正常控制器操作状态*/
        SELECEDOBJECT, /*选中物体状态*/
        FOLLOW /*监控状态*/
    }

    public class InputEventArgs : BaseEventArgs, IEventData
    {
        public object data;
        public Vector3 groud;
        public bool isGroud;
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