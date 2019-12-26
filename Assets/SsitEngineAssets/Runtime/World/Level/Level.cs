/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/4/11 12:07:17                     
*└──────────────────────────────────────────────────────────────┘
*/

using Framework.Data;
using Framework.SceneObject;
using SsitEngine.DebugLog;
using SsitEngine.Unity.Msic;
using Framework.SceneObject;
using UnityEditor;
using UnityEngine;

namespace Framework.Scene
{
    public delegate void OnCreateComplete( object data );
    
    public enum LoadedType
    {
        bundle,
        scene,
        file,
        finished
    }
    
    public struct LevelParams 
    {
        public string guid; // 关卡信息\
        public int mSceneId; // 场景Id
            
        public string mSceFileId; // 场景配置Guid
        public string mSceneGuid; // 场景Guid
        
        public string mSchemeGuid; // 预案guid
        public string mSchFileId; // 预案配置Guid（后台编辑/内置）
        
        /*public string mAccidentName; // 事故名称
        private string mAudioId; // 简介语音
        public string mIconId; // 房间图标
        private string mIntrodution; // 事故简介
        public string mMode; // 演练模式(单人/多人)
        public string mSceneName; // 关卡名称
        public string mStartTime; // 房间创建时间
        public int mState; // 是否保存
        public string mTopic; // 演练主题[泄漏、着火、爆炸等]*/
    }
    
    /// <summary>
    /// 场景资源加载监听（主要监听场景相关资源加载的完成进度，改变资源句柄，便于后面场景关卡的初始化进行）
    /// </summary>
    public class LevelSerializedListener : ResSerializerListener
    {
        public LevelSerializedListener( Level level ) : base(level)
        {
        }

        public override void processCompleted( ResourceBase resource )
        {
            if (resource.mLoadingState == LoadingState.LoadstateLoading)
                resource.mLoadingState = LoadingState.LoadstateLoaded;
            else if (resource.mLoadingState == LoadingState.LoadstateUpdating)
                resource.Update();
            else
                SsitDebug.Error("资源句柄异常" + resource.mLoadingState);
        }
    }

    /// <summary>
    /// 场景镜像【抽象概念：一个关卡 == 场景 + 相应预案】
    /// </summary>
    public class Level : SsitEngine.Unity.Scene.Level
    {
        // 自动卸载标识
        protected readonly bool autoUnload;

        //资源监听队列
        protected readonly LevelSerializedListener mListener;

        /// <summary>
        /// 自定义数据
        /// </summary>
        protected object mData;

        /// <summary>
        /// 场景创建完成
        /// </summary>
        protected OnCreateComplete onCreate;

        protected LevelAtrribute atrribute;

        public Level()
        {
            mListener = new LevelSerializedListener(this);
            autoUnload = true;
        }

        public void SetAttribute( LevelAtrribute attri )
        {
            atrribute = attri;
            resourceName = atrribute.ResourceName;
        }

        public virtual void SetResource( LevelParams param )
        {
            //levelInfo = info;
            if (mListener != null)
                mListener.SetCollectd(true);
        }

        public override void SetCollected( bool b )
        {
            mIsCollectd = b;
        }


        /// <summary>
        ///     设置加载完成回调
        /// </summary>
        /// <param name="onCreate">完成回调</param>
        /// <param name="data">自定义数据</param>
        public void SetCreate( OnCreateComplete onCreate = null, object data = null )
        {
            this.onCreate += onCreate;
            mData = data;
        }

        /// <summary>
        ///     加载
        /// </summary>
        public override void Load()
        {
            PrepareImpl();
            LoadImpl();
        }

        /// <summary>
        ///     卸载
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
        ///     预加载实体
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

            mLoadingState = LoadingState.LoadstateLoading;

            if (mIsCollectd)
                mListener.processCompleted(this);
        }

        /// <summary>
        ///     卸载资源实体
        /// </summary>
        public override void UnloadImpl()
        {
            base.UnloadImpl();
            //todo:unload config
            mLoadingState = LoadingState.LoadstateUnloaded;
        }

        /// <summary>
        ///     外部调用抛出的加载进程回调
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
        ///     更新关卡资源1
        /// </summary>
        public override void Update()
        {
            if (mLoadingState != LoadingState.LoadstateUpdating)
                return;

            if (onCreate != null)
                onCreate.Invoke(mData);

            mLoadingState = LoadingState.LoadstateUpdated;

            if (autoUnload)
                SsitApplication.Instance.UnloadLevel(this);
        }

        public override void Shutdown()
        {
            // 卸载场景物体
            ObjectManager.Instance.DestoryAllObject();
            onCreate = null;
            mData = null;
            //levelInfo = null;
            atrribute.Shutdown();
            base.Shutdown();
        }
    }
}