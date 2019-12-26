using UnityEngine;
using System.Collections;
using System.Linq.Expressions;
using System;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using Invector;

public class vEditorHelper : Editor
{
    /// <summary>
    /// Get PropertyName
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyLambda">You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'</param>
    /// <returns></returns>
    public static string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
    {
        var me = propertyLambda.Body as MemberExpression;

        if (me == null)
        {
            throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
        }

        return me.Member.Name;
    }

    /// <summary>
    /// Check if type is a <see cref="UnityEngine.Events.UnityEvent"/>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsUnityEventyType(Type type)
    {
        if (type.Equals(typeof(UnityEngine.Events.UnityEvent))) return true;
        if (type.BaseType.Equals(typeof(UnityEngine.Events.UnityEvent))) return true;
        if (type.Name.Contains("UnityEvent") || type.BaseType.Name.Contains("UnityEvent")) return true;
        return false;
    }
}

[CanEditMultipleObjects]
[CustomEditor(typeof(vMonoBehaviour), true)]
public class vEditorBase : Editor
{
    #region Variables   
    public string[] ignoreEvents;
    public string[] notEventProperties;
    public string[] ignore_vMono = new string[] { "openCloseWindow", "openCloseEvents", "selectedToolbar" };
    public vClassHeaderAttribute headerAttribute;
    public GUISkin skin;
    public Texture2D m_Logo;
    public List<vToolBar> toolbars;
    #endregion

    public class vToolBar
    {
        public string title;
        public List<string> variables;
        public vToolBar()
        {
            title = string.Empty;
            variables = new List<string>();
        }
    }

