using SsitEngine.Unity.UI.Common.Generic;
using UnityEngine;

namespace SsitEngine.Unity.UI.Common.Utility
{
    /// <summary>Utility for RectTransform.</summary>
    public static class RectUtility
    {
        /// <summary>Mirror RectTransform.</summary>
        /// <param name="rect">Target RectTransform.</param>
        /// <param name="mode">Mode of mirror.</param>
        public static void Mirror( RectTransform rect, MirrorMode mode )
        {
            var vector2 = rect.anchoredPosition;
            var x1 = rect.anchorMin.x;
            var y1 = rect.anchorMin.y;
            var x2 = rect.anchorMax.x;
            var y2 = rect.anchorMax.y;
            var x3 = rect.pivot.x;
            var y3 = rect.pivot.y;
            var num1 = 1;
            var num2 = 1;
            switch (mode)
            {
                case MirrorMode.Horizontal:
                    x1 = 1f - x1;
                    x2 = 1f - x2;
                    x3 = 1f - x3;
                    num1 = -1;
                    break;
                case MirrorMode.Vertical:
                    y1 = 1f - y1;
                    y2 = 1f - y2;
                    y3 = 1f - y3;
                    num2 = -1;
                    break;
                case MirrorMode.Both:
                    x1 = 1f - x1;
                    y1 = 1f - y1;
                    x2 = 1f - x2;
                    y2 = 1f - y2;
                    x3 = 1f - x3;
                    y3 = 1f - y3;
                    num1 = num2 = -1;
                    break;
            }
            rect.anchorMin = new Vector2(x1, y1);
            rect.anchorMax = new Vector2(x2, y2);
            rect.pivot = new Vector2(x3, y3);
            vector2 = new Vector2(vector2.x * num1, vector2.y * num2);
            rect.anchoredPosition = vector2;
        }
    }
}