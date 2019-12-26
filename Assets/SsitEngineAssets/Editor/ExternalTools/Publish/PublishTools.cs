/*
*┌──────────────────────────────────────────────────────────────┐
*│　描    述：                                                    
*│　作    者：Xuxin                                              
*│　版    本：1.0.0                                                 
*│　创建时间：2019/12/26 14:26:16                             
*└──────────────────────────────────────────────────────────────┘
*/
using System;
using System.Collections.Generic;
using System.IO;
using Framework.Config;
using SsitEngine.Editor;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    [Serializable]
	public class CompanyTableModel
	{
		/// <summary> 
		/// Company
		/// </summary>
		public List<CompanyDefine> Company = new List<CompanyDefine>(); 
	
	 	Dictionary<int, CompanyDefine> _CompanyByCompanyID = new Dictionary<int, CompanyDefine>();

	} 

	// Defined in table: Company
    [Serializable]
	public  class CompanyDefine
	{
	
		
		/// <summary> 
		/// 企业ID
		/// </summary>
		public int CompanyID = 0; 
		
		/// <summary> 
		/// 企业名称
		/// </summary>
		public string CompanyName = ""; 
		
		/// <summary> 
		/// 企业描述
		/// </summary>
		public string CompanyDesc = ""; 
		
		/// <summary> 
		/// 企业标识符
		/// </summary>
		public string CompanyIdentifier = ""; 
		
		/// <summary> 
		/// 前台登录背景
		/// </summary>
		public string LoginBgPath = ""; 
		
		/// <summary> 
		/// 前台大厅背景
		/// </summary>
		public string LobbyBgPath = ""; 
		
		/// <summary> 
		/// 后台事故设定背景
		/// </summary>
		public string SettingFormBgPath = ""; 
	
	

	} 
    
    public class PublishTools : EditorWindow
    {
        public static readonly string CompanyTablePath = "CompanyTable.json";

        public Vector2 mScrollPos;
        private float mButtonHeight = 60;
        private float mButtonWidth = 200;
        private readonly bool mIsHelper = true;

        private static EditorWindow m_window;

        //interval variable
        
        private string[] mPlats;
        private string mCurPlat;
        
        
        [MenuItem("Tools/SsitEngine/Pulish Window #&l", false, 1)]
        private static void InitWindow()
        {
            m_window = GetWindow(typeof(PublishTools));
        }

        private void Awake()
        {
            mButtonWidth = EditorGUIUtility.fieldWidth * 2;
            mButtonHeight = EditorGUIUtility.singleLineHeight * 2;

            //mConfigInfo = AssetDatabase.LoadAssetAtPath<PublishConfigInfo>(Path.Combine(CONFIGPATH, CONGFIGASSETNAME));
            //mConfigInfo = DataConverterTool<PublishConfig>.Read(Path.Combine(CONFIGPATH, CONGFIGASSETNAME));
        }

        private void OnEnable()
        {
            var tempPath = Path.Combine(EditorFileUtility.CONFIGPATH, EditorFileUtility.CONGFIGASSETNAME);

            if (!File.Exists(tempPath))
            {
                m_window.Close();

                if (EditorUtility.DisplayDialog("发布提示", "表格配置文件不存在", "确定"))
                {
                }
            }
            else
            {
                var configInfo = AssetDatabase.LoadAssetAtPath<TableFileScrips>(tempPath);

                if (configInfo)
                {
                    var jsonPath = configInfo.JsonExportPath;

                    //加载场景表
                    var json = ObjectToFileTools<CompanyTableModel>.ReadJson($"{jsonPath}/{CompanyTablePath}");

                    if (json != null)
                    {
	                    mPlats = json.Company.ConvertAll(x => x.CompanyIdentifier).ToArray();
                    }
                    else
                    {
                        m_window.Close();
                        EditorUtility.DisplayDialog("发布提示", "企业配置Json文件不存在", "确定");
                        return;
                    }
                }
                AssetDatabase.LoadAssetAtPath<TextAsset>("");
            }
        }


        private void OnGUI()
        {
            mScrollPos = GUILayout.BeginScrollView(mScrollPos);
            {
                EditorGUILayout.BeginVertical();
                {
                    //绘制刷新（文件移动）
                    //DrawFileFresh();

                    //绘制企业（平台选择）
                    DrawPlats();

                    //绘制SDK（SKD选择打包）
                    //DrawSDKs();
                }
                EditorGUILayout.EndVertical();
            }
            GUILayout.EndScrollView();

            //CheckState();
        }

        private void DrawPlats()
        {
	        if (mPlats == null)
	        {
		        return;
	        }
	        
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            HeaderLable("Select Plat");

            EditorGUILayout.EndHorizontal();

            float curWidth = Screen.width;

            var row = Mathf.FloorToInt(curWidth / mButtonWidth);
            var count = 0;
            for (var i = 0; i < mPlats.Length; i += row, count++)
            {
                EditorGUILayout.BeginHorizontal("CN StacktraceBackground",GUILayout.Height(EditorGUIUtility.fieldWidth));
                var rect = EditorGUILayout.GetControlRect();
                for (var j = 0; j < row; j++)
                {
                    var curIndex = row * count + j;
                    if (curIndex >= mPlats.Length)
                    {
                        break;
                    }
               
                    if (!string.IsNullOrEmpty(mCurPlat) && mCurPlat.Equals(mPlats[curIndex]))
                    {
	                    GUI.color = Color.cyan;
                    }
                    if (GUI.Button(new Rect(rect.x + j * mButtonWidth, rect.y, mButtonWidth, mButtonHeight),
                        mPlats[curIndex], new GUIStyle("button")))
                    {
                        OnDrawPlat(mPlats[curIndex]);
                    }
                    GUI.color = Color.white;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void OnDrawPlat( string flag )
        {
            mCurPlat = flag;
            ExportConfig(mCurPlat);
            Build(false);
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
            }*/);
            GUI.color = Color.white;
        }

        #region 配置写入

        private static AppVersionConfig ExportConfig()
        {
#if UNITY_EDITOR

	        //string dataConfigPath = FileUtility.GetDAConfigPath( Application.dataPath );
	        string dataConfigPath = Application.dataPath + AppDefine.APPCONFIGPATH;

#else
			string dataConfigPath = Application.dataPath;
			dataConfigPath = userConfigPath.Substring(0, userConfigPath.LastIndexOf("/"));
			dataConfigPath = FileUtility.GetDAConfigPath( dataConfigPath );
#endif
	        AppVersionConfig mVersionConfigInfo = null;
	        if (!File.Exists(dataConfigPath))
	        {
		        mVersionConfigInfo = new AppVersionConfig();
		        Directory.CreateDirectory(Application.dataPath + "/Resources");
		        //mConfigInfo.hideFlags = HideFlags.DontSaveInEditor;
	        }
	        else
	        {
		        string text = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets" + AppDefine.APPCONFIGPATH).text;
		        mVersionConfigInfo = JsonUtility.FromJson<AppVersionConfig>(text);
	        }

	        return mVersionConfigInfo;
        }

        private static void ExportConfig(params string[] companyIndex )
        {
	        AppVersionConfig versionConfigInfo = ExportConfig();

	        versionConfigInfo.Companys.Clear();
	        versionConfigInfo.Companys.AddRange(companyIndex);
	        versionConfigInfo.mCompanyIndex = companyIndex[0];
	        ObjectToFileTools<AppVersionConfig>.SaveJson(Application.dataPath + AppDefine.APPCONFIGPATH, versionConfigInfo);
	        AssetDatabase.Refresh();
        }
        
        #endregion
        
        #region 发布

        public static void Build( bool isAllScene )
        {
	        string targetPath = "Publish/Emergency/";
	        
	        Directory.CreateDirectory(targetPath);
	        
	        var curTarget = EditorUserBuildSettings.activeBuildTarget;
	        
	        string curPath = EditorUtility.SaveFilePanel("发布路径", targetPath, GenMainFileName(), "exe");
	        if (curPath.Length != 0)
	        {
		        //EditorBuildSettings.
		        BuildPipeline.BuildPlayer(GetSceneList(isAllScene), curPath, curTarget, BuildOptions.None);
		        Debug.Log("Build Success");

	        }
	        
	        AssetDatabase.Refresh();
	        if (curPath.Length != 0)
	        {
		        OpenFolderAndSelectFile(curPath.Replace("/", @"\"));
	        }

        }

        private static string[] GetSceneList( bool isAll )
        {
	        List<string> sceneList = new List<string>();
	        foreach (var item in EditorBuildSettings.scenes)
	        {
		        if (isAll)
		        {
			        sceneList.Add(item.path);
			        continue;
		        }
		        if (!item.enabled)
		        {
			        continue;
		        }
	        }
	        /*foreach (var ss in sceneList)
	        {
		        Debug.Log("publish scene:: " + ss);
	        }*/
	        return sceneList.ToArray();
        }
        
        private static string GenMainFileName()
        {
	        //return PlayerSettings.productName /*+ "_" + PlayerSettings.bundleVersion.Replace( '.', '_' ) + "_" + DateTime.Now.ToString( "MM_dd_HH_mm" )*/;//yyMMddHHmmss
	        return "EmergencyDillSystem";
        }

        private static void OpenFolderAndSelectFile( String fileFullName )
        {
	        System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
	        psi.Arguments = "/e,/select," + fileFullName;
	        System.Diagnostics.Process.Start(psi);
        }
        
        #endregion
    }
}