/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/5 17:32:39                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.Entity
{
    /// <summary>
    ///     实体事件
    /// </summary>
    public enum EnEntityEvent
    {
        /// <summary>
        ///     显示实体
        /// </summary>
        ShowEntity = EnMsgCenter.EntityEvent + 1,

        /// <summary>
        ///     隐藏实体
        /// </summary>
        HideEntity,

        Selected,
        UnSelected
    }
}