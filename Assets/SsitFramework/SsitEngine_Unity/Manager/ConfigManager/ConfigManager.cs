using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace SsitEngine.Unity.Config
{
    /// <summary>
    /// 配置管理器
    /// </summary>
    public class ConfigManager : ManagerBase<ConfigManager>
    {
        public static string HttpIpPort { get; private set; }

        public bool HasCopyConfig => File.Exists(Application.persistentDataPath + "/Config/config.yml");

        /// <summary>
        /// 设置配置辅助器
        /// </summary>
        /// <param name="helper"></param>
        public void SetConfigHelper( IConfigHelper helper )
        {
            Helper = helper;
            Helper.ReadConfig();
            HttpIpPort = Helper.HttpIpPort;
        }


        #region 模块管理

        public override void OnSingletonInit()
        {
            // 拷贝配置
            Assert.IsTrue(HasCopyConfig, "程序初始化异常");
        }

        public override string ModuleName => typeof(ConfigManager).FullName;

        public override int Priority => 11;

        public IConfigHelper Helper { get; private set; }

        public override void OnUpdate( float elapseSeconds )
        {
        }

        public override void Shutdown()
        {
        }

        #endregion
    }
}