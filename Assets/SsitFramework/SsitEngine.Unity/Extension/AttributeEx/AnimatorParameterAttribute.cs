using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

/// <summary>
/// Animator Paramater attribute.
/// </summary>
public class AnimatorParameterAttribute : PropertyAttribute
{
    /// <summary>
    /// 参数定型
    /// </summary>
    public enum ParameterType
    {
        Float = 1,
        Int = 3,
        Bool = 4,
        Trigger = 9,
        None = 9999
    }

    /// <summary>
    /// 参数型。缺省不考虑类型
    /// </summary>
    public ParameterType parameterType = ParameterType.None;

    /// <summary>
    /// 现在选择中的索引
    /// </summary>
    public int selectedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatorParameterAttribute"/> class.
    /// </summary>
    public AnimatorParameterAttribute() : this(ParameterType.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimatorParameterAttribute"/> class.
    /// </summary>
    /// <param name='ParamaterType'>
    /// 指定为选择类型
    /// </param>
    public AnimatorParameterAttribute( ParameterType parameterType )
    {
        this.parameterType = parameterType;
    }
}

#if UNITY_EDITOR

/// <summary>
/// 绘制Animator窗口的参数剧本类型
/// </summary>
/// <exception cref='InvalidCastException'>/*转换异常*/
/// Is thrown when an explicit conversion (casting operation) fails because the source type cannot be converted to the
/// destination type.
/// </exception>
/// <exception cref='MissingComponentException'>/*组件缺失异常*/
/// Is thrown when the missing component exception.
/// </exception>
[CustomPropertyDrawer(typeof(AnimatorParameterAttribute))]
public class AnimatorParameterDrawer : PropertyDrawer
{
    private AnimatorParameterAttribute animatorParameterAttribute => (AnimatorParameterAttribute) attribute;

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        var animatorController = GetAnimatorController(property);

        if (animatorController == null)
        {
            DefaultInspector(position, property, label);
            return;
        }
        var parameters = animatorController.parameters;

        if (parameters.Length == 0)
        {
            Debug.LogWarning("AnimationParamater is 0");
            property.stringValue = string.Empty;
            DefaultInspector(position, property, label);
            return;
        }

        var eventNames = parameters
            .Where(t => CanAddEventName(t.type))
            .Select(t => t.name).ToList();

        if (eventNames.Count == 0)
        {
            Debug.LogWarning(string.Format("{0} Parameter is 0", animatorParameterAttribute.parameterType));
            property.stringValue = string.Empty;
            DefaultInspector(position, property, label);
            return;
        }

        var eventNamesArray = eventNames.ToArray();

        var matchIndex = eventNames
            .FindIndex(eventName => eventName.Equals(property.stringValue));

        if (matchIndex != -1)
        {
            animatorParameterAttribute.selectedValue = matchIndex;
        }

        animatorParameterAttribute.selectedValue = EditorGUI.IntPopup(position, label.text,
            animatorParameterAttribute.selectedValue,
            eventNamesArray, SetOptionValues(eventNamesArray));

        property.stringValue = eventNamesArray[animatorParameterAttribute.selectedValue];
    }

    /// <summary>
    /// Gets the animator controller.
    /// </summary>
    /// <returns>
    /// The animator controller.
    /// </returns>
    /// <param name='property'>
    /// Property.
    /// </param>
    private AnimatorController GetAnimatorController( SerializedProperty property )
    {
        var component = property.serializedObject.targetObject as Component;

        if (component == null)
        {
            throw new InvalidCastException("Couldn't cast targetObject");
        }

        var anim = component.GetComponent<Animator>();

        if (anim == null)
        {
            anim = component.GetComponentInChildren<Animator>();
            if (anim == null)
            {
                Debug.LogException(new MissingComponentException("Missing Aniamtor Component"));
                return null;
            }
        }

        return anim.runtimeAnimatorController as AnimatorController;
    }

    /// <summary>
    /// Determines whether this instance can add event name the specified animatorController index.
    /// </summary>
    /// <returns>
    /// <c>true</c> if this instance can add event name the specified animatorController i; otherwise, <c>false</c>.
    /// </returns>
    /// <param name='animatorController'>
    /// If set to <c>true</c> animator controller.
    /// </param>
    /// <param name='index'>
    /// If set to <c>true</c> index.
    /// </param>
    private bool CanAddEventName( AnimatorControllerParameterType animatorControllerParameterType )
    {
        return !(animatorParameterAttribute.parameterType != AnimatorParameterAttribute.ParameterType.None
                 && (int) animatorControllerParameterType != (int) animatorParameterAttribute.parameterType);
    }

    /// <summary>
    /// Sets the option values.
    /// </summary>
    /// <returns>
    /// The option values.
    /// </returns>
    /// <param name='eventNames'>
    /// Event names.
    /// </param>
    private int[] SetOptionValues( string[] eventNames )
    {
        var optionValues = new int[eventNames.Length];
        for (var i = 0; i < eventNames.Length; i++)
        {
            optionValues[i] = i;
        }
        return optionValues;
    }

    private void DefaultInspector( Rect position, SerializedProperty property, GUIContent label )
    {
        EditorGUI.PropertyField(position, property, label);
    }
}

#endif