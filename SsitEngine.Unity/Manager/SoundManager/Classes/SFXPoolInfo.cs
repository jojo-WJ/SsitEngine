using System.Collections.Generic;
using UnityEngine;

namespace SsitEngine.Unity.Sound
{
    /// <summary>
    ///     Contains information on SFX Pools.
    /// </summary>
    public class SFXPoolInfo
    {
        public float baseVolume = 1f;
        public int currentIndexInPool;
        public List<GameObject> ownedAudioClipPool = new List<GameObject>();
        public float pitchVariation;

        public int prepoolAmount;
        public List<float> timesOfDeath = new List<float>();
        public float volumeVariation;

        public SFXPoolInfo( int index, int minAmount, List<float> times, List<GameObject> pool, float baseVol = 1f,
            float volVar = 0f, float pitchVar = 0f )
        {
            currentIndexInPool = index;
            prepoolAmount = minAmount;
            timesOfDeath = times;
            ownedAudioClipPool = pool;
            baseVolume = baseVol;
            volumeVariation = volVar;
            pitchVariation = pitchVar;
        }
    }
}