/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：系统平台                                                    
*│　作   者：xx                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/11/06 11:04:21                             
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections;
using System.Collections.Generic;
using Framework.Data;
using Framework.Helper;
using Framework.Logic;
using Framework.NetSocket;
using Framework.Procedure;
using Framework.Quest;
using Framework.Scene;
using Framework.SceneObject;
using Framework.SsitInput;
using SSIT.proto;
using SsitEngine.DebugLog;
using SsitEngine.QuestManager;
using SsitEngine.Unity;
using SsitEngine.Unity.Config;
using SsitEngine.Unity.Data;
using SsitEngine.Unity.HUD;
using SsitEngine.Unity.Msic;
using SsitEngine.Unity.Procedure;
using SsitEngine.Unity.Resource;
using SsitEngine.Unity.Scene;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Unity.Avatar;
using SsitEngine.Unity.Sound;
using SsitEngine.Unity.Timer;
using SsitEngine.Unity.UI;
using Table;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Level = Framework.Scene.Level;
using QuestHelper = Framework.Quest.QuestHelper;

namespace Framework
{
    using EnumrateMap = Dictionary<BaseObject, IEnumerator>;
    using TimerEventMap = Dictionary<BaseObject, List<TimerEventTask>>;

    public class SsitApplication : AbstractPlatform<SsitApplication>
    {
        private Main m_mainObj;
        protected TimerEventMap mAnimTimerEvents;
        protected EnumrateMap mEnumrateMap;

        #region 子类实现

        public override void OnSingletonInit()
        {
            base.OnSingletonInit();
            mAnimTimerEvents = new TimerEventMap();
            mEnumrateMap = new EnumrateMap();
        }

        public override void OnStart( GameObject main )
        {
            m_mainObj = main.GetComponent<Main>();
            //m_mainObj.GetComponentInChildren<MainForm>().Init();
            base.OnStart(main);
        }

        public override void InitThirdLibConfig()
        {
            Engine.Debug = m_mainObj.mDebug;
            Engine.Instance.Start(this);
        }

        public override void InitAppEnvironment()
        {
            /*Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;*/
        }

        public override void StartGame()
        {
            base.StartGame();

            // 注册流程管理器
            var proManager = Engine.Instance.CreateModule(ProcedureManager.Instance);

            // 注册配置模块
            SsitEngine.Unity.Config.ConfigManager.Instance.SetConfigHelper(new ConfigHelper());
            Engine.Instance.CreateModule(SsitEngine.Unity.Config.ConfigManager.Instance);
            SsitDebug.Info("注册配置模块。。。");

            // 注册资源管理模块
            //ResourcesManager.Instance.SetResourecesLoaderHelper(new SsitResourcesHelper());
            Engine.Instance.CreateModule(ResourcesManager.Instance);
            SsitDebug.Info("注册资源管理模块。。。");

            // 注册数据管理 -- 迁移至初始进程
            /*DataManager dataManager = Engine.Instance.CreateModule(DataManager.Instance);
            dataManager.SetLoaderHelper(new JsonLoader());
            SsitDebug.Info("注册数据管理。。。");*/

            // 注册音效管理模块
            Engine.Instance.CreateModule(SoundManager.Instance);
            SsitDebug.Info("注册音效管理模块。。。");

            // 注册场景管理
            Engine.Instance.CreateModule(SceneManager.Instance);
            SceneManager.Instance.SetResourceManager(ResourcesManager.Instance);

            // 注册输入模块
            // Engine.Instance.CreateModule(InputManager.Instance);

            // 注册UI模块
            Engine.Instance.CreateModule(UIManager.Instance);
            SsitDebug.Info("注册UI模块。。。");

            // 注册Java网络模块
            Engine.Instance.CreateModule(NetSocketManager.Instance);
            SsitDebug.Info("注册Java网络模块。。。");

            // 注册演练局域网模块
            //SsitDebug.Info("注册演练局域网模块。。。");
            //var temp = ResourcesManager.Instance.LoadAsset<NetworkLobbyManagerExt>("Network/LobbyManager");
            //temp.gameObject.name = typeof(NetworkLobbyManagerExt).Name;
            //Engine.Instance.CreateModule(temp);

            // 注册全局管理器（替换pc中的mainprocess）
            Engine.Instance.CreateModule(GlobalManager.Instance);
            SsitDebug.Info("注册全局管理器。。。");

            Engine.Instance.CreateModule(ObjectManager.Instance);
            SsitDebug.Info("注册实体管理器。。。");

            //Engine.Instance.CreateModule(EZReplayManager.Instance);
            //SsitDebug.Info("注册回放管理器。。。");

            Engine.Instance.CreateModule(InputManager.Instance);
            InputManager.Instance.SetInputHander(new InputHelper());
            SsitDebug.Info("注册回放管理器。。。");

            Facade.Instance.SendNotification((ushort) EnGlobalEvent.Start, ENProcedureType.ProcedureStartUp);
        }

        public override void OnUpdate( float elapsed )
        {
            base.OnUpdate(elapsed);
            // 框架轮询
            Engine.Instance.Update(elapsed);
        }

        public override void OnApplicationQuit()
        {
            Engine.Instance.Shutdown();
        }

        #endregion

        #region Mono引擎

        public void Update()
        {
            OnUpdate(Time.deltaTime);
        }

        private void OnDestroy()
        {
            IsApplicationQuit = true;
        }

        public override void OnApplicationFocus( bool focusStatus )
        {
            Facade.Instance.SendNotification((ushort) EnEngineEvent.OnApplicationFocusChange, focusStatus);
        }

        #endregion

        #region 对象

        /*
        * General
        */
        public BaseObject CreateEzObject( string guid, int itemId, OnCreated func = null, object data = null,
            GameObject warpGameObject = null )
        {
            var flag = itemId / 10000;
            if (flag == 5)
                //todo:创建车辆
                return CreateVehicle(guid, itemId, func, warpGameObject);
            if (flag == 6 || flag == 7) return CreatePlayer(guid, itemId, func, data, warpGameObject);
            return CreateObject(guid, itemId, func, data, warpGameObject);
        }

