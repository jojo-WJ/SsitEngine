﻿/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：场景物体创建工厂                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/27 15:54:23                     
*└──────────────────────────────────────────────────────────────┘
*/


using System;
using UnityEngine;

namespace Framework.SceneObject
{
    public class PlayerFactory : IFactory
    {
        public EnFactoryType GetFactoryType()
        {
            return EnFactoryType.PlayerFactory;
        }

        public object CreateInstance( string guid )
        {
            var obj = new Player(guid);
            return obj;
        }

        public object CreateInstance( string guid, GameObject data )
        {
            var obj = new Player(guid);
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
            var obj = new Player(Guid.NewGuid().ToString());
            return obj;
        }
    }
}