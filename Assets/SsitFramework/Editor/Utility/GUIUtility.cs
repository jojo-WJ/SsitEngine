using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SsitEngine.Editor
{
    public static class GUIUtility
    {
        // Static readonly Fields
        public static readonly string ToStreamingAssets = "SsitEngineAssets/Editor/ToStreamingAssets";
        public static readonly string StreamingAssets = "StreamingAssets";
        // Static Fields Related to locating the TextMesh Pro Asset
        private static string folderPath = "Not Found";
        private static string m_PackageFullPath;
        
        private static GUIStyle welcomeScreenTextHeaderGUIStyle;
        private static GUIStyle welcomeScreenTextDescriptionGUIStyle;
        private static GUIStyle preferencesPaneGUIStyle;
        private static GUIStyle graphBackgroundGUIStyle;
        private static GUIStyle selectionGUIStyle;
        private static GUIStyle toolbarButtonSelectionGUIStyle;
        private static GUIStyle labelWrapGUIStyle;
        private static GUIStyle propertyBoxGUIStyle;

        [NonSerialized] private static Dictionary<Type, Dictionary<FieldInfo, bool>> attributeFieldCache =
            new Dictionary<Type, Dictionary<FieldInfo, bool>>();

        private static readonly Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Texture2D> iconCache = new Dictionary<string, Texture2D>();

        //Texture
        private static Texture2D screenshotBackgroundTexture;


        //Graghic
        private static GUIStyle taskTitleGUIStyle;

        /// <summary>
        /// Returns the fully qualified path of the package.
        /// </summary>
        public static string packageFullPath
        {
            get
            {
                if (string.IsNullOrEmpty(m_PackageFullPath))
                {
                    m_PackageFullPath = GetPackageFullPath();
                }

                return m_PackageFullPath;
            }
        }


        public static GUIStyle TextHeaderGUIStyle
        {
            get
            {
                if (welcomeScreenTextHeaderGUIStyle == null)
                {
                    InitTextHeaderGUIStyle();
                }
                return welcomeScreenTextHeaderGUIStyle;
            }
        }

        public static GUIStyle TextDescriptionGUIStyle
        {
            get
            {
                if (welcomeScreenTextDescriptionGUIStyle == null)
                {
                    InitWelcomeScreenTextDescriptionGUIStyle();
                }
                return welcomeScreenTextDescriptionGUIStyle;
            }
        }

        public static GUIStyle PreferencesPaneGUIStyle
        {
            get
            {
                if (preferencesPaneGUIStyle == null)
                {
                    InitPreferencesPaneGUIStyle();
                }
                return preferencesPaneGUIStyle;
            }
        }

        public static GUIStyle ToolbarButtonSelectionGUIStyle
        {
            get
            {
                if (toolbarButtonSelectionGUIStyle == null)
                {
                    InitToolbarButtonSelectionGUIStyle();
                }
                return toolbarButtonSelectionGUIStyle;
            }
        }

        public static GUIStyle LabelWrapGUIStyle
        {
            get
            {
                if (labelWrapGUIStyle == null)
                {
                    InitLabelWrapGUIStyle();
                }
                return labelWrapGUIStyle;
            }
        }

        public static GUIStyle PropertyBoxGUIStyle
        {
            get
            {
                if (propertyBoxGUIStyle == null)
                {
                    InitPropertyBoxGUIStyle();
                }
                return propertyBoxGUIStyle;
            }
        }

        public static GUIStyle GraphBackgroundGUIStyle
        {
            get
            {
                if (graphBackgroundGUIStyle == null)
                {
                    InitGraphBackgroundGUIStyle();
                }
                return graphBackgroundGUIStyle;
            }
        }

        public static GUIStyle SelectionGUIStyle
        {
            get
            {
                if (selectionGUIStyle == null)
                {
                    InitSelectionGUIStyle();
                }
                return selectionGUIStyle;
            }
        }

        public static Texture2D ScreenshotBackgroundTexture
        {
            get
            {
                if (screenshotBackgroundTexture == null)
                {
                    InitScreenshotBackgroundTexture();
                }
                return screenshotBackgroundTexture;
            }
        }

        public static GUIStyle TaskTitleGUIStyle
        {
            get
            {
                if (taskTitleGUIStyle == null)
                {
                    InitTaskTitleGUIStyle();
                }
                return taskTitleGUIStyle;
            }
        }

        private static void InitTextHeaderGUIStyle()
        {
            welcomeScreenTextHeaderGUIStyle = new GUIStyle(GUI.skin.label);
            welcomeScreenTextHeaderGUIStyle.alignment = TextAnchor.MiddleLeft;
            welcomeScreenTextHeaderGUIStyle.fontSize = 14;
            welcomeScreenTextHeaderGUIStyle.fontStyle = FontStyle.Bold;
        }

        private static void InitWelcomeScreenTextDescriptionGUIStyle()
        {
            welcomeScreenTextDescriptionGUIStyle = new GUIStyle(GUI.skin.label);
            welcomeScreenTextDescriptionGUIStyle.wordWrap = true;
        }

        private static void InitPreferencesPaneGUIStyle()
        {
            preferencesPaneGUIStyle = new GUIStyle(GUI.skin.box);
            preferencesPaneGUIStyle.normal.background = EditorStyles.toolbarButton.normal.background;
        }

        private static void InitToolbarButtonSelectionGUIStyle()
        {
            toolbarButtonSelectionGUIStyle = new GUIStyle(EditorStyles.toolbarButton);
            toolbarButtonSelectionGUIStyle.normal.background =
                toolbarButtonSelectionGUIStyle.active.background;
        }

        private static void InitLabelWrapGUIStyle()
        {
            labelWrapGUIStyle = new GUIStyle(GUI.skin.label);
            labelWrapGUIStyle.wordWrap = true;
            labelWrapGUIStyle.alignment = TextAnchor.MiddleCenter;
        }

        private static void InitGraphBackgroundGUIStyle()
        {
            var texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            if (EditorGUIUtility.isProSkin)
            {
                texture2D.SetPixel(1, 1, new Color(0.1647f, 0.1647f, 0.1647f));
            }
            else
            {
                texture2D.SetPixel(1, 1, new Color(0.3647f, 0.3647f, 0.3647f));
            }
            texture2D.hideFlags = HideFlags.HideAndDontSave;
            texture2D.Apply();
            graphBackgroundGUIStyle = new GUIStyle(GUI.skin.box);
            graphBackgroundGUIStyle.normal.background = texture2D;
            graphBackgroundGUIStyle.active.background = texture2D;
            graphBackgroundGUIStyle.hover.background = texture2D;
            graphBackgroundGUIStyle.focused.background = texture2D;
            graphBackgroundGUIStyle.normal.textColor = Color.white;
            graphBackgroundGUIStyle.active.textColor = Color.white;
            graphBackgroundGUIStyle.hover.textColor = Color.white;
            graphBackgroundGUIStyle.focused.textColor = Color.white;
        }

        private static void InitSelectionGUIStyle()
        {
            var texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            var color = !EditorGUIUtility.isProSkin
                ? new Color(0.243f, 0.5686f, 0.839f, 0.5f)
                : new Color(0.188f, 0.4588f, 0.6862f, 0.5f);
            texture2D.SetPixel(1, 1, color);
            texture2D.hideFlags = HideFlags.HideAndDontSave;
            texture2D.Apply();
            selectionGUIStyle = new GUIStyle(GUI.skin.box);
            selectionGUIStyle.normal.background = texture2D;
            selectionGUIStyle.active.background = texture2D;
            selectionGUIStyle.hover.background = texture2D;
            selectionGUIStyle.focused.background = texture2D;
            selectionGUIStyle.normal.textColor = Color.white;
            selectionGUIStyle.active.textColor = Color.white;
            selectionGUIStyle.hover.textColor = Color.white;
            selectionGUIStyle.focused.textColor = Color.white;
        }

        private static void InitPropertyBoxGUIStyle()
        {
            propertyBoxGUIStyle = new GUIStyle();
            propertyBoxGUIStyle.padding = new RectOffset(2, 2, 0, 0);
        }


        private static void InitScreenshotBackgroundTexture()
        {
            screenshotBackgroundTexture = new Texture2D(1, 1, TextureFormat.RGB24, false, true);
            if (EditorGUIUtility.isProSkin)
            {
                screenshotBackgroundTexture.SetPixel(1, 1, new Color(0.1647f, 0.1647f, 0.1647f));
            }
            else
            {
                screenshotBackgroundTexture.SetPixel(1, 1, new Color(0.3647f, 0.3647f, 0.3647f));
            }
            screenshotBackgroundTexture.Apply();
        }


        public static Texture2D LoadTexture( string imageName, bool useSkinColor = true, Object obj = null )
        {
            if (textureCache.ContainsKey(imageName))
            {
                return textureCache[imageName];
            }
            Texture2D texture2D = null;
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("{0}{1}",
                             !useSkinColor ? string.Empty : !EditorGUIUtility.isProSkin ? "Light" : "Dark",
                             imageName))
                         ?? Assembly.GetExecutingAssembly().GetManifestResourceStream(
                             string.Format("SsitEngine.Editor.Res.{0}{1}",
                                 !useSkinColor
                                     ? string.Empty
                                     : !EditorGUIUtility.isProSkin
                                         ? "Light"
                                         : "Dark", imageName));
            if (stream != null)
            {
                texture2D = new Texture2D(0, 0, TextureFormat.RGBA32, false, true);
                texture2D.LoadImage(ReadToEnd(stream));
                stream.Close();
            }
            if (texture2D != null)
            {
                texture2D.hideFlags = HideFlags.HideAndDontSave;
            }
            else
            {
                Debug.LogWarning("图标加载错误");
            }
            textureCache.Add(imageName, texture2D);
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

        public static Texture2D LoadIcon( string iconName, ScriptableObject obj = null )
        {
            if (iconCache.ContainsKey(iconName))
            {
                return iconCache[iconName];
            }
            var texture2D = (Texture2D) null;
            var stream = Assembly.GetExecutingAssembly()
                             .GetManifestResourceStream(iconName.Replace("{SkinColor}",
                                 !EditorGUIUtility.isProSkin ? "Light" : "Dark"))
                         ?? Assembly.GetExecutingAssembly().GetManifestResourceStream(
                             string.Format("SimpleGameUtility.Resources.{0}",
                                 iconName.Replace("{SkinColor}", !EditorGUIUtility.isProSkin ? "Light" : "Dark")));
            if (stream != null)
            {
                texture2D = new Texture2D(0, 0, TextureFormat.RGBA32, false, true);
                texture2D.LoadImage(ReadToEnd(stream));
                stream.Close();
            }
            if (texture2D == null)
            {
                texture2D = AssetDatabase.LoadAssetAtPath(
                    iconName.Replace("{SkinColor}", !EditorGUIUtility.isProSkin ? "Light" : "Dark"),
                    typeof(Texture2D)) as Texture2D;
            }
            if (texture2D != null)
            {
                texture2D.hideFlags = HideFlags.HideAndDontSave;
            }
            iconCache.Add(iconName, texture2D);
            return texture2D;
        }

        private static byte[] ReadToEnd( Stream stream )
        {
            var buffer = new byte[16384];
            using (var memoryStream = new MemoryStream())
            {
                int count;
                while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    memoryStream.Write(buffer, 0, count);
                }
                return memoryStream.ToArray();
            }
        }


        private static void InitTaskTitleGUIStyle()
        {
            taskTitleGUIStyle = new GUIStyle(GUI.skin.label);
            taskTitleGUIStyle.alignment = TextAnchor.UpperCenter;
            taskTitleGUIStyle.fontSize = 12;
            taskTitleGUIStyle.fontStyle = FontStyle.Normal;
        }

        #region Editor Package

        private static string GetPackageFullPath()
        {
            // Check for potential UPM package
            var packagePath = Path.GetFullPath("Packages/com.coffee.upm-ssitengine");
            if (Directory.Exists(packagePath))
            {
                return packagePath;
            }

            packagePath = Path.GetFullPath("Assets/..");
            if (Directory.Exists(packagePath))
            {
                // Search default location for development package
                if (Directory.Exists(packagePath + "/Assets/Packages/com.coffee.upm-ssitengine/Editor Resources"))
                {
                    return packagePath + "/Assets/Packages/com.coffee.upm-ssitengine";
                }

                // Search for default location of normal TextMesh Pro AssetStore package
                if (Directory.Exists(packagePath + "/Assets/SsitEngine/Editor Resources"))
                {
                    return packagePath + "/Assets/TextMesh Pro";
                }

                // Search for potential alternative locations in the user project
                var matchingPaths =
                    Directory.GetDirectories(packagePath, "SsitEngineAssets", SearchOption.AllDirectories);
                var path = ValidateLocation(matchingPaths, packagePath);
                if (path != null)
                {
                    return packagePath + path;
                }
            }

            return null;
        }


        /// <summary>
        /// Method to validate the location of the asset folder by making sure the GUISkins folder exists.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        private static string ValidateLocation( string[] paths, string projectPath )
        {
            for (var i = 0; i < paths.Length; i++)
            {
                // Check if any of the matching directories contain a GUISkins directory.
                if (Directory.Exists(paths[i] + "/Editor Resources"))
                {
                    folderPath = paths[i].Replace(projectPath, "");
                    folderPath = folderPath.TrimStart('\\', '/');
                    return folderPath;
                }
            }

            return null;
        }

        #endregion
    }
}