        public BaseObject CreateEditorObject( string guid, int itemId, OnCreated func = null, object data = null,
            GameObject warpGameObject = null )
        {
            var flag = itemId / 10000;
            if (flag == 4)
                return CreateNpc(guid, itemId, func, data, warpGameObject);
            if (flag == 5)
                return CreateVehicle(guid, itemId, func, data, warpGameObject);
            if (flag == 6 || flag == 7)
                return CreatePlayer(guid, itemId, func, data, warpGameObject);

            return CreateObject(guid, itemId, func, data, warpGameObject);
        }
        /*
         * SceneObject
         */

        public Framework.SceneObject.SceneObject CreateObject( string guid, int itemId, OnCreated func = null,
            object data = null, GameObject warpGameObject = null )
        {
            var itemDefine = DataManager.Instance.GetData<ItemDefine>((int) EnLocalDataType.DATA_ITEM, itemId);
            if (itemDefine != null)
            {
                var attribute = itemDefine.Create<BaseAtrribute>(0);
                var obj = CreateObject(guid, attribute.PrefabPath, attribute,
                    func, data, warpGameObject);
                return obj;
            }
            return null;
        }

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="resouceName">资源id</param>
        /// <param name="attribute">资源属性</param>
        /// <returns></returns>
        public Framework.SceneObject.SceneObject CreateObject( string guid, string resouceName,
            BaseAtrribute attribute = null, OnCreated func = null, object data = null,
            GameObject warpGameObject = null )
        {
            Framework.SceneObject.SceneObject obj = null;
            if (string.IsNullOrEmpty(guid))
                obj = (Framework.SceneObject.SceneObject) ObjectManager.Instance
                    .CreateObject(EnFactoryType.SceneObjectFactory);
            else
                obj = (Framework.SceneObject.SceneObject) ObjectManager.Instance
                    .CreateObject(guid, EnFactoryType.SceneObjectFactory);

            if (obj != null)
            {
                obj.SetResourName(resouceName);
                if (func == null)
                    obj.SetOnCreated(OnCreatedFunc, data);
                else
                    obj.SetOnCreated(func, data);
                obj.SetAttribute(attribute);
                obj.Load(false, warpGameObject);
            }

            return obj;
        }

        /// <summary>
        /// 卵生对象
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="itemId">数据id</param>
        /// <param name="func">创建完成回调</param>
        /// <param name="data">自定义数据</param>
        /// <param name="warpGameObject"></param>
        /// <returns></returns>
        public void SpawnObject( string guid, int itemId, OnCreated func = null, object data = null,
            GameObject warpGameObject = null )
        {
            if (GlobalManager.Instance.IsSync)
            {
                //通知服务端创建对象
                //Framework.SceneObject.SceneObject obj = CreateObject(guid, itemId, func, data, warpGameObject);

                var request = data as CSSpawnSceneObjectRequest;
                //Facade.Instance.SendNotification((ushort)EnMirrorEvent.SendMessage, new MessagePackage(ConstMessageID.CSSpawnSceneObjectRequest, request), true);
            }
        }

        public void DestoryObject( string guid )
        {
            ObjectManager.Instance.DestroyObject(guid);
        }

        /*
         * Player
         */

        /// <summary>
        /// 创建玩家角色
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="itemId">资源名称/路径</param>
        /// <param name="func">对象属性</param>
        /// <param name="data">自定义数据</param>
        /// <param name="warpGameObject">包装的实体对象</param>
        /// <returns></returns>
        public Player CreatePlayer( string guid, int itemId, OnCreated func = null, object data = null,
            GameObject warpGameObject = null )
        {
            var playerDefine =
                DataManager.Instance.GetData<PlayerDefine>((int) EnLocalDataType.DATA_ITEM, itemId);
            if (playerDefine != null)
            {
                BaseAtrribute attribute = null;

                if (itemId == DataItemProxy.c_sClientTerminal)
                    attribute = playerDefine.CreateVariant<NetPlayerAttribute>(0);
                else
                    attribute = playerDefine.Create<PlayerAttribute>(0);

                var obj = CreatePlayer(guid, attribute.PrefabPath, false, attribute, func, data, warpGameObject);
                return obj;
            }

            return null;
        }

        /// <summary>
        /// 创建玩家角色
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="resouceName">资源名称/路径</param>
        /// <param name="isSync">是否同步</param>
        /// <param name="attribute">对象属性</param>
        /// <param name="func">创建完成回调</param>
        /// <param name="data">自定义数据</param>
        /// <param name="warpGameObject">包装的实体对象</param>
        /// <returns>创建的玩家对象</returns>
        public Player CreatePlayer( string guid, string resouceName, bool isSync, BaseAtrribute attribute = null,
            OnCreated func = null, object data = null, GameObject warpGameObject = null )
        {
            Player obj = null;
            if (string.IsNullOrEmpty(guid))
                obj = (Player) ObjectManager.Instance.CreateObject(EnFactoryType.PlayerFactory);
            else
                obj = (Player) ObjectManager.Instance.CreateObject(guid, EnFactoryType.PlayerFactory);

            if (obj != null)
            {
                obj.SetResourName(resouceName);
                obj.SetAttribute(attribute);
                if (func == null)
                    obj.SetOnCreated(OnPlayerCreatedFunc, data);
                else
                    obj.SetOnCreated(func, data);
                obj.Load(false, warpGameObject);
            }

            return obj;
        }

