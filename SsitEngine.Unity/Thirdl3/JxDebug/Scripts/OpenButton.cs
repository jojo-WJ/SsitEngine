using System;
using UnityEngine;

namespace JxDebug
{
    [Serializable]
    public class OpenButton
    {
        public float height;
        private Rect m_containerRect;
        public float width;

        public Rect ContainerRect => m_containerRect;

        public void Draw( float positionY )
        {
            m_containerRect = new Rect(Screen.width / 2 / JxDebug.Setting.scale - width / 2, positionY, width, height);
            if (GUIUtils.DrawCenteredButton(m_containerRect,
                new GUIContent(JxDebug.Singleton.IsOpen
                    ? JxDebug.Setting.closeConsoleIcon
                    : JxDebug.Setting.openConsoleIcon), JxDebug.Setting.mainColor))
            {
                JxDebug.Singleton.ToggleOpen();
            }
        }
    }
}