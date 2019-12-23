using UnityEngine;

namespace JxDebug
{
    public class Toggle
    {
        public delegate bool ToggleGetter();

        public delegate void ToggleSetter( bool value );

        private const float size = 32;

        private static GUIStyle toggleStyle;
        private readonly ToggleGetter getter;

        private readonly GUIContent label;
        private readonly ToggleSetter setter;

        public Toggle( string label, ToggleGetter getter, ToggleSetter setter )
        {
            this.label = new GUIContent(label);
            this.getter = getter;
            this.setter = setter;
        }

        public bool value { get; private set; }
        public float height => size;

        public static void Initialize()
        {
            toggleStyle = new GUIStyle(GUI.skin.toggle);
            toggleStyle.onActive = toggleStyle.onFocused = toggleStyle.onHover = toggleStyle.onNormal =
                new GUIStyleState {background = JxDebug.Setting.toggleOnIcon, textColor = GUIUtils.slightlyGrayColor};
            toggleStyle.active = toggleStyle.focused = toggleStyle.hover = toggleStyle.normal =
                new GUIStyleState {background = JxDebug.Setting.toggleOffIcon, textColor = GUIUtils.slightlyGrayColor};
            toggleStyle.fixedWidth = size;
            toggleStyle.fixedHeight = size;
            toggleStyle.clipping = TextClipping.Clip;
            toggleStyle.contentOffset = new Vector2(5, 0);
            toggleStyle.alignment = TextAnchor.MiddleLeft;
        }

        public void Draw( Rect rect )
        {
            setter(GUI.Toggle(rect, getter(), GUIContent.none, toggleStyle));
            rect.x += toggleStyle.contentOffset.x + size;
            rect.width -= toggleStyle.contentOffset.x + size;
            rect.height = size;

            var style = new GUIStyle();
            style.fontSize = 24;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleLeft;
            GUI.Label(rect, label, style);
        }
    }
}