        /// <summary>
        /// 卵生角色对象
        /// </summary>
        /// <param name="guid">角色guid</param>
        /// <param name="itemId">数据id</param>
        /// <param name="func">创建完成回调</param>
        /// <param name="data">自定义数据</param>
        /// <param name="warpGameObject">上层包装对象</param>
        /// <returns>底层Player 对象</returns>
        public Player SpawnPlayer( string guid, int itemId, OnCreated func = null, object data = null,
            GameObject warpGameObject = null )
        {
            var playerDefine =
                DataManager.Instance.GetData<PlayerDefine>((int) EnLocalDataType.DATA_ITEM, itemId);
            if (playerDefine != null)
            {
                var attribute = playerDefine.Create<PlayerAttribute>(0);
                func = func ?? OnSyncPlayerCreatedFunc;
                var obj = CreatePlayer(guid, attribute.PrefabPath, true, attribute, func, data, warpGameObject);
                return obj;
            }
            return null;
        }

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public Player CreatePlayer( int itemId )
        {
            return (Player) ObjectManager.Instance.CreateObject(EnFactoryType.PlayerFactory);
        }

        /// <summary>
        /// 销毁对象
        /// </summary>
        /// <param name="guid"></param>
        public void DestoryPlayer( string guid )
        {
            ObjectManager.Instance.DestroyObject(guid, EnFactoryType.PlayerFactory);
        }

        /*
        * Npc
        */

        /// <summary>
        /// 创建Npc角色
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="itemId">资源名称/路径</param>
        /// <param name="func">对象属性</param>
        /// <param name="data">自定义数据</param>
        /// <param name="warpGameObject">包装的实体对象</param>
        /// <returns></returns>
        public Npc CreateNpc( string guid, int itemId, OnCreated func = null, object data = null,
            GameObject warpGameObject = null )
        {
            var npcDefine =
                DataManager.Instance.GetData<NpcDefine>((int) EnLocalDataType.DATA_ITEM, itemId);
            if (npcDefine != null)
            {
                var attribute = npcDefine.Create<NpcAttribute>(0);

                var obj = CreateNpc(guid, attribute.PrefabPath, false, attribute, func, data, warpGameObject);
                return obj;
            }

            return null;
        }

        /// <summary>
        /// 创建Npc角色
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="resouceName">资源名称/路径</param>
        /// <param name="isSync">是否同步</param>
        /// <param name="attribute">对象属性</param>
        /// <param name="func">创建完成回调</param>
        /// <param name="data">自定义数据</param>
        /// <param name="warpGameObject">包装的实体对象</param>
        /// <returns>创建的玩家对象</returns>
        public Npc CreateNpc( string guid, string resouceName, bool isSync, BaseAtrribute attribute = null,
            OnCreated func = null, object data = null, GameObject warpGameObject = null )
        {
            Npc obj = null;
            if (string.IsNullOrEmpty(guid))
                obj = (Npc) ObjectManager.Instance.CreateObject(EnFactoryType.NpcFactory);
            else
                obj = (Npc) ObjectManager.Instance.CreateObject(guid, EnFactoryType.NpcFactory);

            if (obj != null)
            {
                obj.SetResourName(resouceName);
                obj.SetAttribute(attribute);
                if (func == null)
                    obj.SetOnCreated(OnPlayerCreatedFunc, data);
                else
                    obj.SetOnCreated(func, data);
                obj.Load(false, warpGameObject);
            }

            return obj;
        }


        /// <summary>
        /// 销毁对象
        /// </summary>
        /// <param name="guid"></param>
        public void DestoryNpc( string guid )
        {
            ObjectManager.Instance.DestroyObject(guid, EnFactoryType.PlayerFactory);
        }

        /*
        * Vehicle
        */
        public Vehicle CreateVehicle( string guid, int itemId, OnCreated func = null, object data = null,
            GameObject warpGameObject = null )
        {
            var itemDefine =
                DataManager.Instance.GetData<ForceUintDefine>((int) EnLocalDataType.DATA_ITEM, itemId);
            if (itemDefine != null)
            {
                var attribute = itemDefine.Create<VehicleAttribute>(0);
                var obj = CreateVehicle(guid, attribute.PrefabPath, attribute, func, data, warpGameObject);
                return obj;
            }
            return null;
        }

        /// <summary>
        /// 创建对象
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="resouceName">资源id</param>
        /// <param name="attribute">资源属性</param>
        /// <returns></returns>
        public Vehicle CreateVehicle( string guid, string resouceName, BaseAtrribute attribute = null,
            OnCreated func = null, object data = null, GameObject warpGameObject = null )
        {
            Vehicle obj = null;
            if (string.IsNullOrEmpty(guid))
                obj = (Vehicle) ObjectManager.Instance.CreateObject(EnFactoryType.VehicleFactory);
            else
                obj = (Vehicle) ObjectManager.Instance.CreateObject(guid, EnFactoryType.VehicleFactory);

            if (obj != null)
            {
                obj.SetResourName(resouceName);
                if (func == null)
                    obj.SetOnCreated(OnSyncVehicleCreatedFunc, data);
                else
                    obj.SetOnCreated(func, data);
                obj.SetAttribute(attribute);
                obj.Load(false, warpGameObject);
            }

            return obj;
        }

        /// <summary>
        /// 卵生对象
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="itemId">数据id</param>
        /// <param name="func">创建完成回调</param>
        /// <param name="data">自定义数据</param>
        /// <param name="warpGameObject"></param>
        /// <returns></returns>
        public void SpawnVehicle( string guid, int itemId, OnCreated func = null, object data = null,
            GameObject warpGameObject = null )
        {
            if (GlobalManager.Instance.IsSync)
            {
                //通知服务端创建对象
                //Framework.SceneObject.SceneObject obj = CreateObject(guid, itemId, func, data, warpGameObject);

                var request = data as CSSpawnSceneObjectRequest;
                //Facade.Instance.SendNotification((ushort)EnMirrorEvent.SendMessage, new MessagePackage(ConstMessageID.CSSpawnSceneObjectRequest, request), true);
            }
        }

        public void DestoryVehicle( string guid )
        {
            ObjectManager.Instance.DestroyObject(guid, EnFactoryType.SceneObjectFactory);
        }

