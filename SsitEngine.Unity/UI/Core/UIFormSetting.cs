/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：UIForm设置                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 19:29:31                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using UnityEngine;

namespace SsitEngine.Unity.UI
{
    /// <summary>Settings for UI form.</summary>
    [CreateAssetMenu(menuName = "SsitEngine.Unity/UIFormSettings", order = 999)]
    public class UIFormSettings : ScriptableObject
    {
        /// <summary>Layers for custom UI form.</summary>
        [Tooltip("Layers for custom UI form.")]
        public List<string> layers = new List<string>
        {
            "Normal",
            "Fixed",
            "PopUp"
        };
    }
}