    protected virtual void OnEnable()
    {
        var targetObject = serializedObject.targetObject;
        var hasAttributeHeader = targetObject.GetType().IsDefined(typeof(vClassHeaderAttribute), true);
        if (hasAttributeHeader)
        {
            var attributes = Attribute.GetCustomAttributes(targetObject.GetType(), typeof(vClassHeaderAttribute), true);
            if (attributes.Length > 0)
                headerAttribute = (vClassHeaderAttribute)attributes[0];
        }

        skin = Resources.Load("skin") as GUISkin;
        m_Logo = Resources.Load("icon_v2") as Texture2D;
        var prop = serializedObject.GetIterator();

        if (((vMonoBehaviour)target) != null)
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            List<string> events = new List<string>();

            toolbars = new List<vToolBar>();
            var toolbar = new vToolBar();
            toolbar.title = "Default";
            toolbars.Add(toolbar);
            var index = 0;

            while (prop.NextVisible(true))
            {
                var fieldInfo = targetObject.GetType().GetField(prop.name, flags);
                if (fieldInfo != null)
                {
                    var toolBarAttributes = fieldInfo.GetCustomAttributes(typeof(vEditorToolbarAttribute), true);

                    if (toolBarAttributes.Length > 0)
                    {
                        var attribute = toolBarAttributes[0] as vEditorToolbarAttribute;
                        var _toolbar = toolbars.Find(tool => tool != null && tool.title == attribute.title);

                        if (_toolbar == null)
                        {
                            toolbar = new vToolBar();
                            toolbar.title = attribute.title;
                            toolbars.Add(toolbar);
                            index = toolbars.Count - 1;
                        }
                        else index = toolbars.IndexOf(_toolbar);
                    }
                    if (index < toolbars.Count)
                        toolbars[index].variables.Add(prop.name);
                    if ((vEditorHelper.IsUnityEventyType(fieldInfo.FieldType) && !events.Contains(fieldInfo.Name)))
                    {
                        events.Add(fieldInfo.Name);
                    }
                }
            }

            var nullToolBar = toolbars.FindAll(tool => tool != null && (tool.variables == null || tool.variables.Count == 0));
            for (int i = 0; i < nullToolBar.Count; i++)
            {
                if (toolbars.Contains(nullToolBar[i]))
                    toolbars.Remove(nullToolBar[i]);
            }

            ignoreEvents = events.vToArray();
            if (headerAttribute != null)
                m_Logo = Resources.Load(headerAttribute.iconName) as Texture2D;
            //else headerAttribute = new vClassHeaderAttribute(target.GetType().Name);
        }
    }

    protected bool openCloseWindow
    {
        get
        {
            return serializedObject.FindProperty("openCloseWindow").boolValue;
        }
        set
        {
            var _openClose = serializedObject.FindProperty("openCloseWindow");
            if (_openClose != null && value != _openClose.boolValue)
            {
                _openClose.boolValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }

    protected bool openCloseEvents
    {
        get
        {
            var _openCloseEvents = serializedObject.FindProperty("openCloseEvents");
            return _openCloseEvents != null ? _openCloseEvents.boolValue : false;
        }
        set
        {
            var _openCloseEvents = serializedObject.FindProperty("openCloseEvents");
            if (_openCloseEvents != null && value != _openCloseEvents.boolValue)
            {
                _openCloseEvents.boolValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }

    protected int selectedToolBar
    {
        get
        {
            var _selectedToolBar = serializedObject.FindProperty("selectedToolbar");
            return _selectedToolBar != null ? _selectedToolBar.intValue : 0;
        }
        set
        {
            var _selectedToolBar = serializedObject.FindProperty("selectedToolbar");

            if (_selectedToolBar != null && value != _selectedToolBar.intValue)
            {
                _selectedToolBar.intValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (toolbars != null && toolbars.Count > 1)
        {
            GUILayout.BeginVertical(headerAttribute != null ? headerAttribute.header : target.GetType().Name, skin.window);
            GUILayout.Label(m_Logo, skin.label, GUILayout.MaxHeight(25));
            if (headerAttribute.openClose)
            {
                openCloseWindow = GUILayout.Toggle(openCloseWindow, openCloseWindow ? "Close Properties" : "Open Properties", EditorStyles.toolbarButton);
            }

            if (!headerAttribute.openClose || openCloseWindow)
            {
                var titles = getToobarTitles();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
                GUILayout.Space(10);
                var customToolbar = skin.GetStyle("customToolbar");
                selectedToolBar = GUILayout.SelectionGrid(selectedToolBar, titles, 3, customToolbar, GUILayout.MinWidth(250));
                if (!(selectedToolBar < toolbars.Count)) selectedToolBar = 0;
                GUILayout.Space(10);
                GUILayout.Box(toolbars[selectedToolBar].title, skin.box, GUILayout.ExpandWidth(true));
                var ignore = getIgnoreProperties(toolbars[selectedToolBar]);
                var ignoreProperties = ignore.Append(ignore_vMono);
                DrawPropertiesExcluding(serializedObject, ignoreProperties);
            }
            GUILayout.EndVertical();
        }
        else
        {
            if (headerAttribute == null)
            {
                if (((vMonoBehaviour)target) != null)
                    DrawPropertiesExcluding(serializedObject, ignore_vMono);
                else
                    base.OnInspectorGUI();
            }
            else
            {
                GUILayout.BeginVertical(headerAttribute.header, skin.window);
                GUILayout.Label(m_Logo, skin.label, GUILayout.MaxHeight(25));
                if (headerAttribute.openClose)
                {
                    openCloseWindow = GUILayout.Toggle(openCloseWindow, openCloseWindow ? "Close Properties" : "Open Properties", EditorStyles.toolbarButton);
                }

                if (!headerAttribute.openClose || openCloseWindow)
                {
                    if (headerAttribute.useHelpBox)
                        EditorGUILayout.HelpBox(headerAttribute.helpBoxText, MessageType.Info);

                    if (ignoreEvents != null && ignoreEvents.Length > 0)
                    {
                        var ignoreProperties = ignoreEvents.Append(ignore_vMono);
                        DrawPropertiesExcluding(serializedObject, ignoreProperties);
                        openCloseEvents = GUILayout.Toggle(openCloseEvents, (openCloseEvents ? "Close " : "Open ") + "Events ", skin.button);

                        if (openCloseEvents)
                        {
                            foreach (string propName in ignoreEvents)
                            {
                                var prop = serializedObject.FindProperty(propName);
                                if (prop != null)
                                    EditorGUILayout.PropertyField(prop);
                            }
                        }
                    }
                    else
                    {
                        var ignoreProperties = ignoreEvents.Append(ignore_vMono);
                        DrawPropertiesExcluding(serializedObject, ignoreProperties);
                    }
                }
                EditorGUILayout.EndVertical();
            }
        }

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
        }
    }

    public string[] getToobarTitles()
    {
        List<string> props = new List<string>();
        for (int i = 0; i < toolbars.Count; i++)
        {
            if (toolbars[i] != null)
                props.Add(toolbars[i].title);
        }
        return props.vToArray();
    }

    public string[] getIgnoreProperties(vToolBar toolbar)
    {
        List<string> props = new List<string>();
        for (int i = 0; i < toolbars.Count; i++)
        {
            if (toolbars[i] != null && toolbar != null && toolbar.variables != null)
            {
                for (int a = 0; a < toolbars[i].variables.Count; a++)
                {
                    if (!props.Contains(toolbars[i].variables[a]) && !toolbar.variables.Contains(toolbars[i].variables[a]))
                    {
                        props.Add(toolbars[i].variables[a]);
                    }
                }
            }
        }

        props.Add("m_Script");
        return props.vToArray();
    }
}