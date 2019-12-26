/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/27 20:52:59                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Unity.UI.Common.Generic
{
    /// <summary>Mode of color blend.</summary>
    public enum ColorBlendMode
    {
        /// <summary>Normal-Normal Mode.</summary>
        Normal,

        /// <summary>Normal-Dissolve Mode.</summary>
        Dissolve,

        /// <summary>Darken-Darken Mode.</summary>
        Darken,

        /// <summary>Darken-Multiply Mode.</summary>
        Multiply,

        /// <summary>Darken-ColorBurn Mode.</summary>
        ColorBurn,

        /// <summary>Darken-LinearBurn Mode.</summary>
        LinearBurn,

        /// <summary>Darken-DarkerColor Mode.</summary>
        DarkerColor,

        /// <summary>Lighten-Lighten Mode.</summary>
        Lighten,

        /// <summary>Lighten-Screen Mode.</summary>
        Screen,

        /// <summary>Lighten-ColorDodge Mode.</summary>
        ColorDodge,

        /// <summary>Lighten-LinearDodge Mode.</summary>
        LinearDodge,

        /// <summary>Lighten-LighterColor Mode.</summary>
        LighterColor,

        /// <summary>Contrast-Overlay Mode.</summary>
        Overlay,

        /// <summary>Contrast-SoftLight Mode.</summary>
        SoftLight,

        /// <summary>Contrast-HardLight Mode.</summary>
        HardLight,

        /// <summary>Contrast-VividLight Mode.</summary>
        VividLight,

        /// <summary>Contrast-LinearLight Mode.</summary>
        LinearLight,

        /// <summary>Contrast-PinLight Mode.</summary>
        PinLight,

        /// <summary>Contrast-HardMix Mode.</summary>
        HardMix,

        /// <summary>Cancelation-Difference Mode.</summary>
        Difference,

        /// <summary>Cancelation-Exclusion Mode.</summary>
        Exclusion,

        /// <summary>Cancelation-Subtract Mode.</summary>
        Subtract,

        /// <summary>Cancelation-Divide Mode.</summary>
        Divide,

        /// <summary>Component-Hue Mode.</summary>
        Hue,

        /// <summary>Component-Color Mode.</summary>
        Color,

        /// <summary>Component-Saturation Mode.</summary>
        Saturation,

        /// <summary>Component-Luminosity Mode.</summary>
        Luminosity
    }
}