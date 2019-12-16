using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

public class ProfileBlock : MonoBehaviour
{
    private float m_lastUpdateShowTime = 0f;    //上一次更新帧率的时间;
    private readonly float m_UpdateShowDeltaTime = 1f;//更新帧率的时间间隔;
    private int m_frameUpdate = 0;//帧数;
    private UnityAction<float> OnFpsChangeAction;

    private UnityAction<float, float> OnMonoSizeChangeAction;
    private UnityAction<float, float> OnMemoryChangeAction;

    //支持Monon内存写出
    private bool _isSupportedMono;
    // Use this for initialization
    void Start()
    {
        m_lastUpdateShowTime = Time.realtimeSinceStartup;
    }

    private void OnEnable()
    {
#if UNITY_5_6_OR_NEWER
        _isSupportedMono = Profiler.GetMonoUsedSizeLong() > 0;
#else
        _isSupportedMono = Profiler.GetMonoUsedSize() > 0;
#endif
    }

    private void OnDestroy()
    {
        OnFpsChangeAction = null;
        OnMonoSizeChangeAction = null;
        OnMemoryChangeAction = null;
    }

    // Update is called once per frame
    void Update()
    {
        //if (JxDebug.JxDebug.Singleton.IsSettingsOpen)
        {
            m_frameUpdate++;
            if (Time.realtimeSinceStartup - m_lastUpdateShowTime >= m_UpdateShowDeltaTime)
            {
                RefreshMemorySize();
                RefreshMonoSize();
                RefreshFrameCount();
                m_lastUpdateShowTime = Time.realtimeSinceStartup;
            }
        }

    }

    //void OnGUI()
    //{

    //    GUIStyle style = new GUIStyle();
    //    style.normal.textColor = new Color(1, 1, 1);
    //    style.fontSize = 40;
    //    GUI.skin.label.alignment = TextAnchor.UpperCenter;
    //    GUI.Label(new Rect(0, 0, 200, 60), "FPS:" + System.String.Format("{0:F2}", Fps), style);

    //}

    public void AddFpsChangeListener( UnityAction<float> func )
    {
        OnFpsChangeAction = func;
    }

    public void AddMonoSizeChangeListener( UnityAction<float, float> func )
    {
        OnMonoSizeChangeAction = func;
    }

    public void AddMemoryChangeListener( UnityAction<float, float> func )
    {
        OnMemoryChangeAction = func;
    }

    void RefreshFrameCount()
    {
        OnFpsChange(m_frameUpdate / m_UpdateShowDeltaTime);

        m_frameUpdate = 0;
    }
    void RefreshMemorySize()
    {
        long max;
        long current;

#if UNITY_5_6_OR_NEWER
        max = Profiler.GetTotalReservedMemoryLong();
        current = Profiler.GetTotalAllocatedMemoryLong();
#else
        max = Profiler.GetTotalReservedMemory();
        current = Profiler.GetTotalAllocatedMemory();
#endif

        var maxMb = (max >> 10);
        maxMb /= 1024; // On new line to fix il2cpp

        var currentMb = (current >> 10);
        currentMb /= 1024;

        OnMemoryChange(currentMb, maxMb);

    }

    void RefreshMonoSize()
    {
        long max;
        long current;

#if UNITY_5_6_OR_NEWER
        max = _isSupportedMono ? Profiler.GetMonoHeapSizeLong() : GC.GetTotalMemory(false);
        current = Profiler.GetMonoUsedSizeLong();
#else
        max = _isSupportedMono ? Profiler.GetMonoHeapSize() : GC.GetTotalMemory(false);
        current = Profiler.GetMonoUsedSize();
#endif

        var maxMb = (max >> 10);
        maxMb /= 1024; // On new line to workaround IL2CPP bug

        var currentMb = (current >> 10);
        currentMb /= 1024;

        OnMonoSizeChange(currentMb, maxMb);
        //TotalAllocatedText.text = "Total: <color=#FFFFFF>{0}</color>MB".Fmt(maxMb);

        //if (currentMb > 0)
        //{
        //CurrentUsedText.text = "<color=#FFFFFF>{0}</color>MB".Fmt(currentMb);
        //}
    }

    void OnFpsChange( float fps )
    {
        if (OnFpsChangeAction != null)
        {
            OnFpsChangeAction(fps);
        }
    }

    void OnMonoSizeChange( float currentMb, float maxMb )
    {
        if (OnMonoSizeChangeAction != null)
        {
            OnMonoSizeChangeAction(currentMb, maxMb);
        }
    }

    void OnMemoryChange( float currentMb, float maxMb )
    {
        if (OnMemoryChangeAction != null)
        {
            OnMemoryChangeAction(currentMb, maxMb);
        }
    }


}
