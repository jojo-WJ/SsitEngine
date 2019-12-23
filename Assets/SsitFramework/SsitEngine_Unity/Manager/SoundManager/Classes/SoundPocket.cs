using System.Collections.Generic;
using UnityEngine;

namespace SsitEngine.Unity.Sound
{
    [AddComponentMenu("AntiLunchBox/SoundPocket")]
    public class SoundPocket : MonoBehaviour
    {
        private readonly Dictionary<string, string> clipsInGroups = new Dictionary<string, string>();

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        public float autoBaseVolume = 1f;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        public float autoPitchVariation;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        public int autoPrepoolAmount;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        public float autoVolumeVariation;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        public List<string> clipToGroupKeys = new List<string>();

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        public List<string> clipToGroupValues = new List<string>();

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        public int groupAddIndex;

        /// <summary>
        ///     The audio clips in the SoundPocket.
        /// </summary>
        public List<AudioClip> pocketClips = new List<AudioClip>();

        /// <summary>
        ///     Name of the SoundPocket. If a SoundPocket already exists on the SoundManager, it will not be readded.
        /// </summary>
        public string pocketName = "Pocket";

        /// <summary>
        ///     Determines how this pocket will be added to the SoundManager once the SoundPocket is loaded. If additive, these SFX
        ///     will be added to the SoundManager. If subtractive, the SFX currently on the SoundManager will be removed before
        ///     these are added.
        /// </summary>
        public SoundPocketType pocketType = SoundPocketType.Additive;

        /// <summary>
        ///     The sfx base volumes.
        /// </summary>
        public List<float> sfxBaseVolumes = new List<float>();

        /// <summary>
        ///     These are possible group names for SFXs to be applied to. If the group exists on the SoundManager, it'll be added
        ///     to that group. Otherwise, a new group will be created.
        /// </summary>
        public List<string> sfxGroups = new List<string>();

        /// <summary>
        ///     The sfx pitch variations.
        /// </summary>
        public List<float> sfxPitchVariations = new List<float>();

        /// <summary>
        ///     The sfx prepool amounts.
        /// </summary>
        public List<int> sfxPrePoolAmounts = new List<int>();

        /// <summary>
        ///     The sfx volume variations.
        /// </summary>
        public List<float> sfxVolumeVariations = new List<float>();

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        public bool showAsGrouped;

        /// <summary>
        ///     Editor variable -- IGNORE AND DO NOT MODIFY
        /// </summary>
        public List<bool> showSFXDetails = new List<bool>();

        private void Awake()
        {
            Setup();
            DestroyMe();
        }

        /// <summary>
        ///     Setup this instance.
        /// </summary>
        public void Setup()
        {
            SetupDictionaries();
            switch (pocketType)
            {
                case SoundPocketType.Subtractive:
                    if (SoundManager.Instance.currentPockets.Count == 1 &&
                        SoundManager.Instance.currentPockets[0] == pocketName)
                    {
                        return;
                    }
                    SoundManager.DeleteSFX();
                    SoundManager.Instance.currentPockets.Clear();
                    break;
                case SoundPocketType.Additive:
                default:
                    if (SoundManager.Instance.currentPockets.Contains(pocketName))
                    {
                        return;
                    }
                    break;
            }

            for (var i = 0; i < pocketClips.Count; i++)
            {
                var pocketClip = pocketClips[i];
                if (clipsInGroups.ContainsKey(pocketClip.name))
                {
                    SoundManager.SaveSFX(pocketClip, clipsInGroups[pocketClip.name]);
                }
                else
                {
                    SoundManager.SaveSFX(pocketClip);
                }

                SoundManager.ApplySFXAttributes(pocketClip, sfxPrePoolAmounts[i], sfxBaseVolumes[i],
                    sfxVolumeVariations[i], sfxPitchVariations[i]);
            }

            SoundManager.Instance.currentPockets.Add(pocketName);
        }

        private void SetupDictionaries()
        {
            clipsInGroups.Clear();
            for (var i = 0; i < clipToGroupKeys.Count; i++)
            {
                clipsInGroups.Add(clipToGroupKeys[i], clipToGroupValues[i]);
            }
        }

        /// <summary>
        ///     Destroys this instance when finished. Will destroy the GameObject too if it's the only component on it.
        /// </summary>
        public void DestroyMe()
        {
            if (gameObject.GetComponents<Component>().Length - gameObject.GetComponents<Transform>().Length == 1)
            {
                Destroy(gameObject);
            }
            else
            {
                Destroy(this);
            }
        }
    }

    /// <summary>
    ///     Determines how this pocket will be added to the SoundManager once the SoundPocket is loaded. If additive, these SFX
    ///     will be added to the SoundManager. If subtractive, the SFX currently on the SoundManager will be removed before
    ///     these are added.
    /// </summary>
    public enum SoundPocketType
    {
        Additive,
        Subtractive
    }
}