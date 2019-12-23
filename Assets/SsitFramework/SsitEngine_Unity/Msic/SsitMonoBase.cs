using SsitEngine.PureMVC.Patterns;
using UnityEngine;

namespace SsitEngine.Unity.Msic
{
    /// <summary>
    ///     代注册的Mono基类
    /// </summary>
    public class SsitMonoBase : MonoBase
    {
        [HideInInspector] public ushort[] m_msgList;

        #region 消息处理

        /// <summary>
        ///     消息注册
        /// </summary>
        /// <param name="msgs"></param>
        public void RegisterMsg( params ushort[] msgs )
        {
            for (var i = 0; i < msgs.Length; i++)
            {
                Facade.Instance.RegisterObservers(this, msgs[i], HandleNotification);
            }
        }

        /// <summary>
        ///     消息注销
        /// </summary>
        /// <param name="msgs"></param>
        public void UnRegisterMsg( params ushort[] msgs )
        {
            for (var i = 0; i < msgs.Length; i++)
            {
                Facade.Instance.RemoveObservers(this, msgs[i]);
            }
        }

        #endregion
    }
}