        /*
         * Level
         */

        /// <summary>
        /// 创建关卡
        /// </summary>
        /// <param name="sceneId">场景guid</param>
        /// <param name="info">关卡配置信息</param>
        /// <param name="func">创建完成回调</param>
        /// <param name="data">自定义数据</param>
        /// <returns>指定的关卡对象</returns>
        public Level CreateLevel( int sceneId, LevelParams info, OnCreateComplete func = null, object data = null )
        {
            var itemDefine =
                DataManager.Instance.GetData<SceneDefine>((int) EnLocalDataType.DATA_SCENE, sceneId);
            if (itemDefine != null)
            {
                var levelAtrribute = itemDefine.Create<LevelAtrribute>(0);

                Level level = new Level();
                
                level.SetAttribute(levelAtrribute);
                level.SetResource(info);
                level.SetCreate(func, data);

                //#if DEVELOPE_ENABLE
                //level.Load();
                SceneManager.Instance.LoadScene(level);
                //#else
                //SceneManager.Instance.LoadScene(level, progress, data);
                //#endif
                return level;
            }
            return null;
        }

        /// <summary>
        /// 创建关卡
        /// </summary>
        /// <param name="sceneId">场景guid</param>
        /// <param name="info">关卡配置信息</param>
        /// <param name="func">创建完成回调</param>
        /// <param name="data">自定义数据</param>
        /// <returns>指定的关卡对象</returns>
        public T CreateLevel<T>( int sceneId, LevelParams info, OnCreateComplete func = null, object data = null )
            where T : Level, new()
        {
            var itemDefine = DataManager.Instance.GetData<SceneDefine>((int) EnLocalDataType.DATA_SCENE, sceneId);
            if (itemDefine != null)
            {
                var level = new T();

                level.SetResource(info);
                level.SetCreate(func, data);

                //#if DEVELOPE_ENABLE
                //level.Load();
                SceneManager.Instance.LoadScene(level);
                //#else
                //SceneManager.Instance.LoadScene(level, progress, data);
                //#endif
                return level;
            }
            return null;
        }


        /// <summary>
        /// 卸载场景
        /// </summary>
        /// <param name="level">指定的场景对象</param>
        public void UnloadLevel( SsitEngine.Unity.Scene.Level level )
        {
            SsitDebug.Info("Unload scLoadSceneProcess");
            SceneManager.Instance.UnloadScene(level);
        }

        /// <summary>
        /// 销毁场景
        /// </summary>
        /// <param name="level">指定的场景对象</param>
        /// <param name="func">回调方法</param>
        /// <param name="data">自定义数据</param>
        public void DestoryLevel( SsitEngine.Unity.Scene.Level level, OnCreateComplete func = null, object data = null )
        {
            if (level == null) return;

            if (level.mLoadingState != LoadingState.LoadstateUnloaded) UnloadLevel(level);

            // shutdown level
            level.Shutdown();

            if (func != null) func(data);
        }


        /*
         * Item
         */

        /// <summary>
        /// 创建物体道具
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="resouceName">资源名称/路径</param>
        /// <param name="isSync">是否同步</param>
        /// <param name="attribute">对象属性</param>
        /// <param name="func">创建完成回调</param>
        /// <param name="data">自定义数据</param>
        /// <param name="warpGameObject">包装的实体对象</param>
        /// <returns>创建的物体道具</returns>
        public Item CreateItem( string guid, string resouceName, bool isSync, ItemAtrribute attribute = null,
            OnCreated func = null, object data = null, GameObject warpGameObject = null )
        {
            Item obj = null;
            if (string.IsNullOrEmpty(guid))
                obj = (Item) ObjectManager.Instance.CreateObject(EnFactoryType.ItemFactory);
            else
                obj = (Item) ObjectManager.Instance.CreateObject(guid, EnFactoryType.ItemFactory);

            if (obj != null)
            {
                obj.SetResourName(resouceName);
                obj.SetAttribute(attribute);
                if (func == null)
                    obj.SetOnCreated(OnCreatedFunc, data);
                else
                    obj.SetOnCreated(func, data);
                obj.Load(false, warpGameObject);
            }

            return obj;
        }

        /// <summary>
        /// 创建道具对象（仅限服务端使用）
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public Item CreateItem( string guid, int itemId )
        {
            var equipMentDefine =
                DataManager.Instance.GetData<InvEquipMentDefine>((int) EnLocalDataType.DATA_INVEQUIP, itemId);
            if (equipMentDefine != null)
            {
                var attribute = equipMentDefine.Create<ItemAtrribute>(0);
                Item obj = null;
                if (string.IsNullOrEmpty(guid))
                    obj = (Item) ObjectManager.Instance.CreateObject(EnFactoryType.ItemFactory);
                else
                    obj = (Item) ObjectManager.Instance.CreateObject(guid, EnFactoryType.ItemFactory);

                if (obj != null)
                {
                    obj.SetResourName(attribute.Resources);
                    obj.SetAttribute(attribute);
                }
                return obj;
            }
            return null;
        }

        /// <summary>
        /// 创建道具对象（仅限服务端使用）
        /// </summary>
        /// <param name="item"></param>
        /// <param name="func"></param>
        /// <param name="data"></param>
        /// <param name="warpGameObject"></param>
        /// <returns></returns>
        public Item CreateItem( Item item, OnCreated func = null, object data = null, GameObject warpGameObject = null )
        {
            if (func == null)
                item.SetOnCreated(OnCreatedFunc, data);
            else
                item.SetOnCreated(func, data);
            item.Load(false, warpGameObject);
            return item;
        }

