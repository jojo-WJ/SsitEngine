using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SsitEngine.Unity.Sound
{
    public partial class SoundManager
    {
        /// <summary>
        ///     回避开始速度
        /// </summary>
        public static float duckStartSpeed = .1f;

        /// <summary>
        ///     回避终止速度
        /// </summary>
        public static float duckEndSpeed = .5f;

        private readonly Dictionary<string, AudioClip> allClips = new Dictionary<string, AudioClip>();
        private readonly Dictionary<string, float> baseVolumes = new Dictionary<string, float>();

        private readonly Dictionary<string, string> clipsInGroups = new Dictionary<string, string>();

        private readonly Dictionary<string, SFXGroup> groups = new Dictionary<string, SFXGroup>();

        private readonly Dictionary<AudioClip, SFXPoolInfo> ownedPools = new Dictionary<AudioClip, SFXPoolInfo>();
        private readonly Dictionary<string, float> pitchVariations = new Dictionary<string, float>();
        private readonly Dictionary<string, int> prepools = new Dictionary<string, int>();

        private readonly Dictionary<string, float> volumeVariations = new Dictionary<string, float>();

        private bool _mutedSFX;

        private float _pitchSFX = 1f;

        private float _volumeSFX = 1f;

        /// <summary>
        ///     默认的封盖大小
        /// </summary>
        public int capAmount = 3;

        /// <summary>
        ///     音效封盖Map
        /// </summary>
        public Dictionary<int, string> cappedSFXObjects = new Dictionary<int, string>();

        // Map of clip names to group names (dictionaries and hashtables are not supported for serialization)

        [HideInInspector] //不要改变
        public List<string> clipToGroupKeys = new List<string>();

        [HideInInspector] //不要改变
        public List<string> clipToGroupValues = new List<string>();

        /// <summary>
        ///     当前音箱的名称
        /// </summary>
        public List<string> currentPockets = new List<string> {"Default"};

        /// <summary>
        ///     音效播放的延迟map
        /// </summary>
        public Dictionary<AudioSource, float> delayedAudioSources = new Dictionary<AudioSource, float>();

        private UnityEvent duckFunction;
        private int duckNumber;

        private AudioSource duckSource;
        private bool isDucking;

        /// <summary>
        ///     音效开关
        /// </summary>
        public bool offTheSFX;

        private float preDuckPitch = 1f;
        private float preDuckPitchMusic = 1f;
        private float preDuckPitchSFX = 1f;
        private float preDuckVolume = 1f;
        private float preDuckVolumeMusic = 1f;
        private float preDuckVolumeSFX = 1f;

        /// <summary>
        ///     音效加载路径
        /// </summary>
        public string resourcesPath = "Sounds/SFX";

        /// <summary>
        ///     音效运行函数回调
        /// </summary>
        public Dictionary<AudioSource, UnityEvent> runOnEndFunctions = new Dictionary<AudioSource, UnityEvent>();

        public List<float> sfxBaseVolumes = new List<float>();

        /// <summary>
        ///     SFX组列表。在运行时，这是不使用的，所以不要修改这个。
        /// </summary>
        public List<SFXGroup> sfxGroups = new List<SFXGroup>();

        /// <summary>
        ///     对象池之外的对象的SFX对象生存期 .
        /// </summary>
        public float SFXObjectLifetime = 10f;

        public List<float> sfxPitchVariations = new List<float>();

        public List<int> sfxPrePoolAmounts = new List<int>();

        public List<float> sfxVolumeVariations = new List<float>();

        /// <summary>
        ///     本地片段列表
        /// </summary>
        public List<AudioClip> storedSFXs = new List<AudioClip>();

        /// <summary>
        ///     未播放的音效组件列表
        /// </summary>
        public List<GameObject> unOwnedSFXObjects = new List<GameObject>();

        /// <summary>
        ///     音效音量
        /// </summary>
        public float volumeSFX
        {
            get => _volumeSFX;
            set
            {
                foreach (var pair in Instance.ownedPools)
                foreach (var ownedSFXObject in pair.Value.ownedAudioClipPool)
                {
                    if (ownedSFXObject != null)
                    {
                        if (ownedSFXObject.GetComponent<AudioSource>() != null &&
                            (!isDucking || ownedSFXObject.GetComponent<AudioSource>() != duckSource))
                        {
                            ownedSFXObject.GetComponent<AudioSource>().volume = value;
                        }
                    }
                }
                foreach (var unOwnedSFXObject in Instance.unOwnedSFXObjects)
                {
                    if (unOwnedSFXObject != null)
                    {
                        if (unOwnedSFXObject.GetComponent<AudioSource>() != null &&
                            (!isDucking || unOwnedSFXObject.GetComponent<AudioSource>() != duckSource))
                        {
                            unOwnedSFXObject.GetComponent<AudioSource>().volume = value;
                        }
                    }
                }
                _volumeSFX = value;
            }
        }

        /// <summary>
        ///     音效声调
        /// </summary>
        public float pitchSFX
        {
            get => _pitchSFX;
            set
            {
                foreach (var pair in Instance.ownedPools)
                foreach (var ownedSFXObject in pair.Value.ownedAudioClipPool)
                {
                    if (ownedSFXObject != null)
                    {
                        if (ownedSFXObject.GetComponent<AudioSource>() != null &&
                            (!isDucking || ownedSFXObject.GetComponent<AudioSource>() != duckSource))
                        {
                            ownedSFXObject.GetComponent<AudioSource>().pitch = value;
                        }
                    }
                }
                foreach (var unOwnedSFXObject in Instance.unOwnedSFXObjects)
                {
                    if (unOwnedSFXObject != null)
                    {
                        if (unOwnedSFXObject.GetComponent<AudioSource>() != null &&
                            (!isDucking || unOwnedSFXObject.GetComponent<AudioSource>() != duckSource))
                        {
                            unOwnedSFXObject.GetComponent<AudioSource>().pitch = value;
                        }
                    }
                }
                _pitchSFX = value;
            }
        }

        /// <summary>
        ///     音效的最大音量
        /// </summary>
        public float maxSFXVolume { get; set; } = 1f;

        public bool mutedSFX
        {
            get => _mutedSFX;
            set
            {
                foreach (var pair in Instance.ownedPools)
                foreach (var ownedSFXObject in pair.Value.ownedAudioClipPool)
                {
                    if (ownedSFXObject != null)
                    {
                        if (ownedSFXObject.GetComponent<AudioSource>() != null)
                        {
                            if (value)
                            {
                                ownedSFXObject.GetComponent<AudioSource>().mute = value;
                            }
                            else if (!Instance.offTheSFX)
                            {
                                ownedSFXObject.GetComponent<AudioSource>().mute = value;
                            }
                        }
                    }
                }
                foreach (var unOwnedSFXObject in Instance.unOwnedSFXObjects)
                {
                    if (unOwnedSFXObject != null)
                    {
                        if (unOwnedSFXObject.GetComponent<AudioSource>() != null)
                        {
                            if (value)
                            {
                                unOwnedSFXObject.GetComponent<AudioSource>().mute = value;
                            }
                            else if (!Instance.offTheSFX)
                            {
                                unOwnedSFXObject.GetComponent<AudioSource>().mute = value;
                            }
                        }
                    }
                }
                _mutedSFX = value;
            }
        }
    }
}