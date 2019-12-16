using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;

namespace SsitEngine.Editor
{
    public static class GUIUtils
    {

        private static GUIStyle welcomeScreenTextHeaderGUIStyle = null;
        private static GUIStyle welcomeScreenTextDescriptionGUIStyle = null;
        private static GUIStyle preferencesPaneGUIStyle = null;
        private static GUIStyle graphBackgroundGUIStyle = null;
        private static GUIStyle selectionGUIStyle = (GUIStyle)null;
        private static GUIStyle toolbarButtonSelectionGUIStyle = null;
        private static GUIStyle labelWrapGUIStyle = null;
        private static GUIStyle propertyBoxGUIStyle = null;
        [NonSerialized]
        private static Dictionary<System.Type, Dictionary<System.Reflection.FieldInfo, bool>> attributeFieldCache = new Dictionary<System.Type, Dictionary<System.Reflection.FieldInfo, bool>>();
        private static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
        private static Dictionary<string, Texture2D> iconCache = new Dictionary<string, Texture2D>();

        //Texture
        private static Texture2D screenshotBackgroundTexture = null;


        //Graghic
        private static GUIStyle taskTitleGUIStyle = null;


        public static GUIStyle TextHeaderGUIStyle
        {
            get
            {
                if (GUIUtils.welcomeScreenTextHeaderGUIStyle == null)
                    GUIUtils.InitTextHeaderGUIStyle();
                return GUIUtils.welcomeScreenTextHeaderGUIStyle;
            }
        }
        private static void InitTextHeaderGUIStyle()
        {
            GUIUtils.welcomeScreenTextHeaderGUIStyle = new GUIStyle(GUI.skin.label);
            GUIUtils.welcomeScreenTextHeaderGUIStyle.alignment = TextAnchor.MiddleLeft;
            GUIUtils.welcomeScreenTextHeaderGUIStyle.fontSize = 14;
            GUIUtils.welcomeScreenTextHeaderGUIStyle.fontStyle = FontStyle.Bold;
        }
        public static GUIStyle TextDescriptionGUIStyle
        {
            get
            {
                if (GUIUtils.welcomeScreenTextDescriptionGUIStyle == null)
                    GUIUtils.InitWelcomeScreenTextDescriptionGUIStyle();
                return GUIUtils.welcomeScreenTextDescriptionGUIStyle;
            }
        }
        private static void InitWelcomeScreenTextDescriptionGUIStyle()
        {
            GUIUtils.welcomeScreenTextDescriptionGUIStyle = new GUIStyle(GUI.skin.label);
            GUIUtils.welcomeScreenTextDescriptionGUIStyle.wordWrap = true;
        }

        public static GUIStyle PreferencesPaneGUIStyle
        {
            get
            {
                if (GUIUtils.preferencesPaneGUIStyle == null)
                    GUIUtils.InitPreferencesPaneGUIStyle();
                return GUIUtils.preferencesPaneGUIStyle;
            }
        }
        private static void InitPreferencesPaneGUIStyle()
        {
            GUIUtils.preferencesPaneGUIStyle = new GUIStyle(GUI.skin.box);
            GUIUtils.preferencesPaneGUIStyle.normal.background = EditorStyles.toolbarButton.normal.background;
        }

        public static GUIStyle ToolbarButtonSelectionGUIStyle
        {
            get
            {
                if (GUIUtils.toolbarButtonSelectionGUIStyle == null)
                    GUIUtils.InitToolbarButtonSelectionGUIStyle();
                return GUIUtils.toolbarButtonSelectionGUIStyle;
            }
        }

        private static void InitToolbarButtonSelectionGUIStyle()
        {
            GUIUtils.toolbarButtonSelectionGUIStyle = new GUIStyle(EditorStyles.toolbarButton);
            GUIUtils.toolbarButtonSelectionGUIStyle.normal.background = GUIUtils.toolbarButtonSelectionGUIStyle.active.background;
        }

        public static GUIStyle LabelWrapGUIStyle
        {
            get
            {
                if (GUIUtils.labelWrapGUIStyle == null)
                    GUIUtils.InitLabelWrapGUIStyle();
                return GUIUtils.labelWrapGUIStyle;
            }
        }

        private static void InitLabelWrapGUIStyle()
        {
            GUIUtils.labelWrapGUIStyle = new GUIStyle(GUI.skin.label);
            GUIUtils.labelWrapGUIStyle.wordWrap = true;
            GUIUtils.labelWrapGUIStyle.alignment = TextAnchor.MiddleCenter;
        }

