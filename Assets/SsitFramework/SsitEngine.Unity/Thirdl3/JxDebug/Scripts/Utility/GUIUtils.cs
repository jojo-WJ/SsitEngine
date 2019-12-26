using UnityEngine;

namespace JxDebug
{
    /// <summary>
    /// GUI工具类
    /// </summary>
    public static class GUIUtils
    {
        //浅灰度色粉
        private const float slightlyGrayTone = 0.85f;

        //浅灰度色调值
        private const float lightGrayTone = 0.7f;

        //灰度色粉
        private const float grayTone = 0.55f;

        //灰度色调值
        private const float grayerTone = 0.4f;

        //黑度色粉
        private const float darkGrayTone = 0.25f;

        //黑度色调值
        private const float darkerGrayTone = 0.15f;

        //白色
        public static readonly Color whiteColor = Color.white;

        //浅灰色
        public static readonly Color slightlyGrayColor =
            new Color(slightlyGrayTone, slightlyGrayTone, slightlyGrayTone, 1);

        //亮灰色
        public static readonly Color lightGrayColor = new Color(lightGrayTone, lightGrayTone, lightGrayTone, 1);
        public static readonly Color grayColor = new Color(grayTone, grayTone, grayTone, 1);
        public static readonly Color grayerColor = new Color(grayerTone, grayerTone, grayerTone, 1);
        public static readonly Color darkGrayColor = new Color(darkGrayTone, darkGrayTone, darkGrayTone, 1);
        public static readonly Color darkerGrayColor = new Color(darkerGrayTone, darkerGrayTone, darkerGrayTone, 1);
        public static readonly Color blackColor = Color.black;

        public static Texture2D whiteTexture;
        public static Texture2D slightlyGrayTexture;
        public static Texture2D lightGrayTexture;
        public static Texture2D grayTexture;
        public static Texture2D grayerTexture;
        public static Texture2D darkGrayTexture;
        public static Texture2D darkerGrayTexture;
        public static GUIStyle boxStyle;

        private static GUIStyle s_centeredButtonStyle;
        private static GUIStyle s_buttonStyle;
        private static GUIStyle s_textStyle;
        private static GUIStyle s_centeredTextStyle;
        private static GUIStyle s_rightTextStyle;
        private static GUIStyle s_inputStyle;

        /// <summary>
        /// 静态构造
        /// </summary>
        static GUIUtils()
        {
            whiteTexture = Texture2D.whiteTexture;
            slightlyGrayTexture = CreateColorTexture(slightlyGrayColor);
            lightGrayTexture = CreateColorTexture(lightGrayColor);
            grayTexture = CreateColorTexture(grayColor);
            grayerTexture = CreateColorTexture(grayerColor);
            darkGrayTexture = CreateColorTexture(darkGrayColor);
            darkerGrayTexture = CreateColorTexture(darkerGrayColor);

            ButtonStyle = new GUIStyle();
            ButtonStyle.normal = new GUIStyleState {background = whiteTexture};
            ButtonStyle.hover = new GUIStyleState {background = slightlyGrayTexture};
            ButtonStyle.active = new GUIStyleState {background = lightGrayTexture};
            ButtonStyle.alignment = TextAnchor.MiddleLeft;
            ButtonStyle.clipping = TextClipping.Clip;

            CenteredButtonStyle = new GUIStyle(ButtonStyle);
            CenteredButtonStyle.alignment = TextAnchor.MiddleCenter;

            boxStyle = new GUIStyle();
            boxStyle.normal = new GUIStyleState {background = whiteTexture};

            TextStyle = new GUIStyle();
            TextStyle.normal = new GUIStyleState {textColor = slightlyGrayColor};
            TextStyle.wordWrap = true;
            TextStyle.richText = true;
            TextStyle.clipping = TextClipping.Clip;

            CenteredTextStyle = new GUIStyle(TextStyle);
            CenteredTextStyle.alignment = TextAnchor.MiddleCenter;

            RightTextStyle = new GUIStyle(TextStyle);
            RightTextStyle.alignment = TextAnchor.MiddleRight;
            RightTextStyle.richText = true;

            InputStyle = new GUIStyle();
            InputStyle.alignment = TextAnchor.MiddleLeft;
            InputStyle.clipping = TextClipping.Clip;
        }

        public static GUIStyle CenteredButtonStyle
        {
            get
            {
                UpdateFont(s_centeredButtonStyle);
                return s_centeredButtonStyle;
            }
            set => s_centeredButtonStyle = value;
        }

        public static GUIStyle ButtonStyle
        {
            get
            {
                UpdateFont(s_buttonStyle);
                return s_buttonStyle;
            }
            set => s_buttonStyle = value;
        }

        public static GUIStyle TextStyle
        {
            get
            {
                UpdateFont(s_textStyle);
                return s_textStyle;
            }
            set => s_textStyle = value;
        }

        public static GUIStyle CenteredTextStyle
        {
            get
            {
                UpdateFont(s_centeredTextStyle);
                return s_centeredTextStyle;
            }
            set => s_centeredTextStyle = value;
        }

        public static GUIStyle RightTextStyle
        {
            get
            {
                UpdateFont(s_rightTextStyle);
                return s_rightTextStyle;
            }
            set => s_rightTextStyle = value;
        }

        public static GUIStyle InputStyle
        {
            get
            {
                UpdateFont(s_inputStyle);
                s_inputStyle.normal.textColor = JxDebug.Setting.inputTextColor;
                s_inputStyle.fontStyle = FontStyle.Bold;
                return s_inputStyle;
            }
            set => s_inputStyle = value;
        }

        /// <summary>
        /// 创建颜色纹理
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        private static Texture2D CreateColorTexture( Color color )
        {
            return ApplyColorToTexture(whiteTexture, color);
        }

        /// <summary>
        /// 应用颜色到纹理
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Texture2D ApplyColorToTexture( Texture2D texture, Color color )
        {
            var copyTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            var pixels = texture.GetPixels();
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            copyTexture.SetPixels(pixels);
            copyTexture.Apply();
            return copyTexture;
        }

        /// <summary>
        /// 更新GUI字体
        /// </summary>
        /// <param name="style"></param>
        private static void UpdateFont( GUIStyle style )
        {
            style.font = JxDebug.Setting.font;
            style.fontSize = JxDebug.Setting.fontSize;
        }

        /// <summary>
        /// 绘制GUI按钮
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="content"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool DrawButton( Rect rect, GUIContent content, Color color )
        {
            return DrawButton(rect, content, color, ButtonStyle);
        }

        /// <summary>
        /// 绘制GUI中心按钮
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="content"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool DrawCenteredButton( Rect rect, GUIContent content, Color color )
        {
            return DrawButton(rect, content, color, CenteredButtonStyle);
        }

        /// <summary>
        /// 绘制GUI按钮
        /// </summary>
        public static bool DrawButton( Rect rect, GUIContent content, Color color, GUIStyle style )
        {
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            var result = GUI.Button(rect, content, style);
            GUI.backgroundColor = oldColor;
            return result;
        }

        /// <summary>
        /// 绘制GUI重复按钮
        /// </summary>
        public static bool DrawRepeatButton( Rect rect, GUIContent content, Color color )
        {
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            var result = GUI.RepeatButton(rect, content, CenteredButtonStyle);
            GUI.backgroundColor = oldColor;
            return result;
        }

        /// <summary>
        /// 绘制GUI区域框
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        public static void DrawBox( Rect rect, Color color )
        {
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUI.Box(rect, GUIContent.none, boxStyle);
            GUI.backgroundColor = oldColor;
        }
    }
}