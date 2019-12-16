/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/29 15:12:02                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using UnityEngine;

namespace SsitEngine.Unity
{
    /// <summary>
    /// 平台的配置文件
    /// </summary>
    [CreateAssetMenu(menuName = "SsitEngine_Unity/InternalAssetConfig", order = 999)]
    public class InternalAssetConfig : ScriptableObject
    {
        public List<string> mStreamingAsset;

        public InternalAssetConfig()
        {
            mStreamingAsset = new List<string>();
        }
    }
}