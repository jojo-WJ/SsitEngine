/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/8/6 14:37:37                     
*└──────────────────────────────────────────────────────────────┘
*/
//#define ActiveRadar

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SsitEngine.Unity.HUD
{
    public class HudCanvas : SingletonMono<HudCanvas>
    {
        #region Variables

        public _RadarReferences Radar;
        public _CompassBarReferences CompassBar;
        public _IndicatorReferences Indicator;
        public _MinimapReferences Minimap;


        public float CompassBarCurrentDegrees { get; private set; }

        #endregion


        #region Main Methods

        #endregion


        #region Radar Methods

#if ActiveRadar
        public void InitRadar()
        {
            // check references
            if (Radar.Panel == null || Radar.Radar == null || Radar.ElementContainer == null)
            {
                ReferencesMissing("Radar");
                return;
            }

            // show radar
            ShowRadar(true);
        }


        public void ShowRadar(bool value)
        {
            if (Radar.Panel != null)
                Radar.Panel.gameObject.SetActive(value);
        }


        public void UpdateRadar(Transform rotationReference, RadarModes radarType)
        {
            // assign map / player indicator rotation
            if (radarType == RadarModes.RotateRadar)
            {
                // set radar rotation
                Radar.Radar.transform.rotation =
 Quaternion.Euler(Radar.Panel.transform.eulerAngles.x, Radar.Panel.transform.eulerAngles.y, rotationReference.eulerAngles.y);
                if (Radar.PlayerIndicator != null)
                    Radar.PlayerIndicator.transform.rotation = Radar.Panel.transform.rotation;
            }
            else
            {
                // set player indicator rotation
                Radar.Radar.transform.rotation = Radar.Panel.transform.rotation;
                if (Radar.PlayerIndicator != null)
                    Radar.PlayerIndicator.transform.rotation =
 Quaternion.Euler(Radar.Panel.transform.eulerAngles.x, Radar.Panel.transform.eulerAngles.y, -rotationReference.eulerAngles.y);
            }
        }
#endif

        #endregion


        #region Compass Bar Methods

        public void InitCompassBar()
        {
            // check references
            if (CompassBar.Panel == null || CompassBar.Compass == null || CompassBar.ElementContainer == null)
            {
                ReferencesMissing("Compass Bar");
                return;
            }

            // show compass bar
            ShowCompassBar(true);
        }


        public void ShowCompassBar( bool value )
        {
            if (CompassBar.Panel != null)
                CompassBar.Panel.gameObject.SetActive(value);
        }


        public void UpdateCompassBar( Transform rotationReference )
        {
            // set compass bar texture coordinates
            CompassBar.Compass.uvRect = new Rect(rotationReference.eulerAngles.y / 360f - .5f, 0f, 1f, 1f);

            // calculate 0-360 degrees value
            var perpDirection = Vector3.Cross(Vector3.forward, rotationReference.forward);
            var angle = Vector3.Angle(new Vector3(rotationReference.forward.x, 0f, rotationReference.forward.z),
                Vector3.forward);
            CompassBarCurrentDegrees = perpDirection.y >= 0f ? angle : 360f - angle;
        }

        #endregion


        #region Indicator Methods

        public void InitIndicators()
        {
            // check references
            if (Indicator.Panel == null || Indicator.ElementContainer == null)
            {
                ReferencesMissing("Indicator");
                return;
            }

            // show indicators
            ShowIndicators(true);
        }


        public void ShowIndicators( bool value )
        {
            if (Indicator.Panel != null)
                Indicator.Panel.gameObject.SetActive(value);
        }

        #endregion


        #region Minimap Methods

        public void InitMinimap( MapAsset profile )
        {
            // check references
            if (Minimap.Panel == null || Minimap.MapContainer == null || Minimap.ElementContainer == null)
            {
                ReferencesMissing("Minimap");
                return;
            }

            // create minimap image gameobject
            var imageGO = new GameObject(profile.MapTexture.name);
            imageGO.transform.SetParent(Minimap.MapContainer, false);

            // setup minimap image component
            var image = imageGO.AddComponent<Image>();
            image.sprite = profile.MapTexture;
            image.preserveAspect = true;
            image.SetNativeSize();

            // create custom layers
            if (profile.CustomLayers.Count > 0)
            {
                var layerCount = 0;
                foreach (var layer in profile.CustomLayers.Reverse<CustomLayer>())
                {
                    if (layer.sprite == null)
                        continue;

                    // create layer image gameobject
                    var layerGO = new GameObject(layer.name + "_Layer_" + layerCount++);
                    layerGO.transform.SetParent(Minimap.MapContainer, false);
                    layerGO.SetActive(layer.enabled);

                    // setup minimap image component
                    var layerImage = layerGO.AddComponent<Image>();
                    layerImage.sprite = layer.sprite;
                    layerImage.preserveAspect = true;
                    layerImage.SetNativeSize();

                    // assign layer instance
                    layer.instance = layerGO;
                }
            }

            // show minimap
            ShowMinimap(true);
        }


        public void ShowMinimap( bool value )
        {
            if (Minimap.Panel != null)
                Minimap.Panel.gameObject.SetActive(value);
        }


        public void UpdateMinimap( Transform rotationReference, MinimapModes minimapMode, Transform playerTransform,
            MapAsset profile, float scale )
        {
            // assign map rotation
            var identityRotation = Minimap.Panel.transform.rotation;
            var mapRotation = identityRotation;
            if (minimapMode == MinimapModes.RotateMinimap)
                mapRotation = Quaternion.Euler(Minimap.Panel.transform.eulerAngles.x,
                    Minimap.Panel.transform.eulerAngles.y, rotationReference.eulerAngles.y);

            // set player indicator rotation
            if (Minimap.PlayerIndicator != null)
            {
                if (minimapMode == MinimapModes.RotateMinimap)
                    Minimap.PlayerIndicator.transform.rotation = identityRotation;
                else
                    Minimap.PlayerIndicator.transform.rotation = Quaternion.Euler(Minimap.Panel.transform.eulerAngles.x,
                        Minimap.Panel.transform.eulerAngles.y, -rotationReference.eulerAngles.y);
            }

            // calculate map position
            var unitScale = profile.GetMapUnitScale();
            var posOffset = profile.MapBounds.center - playerTransform.position;
            var mapPos = new Vector3(posOffset.x * unitScale.x, posOffset.z * unitScale.y, 0f) * scale;

            // adjust map position when using minimap rotation mode
            if (minimapMode == MinimapModes.RotateMinimap)
                mapPos = playerTransform.MinimapRotationOffset(mapPos);

            // set map position, rotation and scale
            Minimap.MapContainer.localPosition = new Vector2(mapPos.x, mapPos.y);
            Minimap.MapContainer.rotation = mapRotation;
            Minimap.MapContainer.localScale = new Vector3(scale, scale, 1f);
        }

        #endregion


        #region Utility Methods

        private void ReferencesMissing( string feature )
        {
            Debug.LogErrorFormat("{0} references are missing! Please assign them on the HUDNavigationCanvas component.",
                feature);
            enabled = false;
        }

        #endregion


        #region Subclasses

        [Serializable]
        public class _RadarReferences
        {
            public RectTransform ElementContainer;
            public RectTransform Panel;
            public RectTransform PlayerIndicator;
            public RectTransform Radar;
        }


        [Serializable]
        public class _CompassBarReferences
        {
            public RawImage Compass;
            public RectTransform ElementContainer;
            public RectTransform Panel;
        }


        [Serializable]
        public class _IndicatorReferences
        {
            public RectTransform ElementContainer;
            public RectTransform Panel;
        }


        [Serializable]
        public class _MinimapReferences
        {
            public RectTransform ElementContainer;
            public RectTransform MapContainer;
            public RectTransform Panel;
            public RectTransform PlayerIndicator;
        }

        #endregion
    }
}