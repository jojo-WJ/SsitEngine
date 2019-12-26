﻿/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/8/6 14:47:08                     
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;
using UnityEngine.UI;

namespace SsitEngine.Unity.HUD
{
    /// <summary>
    ///     hud-雷达预设
    /// </summary>
    public class ScriptRadar : ScriptHudPrefab
    {
        #region Override Methods

        /// <summary>
        ///     Change the color of the radar icon.
        /// </summary>
        /// <param name="color">Color.</param>
        public override void ChangeIconColor( Color color )
        {
            base.ChangeIconColor(color);
            if (Icon != null)
            {
                Icon.color = color;
            }
        }

        #endregion

        #region Variables

        [Header("Icon")] [Tooltip("Assign an image component.")]
        public Image Icon;

        [Header("Height Arrows")] [Tooltip("Assign the above arrow image component.")]
        public Image ArrowAbove;

        [Tooltip("Assign the above arrow image component.")]
        public Image ArrowBelow;

        #endregion


        #region Main Methods

        #endregion
    }
}