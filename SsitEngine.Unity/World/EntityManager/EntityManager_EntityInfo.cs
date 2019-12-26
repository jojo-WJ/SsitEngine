/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/5 15:04:53                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;

namespace SsitEngine.Unity.Entity
{
    public partial class EntityManager
    {
        /// <summary>
        ///     实体信息。
        /// </summary>
        private sealed class EntityInfo
        {
            private static readonly IEntity[] EmptyArray = { };

            private List<IEntity> m_ChildEntities;

            public EntityInfo( IEntity entity )
            {
                if (entity == null)
                {
                    throw new SsitEngineException("Entity is invalid.");
                }

                Entity = entity;
                Status = EntityStatus.WillInit;
                ParentEntity = null;
                m_ChildEntities = null;
            }

            public IEntity Entity { get; }

            public EntityStatus Status { get; set; }

            public IEntity ParentEntity { get; }

            public IEntity[] GetChildEntities()
            {
                if (m_ChildEntities == null)
                {
                    return EmptyArray;
                }

                return m_ChildEntities.ToArray();
            }

            public void GetChildEntities( List<IEntity> results )
            {
                if (results == null)
                {
                    throw new SsitEngineException("Results is invalid.");
                }

                results.Clear();
                if (m_ChildEntities == null)
                {
                    return;
                }

                foreach (var childEntity in m_ChildEntities)
                {
                    results.Add(childEntity);
                }
            }

            public void AddChildEntity( IEntity childEntity )
            {
                if (m_ChildEntities == null)
                {
                    m_ChildEntities = new List<IEntity>();
                }

                if (m_ChildEntities.Contains(childEntity))
                {
                    throw new SsitEngineException("Can not add child entity which is already exist.");
                }

                m_ChildEntities.Add(childEntity);
            }

            public void RemoveChildEntity( IEntity childEntity )
            {
                if (m_ChildEntities == null || !m_ChildEntities.Remove(childEntity))
                {
                    throw new SsitEngineException("Can not remove child entity which is not exist.");
                }
            }
        }
    }
}