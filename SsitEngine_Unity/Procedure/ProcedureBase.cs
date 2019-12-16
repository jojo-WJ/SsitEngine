/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：系统状态进程                                                   
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/18 15:08:40              
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections;
using System.Collections.Generic;
using SsitEngine.DebugLog;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SsitEngine.Unity.Procedure
{
    /// <summary>
    ///     流程基类。
    /// </summary>
    public abstract class ProcedureBase
    {
        /// <summary>
        ///     进程事件处理器
        /// </summary>
        private readonly Dictionary<int, ProcedureEventHandler> m_eventHandlerMaps;

        /// <summary>
        ///     进程ID
        /// </summary>
        protected int stateId;

        /// <summary>
        ///     流程构造
        /// </summary>
        /// <param name="stateId"></param>
        protected ProcedureBase( int stateId )
        {
            IsLoaded = false;
            IsEntered = false;
            IsAssetLoad = false;
            this.stateId = stateId;
            m_eventHandlerMaps = new Dictionary<int, ProcedureEventHandler>();
        }

        /// <summary>
        ///     场景名称
        /// </summary>
        public string SceneName { get; set; }

        /// <summary>
        ///     资源加载是否完成
        /// </summary>
        public bool IsAssetLoad { get; set; }

        /// <summary>
        ///     是否加载完成
        /// </summary>
        public bool IsLoaded { get; set; }

        /// <summary>
        ///     是否进入
        /// </summary>
        public bool IsEntered { get; set; }

        /// <summary>
        ///     进程自定义的数据
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        ///     进程ID
        /// </summary>
        public int StateId => stateId;

        /// <summary>
        ///     状态初始化时调用。
        /// </summary>
        /// <param name="procedureManager">流程持有者。</param>
        protected internal virtual void OnInit( IProcedureManager procedureManager )
        {
        }

        /// <summary>
        ///     状态预加载
        /// </summary>
        protected internal virtual void OnPrestrain()
        {
        }

        /// <summary>
        ///     进程的延迟加载
        /// </summary>
        /// <returns></returns>
        protected internal virtual IEnumerator StartLoading()
        {
            // delay timer
            IsAssetLoad = true;
            if (!string.IsNullOrEmpty(SceneName))
            {
                //todo:加载场景bundle

                //加载场景
                var displayProgress = 0;
                var toProgress = 0;
#if UNITY_5
                AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(SceneName);//异步加载
#else
                var operation = SceneManager.LoadSceneAsync(SceneName);
#endif
                operation.allowSceneActivation = false;
                while (operation.progress < 0.9f)
                {
                    toProgress = (int) (operation.progress * 100);
                    while (displayProgress < toProgress)
                    {
                        displayProgress += 1;
                        RefreshLoadProcess(displayProgress / 100.0f);
                        yield return new WaitForFixedUpdate();
                    }
                    yield return new WaitForFixedUpdate();
                }
                var step = (100 - displayProgress) / 10;
                toProgress = 100;
                while (displayProgress < toProgress)
                {
                    displayProgress += step;
                    RefreshLoadProcess(displayProgress / 100.0f);
                    yield return new WaitForFixedUpdate();
                }
                operation.allowSceneActivation = true;
            }
            Engine.Instance.Platform.ReleaseResources(); //卸载内存
            GC.Collect();
            IsLoaded = true;
            yield return null;
        }

        /// <summary>
        ///     进入流程状态时调用。
        /// </summary>
        /// <param name="procedureManager">流程持有者。</param>
        protected internal virtual void OnEnter( IProcedureManager procedureManager )
        {
            IsEntered = true;
        }

        /// <summary>
        ///     状态轮询时调用。
        /// </summary>
        /// <param name="procedureManager">流程持有者。</param>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        protected internal virtual void OnUpdate( IProcedureManager procedureManager, float elapseSeconds )
        {
        }

        /// <summary>
        ///     离开状态时调用。
        /// </summary>
        /// <param name="procedureManager">流程持有者。</param>
        /// <param name="isShutdown">是否是关闭状态机时触发。</param>
        protected internal virtual void OnExit( IProcedureManager procedureManager, bool isShutdown )
        {
            m_eventHandlerMaps.Clear();
            IsAssetLoad = false;
            IsEntered = false;
            IsLoaded = false;
        }


        /// <summary>
        ///     状态销毁时调用。
        /// </summary>
        /// <param name="procedureManager">流程持有者。</param>
        protected internal virtual void OnDestroy( IProcedureManager procedureManager )
        {
            m_eventHandlerMaps.Clear();
        }


        /// <summary>
        ///     订阅流程事件。
        /// </summary>
        /// <param name="eventId">事件编号。</param>
        /// <param name="eventHandler">有限状态机事件响应函数。</param>
        public void SubscribeEvent( int eventId, ProcedureEventHandler eventHandler )
        {
            if (eventHandler == null) throw new SsitEngineException("Event handler is invalid.");

            if (!m_eventHandlerMaps.ContainsKey(eventId))
                m_eventHandlerMaps[eventId] = eventHandler;
            else
                m_eventHandlerMaps[eventId] += eventHandler;
        }

        /// <summary>
        ///     取消流程事件。
        /// </summary>
        /// <param name="eventId">事件编号。</param>
        /// <param name="eventHandler">有限状态机事件响应函数。</param>
        public void UnsubscribeEvent( int eventId, ProcedureEventHandler eventHandler )
        {
            if (eventHandler == null) throw new SsitEngineException("Event handler is invalid.");

            if (m_eventHandlerMaps.ContainsKey(eventId)) m_eventHandlerMaps[eventId] -= eventHandler;
        }


        /// <summary>
        ///     响应流程事件时调用。
        /// </summary>
        /// <param name="procedureManager">流程持有者。</param>
        /// <param name="sender">事件源。</param>
        /// <param name="eventId">事件编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnEvent( IProcedureManager procedureManager, object sender, int eventId, object userData )
        {
            ProcedureEventHandler eventHandlers = null;
            if (m_eventHandlerMaps.TryGetValue(eventId, out eventHandlers))
                if (eventHandlers != null)
                    eventHandlers(procedureManager, sender, userData);
        }

        /// <summary>
        ///     刷新加载进度
        /// </summary>
        public virtual void RefreshLoadProcess( float value )
        {
            if (Engine.Debug) SsitDebug.Debug("Scene loading " + value);
            //TODO:send view to refresh scene load process.
        }
    }
}