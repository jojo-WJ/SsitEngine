/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/27 11:46:03                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using SsitEngine;

namespace Framework.SceneObject
{
    using FactoryMap = Dictionary<EnFactoryType, IFactory>;

    public enum EnFactoryType
    {
        /// <summary>
        /// 空
        /// </summary>
        None,

        /// <summary>
        /// 玩家工厂
        /// </summary>
        PlayerFactory,

        /// <summary>
        /// npc工厂
        /// </summary>
        NpcFactory,

        /// <summary>
        /// 场景交互物体工厂
        /// </summary>
        SceneObjectFactory,

        /// <summary>
        /// 道具装备工厂
        /// </summary>
        ItemFactory,

        /// <summary>
        /// 车辆载具工厂
        /// </summary>
        VehicleFactory,

        /// <summary>
        /// 技能工厂
        /// </summary>
        SkillFactory
    }


    public class Creator : AllocatedObject
    {
        protected FactoryMap mFactories;

        public Creator()
        {
            mFactories = new FactoryMap();
        }


        public void AddFactory( IFactory fact )
        {
            if (mFactories.ContainsKey(fact.GetFactoryType()))
                throw new SsitEngineException(
                    TextUtils.Format("A factory of type {0} already exists. Creator:AddFactory",
                        fact.GetFactoryType()));
            // Save
            mFactories[fact.GetFactoryType()] = fact;
            //SsitDebug.Debug("Factory for type '" + fact.GetFactoryType() + "' registered.");
        }

        public void RemoveFactory( IFactory fact )
        {
            if (mFactories.ContainsKey(fact.GetFactoryType())) mFactories.Remove(fact.GetFactoryType());
        }

        public void ReplaceFactory( EnFactoryType typeName, IFactory newFact )
        {
            if (typeName != newFact.GetFactoryType())
                throw new SsitEngineException(TextUtils.Format(
                    "New factory of type {0} is not match to old. Creator:ReplaceFactory", newFact.GetFactoryType()));

            if (mFactories.ContainsKey(typeName))
                mFactories[typeName] = newFact;
        }

        /// <summary>
        /// Checks whether a factory is registered for a given Object type
        /// </summary>
        /// <param name="typeName">指定类型的名称</param>
        /// <returns></returns>
        public bool HasFactory( EnFactoryType typeName )
        {
            return mFactories.ContainsKey(typeName);
        }

        /// <summary>
        ///  Get a Factory for the given type
        /// </summary>
        /// <param name="typeName">指定类型的名称</param>
        /// <returns>返回指定的创建工厂</returns>
        public IFactory GetFactory( EnFactoryType typeName )
        {
            if (mFactories.ContainsKey(typeName))
                return mFactories[typeName];
            return null;
        }

        public override void Shutdown()
        {
            var listIt = mFactories.GetEnumerator();
            while (listIt.MoveNext())
            {
                listIt.Current.Value.Shutdown();
                listIt.Dispose();
            }
            mFactories.Clear();
        }
    }
}