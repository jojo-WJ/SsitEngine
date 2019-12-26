using UnityEngine;

namespace SsitEngine.Unity.HUD
{
    public interface ICompassBar
    {
        HudCanvas._CompassBarReferences CompassBar { get; set; }
        Transform ElementContainer { get; set; }

        void UpdateCompassBar( Transform rotationReference );

        void ShowCompassBar( bool value );
    }
}