        /// <summary>
        /// 创建道具对象
        /// </summary>
        /// <param name="guid">道具guid</param>
        /// <param name="itemId">数据id</param>
        /// <param name="func">创建完成回调</param>
        /// <param name="data">自定义数据</param>
        /// <param name="warpGameObject">上层包装对象</param>
        /// <returns>底层Player 对象</returns>
        public Item CreateItem( string guid, int itemId, OnCreated func, object data = null,
            GameObject warpGameObject = null )
        {
            var equipMentDefine =
                DataManager.Instance.GetData<InvEquipMentDefine>((int) EnLocalDataType.DATA_INVEQUIP, itemId);
            if (equipMentDefine != null)
            {
                var attribute = equipMentDefine.Create<ItemAtrribute>(0);
                func = func ?? OnItemCreatedFunc;
                var obj = CreateItem(guid, attribute.Resources, true, attribute, func, data, warpGameObject);
                return obj;
            }
            return null;
        }

        /// <summary>
        /// 获取装备对应的装备巢
        /// </summary>
        /// <param name="itemId">装备ID</param>
        /// <returns>该装备要装备的装备巢</returns>
        public int GetEquipSlot( int itemId )
        {
            var equipMentDefine =
                DataManager.Instance.GetData<InvEquipMentDefine>((int) EnLocalDataType.DATA_INVEQUIP, itemId);
            return equipMentDefine.SlotType;
        }

        /// <summary>
        /// 卵生物体道具
        /// </summary>
        /// <param name="guid">物体guid</param>
        /// <param name="itemId">数据id</param>
        /// <param name="func">创建完成回调</param>
        /// <param name="data">自定义数据</param>
        /// <param name="warpGameObject">上层包装对象</param>
        /// <returns>底层Player 对象</returns>
        public void SpawnItem( string guid, int itemId, OnCreated func = null, object data = null,
            GameObject warpGameObject = null )
        {
            if (GlobalManager.Instance.IsSync)
            {
                //通知服务端创建对象
                var request = data as CSSyncPlayerInfoRequest;
                //Facade.Instance.SendNotification((ushort)EnMirrorEvent.SendMessage, new MessagePackage(ConstMessageID.CSSyncPlayerInfoRequest, request), true);
            }
        }

        public void DestoryItem( string guid )
        {
            ObjectManager.Instance.DestroyObject(guid, EnFactoryType.ItemFactory);
        }


        /*
         *Skill
         */

        /// <summary>
        /// 创建物体道具
        /// </summary>
        /// <param name="guid">guid</param>
        /// <param name="resouceName">资源名称/路径</param>
        /// <param name="isSync">是否同步</param>
        /// <param name="attribute">对象属性</param>
        /// <param name="func">创建完成回调</param>
        /// <param name="data">自定义数据</param>
        /// <param name="warpGameObject">包装的实体对象</param>
        /// <returns>创建的物体道具</returns>
        public Skill CreateSkill( string guid, string resouceName, bool isSync, SkillAttribute attribute = null,
            OnCreated func = null, object data = null, GameObject warpGameObject = null )
        {
            Skill obj = null;
            if (string.IsNullOrEmpty(guid))
                obj = (Skill) ObjectManager.Instance.CreateObject(EnFactoryType.SkillFactory);
            else
                obj = (Skill) ObjectManager.Instance.CreateObject(guid, EnFactoryType.SkillFactory);

            if (obj != null)
            {
                obj.SetAttribute(attribute);
                obj.SetOwner((BaseObject) data);
                if (func == null)
                    obj.SetOnCreated(OnCreatedFunc, data);
                else
                    obj.SetOnCreated(func, data);
                obj.Load(false, warpGameObject);
            }

            return obj;
        }

        /// <summary>
        /// 创建道具对象
        /// </summary>
        /// <param name="guid">道具guid</param>
        /// <param name="itemId">数据id</param>
        /// <param name="func">创建完成回调</param>
        /// <param name="data">自定义数据</param>
        /// <param name="warpGameObject">上层包装对象</param>
        /// <returns>底层Player 对象</returns>
        public Skill CreateSkill( string guid, int itemId, OnCreated func, object data = null,
            GameObject warpGameObject = null )
        {
            var skillDefine =
                DataManager.Instance.GetData<SkillDefine>((int) EnLocalDataType.DATA_SKILL, itemId);
            if (skillDefine != null)
            {
                var attribute = skillDefine.Create<SkillAttribute>(0);
                func = func ?? OnItemCreatedFunc;
                var obj = CreateSkill(guid, string.Empty, false, attribute, func, data, warpGameObject);
                return obj;
            }
            return null;
        }

        public void DestorySkill( string guid )
        {
            ObjectManager.Instance.DestroyObject(guid, EnFactoryType.SkillFactory);
        }

        #endregion

        #region 对象回调

        /// <summary>
        /// 实体创建回调（仅包含场景物体和角色）
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="render">渲染对象</param>
        /// <param name="data">自定义数据</param>
        public void OnCreatedFunc( BaseObject obj, object render, object data )
        {
            var represent = obj.GetRepresent();

            if (data is SpawnSceneObjectInfo spawnInfo)
            {
                represent.transform.position = spawnInfo.position;
                represent.transform.rotation = spawnInfo.rotation;
                represent.transform.localScale = spawnInfo.scale;
            }
            else if (data is InteractiveDataInfo sceneInfo)
            {
                represent.transform.position = sceneInfo.position;
                represent.transform.localEulerAngles = sceneInfo.angle;
                represent.transform.localScale = sceneInfo.scale;
            }

            if (Facade.Instance.RetrieveProxy(SceneInfoProxy.NAME) is SceneInfoProxy sceneInfoProxy)
                // 初始化对象风向
                // ReSharper disable once Unity.NoNullPropagation
                obj.SceneInstance?.OnWind(sceneInfoProxy.GetWindDirection(), sceneInfoProxy.GetWindLevel());
            // hack:全局信息同步

            // 自定义事件
            // OnPlatCreatedFunc(obj, represent);

            // 同步网络属性设置
            obj.GetAttribute().ID = obj.Guid;

            //hack:更新同步网络属性设置

            obj.GetAttribute()?.Apply(data);

            if (data == null)
                Facade.Instance.SendNotification((ushort) EnInputEvent.FinishAddObject, obj);
        }

