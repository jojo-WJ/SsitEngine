using UnityEngine;
using JxDebug;
using System.Reflection;

namespace JxDebug {
    public class Settings : ScriptableObject {
        [SerializeField]
        int selectedTab;

        //[EnumFlags(displayOptions = new string[] { "Log", "Warning", "Error", "Exception", "Assert" })]
        //public LogLevel attachedLogLevel;
        public KeyCode openKey;
        //public KeyCode autoCompleteKey;
        //public KeyCode historyKey;

        public bool autoInstantiate;
        public bool dontDestroyOnLoad;
        public bool showTimeStamp;
        public bool optimizeForOnGUI;
        public bool groupIdenticalEntries;
        public bool useAndFiltering;
        public bool clearInputOnClose;
        //public bool enableHistory;
        //public bool autoCompleteWithEnter;
        public string defaultTag;

        [Range(0, 1)]
        public float preferredHeight;
        public bool showGUIButton;
        public bool showOpenButton;
        public float scale;
        public Font font;
        public int fontSize;
        public float entriesSpacing;
        public float maxButtonSize;

        public Color mainColor;
        public Color secondaryColor;
        public Color inputTextColor;
        public Color entryDefaultColor;
        public Color warningColor;
        public Color errorColor;

        public float animationDuration;
        public AnimationCurve animationY;
        public AnimationCurve animationX;

        public Texture2D errorIcon;
        public Texture2D warningIcon;
        public Texture2D closeConsoleIcon;
        public Texture2D openConsoleIcon;
        public Texture2D deleteInputIcon;
        public Texture2D submitInputIcon;
        public Texture2D historyIcon;
        public Texture2D autoCompleteIcon;
        public Texture2D entryExpandedIcon;
        public Texture2D entryCollapsedIcon;
        public Texture2D stackTraceIcon;
        public Texture2D removeEntryIcon;
        public Texture2D closeWindowIcon;
        public Texture2D scrollUpIcon;
        public Texture2D scrollDownIcon;
        public Texture2D clearLogIcon;
        public Texture2D screenShotIcon;
        public Texture2D groupEntriesIcon;
        public Texture2D settingsIcon;
        public Texture2D toggleOnIcon;
        public Texture2D toggleOffIcon;
        public Texture2D sliderIcon;
        public Texture2D sliderThumbIcon;

        public void CopyFrom(Settings settings) {
            FieldInfo[] fields = GetType().GetFields();
            for(int i = 0; i < fields.Length; i++)
                fields[i].SetValue(this, fields[i].GetValue(settings));
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