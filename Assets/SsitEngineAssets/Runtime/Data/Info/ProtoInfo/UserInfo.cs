using SsitEngine.Data;

namespace SSIT.proto
{
    public partial class UserInfo : IInfoData
    {
        /// <summary>
        /// 组织名称
        /// </summary>
        public string OrganizeName;

        public string ID { get; set; }
        
        public string Guid { get; set; }
        
        public string IconPath { get; set; }
        
        public string Name { get; set; }
    }
}