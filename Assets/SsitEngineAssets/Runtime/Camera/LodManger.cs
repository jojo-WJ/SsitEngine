/*
*┌──────────────────────────────────────────────────────────────┐
*│  作    者：Shell Lee
*│  版    本：1.0.0
*│  创建时间：20190610 19:04:48
*│  功能描述：
*│      本模块用于维护主相机脚本以及配置
*│  注意事项:
*│  
*│  修改人员：
*│  修改时间：
*│  修改内容：
*└──────────────────────────────────────────────────────────────┘
*/

using System;
using SsitEngine.DebugLog;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.SceneObject.Cull
{
    [Serializable]
    public class CameraConfig
    {
        public bool drawVolumeIntersect; // 绘制通过视锥体检测的八叉树区域
        public bool drawVolumeRendererShow; // 绘制有Renderer显示的八叉树区域
        public float LodDistancePipe = 25.0f;
        public float LodDistanceValve = 25.0f;

        public bool needReCalculateCulling;
        public float ViewpointTranslationTime = 4.0f;

        public bool ViewpointTranslationWithRotate;
    }

    public class LodManger : MonoBehaviour
    {
        public static UnityEvent OnMainCameraStart = new UnityEvent();

        public static LodManger _instance;

        [AddBindPath("OctreeCullCamera")] public Camera CullCamera;


        private Vector3 m_cameraPositionOld;

        [SerializeField] private CameraConfig m_config = new CameraConfig();

        public CameraConfig Config => m_config;

        private void Awake()
        {
            if (_instance != null) SsitDebug.Error("LodManger instance is exception");
            _instance = this;
        }

        public virtual void Start()
        {
            OnMainCameraStart?.Invoke();
        }

        protected virtual void Update()
        {
            Config.needReCalculateCulling = m_cameraPositionOld != transform.position;

            m_cameraPositionOld = transform.position;
        }

        /*protected virtual void OnPreCull()
        {

        }*/

        private void OnDestroy()
        {
            _instance = null;
        }
    }
}