        public static GUIStyle PropertyBoxGUIStyle
        {
            get
            {
                if (GUIUtils.propertyBoxGUIStyle == null)
                    GUIUtils.InitPropertyBoxGUIStyle();
                return GUIUtils.propertyBoxGUIStyle;
            }
        }

        public static GUIStyle GraphBackgroundGUIStyle
        {
            get
            {
                if (GUIUtils.graphBackgroundGUIStyle == null)
                    GUIUtils.InitGraphBackgroundGUIStyle();
                return GUIUtils.graphBackgroundGUIStyle;
            }
        }
        private static void InitGraphBackgroundGUIStyle()
        {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            if (EditorGUIUtility.isProSkin)
                texture2D.SetPixel(1, 1, new Color(0.1647f, 0.1647f, 0.1647f));
            else
                texture2D.SetPixel(1, 1, new Color(0.3647f, 0.3647f, 0.3647f));
            texture2D.hideFlags = HideFlags.HideAndDontSave;
            texture2D.Apply();
            GUIUtils.graphBackgroundGUIStyle = new GUIStyle(GUI.skin.box);
            GUIUtils.graphBackgroundGUIStyle.normal.background = texture2D;
            GUIUtils.graphBackgroundGUIStyle.active.background = texture2D;
            GUIUtils.graphBackgroundGUIStyle.hover.background = texture2D;
            GUIUtils.graphBackgroundGUIStyle.focused.background = texture2D;
            GUIUtils.graphBackgroundGUIStyle.normal.textColor = Color.white;
            GUIUtils.graphBackgroundGUIStyle.active.textColor = Color.white;
            GUIUtils.graphBackgroundGUIStyle.hover.textColor = Color.white;
            GUIUtils.graphBackgroundGUIStyle.focused.textColor = Color.white;
        }
        public static GUIStyle SelectionGUIStyle
        {
            get
            {
                if (GUIUtils.selectionGUIStyle == null)
                    GUIUtils.InitSelectionGUIStyle();
                return GUIUtils.selectionGUIStyle;
            }
        }

        private static void InitSelectionGUIStyle()
        {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            Color color = !EditorGUIUtility.isProSkin ? new Color(0.243f, 0.5686f, 0.839f, 0.5f) : new Color(0.188f, 0.4588f, 0.6862f, 0.5f);
            texture2D.SetPixel(1, 1, color);
            texture2D.hideFlags = HideFlags.HideAndDontSave;
            texture2D.Apply();
            GUIUtils.selectionGUIStyle = new GUIStyle(GUI.skin.box);
            GUIUtils.selectionGUIStyle.normal.background = texture2D;
            GUIUtils.selectionGUIStyle.active.background = texture2D;
            GUIUtils.selectionGUIStyle.hover.background = texture2D;
            GUIUtils.selectionGUIStyle.focused.background = texture2D;
            GUIUtils.selectionGUIStyle.normal.textColor = Color.white;
            GUIUtils.selectionGUIStyle.active.textColor = Color.white;
            GUIUtils.selectionGUIStyle.hover.textColor = Color.white;
            GUIUtils.selectionGUIStyle.focused.textColor = Color.white;
        }

        private static void InitPropertyBoxGUIStyle()
        {
            GUIUtils.propertyBoxGUIStyle = new GUIStyle();
            GUIUtils.propertyBoxGUIStyle.padding = new RectOffset(2, 2, 0, 0);
        }

        public static Texture2D ScreenshotBackgroundTexture
        {
            get
            {
                if (GUIUtils.screenshotBackgroundTexture == null)
                    GUIUtils.InitScreenshotBackgroundTexture();
                return GUIUtils.screenshotBackgroundTexture;
            }
        }


        private static void InitScreenshotBackgroundTexture()
        {
            screenshotBackgroundTexture = new Texture2D(1, 1, TextureFormat.RGB24, false, true);
            if (EditorGUIUtility.isProSkin)
                screenshotBackgroundTexture.SetPixel(1, 1, new Color(0.1647f, 0.1647f, 0.1647f));
            else
                screenshotBackgroundTexture.SetPixel(1, 1, new Color(0.3647f, 0.3647f, 0.3647f));
            screenshotBackgroundTexture.Apply();
        }


