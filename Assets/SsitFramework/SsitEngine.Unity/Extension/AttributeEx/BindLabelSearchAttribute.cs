using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;

#endif

/// <summary>
/// 标签搜索
/// </summary>
public class BindLabelSearchAttribute : PropertyAttribute
{
    /// <summary>
    /// 检索方向枚举
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// 上升ascend
        /// </summary>
        ASC,

        /// <summary>
        /// 下降descend
        /// </summary>
        DESC
    }


    /// <summary>
    /// <para>检索的字典缓存</para>
    /// </summary>
    public static Dictionary<string, Type> assetTypes = new Dictionary<string, Type>();

    /// <summary>
    /// <para>打印标签名称</para>
    /// <para>true打印</para>
    /// </summary>
    public bool canPrintLabelName = false;

    /// <summary>
    /// <para>检索的顺序</para>
    /// </summary>
    public Direction direction = Direction.ASC;

    public bool error;

    /// <summary>
    /// <para>是否折叠</para>
    /// </summary>
    public bool foldout = false;

    /// <summary>
    /// <para> false检索未开始</para>
    /// <para>true是否初始化（也就是检索完成）</para>
    /// </summary>
    public bool init;

    /// <summary>
    /// <para>搜索标签名</para>
    /// </summary>
    public string labelName;

    /// <summary>
    /// <para>检索的最大数量</para>
    /// <para>负数和0也是32位最大整型常量 2^31-1————2147483647
    /// </summary>
    public int limit = 2147483647;

    /// <summary>
    /// <para>true 是否搜索</para>
    /// <para>false检索</para>
    /// </summary>
    public bool search = true;

    public BindLabelSearchAttribute()
    {
    }

    public BindLabelSearchAttribute( string labelName )
    {
        this.labelName = labelName;
    }

    public BindLabelSearchAttribute( string labelName, int limit )
    {
        //当 f 为正或为0返回1，为负返回-1。
        if (Mathf.Sign(limit) == 1)
        {
            this.limit = limit;
        }
        this.labelName = labelName;
    }

    public BindLabelSearchAttribute( int limit )
    {
        //当 f 为正或为0返回1，为负返回-1。
        if (Mathf.Sign(limit) == 1)
        {
            this.limit = limit;
        }
    }

    public BindLabelSearchAttribute( string labelName, Direction direction )
    {
        this.labelName = labelName;
        this.direction = direction;
    }

    public BindLabelSearchAttribute( string labelName, int limit, Direction direction )
    {
        this.labelName = labelName;

        if (Mathf.Sign(limit) == 1)
        {
            this.limit = limit;
        }

        this.direction = direction;
    }
}


#if UNITY_EDITOR
/// <summary>
/// Label drawer.
/// </summary>
[CustomPropertyDrawer(typeof(BindLabelSearchAttribute))]
public class BindLabelSearchDrawer : PropertyDrawer
{
    /// <summary>
    /// GUI的高度
    /// </summary>
    private const int CONTENT_HEIGHT = 16;

    /// <summary>
    /// 反射属性获取
    /// </summary>
    private BindLabelSearchAttribute SimpleLabelSearchAttribute => (BindLabelSearchAttribute) attribute;

    /// <summary>
    /// 设置绘制高度
    /// </summary>
    /// <param name="property"></param>
    /// <param name="label"></param>
    /// <returns></returns>
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        float height = 0;

