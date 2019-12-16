/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/8/6 16:13:10                     
*└──────────────────────────────────────────────────────────────┘
*/
using SsitEngine.Unity.HUD;
using UnityEditor;
using UnityEngine;

namespace SsitEngine.Editor.HUD
{
    [CustomEditor(typeof(HudSetting))]
    public class HudSettingEditor : HudElementSettingEditorBase
    {
        #region Variables
        protected HudSetting hudTarget;
        protected GameObject settingsReference;
        #endregion


        #region Main Methods
        protected override void OnEnable()
        {
            base.OnEnable();

            editorTitle = "HUD Navigation Settings";
            splashTexture = (Texture2D)Resources.Load("Textures/splashTexture_Settings", typeof(Texture2D));

            hudTarget = (HudSetting)target;
        }


        protected override void OnChildInspectorGUI()
        {
            base.OnChildInspectorGUI();

            // COPY SETTINGS
            EditorGUILayout.BeginVertical(boxStyle);
            GUILayout.Space(4); // SPACE
            settingsReference = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Copy From", "Assign the GameObject from which you want to extract the settings."), settingsReference, typeof(GameObject), true);
            if (settingsReference != null)
            {
                GUILayout.Space(4); // SPACE
                HudElement element = settingsReference.GetComponent<HudElement>();
                if (element != null)
                {
                    // show paste button
                    if (GUILayout.Button("Copy Settings", GUILayout.Height(18)))
                        hudTarget.CopySettings(element);
                }
                else
                {
                    // show error message
                    EditorGUILayout.HelpBox("No HUDNavigationElement component found on GameObject.", MessageType.Error);
                }
            }
            GUILayout.Space(4); // SPACE
            EditorGUILayout.EndVertical();

            GUILayout.Space(8); // SPACE
        }


        protected override void OnChildEndInspectorGUI()
        {
            base.OnChildEndInspectorGUI();
        }
        #endregion
    }
}
