/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/8/6 15:36:04                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SsitEngine.Unity.HUD
{
    public class MapAsset : ScriptableObject
    {
        #region Variables

        public Sprite MapTexture;
        public Vector2Int MapTextureSize;
        public Bounds MapBounds = new Bounds(Vector3.zero, Vector3.one);

        [HideInInspector] public List<CustomLayer> CustomLayers = new List<CustomLayer>();

        #endregion


        #region Main Methods

        public void Init( Sprite mapTexture, Vector2Int mapTextureSize, Bounds mapBounds )
        {
            MapTexture = mapTexture;
            MapTextureSize = mapTextureSize;
            MapBounds = mapBounds;
        }


        /// <summary>
        ///     Gets a custom layer.
        /// </summary>
        /// <returns>Custom layer gameobject.</returns>
        /// <param name="name">Unique Name.</param>
        public GameObject GetCustomLayer( string name )
        {
            var custom = CustomLayers.FirstOrDefault(cl => cl.name.Equals(name));
            if (custom != null)
                return custom.instance;

            return null;
        }

        #endregion
    }


    #region Subclasses

    [Serializable]
    public class CustomLayer
    {
        [Tooltip("If checked, this layer will be enabled by default.")]
        public bool enabled;

        [HideInInspector] public GameObject instance;

        [Tooltip("Enter a unique name for this layer.")]
        public string name;

        [Tooltip("Assign the sprite texture you want to add.")]
        public Sprite sprite;
    }

    #endregion
}