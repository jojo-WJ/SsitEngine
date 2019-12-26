/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/9 17:35:46                     
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using Framework.Helper;
using Framework.Logic;
using Framework.Service;
using SsitEngine.Core;
using SsitEngine.Data;
using SsitEngine.DebugLog;
using SsitEngine.Unity.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Logger = Tabtoy.Logger;
using ConfigManager = Framework.Config.ConfigManager;
namespace Framework.Data
{
    public enum EnLocalDataType
    {
        /// <summary>
        /// 道具
        /// </summary>
        DATA_ITEM = 1000,

        /// <summary>
        /// 玩家
        /// </summary>
        DATA_PLAYER = 1001,

        /// <summary>
        /// Npc
        /// </summary>
        DATA_NPC = 1002,

        /// <summary>
        ///应急措施
        /// </summary>
        DATA_SKILL = 1003,

        /// <summary>
        /// 场景
        /// </summary>
        DATA_SCENE = 1004,

        /// <summary>
        /// 资源
        /// </summary>
        DATA_Res = 1005,

        /// <summary>
        /// UI
        /// </summary>
        DATA_UI = 1006,

        /// <summary>
        /// Texture
        /// </summary>
        DATA_TEXTURE = 1007,

        /// <summary>
        /// 文件
        /// </summary>
        DATA_TEXTFILE = 1008,

        /// <summary>
        /// 图集
        /// </summary>
        DATA_ATLAS = 1009,

        /// <summary>
        /// 文本表
        /// </summary>
        DATA_STRING = 1011,

        /// <summary>
        /// 引导
        /// </summary>
        DATA_GUIDE = 1014,

        /// <summary>
        /// 活动
        /// </summary>
        DATA_ACTIVITY = 1015,


        DATA_COMPANY = 1016,

        DATA_GLOABLVALUE = 1017,

        //...往后加
        DATA_ACCIDENT = 2000,

        //工具箱 工具类别
        DATA_ToolBox = 2001,
        DATA_IndividualProtection,

        DATA_MEDIUM,
        DATA_INVEQUIP,

        DATA_BACKGROUNDITEM,

        //应急力量 模型
        DATA_EMERGENCYDODEL
    }

    public class JsonLoader : DataLoader
    {
        public const string DefaultComponeyName = "DefaultComponeyName";

        private static readonly string DataPath_Resources = "JsonData/";

        public int allResCount;

        public int curLoadCount;

        public override float LoadPrecent => (float) curLoadCount / allResCount;

        private static string DataPath_Config
        {
            get
            {
#if !UNITY_STANDALONE || !UNITY_STANDALONE_WIN
                var dataConfigPath = Application.persistentDataPath;
#else
                string dataConfigPath = Application.dataPath;
#endif
                return dataConfigPath.Substring(0, dataConfigPath.LastIndexOf("/")) + "/";
            }
        }

