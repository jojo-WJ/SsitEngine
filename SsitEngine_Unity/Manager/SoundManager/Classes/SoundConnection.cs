using System;
using System.Collections.Generic;
using UnityEngine;

namespace SsitEngine.Unity.Sound
{
    [Serializable]
    public class SoundConnection
    {
        /// <summary>
        ///     每个声音片段的默认音量大小(0--1)
        /// </summary>
        public List<float> baseVolumes = new List<float>();

        /// <summary>
        ///     确定的延迟
        /// </summary>
        public float delay;

        /// <summary>
        ///     它是否是一个自定义的声音连接。
        /// </summary>
        public bool isCustomLevel;

        /// <summary>
        ///     声响连接的场景名称，或自定义声音连接的名称。
        /// </summary>
        public string level;

        /// <summary>
        ///     随机延迟最大范围
        /// </summary>
        public float maxDelay;

        /// <summary>
        ///     随机延迟最小范围
        /// </summary>
        public float minDelay;

        /// <summary>
        ///     播放方式。
        /// </summary>
        public SoundManager.PlayMethod playMethod;

        /// <summary>
        ///     声音片段
        /// </summary>
        public List<AudioClip> soundsToPlay;

        /// <summary>
        ///     配置源构造(默认连续无延迟)
        ///     <param name="lvl">场景名称</param>
        ///     <param name="audioList">声音片段列表</param>
        /// </summary>
        public SoundConnection( string lvl, params AudioClip[] audioList )
        {
            level = lvl;
            isCustomLevel = false;
            playMethod = SoundManager.PlayMethod.ContinuousPlayThrough;
            minDelay = 0f;
            maxDelay = 0f;
            delay = 0f;
            soundsToPlay = new List<AudioClip>();
            baseVolumes = new List<float>();
            foreach (var audio in audioList)
                if (!soundsToPlay.Contains(audio))
                {
                    soundsToPlay.Add(audio);
                    baseVolumes.Add(1f);
                }
        }

        /// <summary>
        ///     无延迟
        /// </summary>
        public SoundConnection( string lvl, SoundManager.PlayMethod method, params AudioClip[] audioList )
        {
            level = lvl;
            isCustomLevel = false;
            playMethod = method;
            switch (playMethod)
            {
                case SoundManager.PlayMethod.ContinuousPlayThrough:
                case SoundManager.PlayMethod.OncePlayThrough:
                case SoundManager.PlayMethod.ShufflePlayThrough:
                    break;
                default:
                    Debug.LogWarning("No delay was set in the constructor so there will be none.");
                    break;
            }
            minDelay = 0f;
            maxDelay = 0f;
            delay = 0f;
            soundsToPlay = new List<AudioClip>();
            baseVolumes = new List<float>();
            foreach (var audio in audioList)
                if (!soundsToPlay.Contains(audio))
                {
                    soundsToPlay.Add(audio);
                    baseVolumes.Add(1f);
                }
        }

        /// <summary>
        ///     确定延迟 delay（延迟范围0-delay）
        /// </summary>
        public SoundConnection( string lvl, SoundManager.PlayMethod method, float delayPlay,
            params AudioClip[] audioList )
        {
            level = lvl;
            isCustomLevel = false;
            playMethod = method;
            minDelay = 0f;
            maxDelay = delayPlay;
            delay = delayPlay;
            soundsToPlay = new List<AudioClip>();
            baseVolumes = new List<float>();
            foreach (var audio in audioList)
                if (!soundsToPlay.Contains(audio))
                {
                    soundsToPlay.Add(audio);
                    baseVolumes.Add(1f);
                }
        }

        /// <summary>
        ///     确定延迟范围（min - max）/ 2
        /// </summary>
        public SoundConnection( string lvl, SoundManager.PlayMethod method, float minDelayPlay, float maxDelayPlay,
            params AudioClip[] audioList )
        {
            level = lvl;
            isCustomLevel = false;
            playMethod = method;
            minDelay = minDelayPlay;
            maxDelay = maxDelayPlay;
            delay = (maxDelayPlay + minDelayPlay) / 2f;
            soundsToPlay = new List<AudioClip>();
            baseVolumes = new List<float>();
            foreach (var audio in audioList)
                if (!soundsToPlay.Contains(audio))
                {
                    soundsToPlay.Add(audio);
                    baseVolumes.Add(1f);
                }
        }

        /// <summary>
        ///     适用于流式的加载场景（必须设置事件处理它）
        /// </summary>
        public void SetToCustom()
        {
            isCustomLevel = true;
        }
    }
}