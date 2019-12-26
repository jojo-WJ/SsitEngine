/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/11 12:07:17                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework.Data;
using Framework.Logic;
using Framework.SceneObject;
using SsitEngine.DebugLog;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Unity.Msic;
using SsitEngine.Unity.Resource;
using Table;
using UnityEngine;

namespace Framework.Scene
{
    /// <summary>
    /// 场景镜像【抽象概念：一个关卡 == 场景 + 相应预案】
    /// </summary>
    public class SchemeLevel : Level
    {
        private string m_sceFileId; // 场景Guid
        private string m_sceneGuid; // 场景Guid

        /// <summary>
        /// 场景编辑配置文件
        /// </summary>
        private SceneDataInfo m_sceneDataInfo;

        /// <summary>
        /// 预案配置文件
        /// </summary>
        private SchemeDataInfo m_schemeDataInfo;

        private string m_schemeGuid; // 预案Guid
        private string m_schFileId; // 预案Guid

        public SceneDataInfo SceneDataInfo => m_sceneDataInfo;

        public SchemeDataInfo SchemeDataInfo => m_schemeDataInfo;

        public string SchemeGuid => m_schemeGuid;

        public string SceneGuid => m_sceneGuid;

        /// <summary>
        /// 设置关卡资源
        /// </summary>
        /// <param name="param">关卡信息</param>
        public override void SetResource( LevelParams param )
        {
            m_sceneGuid = param.mSceneGuid;
            m_sceFileId = param.mSceFileId;

            m_schemeGuid = param.mSchemeGuid;
            m_schFileId = param.mSchFileId;


            if (!string.IsNullOrEmpty(m_sceFileId))
                mListener?.AddResouces(m_sceFileId);

            if (!string.IsNullOrEmpty(m_schFileId))
                mListener?.AddResouces(m_schFileId);

            if (mListener != null)
                mListener.SetCollectd(true);
        }

        public override void SetCollected( bool b )
        {
            mIsCollectd = b;
        }

        /// <summary>
        /// 加载
        /// </summary>
        public override void Load()
        {
            PrepareImpl();
            LoadImpl();
        }

        /// <summary>
        ///  卸载
        /// </summary>
        public override void Unload()
        {
            if (mLoadingState == LoadingState.LoadstateUnloaded) return;

            if (mLoadingState != LoadingState.LoadstateLoading)
            {
                mLoadingState = LoadingState.LoadstateLoading;

                UnloadImpl();
            }
        }

        /// <summary>
        /// 预加载实体
        /// </summary>
        public override void PrepareImpl()
        {
            //todo:网络请求场景配置和预案配置及服务器配置的一些语音图标etc
            mLoadingState = LoadingState.LoadstatePrepared;
        }

        /// <summary>
        ///     加载关卡的实体
        /// </summary>
        public override void LoadImpl()
        {
            base.LoadImpl();

            //加载场景配置
            if (!string.IsNullOrEmpty(m_sceFileId))
            {
                ResourcesManager.Instance.LoadWebTextFile(m_sceFileId,
                    NetHelper.BuildRomoteFilePath(EnGlobalValue.Scene_Config, m_sceneGuid),
                    uwr =>
                    {
                        Debug.Log("加载场景配置 success" + uwr);
                        m_sceneDataInfo = ObjectToFileTools<SceneDataInfo>.ReadJsonStr(uwr);
                        mListener.OnLoadCompleted(m_sceFileId, this, null);
                    },
                    uwr =>
                    {
                        mListener.OnLoadCompleted(m_sceFileId, this, null);
                        SsitDebug.Error("服务端场景配置异常请联系服务器开发人员 " + uwr + m_sceFileId);

                        //hack 冗错处理
                        m_sceneDataInfo = new SceneDataInfo();
                    });
            }
            else
            {
                m_sceneDataInfo = new SceneDataInfo();
            }


            //加载预案配置
            if (!string.IsNullOrEmpty(m_schFileId))
            {
                ResourcesManager.Instance.LoadWebTextFile(m_schFileId,
                    NetHelper.BuildRomoteFilePath(EnGlobalValue.Scheme_Config, m_sceneGuid, m_schemeGuid),
                    uwr =>
                    {
                        Debug.Log("加载预案配置 success" + uwr);
                        m_schemeDataInfo = ObjectToFileTools<SchemeDataInfo>.ReadJsonStr(uwr);
                        mListener.OnLoadCompleted(m_schFileId, this, null);
                    },
                    uwr =>
                    {
                        mListener.OnLoadCompleted(m_schFileId, this, null);
                        SsitDebug.Error("服务端场景配置异常请联系服务器开发人员 " + uwr + m_sceFileId);
                        //hack 冗错处理
                        m_schemeDataInfo = new SchemeDataInfo();
                    }
                );
            }
            else
            {
                m_schemeDataInfo = new SchemeDataInfo();
            }

            mLoadingState = LoadingState.LoadstateLoading;

            if (mIsCollectd)
                mListener.processCompleted(this);
        }

