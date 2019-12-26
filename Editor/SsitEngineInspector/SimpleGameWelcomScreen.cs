using SsitEngine.Editor;
using UnityEditor;
using UnityEngine;

namespace SimpleGameEditor
{
    public class SimpleGameWelcomScreen : EditorWindow
    {

        private static readonly string URL = "https://github.com/jojo-WJ/SsitEngine.git";
        private static readonly Vector2 PreSize = new Vector2(340f, 410f);

        private Rect mWelcomeScreenImageRect = new Rect(0.0f, 0.0f, 340f, 44f);
        private Rect mWelcomeIntroRect = new Rect(70f, 57f, 250f, 20f);

        private Rect mDocImageRect = new Rect(15f, 124f, 53f, 50f);
        private Rect mDocHeaderRect = new Rect(70f, 123f, 250f, 20f);
        private Rect mDocDescriptionRect = new Rect(70f, 143f, 250f, 30f);

        private Rect mUserHeaderRect = new Rect(70f, 165f, 250f, 20f);
        private Rect mUserInputRect = new Rect(70f, 185f, 250f, 20f);

        private Rect mToggleButtonRect = new Rect(220f, 385f, 125f, 20f);
        private Texture mWelcomeScreenImage;
        
        [MenuItem("Tools/SsitEngine/Welcome Screen", false, 0)]
        public static void ShowWindow()
        {
            SimpleGameWelcomScreen window = GetWindow<SimpleGameWelcomScreen>(true, "Welcome to SsitEngine");
            window.maxSize = PreSize;
            window.minSize = PreSize;
        }
        public void OnEnable()
        {
            GameReference.InitPrefernces();
        }
        
        public void OnGUI()
        {
            //GUI.DrawTexture(this.mWelcomeScreenImageRect, this.mWelcomeScreenImage);
            GUI.Label(this.mWelcomeIntroRect, "Welcome To SsitEngine", GUIUtils.TextHeaderGUIStyle);

            GUI.Label(this.mDocHeaderRect, "Documentation", GUIUtils.TextHeaderGUIStyle);
            GUI.Label(this.mDocDescriptionRect, URL, GUIUtils.TextDescriptionGUIStyle);
            
            GUI.Label(this.mUserHeaderRect, "User", GUIUtils.TextHeaderGUIStyle);
            var  userName = GUI.TextField(this.mUserInputRect, GameReference.GetString(BDPreferences.UserName));

            if (GUI.changed)
            {
                GameReference.SetString(BDPreferences.UserName,userName);
            }
            
            bool flag = GUI.Toggle(this.mToggleButtonRect, GameReference.GetBool(BDPreferences.ShowWelcomeScreen), "Show at Startup");
            if (flag != GameReference.GetBool(BDPreferences.ShowWelcomeScreen))
            {
                GameReference.SetBool(BDPreferences.ShowWelcomeScreen, flag);
            }
            EditorGUIUtility.AddCursorRect(this.mDocDescriptionRect, MouseCursor.Link);
            if (Event.current.type != EventType.MouseUp)
                return;
            Vector2 mousePosition = Event.current.mousePosition;
            if (mDocDescriptionRect.Contains(mousePosition))
            {
                Application.OpenURL(URL);
            }
        }

        /*public void OnDisable()
        {
            
        }*/
    }
}
