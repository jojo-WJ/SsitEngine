/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/28 20:24:50                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.IO;
using SsitEngine.DebugLog;
using UnityEngine;

namespace SsitEngine.Unity.Config
{
    public class NetConfig
    {
        public string HttpIp;
        public ushort HttpPort;
        public string SocketIp;
        public ushort SocketPort;

        public string GetHttpIpPort()
        {
            return "http://" + HttpIp + ":" + HttpPort + "/";
        }
    }

    public class Config
    {
        public NetConfig NetWorkConfig;
    }

    /// <summary>
    ///     配置辅助器
    /// </summary>
    public class ConfigHelper : AllocatedObject, IConfigHelper
    {
        public Config config;


        public string SocketIp => config.NetWorkConfig.SocketIp;

        public ushort SocketPort => config.NetWorkConfig.SocketPort;

        public string HttpIp => config.NetWorkConfig.HttpIp;

        public ushort HttpPort => config.NetWorkConfig.HttpPort;

        public string HttpIpPort => config.NetWorkConfig.GetHttpIpPort();


        public virtual void ReadConfig()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsEditor:
                {
                    //streaming                不带/
                    var path = Application.streamingAssetsPath + "/Config/config.yml";
                    //直接读取
                    if (!File.Exists(path))
                    {
                        SsitDebug.Error("The config file is not exist. The file path: " + path);
                    }

                    var configText = File.ReadAllText(path);
                    config = YamlUtils.Deserialize<Config>(configText);
                    if (config == null)
                    {
                        SsitDebug.Error("The config text is error. The file path: " + path);
                    }
                }
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.Android:
                {
                    // persistent
                    //streaming                不带/
                    var path = Application.persistentDataPath + "/Config/config.yml";
                    //直接读取
                    if (!File.Exists(path))
                    {
                        SsitDebug.Error("The config file is not exist. The file path: " + path);
                    }

                    var configText = File.ReadAllText(path);
                    config = YamlUtils.Deserialize<Config>(configText);
                    if (config == null)
                    {
                        SsitDebug.Error("The config text is error. The file path: " + path);
                    }
                }
                    break;
                default:
                {
                }
                    break;
            }
        }

        public override void Shutdown()
        {
            config = null;
            base.Shutdown();
        }
    }
}