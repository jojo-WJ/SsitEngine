/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/8/6 14:58:31                     
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;
using UnityEngine.UI;

namespace SsitEngine.Unity.HUD
{
    /// <summary>
    ///     对象HUD
    /// </summary>
    public class ScriptHud : ScriptHudPrefab
    {
        #region Variables

        [Header("Icon")] [Tooltip("Assign an image component.")]
        public Image Icon;

        [Header("Distance Text")] [Tooltip("Assign the distance text component.")]
        public Text NameText;

        [Header("Distance Text")] [Tooltip("Assign the distance text component.")]
        public Text StateText;

        #endregion


        #region Override Methods

        /// <summary>
        ///     Change the color of the compass bar icon.
        /// </summary>
        /// <param name="color">Color.</param>
        public override void ChangeIconColor( Color color )
        {
            base.ChangeIconColor(color);
            if (Icon != null)
                Icon.color = color;
        }

        /// <summary>
        ///     Change Text by Name
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="enable"></param>
        /// >
        public void ChangeNameText( string msg, bool enable = true )
        {
            if (!string.IsNullOrEmpty(msg) && NameText) NameText.text = msg;
            gameObject.SetActive(enable);
        }

        /// <summary>
        ///     change state context
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="enable"></param>
        /// >
        public void ChangeStateText( string msg )
        {
            if (StateText) StateText.text = msg;
        }

        #endregion
    }
}