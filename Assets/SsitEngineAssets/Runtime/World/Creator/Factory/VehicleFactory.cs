/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/8/19 16:52:38                     
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using UnityEngine;

namespace Framework.SceneObject
{
    public class VehicleFactory : IFactory
    {
        public EnFactoryType GetFactoryType()
        {
            return EnFactoryType.VehicleFactory;
        }

        public object CreateInstance( string guid )
        {
            var obj = new Vehicle(guid);
            return obj;
        }

        public object CreateInstance( string guid, GameObject data )
        {
            var obj = new Vehicle(guid);
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
            var obj = new Vehicle(Guid.NewGuid().ToString());
            return obj;
        }
    }
}