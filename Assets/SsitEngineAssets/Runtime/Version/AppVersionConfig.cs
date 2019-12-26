using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Config
{
    [Serializable]
    public class AppVersionConfig
    {
        public List<string> Companys = new List<string>();
        public string mCompanyIndex;
        public string Version = "1.1.0";

        //Temp Amblance's targetPosition
        //public List<Vector3> AmlanceTargetPos = new List<Vector3>();
    }

    [Serializable]
    public class CompanyProtoConfig
    {
        //public bool IsAgree;
        public string ProtoContent;
    }

    public class AppDefine
    {
        // 程序配置路径
        public static readonly string APPCONFIGPATH = "/Resources/PublishConfig.json";

        /// <summary>
        /// 公司安全协议路径
        /// </summary>
        public static readonly string APPPROTOPATH = $"{Application.streamingAssetsPath}/Config/CompanyProto.json";

        // 当前企业的索引
        public static int CompanyIndex = 0;

        /*
         * 注册表协议
         */
        public static readonly string KEY_USERCOUNTCACHE = "KEY_USERCOUNTCACHE"; //用户名密码缓存可以
        public static readonly string KEY_APPPROTOREADCACHE = "KEY_APPPROTOREADCACHE"; //用户协议读取缓存
    }
}