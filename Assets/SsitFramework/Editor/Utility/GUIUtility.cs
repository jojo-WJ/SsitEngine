using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SsitEngine.Editor
{
    public static class GUIUtility
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
                if (GUIUtility.welcomeScreenTextHeaderGUIStyle == null)
                    GUIUtility.InitTextHeaderGUIStyle();
                return GUIUtility.welcomeScreenTextHeaderGUIStyle;
            }
        }
        private static void InitTextHeaderGUIStyle()
        {
            GUIUtility.welcomeScreenTextHeaderGUIStyle = new GUIStyle(GUI.skin.label);
            GUIUtility.welcomeScreenTextHeaderGUIStyle.alignment = TextAnchor.MiddleLeft;
            GUIUtility.welcomeScreenTextHeaderGUIStyle.fontSize = 14;
            GUIUtility.welcomeScreenTextHeaderGUIStyle.fontStyle = FontStyle.Bold;
        }
        public static GUIStyle TextDescriptionGUIStyle
        {
            get
            {
                if (GUIUtility.welcomeScreenTextDescriptionGUIStyle == null)
                    GUIUtility.InitWelcomeScreenTextDescriptionGUIStyle();
                return GUIUtility.welcomeScreenTextDescriptionGUIStyle;
            }
        }
        private static void InitWelcomeScreenTextDescriptionGUIStyle()
        {
            GUIUtility.welcomeScreenTextDescriptionGUIStyle = new GUIStyle(GUI.skin.label);
            GUIUtility.welcomeScreenTextDescriptionGUIStyle.wordWrap = true;
        }

        public static GUIStyle PreferencesPaneGUIStyle
        {
            get
            {
                if (GUIUtility.preferencesPaneGUIStyle == null)
                    GUIUtility.InitPreferencesPaneGUIStyle();
                return GUIUtility.preferencesPaneGUIStyle;
            }
        }
        private static void InitPreferencesPaneGUIStyle()
        {
            GUIUtility.preferencesPaneGUIStyle = new GUIStyle(GUI.skin.box);
            GUIUtility.preferencesPaneGUIStyle.normal.background = EditorStyles.toolbarButton.normal.background;
        }

        public static GUIStyle ToolbarButtonSelectionGUIStyle
        {
            get
            {
                if (GUIUtility.toolbarButtonSelectionGUIStyle == null)
                    GUIUtility.InitToolbarButtonSelectionGUIStyle();
                return GUIUtility.toolbarButtonSelectionGUIStyle;
            }
        }

        private static void InitToolbarButtonSelectionGUIStyle()
        {
            GUIUtility.toolbarButtonSelectionGUIStyle = new GUIStyle(EditorStyles.toolbarButton);
            GUIUtility.toolbarButtonSelectionGUIStyle.normal.background = GUIUtility.toolbarButtonSelectionGUIStyle.active.background;
        }

        public static GUIStyle LabelWrapGUIStyle
        {
            get
            {
                if (GUIUtility.labelWrapGUIStyle == null)
                    GUIUtility.InitLabelWrapGUIStyle();
                return GUIUtility.labelWrapGUIStyle;
            }
        }

        private static void InitLabelWrapGUIStyle()
        {
            GUIUtility.labelWrapGUIStyle = new GUIStyle(GUI.skin.label);
            GUIUtility.labelWrapGUIStyle.wordWrap = true;
            GUIUtility.labelWrapGUIStyle.alignment = TextAnchor.MiddleCenter;
        }

        public static GUIStyle PropertyBoxGUIStyle
        {
            get
            {
                if (GUIUtility.propertyBoxGUIStyle == null)
                    GUIUtility.InitPropertyBoxGUIStyle();
                return GUIUtility.propertyBoxGUIStyle;
            }
        }

        public static GUIStyle GraphBackgroundGUIStyle
        {
            get
            {
                if (GUIUtility.graphBackgroundGUIStyle == null)
                    GUIUtility.InitGraphBackgroundGUIStyle();
                return GUIUtility.graphBackgroundGUIStyle;
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
            GUIUtility.graphBackgroundGUIStyle = new GUIStyle(GUI.skin.box);
            GUIUtility.graphBackgroundGUIStyle.normal.background = texture2D;
            GUIUtility.graphBackgroundGUIStyle.active.background = texture2D;
            GUIUtility.graphBackgroundGUIStyle.hover.background = texture2D;
            GUIUtility.graphBackgroundGUIStyle.focused.background = texture2D;
            GUIUtility.graphBackgroundGUIStyle.normal.textColor = Color.white;
            GUIUtility.graphBackgroundGUIStyle.active.textColor = Color.white;
            GUIUtility.graphBackgroundGUIStyle.hover.textColor = Color.white;
            GUIUtility.graphBackgroundGUIStyle.focused.textColor = Color.white;
        }
        public static GUIStyle SelectionGUIStyle
        {
            get
            {
                if (GUIUtility.selectionGUIStyle == null)
                    GUIUtility.InitSelectionGUIStyle();
                return GUIUtility.selectionGUIStyle;
            }
        }

        private static void InitSelectionGUIStyle()
        {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            Color color = !EditorGUIUtility.isProSkin ? new Color(0.243f, 0.5686f, 0.839f, 0.5f) : new Color(0.188f, 0.4588f, 0.6862f, 0.5f);
            texture2D.SetPixel(1, 1, color);
            texture2D.hideFlags = HideFlags.HideAndDontSave;
            texture2D.Apply();
            GUIUtility.selectionGUIStyle = new GUIStyle(GUI.skin.box);
            GUIUtility.selectionGUIStyle.normal.background = texture2D;
            GUIUtility.selectionGUIStyle.active.background = texture2D;
            GUIUtility.selectionGUIStyle.hover.background = texture2D;
            GUIUtility.selectionGUIStyle.focused.background = texture2D;
            GUIUtility.selectionGUIStyle.normal.textColor = Color.white;
            GUIUtility.selectionGUIStyle.active.textColor = Color.white;
            GUIUtility.selectionGUIStyle.hover.textColor = Color.white;
            GUIUtility.selectionGUIStyle.focused.textColor = Color.white;
        }

        private static void InitPropertyBoxGUIStyle()
        {
            GUIUtility.propertyBoxGUIStyle = new GUIStyle();
            GUIUtility.propertyBoxGUIStyle.padding = new RectOffset(2, 2, 0, 0);
        }

        public static Texture2D ScreenshotBackgroundTexture
        {
            get
            {
                if (GUIUtility.screenshotBackgroundTexture == null)
                    GUIUtility.InitScreenshotBackgroundTexture();
                return GUIUtility.screenshotBackgroundTexture;
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
            GUIUtility.textureCache.Add(imageName, texture2D);
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
            if (GUIUtility.iconCache.ContainsKey(iconName))
                return GUIUtility.iconCache[iconName];
            Texture2D texture2D = (Texture2D)null;
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(iconName.Replace("{SkinColor}", !EditorGUIUtility.isProSkin ? "Light" : "Dark"))
                ?? Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("SimpleGameUtility.Resources.{0}", iconName.Replace("{SkinColor}", !EditorGUIUtility.isProSkin ? "Light" : "Dark")));
            if (stream != null)
            {
                texture2D = new Texture2D(0, 0, TextureFormat.RGBA32, false, true);
                texture2D.LoadImage(GUIUtility.ReadToEnd(stream));
                stream.Close();
            }
            if (texture2D == null)
                texture2D = AssetDatabase.LoadAssetAtPath(iconName.Replace("{SkinColor}", !EditorGUIUtility.isProSkin ? "Light" : "Dark"), typeof(Texture2D)) as Texture2D;
            if (texture2D != null)
                texture2D.hideFlags = HideFlags.HideAndDontSave;
            GUIUtility.iconCache.Add(iconName, texture2D);
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
            GUIUtility.taskTitleGUIStyle = new GUIStyle(GUI.skin.label);
            GUIUtility.taskTitleGUIStyle.alignment = TextAnchor.UpperCenter;
            GUIUtility.taskTitleGUIStyle.fontSize = 12;
            GUIUtility.taskTitleGUIStyle.fontStyle = FontStyle.Normal;
        }
        public static GUIStyle TaskTitleGUIStyle
        {
            get
            {
                if (GUIUtility.taskTitleGUIStyle == null)
                    GUIUtility.InitTaskTitleGUIStyle();
                return GUIUtility.taskTitleGUIStyle;
            }
        }
    }
}
