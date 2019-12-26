using System;
using UnityEngine;

namespace Framework.SceneObject
{
    [Serializable]
    public class SceneConfigScripts : ScriptableObject
    {
        public BaseSceneInstance[] interactionMaps;

        public static SceneConfigScripts CreateInstance()
        {
            return CreateInstance<SceneConfigScripts>();
        }
    }
}