        /// <summary>
        /// 实体创建回调（仅包含场景物体和角色）
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="render">渲染对象</param>
        /// <param name="data">自定义数据</param>
        public void OnPlayerCreatedFunc( BaseObject obj, object render, object data )
        {
            var represent = obj.GetRepresent();

            if (data is SpawnSceneObjectInfo spawnInfo)
            {
                represent.transform.position = spawnInfo.position;
                represent.transform.rotation = spawnInfo.rotation;
                represent.transform.localScale = spawnInfo.scale;
            }
            else if (data is UnitDataInfoInfo sceneInfo)
            {
                represent.transform.position = sceneInfo.position;
                represent.transform.localEulerAngles = sceneInfo.angle;
                represent.transform.localScale = sceneInfo.scale;
            }
            /*else if (data is KeyValuePair<NpcDataInfo, UnitDataInfoInfo> keyValue)
            {
                sceneInfo = keyValue.Value;
                represent.transform.position = sceneInfo.position;
                represent.transform.localEulerAngles = sceneInfo.angle;
                represent.transform.localScale = sceneInfo.scale;
            }*/

            if (Facade.Instance.RetrieveProxy(SceneInfoProxy.NAME) is SceneInfoProxy sceneInfoProxy)
            {
                // 初始化对象风向(角色不受风向控制)
                // obj.SceneInstance?.SetWind(sceneInfoProxy.GetWindDirection(), sceneInfoProxy.GetWindVelocity());
                // hack:全局信息同步
            }
            //自定义事件
            //OnPlatCreatedFunc(obj);

            // 同步网络属性设置
            obj.GetAttribute().ID = obj.Guid;
            obj.GetAttribute()?.Apply(data);

            // hud
            // (obj as Player)?.InitHUD(represent.transform, Util.GetPlayerName(obj), string.Empty);
        }

        /// <summary>
        /// 实体创建回调（仅包含场景物体和角色）
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="render">渲染对象</param>
        /// <param name="data">自定义数据</param>
        public void OnVehicleCreatedFunc( BaseObject obj, object render, object data )
        {
            var represent = obj.GetRepresent();

            if (data is SpawnSceneObjectInfo spawnInfo)
            {
                represent.transform.position = spawnInfo.position;
                represent.transform.rotation = spawnInfo.rotation;
                represent.transform.localScale = spawnInfo.scale;
            }
            else if (data is InteractiveDataInfo sceneInfo)
            {
                represent.transform.position = sceneInfo.position;
                represent.transform.localEulerAngles = sceneInfo.angle;
                represent.transform.localScale = sceneInfo.scale;
            }
            /*else if (data is KeyValuePair<NpcDataInfo, UnitDataInfoInfo> keyValue)
            {
                represent.transform.position = keyValue.Value.position;
                represent.transform.localEulerAngles = keyValue.Value.angle;
                represent.transform.localScale = keyValue.Value.scale;
            }*/

            if (Facade.Instance.RetrieveProxy(SceneInfoProxy.NAME) is SceneInfoProxy sceneInfoProxy)
            {
                // 初始化对象风向
                //obj.SceneInstance?.OnWind(sceneInfoProxy.GetWindDirection(), sceneInfoProxy.GetWindLevel());
                // hack:全局信息同步
            }

            //自定义事件
            //OnPlatCreatedFunc(obj);

            // 同步网络属性设置
            obj.GetAttribute().ID = obj.Guid;
            obj.GetAttribute()?.Apply(data);

            // hud
            (obj as Vehicle)?.InitHUD(represent.transform, ObjectHelper.GetPlayerName(obj), string.Empty);
        }

        /// <summary>
        /// 道具创建回调
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="render"></param>
        /// <param name="data"></param>
        public void OnItemCreatedFunc( BaseObject obj, object render, object data )
        {
            var represent = obj.GetRepresent();

            // 同步网络属性设置
            obj.GetAttribute()?.Apply(data);
        }

        /// <summary>
        /// 实体创建同步回调
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="render">渲染对象</param>
        /// <param name="data">自定义数据</param>
        public void OnSyncPlayerCreatedFunc( BaseObject obj, object render, object data )
        {
            // no net
            OnPlayerCreatedFunc(obj, render, data);
        }

        /// <summary>
        /// 实体创建同步回调
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="render">渲染对象</param>
        /// <param name="data">自定义数据</param>
        public void OnSyncVehicleCreatedFunc( BaseObject obj, object render, object data )
        {
            // no net
            OnVehicleCreatedFunc(obj, render, data);

            ((VehicleInstance) obj.SceneInstance).SetAuthiroty(false);
        }

        private void OnPlatCreatedFunc( BaseObject baseObject, GameObject render )
        {
            //hud:临时方案 by xuxin 后期以配表或者预案配置为准
            if (baseObject.SceneInstance is BaseInteractiveInstance
                || baseObject.SceneInstance is SceneTriggerInstance)
            {
                var hud = render.GetComponentInChildren<HudElement>();
                if (hud == null)
                {
                    hud = render.AddComponent<HudElement>();
                    hud.showIndicator = true;
                    hud.ignoreIndicatorHideDistance = true;
                    hud.showOffscreenIndicatorDistance = true;
                    hud.Prefabs.IndicatorPrefab = Resources.Load<ScriptIndicator>("UI/HUD/IndicatorPrefab");
                }
            }
        }

        #endregion

        #region UI

        public override void InitRootCanvasLoading( UnityAction complete )
        {
            //base.InitRootCanvasLoading(complete);

            var canvas = Resources.Load<GameObject>("UI/Forms/Canvas/Canvas");

            if (canvas != null)
            {
                var goClone = Instantiate(canvas);
                goClone.name = goClone.name.Replace("(Clone)", "");
                complete.Invoke();
            }
            else
            {
                SsitDebug.Error("Canvas is load exception");
            }
        }

