using UnityEngine;

namespace SsitEngine.Unity
{
    /// <summary>
    ///     时间模式
    /// </summary>
    public enum GameTimeMode
    {
        /// <summary>
        ///     Direct mapping to Unity's Time class (e.g., Time.time).
        /// </summary>
        UnityStandard,

        /// <summary>
        ///     Realtime, ignoring Time.timeScale. Never pauses.
        /// </summary>
        Realtime,

        /// <summary>
        ///     Manually-controlled time. You must set GameTime.time and GameTime.deltaTime.
        /// </summary>
        Manual
    }

    /// <summary>
    ///     This is a wrapper around Unity's Time class that allows you to specify a mode:
    ///     UnityStandard (Time.time), Realtime (Time.realtimeSinceStartup), or Manual
    ///     (you set the time values each frame).
    /// </summary>
    public static class GameTime
    {
        // The target framerate. Application.targetFramerate can return 0 so it isn't used.
        private const int c_TargetFramerate = 60;

        private static float s_manualTime;
        private static float s_manualDeltaTime;
        private static bool s_manualPaused;

        /// <summary>
        ///     时间模式
        /// </summary>
        public static GameTimeMode Mode { get; set; } = GameTimeMode.UnityStandard;

        /// <summary>
        ///     程序时间
        /// </summary>
        public static float Time
        {
            get
            {
                switch (Mode)
                {
                    default:
                    case GameTimeMode.UnityStandard:
                        return UnityEngine.Time.time;
                    case GameTimeMode.Realtime:
                        return UnityEngine.Time.realtimeSinceStartup;
                    case GameTimeMode.Manual:
                        return s_manualTime;
                }
            }
            set => s_manualTime = value;
        }

        /// <summary>
        ///     真实流逝时间，以秒为单位
        /// </summary>
        public static float Realtime => UnityEngine.Time.realtimeSinceStartup;

        /// <summary>
        ///     时间间隔
        /// </summary>
        public static float DeltaTime
        {
            get
            {
                switch (Mode)
                {
                    default:
                    case GameTimeMode.UnityStandard:
                    case GameTimeMode.Realtime:
                        return UnityEngine.Time.deltaTime;
                    case GameTimeMode.Manual:
                        return s_manualDeltaTime;
                }
            }
            set => s_manualDeltaTime = value;
        }

        /// <summary>
        ///     时间缩放
        /// </summary>
        public static float TimeScale => UnityEngine.Time.timeScale;

        /// <summary>
        ///     时间暂停
        /// </summary>
        public static bool IsPaused
        {
            get
            {
                switch (Mode)
                {
                    default:
                    case GameTimeMode.UnityStandard:
                        return Mathf.Approximately(0, UnityEngine.Time.timeScale);
                    case GameTimeMode.Realtime:
                        return false;
                    case GameTimeMode.Manual:
                        return s_manualPaused;
                }
            }
            set
            {
                switch (Mode)
                {
                    default:
                    case GameTimeMode.UnityStandard:
                        UnityEngine.Time.timeScale = value ? 1 : 0;
                        break;
                    case GameTimeMode.Realtime:
                        break;
                    case GameTimeMode.Manual:
                        s_manualPaused = value;
                        break;
                }
            }
        }

        /// <summary>
        ///     返回基于帧速率的可选增量时间，其中“增量1”对应于60 fps
        /// </summary>
        /// <returns>The target framerate-based delta time</returns>
        public static float FramerateDeltaTime => UnityEngine.Time.deltaTime * c_TargetFramerate;

        /// <summary>
        ///     Returns the delta time modified by the timescale.
        /// </summary>
        /// <returns>Delta time modified by the timescale.</returns>
        public static float DeltaTimeScaled => UnityEngine.Time.deltaTime * UnityEngine.Time.timeScale;

        /// <summary>
        ///     Returns an alternative delta time which is based on framerate and timescale where "delta 1" corresponds to 60 FPS.
        /// </summary>
        /// <returns>The target framerate-based delta time.</returns>
        public static float FramerateDeltaTimeScaled =>
            UnityEngine.Time.deltaTime * c_TargetFramerate * UnityEngine.Time.timeScale;

        /// <summary>
        ///     Returns an alternative fixed delta time which is based on framerate where "delta 1" corresponds to 60 FPS.
        /// </summary>
        /// <returns>The target framerate-based fixed delta time.</returns>
        public static float FramerateFixedDeltaTime => UnityEngine.Time.fixedDeltaTime * c_TargetFramerate;

        /// <summary>
        ///     Returns an alternative fixed delta time which is based on framerate and timescale where "delta 1" corresponds to 60
        ///     FPS.
        /// </summary>
        /// <returns>The target framerate-based fixed delta time.</returns>
        public static float FramerateFixedDeltaTimeScaled => FramerateFixedDeltaTime * UnityEngine.Time.timeScale;

        /// <summary>
        ///     Returns the fixed delta time modified by the timescale.
        /// </summary>
        /// <returns>Fixed delta time modified by the timescale.</returns>
        public static float FixedDeltaTimeScaled => UnityEngine.Time.fixedDeltaTime * UnityEngine.Time.timeScale;
    }
}