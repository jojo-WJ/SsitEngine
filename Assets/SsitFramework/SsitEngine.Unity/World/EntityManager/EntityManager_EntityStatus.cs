/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/5 15:18:19                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.Entity
{
    public partial class EntityManager
    {
        /// <summary>
        ///     实体状态。
        /// </summary>
        private enum EntityStatus
        {
            /// <summary>
            ///     准备初始化
            /// </summary>
            WillInit,

            /// <summary>
            ///     已经初始化
            /// </summary>
            Inited,

            /// <summary>
            ///     准备显示
            /// </summary>
            WillShow,

            /// <summary>
            ///     显示
            /// </summary>
            Showed,

            /// <summary>
            ///     准备隐藏
            /// </summary>
            WillHide,

            /// <summary>
            ///     隐藏
            /// </summary>
            Hidden,

            /// <summary>
            ///     准备复用
            /// </summary>
            WillRecycle,

            /// <summary>
            ///     复用
            /// </summary>
            Recycled
        }
    }
}