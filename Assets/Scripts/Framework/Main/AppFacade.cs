/**
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019/3/27 19:43:09                     
*└──────────────────────────────────────────────────────────────┘
*/

using SsitEngine.PureMVC.Interfaces;
using SsitEngine.PureMVC.Patterns;
using SsitEngine.Unity;
using UnityEditor;
using UnityEngine;

namespace Framework
{
    public class AppFacade : Facade
    {
        /// <summary>
        /// 初始化包装器
        /// </summary>
        protected override void InitializeFacade()
        {
            base.InitializeFacade();
            Instance.RegisterObservers(this, (ushort) EnEngineEvent.OnApplicationStart, OnApplicationStart);
            Instance.RegisterObservers(this, (ushort) EnEngineEvent.OnApplicationQuit, OnApplicationQuit);
        }


        private void OnApplicationStart( INotification msgArgs )
        {
            //sitApplication.Instance.OnStart(msgArgs.Body as GameObject);
        }

        private void OnApplicationQuit( INotification obj )
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}