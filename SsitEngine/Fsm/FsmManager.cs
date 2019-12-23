/**
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：有限状态管理器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/18 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;

namespace SsitEngine.Fsm
{
    /// <summary>
    ///     有限状态机管理器。
    /// </summary>
    internal sealed class FsmManager : IModule, IFsmManager
    {
        private readonly Dictionary<string, FsmBase> m_Fsms;
        private readonly List<FsmBase> m_TempFsms;

        /// <summary>
        ///     初始化有限状态机管理器的新实例。
        /// </summary>
        public FsmManager()
        {
            m_Fsms = new Dictionary<string, FsmBase>();
            m_TempFsms = new List<FsmBase>();
        }

        /// <summary>
        ///     获取有限状态机数量。
        /// </summary>
        public int Count => m_Fsms.Count;

        /// <summary>
        ///     检查是否存在有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <returns>是否存在有限状态机。</returns>
        public bool HasFsm<T>() where T : class
        {
            return HasFsm(TextUtils.GetFullName<T>(string.Empty));
        }

        /// <summary>
        ///     检查是否存在有限状态机。
        /// </summary>
        /// <param name="ownerType">有限状态机持有者类型。</param>
        /// <returns>是否存在有限状态机。</returns>
        public bool HasFsm( Type ownerType )
        {
            if (ownerType == null)
            {
                throw new SsitEngineException("Owner type is invalid.");
            }

            return HasFsm(TextUtils.GetFullName(ownerType, string.Empty));
        }

        /// <summary>
        ///     检查是否存在有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <param name="name">有限状态机名称。</param>
        /// <returns>是否存在有限状态机。</returns>
        public bool HasFsm<T>( string name ) where T : class
        {
            return HasFsm(TextUtils.GetFullName<T>(name));
        }

        /// <summary>
        ///     检查是否存在有限状态机。
        /// </summary>
        /// <param name="ownerType">有限状态机持有者类型。</param>
        /// <param name="name">有限状态机名称。</param>
        /// <returns>是否存在有限状态机。</returns>
        public bool HasFsm( Type ownerType, string name )
        {
            if (ownerType == null)
            {
                throw new SsitEngineException("Owner type is invalid.");
            }

            return HasFsm(TextUtils.GetFullName(ownerType, name));
        }

        /// <summary>
        ///     获取有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <returns>要获取的有限状态机。</returns>
        public IFsm<T> GetFsm<T>() where T : class
        {
            return (IFsm<T>) GetFsm(TextUtils.GetFullName<T>(string.Empty));
        }

        /// <summary>
        ///     获取有限状态机。
        /// </summary>
        /// <param name="ownerType">有限状态机持有者类型。</param>
        /// <returns>要获取的有限状态机。</returns>
        public FsmBase GetFsm( Type ownerType )
        {
            if (ownerType == null)
            {
                throw new SsitEngineException("Owner type is invalid.");
            }

            return GetFsm(TextUtils.GetFullName(ownerType, string.Empty));
        }

        /// <summary>
        ///     获取有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <param name="name">有限状态机名称。</param>
        /// <returns>要获取的有限状态机。</returns>
        public IFsm<T> GetFsm<T>( string name ) where T : class
        {
            return (IFsm<T>) GetFsm(TextUtils.GetFullName<T>(name));
        }

        /// <summary>
        ///     获取有限状态机。
        /// </summary>
        /// <param name="ownerType">有限状态机持有者类型。</param>
        /// <param name="name">有限状态机名称。</param>
        /// <returns>要获取的有限状态机。</returns>
        public FsmBase GetFsm( Type ownerType, string name )
        {
            if (ownerType == null)
            {
                throw new SsitEngineException("Owner type is invalid.");
            }

            return GetFsm(TextUtils.GetFullName(ownerType, name));
        }

        /// <summary>
        ///     获取所有有限状态机。
        /// </summary>
        /// <returns>所有有限状态机。</returns>
        public FsmBase[] GetAllFsms()
        {
            var index = 0;
            var results = new FsmBase[m_Fsms.Count];
            foreach (var fsm in m_Fsms)
            {
                results[index++] = fsm.Value;
            }

            return results;
        }

        /// <summary>
        ///     获取所有有限状态机。
        /// </summary>
        /// <param name="results">所有有限状态机。</param>
        public void GetAllFsms( List<FsmBase> results )
        {
            if (results == null)
            {
                throw new SsitEngineException("Results is invalid.");
            }

            results.Clear();
            foreach (var fsm in m_Fsms)
            {
                results.Add(fsm.Value);
            }
        }

        /// <summary>
        ///     创建有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <param name="owner">有限状态机持有者。</param>
        /// <param name="states">有限状态机状态集合。</param>
        /// <returns>要创建的有限状态机。</returns>
        public IFsm<T> CreateFsm<T>( T owner, params FsmState<T>[] states ) where T : class
        {
            return CreateFsm(string.Empty, owner, states);
        }

        /// <summary>
        ///     创建有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <param name="name">有限状态机名称。</param>
        /// <param name="owner">有限状态机持有者。</param>
        /// <param name="states">有限状态机状态集合。</param>
        /// <returns>要创建的有限状态机。</returns>
        public IFsm<T> CreateFsm<T>( string name, T owner, params FsmState<T>[] states ) where T : class
        {
            if (HasFsm<T>(name))
            {
                throw new SsitEngineException(TextUtils.Format("Already exist FSM '{0}'.",
                    TextUtils.GetFullName<T>(name)));
            }

            var fsm = new Fsm<T>(name, owner, states);
            m_Fsms.Add(TextUtils.GetFullName<T>(name), fsm);
            return fsm;
        }

        /// <summary>
        ///     销毁有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <returns>是否销毁有限状态机成功。</returns>
        public bool DestroyFsm<T>() where T : class
        {
            return DestroyFsm(TextUtils.GetFullName<T>(string.Empty));
        }

        /// <summary>
        ///     销毁有限状态机。
        /// </summary>
        /// <param name="ownerType">有限状态机持有者类型。</param>
        /// <returns>是否销毁有限状态机成功。</returns>
        public bool DestroyFsm( Type ownerType )
        {
            if (ownerType == null)
            {
                throw new SsitEngineException("Owner type is invalid.");
            }

            return DestroyFsm(TextUtils.GetFullName(ownerType, string.Empty));
        }

        /// <summary>
        ///     销毁有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <param name="name">要销毁的有限状态机名称。</param>
        /// <returns>是否销毁有限状态机成功。</returns>
        public bool DestroyFsm<T>( string name ) where T : class
        {
            return DestroyFsm(TextUtils.GetFullName<T>(name));
        }

        /// <summary>
        ///     销毁有限状态机。
        /// </summary>
        /// <param name="ownerType">有限状态机持有者类型。</param>
        /// <param name="name">要销毁的有限状态机名称。</param>
        /// <returns>是否销毁有限状态机成功。</returns>
        public bool DestroyFsm( Type ownerType, string name )
        {
            if (ownerType == null)
            {
                throw new SsitEngineException("Owner type is invalid.");
            }

            return DestroyFsm(TextUtils.GetFullName(ownerType, name));
        }

        /// <summary>
        ///     销毁有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <param name="fsm">要销毁的有限状态机。</param>
        /// <returns>是否销毁有限状态机成功。</returns>
        public bool DestroyFsm<T>( IFsm<T> fsm ) where T : class
        {
            if (fsm == null)
            {
                throw new SsitEngineException("FSM is invalid.");
            }

            return DestroyFsm(TextUtils.GetFullName<T>(fsm.Name));
        }

        /// <summary>
        ///     销毁有限状态机。
        /// </summary>
        /// <param name="fsm">要销毁的有限状态机。</param>
        /// <returns>是否销毁有限状态机成功。</returns>
        public bool DestroyFsm( FsmBase fsm )
        {
            if (fsm == null)
            {
                throw new SsitEngineException("FSM is invalid.");
            }

            return DestroyFsm(TextUtils.GetFullName(fsm.OwnerType, fsm.Name));
        }

        private bool HasFsm( string fullName )
        {
            return m_Fsms.ContainsKey(fullName);
        }

        private FsmBase GetFsm( string fullName )
        {
            FsmBase fsm = null;
            if (m_Fsms.TryGetValue(fullName, out fsm))
            {
                return fsm;
            }

            return null;
        }

        private bool DestroyFsm( string fullName )
        {
            FsmBase fsm = null;
            if (m_Fsms.TryGetValue(fullName, out fsm))
            {
                fsm.Shutdown();
                return m_Fsms.Remove(fullName);
            }

            return false;
        }

        #region 模块接口实现

        public string ModuleName => typeof(FsmManager).FullName;

        /// <summary>
        ///     获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public int Priority => 60;

        /// <summary>
        ///     有限状态机管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void OnUpdate( float elapseSeconds )
        {
            m_TempFsms.Clear();
            if (m_Fsms.Count <= 0)
            {
                return;
            }

            foreach (var fsm in m_Fsms)
            {
                m_TempFsms.Add(fsm.Value);
            }

            foreach (var fsm in m_TempFsms)
            {
                if (fsm.IsDestroyed)
                {
                    continue;
                }

                fsm.OnUpdate(elapseSeconds);
            }
        }

        /// <summary>
        ///     关闭并清理有限状态机管理器。
        /// </summary>
        public void Shutdown()
        {
            foreach (var fsm in m_Fsms)
            {
                fsm.Value.Shutdown();
            }

            m_Fsms.Clear();
            m_TempFsms.Clear();
        }

        #endregion
    }
}