        if (property.isArray && SimpleLabelSearchAttribute.foldout)
        {
            height = (property.arraySize + 1) * CONTENT_HEIGHT;
        }
        if (SimpleLabelSearchAttribute.error)
        {
            height += CONTENT_HEIGHT * 2;
        }
        return base.GetPropertyHeight(property, label) + height;
    }

    /// <summary>
    /// 绘制属性面板
    /// </summary>
    /// <param name="position"></param>
    /// <param name="property"></param>
    /// <param name="label"></param>
    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        if (SimpleLabelSearchAttribute.init == false)
        {
            SimpleLabelSearchAttribute.init = true;
            return;
        }
        //检索时间 ( 検索時間 )EdiorApplication.timeSinceStartUp
        //double start = EditorApplication.timeSinceStartup;
        if (SimpleLabelSearchAttribute.canPrintLabelName)
        {
            label.text += string.Format(" ( Label = {0} )", SimpleLabelSearchAttribute.labelName);
        }
        if (!property.isArray)
        {
            if (SimpleLabelSearchAttribute.search)
            {
                DrawSingleProperty(position, property, label);
                //// ( 検索時間 )
                //Debug.Log (((float)EditorApplication.timeSinceStartup - start) + "ms");
            }
            else
            {
                DrawCachedSingleProperty(position, property, label);
            }
        }
        SimpleLabelSearchAttribute.search = false;
    }


    /// <summary>
    /// <para>绘制属性</para>
    /// </summary>
    /// <param name='position'>
    /// Position.
    /// </param>
    /// <param name='property'>
    /// Property.
    /// </param>
    /// <param name='label'>
    /// Label.
    /// </param>
    private void DrawCachedSingleProperty( Rect position, SerializedProperty property, GUIContent label )
    {
        if (!string.IsNullOrEmpty(SimpleLabelSearchAttribute.labelName))
        {
            if (property.objectReferenceValue == null)
            {
                SimpleLabelSearchAttribute.error = true;
                var helpRect = new Rect(position.x, position.y + CONTENT_HEIGHT, position.width, CONTENT_HEIGHT * 2);
                EditorGUI.HelpBox(helpRect, string.Format("特性绑定资源{0}搜索不到", SimpleLabelSearchAttribute.labelName),
                    MessageType.Warning);
                GUI.color = Color.white;
                GUI.enabled = true;
            }
            else
            {
                if (!property.objectReferenceValue.name.Equals(SimpleLabelSearchAttribute.labelName))
                {
                    SimpleLabelSearchAttribute.error = true;
                    var helpRect = new Rect(position.x, position.y + CONTENT_HEIGHT, position.width,
                        CONTENT_HEIGHT * 2);
                    EditorGUI.HelpBox(helpRect,
                        string.Format("选取的资源与特性绑定资源 [{0}] 不匹配", SimpleLabelSearchAttribute.labelName),
                        MessageType.Error);
                }
                else
                {
                    SimpleLabelSearchAttribute.error = false;
                    GUI.color = Color.gray;
                    GUI.enabled = false;
                }
            }
        }
        else
        {
            GUI.color = Color.white;
            GUI.enabled = true;
        }
        var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.objectReferenceValue = EditorGUI.ObjectField(labelRect, label, property.objectReferenceValue,
            GetType(property), false);
        GUI.color = Color.white;
        GUI.enabled = true;
    }

    /// <summary>
    /// <para>检索资源</para>
    /// </summary>
    /// <param name='position'>
    /// Position.
    /// </param>
    /// <param name='property'>
    /// Property.
    /// </param>
    /// <param name='label'>
    /// Label.
    /// </param>
    private void DrawSingleProperty( Rect position, SerializedProperty property, GUIContent label )
    {
        property.objectReferenceValue = null;
        //如果没有筛选标签就不做遍历
        if (string.IsNullOrEmpty(SimpleLabelSearchAttribute.labelName))
        {
            return;
        }
        var type = GetType(property);
        foreach (var path in GetAllAssetPath())
        {
            Type assetType = null;
            Object asset = null;

            if (BindLabelSearchAttribute.assetTypes.TryGetValue(path, out assetType) == false)
            {
                asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset == null)
                {
                    continue;
                }
                assetType = asset.GetType();
                BindLabelSearchAttribute.assetTypes.Add(path, assetType);
            }
            if (type != assetType)
            {
                continue;
            }
            if (asset == null)
            {
                asset = AssetDatabase.LoadMainAssetAtPath(path);
                if (asset == null)
                {
                    continue;
                }
            }
            /*if (string.IsNullOrEmpty(AssetDatabase.GetLabels(asset).FirstOrDefault(l => l.Equals(labelSearchAttribute.labelName))) == false)
            {
                property.objectReferenceValue = asset;
                break;
            }*/
            if (!string.IsNullOrEmpty(asset.name) && !string.IsNullOrEmpty(SimpleLabelSearchAttribute.labelName)
                                                  && asset.name.Equals(SimpleLabelSearchAttribute.labelName))
            {
                GUI.enabled = false;
                GUI.color = Color.gray;
                property.objectReferenceValue = asset;
                EditorUtility.SetDirty(asset);
                break;
            }
        }

        property.objectReferenceValue = EditorGUI.ObjectField(position, label, property.objectReferenceValue, type,
            false);
        GUI.color = Color.white;
        GUI.enabled = true;
    }


    /// <summary>
    /// 取得所有资源的路径
    /// </summary>
    /// <returns>
    /// The all asset path.
    /// </returns>
    private string[] GetAllAssetPath()
    {
        var allAssetPath = AssetDatabase.GetAllAssetPaths();

        Array.Sort(allAssetPath);

        if (SimpleLabelSearchAttribute.direction.Equals(BindLabelSearchAttribute.Direction.DESC))
        {
            Array.Reverse(allAssetPath);
        }
        return allAssetPath;
    }

    /// <summary>
    /// 获取属性类型
    /// </summary>
    /// <returns>
    /// The type.
    /// </returns>
    /// <param name='property'>
    /// Property.
    /// </param>
    private Type GetType( SerializedProperty property )
    {
        return
            Assembly.Load("UnityEngine.dll")
                .GetType("UnityEngine." + property.type.Replace("PPtr<$", "").Replace(">", ""));
    }
}

#endif