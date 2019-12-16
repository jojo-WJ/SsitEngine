using UnityEngine;

namespace JxDebug
{
    public class Slider
    {
        public delegate float SliderGetter();
        public delegate void SliderSetter( float value );

        static GUIStyle originalStyle;
        static GUIStyle originalThumbStyle;
        static GUIStyle style;
        static GUIStyle thumbStyle;

        GUIContent label;
        SliderGetter getter;
        SliderSetter setter;
        bool hasFocus;
        bool lastHasFocus;
        float delayedValue;

        public int decimalsToShow { get; set; }
        public bool delayed;

        public static void Initialize()
        {
            style = new GUIStyle(GUI.skin.horizontalSlider);
            style.normal = new GUIStyleState() { background = JxDebug.Setting.sliderIcon };
            thumbStyle = new GUIStyle();
            thumbStyle.normal = thumbStyle.active = thumbStyle.focused = thumbStyle.hover = new GUIStyleState() { background = JxDebug.Setting.sliderThumbIcon };
            thumbStyle.overflow = new RectOffset(8, 8, 0, 0);
            /*thumbStyle.fixedHeight = */thumbStyle.fixedWidth = 16;
        }

        public Slider(string label, SliderGetter getter, SliderSetter setter) : this(new GUIContent(label), getter,setter)
        {

        }
        public Slider( GUIContent label, SliderGetter getter, SliderSetter setter )
        {
            this.label = label;
            this.getter = getter;
            this.setter = setter;
        }

        public void Draw( Rect rect, float min, float max )
        {
            ChangeSkinStylesIfNecessary();
            float originalValue = getter();
            float value = hasFocus && delayed ? delayedValue : originalValue;
            if (setter == null)
            {
                GUI.enabled = false;
            }
            float widthFraction = rect.width / 10;
            rect.y += rect.height / 2;
            DrawLabel(ref rect, widthFraction);
            DrawSlider(ref rect, widthFraction, ref value, min, max);
            DrawValue(ref rect, widthFraction, value);
            CallSetterIfAppropriate(originalValue, value);
            RestoreSkinStylesIfNecessary();

            GUI.enabled = true;
        }

        //Private Members

        private void DrawLabel( ref Rect rect, float widthFraction )
        {
            rect.width = 3 * widthFraction;
            GUI.Label(rect, label, GUIUtils.CenteredTextStyle);
            rect.x += rect.width;
        }

        private void DrawSlider( ref Rect rect, float widthFraction, ref float value, float min, float max )
        {
            rect.width = 5 * widthFraction;
            int id = GUIUtility.GetControlID(label.text.GetHashCode(), FocusType.Passive, rect);
            value = GUI.HorizontalSlider(rect, value, min, max);
            rect.x += rect.width;
            hasFocus = GUIUtility.hotControl == id + 1;
        }

        private void DrawValue( ref Rect rect, float widthFraction, float value )
        {
            rect.width = 2 * widthFraction;
            string valueToShow = CutDecimals(value).ToString();
            GUI.Label(rect, hasFocus && delayed ? "*" + valueToShow + "*" : valueToShow, GUIUtils.RightTextStyle);
        }

        /// <summary>
        /// 判断值差时调用设置
        /// </summary>
        /// <param name="originalValue"></param>
        /// <param name="value"></param>
        private void CallSetterIfAppropriate( float originalValue, float value )
        {
            if (hasFocus)
                delayedValue = value;

            if (originalValue != value)
            {
                if (!delayed || lastHasFocus && !hasFocus)
                    setter(value);
            }
            lastHasFocus = hasFocus;
        }

        private void ChangeSkinStylesIfNecessary()
        {
            if (GUI.skin.horizontalSlider != style)
            {
                originalStyle = GUI.skin.horizontalSlider;
                GUI.skin.horizontalSlider = style;
            }
            if (GUI.skin.horizontalSliderThumb != thumbStyle)
            {
                originalThumbStyle = GUI.skin.horizontalSliderThumb;
                GUI.skin.horizontalSliderThumb = thumbStyle;
            }
        }

        /// <summary>
        /// 阉割小数
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private float CutDecimals( float value )
        {
            int factor = 1;
            for (int i = 0; i < decimalsToShow; i++)
                factor *= 10;
            value = (int)(factor * value);
            value /= factor;
            return value;
        }

        /// <summary>
        /// 必要时回复GUI设置
        /// </summary>
        private void RestoreSkinStylesIfNecessary()
        {
            if (JxDebug.Setting.optimizeForOnGUI)
                return;
            if (originalStyle != null)
                GUI.skin.horizontalSlider = originalStyle;
            if (originalThumbStyle != null)
                GUI.skin.horizontalSliderThumb = originalThumbStyle;
        }
    }
}