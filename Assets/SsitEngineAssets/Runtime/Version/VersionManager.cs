using System.IO;
using System.Text;
using SsitEngine;
using SsitEngine.DebugLog;
using UnityEngine;

namespace Framework.Config
{
    public enum GameVersionStatus //游戏版本状态
    {
        VERSION_UNKNOWN = 0, //未知
        VERSION_NORAML = 1, //正常,可登录
        VERSION_LIMITED = 2, //版本过低,功能受限
        VERSION_OVERDUE = 3 //版本过低,不能进行游戏
    }

    public class ConfigManager : Singleton<ConfigManager>, ISingleton
    {
        /// <summary>
        /// 版本配置
        /// </summary>
        public AppVersionConfig VersionConfig { get; private set; }

        /// <summary>
        /// 公司协议
        /// </summary>
        public CompanyProtoConfig CompanyProto { get; private set; }

        /// <summary>
        /// 企业编号
        /// </summary>
        public string CompanyIndex => VersionConfig.mCompanyIndex;


        public void OnSingletonInit()
        {
            var dataConfigPath = AppDefine.APPCONFIGPATH.Substring(AppDefine.APPCONFIGPATH.LastIndexOf("/") + 1);
            dataConfigPath = dataConfigPath.Substring(0, dataConfigPath.LastIndexOf("."));

            var textAsset = Resources.Load<TextAsset>(dataConfigPath);
            if (textAsset == null)
            {
                SsitDebug.Error("程序配置异常");
                return;
            }
            VersionConfig = JsonUtility.FromJson<AppVersionConfig>(textAsset.text);


            if (!File.Exists(AppDefine.APPPROTOPATH))
            {
                SsitDebug.Error($"公司协议不存在。协议文件路径：{AppDefine.APPPROTOPATH}");
                return;
            }

            var file = File.ReadAllText(AppDefine.APPPROTOPATH, Encoding.UTF8);
            CompanyProto = JsonUtility.FromJson<CompanyProtoConfig>(file);
        }

        /*
        /// <summary>
        /// 是否同意公司协议
        /// </summary>
        /// <param name="isAgree"></param>
        public void AgreeCompanyProto( bool isAgree )
        {
            CompanyProto.IsAgree = isAgree;
        }
        
        /// <summary>
        /// 将公司协议重新序列化到本地
        /// </summary> 
        public void SerializeCompanyProto()
        {
            object companyProto = JsonUtility.ToJson( CompanyProto );
            File.WriteAllText( AppDefine.APPPROTOPATH , companyProto.ToString() );
        }*/
    }
}