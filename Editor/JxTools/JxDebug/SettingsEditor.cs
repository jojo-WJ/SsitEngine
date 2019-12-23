using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JxDebug
{
    [CustomEditor(typeof(Settings))]
    public class SettingsEditor : Editor
    {
        private SerializedProperty selectedTab;

        private readonly GUIContent[] tabsContents =
        {
            new GUIContent("Behaviour"),
            new GUIContent("Look & Feel"),
            new GUIContent("Icons")
        };

        private void OnEnable()
        {
            selectedTab = serializedObject.FindProperty("selectedTab");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawScript();
            DrawTabs();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawScript()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            GUI.enabled = true;
        }

        private void DrawTabs()
        {
            selectedTab.intValue = GUILayout.Toolbar(selectedTab.intValue, tabsContents);
            switch (selectedTab.intValue)
            {
                case 0:
                    DrawBehaviour();
                    break;
                case 1:
                    DrawLookAndFeel();
                    break;
                case 2:
                    DrawIcons();
                    break;
                case 3:
                    GUI.enabled = !EditorApplication.isPlaying;
                    DrawBuiltInCommands();
                    GUI.enabled = true;
                    break;
            }
        }

        private void DrawBehaviour()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showGUIButton"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showOpenButton"));

            //EditorGUILayout.PropertyField(serializedObject.FindProperty("attachedLogLevel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("openKey"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("autoCompleteKey"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("historyKey"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoInstantiate"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("dontDestroyOnLoad"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showTimeStamp"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("groupIdenticalEntries"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useAndFiltering"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("optimizeForOnGUI"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clearInputOnClose"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("enableHistory"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("autoCompleteWithEnter"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("autoCompleteBehaviour"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultTag"));
        }

        private void DrawLookAndFeel()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("preferredHeight"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showOpenButton"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("font"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fontSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxButtonSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("entriesSpacing"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mainColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("secondaryColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("inputTextColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("entryDefaultColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("warningColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("errorColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animationDuration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animationX"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("animationY"));
        }

        private void DrawIcons()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("errorIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("warningIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("closeConsoleIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("openConsoleIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("deleteInputIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("submitInputIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("historyIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autoCompleteIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("entryExpandedIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("entryCollapsedIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stackTraceIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("removeEntryIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("closeWindowIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scrollUpIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scrollDownIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clearLogIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("screenShotIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("groupEntriesIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("settingsIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("toggleOnIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("toggleOffIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sliderIcon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sliderThumbIcon"));
        }

        private void DrawBuiltInCommands()
        {
            var currentCommand = serializedObject.FindProperty("builtInCommands");
            var commands = new SerializedProperty[currentCommand.Copy().CountRemaining()];
            for (var i = 0; currentCommand.Next(true); i++)
            {
                commands[i] = currentCommand.Copy();
            }
            commands = commands.OrderBy(x => x.name).ToArray();

            var firstColumnCount = commands.Length / 2;
            SerializedProperty commandToggled = null;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            for (var i = 0; i < commands.Length; i++)
            {
                if (i == firstColumnCount + 1)
                {
                    EditorGUILayout.BeginVertical();
                }
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(commands[i], true);
                if (EditorGUI.EndChangeCheck())
                {
                    commandToggled = commands[i];
                }
                if (i == firstColumnCount)
                {
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            ProcessCommandToggled(commandToggled);
        }

        private void ProcessCommandToggled( SerializedProperty command )
        {
            if (command == null)
            {
                return;
            }
            switch (command.name)
            {
                case "microphone":
                    AddOrRemoveDefineSymbol("COMMAND_SYSTEM_USE_MICROPHONE", BuildTargetGroup.Android,
                        command.boolValue);
                    break;
                case "locationService":
                    AddOrRemoveDefineSymbol("COMMAND_SYSTEM_USE_LOCATION", BuildTargetGroup.Android, command.boolValue);
                    break;
            }
        }

        private void AddOrRemoveDefineSymbol( string define, BuildTargetGroup group, bool add )
        {
            if (add)
            {
                DefineSymbolsManager.AddDefine(define, group);
            }
            else
            {
                DefineSymbolsManager.RemoveDefine(define, group);
            }
        }
    }
}