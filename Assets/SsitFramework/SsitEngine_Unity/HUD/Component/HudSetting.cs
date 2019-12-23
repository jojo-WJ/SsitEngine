/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/8/6 14:17:47                     
*└──────────────────────────────────────────────────────────────┘
*/

using UnityEngine;

namespace SsitEngine.Unity.HUD
{
    /// <summary>
    ///     hud设置
    /// </summary>
    [CreateAssetMenu(fileName = "New Element Settings", menuName = "HudSetting_" + "/New Element Settings")]
    public class HudSetting : ScriptableObject
    {
        #region Main Methods

        public void CopySettings( HudElement element )
        {
            if (element == null)
            {
                return;
            }

            // misc
            Prefabs = element.Prefabs;

            // radar settings
            //this.hideInRadar = element.hideInRadar;
            //this.ignoreRadarRadius = element.ignoreRadarRadius;
            //this.rotateWithGameObject = element.rotateWithGameObject;
            //this.useRadarHeightSystem = element.useRadarHeightSystem;

            // compass bar settings
            hideInCompassBar = element.HideInCompassBar;
            ignoreCompassBarRadius = element.ignoreCompassBarRadius;
            useCompassBarDistanceText = element.useCompassBarDistanceText;
            compassBarDistanceTextFormat = element.compassBarDistanceTextFormat;

            // indicator settings
            showIndicator = element.ShowIndicator;
            showOffscreenIndicator = element.showOffscreenIndicator;
            ignoreIndicatorRadius = element.ignoreIndicatorRadius;
            ignoreIndicatorHideDistance = element.ignoreIndicatorHideDistance;
            ignoreIndicatorScaling = element.ignoreIndicatorScaling;
            useIndicatorDistanceText = element.useIndicatorDistanceText;
            showOffscreenIndicatorDistance = element.showOffscreenIndicatorDistance;
            indicatorOnscreenDistanceTextFormat = element.indicatorOnscreenDistanceTextFormat;
            indicatorOffscreenDistanceTextFormat = element.indicatorOffscreenDistanceTextFormat;

            // minimap settings
            //this.hideInMinimap = element.hideInMinimap;
            //this.ignoreMinimapRadius = element.ignoreMinimapRadius;
            //this.rotateWithGameObjectMM = element.rotateWithGameObjectMM;
            //this.useMinimapHeightSystem = element.useMinimapHeightSystem;
        }

        #endregion

        #region Variables

        // MISC
        public HudCollection Prefabs = new HudCollection();

        // RADAR SETTINGS
        // public bool hideInRadar = false;
        // public bool ignoreRadarRadius = false;
        // public bool rotateWithGameObject = true;
        // public bool useRadarHeightSystem = true;

        // COMPASS BAR SETTINGS
        public bool hideInCompassBar;
        public bool ignoreCompassBarRadius;
        public bool useCompassBarDistanceText = true;
        public string compassBarDistanceTextFormat = "{0}m";

        // INDICATOR SETTINGS
        public bool showIndicator = true;
        public bool showOffscreenIndicator = true;
        public bool ignoreIndicatorRadius = true;
        public bool ignoreIndicatorHideDistance;
        public bool ignoreIndicatorScaling;
        public bool useIndicatorDistanceText = true;
        public bool showOffscreenIndicatorDistance;
        public string indicatorOnscreenDistanceTextFormat = "{0}m";
        public string indicatorOffscreenDistanceTextFormat = "{0}";

        // MINIMAP
        public bool hideInMinimap;
        public bool ignoreMinimapRadius;
        public bool rotateWithGameObjectMM = true;
        public bool useMinimapHeightSystem = true;

        #endregion
    }
}