        public override void OpenLoadingForm()
        {
            Facade.Instance.SendNotification((ushort) UIMsg.OpenForm,
                new UIParam {formId = (int) En_UIForm.LoadingForm, isAsync = false});
        }

        public override void CloseLoadingForm()
        {
            Facade.Instance.SendNotification((ushort) UIMsg.CloseForm, En_UIForm.LoadingForm);
        }

        #endregion

        #region 动画

        // 播放动画
        public bool PlayAnimation( BaseObject obj, string animationName, int layer = 0, bool glide = false,
            float fixedOrNormalizeTime = 0, OnTimerEventHandler func = null )
        {
            var go = obj.GetRepresent();
            if (go == null)
                return false;
            var animator = go.GetComponent<Animator>();
            if (animator == null)
            {
                var body = go.transform.Find("Skeleton");
                if (body != null) animator = body.GetComponent<Animator>();
            }

            if (animator == null) return false;

            var isPlay = false;
            var info = animator.GetCurrentAnimatorStateInfo(layer);


            isPlay = !info.IsName("Null");

            if (isPlay)
            {
                NotifyTimerEventStop(mAnimTimerEvents, obj);
                NotifyIEnumeratorRemove(mEnumrateMap, obj);
                animator.Play("Null", layer);
            }

            //Play

            if (!glide)
            {
                if (fixedOrNormalizeTime != 0)
                    animator.Play(animationName, layer);
                else
                    animator.PlayInFixedTime(animationName, layer, fixedOrNormalizeTime);
            }
            else
            {
                if (fixedOrNormalizeTime != 0)
                    animator.CrossFade(animationName, layer);
                else
                    animator.CrossFadeInFixedTime(animationName, fixedOrNormalizeTime, layer);
            }

            if (string.IsNullOrEmpty(animationName)) return true;

            if (func != null)
            {
                var enumerator = OnStartAnimationIEnumerator(obj, animator, animationName, layer, func);
                NotifyIEnumeratorAdd(mEnumrateMap, obj, enumerator);
                StartPlatCoroutine(enumerator);
            }


            return true;
        }


        private IEnumerator OnStartAnimationIEnumerator( BaseObject obj, Animator animator, string animationName,
            int layer,
            OnTimerEventHandler func = null )
        {
            //Wait until we enter the current state glide = true当前系统不在考虑范围内(这种方式比较准确，但是需要用协程不可控因素太多)
            while (!animator.GetCurrentAnimatorStateInfo(layer).IsName(animationName)) yield return null;

            //Debug.Log("OnStartAnimationIEnumerator" + animationName);

            var info = animator.GetCurrentAnimatorStateInfo(layer);
            if (info.IsName(animationName))
            {
                var length = info.length;
                if (!info.loop)
                {
                    var data =
                        new KeyValuePair<string, KeyValuePair<BaseObject, OnTimerEventHandler>>(animationName,
                            new KeyValuePair<BaseObject, OnTimerEventHandler>(obj, func));
                    var tv = AddTimerEvent(TimerEventType.TeveOnce, 0, length, 0, OnAnimateEnd, data);
                    if (tv != null)
                        NotifyTimerEventAdd(mAnimTimerEvents, obj, tv);
                }
            }
            NotifyIEnumeratorRemove(mEnumrateMap, obj);
            yield return null;
        }

        private void OnAnimateEnd( TimerEventTask eve, float timeelapsed, object data )
        {
            var param = (KeyValuePair<string, KeyValuePair<BaseObject, OnTimerEventHandler>>) data;
            var func = param.Value.Value;
            NotifyTimerEventRemove(mAnimTimerEvents, param.Value.Key, eve);

            if (param.Value.Key.IsDirty)
                return;

            if (func != null)
                func(null, timeelapsed, param.Key);
        }

        private void NotifyTimerEventStop( TimerEventMap map, BaseObject obj )
        {
            if (!map.ContainsKey(obj))
                return;

            var list = map[obj];
            for (var i = 0; i < list.Count; ++i)
            {
                var tv = list[i];

                if (tv.Handler != null)
                    tv.Handler(null, -1, tv.Data);

                Instance.RemoveTimerEvent(tv);
            }
            list.Clear();
        }

        private void NotifyTimerEventAdd( TimerEventMap map, BaseObject obj, TimerEventTask eve )
        {
            List<TimerEventTask> list = null;

            if (map.ContainsKey(obj))
            {
                list = map[obj];
            }
            else
            {
                list = new List<TimerEventTask>();
                map.Add(obj, list);
            }

            list.Add(eve);
        }

        private void NotifyTimerEventRemove( TimerEventMap map, BaseObject obj, TimerEventTask eve )
        {
            if (!map.ContainsKey(obj))
                return;
            map[obj].Remove(eve);

            if (map.Count == 0)
                map.Remove(obj);
        }

        private void NotifyIEnumeratorAdd( EnumrateMap map, BaseObject obj, IEnumerator eve )
        {
            NotifyIEnumeratorRemove(map, obj);
            if (map.ContainsKey(obj))
                map[obj] = eve;
            else
                map.Add(obj, eve);
        }

        private void NotifyIEnumeratorRemove( EnumrateMap map, BaseObject obj )
        {
            if (!map.ContainsKey(obj))
                return;
            StopCoroutine(map[obj]);
            map.Remove(obj);
        }

        #endregion

        #region 任务模块

        //任务创建
        public SsitEngine.QuestManager.Quest CreateQuest( string id, string title, string group, string desc,
            QuestTemplate questTemplate,
            List<QuestContent> rewardsUiContents = null, List<RewardSystem> rewardSystems = null )
        {
            if (Engine.Instance.HasModule(typeof(QuestManager).FullName))
                return QuestManager.Instance.QuestHelper?.ConvertTemplateToQuest(id, title, group, desc, questTemplate,
                    rewardsUiContents, rewardSystems);
            Debug.LogWarning("程序缺少任务模块");
            return null;
        }

