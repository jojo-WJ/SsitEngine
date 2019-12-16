using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SsitEngine.Unity.Sound
{
    public partial class SoundManager
    {
        public const int SOUNDMANAGER_FALSE = -1;

        private bool _mutedMusic;

        [SerializeField] public AudioSource[] audios;

        private bool[] audiosPaused;

        /// <summary>
        ///     The crossfade duration.
        /// </summary>
        public float crossDuration = 5f;

        /// <summary>
        ///     The current level.
        /// </summary>
        public string currentLevel;

        private int currentSongIndex = -1;

        /// <summary>
        ///     The current SoundConnection.
        /// </summary>
        public SoundConnection currentSoundConnection;

        private AudioSource currentSource;

        private bool ignoreFromLosingFocus;

        /// <summary>
        ///     Set this if you wish to ignore the level loading functionality.
        /// </summary>
        public bool ignoreLevelLoad;

        private readonly bool[] inCrossing = {false, false};

        private UnityEvent InternalCallback;

        /// <summary>
        ///     Whether sound is paused.
        /// </summary>
        public bool isPaused;

        private float lastLevelLoad;

        private float modifiedCrossDuration;

        /// <summary>
        ///     背景音乐是否被迫移到下一首歌。建议不要修改此值。
        /// </summary>
        [HideInInspector] public bool movingOnFromSong;

        /// <summary>
        ///     Turn off the background music.
        /// </summary>
        public bool offTheBGM;

        /// <summary>
        ///     Called when crossfade in begins.
        /// </summary>
        public UnityEvent OnCrossInBegin;

        /// <summary>
        ///     Called when crossfade out begins.
        /// </summary>
        public UnityEvent OnCrossOutBegin;

        /// <summary>
        ///     Called when a song begins, AFTER crossfade in ends.
        /// </summary>
        public UnityEvent OnSongBegin;

        /*
        /// <summary>
        /// Song callback delegate.
        /// </summary>
        public SongCallBack SongCallBack;
        */

        /// <summary>
        ///     Called when a song ends, AFTER crossfade out ends as well.
        /// </summary>
        public UnityEvent OnSongEnd;

        private readonly bool[] outCrossing = {false, false};

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        public bool showDebug;

        private int skipAmount;

        private bool skipSongs;

        /// <summary>
        ///     The sound connections.
        /// </summary>
        public List<SoundConnection> soundConnections = new List<SoundConnection>();

        /// <summary>
        ///     Gets or sets the volume of BGM track 1.
        /// </summary>
        /// <value>
        ///     The volume.
        /// </value>
        public float volume1
        {
            get => audios[0].volume;
            set => audios[0].volume = value;
        }

        /// <summary>
        ///     Gets or sets the volume of BGM track 2.
        /// </summary>
        /// <value>
        ///     The volume.
        /// </value>
        public float volume2
        {
            get => audios[1].volume;
            set => audios[1].volume = value;
        }

        /// <summary>
        ///     Gets or sets the max music volume.
        /// </summary>
        /// <value>
        ///     The max music volume.
        /// </value>
        public float maxMusicVolume { get; set; } = 1f;

        /// <summary>
        ///     Gets or sets the max volume.
        /// </summary>
        /// <value>
        ///     The max volume.
        /// </value>
        public float maxVolume { get; set; } = 1f;

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="SoundManager" /> muted music.
        /// </summary>
        /// <value>
        ///     <c>true</c> if muted music; otherwise, <c>false</c>.
        /// </value>
        public bool mutedMusic
        {
            get => _mutedMusic;
            set
            {
                audios[0].mute = audios[1].mute = value;
                _mutedMusic = value;
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="SoundManager" /> is muted.
        /// </summary>
        /// <value>
        ///     <c>true</c> if muted; otherwise, <c>false</c>.
        /// </value>
        public bool muted
        {
            get => mutedMusic || mutedSFX;
            set => mutedMusic = mutedSFX = value;
        }

        private bool crossingIn => inCrossing[0] || inCrossing[1];

        private bool crossingOut => outCrossing[0] || outCrossing[1];

        #region Interval Variable

        private bool silentLevel;
        private bool ignoreCrossDuration;
        private int currentPlaying;

        #endregion
    }
}