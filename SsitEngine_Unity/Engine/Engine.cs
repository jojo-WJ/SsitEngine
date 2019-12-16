/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：框架入口                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/18 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections.Generic;
using System.Linq;
using SsitEngine.DebugLog;
using SsitEngine.Unity.Timer;
using UnityEngine;

namespace SsitEngine.Unity
{
    /// <summary>
    ///     引擎
    /// </summary>
    public class Engine : Singleton<Engine>, ISingleton
    {
        /// <summary>
        ///     模块链表
        /// </summary>
        private LinkedList<IModule> m_moduleMap;


        /// <summary>
        ///     等待加载模块
        /// </summary>
        private List<IModule> m_waitLoadModuleMap;

        /// <summary>
        ///     等待卸载模块
        /// </summary>
        private List<IModule> m_waitUnloadModuleMap;

        /// <summary>
        ///     引擎的调试开关
        /// </summary>
        public static bool Debug { get; set; } = false;

        /// <summary>
        ///     接入平台接口
        /// </summary>
        public IPlatform Platform { get; private set; }

        /// <summary>
        ///     平台配置
        /// </summary>
        public PlatformConfig PlatConfig { get; private set; }


        /// <summary>
        ///     场景根节点
        /// </summary>
        public Transform SceneRoot { get; private set; }

        public Transform ClientRoot { get; private set; }

        /// <summary>
        ///     场景根节点
        /// </summary>
        public Transform PlayerRoot { get; private set; }

        /// <summary>
        ///     引擎初始化
        /// </summary>
        public void OnSingletonInit()
        {
            SceneRoot = null;
            m_moduleMap = new LinkedList<IModule>();
            m_waitLoadModuleMap = new List<IModule>();
            m_waitUnloadModuleMap = new List<IModule>();
        }


        /// <summary>
        ///     获取对应平台
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetPlatform<T>() where T : AbstractPlatform<T>
        {
            return Platform as T;
        }

        /// <summary>
        ///     引擎启动
        /// </summary>
        public void Start( IPlatform platform )
        {
            Platform = platform;

            if (Platform == null)
            {
                SsitDebug.Fatal("框架平台missing");
                return;
            }
            if (Platform.PlatformConfig == null)
            {
                PlatConfig = ScriptableObject.CreateInstance<PlatformConfig>();
                PlatConfig.targetFrameRate = 60;
                PlatConfig.timeTaskAgentMaxCount = 50;
                PlatConfig.webTaskAgentMaxCount = 20;
                Platform.PlatformConfig = PlatConfig;
            }
            else
            {
                PlatConfig = Platform.PlatformConfig;
            }
            SsitDebug.Info("初始化平台配置。。。");

            Application.targetFrameRate = PlatConfig.targetFrameRate;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            SceneRoot = new GameObject("SceneRoot").transform;
            Object.DontDestroyOnLoad(SceneRoot);

            PlayerRoot = new GameObject("Players").transform;
            Object.DontDestroyOnLoad(PlayerRoot);

            if (PlatConfig.isSync)
            {
                ClientRoot = new GameObject("Clients").transform;
                Object.DontDestroyOnLoad(ClientRoot);
            }

            SsitDebug.Info("框架平台启动成功");
        }

        #region MoudleManager

        /// <summary>
        ///     创建框架模块。
        ///     模块轮询之前可用
        /// </summary>
        /// <param name="module">要创建的游戏框架模块类型。</param>
        /// <returns>要创建的游戏框架模块。</returns>
        public T CreateModule<T>( T module ) where T : MonoBase, IModule
        {
            if (module == null) throw new SsitEngineException(TextUtils.Format("Can not create module param isnull"));
            var curModule = m_moduleMap.First;
            while (curModule != null)
            {
                if (module.Priority > curModule.Value.Priority) break;
                curModule = curModule.Next;
            }
            if (curModule != null)
                m_moduleMap.AddBefore(curModule, module);
            else
                m_moduleMap.AddLast(module);
            module.transform.SetParent(Platform.PlatformEntity.transform);

            return module;
        }

        /// <summary>
        ///     添加框架模块
        /// </summary>
        /// <typeparam name="T">添加模块</typeparam>
        /// <param name="module"></param>
        /// <returns></returns>
        public T AddModule<T>( T module ) where T : MonoBase, IModule
        {
            if (module == null) throw new SsitEngineException(TextUtils.Format("Can not create module param isnull"));

            if (HasModule(module.ModuleName))
                return null;

            m_waitLoadModuleMap.Add(module);
            module.transform.SetParent(Platform.PlatformEntity.transform);
            return module;
        }

        /// <summary>
        ///     移除模块
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <returns></returns>
        public bool RemoveMoudle( string moduleName )
        {
            if (m_moduleMap == null) return false;
            var module = m_moduleMap.First(x => x.ModuleName == moduleName);
            if (null == module) return false;
            m_waitUnloadModuleMap.Add(module);
            return true;
        }

        /// <summary>
        ///     模块检测
        /// </summary>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public bool HasModule( string moduleName )
        {
            foreach (var module in m_moduleMap)
                if (module.ModuleName.Equals(moduleName))
                    return true;
            return false;
        }

        /// <summary>
        ///     获取游戏框架模块。
        /// </summary>
        /// <param name="moduleName">要获取的框架模块名称。</param>
        /// <returns>要获取的游戏框架模块。</returns>
        /// <remarks>如果要获取的游戏框架模块不存在，则自动创建该游戏框架模块。</remarks>
        public T GetModule<T>( string moduleName ) where T : class, IModule
        {
            foreach (var module in m_moduleMap)
                if (module.ModuleName.Equals(moduleName))
                    return module as T;
            throw new SsitEngineException(TextUtils.Format("Can not get module {0}", moduleName));
        }

        /// <summary>
        ///     所有游戏框架模块轮询。
        /// </summary>
        /// <param name="elapsed">逻辑流逝时间，以秒为单位。</param>
        public void Update( float elapsed )
        {
            foreach (var module in m_waitUnloadModuleMap)
            {
                m_moduleMap.Remove(module);
                module.Shutdown();
            }
            m_waitUnloadModuleMap.Clear();

            foreach (var module in m_waitLoadModuleMap) AddModule(module);
            m_waitLoadModuleMap.Clear();

            foreach (var module in m_moduleMap)
                if (module != null)
                    module.OnUpdate(elapsed);
        }

        /// <summary>
        ///     关闭并清理所有游戏框架模块。
        /// </summary>
        public void Shutdown()
        {
            if (m_moduleMap == null) return;
            if (m_moduleMap.Count > 0)
                for (var current = m_moduleMap.Last; current != null; current = current.Previous)
                    if (current.Value != null)
                        current.Value.Shutdown();

            m_moduleMap.Clear();
        }

        // Private Members

        /// <summary>
        ///     添加模块
        /// </summary>
        /// <param name="module">模块对象</param>
        private void AddModule( IModule module )
        {
            if (HasModule(module.ModuleName))
                return;

            var curModule = m_moduleMap.First;
            while (curModule != null)
            {
                if (module.Priority > curModule.Value.Priority) break;
                curModule = curModule.Next;
            }
            if (curModule != null)
                m_moduleMap.AddBefore(curModule, module);
            else
                m_moduleMap.AddLast(module);
        }

        #endregion
    }
}