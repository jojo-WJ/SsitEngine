/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/6/3 18:01:48                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using UnityEngine;

namespace Framework.SceneObject
{
    public class LayerUtils
    {
        // Built-in Unity layers.
        private const int DefaultLayer = 0;
        private const int TransparentFXLayer = 1;
        private const int IgnoreRaycastLayer = 2;
        private const int WaterLayer = 4;
        private const int UILayer = 5;

        // Custom layers.
        private const int SceneObject = 8;
        private const int CharacterLayer = 9;
        private const int EnemyLayer = 10;
        private const int VisualEffectLayer = 17;
        private const int VisualEffectLayerWith = 18;
        private const int CameraCullLayer = 19;
        private const int CameraCullWithLayer = 20;

        private const int OverlayLayer = 29;
        private const int SubCharacterLayer = 30;

        private const int RoadLayer = 15;
        private const int GroundLayer = 16;
        private static bool s_Initialized;

        public static LayerMask InputDetectLayer = (1 << DefaultLayer)
                                                   | (1 << CharacterLayer)
                                                   | (1 << GroundLayer)
                                                   | (1 << RoadLayer)
                                                   | (1 << VisualEffect)
                                                   | (1 << VisualEffectLayerWith)
                                                   | (1 << CameraCullWith);

        public static LayerMask DragDetectedLayerMask = (1 << GroundLayer)
                                                        | (1 << RoadLayer)
                                                        | (1 << DefaultLayer);


        public static LayerMask PlayerGroudMask = (1 << DefaultLayer)
                                                  | (1 << RoadLayer)
                                                  | (1 << CameraCullWith)
                                                  | (1 << GroundLayer);

        private static Dictionary<Collider, List<Collider>> s_IgnoreCollisionMap;

        public static int Default => DefaultLayer;
        public static int TransparentFX => TransparentFXLayer;
        public static int IgnoreRaycast => IgnoreRaycastLayer;
        public static int Water => WaterLayer;
        public static int UI => UILayer;

        public static int Enemy => EnemyLayer;
        public static int VisualEffect => VisualEffectLayer;
        public static int VisualEffectWith => VisualEffectLayer;
        public static int CameraCull => CameraCullLayer;
        public static int CameraCullWith => CameraCullWithLayer;
        public static int Overlay => OverlayLayer;
        public static int SubCharacter => SubCharacterLayer;
        public static int Character => CharacterLayer;
        public static int Ground => GroundLayer;

        public static int Road => RoadLayer;

        /*
        /// <summary>
        /// Setup the layer collisions.
        /// </summary>
        private void Awake()
        {
            Physics.IgnoreLayerCollision(IgnoreRaycast, VisualEffect);
            Physics.IgnoreLayerCollision(SubCharacter, Default);
            Physics.IgnoreLayerCollision(SubCharacter, VisualEffect);
            Physics.IgnoreLayerCollision(VisualEffect, VisualEffect);
        }
        */

        /// <summary>
        /// Ignore the collision between the main collider and the other collider.
        /// </summary>
        /// <param name="mainCollider">The main collider collision to ignore.</param>
        /// <param name="otherCollider">The collider to ignore.</param>
        public static void IgnoreCollision( Collider mainCollider, Collider otherCollider )
        {
            // Keep a mapping of the colliders that mainCollider is ignorning so the collision can easily be reverted.
            if (s_IgnoreCollisionMap == null) s_IgnoreCollisionMap = new Dictionary<Collider, List<Collider>>();

            // Add the collider to the list so it can be reverted.
            List<Collider> colliderList;
            if (!s_IgnoreCollisionMap.TryGetValue(mainCollider, out colliderList))
            {
                colliderList = new List<Collider>();
                s_IgnoreCollisionMap.Add(mainCollider, colliderList);
            }
            colliderList.Add(otherCollider);

            // The otherCollider must also keep track of the mainCollder. This allows otherCollider to be removed before mainCollider.
            if (!s_IgnoreCollisionMap.TryGetValue(otherCollider, out colliderList))
            {
                colliderList = new List<Collider>();
                s_IgnoreCollisionMap.Add(otherCollider, colliderList);
            }
            colliderList.Add(mainCollider);

            // Do the actual ignore.
            Physics.IgnoreCollision(mainCollider, otherCollider);
        }

        /// <summary>
        /// The main collider should no longer ignore any collisions.
        /// </summary>
        /// <param name="mainCollider">The collider to revert the collisions on.</param>
        public static void RevertCollision( Collider mainCollider )
        {
            List<Collider> colliderList;
            List<Collider> otherColliderList;
            // Revert the IgnoreCollision setting on all of the colliders that the object is currently ignoring.
            if (s_IgnoreCollisionMap != null && s_IgnoreCollisionMap.TryGetValue(mainCollider, out colliderList))
            {
                for (var i = 0; i < colliderList.Count; ++i)
                {
                    if (!mainCollider.enabled || !mainCollider.gameObject.activeInHierarchy ||
                        !colliderList[i].enabled || !colliderList[i].gameObject.activeInHierarchy) continue;

                    Physics.IgnoreCollision(mainCollider, colliderList[i], false);

                    // A two way map was added when the initial IgnoreCollision was added. Remove that second map because the IgnoreCollision has been removed.
                    if (s_IgnoreCollisionMap.TryGetValue(colliderList[i], out otherColliderList))
                        for (var j = 0; j < otherColliderList.Count; ++j)
                            if (otherColliderList[j].Equals(mainCollider))
                            {
                                otherColliderList.RemoveAt(j);
                                break;
                            }
                }
                colliderList.Clear();
            }
        }

        public static void MoveToLayer( Transform root, int layer, Dictionary<GameObject, int> layerBuffer = null )
        {
            if (layerBuffer != null) layerBuffer.Add(root.gameObject, root.gameObject.layer);
            root.gameObject.layer = layer;
            foreach (Transform child in root) MoveToLayer(child, layer, layerBuffer);
        }
    }
}