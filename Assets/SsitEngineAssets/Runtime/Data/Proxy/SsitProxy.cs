/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/8/13 17:00:15                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework.Data;
using SsitEngine.PureMVC.Patterns;
using UnityEngine;

namespace Framework.Logic
{
    public class SsitProxy : Proxy, ISave
    {
        public GameObject m_present;

        public SsitProxy( string proxyName, object data = null ) : base(proxyName, data)
        {
        }

        #region EzReplay

        public virtual void InitEzReplay()
        {
            m_present = new GameObject(NAME);
            m_present.transform.SetParent(GlobalManager.Instance.transform);
        }

        #endregion

        #region ISave

        public bool InitSave { get; set; }

        public virtual string Guid
        {
            get => NAME;
            set { }
        }

        public int ItemID
        {
            get => -1;
            set { }
        }

        public virtual SavedBase GeneralSaveData( bool isDeepClone = false )
        {
            return null;
        }

        public virtual GameObject GetRepresent()
        {
            return m_present;
        }

        public virtual void SynchronizeProperties( SavedBase savedState, bool isReset, bool isFristFrame )
        {
        }

        public void SaveRecord()
        {
            Facade.Instance.SendNotification((ushort) EnEzReplayEvent.Mark, this);
        }

        #endregion

        #region 子类实现

        public override void OnRegister()
        {
            InitEzReplay();

//            if (GlobalManager.Instance.ReplayMode == ActionMode.READY)
//            {
//                Facade.Instance.SendNotification((ushort)EnEzReplayEvent.Mark, this);
//                Debug.Log("OnRegister Mark");
//            }
        }

        public override void OnRemove()
        {
            if (m_present) Object.Destroy(m_present);
        }

        #endregion
    }
}