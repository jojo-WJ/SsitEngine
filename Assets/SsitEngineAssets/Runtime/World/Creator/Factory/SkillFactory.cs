using System;
using UnityEngine;

namespace Framework.SceneObject
{
    public class SkillFactory : IFactory
    {
        public EnFactoryType GetFactoryType()
        {
            return EnFactoryType.SkillFactory;
        }

        public object CreateInstance( string guid )
        {
            var obj = new Skill(guid);
            return obj;
        }

        public object CreateInstance( string guid, GameObject data )
        {
            var obj = new Skill(guid);
            return obj;
        }

        public void DestroyInstance( object obj )
        {
            ((BaseObject) obj).Shutdown();
        }

        public void Shutdown()
        {
        }

        public object CreateInstance()
        {
            var obj = new Skill(Guid.NewGuid().ToString());
            return obj;
        }
    }
}