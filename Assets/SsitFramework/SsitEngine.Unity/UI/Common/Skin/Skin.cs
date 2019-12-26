/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 20:55:57                     
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Skin
{
    /// <summary>Interface of skinned mesh.</summary>
    public interface ISkin
    {
        /// <summary>Skinned mesh renderer of skin.</summary>
        SkinnedMeshRenderer Renderer { get; }

        /// <summary>Mesh collider of skin.</summary>
        MeshCollider Collider { get; }

        /// <summary>Rebuild the mesh of skin.</summary>
        void Rebuild();

        /// <summary>Attach collider to skin mesh.</summary>
        void AttachCollider();

        /// <summary>Remove collider from skin mesh.</summary>
        void RemoveCollider();
    }
}