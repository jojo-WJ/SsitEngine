using System;
using UnityEngine;

namespace JxDebug
{
    /// <summary>
    /// 列表
    /// </summary>
    [Serializable]
    public class Tab
    {
        private static GUIStyle s_style;

        private readonly GUIContent content;

        public Tab( Filter filter )
        {
            this.filter = filter;
            content = new GUIContent(filter.Tag);
        }

        public Filter filter { get; private set; }

        public static void Initialize()
        {
            s_style = new GUIStyle(GUIUtils.CenteredButtonStyle);
            s_style.normal.textColor = s_style.hover.textColor = s_style.active.textColor = GUIUtils.slightlyGrayColor;
        }

        public void Draw( Rect rect )
        {
            if (GUIUtils.DrawButton(rect, content, filter.isActive ? GUIUtils.grayerColor : GUIUtils.darkerGrayColor,
                s_style))
            {
                filter.isActive = !filter.isActive;
            }
        }
    }
}