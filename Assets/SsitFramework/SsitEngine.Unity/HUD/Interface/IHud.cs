using UnityEngine;

namespace SsitEngine.Unity.HUD.Interface
{
    public interface IHud
    {
        HudCanvas._IndicatorReferences HUD { get; set; }

        Transform ElementContainer { get; set; }

        void ShowHuds( bool value );
    }
}