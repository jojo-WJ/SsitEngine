﻿using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using SsitEngine;
using SsitEngine.DebugLog;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace JxDebug
{
    public class JxDebug : MonoBehaviour, SsitDebug.ILogHelper, ISerializationCallbackReceiver
    {
        public delegate void OnOpenStateChanged( bool isOpen );

        private static JxDebug s_singleton;

        private static Settings s_settingsCopy;

        private static Settings s_settings;
        [SerializeField] protected OpenButton _openButton;
        private Image buttonBlocker;
        private Canvas canvas;

        [NonSerialized] private bool initialized;
        private int lastFrame;
        private float lastScreenHeight = Screen.height;
        [SerializeField] protected Logger logger;

        [SerializeField] protected GUISkin mGuiBackSkin;

        [SerializeField] protected GUISkin mGuiSkin;

        private bool mouseDragEventProcessed;
        private bool open;
        private Coroutine openCoroutine;
        private Vector2 position;
        private Settings serializedSettings;
        [SerializeField] protected SettingsPanel settingsPanel;

        [SerializeField] protected Toolbar toolbar;
        private Image windowBlocker;

        public bool IsOpen => openCoroutine != null || open;

        public float Height =>
            Mathf.Clamp(Setting.preferredHeight, 0, 1 - OpenButton.height * Setting.scale / Screen.height) *
            Screen.height / Setting.scale;

        public bool IsSettingsOpen { get; private set; }

        public OpenButton OpenButton => _openButton;

        public static JxDebug Singleton
        {
            get
            {
                if (s_singleton == null)
                {
                    Instantiate();
                }
                return s_singleton;
            }
        }

        public static Settings Setting
        {
            get
            {
                if (s_settings == null)
                {
                    s_settings = Resources.Load<Settings>("JxDebugSettings");
                    s_settingsCopy = Instantiate(Setting);
                }

                return s_settings;
            }
        }

        public void OnBeforeSerialize()
        {
            if (Application.isPlaying)
            {
                serializedSettings = s_settingsCopy;
            }
        }

        public void OnAfterDeserialize()
        {
        }


        public event OnOpenStateChanged onOpenStateChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            if (Setting.autoInstantiate)
            {
                Instantiate();
            }
        }

        private static void Instantiate()
        {
            s_singleton = FindObjectOfType<JxDebug>();
            if (s_singleton == null)
            {
                s_singleton = Instantiate(Resources.Load<JxDebug>("JxDebug"));
                s_singleton.gameObject.hideFlags = HideFlags.HideInHierarchy;
            }
        }

        private void Awake()
        {
            if (Singleton != this)
            {
                SsitDebug.Warning("There can only be one Console per project");
                DestroyImmediate(gameObject);
                return;
            }

            if (Setting.dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
            useGUILayout = false;
            openCoroutine = null;
            if (Setting.showGUIButton)
            {
                EvaluatePosition(0);
                CreateCanvas();
                InitSave();
                InitDebugInfo();
            }

            SsitDebug.SetLogHelper(this);
        }

        private void OnEnable()
        {
            if (Setting.showGUIButton)
            {
                Application.logMessageReceived += OnLogMessageReceived;
            }
        }

        private void OnDisable()
        {
            if (Setting.showGUIButton)
            {
                Application.logMessageReceived -= OnLogMessageReceived;
            }
        }

        private void OnDestroy()
        {
            Setting.CopyFrom(s_settingsCopy);
        }

        private void CreateCanvas()
        {
            canvas = gameObject.AddComponent<Canvas>();
            gameObject.AddComponent<GraphicRaycaster>();
            StartCoroutine(CreateBlockers());
        }

        private IEnumerator CreateBlockers()
        {
            yield return null;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            windowBlocker = new GameObject("Window Blocker").AddComponent<Image>();
            windowBlocker.transform.SetParent(canvas.transform);
            windowBlocker.color = Color.clear;
            windowBlocker.rectTransform.pivot = new Vector2(0, 1);
            windowBlocker.rectTransform.anchorMin = new Vector2(0, 1);
            windowBlocker.rectTransform.anchorMax = new Vector2(1, 1);

            buttonBlocker = new GameObject("Button Blocker").AddComponent<Image>();
            buttonBlocker.transform.SetParent(canvas.transform);
            buttonBlocker.color = Color.clear;
            buttonBlocker.rectTransform.pivot = new Vector2(0.5f, 1);
            buttonBlocker.rectTransform.anchorMin = new Vector2(0.5f, 1);
            buttonBlocker.rectTransform.anchorMax = new Vector2(0.5f, 1);
            yield return null;
            windowBlocker.rectTransform.anchoredPosition = Vector2.zero;
            RepositionBlockers(0);
        }

        public void Open()
        {
            if (!CanSetOpenState(true))
            {
                return;
            }
            SetOpenState(true);
        }

        public void Close()
        {
            if (!CanSetOpenState(false))
            {
                return;
            }
            SetOpenState(false);
        }

        public void ToggleOpen()
        {
            if (open)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        private bool CanSetOpenState( bool open )
        {
            return this.open != open && openCoroutine == null;
        }

        private void SetOpenState( bool open )
        {
            this.open = open;
            openCoroutine = StartCoroutine(OpenCoroutine());
            if (onOpenStateChanged != null)
            {
                onOpenStateChanged(open);
            }
        }

        public void OpenSettings()
        {
            IsSettingsOpen = true;
        }

        public void CloseSettings()
        {
            IsSettingsOpen = false;
        }

        public void ToggleSettings()
        {
            IsSettingsOpen = !IsSettingsOpen;
        }

        #region 界面弹出动画

        private IEnumerator OpenCoroutine()
        {
            if (openCoroutine != null)
            {
                yield break;
            }

            float time = 0;
            do
            {
                time = Mathf.Clamp(time + Time.unscaledDeltaTime, 0, Setting.animationDuration);
                var evaluateTime = time / Setting.animationDuration;
                if (!open)
                {
                    evaluateTime = 1 - evaluateTime;
                }
                EvaluatePosition(evaluateTime);
                RepositionBlockers(evaluateTime);
                yield return null;
            } while (time < Setting.animationDuration);

            openCoroutine = null;
        }

        private void EvaluatePosition( float percentage )
        {
            position = new Vector2(Screen.width * (Setting.animationX.Evaluate(percentage) - 1),
                Height * Setting.scale * (Setting.animationY.Evaluate(percentage) - 1));
        }

        private void RepositionBlockers( float percentage )
        {
            if (windowBlocker == null)
            {
                return;
            }
            var windowBlockerHeight = Height * Setting.scale * percentage;
            windowBlocker.rectTransform.sizeDelta = new Vector2(0, windowBlockerHeight);
            buttonBlocker.rectTransform.anchoredPosition = new Vector2(0, -windowBlockerHeight);
        }

        #endregion

        #region 界面绘制

        private bool ShouldSkipEvent()
        {
            if (Event.current.type != EventType.MouseDrag)
            {
                return false;
            }

            if (lastFrame != Time.frameCount)
            {
                lastFrame = Time.frameCount;
                mouseDragEventProcessed = false;
            }

            if (mouseDragEventProcessed)
            {
                return true;
            }
            mouseDragEventProcessed = true;
            return false;
        }

        private void Initialize()
        {
            Slider.Initialize();
            Toggle.Initialize();
            settingsPanel.Initialize();
            Tab.Initialize();
            toolbar.Initialize(logger);
            logger.Initialize();
            ScrollBar.Initialize();
            InitScreenShotListener();
            initialized = true;
            if (serializedSettings != null)
            {
                s_settingsCopy.CopyFrom(serializedSettings);
            }
        }

        private void Reposition()
        {
            if (Screen.height != lastScreenHeight)
            {
                if (openCoroutine == null && !IsOpen)
                {
                    EvaluatePosition(0);
                }
                lastScreenHeight = Screen.height;
            }
        }

        private void OnGUI()
        {
            if (!Setting.showGUIButton)
            {
                return;
            }
            if (mGuiSkin != GUI.skin)
            {
                GUI.skin = mGuiSkin;
            }
            if (ShouldSkipEvent())
            {
                return;
            }
            if (Event.current.type == EventType.Repaint)
            {
                Reposition();
            }

            if (!initialized)
            {
                Initialize();
            }

            DetectInput();
            ApplyScaleAndDraw();
        }

        private void DetectInput()
        {
            var e = Event.current;

            //Prevent tabbing
            if (e.type == EventType.Layout || e.type == EventType.Repaint)
            {
                return;
            }
            if (e.character == '\t')
            {
                e.Use();
            }
            else if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == Setting.openKey)
                {
                    ToggleOpen();
                }
                else
                {
                    if (!IsOpen)
                    {
                        return;
                    }
                    if (e.keyCode == KeyCode.DownArrow)
                    {
                        Navigate(1);
                    }
                    else if (e.keyCode == KeyCode.UpArrow)
                    {
                        Navigate(-1);
                    }
                }
            }
        }

        private void ApplyScaleAndDraw()
        {
            var oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one * Setting.scale);
            Draw();
            GUI.matrix = oldMatrix;
        }

        private void Draw()
        {
            if (Setting.showOpenButton)
            {
                OpenButton.Draw(Height);
            }
            if (buttonBlocker != null)
            {
                buttonBlocker.rectTransform.sizeDelta = Setting.showOpenButton
                    ? new Vector2(OpenButton.width * Setting.scale, OpenButton.height * Setting.scale)
                    : Vector2.zero;
            }
            if (!IsOpen)
            {
                return;
            }

            //When layouting, only AutoComplete & History need to be drawn
            if (Event.current.type == EventType.Layout)
            {
                return;
            }

            //Input has to be drawn first thing to keep focus across frames
            toolbar.Draw();
            var mainWindowHeight = Height - toolbar.height;
            if (IsSettingsOpen)
            {
                settingsPanel.Draw(toolbar.height, mainWindowHeight);
            }
            else
            {
                logger.Draw(toolbar.height, mainWindowHeight);
            }
        }

        private void Navigate( int direction )
        {
            Event.current.Use();
        }

        #endregion

        #region Debug

        private string folderPath;
        private string filePath;

        public void InitSave()
        {
#if UNITY_EDITOR
            folderPath = Application.dataPath;
#elif UNITY_STANDALONE_WIN
            folderPath = Environment.CurrentDirectory;
#else
            folderPath = Application.persistentDataPath;
#endif
            //文件路径
            filePath = Path.Combine(folderPath, "Log/Log.log");
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            //如果存在就重命名为oldlog.log
            if (File.Exists(filePath))
            {
                var info = new FileInfo(filePath);
                var newName = Path.Combine(folderPath, "Log/OldLog.log");
                //如果oldlog.log存在就删除
                if (File.Exists(newName))
                {
                    File.Delete(newName);
                }

                info.MoveTo(newName); //重命名
            }

            File.Create(filePath).Dispose();

            SsitDebug.Info("Log save File: " + filePath);
        }

        private void OnLogMessageReceived( string condition, string stackTrace, LogType type )
        {
            EntryData entry;
            if (type == LogType.Error || type == LogType.Exception)
            {
                entry = new EntryData(condition, stackTrace, null, type.ToString());
            }
            else
            {
                entry = new EntryData(condition, string.Empty, null, type.ToString());
            }

            switch (type)
            {
                case LogType.Assert:
                case LogType.Log:
                    Log(entry);
                    break;
                case LogType.Warning:
                    LogWarning(entry);
                    break;
                case LogType.Exception:
                case LogType.Error:
                    LogError(entry);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type", type, null);
            }
            //LogLevel level = (LogLevel)Enum.Parse(typeof(LogLevel), type.ToString());
            //if ((settings.attachedLogLevel & level) == level)
            //{
            //    EntryData entry = new EntryData(condition, level.ToString());
            //    entry.stackTrace = stackTrace;
            //    if (level == LogLevel.Exception || level == LogLevel.Error)
            //        LogError(entry);
            //    else if (level == LogLevel.Warning)
            //        LogWarning(entry);
            //    else
            //        Log(entry);
            //}

            //以附加方式打开文件写入流
            //#if  UNITY_5_5_OR_NEWER
            using (var fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
            {
                using (var sw = new StreamWriter(fs))
                {
                    //写入数据
                    sw.WriteLine(entry.ToString());
                }
            }
            //#endif
        }

        [Conditional("DEBUGLOG")]
        public void Log( EntryData data )
        {
            logger.AddEntry(data);
        }

        public void LogWarning( EntryData data )
        {
            data.options.color = Setting.warningColor;
            data.icon = Setting.warningIcon;
            logger.AddEntry(data);
        }

        public void LogError( EntryData data )
        {
            data.options.color = Setting.errorColor;
            data.icon = Setting.errorIcon;
            logger.AddEntry(data);
        }

        public void ClearLog()
        {
            logger.Clear();
        }

        public Entry[] GetEntries()
        {
            return logger.GetEntries();
        }

        #endregion

        #region 屏幕截图

        private string m_log = "...";

        private void InitScreenShotListener()
        {
            //CaptureAndSaveEventListener.onError += OnError;
            //CaptureAndSaveEventListener.onSuccess += OnSuccess;
        }

        private void OnError( string error )
        {
            m_log += "\n" + error;
            SsitDebug.Info("Screenshot Error : " + error);
        }

        private void OnSuccess( string msg )
        {
            m_log += "\n" + msg;
            SsitDebug.Info("Screenshot Success : " + msg);
        }

        /// <summary>
        /// 屏幕截图
        /// </summary>
        public void ScreenShot()
        {
            /*if (captureTools)
            {
                int width = Mathf.CeilToInt(Screen.width);
                int height = Mathf.FloorToInt(windowBlocker.rectTransform.sizeDelta.y);
                captureTools.CaptureAndSaveToAlbum(0, 0, width, height);
            }*/
        }

        #endregion

        #region IDebugLogHelper

        /// <summary>
        /// unity实现接口方法
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public void Log( DebugLogLevel level, object message )
        {
            switch (level)
            {
                case DebugLogLevel.Debug:
                    LogDebug(level, message);
                    break;
                case DebugLogLevel.Info:
                    LogDebug(level, TextUtils.Format("<color=cyan>{0}</color>", message));
                    break;
                case DebugLogLevel.Warning:
                    Debug.LogWarning(message);
                    break;
                case DebugLogLevel.Error:
                    Debug.LogError(message);
                    break;
                case DebugLogLevel.Fatal:
                    Debug.LogError(message);
                    break;
            }
        }

        /// <summary>
        /// 调试日志接口
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <summary>仅在编辑条件DEBUGLOG具备的条件下，调试日志才会编译</summary>
        [Conditional("DEBUGLOG")]
        public void LogDebug( DebugLogLevel level, object message )
        {
            Debug.Log(message);
        }

        #endregion

        #region DebugSize

        public float Fps { get; private set; }

        public float MonoMemorySize { get; private set; }

        public float MonoMemoryMaxSize { get; private set; }

        public float MemorySize { get; private set; }

        public float MemoryMaxSize { get; private set; }

        public GUISkin MGuiBackSkin => mGuiBackSkin;

        public GUISkin MGuiSkin => mGuiSkin;

        private void InitDebugInfo()
        {
            var profileListenner = GetComponent<ProfileBlock>();
            profileListenner.AddFpsChangeListener(value => { Fps = value; });
            profileListenner.AddMonoSizeChangeListener(( size, maxSize ) =>
            {
                MonoMemorySize = size;
                MonoMemoryMaxSize = maxSize;
            });
            profileListenner.AddMemoryChangeListener(( size, maxSize ) =>
            {
                MemorySize = size;
                MemoryMaxSize = maxSize;
            });
        }

        #endregion
    }
}