/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/16 15:31:11                     
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;

namespace SsitEngine.Unity.SsitInput
{
    /// <summary>
    ///     操作器接口
    /// </summary>
    public interface IInputManager
    {
        /// <summary>
        ///     操作摄像机
        /// </summary>
        Camera Cam { get; }
    }
}