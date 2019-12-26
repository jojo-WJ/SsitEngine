using Framework.Data;
using Framework.SceneObject;
using SSIT.proto;
using SsitEngine.Unity.SceneObject;
using SsitEngine.Unity.Utility;
using UnityEngine;

namespace Framework.Logic
{
    public class SceneInfoProxy : SsitProxy
    {
        public new static string NAME = "SceneInfoProxy";

        private SceneInfo m_SceneInfo;

        public SceneInfoProxy() : base(NAME)
        {
            m_SceneInfo = new SceneInfo();


            //Facade.Instance.RegisterProxy(new SceneTipProxy());
        }


        /// <summary>
        /// 初始化设置天气信息（不会通知物体风向影响）
        /// </summary>
        /// <param name="info"></param>
        public void InitSetWeatherInfo( WeatherInfo info )
        {
            if (m_SceneInfo.weatherInfo.Weather != info.Weather ||
                m_SceneInfo.weatherInfo.WindDirection != info.WindDirection ||
                m_SceneInfo.weatherInfo.WindLevel != info.WindLevel)
            {
                m_SceneInfo.weatherInfo.Weather = info.Weather;
                m_SceneInfo.weatherInfo.WindDirection = info.WindDirection;
                m_SceneInfo.weatherInfo.WindLevel = info.WindLevel;
            }
        }


        /// <summary>
        /// 设置天气信息（runtime - 会通知物体风向影响）
        /// </summary>
        /// <param name="info"></param>
        public void SetWeather( WeatherInfo info )
        {
            if (m_SceneInfo == null) return;
            if (m_SceneInfo.weatherInfo.Weather != info.Weather ||
                m_SceneInfo.weatherInfo.WindDirection != info.WindDirection ||
                m_SceneInfo.weatherInfo.WindLevel != info.WindLevel)
            {
                m_SceneInfo.IsChange = true;
                m_SceneInfo.weatherInfo.Weather = info.Weather;
                m_SceneInfo.weatherInfo.WindDirection = info.WindDirection;
                m_SceneInfo.weatherInfo.WindLevel = info.WindLevel;

                //通知物体改变
                ObjectManager.Instance.Send2AllSceneObject((ushort) EnPropertyId.Wind, m_SceneInfo.weatherInfo);
                //todo：通知界面刷新
                //Facade.Instance.SendNotification((ushort)UIGuiderFormEvent.WeatherChanged);
            }
        }

        /// <summary>
        /// 获取环境信息
        /// </summary>
        /// <returns></returns>
        public WeatherInfo GetWeatherInfo()
        {
            return m_SceneInfo.weatherInfo;
        }

        /// <summary>
        /// 获取风向
        /// </summary>
        /// <returns></returns>
        public EnWindDirection GetWindDirection()
        {
            return m_SceneInfo.weatherInfo.WindDirection;
        }

        /// <summary>
        /// 获取风级
        /// </summary>
        /// <returns></returns>
        public int GetWindLevel()
        {
            return m_SceneInfo.weatherInfo.WindLevel;
        }

        /// <summary>
        /// 获取请求天气的环境信息
        /// </summary>
        /// <returns></returns>
        public EnvironmentInfo GetWeatherRequest()
        {
            var enInfo = new EnvironmentInfo();
            enInfo.Weather = (SSIT.proto.EnWeather) m_SceneInfo.weatherInfo.Weather;
            enInfo.WindDirection = (SSIT.proto.EnWindDirection) m_SceneInfo.weatherInfo.WindDirection;
            enInfo.WindLevel = m_SceneInfo.weatherInfo.WindLevel;
            enInfo.WindVelocity = (float) m_SceneInfo.weatherInfo.WindVelocity;
            return enInfo;
        }

        public SceneInfo GetSceneInfo()
        {
            return m_SceneInfo;
        }

        #region 子类实现

        public override void OnRegister()
        {
            base.OnRegister();
        }

        public override void OnRemove()
        {
            base.OnRemove();
            m_SceneInfo = null;
        }

        #endregion

        #region 回放写入

        public override string Guid
        {
            get => NAME;
            set { }
        }

        public override void InitEzReplay()
        {
            m_present = new GameObject(NAME);
            m_present.transform.SetParent(GlobalManager.Instance.transform);
        }

        public override SavedBase GeneralSaveData( bool isDeepClone = false )
        {
            var ret = m_SceneInfo;
            if (isDeepClone)
            {
                var temp = SerializationUtils.Clone(ret);
                ret.IsChange = false;
                return temp;
            }
            return ret;
        }


        public override void SynchronizeProperties( SavedBase savedState, bool isReset, bool isFristFrame )
        {
            var info = savedState as SceneInfo;

            if (info == null)
                return;

            SetWeather(info.weatherInfo);
        }

        #endregion
    }
}