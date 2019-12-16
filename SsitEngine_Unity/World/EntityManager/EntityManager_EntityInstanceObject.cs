/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：实体实例对象                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/5/5 15:09:08                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.Core.ObjectPool;

namespace SsitEngine.Unity.Entity
{
    public partial class EntityManager
    {
        /// <summary>
        ///     实体实例对象。
        /// </summary>
        private sealed class EntityInstanceObject : ObjectBase
        {
            private readonly object m_EntityAsset;
            private readonly IEntityHelper m_EntityHelper;

            /// <summary>
            ///     创建实体对象实例
            /// </summary>
            /// <param name="name"></param>
            /// <param name="entityAsset"></param>
            /// <param name="entityInstance"></param>
            /// <param name="entityHelper"></param>
            public EntityInstanceObject( string name, object entityAsset, object entityInstance,
                IEntityHelper entityHelper )
                : base(entityInstance)
            {
                if (entityAsset == null) throw new SsitEngineException("Entity asset is invalid.");

                if (entityHelper == null) throw new SsitEngineException("Entity helper is invalid.");

                m_EntityAsset = entityAsset;
                m_EntityHelper = entityHelper;
            }

            protected override void Release( bool isShutdown )
            {
                m_EntityHelper.ReleaseEntity(m_EntityAsset, Target);
            }
        }
    }
}