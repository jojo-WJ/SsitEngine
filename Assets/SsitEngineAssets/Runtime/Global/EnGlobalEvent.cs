/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/4 10:39:12                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Unity;

public enum EnGlobalEvent
{
    // System
    Start = EnMsgCenter.GlobalEvent + 1,        //启动(这里的启动指进入启动流程)
    LoginIn,                                    //登陆
    LoginOut,                                   //注销
    Exit,                                       //退出(这里的退出只退出流程退回系统主流程)

    // lobby
    InitRoomInfo,
    UpdateRoomState,
    ApplyRoomInfo,
    UpdateRoomInfo,                             //服务器执行
    OnUpdateRoomInfo,                           //客户端执行

    // emergency
    InitGlobalProxy,                            //初始化全局数据代理
    RemoveGlobalProxy,                          //移除全局数据代理
    SceneLoadProcess,
    ChangeEnvInfo,                              //天气改变(Model层)--server
    ReceiveChatMsg,                             //接收到聊天信息
    OnChangeEnvInfo,                            //天气改变(View层【UI、GameObject】)
    FollowClient,                               //监控参演成员视角
    OnFollowClient,
    // input
    ChangeInputMode,                            //操作模式
    OnChangeInputMode,


    ChangePlayer,
    //...
    MaxValue,

}

public enum EnEzReplayEvent
{
    //Init
    Init = EnGlobalEvent.MaxValue,
    Mark,

    Record,

    Save,
    Upload,
    Read,

    //...
    MaxValue,
}

public enum EnDarwArrowEvent
{
    //Init
    Init = EnEzReplayEvent.MaxValue,

    StartDrawArrowPath,
    AddArrowPathPoint,
    EndDrawArrowPath,
    SyncAddDrawnArrow,
    OnAddDrawnArrow,

    //...
    MaxValue,
}

public enum EnAnimationEvent
{
    //Init
    None = EnDarwArrowEvent.MaxValue,
    HideAccident,
    OnHideAccident,

    //AddArrowPathPoint,
    //EndDrawArrowPath,
    //SyncAddDrawnArrow,
    //OnAddDrawnArrow,

    //...
    MaxValue,
}

/// <summary>
/// 单人系统
/// </summary>
public enum EnSingleEvent
{
    Init = EnAnimationEvent.MaxValue,

    SyncRecordResult,

    //...
    MaxValue,
}
