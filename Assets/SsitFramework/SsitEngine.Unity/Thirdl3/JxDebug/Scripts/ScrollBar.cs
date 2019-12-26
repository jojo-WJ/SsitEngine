using System;
using UnityEngine;

namespace JxDebug
{
    [Serializable]
    public class ScrollBar
    {
        private const float padding = 3;
        private static GUISkin lastSkin;

        private static GUIStyle originalStyle;
        private static GUIStyle originalThumbStyle;
        private static GUIStyle style;
        private static GUIStyle thumbStyle;
        private static GUIContent scrollUpContent;
        private static GUIContent scrollDownContent;

        [SerializeField] protected bool invert;

        [SerializeField] protected float minHeight;

        public float sensitivity;
        public float width;

        public static void Initialize()
        {
            style = new GUIStyle();
            thumbStyle = new GUIStyle();
            thumbStyle.normal = new GUIStyleState {background = GUIUtils.grayTexture};
            scrollUpContent = new GUIContent(JxDebug.Setting.scrollUpIcon);
            scrollDownContent = new GUIContent(JxDebug.Setting.scrollDownIcon);
        }

        public float Draw( Rect rect, float position, float maxValue )
        {
            var originalRect = new Rect(rect);
            DrawScrollUpButton(ref rect, ref position);
            rect.height = originalRect.height;
            DrawScrollDownButton(ref rect, ref position);
            CalculateScrollBarRect(ref rect, originalRect);
            DrawScrollBar(rect, ref position, maxValue);
            return position;
        }

        private void DrawScrollUpButton( ref Rect rect, ref float position )
        {
            rect.height = GetButtonWidth(rect.width);
            if (GUIUtils.DrawRepeatButton(rect, scrollUpContent, Color.clear))
            {
                position -= sensitivity;
            }
        }

        private void DrawScrollDownButton( ref Rect rect, ref float position )
        {
            rect.y += rect.height - GetButtonWidth(rect.width);
            rect.height = GetButtonWidth(rect.width);
            if (GUIUtils.DrawRepeatButton(rect, scrollDownContent, Color.clear))
            {
                position += sensitivity;
            }
        }

        private void CalculateScrollBarRect( ref Rect rect, Rect originalRect )
        {
            rect.x += padding;
            rect.y = originalRect.y + rect.height;
            rect.width -= padding * 2;
            rect.height = originalRect.height - rect.height * 2;
        }

        private void DrawScrollBar( Rect rect, ref float position, float maxValue )
        {
            ChangeSkinStylesIfNecessary();
            thumbStyle.fixedHeight = Mathf.Max(rect.height * rect.height / maxValue, minHeight) / JxDebug.Setting.scale;
            maxValue -= rect.height + Mathf.Min(rect.width + padding * 2, scrollUpContent.image.height) * 2;
            position = GUI.VerticalScrollbar(rect, position, 0, invert ? maxValue : 0, invert ? 0 : maxValue);
            RestoreSkinStylesIfNecessary();
        }

        private float GetButtonWidth( float rectWidth )
        {
            return Mathf.Min(rectWidth, scrollUpContent.image.height);
        }

        private void ChangeSkinStylesIfNecessary()
        {
            if (GUI.skin.verticalScrollbar != style)
            {
                originalStyle = GUI.skin.verticalScrollbar;
                GUI.skin.verticalScrollbar = style;
            }
            if (GUI.skin.verticalScrollbarThumb != thumbStyle)
            {
                originalThumbStyle = GUI.skin.verticalScrollbarThumb;
                GUI.skin.verticalScrollbarThumb = thumbStyle;
            }
        }

        private void RestoreSkinStylesIfNecessary()
        {
            if (JxDebug.Setting.optimizeForOnGUI)
            {
                return;
            }
            if (originalStyle != null)
            {
                GUI.skin.verticalScrollbar = originalStyle;
            }
            if (originalThumbStyle != null)
            {
                GUI.skin.verticalScrollbarThumb = originalThumbStyle;
            }
        }
    }
}