/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/26 20:26:28                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity
{
    /// <summary>
    ///     系统消息重心
    /// </summary>
    public enum EnMsgCenter
    {
        SystemEvent = SsitFrameUtils.MsgSpan * 1,
        GlobalEvent = SsitFrameUtils.MsgSpan * 2,

        /// <summary>
        ///     引擎事件
        /// </summary>
        EngineEvent = SsitFrameUtils.MsgSpan * 3,

        /// <summary>
        ///     Socket网络事件
        /// </summary>
        SocketEvent = SsitFrameUtils.MsgSpan * 4,

        /// <summary>
        ///     hud关联事件
        /// </summary>
        HudEvent = SsitFrameUtils.MsgSpan * 5,

        /// <summary>
        ///     任务系统事件
        /// </summary>
        QuestEvent = SsitFrameUtils.MsgSpan * 11,

        /// <summary>
        ///     计时器系统事件
        /// </summary>
        TimerEvent = SsitFrameUtils.MsgSpan * 12,
        NetEvent = SsitFrameUtils.MsgSpan * 13,
        UIEvent = SsitFrameUtils.MsgSpan * 14,
        NPCEvent = SsitFrameUtils.MsgSpan * 15,
        EntityEvent = SsitFrameUtils.MsgSpan * 16,
        AssetEvent = SsitFrameUtils.MsgSpan * 17,
        DataEvent = SsitFrameUtils.MsgSpan * 18,
        AudioEvent = SsitFrameUtils.MsgSpan * 19,
        DBEvent = SsitFrameUtils.MsgSpan * 20,

        /// <summary>
        ///     输入事件
        /// </summary>
        InputEvent = SsitFrameUtils.MsgSpan * 21
    }
}