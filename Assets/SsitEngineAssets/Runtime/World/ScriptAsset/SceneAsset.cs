using System;
using UnityEngine;

namespace Framework.SceneObject
{
    [Serializable]
    public class SceneAsset : ScriptableObject
    {
        //public ScriptInstance[] interactionMaps;
        public BaseSceneInstance[] interactionMaps;

        public static SceneAsset CreateInstance()
        {
            return CreateInstance<SceneAsset>();
        }
    }
}