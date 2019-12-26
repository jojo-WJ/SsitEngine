/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述： UI模块消息枚举                                                   
*│　作   者：Jusam                                          
*│　版   本：1.0.0                                                 
*│　创建时间：2019/04/29                             
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.UI
{
    /// <summary>
    ///     UI模块消息
    /// </summary>
    public enum UIMsg
    {
        /// <summary>
        ///     打开面板
        /// </summary>
        OpenForm = EnMsgCenter.UIEvent + 1,

        /// <summary>
        ///     关闭面板
        /// </summary>
        CloseForm,


        /// <summary>
        ///     max value
        /// </summary>
        MaxValue
    }
}