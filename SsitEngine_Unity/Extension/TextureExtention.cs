﻿/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：纹理扩展                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 20:49:12                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.DebugLog;
using UnityEngine;

namespace SsitEngine.Unity.Extension
{
    /// <summary>Extention for UnityEngine.Texture2D.</summary>
    public static class TextureExtention
    {
        /// <summary>Update the pixels of Texture2D.</summary>
        /// <param name="texture2D">Base Texture2D.</param>
        /// <param name="colorArray">Color array for pixels.</param>
        /// <param name="mipLevel">The mip level of the texture to write to.</param>
        /// <param name="updateMipmaps">When set to true, mipmap levels are recalculated.</param>
        /// <param name="makeNointerReadable">When set to true, system memory copy of a texture is released.</param>
        public static void UpdatePixels(
            this Texture2D texture2D,
            Color[] colorArray,
            int mipLevel = 0,
            bool updateMipmaps = false,
            bool makeNointerReadable = false )
        {
            if (colorArray == null || colorArray.Length != texture2D.width * texture2D.height)
            {
                SsitDebug.Error("Update pixels of Texture2D error: The color array is null or invalid.");
            }
            else
            {
                texture2D.SetPixels(colorArray, mipLevel);
                texture2D.Apply(updateMipmaps, makeNointerReadable);
            }
        }


        /// <summary>Update the pixels of Texture2D.</summary>
        /// <param name="texture2D">Base Texture2D.</param>
        /// <param name="colorArray">Color array for pixels.</param>
        /// <param name="mipLevel">The mip level of the texture to write to.</param>
        /// <param name="updateMipmaps">When set to true, mipmap levels are recalculated.</param>
        /// <param name="makeNointerReadable">When set to true, system memory copy of a texture is released.</param>
        public static void UpdatePixels(
            this Texture2D texture2D,
            Color32[] colorArray,
            int mipLevel = 0,
            bool updateMipmaps = false,
            bool makeNointerReadable = false )
        {
            if (colorArray == null || colorArray.Length != texture2D.width * texture2D.height)
            {
                SsitDebug.Error("Update pixels of Texture2D error: The color array is null or invalid.");
            }
            else
            {
                texture2D.SetPixels32(colorArray, mipLevel);
                texture2D.Apply(updateMipmaps, makeNointerReadable);
            }
        }
    }
}