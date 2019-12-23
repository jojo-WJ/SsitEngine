#if UNITY_EDITOR
using System;
using UnityEditor;
using System.IO;
using UnityEngine;

#endif
public class PreviewTextureAttribute : PropertyAttribute
{
    public Texture2D cached;

    /// <summary>
    /// 加载等待时长
    /// </summary>
    public long expire = 6000000000; // 10min

    public Rect lastPosition = new Rect(0, 0, 0, 0);
#pragma warning disable 618
    public WWW www;
#pragma warning restore 618

    public PreviewTextureAttribute()
    {
    }

    public PreviewTextureAttribute( int expire )
    {
        this.expire = expire * 1000 * 10000;
    }
}


#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(PreviewTextureAttribute))]
public class PreviewTextureDrawer : PropertyDrawer
{
    private GUIStyle style;

    private PreviewTextureAttribute previewTextureAttribute => (PreviewTextureAttribute) attribute;

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        position.height = 16;
        if (property.propertyType == SerializedPropertyType.String)
        {
            DrawStringValue(position, property, label);
        }
        else if (property.propertyType == SerializedPropertyType.ObjectReference)
        {
            DrawTextureValue(position, property, label);
        }
    }

    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        return base.GetPropertyHeight(property, label) + previewTextureAttribute.lastPosition.height;
    }

    private void DrawTextureValue( Rect position, SerializedProperty property, GUIContent label )
    {
        property.objectReferenceValue = (Texture2D) EditorGUI.ObjectField(position, label,
            property.objectReferenceValue, typeof(Texture2D), false);

        if (property.objectReferenceValue != null)
        {
            DrawTexture(position, (Texture2D) property.objectReferenceValue);
        }
    }

    private void DrawStringValue( Rect position, SerializedProperty property, GUIContent label )
    {
        EditorGUI.BeginChangeCheck();
        property.stringValue = EditorGUI.TextField(position, label, property.stringValue);
        if (EditorGUI.EndChangeCheck())
        {
            previewTextureAttribute.www = null;
            previewTextureAttribute.cached = null;
        }
        var path = GetCachedTexturePath(property.stringValue);

        if (!string.IsNullOrEmpty(path))
        {
            if (IsExpired(path))
            {
                Delete(path);
            }
            else if (previewTextureAttribute.cached == null)
            {
                previewTextureAttribute.cached = GetTextureFromCached(path);
            }
        }
        else
        {
            previewTextureAttribute.cached = null;
        }

        if (previewTextureAttribute.cached == null)
        {
            previewTextureAttribute.cached = GetTextureFromWWW(position, property);
        }
        else
        {
            DrawTexture(position, previewTextureAttribute.cached);
        }
    }

    private bool IsExpired( string path )
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var split = fileName.Split('_');
        return DateTime.Now.Ticks >= long.Parse(split[1]);
    }

    private string GetCachedTexturePath( string stringValue )
    {
        var hash = stringValue.GetHashCode();
        foreach (var path in Directory.GetFiles("Temp"))
        {
            if (Path.GetFileNameWithoutExtension(path).StartsWith(hash.ToString()))
            {
                return path;
            }
        }
        return string.Empty;
    }

    private Texture2D GetTextureFromWWW( Rect position, SerializedProperty property )
    {
        if (previewTextureAttribute.www == null)
        {
#pragma warning disable 618
            previewTextureAttribute.www = new WWW(property.stringValue);
#pragma warning restore 618
        }
        else if (!previewTextureAttribute.www.isDone)
        {
            previewTextureAttribute.lastPosition = new Rect(position.x, position.y + 16, position.width, 16);
            EditorGUI.ProgressBar(previewTextureAttribute.lastPosition, previewTextureAttribute.www.progress,
                "Downloading... " + previewTextureAttribute.www.progress * 100 + "%");
        }
        else if (previewTextureAttribute.www.isDone)
        {
            if (previewTextureAttribute.www.error != null)
            {
                return null;
            }

            var hash = property.stringValue.GetHashCode();
            var expire = DateTime.Now.Ticks + previewTextureAttribute.expire;
            var path = string.Format("Temp/{0}_{1}_{2}_{3}", hash, expire, previewTextureAttribute.www.texture.width,
                previewTextureAttribute.www.texture.height);
            File.WriteAllBytes(path, previewTextureAttribute.www.bytes);
            EditorUtility.DisplayDialog("", "加载完成", "ok");
            return previewTextureAttribute.www.texture;
        }
        return null;
    }

    private Texture2D GetTextureFromCached( string path )
    {
        var split = Path.GetFileNameWithoutExtension(path).Split('_');
        var width = int.Parse(split[2]);
        var height = int.Parse(split[3]);
        var t = new Texture2D(width, height);

        return t.LoadImage(File.ReadAllBytes(path)) ? t : null;
    }

    private void DrawTexture( Rect position, Texture2D texture )
    {
        var width = Mathf.Clamp(texture.width, position.width * 0.7f, position.width * 0.7f);
        previewTextureAttribute.lastPosition = new Rect(position.width * 0.15f, position.y + 16, width,
            texture.height * (width / texture.width));

        if (style == null)
        {
            style = new GUIStyle();
            style.imagePosition = ImagePosition.ImageOnly;
        }
        style.normal.background = texture;
        GUI.Label(previewTextureAttribute.lastPosition, "", style);
    }

    private void Delete( string path )
    {
        File.Delete(path);
        previewTextureAttribute.cached = null;
    }
}

#endif