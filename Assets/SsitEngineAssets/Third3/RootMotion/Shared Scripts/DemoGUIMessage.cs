using UnityEngine;

namespace RootMotion
{
    // Just for displaying a GUI text in the Game View.
    public class DemoGUIMessage : MonoBehaviour
    {
        public Color color = Color.white;

        public string text;

        private void OnGUI()
        {
            GUI.color = color;
            GUILayout.Label(text);
            GUI.color = Color.white;
        }
    }
}