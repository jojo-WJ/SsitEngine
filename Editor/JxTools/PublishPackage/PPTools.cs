/*using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public class PPTools : EditorWindow
{
    private static readonly string COPYFILEPATH = "/Packages";
    private static readonly string TARGETFOLDERPATH = "/Resources/Packages";
    private static readonly string CONFIGPATH = "Assets/Editor/MyTools/PublishPackage";
    private static readonly string CONGFIGASSETNAME = "GamePublishConfig.asset";


    //Config
    private static PublishConfigInfo mConfigInfo;
    private static EnumPlat mCurEnumPlat = EnumPlat.None;
    private static EnumSDK mCurEnumSDK = EnumSDK.None;
    private static bool mCanPublish;
    private float mButtonHeight = 60;

    private float mButtonWidth = 200;
    //private static PublishConfig mConfigInfo;

    //interval variable
    private string mCurPlat;
    private string mCurSdk;

    private Dictionary<string, object> mFielDic;

    private readonly bool mIsHelper = true;
    //static string CONGFIGASSETNAME = "GamePublishConfig.json";

    private readonly string[] mPlats =
    {
        "Pc",
        "Ios",
        "Andirod"
    };

    public Vector2 mScrollPos;

    private readonly string[] mSdks =
    {
        "Aone",
        "微信",
        "QQ",
        "小米"
    };

    private void Awake()
    {
        mButtonWidth = EditorGUIUtility.fieldWidth * 2;
        mButtonHeight = EditorGUIUtility.singleLineHeight * 2;

        mConfigInfo = AssetDatabase.LoadAssetAtPath<PublishConfigInfo>(Path.Combine(CONFIGPATH, CONGFIGASSETNAME));
        //mConfigInfo = DataConverterTool<PublishConfig>.Read(Path.Combine(CONFIGPATH, CONGFIGASSETNAME));
    }

    private void OnEnable()
    {
        if (mConfigInfo == null)
        {
            mConfigInfo = AssetDatabase.LoadAssetAtPath<PublishConfigInfo>(Path.Combine(CONFIGPATH, CONGFIGASSETNAME));
            //mConfigInfo = DataConverterTool<PublishConfig>.Read(Path.Combine(CONFIGPATH, CONGFIGASSETNAME));
        }

        mFielDic = new Dictionary<string, object>();
    }

    private void OnDisable()
    {
        mFielDic.Clear();
        mFielDic = null;

        mCurPlat = null;
        mCurSdk = null;
        mCurEnumPlat = EnumPlat.None;
        mCurEnumSDK = EnumSDK.None;
        mCanPublish = false;
    }

    //[MenuItem("Tools/Pulish package/Pulish Window #&o", false, 1)]
    private static void InitWindow()
    {
        var mWindow = GetWindow(typeof(PPTools));
    }

    private void OnGUI()
    {
        mScrollPos = GUILayout.BeginScrollView(mScrollPos);
        {
            EditorGUILayout.BeginVertical();
            {
                //绘制刷新（文件移动）
                DrawFileFresh();

                //绘制平台（平台选择）
                DrawPlats();

                //绘制SDK（SKD选择打包）
                DrawSDKs();
            }
            EditorGUILayout.EndVertical();
        }
        GUILayout.EndScrollView();

        //CheckState();
    }

    private void CheckState()
    {
        if (mCurEnumPlat != EnumPlat.None)
        {
            mCanPublish = true;
        }
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }


    #region 资源卸载

    private static void UnloadPackageAssets( string folder )
    {
        var allAssetPath = AssetDatabase.GetAllAssetPaths();
        foreach (var path in allAssetPath)
        {
            if (!path.Contains(folder))
            {
                continue;
            }
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null)
            {
                continue;
            }
            //报错？？不能用【只能卸载Asset/component失败】
            Resources.UnloadAsset(asset);
        }
    }

    #endregion

    private enum EnumPlat
    {
        None,
        Pc,
        Ios,
        Andirod
    }

    private enum EnumSDK
    {
        None,
        Aone,
        微信,
        QQ,
        小米
    }

    #region Draw window

    private void DrawFileFresh()
    {
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        EditorGUI.indentLevel = 10;
        HeaderLable("Refresh Package");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal("AnimationCurveEditorBackground",
            GUILayout.Height(EditorGUIUtility.fieldWidth));
        EditorGUI.indentLevel = 2;
        var rect = EditorGUILayout.GetControlRect();
        GUI.enabled = mCanPublish;
        var refresh = "Refresh";
        if (GUI.Button(new Rect(rect.x, rect.y, mButtonWidth, mButtonHeight), refresh))
        {
            if (mCurEnumPlat != EnumPlat.None)
            {
                EditorGUI.BeginChangeCheck();
                var path = "";
                switch (mCurEnumPlat)
                {
                    case EnumPlat.Pc:
                    {
                        mConfigInfo.mConfig.pcConfig.OutPath = EditorUtility.SaveFilePanel("存储位置",
                            Application.dataPath, "魂迹", "apk");
                        path = mConfigInfo.mConfig.pcConfig.OutPath;
                        break;
                    }
                    case EnumPlat.Ios:
                    {
                        mConfigInfo.mConfig.iosConfig.OutPath = EditorUtility.SaveFilePanel("存储位置",
                            Application.dataPath, "魂迹", "apk");
                        path = mConfigInfo.mConfig.iosConfig.OutPath;
                        break;
                    }
                    case EnumPlat.Andirod:
                    {
                        mConfigInfo.mConfig.andriodConfig.OutPath = EditorUtility.SaveFilePanel("存储位置",
                            Application.dataPath, "魂迹", "apk");
                        path = mConfigInfo.mConfig.andriodConfig.OutPath;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(path))
                {
                    Debug.Log("路径确认");
                    OnDrawFresh(refresh);
                }
            }
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }


    private void DrawPlats()
    {
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        HeaderLable("Select Plat");

        EditorGUILayout.EndHorizontal();

        float curWidth = Screen.width;

        var row = Mathf.FloorToInt(curWidth / mButtonWidth);
        var count = 0;
        for (var i = 0; i < mPlats.Length; i += row, count++)
        {
            EditorGUILayout.BeginHorizontal("AnimationCurveEditorBackground",
                GUILayout.Height(EditorGUIUtility.fieldWidth));
            var rect = EditorGUILayout.GetControlRect();
            for (var j = 0; j < row; j++)
            {
                var curIndex = row * count + j;
                if (curIndex >= mPlats.Length)
                {
                    break;
                }
                var curButtonSytle = "button";
                if (!string.IsNullOrEmpty(mCurPlat) && mCurPlat.Equals(mPlats[curIndex]))
                {
                    curButtonSytle = "TL SelectionButton PreDropGlow";
                }
                if (GUI.Button(new Rect(rect.x + j * mButtonWidth, rect.y, mButtonWidth, mButtonHeight),
                    mPlats[curIndex], new GUIStyle(curButtonSytle)))
                {
                    OnDrawPlat(mPlats[curIndex]);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawSDKs()
    {
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        HeaderLable("Select SDK");

        EditorGUILayout.EndHorizontal();

        float curWidth = Screen.width;

        var row = Mathf.FloorToInt(curWidth / mButtonWidth);
        var count = 0;
        for (var i = 0; i < mSdks.Length; i += row, count++)
        {
            EditorGUILayout.BeginHorizontal("AnimationCurveEditorBackground",
                GUILayout.Height(EditorGUIUtility.fieldWidth));
            var rect = EditorGUILayout.GetControlRect();
            for (var j = 0; j < row; j++)
            {
                var curIndex = row * count + j;
                if (curIndex >= mSdks.Length)
                {
                    break;
                }
                var curButtonSytle = "button";
                if (!string.IsNullOrEmpty(mCurSdk) && mCurSdk.Equals(mSdks[curIndex]))
                {
                    curButtonSytle = "TL SelectionButton PreDropGlow";
                }
                if (GUI.Button(new Rect(rect.x + j * mButtonWidth, rect.y, mButtonWidth, mButtonHeight),
                    mSdks[curIndex], new GUIStyle(curButtonSytle)))
                {
                    OnDrawSdk(mSdks[curIndex]);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    #endregion

    #region 绘制回调

    private void OnDrawFresh( string flag )
    {
        Debug.Log("点击" + flag);

        switch (mCurEnumPlat)
        {
            case EnumPlat.Pc:
            {
                BuildPCGamePackage();
                break;
            }
            case EnumPlat.Andirod:
            {
                BuildAndriodGamePackage();
                break;
            }
        }
    }

    private void OnDrawPlat( string flag )
    {
        mCurPlat = flag;
        mCurEnumPlat = (EnumPlat) Enum.Parse(typeof(EnumPlat), mCurPlat, true);


        SwitchPlat();
    }

    private void OnDrawSdk( string flag )
    {
        mCurSdk = flag;
        mCurEnumSDK = (EnumSDK) Enum.Parse(typeof(EnumSDK), mCurSdk, true);
    }

    #endregion

    #region Publish

    private static void SwitchPlat()
    {
        var curTarget = EditorUserBuildSettings.activeBuildTarget;
        switch (mCurEnumPlat)
        {
            case EnumPlat.Pc:
            {
                if (curTarget == BuildTarget.StandaloneWindows)
                {
                    mCanPublish = true;
                    Debug.Log("平台确认");

                    return;
                }

                EditorUserBuildSettings.activeBuildTargetChanged = delegate
                {
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows)
                    {
                        mCanPublish = true;
                        Debug.Log("平台转换完成");
                    }
                };
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneWindows);
                break;
            }
            case EnumPlat.Ios:
            {
                break;
            }
            case EnumPlat.Andirod:
            {
                if (curTarget == BuildTarget.Android)
                {
                    mCanPublish = true;
                    Debug.Log("平台确认");

                    return;
                }

                EditorUserBuildSettings.activeBuildTargetChanged = delegate
                {
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                    {
                        mCanPublish = true;
                        Debug.Log("平台转换完成");
                    }
                };
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
                break;
            }
        }
    }

    private static void BuildPCGamePackage()
    {
        MovePackage();
    }

    private static void BuildAndriodGamePackage()
    {
        MovePackage();
    }

    private static void PPBegain()
    {
        //卸载回调
//        UnityAssetsOperatorCompilingListener.OnFileOperatorEvent -= PPBegain;

        //注册完成回调
//        UnityAssetsOperatorCompilingListener.OnBuildCompleteEvent -= OnBuildComplete;
//        UnityAssetsOperatorCompilingListener.OnBuildCompleteEvent += OnBuildComplete;

        Debug.Log("注册完成");

        //设置配置


        var path = SetBwPlayerSetting();

        //Debug.LogError("这是打完之后的包,存放的路径:" + path);
        //选择平台
        BuildTarget target;
#if (UNITY_EDITOR_WIN)
        {
            target = BuildTarget.StandaloneWindows;
        }
#else
        {
           target = BuildTarget.StandaloneOSXIntel;
        }
#endif

        switch (mCurEnumPlat)
        {
            case EnumPlat.Pc:
                break;
            case EnumPlat.Ios:
                target = BuildTarget.iOS;
                break;
            case EnumPlat.Andirod:
                target = BuildTarget.Android;
                break;
        }

        BuildPipeline.BuildPlayer(GetBuildScenes(), path, target, BuildOptions.None); //开始进行打包......
    }

    #endregion


    #region MenuItem

    [MenuItem("Assets/Create/Publish Asset")]
    private static void Execute()
    {
        //实例化SysData               

        var sd = CreateInstance<PublishConfigInfo>();

        //随便设置一些数据给content                 


        // SysData将创建为一个对象，这时在project面板上会看到这个对象。                 

//        UnityEngine.Object[] curFolder = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.TopLevel);
        var path = CONFIGPATH;

//        if (curFolder.Length > 0)
//        {
//            path = AssetDatabase.GetAssetPath(curFolder[0]);
//        }
        var fileName = CONGFIGASSETNAME;
        path = Path.Combine(path, fileName);

        AssetDatabase.CreateAsset(sd, path);
        AssetDatabase.Refresh();
        //        Object o = AssetDatabase.LoadAssetAtPath(p, typeof(SysData));

        //打包为SysData.assetbundle文件。                 

        //        BuildPipeline.BuildAssetBundle(o, null, "SysData.assetbundle");

        //删除面板上的那个临时对象               

        //AssetDatabase.DeleteAsset(p);
    }

    [MenuItem("Tools/Pulish package/Move Package")]
    private static void MovePackageFolder()
    {
        if (Directory.Exists(EditorFileUtility.DataPathInEditor + COPYFILEPATH))
        {
            if (Directory.Exists(EditorFileUtility.DataPathInEditor + TARGETFOLDERPATH))
            {
                FileUtil.DeleteFileOrDirectory(EditorFileUtility.DataPathInEditor + TARGETFOLDERPATH);
            }

            //移动【亲测：文件移动会发生异常】
            //FileUtil.MoveFileOrDirectory();
            AssetDatabase.MoveAsset("Assets" + COPYFILEPATH,
                "Assets" + TARGETFOLDERPATH);

            //回调
//            UnityAssetsOperatorCompilingListener.ResetOnFileOperatorEvent();
//            UnityAssetsOperatorCompilingListener.OnFileOperatorEvent += OnMoveFolderPackage;
        }
        else
        {
            Debug.Log(EditorFileUtility.DataPathInEditor + COPYFILEPATH + "不存在");
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    //[MenuItem("Tools/Pulish package/Reset Package")]
    private static void ResetPackageFolder()
    {
        if (Directory.Exists(EditorFileUtility.DataPathInEditor + TARGETFOLDERPATH))
        {
            if (Directory.Exists(EditorFileUtility.DataPathInEditor + COPYFILEPATH))
            {
                FileUtil.DeleteFileOrDirectory(EditorFileUtility.DataPathInEditor + COPYFILEPATH);
            }

            //文件位置还原
            /*FileUtil.MoveFileOrDirectory(EditorFileUtility.DataPathInEditor + TARGETFOLDERPATH,
                EditorFileUtility.DataPathInEditor + COPYFILEPATH);#1#
            AssetDatabase.MoveAsset("Assets" + TARGETFOLDERPATH,
                "Assets" + COPYFILEPATH);
        }
        else
        {
            Debug.Log(EditorFileUtility.DataPathInEditor + TARGETFOLDERPATH + "不存在");
            return;
        }
        AssetDatabase.Refresh();
    }

    #endregion


    #region 配置加载

    [MenuItem("Tools/Pulish package/LoadLocalConfig", false, 2)]
    private static void LoadLocalConfig()
    {
        var configInfo = AssetDatabase.LoadAssetAtPath<PublishConfigInfo>(Path.Combine(CONFIGPATH, CONGFIGASSETNAME));

        configInfo.mConfig.baseConfig.CompanyName = PlayerSettings.companyName;
        configInfo.mConfig.baseConfig.ProductName = PlayerSettings.productName;


        var curTarget = EditorUserBuildSettings.activeBuildTarget;
        Debug.Log(curTarget);

        switch (curTarget)
        {
            case BuildTarget.StandaloneLinuxUniversal:
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneWindows:
            {
                break;
            }
            case BuildTarget.iOS:
            {
                break;
            }
            case BuildTarget.Android:
            {
                var curConfig = configInfo.mConfig.andriodConfig;
                curConfig.ScriptingDefine =
                    PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
                curConfig.ApplicationIdentifier = PlayerSettings.applicationIdentifier;
                curConfig.BundleVersion = PlayerSettings.bundleVersion;

                curConfig.KeystorePass = PlayerSettings.keystorePass;

                curConfig.BundleVersionCode = PlayerSettings.Android.bundleVersionCode;
                curConfig.KeystoreName = PlayerSettings.Android.keystoreName;

                break;
            }
        }
    }

    private static string SetBwPlayerSetting()
    {
        var path = "";

        PlayerSettings.companyName = mConfigInfo.mConfig.baseConfig.CompanyName;
        PlayerSettings.productName = mConfigInfo.mConfig.baseConfig.ProductName;


        switch (mCurEnumPlat)
        {
            case EnumPlat.Pc:
            {
                var curConfig = mConfigInfo.mConfig.pcConfig;
                path = curConfig.OutPath;
                break;
            }
            case EnumPlat.Ios:
            {
                var curConfig = mConfigInfo.mConfig.iosConfig;
                path = curConfig.OutPath;
                break;
            }
            case EnumPlat.Andirod:
            {
                var curConfig = mConfigInfo.mConfig.andriodConfig;

                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android,
                    curConfig.ScriptingDefine); //设置宏,打包的为Android

                PlayerSettings.applicationIdentifier = curConfig.ApplicationIdentifier; // "com.playbyone.chyz.bw";
                PlayerSettings.bundleVersion = curConfig.BundleVersion;
                PlayerSettings.Android.bundleVersionCode = ++curConfig.BundleVersionCode;

                PlayerSettings.Android.keystoreName = curConfig.KeystoreName;
                PlayerSettings.keystorePass = curConfig.KeystorePass;

                path = curConfig.OutPath;
                break;
            }
        }
        return path;
    }

    private static void MovePackage()
    {
        if (Directory.Exists(EditorFileUtility.DataPathInEditor + COPYFILEPATH))
        {
            if (Directory.Exists(EditorFileUtility.DataPathInEditor + TARGETFOLDERPATH))
            {
                FileUtil.DeleteFileOrDirectory(EditorFileUtility.DataPathInEditor + TARGETFOLDERPATH);
            }
            //打包回调
//            UnityAssetsOperatorCompilingListener.ResetOnFileOperatorEvent();
//            UnityAssetsOperatorCompilingListener.OnFileOperatorEvent += PPBegain;

            //移动
            AssetDatabase.MoveAsset("Assets" + COPYFILEPATH,
                "Assets" + TARGETFOLDERPATH);
        }
        else
        {
            Debug.Log(EditorFileUtility.DataPathInEditor + COPYFILEPATH + "不存在");
        }
        AssetDatabase.Refresh();
    }

    #endregion

    #region Postprocess事件监听回调

    private static void OnMoveFolderPackage()
    {
        Debug.Log("文件导入完成：：OnPublishPackage");
        //AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
        mCanPublish = true;
//        UnityAssetsOperatorCompilingListener.OnFileOperatorEvent -= OnMoveFolderPackage;
    }

    private static void OnBuildComplete()
    {
//        UnityAssetsOperatorCompilingListener.OnBuildCompleteEvent -= OnBuildComplete;
        //文件位置还原
        ResetPackageFolder();
        Debug.Log("文件还原中");
    }

    #endregion


    #region Interval helper

    /// <summary>
    /// 获取选定的Scene
    /// </summary>
    /// <returns></returns>
    private static string[] GetBuildScenes()
    {
        var names = new List<string>();
        foreach (var e in EditorBuildSettings.scenes)
        {
            if (e == null)
            {
                continue;
            }
            if (e.enabled)
            {
                names.Add(e.path);
            }
        }
        //Debug.LogError("添加的场景的个数为:" + names.Count);
        return names.ToArray();
    }

    private static void LoadConfigInfo()
    {
        /*using (StringReader rdr = new StringReader()
        {
            //声明序列化对象实例serializer
            XmlSerializer serializer = new XmlSerializer(typeof(int));
            //反序列化，并将反序列化结果值赋给变量i
            int i = (int)serializer.Deserialize(rdr);
            //输出反序列化结果
        }#1#
    }

    private static void SaveConfigInfo()
    {
        //序列化这个对象
        var serializer = new XmlSerializer(typeof(PublishConfig));

        //将对象序列化输出到控制台
        //serializer.Serialize(,);
    }

    private void CheckButtonValue( string fieldName, Action<bool> action )
    {
        if (action != null)
        {
            action(GetFieldByFieldName<bool>(fieldName, false));
        }
    }

    private T GetFieldByFieldName<T>( string fieldName, object defaultValue )
    {
        if (string.IsNullOrEmpty(fieldName))
        {
            return default;
        }
        if (mFielDic.ContainsKey(fieldName))
        {
            return (T) mFielDic[fieldName];
        }
        mFielDic.Add(fieldName, defaultValue);
        return (T) mFielDic[fieldName];
    }

    private void SetFieldByFeildName( string fieldName, object value )
    {
        if (string.IsNullOrEmpty(fieldName))
        {
            return;
        }
        if (mFielDic.ContainsKey(fieldName))
        {
            mFielDic[fieldName] = value;
        }
        else
        {
            mFielDic.Add(fieldName, value);
        }
    }

    private void HeaderLable( string message )
    {
        if (!mIsHelper)
        {
            return;
        }
        GUI.color = Color.cyan;
        //GUILayout.Space(Screen.width / 2 - EditorGUIUtility.fieldWidth * 5);
        GUILayout.Label(message, "label" /*, new GUILayoutOption[]
        {
            GUILayout.ExpandWidth(true)
        }#1#);
        GUI.color = Color.white;
    }

    private void HelpLable( string message )
    {
        if (!mIsHelper)
        {
            return;
        }
        GUI.color = Color.yellow;
        GUILayout.Label(message, "label");
        GUI.color = Color.white;
    }

    #endregion
}

/*[XmlRoot]
public class Config
{
    public GameEngineConfig baseConfig;

    public PcConfig pcConfig;

    public AndriodConfig andriodConfig;
}
[XmlRoot]
public class GameEngineConfig
{
    [XmlAttribute("companyName")]
    public string CompanyName { get; set; }
    [XmlAttribute("productName")]
    public string ProductName { get; set; }
}
[XmlRoot]
public class PcConfig
{

}
[XmlRoot]
public class AndriodConfig
{
    [XmlAttribute("outPath")]
    public string OutPath { get; set; }
}#1#*/