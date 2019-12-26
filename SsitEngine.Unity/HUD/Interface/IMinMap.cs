using UnityEngine;

namespace SsitEngine.Unity.HUD.Interface
{
    public interface IMinMap
    {
        HudCanvas._MinimapReferences Minimap { get; set; }
        Transform ElementContainer { get; set; }
        void ShowMinimap( bool value );

        void UpdateMinimap( Transform rotationReference, MinimapModes minimapMode, Transform playerController,
            MapAsset minimapProfile, float minimapScale );
    }
}