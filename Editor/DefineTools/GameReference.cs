using System;
using UnityEditor;

namespace SsitEngine.Editor
{
    public enum BDPreferences
    {
        ShowWelcomeScreen = 0,
        UserName = 1
    }

    public class GameReference
    {
        private static string[] prefString;

        private static string[] PrefString
        {
            get
            {
                if (prefString == null)
                {
                    InitPrefString();
                }
                return prefString;
            }
        }

        private static void InitPrefString()
        {
            prefString = new string[2];
            for (int index = 0; index < prefString.Length; ++index)
            {
                prefString[index] = $"SsitEngine{index}";
            }
        }

        public static void InitPrefernces()
        {
            if (!EditorPrefs.HasKey(PrefString[0]))
                SetBool(BDPreferences.ShowWelcomeScreen, true);
            if (!EditorPrefs.HasKey(PrefString[1]))
                SetString(BDPreferences.UserName, Environment.UserName);
        }

        public static void SetBool( BDPreferences pref, bool value )
        {
            EditorPrefs.SetBool(PrefString[(int) pref], value);
        }

        public static bool GetBool( BDPreferences pref )
        {
            return EditorPrefs.GetBool(PrefString[(int) pref]);
        }

        public static void SetInt( BDPreferences pref, int value )
        {
            EditorPrefs.SetInt(PrefString[(int) pref], value);
        }

        public static int GetInt( BDPreferences pref )
        {
            return EditorPrefs.GetInt(PrefString[(int) pref]);
        }
        
        public static void SetString( BDPreferences pref, string value )
        {
            EditorPrefs.SetString(PrefString[(int) pref], value);
        }

        public static string GetString( BDPreferences pref,string defaultStr )
        {
            return EditorPrefs.GetString(PrefString[(int) pref], defaultStr);
        }
        
        public static string GetString( BDPreferences pref)
        {
            return EditorPrefs.GetString(PrefString[(int) pref]);
        }

        private static void ResetPrefs()
        {
            SetBool(BDPreferences.ShowWelcomeScreen, true);
            SetString( BDPreferences.UserName, String.Empty );
        }
    }
}