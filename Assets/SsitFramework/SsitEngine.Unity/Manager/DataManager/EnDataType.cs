/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/9 15:57:14                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.Data
{
    /// <summary>
    ///     数据类型
    /// </summary>
    public enum EnDataType
    {
        /// <summary>
        ///     道具
        /// </summary>
        DATA_ITEM = 1000,

        /// <summary>
        ///     玩家
        /// </summary>
        DATA_PLAYER = 1001,

        /// <summary>
        ///     Npc
        /// </summary>
        DATA_NPC = 1002,

        /// <summary>
        ///     应急措施
        /// </summary>
        DATA_SKILL = 1003,

        /// <summary>
        ///     场景
        /// </summary>
        DATA_Scene = 1004,

        /// <summary>
        ///     资源
        /// </summary>
        DATA_Res = 1005,

        /// <summary>
        ///     UI
        /// </summary>
        DATA_UI = 1006,

        /// <summary>
        ///     Texture
        /// </summary>
        DATA_TEXTURE = 1007,

        /// <summary>
        ///     文件
        /// </summary>
        DATA_TEXTFILE = 1008,

        /// <summary>
        ///     图集
        /// </summary>
        DATA_ATLAS = 1009,

        /// <summary>
        ///     文本表
        /// </summary>
        DATA_STRING = 1011,

        /// <summary>
        ///     引导
        /// </summary>
        DATA_GUIDE = 1014,

        /// <summary>
        ///     活动
        /// </summary>
        DATA_Activity = 1015,


        DATA_Company = 1016,

        /// <summary>
        ///     数据最大标识（前台可以在这个基础上++）
        /// </summary>
        DATA_MaxValue
    }
}