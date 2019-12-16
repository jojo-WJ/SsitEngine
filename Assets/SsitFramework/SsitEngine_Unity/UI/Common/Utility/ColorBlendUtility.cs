using SsitEngine.Unity.UI.Common.Generic;
using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Utility
{
    /// <summary>Utility for color blend.</summary>
    public static class ColorBlendUtility
    {
        /// <summary>Blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <param name="mode">Mode of color blend.</param>
        /// <returns>Blended color.</returns>
        public static Color Blend( Color a, Color b, ColorBlendMode mode )
        {
            var color = Color.white;
            switch (mode)
            {
                case ColorBlendMode.Normal:
                    color = NormalBlend(a, b);
                    break;
                case ColorBlendMode.Dissolve:
                    color = DissolveBlend(a, b);
                    break;
                case ColorBlendMode.Darken:
                    color = DarkenBlend(a, b);
                    break;
                case ColorBlendMode.Multiply:
                    color = MultiplyBlend(a, b);
                    break;
                case ColorBlendMode.ColorBurn:
                    color = ColorBurnBlend(a, b);
                    break;
                case ColorBlendMode.LinearBurn:
                    color = LinearBurnBlend(a, b);
                    break;
                case ColorBlendMode.DarkerColor:
                    color = DarkerColorBlend(a, b);
                    break;
                case ColorBlendMode.Lighten:
                    color = LightenBlend(a, b);
                    break;
                case ColorBlendMode.Screen:
                    color = ScreenBlend(a, b);
                    break;
                case ColorBlendMode.ColorDodge:
                    color = ColorDodgeBlend(a, b);
                    break;
                case ColorBlendMode.LinearDodge:
                    color = LinearDodgeBlend(a, b);
                    break;
                case ColorBlendMode.LighterColor:
                    color = LighterColorBlend(a, b);
                    break;
                case ColorBlendMode.Overlay:
                    color = OverlayBlend(a, b);
                    break;
                case ColorBlendMode.SoftLight:
                    color = SoftLightBlend(a, b);
                    break;
                case ColorBlendMode.HardLight:
                    color = HardLightBlend(a, b);
                    break;
                case ColorBlendMode.VividLight:
                    color = VividLightBlend(a, b);
                    break;
                case ColorBlendMode.LinearLight:
                    color = LinearLightBlend(a, b);
                    break;
                case ColorBlendMode.PinLight:
                    color = PinLightBlend(a, b);
                    break;
                case ColorBlendMode.HardMix:
                    color = HardMixBlend(a, b);
                    break;
                case ColorBlendMode.Difference:
                    color = DifferenceBlend(a, b);
                    break;
                case ColorBlendMode.Exclusion:
                    color = ExclusionBlend(a, b);
                    break;
                case ColorBlendMode.Subtract:
                    color = SubtractBlend(a, b);
                    break;
                case ColorBlendMode.Divide:
                    color = DivideBlend(a, b);
                    break;
                case ColorBlendMode.Hue:
                    color = HueBlend(a, b);
                    break;
                case ColorBlendMode.Color:
                    color = ColorBlend(a, b);
                    break;
                case ColorBlendMode.Saturation:
                    color = SaturationBlend(a, b);
                    break;
                case ColorBlendMode.Luminosity:
                    color = LuminosityBlend(a, b);
                    break;
            }
            return color;
        }

        /// <summary>Normal mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color NormalBlend( Color a, Color b )
        {
            return b * b.a + a * (1f - b.a);
        }

        /// <summary>Dissolve mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color DissolveBlend( Color a, Color b )
        {
            if (Random.Range(0, 1) != 0)
                return b;
            return a;
        }

        /// <summary>Darken mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color DarkenBlend( Color a, Color b )
        {
            var magnitude1 = a.maxColorComponent;
            var magnitude2 = b.maxColorComponent;
            if (Mathf.Min(magnitude1, magnitude2) != magnitude1)
                return b;
            return a;
        }

        /// <summary>Multiply mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color MultiplyBlend( Color a, Color b )
        {
            return a * b;
        }

        /// <summary>ColorBurn mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color ColorBurnBlend( Color a, Color b )
        {
            var color = Color.white - a;
            return Color.white - new Color(color.r / b.r, color.g / b.g, color.b / b.b, color.a / b.a);
        }

        /// <summary>LinearBurn vblend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color LinearBurnBlend( Color a, Color b )
        {
            return a + b - Color.white;
        }

        /// <summary>DarkerColor mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color DarkerColorBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>Lighten mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color LightenBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>Screen mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color ScreenBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>ColorDodge mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color ColorDodgeBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>LinearDodge mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color LinearDodgeBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>LighterColor mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color LighterColorBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>Overlay mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color OverlayBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>SoftLight mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color SoftLightBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>HardLight mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color HardLightBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>VividLight mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color VividLightBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>LinearLight mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color LinearLightBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>PinLight mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color PinLightBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>HardMix mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color HardMixBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>Difference mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color DifferenceBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>Exclusion mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color ExclusionBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>Subtract mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color SubtractBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>Divide mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color DivideBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>Hue mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color HueBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>Component-Color mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color ColorBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>Saturation mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color SaturationBlend( Color a, Color b )
        {
            return Color.red;
        }

        /// <summary>Luminosity mode blend color a and color b.</summary>
        /// <param name="a">Color a.</param>
        /// <param name="b">Color b.</param>
        /// <returns>Blended color.</returns>
        public static Color LuminosityBlend( Color a, Color b )
        {
            return Color.red;
        }
    }
}