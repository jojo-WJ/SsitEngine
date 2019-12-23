using System;
using UnityEngine;

namespace JxDebug
{
    [Serializable]
    public class SettingsPanel
    {
        private const float padding = 5;
        private static readonly Vector2 spacing = new Vector2(20, 5);
        private Slider fontSizeSlider;
        private GUIContent FpsContent;
        private float heightSeparation;
        private SliderHUV memorySizeSlider;

        private SliderHUV monoSizeSlider;

        private float panelHeight;
        private Slider preferredHeightSlider;
        private Slider scaleSlider;

        [SerializeField] protected ScrollView scrollView;

        private Toggle showTimeStampToggle;
        private GUIContent titleContent;
        private Toggle useAndFilteringToggle;

        public void Initialize()
        {
            titleContent = new GUIContent("设置及系统信息");
            FpsContent = new GUIContent("FPS:");
            showTimeStampToggle = new Toggle("时间戳", () => JxDebug.Setting.showTimeStamp,
                value => JxDebug.Setting.showTimeStamp = value);
            useAndFilteringToggle = new Toggle("使用过滤", () => JxDebug.Setting.useAndFiltering,
                value => JxDebug.Setting.useAndFiltering = value);
            preferredHeightSlider = new Slider("预览高度", () => JxDebug.Setting.preferredHeight * Screen.height,
                value => JxDebug.Setting.preferredHeight = value / Screen.height);
            preferredHeightSlider.decimalsToShow = 2;
            scaleSlider = new Slider("缩放", () => JxDebug.Setting.scale, value => JxDebug.Setting.scale = value);
            scaleSlider.decimalsToShow = 2;
            scaleSlider.delayed = true;
            fontSizeSlider = new Slider("字体大小", () => JxDebug.Setting.fontSize,
                value => JxDebug.Setting.fontSize = (int) value);


            monoSizeSlider = new SliderHUV("MonoMemoryUsage", () => JxDebug.Singleton.MonoMemorySize, null);
            memorySizeSlider = new SliderHUV("MemoryUsage", () => JxDebug.Singleton.MemorySize, null);

            heightSeparation = showTimeStampToggle.height;
        }

        public void Draw( float positionY, float height )
        {
            var viewRect = new Rect(0, positionY, Screen.width / JxDebug.Setting.scale, height);
            var contentsRect = new Rect(viewRect.x, viewRect.y, viewRect.width, panelHeight);
            GUIUtils.DrawBox(viewRect, JxDebug.Setting.mainColor);
            scrollView.Draw(viewRect, contentsRect, DrawContents);
            if (!scrollView.isScrollbarVisible)
            {
                scrollView.ScrollToTop();
            }
        }

        private void DrawContents( Rect rect, Vector2 scrollPosition )
        {
            //float rectHeight = rect.height;
            rect.y = 0;
            DrawTitle(ref rect);
            rect.y += rect.height;
            rect.x = padding;
            rect.width -= padding * 2;
            //rect.width -= spacing.x * 2;
            var elementWidth = rect.width / 3;
            var elementWidth1 = rect.width / 2;
            rect.width = elementWidth;
            rect.height = 32;
            DrawShowTimeStamp(rect);
            rect.x += rect.width + spacing.x;
            DrawUseAndFiltering(rect);
            rect.x += rect.width + spacing.x;
            //绘图首选高度
            rect.height = rect.height + 20;

            rect.x = padding;
            rect.y += heightSeparation + spacing.y;
            DrawScale(rect);
            rect.x += rect.width + spacing.x;
            DrawFontSize(rect);
            rect.x += rect.width + spacing.x;
            DrawPreferredHeight(rect);
            //todo:显示系统信息情况
            rect.x = padding;
            rect.y += heightSeparation * 3 + spacing.y;
            rect.height = 32;
            GUIUtils.DrawBox(rect, GUIUtils.lightGrayColor);

            GUI.Label(rect, FpsContent, GUIUtils.InputStyle);
            rect.x += spacing.x * 3;
            GUI.Label(rect, new GUIContent(string.Format("{0:F2}", JxDebug.Singleton.Fps)), GUIUtils.InputStyle);
            rect.x = padding;
            rect.y += heightSeparation + spacing.y;
            rect.width = elementWidth1;
            rect.height = rect.height + 20;
            DrawMonoUsageSize(rect);
            rect.y += heightSeparation * 2 + spacing.y;
            DrawMemoryUsageSize(rect);

            panelHeight = rect.y + heightSeparation * 4;
        }

        private void DrawTitle( ref Rect rect )
        {
            rect.height = 35;
            GUIUtils.DrawBox(rect, GUIUtils.darkerGrayColor);
            GUI.Label(rect, titleContent, GUIUtils.CenteredTextStyle);
        }

        private void DrawShowTimeStamp( Rect rect )
        {
            showTimeStampToggle.Draw(rect);
        }

        private void DrawUseAndFiltering( Rect rect )
        {
            useAndFilteringToggle.Draw(rect);
        }

        private void DrawPreferredHeight( Rect rect )
        {
            preferredHeightSlider.Draw(rect, 0, Screen.height);
        }

        private void DrawScale( Rect rect )
        {
            scaleSlider.Draw(rect, 0.5f, 3);
        }

        private void DrawFontSize( Rect rect )
        {
            fontSizeSlider.Draw(rect, 0, 30);
        }

        private void DrawMonoUsageSize( Rect rect )
        {
            monoSizeSlider.Draw(rect, JxDebug.Singleton.MonoMemorySize, JxDebug.Singleton.MonoMemoryMaxSize);
        }

        private void DrawMemoryUsageSize( Rect rect )
        {
            memorySizeSlider.Draw(rect, JxDebug.Singleton.MemorySize, JxDebug.Singleton.MemoryMaxSize);
        }
    }
}