/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/29 17:03:19                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.SceneObject
{
    public enum EnObjectEvent
    {
        SyncSceneObjInfoRequest = EnMsgCenter.EntityEvent,
        SyncSceneObjInfoResult,
        SyncPlayerInfoResult,
        SyncAssignClientAuthorityResult,
        SyncSyncTransResult,

        SpawnSceneObjectResult,
        DestorySceneObjectResult,
        SpawnPlayer,
        RecordAccidentInfo, //记录险情汇报信息


        MaxValue
    }

    public enum EnPropertyId
    {
        //属性
        Selected = EnObjectEvent.MaxValue, //(ture,false)
        Active,
        Init,

        Color, // maincolor
        AddtionColor, // maskColor
        Position, // Position
        Rotate, // rotate
        Scale, // scale
        Wind, // wind

        Show3DTag, // show tag
        Show2DTag,

        //权限归属
        Authority,

        //Switch
        SwitchState,
        SwitchIK,

        Switch,
        OnSwitch,

        SwitchControl, //摇杆
        SwitchAnim, //瞄准

        //hit
        Consume,
        OnConsume,

        Hit,
        OnHit,

        //character state 
        State,
        OnState,
        Input,
        Interaction,


        //xfp
        Presure, //压力
        Range, //开花范围
        Swing, //自摆范围

        //OnSwitchControl,
        //mhq
        AnimNormal = Presure + 10, //动画权重

        //todo：一定要往后加，不要闲麻烦，以上冗余也别轻易删除，容易引起数据混乱
        Follow = AnimNormal + 10, //跟随
        Trigger = Follow + 10, //触发
        MaxValue
    }
}