        /// <summary>
        /// 初始化数据代理
        /// </summary>
        /// <param name="dataManager"></param>
        /// <returns></returns>
        public override Dictionary<int, IDataProxy> InitProxy( IDataManager dataManager )
        {
            var retProxyDic = new Dictionary<int, IDataProxy>();

            // 通用表加载
            retProxyDic.Add((int) EnLocalDataType.DATA_Res, new DataResourceProxy(dataManager));
            retProxyDic.Add((int) EnLocalDataType.DATA_UI, new DataUIProxy(dataManager));
            retProxyDic.Add((int) EnLocalDataType.DATA_TEXTURE, new DataTextureProxy(dataManager));
            retProxyDic.Add((int) EnLocalDataType.DATA_ATLAS, new DataAtlasProxy(dataManager));
            retProxyDic.Add((int) EnLocalDataType.DATA_STRING, new DataContentProxy(dataManager));
            retProxyDic.Add((int) EnLocalDataType.DATA_GLOABLVALUE, new DataGlobalValueProxy(dataManager));
//
            retProxyDic.Add((int) EnLocalDataType.DATA_SCENE, new DataSceneProxy(dataManager));
            retProxyDic.Add((int) EnLocalDataType.DATA_SKILL, new DataSkillProxy(dataManager));
            retProxyDic.Add((int) EnLocalDataType.DATA_COMPANY, new DataCompanyProxy(dataManager));
            retProxyDic.Add((int) EnLocalDataType.DATA_ACCIDENT, new DataAccidentProxy(dataManager));
            retProxyDic.Add((int) EnLocalDataType.DATA_BACKGROUNDITEM, new DataBackgroundItemProxy(dataManager));
            retProxyDic.Add((int) EnLocalDataType.DATA_ITEM, new DataItemProxy(dataManager));
            retProxyDic.Add((int) EnLocalDataType.DATA_INVEQUIP, new DataInvEquipProxy(dataManager));
            retProxyDic.Add((int) EnLocalDataType.DATA_EMERGENCYDODEL, new DataEmergencyModelProxy(dataManager));

            /*
            LoadAndInitModel<ToolBoxModel>(ConstTableName.ToolBox);
            LoadAndInitModel<TaskTypeModel>(ConstTableName.TaskType);
            LoadAndInitModel<TaskContentModel>(ConstTableName.TaskContent);
            LoadAndInitModel<ProtectionsBaseModel>(ConstTableName.ProtectionsBase);
            LoadAndInitModel<QuickChatBaseModel>(ConstTableName.QuickChatBase);
            LoadAndInitModel<TelReportBaseModel>(ConstTableName.TelReportBase);
            LoadAndInitModel<DeviceBaseModel>(ConstTableName.DeviceBase);
            LoadAndInitModel<PlayerBaseModel>(ConstTableName.PlayerBase);
            LoadAndInitModel<ForceUintBaseModel>(ConstTableName.ForceUintBase);
            LoadAndInitModel<TaskBatchModel>(ConstTableName.TaskBatch);
            */


            return retProxyDic;
        }

        public override string GetFileExt()
        {
            return ".json";
        }


        public override List<T> Load<T>( string path, bool isFlag = false )
        {
            if (isFlag) return LoadJsonData<T>(path + GetFileExt(), new List<string> {"CLSH"});
            return LoadJsonData<T>(path + GetFileExt());
        }

        public override void Load<T>( string path, bool isFlag, SsitAction<Dictionary<string, T>> func )
        {
            if (isFlag)
                LoadJsonData(path, func, ConfigManager.Instance.VersionConfig.Companys);
            else
                LoadJsonData(path, func);
        }


        public override string Save<T>( Dictionary<int, DataBase> list )
        {
            return null;
        }

        public List<T> LoadJsonData<T>( string fileName, List<string> folder = null ) where T : ModelBase
        {
            var ret = new List<T>();
            if (folder != null && folder.Count > 0)
            {
                IEnumerator<string> iEnumerator = folder.GetEnumerator();

                while (iEnumerator.MoveNext())
                {
                    var path = string.Format("{0}{1}/{2}", DataPath_Resources, iEnumerator.Current, fileName);
                    T t = null;

                    if (t == null)
                    {
                        /*string text = ResourcesManager.Instance.LoadTextAsset(path);

                        if (string.IsNullOrEmpty(text))
                        {
                            //SsitDebug.Warning("Json文本异常：文本无内容 " + path);
                            continue;
                        }

                        t = JsonUtility.FromJson<T>(text);*/
                    }
                    t.Deserialized(t);
                    ret.Add(t);
                }
            }

            if (ret.Count == 0)
            {
                var path = DataPath_Resources + fileName;
                T t = null;
                if (t == null)
                {
                    /*string text = ResourcesManager.Instance.LoadTextAsset(path);
                    if (string.IsNullOrEmpty(text))
                    {
                        SsitDebug.Error("Json配置错误：文本无内容 " + path);
                        return null;
                    }

                    t = JsonUtility.FromJson<T>(text);*/
                }

                t.Deserialized(t);
                ret.Add(t);
            }
            if (ret.Count == 0) Logger.Instance().ErrorLine("table data is null: {0}", fileName);
            return ret;
        }