        public static Texture2D LoadTexture(string imageName, bool useSkinColor = true, UnityEngine.Object obj = null)
        {
            if (textureCache.ContainsKey(imageName))
                return textureCache[imageName];
            Texture2D texture2D = null;
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("{0}{1}", !useSkinColor ? string.Empty : (!EditorGUIUtility.isProSkin ? "Light" : "Dark"), imageName))
                ?? Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("SsitEngine.Editor.Res.{0}{1}", !useSkinColor ? (object)string.Empty : (!EditorGUIUtility.isProSkin ? "Light" : "Dark"), imageName));
            if (stream != null)
            {
                texture2D = new Texture2D(0, 0, TextureFormat.RGBA32, false, true);
                texture2D.LoadImage(ReadToEnd(stream));
                stream.Close();
            }
            if (texture2D != null)
                texture2D.hideFlags = HideFlags.HideAndDontSave;
            else
                Debug.LogWarning("图标加载错误");                
            GUIUtils.textureCache.Add(imageName, texture2D);
            return texture2D;
        }

        //public static string LoadFile( string imageName, bool useSkinColor = true, UnityEngine.Object obj = null )
        //{
        //    Texture2D texture2D = (Texture2D)null;
        //    Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("{0}{1}", !useSkinColor ? string.Empty : (!EditorGUIUtility.isProSkin ? "Light" : "Dark"), imageName))
        //                    ?? Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("Res.{0}{1}", !useSkinColor ? (object)string.Empty : (!EditorGUIUtility.isProSkin ? "Light" : "Dark"), imageName));

        //    if (stream != null)
        //    {
        //        texture2D = new Texture2D(0, 0, TextureFormat.RGBA32, false, true);
        //        texture2D.LoadImage(GuiUtility.ReadToEnd(stream));
        //        stream.Close();
        //    }
        //    if (texture2D != null)
        //        texture2D.hideFlags = HideFlags.HideAndDontSave;
        //    else
        //        Debug.LogWarning("图标加载错误");
        //    GuiUtility.textureCache.Add(imageName, texture2D);
        //    return texture2D;
        //}

        public static Texture2D LoadIcon(string iconName, ScriptableObject obj = null)
        {
            if (GUIUtils.iconCache.ContainsKey(iconName))
                return GUIUtils.iconCache[iconName];
            Texture2D texture2D = (Texture2D)null;
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(iconName.Replace("{SkinColor}", !EditorGUIUtility.isProSkin ? "Light" : "Dark"))
                ?? Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("SimpleGameUtility.Resources.{0}", iconName.Replace("{SkinColor}", !EditorGUIUtility.isProSkin ? "Light" : "Dark")));
            if (stream != null)
            {
                texture2D = new Texture2D(0, 0, TextureFormat.RGBA32, false, true);
                texture2D.LoadImage(GUIUtils.ReadToEnd(stream));
                stream.Close();
            }
            if (texture2D == null)
                texture2D = AssetDatabase.LoadAssetAtPath(iconName.Replace("{SkinColor}", !EditorGUIUtility.isProSkin ? "Light" : "Dark"), typeof(Texture2D)) as Texture2D;
            if (texture2D != null)
                texture2D.hideFlags = HideFlags.HideAndDontSave;
            GUIUtils.iconCache.Add(iconName, texture2D);
            return texture2D;
        }

        public static Texture2D LoadEditorIcon( string iconName, ScriptableObject obj = null )
        {
            if (GUIUtils.iconCache.ContainsKey(iconName))
                return GUIUtils.iconCache[iconName];
            Texture2D texture2D = EditorResources.Load<Texture2D>("Assets/Editor/Gizmos/" + iconName);
            GUIUtils.iconCache.Add(iconName, texture2D);
            return texture2D;
        }

        private static byte[] ReadToEnd(Stream stream)
        {
            byte[] buffer = new byte[16384];
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int count;
                while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                    memoryStream.Write(buffer, 0, count);
                return memoryStream.ToArray();
            }
        }


        private static void InitTaskTitleGUIStyle()
        {
            GUIUtils.taskTitleGUIStyle = new GUIStyle(GUI.skin.label);
            GUIUtils.taskTitleGUIStyle.alignment = TextAnchor.UpperCenter;
            GUIUtils.taskTitleGUIStyle.fontSize = 12;
            GUIUtils.taskTitleGUIStyle.fontStyle = FontStyle.Normal;
        }
        public static GUIStyle TaskTitleGUIStyle
        {
            get
            {
                if (GUIUtils.taskTitleGUIStyle == null)
                    GUIUtils.InitTaskTitleGUIStyle();
                return GUIUtils.taskTitleGUIStyle;
            }
        }
    }
}
