using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace JxDebug {
    [Serializable]
    public class Toolbar {
        const float lineHeight = 1;
        const float padding = 2;

        public float height;
        [SerializeField]
        protected float spacing;
        [SerializeField]
        protected float tabWidth;

        GUIContent clearContent;
        GUIContent creenShotContent;
        GUIContent groupContent;
        GUIContent settingsContent;
        List<Tab> tabs;

        public void Initialize(Logger logger) {
            clearContent = new GUIContent(JxDebug.Setting.clearLogIcon);
            groupContent = new GUIContent(JxDebug.Setting.groupEntriesIcon);
            creenShotContent = new GUIContent(JxDebug.Setting.screenShotIcon);
            settingsContent = new GUIContent(JxDebug.Setting.settingsIcon);
            logger.onFilterAdded += OnFilterAdded;
            logger.onFilterRemoved += OnFilterRemoved;
            tabs = new List<Tab>();
            Filter[] filters = logger.GetFilters(); 
            for(int i = 0; i < filters.Length; i++)
                OnFilterAdded(filters[i]);
        }

        void OnFilterAdded(Filter filter) {
            Tab tab = new Tab(filter);
            tabs.Add(tab);
        }

        void OnFilterRemoved(Filter filter) {
            for(int i = tabs.Count-1; i >= 0; i--) {
                if(tabs[i].filter == filter) {
                    tabs.RemoveAt(i);
                    return;
                }
            }
        }

        public void Draw() {
            Rect rect = new Rect(0, 0, Screen.width/ JxDebug.Setting.scale, height-lineHeight);
            GUIUtils.DrawBox(rect, JxDebug.Setting.mainColor);
            DrawLine(rect);
            rect.y += padding;
            rect.height -= padding * 2;
            DrawButtonsPanel(ref rect);
            DrawTabs(ref rect);
        }

        void DrawButtonsPanel(ref Rect rect) {
            float originalWidth = rect.width;

            rect.width = CalculateButtonsPanelWidth(rect);
            rect.x = originalWidth - rect.width;

            DrawGenericButton(ref rect, clearContent, JxDebug.Singleton.ClearLog);
            rect.x += rect.width;
            rect.x += spacing;
            DrawGenericButton(ref rect, creenShotContent, ScreenShot);
            rect.x += rect.width;
            rect.x += spacing;
            Color oldColor = GUI.contentColor;
            if(JxDebug.Setting.groupIdenticalEntries)
                GUI.contentColor = JxDebug.Setting.secondaryColor;
            DrawGenericButton(ref rect, groupContent, ToggleGroupEntries);
            GUI.contentColor = oldColor;
            rect.x += rect.width;
            rect.x += spacing;
            DrawGenericButton(ref rect, settingsContent, JxDebug.Singleton.ToggleSettings);
            rect.x += rect.width;
            rect.x += padding;

            rect.width = rect.x;
            rect.x = 0;
        }

        float CalculateButtonsPanelWidth(Rect rect) {
            float width = 0;
            width += GetClampedButtonSize(rect.height, clearContent.image.width);
            width += GetClampedButtonSize(rect.height, groupContent.image.width);
            width += GetClampedButtonSize(rect.height, creenShotContent.image.width);
            width += GetClampedButtonSize(rect.height, settingsContent.image.width);
            width += spacing * 3;
            width += padding;
            return width;
        }

        float GetClampedButtonSize(float rectHeight, float imageWidth) {
            return Mathf.Min(rectHeight, imageWidth, JxDebug.Setting.maxButtonSize);
        }

        void DrawGenericButton(ref Rect rect, GUIContent content, Action callback) {
            rect.width = GetClampedButtonSize(rect.height, content.image.width);
            if(GUIUtils.DrawCenteredButton(rect, content, Color.clear))
                callback();
        }

        void ToggleGroupEntries() {
            JxDebug.Setting.groupIdenticalEntries = !JxDebug.Setting.groupIdenticalEntries;
        }

        void ScreenShot() {
            //todo:屏幕截图
            JxDebug.Singleton.ScreenShot();
        }

        void DrawLine(Rect rect) {
            rect.y += rect.height;
            rect.height = lineHeight;
            GUIUtils.DrawBox(rect, GUIUtils.blackColor);
        }

        void DrawTabs(ref Rect rect) {
            rect.x += padding;
            rect.width -= padding;
            rect.width = tabWidth;
            for(int i = 0; i < tabs.Count; i++) {
                tabs[i].Draw(rect);
                rect.x += tabWidth + spacing;
            }
        }
    }
}