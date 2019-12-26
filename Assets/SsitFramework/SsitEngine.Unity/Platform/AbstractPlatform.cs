using System;
using System.Collections;
using SsitEngine.Core;
using SsitEngine.Core.ObjectPool;
using SsitEngine.Core.ReferencePool;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Unity.ObjectPool;
using SsitEngine.Unity.Resource;
using SsitEngine.Unity.Setting;
using SsitEngine.Unity.Timer;
using SsitEngine.Unity.UI;
using SsitEngine.Unity.WebRequest;
using UnityEngine;
using UnityEngine.Events;

namespace SsitEngine.Unity
{
    /// <summary>
    ///     抽象代理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AbstractPlatform<T> : SingletonMono<T>, IPlatform where T : SingletonMono<T>
    {
        [SerializeField] private PlatformConfig m_platformConfig;

        private ITimerEventTaskHelper m_timerEventTaskHelper;

        /// <inheritdoc />
        public PlatformConfig PlatformConfig
        {
            get
            {
                if (m_platformConfig == null)
                {
                    m_platformConfig = Resources.Load<PlatformConfig>("PlatformConfig");
                }
                return m_platformConfig;
            }
            set => m_platformConfig = value;
        }

        /// <inheritdoc />
        public GameObject PlatformEntity => gameObject;

        /// <inheritdoc />
        public virtual void OnStart( GameObject main )
        {
            StartApp();
        }

        /// <inheritdoc />
        public void StartApp()
        {
            InitThirdLibConfig();
            InitAppEnvironment();
            StartGame();
        }

        /// <inheritdoc />
        public override void OnSingletonInit()
        {
            m_timerEventTaskHelper = new TimerEventTaskHelper();
        }

        //-------------
        //-- 平台实现
        //-------------

        #region 文件处理

        /// <summary>
        ///     streaming 目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns>路径</returns>
        public string StreamingAssetsPath( string path )
        {
            var fullPath = Application.streamingAssetsPath + "/";
            if (Application.platform == RuntimePlatform.Android)
                //安卓自己会加头
            {
                fullPath = fullPath + path;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                fullPath = "file://" + fullPath + path;
            }
            else
                // Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor
            {
                fullPath = "file:///" + fullPath + path;
            }

            return fullPath;
        }

        #endregion


        #region 子类实现

        /// <inheritdoc />
        public virtual void InitThirdLibConfig()
        {
        }

        /// <inheritdoc />
        public virtual void InitAppEnvironment()
        {
        }

        /// <inheritdoc />
        public virtual void StartGame()
        {
            // 内嵌设置模块
            Engine.Instance.CreateModule(SettingManager.Instance);
            SettingManager.Instance.SetSettingHelper(new SettingHelper());

            // 内嵌计时器模块
            Engine.Instance.CreateModule(TimerManager.Instance);

            // 内嵌对象池模块
            Engine.Instance.CreateModule(ObjectPoolManager.Instance);

            // Sqlite本地数据库模块(2019版本设计移除 by jojo)
            // Engine.Instance.CreateModule(SqliteManager.Instance);

            // Web请求模块
            Engine.Instance.CreateModule(WebRequestManager.Instance);
        }

        /// <inheritdoc />
        public virtual void OnUpdate( float elapsed )
        {
        }

        /// <inheritdoc />
        public virtual void OnApplicationQuit()
        {
            IsApplicationQuit = true;
            Facade.Instance.SendNotification((ushort) EnEngineEvent.OnApplicationQuit);
        }

        /// <inheritdoc />
        public virtual void OnApplicationPause( bool pauseStatus )
        {
            Facade.Instance.SendNotification((ushort) EnEngineEvent.OnApplicationPauseChange, pauseStatus);
        }

        /// <inheritdoc />
        public virtual void OnApplicationFocus( bool focusStatus )
        {
            Facade.Instance.SendNotification((ushort) EnEngineEvent.OnApplicationFocusChange, focusStatus);
        }

        /// <inheritdoc />
        public Coroutine StartPlatCoroutine( IEnumerator iEnumerator )
        {
            return StartCoroutine(iEnumerator);
        }

        /// <inheritdoc />
        public void StopPlatCoroutine( Coroutine coroutine )
        {
            StopCoroutine(coroutine);
        }

        /// <inheritdoc />
        public void StopAllPlatCoroutine()
        {
            StopAllCoroutines();
        }

        /// <inheritdoc />
        public void ReleaseResources()
        {
            ReferencePool.ClearAll();
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        #endregion

        #region 任务模块

        /*/// <summary>
        /// 发送任务
        /// </summary>
        /// <param name="quest">任务</param>
        /// <param name="questGiverId">任务发送者</param>
        /// <param name="questerId">任务执行者</param>
        /// <param name="mode">执行模式</param>
        public void GiveQuestToQuester(Quest quest, string questGiverId, string questerId, QuestCompleteMode mode = QuestCompleteMode.SingleComplet)
        {
            if (SsitEngine.Instance.HasModule(typeof(QuestManager).FullName))
            {
                QuestManager.Instance.GiveQuestToQuester(quest, questGiverId, questerId, mode);
            }
            if (SsitEngine.Debug)
            {
                Debug.LogWarning("程序缺少任务模块");
            }
        }

        public void RegisterQuestMutilPlayerCallBack(OnQuestComleteAttachConditionHandler func)
        {
            if (SsitEngine.Instance.HasModule(typeof(QuestManager).FullName))
            {
                QuestManager.Instance.SetAttachHandler(func);
            }
        }
        public bool AddQuestStateChangeListener(QuestParameterDelegate func)
        {
            if (!SsitEngine.Instance.HasModule(typeof(QuestManager).FullName))
            {
                return false;
            }
            QuestManager.Instance.SysQuestJournal.questStateChanged -= func;
            QuestManager.Instance.SysQuestJournal.questStateChanged += func;
            return true;
        }

        public bool AddQuestAddListener(QuestParameterDelegate func)
        {
            if (!SsitEngine.Instance.HasModule(typeof(QuestManager).FullName))
            {
                return false;
            }
            QuestManager.Instance.SysQuestJournal.questAdded -= func;
            QuestManager.Instance.SysQuestJournal.questAdded += func;
            return true;
        }

        public bool RemoveQuestStateChangeListener(QuestParameterDelegate func)
        {
            if (!SsitEngine.Instance.HasModule(typeof(QuestManager).FullName))
            {
                return false;
            }
            QuestManager.Instance.SysQuestJournal.questStateChanged -= func;
            return true;
        }

        public bool RemoveQuestAddListener(QuestParameterDelegate func)
        {
            if (!SsitEngine.Instance.HasModule(typeof(QuestManager).FullName))
            {
                return false;
            }
            QuestManager.Instance.SysQuestJournal.questAdded -= func;
            return true;
        }*/

        #endregion

        #region 计时器

        /// <summary>
        ///     创建计时任务
        /// </summary>
        /// <param name="type">计时类型</param>
        /// <param name="priority">优先级</param>
        /// <param name="second">周期</param>
        /// <param name="span">间隔</param>
        /// <param name="func">回调</param>
        /// <param name="data">自定义数据</param>
        public virtual TimerEventTask AddTimerEvent( TimerEventType type, int priority, float second, float span,
            OnTimerEventHandler func, object data = null )
        {
            var task = m_timerEventTaskHelper.CreateTimeEventTask(type, priority, second, span, func, data);
            if (task != null && Engine.Instance.HasModule(typeof(TimerManager).FullName))
            {
                return TimerManager.Instance.AddTimerEvent(task);
            }
            return null;
        }

        /// <summary>
        ///     移除计时任务
        /// </summary>
        /// <param name="task"></param>
        public virtual bool RemoveTimerEvent( TimerEventTask task )
        {
            return m_timerEventTaskHelper.RemoveTimeEventTask(task);
        }

        #endregion

        #region 对象池

        /// <summary>
        ///     创建允许单次获取的对象池。
        /// </summary>
        /// <typeparam name="T1">对象类型。</typeparam>
        /// <param name="name">对象池名称。</param>
        /// <param name="autoReleaseInterval">对象池自动释放可释放对象的间隔秒数。</param>
        /// <param name="capacity">对象池的容量。</param>
        /// <param name="expireTime">对象池对象过期秒数。</param>
        /// <param name="priority">对象池的优先级。</param>
        /// <param name="loadFunction">对象池的对象加载回调。</param>
        /// <param name="spawncondition">对象池的过滤附加条件。</param>
        /// <returns>要创建的允许单次获取的对象池。</returns>
        public IObjectPool<T1> CreatePool<T1>( string name, float autoReleaseInterval,
            int capacity = ObjectPoolManager.DefaultCapacity, float expireTime = ObjectPoolManager.DefaultExpireTime,
            int priority = ObjectPoolManager.DefaultPriority, SsitFunction<T1> loadFunction = null,
            SsitFunction<bool> spawncondition = null ) where T1 : ObjectBase
        {
            if (Engine.Instance.HasModule(typeof(ObjectPoolManager).FullName))
            {
                return ObjectPoolManager.Instance.CreateSingleSpawnObjectPool(name, autoReleaseInterval, capacity,
                    expireTime, priority, loadFunction, spawncondition);
            }
            throw new SsitEngineException(TextUtils.Format("模块管理{0}系统加载异常", typeof(ObjectPoolManager).FullName));
        }

        /// <summary>
        ///     销毁对象池。
        /// </summary>
        /// <typeparam name="T1">对象类型。</typeparam>
        /// <param name="name">要销毁的对象池名称。</param>
        /// <returns>是否销毁对象池成功。</returns>
        public bool DestroyObjectPool<T1>( string name ) where T1 : ObjectBase
        {
            if (Engine.Instance.HasModule(typeof(ObjectPoolManager).FullName))
            {
                return ObjectPoolManager.Instance.DestroyObjectPool<T1>(name);
            }
            return false;
        }

        #endregion


        #region Web Request

        public ulong AddWebRequestTask( WebRequestInfo task )
        {
            if (WebRequestManager.Instance == null)
            {
                return 0;
            }
            return WebRequestManager.Instance.AddWebRequest(task);
        }

        public bool RemoveWebRequestTask( ulong taskid )
        {
            if (WebRequestManager.Instance == null)
            {
                return false;
            }
            return WebRequestManager.Instance.RemoveWebRequest(taskid);
        }

        #endregion

        #region UI

        /// <summary>
        ///     获取主Canvas
        /// </summary>
        public Canvas MainCanvas => UIManager.Instance.GetCanvas();

        /// <summary>
        ///     打开通用加载界面
        /// </summary>
        public virtual void OpenLoadingForm()
        {
        }

        /// <inheritdoc />
        public virtual void CloseLoadingForm()
        {
        }

        /// <inheritdoc />
        public virtual void InitRootCanvasLoading( UnityAction complete )
        {
            ResourcesManager.Instance.LoadAsset<GameObject>(0, false, go =>
                {
                    var goClone = Instantiate(go);
                    goClone.name = goClone.name.Replace("(Clone)", "");
                    complete.Invoke();
                }
            );
        }

        #endregion
    }
}