        /// <summary>
        /// 卸载资源实体
        /// </summary>
        public override void UnloadImpl()
        {
            base.UnloadImpl();
            //todo:unload config
            mLoadingState = LoadingState.LoadstateUnloaded;
        }

        /// <summary>
        /// 外部调用抛出的加载进程回调
        /// </summary>
        public override void PostLoadProcess()
        {
            // 编辑器场景配置
            var config = Object.FindObjectOfType<ScriptScene>();
            if (config)
            {
                if (config.sceneSimpleConfig)
                {
                    foreach (var tt in config.sceneSimpleConfig.interactionMaps)
                    {
                        if (tt == null) continue;

                        //tt.gameObject.SetActive(true);
                        tt.OnPostRegister();
                    }

                    SsitDebug.Info("编辑器场景配置 sceneSimpleConfig 注册完成");
                }
                else if (config.sceneConfig)
                {
                    foreach (var tt in config.sceneConfig.interactionMaps)
                    {
                        if (tt == null) continue;
                        tt.OnPostRegister();
                    }

                    SsitDebug.Info("编辑器场景配置 sceneConfig 注册完成");
                }
            }

            if (mLoadingState == LoadingState.LoadstateUpdating ||
                mLoadingState == LoadingState.LoadstateUpdated) return;

            if (mLoadingState == LoadingState.LoadstateLoaded)
            {
                mLoadingState = LoadingState.LoadstateUpdating;

                Update();
            }
            else
            {
                mLoadingState = LoadingState.LoadstateUpdating;
            }
        }

        /// <summary>
        /// 更新关卡资源1
        /// </summary>
        public override void Update()
        {
            if (mLoadingState != LoadingState.LoadstateUpdating)
                return;

            // 初始化场景文件配置
            if (m_sceneDataInfo != null)
                //场景添加的模型资源
                foreach (var cc in m_sceneDataInfo.SceneObjectDataList)
                    SsitApplication.Instance.CreateObject(cc.guid, cc.dataId, InternalCreateSceneObject, cc);

            //预案的配置
            if (m_schemeDataInfo != null)
            {
                UpdateGloablInfo();

                AddObjectForLevel();

                //SpawnPlayerForGroup();
                //AddPlayerForLevel();

                //AddQuestProcedureForLevel();
            }

            if (onCreate != null)
                onCreate.Invoke(mData);

            mLoadingState = LoadingState.LoadstateUpdated;

            if (autoUnload) SsitApplication.Instance.UnloadLevel(this);
        }


        public override void Shutdown()
        {
            // 场景
            m_sceneGuid = null;
            m_sceFileId = null;
            m_sceneDataInfo = null;

            // 预案
            m_schemeGuid = null;
            m_schFileId = null;
            m_schemeDataInfo = null;

            base.Shutdown();
        }

        #region Internal Members

