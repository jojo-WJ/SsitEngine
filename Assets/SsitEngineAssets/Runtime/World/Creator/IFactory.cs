using UnityEngine;

namespace Framework.SceneObject
{
    public interface IFactory
    {
        EnFactoryType GetFactoryType();

        object CreateInstance( string guid );

        object CreateInstance( string guid, GameObject data );

        void DestroyInstance( object obj );

        void Shutdown();
    }
}