        //任务步骤创建

        /// <summary>
        /// 创建通用单条件任务步骤
        /// </summary>
        public Step CreateStep( En_QuestsMsg questType, string name, string desc, string countName, string message,
            string param, int score, int requireValue = 0, bool hasFailtureNode = false )
        {
            if (Engine.Instance.HasModule(typeof(QuestManager).FullName))
                return ((QuestHelper) QuestManager.Instance.QuestHelper)?.CreateStep(questType, name, desc, countName,
                    message, param, score, requireValue, hasFailtureNode);
            Debug.LogWarning("程序缺少任务模块");
            return null;
        }

        /// <summary>
        /// 创建多参数条件的任务步骤（举例:创建公议规则任务步骤)
        /// </summary>
        public Step CreateStep( En_QuestsMsg questType, string name, string desc, string countName, string message,
            List<string> openList, List<string> closemList, int score )
        {
            if (Engine.Instance.HasModule(typeof(QuestManager).FullName))
                return ((QuestHelper) QuestManager.Instance.QuestHelper)?.CreateStep(questType, name, desc, countName,
                    message, openList, closemList, score);
            Debug.LogWarning("程序缺少任务模块");
            return null;
        }

        /// <summary>
        /// 创建多参数条件的任务步骤（举例：穿戴个体防护）
        /// </summary>
        public Step CreateStep( En_QuestsMsg questType, string name, string desc, string countName, string message,
            List<string> paramList, int score )
        {
            if (Engine.Instance.HasModule(typeof(QuestManager).FullName))
                return ((QuestHelper) QuestManager.Instance.QuestHelper)?.CreateStep(questType, name, desc, countName,
                    message, paramList, score);
            Debug.LogWarning("程序缺少任务模块");
            return null;
        }

        //任务销毁
        public void DeleteQuest( SsitEngine.QuestManager.Quest quest )
        {
            //todo:
        }

        //任务发送

        /// <summary>
        /// 发送任务
        /// </summary>
        /// <param name="quest">任务</param>
        /// <param name="questGiverId">任务发送者</param>
        /// <param name="questerId">任务执行者</param>
        /// <param name="mode">执行模式</param>
        public void GiveQuestToQuester( SsitEngine.QuestManager.Quest quest, string questGiverId, string questerId,
            QuestCompleteMode mode = QuestCompleteMode.SingleComplet )
        {
            if (Engine.Instance.HasModule(typeof(QuestManager).FullName))
                QuestManager.Instance.GiveQuestToQuester(quest, questGiverId, questerId, mode);
            else
                Debug.LogWarning("程序缺少任务模块");
        }

        //添加任务（单机任务接口）
        public void AddQuestToQuester( SsitEngine.QuestManager.Quest quest, string questGiverId, string questerId,
            QuestCompleteMode mode = QuestCompleteMode.SingleComplet )
        {
            if (Engine.Instance.HasModule(typeof(QuestManager).FullName))
            {
                var questGiverTextInfo = new QuestParticipantTextInfo(questGiverId, questGiverId, null);
                var questerTextInfo = new QuestParticipantTextInfo(questerId, questerId, null);
                // Add the copy to the quester and activate it:
                quest.AssignQuestGiver(questGiverTextInfo);
                quest.AssignQuester(questerTextInfo, mode);

                QuestManager.Instance.SysQuestJournal.AddQuest(quest);
            }
            else
            {
                Debug.LogWarning("程序缺少任务模块");
            }
        }
        //任务回调

        /// <summary>
        /// 注册任务多人条件检测回调
        /// </summary>
        /// <param name="func"></param>
        public void RegisterQuestMutilPlayerCallBack( OnQuestComleteAttachConditionHandler func )
        {
            //if (Engine.Instance.HasModule(typeof(QuestManager).FullName))
            {
                QuestManager.Instance.SetAttachHandler(func);
            }
        }

        /// <summary>
        /// 添加任务状态改变回调
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public bool AddQuestStateChangeListener( QuestParameterDelegate func )
        {
            //if (!Engine.Instance.HasModule(typeof(QuestManager).FullName))
            //{
            //    return false;
            //}
            QuestManager.Instance.SysQuestJournal.questStateChanged -= func;
            QuestManager.Instance.SysQuestJournal.questStateChanged += func;
            return true;
        }

        /// <summary>
        /// 移除任务状态改变回调
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public bool RemoveQuestStateChangeListener( QuestParameterDelegate func )
        {
            //if (!Engine.Instance.HasModule(typeof(QuestManager).FullName))
            //{
            //    return false;
            //}
            QuestManager.Instance.SysQuestJournal.questStateChanged -= func;
            return true;
        }

        /// <summary>
        /// 任务添加回调
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public bool AddQuestAddListener( QuestParameterDelegate func )
        {
            //if (!Engine.Instance.HasModule(typeof(QuestManager).FullName))
            //{
            //    return false;
            //}
            QuestManager.Instance.SysQuestJournal.questAdded -= func;
            QuestManager.Instance.SysQuestJournal.questAdded += func;
            return true;
        }

        /// <summary>
        /// 移除任务添加回调
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public bool RemoveQuestAddListener( QuestParameterDelegate func )
        {
            if (!Engine.Instance.HasModule(typeof(QuestManager).FullName)) return false;
            QuestManager.Instance.SysQuestJournal.questAdded -= func;
            return true;
        }

        #endregion

        #region Invoke Async

        /// <summary>
        /// 延时执行，封装携程，统一管理
        /// </summary>
        /// <param name="action"></param>
        public void InvokeWaitForEndOfFrame( UnityAction action )
        {
            StartCoroutine(WaitForEndOfFrame(action));
        }

        private IEnumerator WaitForEndOfFrame( UnityAction action )
        {
            yield return new WaitForEndOfFrame();

            action?.Invoke();
        }

        #endregion
    }
}