        public void LoadJsonData<T>( string fileName, SsitAction<Dictionary<string, T>> func,
            List<string> folder = null ) where T : ModelBase
        {
            if (folder != null)
                SyncLoaderText(fileName, folder, func);
            else
                SyncLoaderText(fileName, func);
        }

        #region C#5.0 Sync 实现

        private async void LoaderText<T>( string fileName, SsitAction<Dictionary<string, T>> complete )
            where T : ModelBase
        {
            var ret = new Dictionary<string, T>();
            var path = $"{DataPath_Resources}{fileName}";
            var handler = Addressables.LoadAssetAsync<TextAsset>(path);
            var tt = await handler;
            if (tt != null)
            {
                var data = JsonUtility.FromJson<T>(tt.text);
                //Debug.Log($"result:: {tt.text}");
                data.Deserialized(data);
                ret.Add(DefaultComponeyName, data);
            }
            else
            {
                SsitDebug.Error($"数据加载异常 {path}");
            }
            Addressables.Release(handler);
            complete?.Invoke(ret);

            /*var handler = Addressables.DownloadDependenciesAsync("Table", true);

            handler.Completed += handle =>
            {
                Debug.Log($"handle{handle.Result}");
            };*/
        }

        private async void LoaderText<T>( string fileName, List<string> resPath,
            SsitAction<Dictionary<string, T>> complete )
            where T : ModelBase
        {
            var ret = new Dictionary<string, T>();
            foreach (var item in resPath)
            {
                var path = $"{DataPath_Resources}{item}/{fileName}";
                var handler = Addressables.LoadAssetAsync<TextAsset>(path);
                var tt = await handler;
                var data = JsonUtility.FromJson<T>(tt.text);
                if (data != null)
                {
                    //Debug.Log($"result:: {tt.text}");
                    data.Deserialized(data);
                    ret.Add(item, data);
                }
                else
                {
                    SsitDebug.Error($"数据加载异常 {path}");
                }
                Addressables.Release(handler);
            }
            complete?.Invoke(ret);
        }

        #endregion

        #region 回调实现

        private void SyncLoaderText<T>( string fileName, SsitAction<Dictionary<string, T>> complete )
            where T : ModelBase
        {
            var ret = new Dictionary<string, T>();
            var path = $"{DataPath_Resources}{fileName}";
            var handler = Addressables.LoadAssetAsync<TextAsset>(path);
            allResCount++;
            handler.Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    var result = handle.Result.text;
                    var data = JsonUtility.FromJson<T>(result);
                    //Debug.Log($"result:: {tt.text}");
                    data.Deserialized(data);
                    ret.Add(DefaultComponeyName, data);
                    curLoadCount++;
                }
                else
                {
                    SsitDebug.Error($"数据加载异常 {path}");
                }
                Addressables.Release(handler);
                complete?.Invoke(ret);
            };
        }

        private void SyncLoaderText<T>( string fileName, List<string> resPath,
            SsitAction<Dictionary<string, T>> complete )
            where T : ModelBase
        {
            var ret = new Dictionary<string, T>();
            foreach (var item in resPath)
            {
                var path = $"{DataPath_Resources}{item}/{fileName}";
                var tt = Addressables.LoadAssetAsync<TextAsset>(path);
                allResCount++;
                tt.Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        var data = JsonUtility.FromJson<T>(tt.Result.text);
                        curLoadCount++;
                        //Debug.Log($"result:: {tt.text}");
                        data.Deserialized(data);
                        ret.Add(item, data);
                        Addressables.Release(handle);
                        complete?.Invoke(ret);
                    }
                    else
                    {
                        SsitDebug.Error($"数据加载异常 {path}");
                    }
                };
            }
        }

        #endregion
    }
}