using SSIT.proto;
using SsitEngine.PureMVC.Patterns;

namespace Framework.Logic
{
    public class UserProxy : Proxy
    {
        public new static string NAME = "UserProxy";
        private UserInfo m_UserInfo;


        public UserProxy( UserInfo userInfo ) : base(NAME, userInfo)
        {
            m_UserInfo = userInfo;
        }

        public void SetUserInfo( UserInfo userInfo )
        {
            m_UserInfo = userInfo;
        }


        public override void OnRegister()
        {
            base.OnRegister();
        }

        public override void OnRemove()
        {
            base.OnRemove();
        }

        public string GetUserID()
        {
            return m_UserInfo?.Guid;
        }

        public string GetIcon()
        {
            return m_UserInfo?.IconPath;
        }

        public string GetUserName()
        {
            return m_UserInfo?.Name;
        }
        
        //public void SetGroupName(string name)
        //{
        //    m_UserInfo.GroupName = name;
        //}
        //public void SetGroupID( string id )
        //{
        //    m_UserInfo.GroupID = id;
        //}

        public UserInfo GetUserInfo()
        {
            return m_UserInfo;
        }
    }
}