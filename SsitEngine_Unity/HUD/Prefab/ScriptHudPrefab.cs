/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/8/6 15:02:04                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SsitEngine.Unity.HUD
{
    /// <summary>
    ///     hud-预设基类
    /// </summary>
    public class ScriptHudPrefab : MonoBase
    {
        #region Variables

        [HideInInspector] public List<CustomTransform> CustomTransforms = new List<CustomTransform>();

        [HideInInspector] public RectTransform PrefabRect;

        #endregion


        #region Main Methods

        protected virtual void OnEnable()
        {
            // assign own rect transform
            PrefabRect = GetComponent<RectTransform>();
        }


        /// <summary>
        ///     Gets a custom transform.
        /// </summary>
        /// <returns>Custom transform.</returns>
        /// <param name="name">Unique Name.</param>
        public Transform GetCustomTransform( string name )
        {
            var custom = CustomTransforms.FirstOrDefault(ct => ct.name.Equals(name));
            if (custom != null)
                return custom.transform;

            return null;
        }


        public virtual void ChangeIconColor( Color color )
        {
        }

        #endregion
    }


    #region Subclasses

    [Serializable]
    public class CustomTransform
    {
        [Tooltip("Enter a unique name for this transform.")]
        public string name;

        [Tooltip("Assign the transform you want to add.")]
        public Transform transform;
    }

    #endregion
}