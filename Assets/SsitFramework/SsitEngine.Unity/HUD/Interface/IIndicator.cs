using UnityEngine;

namespace SsitEngine.Unity.HUD.Interface
{
    public interface IIndicator
    {
        HudCanvas._IndicatorReferences Indicator { get; set; }
        Transform ElementContainer { get; set; }

        void ShowIndicators( bool value );
    }
}