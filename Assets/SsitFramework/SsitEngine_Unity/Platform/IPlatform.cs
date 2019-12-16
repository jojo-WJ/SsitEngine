using System.Collections;
using SsitEngine.Core;
using SsitEngine.Core.ObjectPool;
using SsitEngine.Unity.ObjectPool;
using SsitEngine.Unity.Timer;
using SsitEngine.Unity.WebRequest;
using UnityEngine;
using UnityEngine.Events;

namespace SsitEngine.Unity
{
    /// <summary>
    ///     平台接口
    /// </summary>
    public interface IPlatform
    {
        /// <summary>
        ///     引擎配置文件
        /// </summary>
        PlatformConfig PlatformConfig { get; set; }

        /// <summary>
        ///     平台实体
        /// </summary>
        GameObject PlatformEntity { get; }

        Canvas MainCanvas { get; }


        /// <summary>
        ///     外包装器启动回调
        /// </summary>
        /// <param name="main"></param>
        void OnStart( GameObject main );

        /// <summary>
        ///     启动程序
        /// </summary>
        void StartApp();

        /// <summary>
        ///     初始化程序运行必须第三方库
        /// </summary>
        void InitThirdLibConfig();

        /// <summary>
        ///     初始化程序运行环境
        /// </summary>
        void InitAppEnvironment();

        /// <summary>
        ///     程序启动
        /// </summary>
        void StartGame();

        /// <summary>
        ///     程序轮询
        /// </summary>
        /// <param name="elapsed"></param>
        void OnUpdate( float elapsed );

        /// <summary>
        ///     程序退出
        /// </summary>
        void OnApplicationQuit();

        /// <summary>
        ///     程序暂停
        /// </summary>
        /// <param name="pauseStatus"></param>
        void OnApplicationPause( bool pauseStatus );

        /// <summary>
        ///     程序获得焦点
        /// </summary>
        /// <param name="focusStatus"></param>
        void OnApplicationFocus( bool focusStatus );

        /// <summary>
        ///     启动一个平台协程
        /// </summary>
        /// <param name="iEnumerator"></param>
        Coroutine StartPlatCoroutine( IEnumerator iEnumerator );

        /// <summary>
        ///     停止平台所有协程
        /// </summary>
        void StopPlatCoroutine( Coroutine coroutine );

        /// <summary>
        ///     停止平台所有协程
        /// </summary>
        void StopAllPlatCoroutine();

        /// <summary>
        ///     释放程序资源
        /// </summary>
        void ReleaseResources();

        //框架的

        //任务系统

        //void GiveQuestToQuester(Quest quest, string questGiverId, string questerId,QuestCompleteMode mode = QuestCompleteMode.SingleComplet);

        //void RegisterQuestMutilPlayerCallBack(OnQuestComleteAttachConditionHandler func);

        //bool AddQuestStateChangeListener(QuestParameterDelegate func);

        //bool RemoveQuestStateChangeListener( QuestParameterDelegate func );

        //bool AddQuestAddListener(QuestParameterDelegate func);

        //bool RemoveQuestAddListener(QuestParameterDelegate func);

        //--------
        //计时器
        //--------

        /// <summary>
        ///     创建计时任务
        /// </summary>
        /// <param name="type">计时类型</param>
        /// <param name="priority">优先级</param>
        /// <param name="second">周期</param>
        /// <param name="span">间隔</param>
        /// <param name="func">回调</param>
        /// <param name="data">自定义数据</param>
        TimerEventTask AddTimerEvent( TimerEventType type, int priority, float second, float span,
            OnTimerEventHandler func, object data = null );

        /// <summary>
        ///     移除计时任务
        /// </summary>
        /// <param name="task">计时任务</param>
        bool RemoveTimerEvent( TimerEventTask task );

        //--------
        // 对象池
        //--------

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
        IObjectPool<T1> CreatePool<T1>( string name, float autoReleaseInterval,
            int capacity = ObjectPoolManager.DefaultCapacity, float expireTime = ObjectPoolManager.DefaultExpireTime,
            int priority = ObjectPoolManager.DefaultPriority, SsitFunction<T1> loadFunction = null,
            SsitFunction<bool> spawncondition = null ) where T1 : ObjectBase;

        /// <summary>
        ///     销毁对象池。
        /// </summary>
        /// <typeparam name="T1">对象类型。</typeparam>
        /// <param name="name">要销毁的对象池名称。</param>
        /// <returns>是否销毁对象池成功。</returns>
        bool DestroyObjectPool<T1>( string name ) where T1 : ObjectBase;


        //--------
        // 文件流
        //--------


        //-------
        // 实体创建
        //-------

        // void CreateSceneObject(int itemid, string guid);

        //-------
        //环境设置
        //-------

        /// <summary>
        ///     添加http/file 文件下载上传等任务
        /// </summary>
        ulong AddWebRequestTask( WebRequestInfo task );

        /// <summary>
        ///     移除下载请求任务
        /// </summary>
        /// <param name="taskid"></param>
        /// <returns></returns>
        bool RemoveWebRequestTask( ulong taskid );


        //-------
        //UI
        //-------

        /// <summary>
        ///     打开统一加载界面
        /// </summary>
        void OpenLoadingForm();

        /// <summary>
        ///     关闭统一加载界面
        /// </summary>
        void CloseLoadingForm();

        /// <summary>
        ///     加载初始化界面
        /// </summary>
        /// <param name="complete"></param>
        void InitRootCanvasLoading( UnityAction complete );
    }
}