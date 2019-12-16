using SsitEngine;
using UnityEngine;

namespace JxDebug
{
    public class SliderHUV
    {
        public delegate float SliderGetter();
        public delegate void SliderSetter( float value );

        //static GUIStyle style;
        //static GUIStyle thumbStyle;

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
            //style = new GUIStyle(GUI.skin.horizontalSlider);
            //style.normal = new GUIStyleState() { background = JxDebug.Setting.sliderIcon };
            //style.fixedHeight = style.fixedHeight = 5;
            //thumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb);
            //thumbStyle.normal = thumbStyle.active = thumbStyle.focused = thumbStyle.hover = new GUIStyleState() { background = null };
            //thumbStyle.overflow = new RectOffset(8, 8, 8, 8);
            //thumbStyle.fixedWidth = thumbStyle.fixedWidth = 0;
            //thumbStyle.fixedHeight = thumbStyle.fixedWidth = 10;
        }

        public SliderHUV( string label, SliderGetter getter, SliderSetter setter ) : this(new GUIContent(label), getter, setter)
        {

        }
        public SliderHUV( GUIContent label, SliderGetter getter, SliderSetter setter )
        {
            this.label = label;
            this.getter = getter;
            this.setter = setter;
        }

        public void Draw( Rect rect, float min, float max )
        {
            //ChangeSkinStylesIfNecessary();
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
            DrawValue(ref rect, widthFraction, value,max);
            CallSetterIfAppropriate(originalValue, value);
            //RestoreSkinStylesIfNecessary();

            GUI.enabled = true;
        }

        //Private Members

        private void DrawLabel( ref Rect rect, float widthFraction )
        {
            rect.width = 4 * widthFraction;
            GUI.Label(rect, label, GUIUtils.CenteredTextStyle);
            rect.x += rect.width;
        }

        private void DrawSlider( ref Rect rect, float widthFraction, ref float value, float min, float max )
        {
            GUI.skin = JxDebug.Singleton.MGuiBackSkin;
            rect.width = 5 * widthFraction;
            int id = GUIUtility.GetControlID(label.text.GetHashCode(), FocusType.Passive, rect);
            value = GUI.HorizontalSlider(rect, value, min, max);
            GUI.color = Color.cyan;
            Rect slideRect = new Rect(rect.x, rect.y, rect.width * value / max, rect.height);
            GUI.HorizontalSlider(slideRect, value, min, max);
            rect.x += rect.width;
            hasFocus = GUIUtility.hotControl == id + 1;
            GUI.skin = JxDebug.Singleton.MGuiSkin;
            GUI.color = Color.white;

        }

        private void DrawValue( ref Rect rect, float widthFraction, float value ,float max)
        {
            rect.width = 2 * widthFraction;
            string total = TextUtils.Format("Current:<color=#FFFFFF>{0}</color>MB\nTotal: <color=#FFFFFF>{1}</color>MB", value, max);

          
            //string valueToShow = CutDecimals(value).ToString();
            GUI.Label(rect, total, GUIUtils.RightTextStyle);
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
    }
}