        /// <summary>
        /// 更新关卡信息
        /// </summary>
        private void UpdateGloablInfo()
        {
            if (m_schemeDataInfo.Weather == null)
            {
                return;
            }

            //hack：通知全局控制器天气改变
            if (!(Facade.Instance.RetrieveProxy(SceneInfoProxy.NAME) is SceneInfoProxy proxy))
            {
                SsitDebug.Error("天气信息来早了");
                return;
            }
            //初始化天气
            proxy.SetWeather(new WeatherInfo
            {
                Weather = m_schemeDataInfo.Weather.Weather,
                WindDirection = m_schemeDataInfo.Weather.WindDirection,
                WindLevel = m_schemeDataInfo.Weather.WindLevel,
                WindVelocity = m_schemeDataInfo.Weather.WindVelocity
            });
            
        }

        /// <summary>
        /// 添加对象到关卡
        /// </summary>
        private void AddObjectForLevel()
        {
            //数据校验
            var errorCount =m_schemeDataInfo.objList.RemoveAll(x => x.dataId == 0);
            if (errorCount != 0)
            {
                SsitDebug.Info($"自动数据校验{errorCount}");
            }
            
            foreach (var cc in m_schemeDataInfo.objList)
            {
                SsitApplication.Instance.CreateObject(cc.guid, cc.dataId, InternalCreateSchemeObject, cc);
            }
            //数据校验
            errorCount = m_schemeDataInfo.UintList.RemoveAll(x => x.dataId == 0);
            if (errorCount != 0)
            {
                SsitDebug.Info($"自动数据校验{errorCount}");
            }
            
            foreach (var cc in m_schemeDataInfo.UintList)
            {
                Debug.Log("m_SchemeDataInfo UintList" + cc.dataId);
                SsitApplication.Instance.CreateEditorObject(cc.guid, cc.dataId, InternalCreateSchemeObject, cc);
            }
        }

        /// <summary>
        /// 添加预案任务流程
        /// </summary>
        private void AddQuestProcedureForLevel()
        {
        }

        /// <summary>
        /// 加载配置角色（不参与分组筛选）
        /// </summary>
        private void AddPlayerForLevel()
        {
            /*foreach (var cc in m_SchemeDataInfo.UintList)
            {
                switch (cc.department)
                {
                    case (int)ENForceUnitType.EN_Character:
                        Debug.Log("m_SchemeDataInfo EN_Character EN_Npc dataId: " + cc.dataId + "guid " + cc.guid);
                        //access:npc后面进行单拆 ，暂时统一处理
                        if (GlobalManager.Instance.IsSync)
                        {
                            SsitApplication.Instance.SpawnPlayer(cc.guid, cc.dataId, null, cc);
                        }
                        else
                        {
                            SsitApplication.Instance.CreatePlayer(cc.guid, cc.dataId, null, cc);
                        }

                        break;
                }
            }*/
        }

        /// <summary>
        /// 根据后台分组配置进行有效显示
        /// </summary>
        private void SpawnPlayerForGroup()
        {
        }

        #endregion


        #region Internal Event

        private void InternalCreateSceneObject( BaseObject obj, object render, object data )
        {
            SsitApplication.Instance.OnCreatedFunc(obj, render, data);
            /*var go = m_sceneDataInfo.SceneObjectDataList.Find(x => x.guid == obj.Guid);

            if (go != null)
            {
                go.sceneObj = obj;
                obj.SceneInstance.CanEdit = false;
            }*/
        }

        private void InternalCreateSchemeObject( BaseObject obj, object render, object data )
        {
            SsitApplication.Instance.OnCreatedFunc(obj, render, data);
            /*if (data is InteractiveDataInfo interactiveData)
            {
                interactiveData.sceneObj = obj;
            }*/
        }

        #endregion

        /*/// <summary>
        /// 保存预案相关数据
        /// </summary>
        /// <returns></returns>
        public byte[] SaveSchemeDataInfo()
        {
            return ObjectToFileTools<SchemeDataInfo>.WriteJsonToBytes(SchemeDataInfo);
        }*/
    }
}