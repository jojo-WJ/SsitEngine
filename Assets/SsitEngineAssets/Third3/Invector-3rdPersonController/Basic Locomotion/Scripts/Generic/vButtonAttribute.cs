using System;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class vButtonAttribute : PropertyAttribute
    {
        public readonly bool enabledJustInPlayMode;
        public readonly string function;
        public readonly int id;
        public readonly string label;
        public readonly Type type;

        /// <summary>
        /// Create a button in Inspector
        /// </summary>
        /// <param name="label">button label</param>
        /// <param name="function">function to call on press</param>
        /// <param name="type">parent class type button</param>
        /// <param name="enabledJustInPlayMode">button is enabled just in play mode</param>
        public vButtonAttribute( string label, string function, Type type, bool enabledJustInPlayMode = true )
        {
            this.label = label;
            this.function = function;
            this.type = type;
            this.enabledJustInPlayMode = enabledJustInPlayMode;
        }
    }
}