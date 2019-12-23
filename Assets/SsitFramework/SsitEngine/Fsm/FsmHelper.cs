/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：有限状态机的辅助器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/20 11:40:56                     
*└──────────────────────────────────────────────────────────────┘
*/

namespace SsitEngine.Fsm
{
    /// <summary>
    ///     状态机助手
    /// </summary>
    public class FsmHelper
    {
        /// <summary>
        ///     创建有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <param name="name">有限状态机名称。</param>
        /// <param name="owner">有限状态机持有者。</param>
        /// <param name="states">有限状态机状态集合。</param>
        /// <returns>要创建的有限状态机。</returns>
        public static Fsm<T> CreateFsm<T>( string name, T owner, params FsmState<T>[] states ) where T : class
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new SsitEngineException(TextUtils.Format("Fsm  name is null or empty '{0}'.",
                    TextUtils.GetFullName<T>(name)));
            }

            var fsm = new Fsm<T>(name, owner, states);

            //todo：预留管理接口

            return fsm;
        }

        /// <summary>
        ///     销毁有限状态机
        /// </summary>
        /// <param name="fsm"></param>
        /// <returns></returns>
        public static bool DestroyFsm( FsmBase fsm )
        {
            if (fsm != null)
                //todo：预留管理接口

            {
                fsm.Shutdown();
            }
            return false;
        }
    }
}