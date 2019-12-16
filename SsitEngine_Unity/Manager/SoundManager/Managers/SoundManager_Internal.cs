/*
*┌──────────────────────────────────────────────────────────────┐
*│　描   述：底层音效管理器                                                    
*│　作   者：xuXin                                              
*│　版   本：1.0.0                                                 
*│　创建时间：2019年4月24日                             
*└──────────────────────────────────────────────────────────────┘
*/

using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace SsitEngine.Unity.Sound
{
    /// <summary>
    ///     音效管理器
    /// </summary>
    public partial class SoundManager : ManagerBase<SoundManager>
    {
        #region Mono

        private void Awake()
        {
            Setup();
            /* 不要添加任何东西 */
        }

        #endregion

        /// <summary>
        ///     Stops all sound immediately.
        /// </summary>
        private void _StopMusicImmediately()
        {
            StopAllCoroutines();
            StartCoroutine("CrossoutAll", 0f);
        }

        /// <summary>
        ///     Crosses out all AudioSources.
        /// </summary>
        private void _StopMusic()
        {
            StopAllCoroutines();
            StartCoroutine("CrossoutAll", crossDuration);
        }

        /// <summary>
        ///     Pause's all sounds
        /// </summary>
        private void _Pause()
        {
            if (!isPaused)
            {
                isPaused = !isPaused;
                if (audios[0].isPlaying)
                    audiosPaused[0] = true;
                audios[0].Pause();
                if (audios[1].isPlaying)
                    audiosPaused[1] = true;
                audios[1].Pause();
                PSFX(true);
            }
        }

        /// <summary>
        ///     Unpause's all sounds
        /// </summary>
        private void _UnPause()
        {
            if (isPaused)
            {
                if (audiosPaused[0])
                    audios[0].Play();
                if (audiosPaused[1])
                    audios[1].Play();

                audiosPaused[0] = audiosPaused[1] = false;

                PSFX(false);
                isPaused = !isPaused;
            }
        }

        private AudioSource IsPlaying( AudioClip clip )
        {
            return IsPlaying(clip, true);
        }

        private AudioSource IsPlaying( AudioClip clip, bool regardlessOfCrossOut )
        {
            for (var i = 0; i < audios.Length; i++)
                if (audios[i].isPlaying && audios[i].clip == clip && (regardlessOfCrossOut || !outCrossing[i]))
                    return audios[i];
            return null;
        }

        /// <summary>
        ///     有无播放声音
        /// </summary>
        private bool IsPlaying()
        {
            for (var i = 0; i < audios.Length; i++)
                if (audios[i].isPlaying)
                    return true;
            return false;
        }

        private int GetAudioSourceIndex( AudioSource source )
        {
            for (var i = 0; i < audios.Length; i++)
                if (source == audios[i])
                    return i;
            return SOUNDMANAGER_FALSE;
        }

        private void StopAllNonSoundConnectionCoroutines()
        {
            StopCoroutine("Crossfade");
            StopCoroutine("Crossout");
            StopCoroutine("Crossin");
            StopCoroutine("CrossoutAll");
        }

        private int GetActualCurrentPlayingIndex()
        {
            if (audios[0].isPlaying && audios[1].isPlaying)
            {
                //Both are playing, so figure out whos going to be playing when its over.
                if (inCrossing[0] && inCrossing[1])
                    return SOUNDMANAGER_FALSE; //not possible!
                if (inCrossing[0])
                    return 0;
                if (inCrossing[1])
                    return 1;
                return SOUNDMANAGER_FALSE; // not possible!
            }
            for (var i = 0; i < audios.Length; i++)
                if (audios[i].isPlaying)
                    return i;
            return SOUNDMANAGER_FALSE;
        }

        private void SetNextSongInQueue()
        {
            if (currentSongIndex == -1)
                return;
            if (currentSoundConnection == null || currentSoundConnection.soundsToPlay == null ||
                currentSoundConnection.soundsToPlay.Count <= 0)
                return;
            var nextSongPlaying = (currentSongIndex + 1) % currentSoundConnection.soundsToPlay.Count;
            var nextSong = currentSoundConnection.soundsToPlay[nextSongPlaying];

            var notPlaying = CheckWhosNotPlaying();
            if (audios[notPlaying] == null ||
                audios[notPlaying].clip != null && !audios[notPlaying].clip.Equals(nextSong))
                audios[notPlaying].clip = nextSong;
        }

        #region 模块化接口实现

        public override string ModuleName => typeof(SoundManager).FullName;

        public override int Priority => 5;

        public override void OnUpdate( float elapseSeconds )
        {
            // Handle SFX on update
            HandleSFX();
        }

        public override void Shutdown()
        {
        }

        #endregion

        #region 初始化

        /// <summary>
        ///     初始化管理器(检测单列、清除现有音效源、初始化场景音乐播放组件、初始化场景特效播放池、初始化当前场景的音乐播放(淡入淡出))
        /// </summary>
        private void Setup()
        {
            if (Instance && Instance.gameObject != gameObject)
            {
                Destroy(gameObject);
            }
            else
            {
                ClearAudioSources();
                Init();
                SetupSoundFX();
                //#if UNITY_5_4_OR_NEWER
                //OnLevelLoaded(SceneManager.GetActiveScene().buildIndex);
                SceneManager.sceneLoaded += OnLevelLoaded;
                //#else
                //              OnLevelLoaded(Application.loadedLevel);
                //#endif
            }
        }

        /// <summary>
        ///     初始化场景音乐播放组件(仅调用一次)
        /// </summary>
        private void Init()
        {
            if (Instance)
            {
                audios = new AudioSource[2];
                audios[0] = gameObject.AddComponent<AudioSource>();
                audios[1] = gameObject.AddComponent<AudioSource>();

                audiosPaused = new bool[2];
                audiosPaused[0] = audiosPaused[1] = false;

                audios[0].hideFlags = HideFlags.HideInInspector;
                audios[1].hideFlags = HideFlags.HideInInspector;

                SoundManagerTools.make2D(ref audios[0]);
                SoundManagerTools.make2D(ref audios[1]);

                audios[0].volume = 0f;
                audios[1].volume = 0f;

                audios[0].ignoreListenerVolume = true;
                audios[1].ignoreListenerVolume = true;

                for (var i = 0; i < 2; i++)
                {
                    audios[i].loop = false;
                    audios[i].Stop();
                    audios[i].playOnAwake = false;
                }

                maxVolume = AudioListener.volume;

                currentPlaying = CheckWhosPlaying();
            }
        }

        /// <summary>
        ///     清除现有音效源
        /// </summary>
        private void ClearAudioSources()
        {
            var currentSources = gameObject.GetComponents<AudioSource>();
            foreach (var source in currentSources)
                Destroy(source);
        }

        #endregion

        #region Interval InterTools

        #region 音效配置寄存器播放逻辑

        /// <summary>
        ///     播放音乐配置文件寄存器
        ///     <param name="sc">音效配置文件</param>
        /// </summary>
        private void _PlayConnection( SoundConnection sc )
        {
            if (offTheBGM || isPaused) return;
            if (string.IsNullOrEmpty(sc.level))
            {
                var i = 1;
                while (SoundConnectionsContainsThisLevel("CustomConnection" + i) != SOUNDMANAGER_FALSE)
                    i++;
                sc.level = "CustomConnection" + i;
            }
            StopPreviousPlaySoundConnection();
            StartCoroutine("PlaySoundConnection", sc);
        }

        /// <summary>
        ///     播放音乐配置文件寄存器
        ///     <param name="levelName">匹配场景名称</param>
        /// </summary>
        private void _PlayConnection( string levelName )
        {
            if (offTheBGM || isPaused) return;
            var _indexOf = SoundConnectionsContainsThisLevel(levelName);
            if (_indexOf != SOUNDMANAGER_FALSE)
            {
                StopPreviousPlaySoundConnection();
                StartCoroutine("PlaySoundConnection", soundConnections[_indexOf]);
            }
            else
            {
                Debug.LogError("There are no SoundConnections with the name: " + levelName);
            }
        }

        /// <summary>
        ///     停止上一个配置源的播放
        /// </summary>
        private void StopPreviousPlaySoundConnection()
        {
            StopCoroutine("PlaySoundConnection");
        }

        #endregion

        #region BGM 的播放逻辑

        /// <summary>
        ///     场景加载回调(自动播放场景配置源)
        /// </summary>
        private void HandleLevel( int level )
        {
            if (gameObject != gameObject || isPaused) return;

            if (Time.realtimeSinceStartup != 0f && lastLevelLoad == Time.realtimeSinceStartup)
                return;
            lastLevelLoad = Time.realtimeSinceStartup;

            if (showDebug) Debug.Log("(" + Time.time + ") In Level Loaded: " + SceneManager.GetActiveScene().name);
            var _indexOf = SoundConnectionsContainsThisLevel(SceneManager.GetActiveScene().name);
            if (_indexOf == SOUNDMANAGER_FALSE || soundConnections[_indexOf].isCustomLevel)
            {
                silentLevel = true;
            }
            else
            {
                silentLevel = false;
                currentLevel = SceneManager.GetActiveScene().name;
                currentSoundConnection = soundConnections[_indexOf];
            }

            if (!silentLevel && !offTheBGM)
            {
                if (showDebug) Debug.Log("BGM activated.");
                StopPreviousPlaySoundConnection();
                StartCoroutine("PlaySoundConnection", currentSoundConnection);
            }
            else
            {
                if (showDebug) Debug.Log("BGM deactivated.");
                currentSoundConnection = null;
                audios[0].loop = false;
                audios[1].loop = false;
                if (showDebug) Debug.Log("Don't play anything in this scene, cross out.");
                currentPlaying = CheckWhosPlaying();
                StopAllCoroutines();
                if (currentPlaying == SOUNDMANAGER_FALSE)
                {
                    if (showDebug) Debug.Log("Nothing is playing, don't do anything.");
                    return;
                }
                if (CheckWhosNotPlaying() == SOUNDMANAGER_FALSE)
                {
                    if (showDebug)
                        Debug.Log("Both sources are playing, probably in a crossfade. Crossfade them both out.");
                    StartCoroutine("CrossoutAll", crossDuration);
                }
                else if (audios[currentPlaying].isPlaying)
                {
                    if (showDebug) Debug.Log("Crossing out the source that is playing.");
                    StartCoroutine("Crossout", new object[] {audios[currentPlaying], crossDuration});
                }
            }

            //场景音效处理
            StopSFX();
        }

        /// <summary>
        ///     检测哪个没有播放
        ///     </returns>
        private int CheckWhosNotPlaying()
        {
            if (!audios[0].isPlaying) return 0;
            if (!audios[1].isPlaying) return 1;
            return SOUNDMANAGER_FALSE;
        }

        /// <summary>
        ///     检测哪个通道在播放
        ///     </returns>
        private int CheckWhosPlaying()
        {
            if (audios[0].isPlaying) return 0;
            if (audios[1].isPlaying) return 1;
            return SOUNDMANAGER_FALSE;
        }

        /// <summary>
        ///     播放音乐（无延迟，播放完成后不会回复声音源、需要的自己作回调处理）
        /// </summary>
        private void _PlayImmediately( AudioClip clip2play, bool loop, UnityEvent runOnEndFunction )
        {
            if (InternalCallback != null)
                OnSongEnd = InternalCallback;
            StopMusicImmediately();
            InternalCallback = runOnEndFunction;

            if (offTheBGM || isPaused) return;
            SoundConnection sc;
            if (loop)
                sc = new SoundConnection("", PlayMethod.ContinuousPlayThrough, clip2play);
            else
                sc = new SoundConnection("", PlayMethod.OncePlayThrough, clip2play);
            ignoreCrossDuration = true;
            PlayConnection(sc);
            //this._PlayConnection(currentSoundConnection);
        }

        private void _PlayImmediately( AudioClip clip2play, bool loop )
        {
            _PlayImmediately(clip2play, loop, null);
        }

        private void _PlayImmediately( AudioClip clip2play )
        {
            _PlayImmediately(clip2play, false);
        }

        /// <summary>
        ///     等待当前声音源谈出后播放音乐
        /// </summary>
        private void _Play( AudioClip clip2play, bool loop, UnityEvent runOnEndFunction )
        {
            if (offTheBGM || isPaused) return;
            if (InternalCallback != null)
                OnSongEnd = InternalCallback;
            InternalCallback = runOnEndFunction;

            SoundConnection sc;
            if (loop)
                sc = new SoundConnection(SceneManager.GetActiveScene().name, PlayMethod.ContinuousPlayThrough,
                    clip2play);
            else
                sc = new SoundConnection(SceneManager.GetActiveScene().name, PlayMethod.OncePlayThrough, clip2play);
            PlayConnection(sc);
        }

        private void _Play( AudioClip clip2play, bool loop )
        {
            _Play(clip2play, loop, null);
        }

        private void _Play( AudioClip clip2play )
        {
            _Play(clip2play, false);
        }

        /// <summary>
        ///     Plays the clip.  This is a private method because it must be used appropriately with a SoundConnection.
        ///     It's logic is dependent on it being used correctly.
        /// </summary>
        private int PlayClip( AudioClip clip2play, float clipVolume = 1f )
        {
            if (showDebug) Debug.Log("Playing: " + clip2play.name);
            currentPlaying = CheckWhosPlaying();
            var notPlaying = CheckWhosNotPlaying();
            if (currentPlaying != SOUNDMANAGER_FALSE) //If an AudioSource is playing...
            {
                if (notPlaying != SOUNDMANAGER_FALSE) //If one AudioSources is playing...
                {
                    if (audios[currentPlaying].clip.Equals(clip2play) && audios[currentPlaying].isPlaying)
                        //If the current playing source is playing the clip...
                    {
                        if (showDebug)
                            Debug.Log("Already playing BGM, check if crossing out(" + outCrossing[currentPlaying] +
                                      ") or in(" + inCrossing[currentPlaying] + ").");
                        if (outCrossing[currentPlaying]) //If that source is crossing out, stop it and cross it back in.
                        {
                            StopAllNonSoundConnectionCoroutines();
                            if (showDebug)
                                Debug.Log("In the process of crossing out, so that is being changed to cross in now.");
                            outCrossing[currentPlaying] = false;
                            StartCoroutine("Crossin", new object[] {audios[currentPlaying], crossDuration, clipVolume});
                            return currentPlaying;
                        }
                        if (movingOnFromSong)
                        {
                            if (showDebug)
                                Debug.Log("Current song is actually done, so crossfading to another instance of it.");
                            if (audios[notPlaying] == null || audios[notPlaying].clip == null ||
                                !audios[notPlaying].clip.Equals(clip2play))
                                audios[notPlaying].clip = clip2play;
                            StartCoroutine("Crossfade",
                                new object[] {audios[currentPlaying], audios[notPlaying], crossDuration, clipVolume});
                            return notPlaying;
                        }
                        return currentPlaying;
                    }
                    StopAllNonSoundConnectionCoroutines();
                    if (showDebug) Debug.Log("Playing another track, crossfading to that.");
                    audios[notPlaying].clip = clip2play;
                    StartCoroutine("Crossfade",
                        new object[] {audios[currentPlaying], audios[notPlaying], crossDuration, clipVolume});
                    return notPlaying;
                }
                var lastPlaying = GetActualCurrentPlayingIndex();
                if (showDebug) Debug.Log("Both are playing (crossfade situation).");
                if (clip2play.Equals(audios[0].clip) && clip2play.Equals(audios[1].clip))
                {
                    if (showDebug)
                        Debug.Log("If clip == clip in audio1 AND audio2, then do nothing and let it finish.");
                    var swapIn = lastPlaying == 0 ? 0 : 1;

                    if (!audios[0].isPlaying) audios[0].Play();
                    if (!audios[1].isPlaying) audios[1].Play();

                    return swapIn;
                }
                if (clip2play.Equals(audios[0].clip)) //If the clip is the same clip playing in source1...
                {
                    var switcheroo = false;
                    if (outCrossing[0] &&
                        (audios[0].clip.samples - audios[0].timeSamples) * 1f / (audios[0].clip.frequency * 1f) <=
                        crossDuration) // If the clip is crossing out though, cross in the new track started over.
                    {
                        if (showDebug)
                            Debug.Log(
                                "Clip == clip in audio1, but it's crossing out so cross it in from the beginning.");
                        audios[1].clip = clip2play;
                        audios[1].timeSamples = 0;
                        switcheroo = true;
                    }
                    else if (showDebug)
                    {
                        Debug.Log("If clip == clip in audio1, then just switch them.");
                    }

                    var swapIn = lastPlaying == 0 ? 0 : 1;
                    var swapOut = swapIn == 0 ? 1 : 0;
                    StopAllNonSoundConnectionCoroutines();
                    if (switcheroo) // Perform the switch if needed
                    {
                        var tempSwap = swapIn;
                        swapIn = swapOut;
                        swapOut = tempSwap;
                    }
                    if (swapIn != 0 || switcheroo && swapIn != 1)
                        //If the source crossing out is not the clip OR it is but is crossing out, just continue with crossfade
                        StartCoroutine("Crossfade",
                            new object[] {audios[swapIn], audios[swapOut], crossDuration, clipVolume});
                    else
                        //If the source crossing out is the clip, swap them so that it's now crossing in
                        StartCoroutine("Crossfade",
                            new object[] {audios[swapOut], audios[swapIn], crossDuration, clipVolume});
                    if (!audios[0].isPlaying) audios[0].Play();
                    if (!audios[1].isPlaying) audios[1].Play();
                    if (swapIn != 0)
                        return swapOut;
                    return swapIn;
                }
                if (clip2play.Equals(audios[1].clip)) //If the clip is the same clip playing in source2...
                {
                    var switcheroo = false;
                    if (outCrossing[1] &&
                        (audios[1].clip.samples - audios[1].timeSamples) * 1f / (audios[1].clip.frequency * 1f) <=
                        crossDuration) // If the clip is crossing out though, cross in the new track started over.
                    {
                        if (showDebug)
                            Debug.Log(
                                "Clip == clip in audio2, but it's crossing out so cross it in from the beginning.");
                        audios[0].clip = clip2play;
                        audios[0].timeSamples = 0;
                        switcheroo = true;
                    }
                    else if (showDebug)
                    {
                        Debug.Log("If clip == clip in audio2, then just switch them.");
                    }

                    var swapIn = lastPlaying == 0 ? 0 : 1;
                    var swapOut = swapIn == 0 ? 1 : 0;
                    StopAllNonSoundConnectionCoroutines();
                    if (switcheroo) // Perform the switch if needed
                    {
                        var tempSwap = swapIn;
                        swapIn = swapOut;
                        swapOut = tempSwap;
                    }
                    if (swapIn != 1 || switcheroo && swapIn != 0)
                        //If the source crossing out is not the clip, just continue with crossfade
                        StartCoroutine("Crossfade",
                            new object[] {audios[swapIn], audios[swapOut], crossDuration, clipVolume});
                    else
                        //If the source crossing out is the clip, swap them so that it's now crossing in
                        StartCoroutine("Crossfade",
                            new object[] {audios[swapOut], audios[swapIn], crossDuration, clipVolume});
                    ;
                    if (!audios[0].isPlaying) audios[0].Play();
                    if (!audios[1].isPlaying) audios[1].Play();
                    if (swapIn != 1)
                        return swapOut;
                    return swapIn;
                }
                // If the clip is not in either source1 or source2...
                StopAllNonSoundConnectionCoroutines();
                if (showDebug)
                    Debug.Log("If clip is in neither, find the louder one and crossfade from that one.");
                if (audios[0].volume > audios[1].volume)
                    //If source1 is louder than source2, then crossfade from source1.
                {
                    audios[1].clip = clip2play;
                    StartCoroutine("Crossfade", new object[] {audios[0], audios[1], crossDuration, clipVolume});
                    return 1;
                }
                audios[0].clip = clip2play;
                StartCoroutine("Crossfade", new object[] {audios[1], audios[0], crossDuration, clipVolume});
                return 0;
            }
            if (audiosPaused[0] && audiosPaused[1]) // paused and playing two tracks (crossfading)
            {
                if (showDebug)
                    Debug.Log(
                        "All sound is paused and it's crossfading between two songs. Replace the lower volume song and prepare for crossfade on unpause.");
                var lesserAudio = audios[0].volume > audios[1].volume ? 1 : 0;
                var greaterAudio = lesserAudio == 0 ? 1 : 0;
                audios[lesserAudio].clip = clip2play;
                StartCoroutine("Crossfade",
                    new object[] {audios[greaterAudio], audios[lesserAudio], crossDuration, clipVolume});
            }
            else if (audiosPaused[0]) // track 1 is paused
            {
                if (showDebug)
                    Debug.Log("All sound is paused and track1 is playing. Prepare for crossfade on unpause.");
                audios[1].clip = clip2play;
                audiosPaused[1] = true;
                StartCoroutine("Crossfade", new object[] {audios[0], audios[1], crossDuration, clipVolume});
            }
            else if (audiosPaused[1]) // track 2 is paused
            {
                if (showDebug)
                    Debug.Log("All sound is paused and track2 is playing. Prepare for crossfade on unpause.");
                audios[0].clip = clip2play;
                audiosPaused[0] = true;
                StartCoroutine("Crossfade", new object[] {audios[1], audios[0], crossDuration, clipVolume});
            }
            else // silent scene
            {
                if (showDebug) Debug.Log("Wasn't playing anything, crossing in.");
                audios[notPlaying].clip = clip2play;
                StartCoroutine("Crossin", new object[] {audios[notPlaying], crossDuration, clipVolume});
            }
            return SOUNDMANAGER_FALSE;
        }

        #endregion

        #endregion

        #region 播放协程

        /// <summary>
        ///     Crossfade from a1 to a2, for a duration.
        /// </summary>
        private IEnumerator Crossfade( object[] param )
        {
            if (OnCrossInBegin != null)
                OnCrossInBegin.Invoke();
            OnCrossInBegin = null;

            if (OnCrossOutBegin != null)
                OnCrossOutBegin.Invoke();
            OnCrossOutBegin = null;

            var a1 = param[0] as AudioSource;
            var a2 = param[1] as AudioSource;

            var index1 = GetAudioSourceIndex(a1);
            var index2 = GetAudioSourceIndex(a2);

            var duration = (float) param[2];
            if (ignoreCrossDuration)
            {
                ignoreCrossDuration = false;
                duration = 0f;
            }
            var realSongLength = 0f;
            if (a2.clip != null)
                realSongLength = (a2.clip.samples - a2.timeSamples) * 1f / (a2.clip.frequency * 1f);
            if (duration - realSongLength / 2f > .1f)
            {
                duration = Mathf.Floor(realSongLength / 2f * 100f) / 100f;
                Debug.LogWarning("Had to reduce the cross duration to " + duration +
                                 " for this transition as the cross duration is longer than half the track length.");
            }
            modifiedCrossDuration = duration;

            var clipVolume = (float) param[3];

            if (OnSongBegin != null)
                OnSongBegin.Invoke();
            OnSongBegin = null;

            if (index1 == SOUNDMANAGER_FALSE || index2 == SOUNDMANAGER_FALSE)
                Debug.LogWarning(
                    "You passed an AudioSource that is not used by the SoundManager May cause erratic behavior");
            outCrossing[index1] = true;
            inCrossing[index2] = true;
            outCrossing[index2] = false;
            inCrossing[index1] = false;

            var startTime = Time.realtimeSinceStartup;
            var endTime = startTime + duration;
            if (!a2.isPlaying) a2.Play();
            float a1StartVolume = a1.volume,
                a2StartVolume = a2.volume,
                deltaPercent = 0f,
                a1DeltaVolume = 0f,
                a2DeltaVolume = 0f,
                startMaxMusicVolume = maxMusicVolume,
                volumePercent = 1f;
            bool passedFirstPause = false, passedFirstUnpause = true;
            var pauseTimeRemaining = 0f;
            while (isPaused || passedFirstPause || Time.realtimeSinceStartup < endTime)
                if (isPaused)
                {
                    if (!passedFirstPause)
                    {
                        pauseTimeRemaining = endTime - Time.realtimeSinceStartup;
                        passedFirstPause = true;
                        passedFirstUnpause = false;
                    }
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    if (!passedFirstUnpause)
                    {
                        var oldEndTime = endTime;
                        endTime = Time.realtimeSinceStartup + pauseTimeRemaining;
                        startTime += endTime - oldEndTime;
                        passedFirstPause = false;
                        passedFirstUnpause = true;
                    }
                    if (startMaxMusicVolume == 0f)
                        volumePercent = 1f;
                    else
                        volumePercent = maxMusicVolume / startMaxMusicVolume;

                    if (endTime - Time.realtimeSinceStartup > duration)
                    {
                        startTime = Time.realtimeSinceStartup;
                        endTime = startTime + duration;
                    }
                    deltaPercent = (Time.realtimeSinceStartup - startTime) / duration;
                    a1DeltaVolume = deltaPercent * a1StartVolume;
                    a2DeltaVolume = deltaPercent * (startMaxMusicVolume - a2StartVolume);

                    a1.volume = Mathf.Clamp01((a1StartVolume - a1DeltaVolume) * volumePercent);
                    a2.volume = Mathf.Clamp01((a2DeltaVolume + a2StartVolume) * volumePercent * clipVolume);
                    yield return null;
                }
            a1.volume = 0f;
            a2.volume = maxMusicVolume * clipVolume;
            a1.Stop();
            a1.timeSamples = 0;
            modifiedCrossDuration = crossDuration;
            currentPlaying = CheckWhosPlaying();

            outCrossing[index1] = false;
            inCrossing[index2] = false;

            if (OnSongEnd != null)
            {
                OnSongEnd.Invoke();
                if (InternalCallback != null)
                    OnSongEnd = InternalCallback;
                else
                    OnSongEnd = null;
                InternalCallback = null;
            }

            if (InternalCallback != null)
                OnSongEnd = InternalCallback;
            InternalCallback = null;

            if (OnSongBegin != null)
                OnSongBegin.Invoke();
            OnSongBegin = null;

            SetNextSongInQueue();
        }

        /// <summary>
        ///     Crossout from a1 for duration.
        /// </summary>
        private IEnumerator Crossout( object[] param )
        {
            if (OnCrossOutBegin != null)
                OnCrossOutBegin.Invoke();
            OnCrossOutBegin = null;

            var a1 = param[0] as AudioSource;
            var duration = (float) param[1];
            if (ignoreCrossDuration)
            {
                ignoreCrossDuration = false;
                duration = 0f;
            }
            var realSongLength = 0f;
            if (a1.clip != null)
                realSongLength = (a1.clip.samples - a1.timeSamples) * 1f / (a1.clip.frequency * 1f);
            if (duration - realSongLength / 2f > .1f)
            {
                duration = Mathf.Floor(realSongLength / 2f * 100f) / 100f;
                Debug.LogWarning("Had to reduce the cross duration to " + duration +
                                 " for this transition as the cross duration is longer than half the track length.");
            }
            modifiedCrossDuration = duration;

            var index1 = GetAudioSourceIndex(a1);

            if (index1 == SOUNDMANAGER_FALSE)
                Debug.LogWarning(
                    "You passed an AudioSource that is not used by the SoundManager May cause erratic behavior");
            outCrossing[index1] = true;
            inCrossing[index1] = false;

            var startTime = Time.realtimeSinceStartup;
            var endTime = startTime + duration;
            float maxVolume = a1.volume, deltaVolume = 0f, startMaxMusicVolume = maxMusicVolume, volumePercent = 1f;
            bool passedFirstPause = false, passedFirstUnpause = true;
            var pauseTimeRemaining = 0f;
            while (isPaused || passedFirstPause || Time.realtimeSinceStartup < endTime)
                if (isPaused)
                {
                    if (!passedFirstPause)
                    {
                        pauseTimeRemaining = endTime - Time.realtimeSinceStartup;
                        passedFirstPause = true;
                        passedFirstUnpause = false;
                    }
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    if (!passedFirstUnpause)
                    {
                        var oldEndTime = endTime;
                        endTime = Time.realtimeSinceStartup + pauseTimeRemaining;
                        startTime += endTime - oldEndTime;
                        passedFirstPause = false;
                        passedFirstUnpause = true;
                    }
                    if (startMaxMusicVolume == 0f)
                        volumePercent = 1f;
                    else
                        volumePercent = maxMusicVolume / startMaxMusicVolume;

                    if (endTime - Time.realtimeSinceStartup > duration)
                    {
                        startTime = Time.realtimeSinceStartup;
                        endTime = startTime + duration;
                    }
                    deltaVolume = (Time.realtimeSinceStartup - startTime) / duration * maxVolume;

                    a1.volume = Mathf.Clamp01((maxVolume - deltaVolume) * volumePercent);
                    yield return null;
                }
            a1.volume = 0f;
            a1.Stop();
            a1.timeSamples = 0;
            modifiedCrossDuration = crossDuration;
            currentPlaying = CheckWhosPlaying();

            outCrossing[index1] = true;

            if (OnSongEnd != null)
                OnSongEnd.Invoke();
            OnSongEnd = null;

            if (InternalCallback != null)
                InternalCallback.Invoke();
            InternalCallback = null;
        }

        /// <summary>
        ///     Crossout from all AudioSources for a duration.
        /// </summary>
        private IEnumerator CrossoutAll( float duration )
        {
            if (CheckWhosPlaying() == SOUNDMANAGER_FALSE) yield break;

            if (OnCrossOutBegin != null)
                OnCrossOutBegin.Invoke();
            OnCrossOutBegin = null;

            outCrossing[0] = true;
            outCrossing[1] = true;
            inCrossing[0] = false;
            inCrossing[1] = false;

            var realSongLength = 0f;
            if (audios[0].clip != null && audios[1].clip != null)
                realSongLength =
                    Mathf.Max((audios[0].clip.samples - audios[0].timeSamples) * 1f / (audios[0].clip.frequency * 1f),
                        (audios[1].clip.samples - audios[1].timeSamples) * 1f / (audios[1].clip.frequency * 1f));
            else if (audios[0].clip != null)
                realSongLength = (audios[0].clip.samples - audios[0].timeSamples) * 1f /
                                 (audios[0].clip.frequency * 1f);
            else if (audios[1].clip != null)
                realSongLength = (audios[1].clip.samples - audios[1].timeSamples) * 1f /
                                 (audios[1].clip.frequency * 1f);

            if (ignoreCrossDuration)
            {
                ignoreCrossDuration = false;
                duration = 0f;
            }
            if (duration - realSongLength / 2f > .1f)
            {
                duration = Mathf.Floor(realSongLength / 2f * 100f) / 100f;
                Debug.LogWarning("Had to reduce the cross duration to " + duration +
                                 " for this transition as the cross duration is longer than half the track length.");
            }
            modifiedCrossDuration = duration;

            var startTime = Time.realtimeSinceStartup;
            var endTime = Time.realtimeSinceStartup + duration;
            float a1MaxVolume = volume1, a2MaxVolume = volume2;
            float deltaPercent = 0f,
                a1DeltaVolume = 0f,
                a2DeltaVolume = 0f,
                startMaxMusicVolume = maxMusicVolume,
                volumePercent = 1f;
            bool passedFirstPause = false, passedFirstUnpause = true;
            var pauseTimeRemaining = 0f;
            while (isPaused || passedFirstPause || Time.realtimeSinceStartup < endTime)
                if (isPaused)
                {
                    if (!passedFirstPause)
                    {
                        pauseTimeRemaining = endTime - Time.realtimeSinceStartup;
                        passedFirstPause = true;
                        passedFirstUnpause = false;
                    }
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    if (!passedFirstUnpause)
                    {
                        var oldEndTime = endTime;
                        endTime = Time.realtimeSinceStartup + pauseTimeRemaining;
                        startTime += endTime - oldEndTime;
                        passedFirstPause = false;
                        passedFirstUnpause = true;
                    }
                    if (startMaxMusicVolume == 0f)
                        volumePercent = 1f;
                    else
                        volumePercent = maxMusicVolume / startMaxMusicVolume;

                    if (endTime - Time.realtimeSinceStartup > duration)
                    {
                        startTime = Time.realtimeSinceStartup;
                        endTime = startTime + duration;
                    }
                    deltaPercent = (Time.realtimeSinceStartup - startTime) / duration;
                    a1DeltaVolume = deltaPercent * a1MaxVolume;
                    a2DeltaVolume = deltaPercent * a2MaxVolume;

                    volume1 = Mathf.Clamp01((a1MaxVolume - a1DeltaVolume) * volumePercent);
                    volume2 = Mathf.Clamp01((a2MaxVolume - a2DeltaVolume) * volumePercent);
                    yield return null;
                }
            volume1 = volume2 = 0f;
            audios[0].Stop();
            audios[1].Stop();
            audios[0].timeSamples = 0;
            audios[1].timeSamples = 0;
            modifiedCrossDuration = crossDuration;
            currentPlaying = CheckWhosPlaying();

            outCrossing[0] = false;
            outCrossing[1] = false;

            if (OnSongEnd != null)
                OnSongEnd.Invoke();
            OnSongEnd = null;

            if (InternalCallback != null)
                InternalCallback.Invoke();
            InternalCallback = null;
        }

        /// <summary>
        ///     Crossin from a1 for duration.
        /// </summary>
        private IEnumerator Crossin( object[] param )
        {
            if (OnCrossInBegin != null)
                OnCrossInBegin.Invoke();
            OnCrossInBegin = null;

            var a1 = param[0] as AudioSource;

            var duration = (float) param[1];
            if (ignoreCrossDuration)
            {
                ignoreCrossDuration = false;
                duration = 0f;
            }
            var realSongLength = 0f;
            if (a1.clip != null)
                realSongLength = a1.clip.samples * 1f / (a1.clip.frequency * 1f);
            if (duration - realSongLength / 2f > .1f)
            {
                duration = Mathf.Floor(realSongLength / 2f * 100f) / 100f;
                Debug.LogWarning("Had to reduce the cross duration to " + duration +
                                 " for this transition as the cross duration is longer than half the track length.");
            }
            modifiedCrossDuration = duration;

            var clipVolume = (float) param[2];

            var index1 = GetAudioSourceIndex(a1);

            if (index1 == SOUNDMANAGER_FALSE)
                Debug.LogWarning(
                    "You passed an AudioSource that is not used by the SoundManager May cause erratic behavior");
            inCrossing[index1] = true;
            outCrossing[index1] = false;

            var startTime = Time.realtimeSinceStartup;
            var endTime = startTime + duration;
            float a1StartVolume = a1.volume, startMaxMusicVolume = maxMusicVolume, volumePercent = 1f;
            if (!a1.isPlaying)
            {
                a1StartVolume = 0f;
                a1.Play();
            }
            var deltaVolume = 0f;
            bool passedFirstPause = false, passedFirstUnpause = true;
            var pauseTimeRemaining = 0f;
            while (isPaused || passedFirstPause || Time.realtimeSinceStartup < endTime)
                if (isPaused)
                {
                    if (!passedFirstPause)
                    {
                        pauseTimeRemaining = endTime - Time.realtimeSinceStartup;
                        passedFirstPause = true;
                        passedFirstUnpause = false;
                    }
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    if (!passedFirstUnpause)
                    {
                        var oldEndTime = endTime;
                        endTime = Time.realtimeSinceStartup + pauseTimeRemaining;
                        startTime += endTime - oldEndTime;
                        passedFirstPause = false;
                        passedFirstUnpause = true;
                    }
                    if (startMaxMusicVolume == 0f)
                        volumePercent = 1f;
                    else
                        volumePercent = maxMusicVolume / startMaxMusicVolume;

                    if (endTime - Time.realtimeSinceStartup > duration)
                    {
                        startTime = Time.realtimeSinceStartup;
                        endTime = startTime + duration;
                    }
                    deltaVolume = (Time.realtimeSinceStartup - startTime) / duration *
                                  (startMaxMusicVolume - a1StartVolume);

                    a1.volume = Mathf.Clamp01((deltaVolume + a1StartVolume) * volumePercent * clipVolume);
                    yield return null;
                }
            a1.volume = maxMusicVolume * clipVolume;
            modifiedCrossDuration = crossDuration;
            currentPlaying = CheckWhosPlaying();

            inCrossing[index1] = false;

            if (OnSongBegin != null)
                OnSongBegin.Invoke();
            OnSongBegin = null;

            if (InternalCallback != null)
                OnSongEnd = InternalCallback;
            InternalCallback = null;

            SetNextSongInQueue();
        }

        private IEnumerator PlaySoundConnection( SoundConnection sc )
        {
            if (isPaused) yield break;
            currentSoundConnection = sc;
            if (sc.soundsToPlay.Count == 0)
            {
                Debug.LogWarning("The SoundConnection for this level has no sounds to play.  Will cross out.");
                StartCoroutine("CrossoutAll", crossDuration);
                yield break;
            }
            var songPlaying = 0;
            if (skipSongs)
            {
                songPlaying = (songPlaying + skipAmount) % sc.soundsToPlay.Count;
                if (songPlaying < 0) songPlaying += sc.soundsToPlay.Count;
                skipSongs = false;
                skipAmount = 0;
            }


            switch (sc.playMethod)
            {
                case PlayMethod.ContinuousPlayThrough:
                    while (Application.isPlaying)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the time left is less than the cross duration
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus ||
                                   (currentSource.isPlaying || isPaused || currentSource.mute) &&
                                   (currentSource.clip.samples - currentSource.timeSamples) * 1f /
                                   (currentSource.clip.frequency * 1f) > modifiedCrossDuration)
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        if (!skipSongs)
                        {
                            songPlaying = (songPlaying + 1) % sc.soundsToPlay.Count;
                        }
                        else
                        {
                            songPlaying = (songPlaying + skipAmount) % sc.soundsToPlay.Count;
                            if (songPlaying < 0) songPlaying += sc.soundsToPlay.Count;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                    }
                    break;
                case PlayMethod.ContinuousPlayThroughWithDelay:
                    while (Application.isPlaying)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the song is done before moving on with the delay
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus || currentSource.isPlaying || isPaused || currentSource.mute)
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        yield return new WaitForSeconds(sc.delay);
                        if (!skipSongs)
                        {
                            songPlaying = (songPlaying + 1) % sc.soundsToPlay.Count;
                        }
                        else
                        {
                            songPlaying = (songPlaying + skipAmount) % sc.soundsToPlay.Count;
                            if (songPlaying < 0) songPlaying += sc.soundsToPlay.Count;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                    }
                    break;
                case PlayMethod.ContinuousPlayThroughWithRandomDelayInRange:
                    while (Application.isPlaying)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the song is done before moving on with the delay
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus || currentSource.isPlaying || isPaused || currentSource.mute)
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        var randomDelay = Random.Range(sc.minDelay, sc.maxDelay);
                        yield return new WaitForSeconds(randomDelay);
                        if (!skipSongs)
                        {
                            songPlaying = (songPlaying + 1) % sc.soundsToPlay.Count;
                        }
                        else
                        {
                            songPlaying = (songPlaying + skipAmount) % sc.soundsToPlay.Count;
                            if (songPlaying < 0) songPlaying += sc.soundsToPlay.Count;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                    }
                    break;
                case PlayMethod.OncePlayThrough:
                    while (songPlaying < sc.soundsToPlay.Count)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the time left is less than the cross duration
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus ||
                                   (currentSource.isPlaying || isPaused || currentSource.mute) &&
                                   (currentSource.clip.samples - currentSource.timeSamples) * 1f /
                                   (currentSource.clip.frequency * 1f) > modifiedCrossDuration)
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        if (!skipSongs)
                        {
                            songPlaying++;
                        }
                        else
                        {
                            songPlaying = songPlaying + skipAmount;
                            if (songPlaying < 0) songPlaying = 0;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                        if (sc.soundsToPlay.Count <= songPlaying) StartCoroutine("CrossoutAll", crossDuration);
                    }
                    break;
                case PlayMethod.OncePlayThroughWithDelay:
                    while (songPlaying < sc.soundsToPlay.Count)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the song is done before moving on with the delay
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus || currentSource.isPlaying || isPaused || currentSource.mute)
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        yield return new WaitForSeconds(sc.delay);
                        if (!skipSongs)
                        {
                            songPlaying++;
                        }
                        else
                        {
                            songPlaying = songPlaying + skipAmount;
                            if (songPlaying < 0) songPlaying = 0;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                        if (sc.soundsToPlay.Count <= songPlaying) StartCoroutine("CrossoutAll", crossDuration);
                    }
                    break;
                case PlayMethod.OncePlayThroughWithRandomDelayInRange:
                    while (songPlaying < sc.soundsToPlay.Count)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the song is done before moving on with the delay
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus || currentSource.isPlaying || isPaused || currentSource.mute)
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        var randomDelay = Random.Range(sc.minDelay, sc.maxDelay);
                        yield return new WaitForSeconds(randomDelay);
                        if (!skipSongs)
                        {
                            songPlaying++;
                        }
                        else
                        {
                            songPlaying = songPlaying + skipAmount;
                            if (songPlaying < 0) songPlaying = 0;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                        if (sc.soundsToPlay.Count <= songPlaying) StartCoroutine("CrossoutAll", crossDuration);
                    }
                    break;
                case PlayMethod.ShufflePlayThrough:
                    SoundManagerTools.ShuffleTwo(ref sc.soundsToPlay, ref sc.baseVolumes);
                    while (Application.isPlaying)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the time left is less than the cross duration
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus ||
                                   (currentSource.isPlaying || isPaused || currentSource.mute) &&
                                   (currentSource.clip.samples - currentSource.timeSamples) * 1f /
                                   (currentSource.clip.frequency * 1f) > modifiedCrossDuration)
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        if (!skipSongs)
                        {
                            songPlaying = (songPlaying + 1) % sc.soundsToPlay.Count;
                        }
                        else
                        {
                            songPlaying = (songPlaying + skipAmount) % sc.soundsToPlay.Count;
                            if (songPlaying < 0) songPlaying += sc.soundsToPlay.Count;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                        if (songPlaying == 0)
                            SoundManagerTools.ShuffleTwo(ref sc.soundsToPlay, ref sc.baseVolumes);
                    }
                    break;
                case PlayMethod.ShufflePlayThroughWithDelay:
                    SoundManagerTools.ShuffleTwo(ref sc.soundsToPlay, ref sc.baseVolumes);
                    while (Application.isPlaying)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the song is done before moving on with the delay
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus || currentSource.isPlaying || isPaused || currentSource.mute)
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        yield return new WaitForSeconds(sc.delay);
                        if (!skipSongs)
                        {
                            songPlaying = (songPlaying + 1) % sc.soundsToPlay.Count;
                        }
                        else
                        {
                            songPlaying = (songPlaying + skipAmount) % sc.soundsToPlay.Count;
                            if (songPlaying < 0) songPlaying += sc.soundsToPlay.Count;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                        if (songPlaying == 0)
                            SoundManagerTools.ShuffleTwo(ref sc.soundsToPlay, ref sc.baseVolumes);
                    }
                    break;
                case PlayMethod.ShufflePlayThroughWithRandomDelayInRange:
                    SoundManagerTools.ShuffleTwo(ref sc.soundsToPlay, ref sc.baseVolumes);
                    while (Application.isPlaying)
                    {
                        modifiedCrossDuration = crossDuration;
                        currentSongIndex = songPlaying;
                        PlayClip(sc.soundsToPlay[songPlaying], sc.baseVolumes[songPlaying]);
                        movingOnFromSong = false;

                        // While the clip is playing, wait until the song is done before moving on with the delay
                        currentSource = IsPlaying(sc.soundsToPlay[songPlaying], false);
                        if (currentSource != null)
                            while (ignoreFromLosingFocus || currentSource.isPlaying || isPaused || currentSource.mute)
                            {
                                if (skipSongs)
                                    break;
                                yield return null;
                            }

                        // Then go to the next song.
                        movingOnFromSong = true;
                        var randomDelay = Random.Range(sc.minDelay, sc.maxDelay);
                        yield return new WaitForSeconds(randomDelay);
                        if (!skipSongs)
                        {
                            songPlaying = (songPlaying + 1) % sc.soundsToPlay.Count;
                        }
                        else
                        {
                            songPlaying = (songPlaying + skipAmount) % sc.soundsToPlay.Count;
                            if (songPlaying < 0) songPlaying += sc.soundsToPlay.Count;
                            skipSongs = false;
                            skipAmount = 0;
                        }
                        if (songPlaying == 0)
                            SoundManagerTools.ShuffleTwo(ref sc.soundsToPlay, ref sc.baseVolumes);
                    }
                    break;
                default:
                    Debug.LogError("This SoundConnection has an invalid PlayMethod.");
                    break;
            }
        }

        #endregion
    }
}