using UnityEngine;

namespace JxDebug
{
    public class Settings : ScriptableObject
    {
        public float animationDuration;
        public AnimationCurve animationX;
        public AnimationCurve animationY;

        public Texture2D autoCompleteIcon;
        //public KeyCode autoCompleteKey;
        //public KeyCode historyKey;

        public bool autoInstantiate;
        public bool clearInputOnClose;
        public Texture2D clearLogIcon;
        public Texture2D closeConsoleIcon;

        public Texture2D closeWindowIcon;

        //public bool enableHistory;
        //public bool autoCompleteWithEnter;
        public string defaultTag;
        public Texture2D deleteInputIcon;
        public bool dontDestroyOnLoad;
        public float entriesSpacing;
        public Texture2D entryCollapsedIcon;
        public Color entryDefaultColor;
        public Texture2D entryExpandedIcon;
        public Color errorColor;

        public Texture2D errorIcon;
        public Font font;
        public int fontSize;
        public Texture2D groupEntriesIcon;
        public bool groupIdenticalEntries;
        public Texture2D historyIcon;
        public Color inputTextColor;

        public Color mainColor;
        public float maxButtonSize;
        public Texture2D openConsoleIcon;

        //[EnumFlags(displayOptions = new string[] { "Log", "Warning", "Error", "Exception", "Assert" })]
        //public LogLevel attachedLogLevel;
        public KeyCode openKey;
        public bool optimizeForOnGUI;

        [Range(0, 1)] public float preferredHeight;

        public Texture2D removeEntryIcon;
        public float scale;
        public Texture2D screenShotIcon;
        public Texture2D scrollDownIcon;
        public Texture2D scrollUpIcon;
        public Color secondaryColor;

        [SerializeField] private int selectedTab;

        public Texture2D settingsIcon;
        public bool showGUIButton;
        public bool showOpenButton;
        public bool showTimeStamp;
        public Texture2D sliderIcon;
        public Texture2D sliderThumbIcon;
        public Texture2D stackTraceIcon;
        public Texture2D submitInputIcon;
        public Texture2D toggleOffIcon;
        public Texture2D toggleOnIcon;
        public bool useAndFiltering;
        public Color warningColor;
        public Texture2D warningIcon;

        public void CopyFrom( Settings settings )
        {
            var fields = GetType().GetFields();
            for (var i = 0; i < fields.Length; i++)
            {
                fields[i].SetValue(this, fields[i].GetValue(settings));
            }
        }
    }

    //public enum LogLevel {
    //    None = 0,
    //    Log = 1,
    //    Warning = 2,
    //    Error = 4,
    //    Exception = 8,
    //    Assert = 16
    //}
}