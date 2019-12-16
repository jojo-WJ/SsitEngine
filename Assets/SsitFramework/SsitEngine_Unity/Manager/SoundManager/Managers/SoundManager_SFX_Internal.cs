using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SsitEngine.Unity.Sound
{
    public partial class SoundManager
    {
        /// <summary>
        ///     初始化声音效果池
        /// </summary>
        private void SetupSoundFX()
        {
            SetupDictionary();

            foreach (var pair in ownedPools)
                pair.Value.ownedAudioClipPool.Clear();
            ownedPools.Clear();
            unOwnedSFXObjects.Clear();
            cappedSFXObjects.Clear();

            foreach (var entry in allClips)
                PrePoolClip(entry.Value, prepools[entry.Key]);
        }

        private void PrePoolClip( AudioClip clip, int prepoolAmount )
        {
            for (var i = 0; i < prepoolAmount; i++)
                AddOwnedSFXObject(clip);
        }

        private void RemoveSFXObject( SFXPoolInfo info, int index )
        {
            var gO = info.ownedAudioClipPool[index];
            info.ownedAudioClipPool.RemoveAt(index);
            info.timesOfDeath.RemoveAt(index);

            if (info.currentIndexInPool >= index)
                info.currentIndexInPool = 0;

            Destroy(gO);
        }

        /* these functions convert the editor dictionaries to efficient dictionaries while in play */

        private void SetupDictionary()
        {
            allClips.Clear();
            prepools.Clear();
            baseVolumes.Clear();
            volumeVariations.Clear();
            pitchVariations.Clear();
            for (var i = 0; i < storedSFXs.Count; i++)
            {
                if (storedSFXs[i] == null) continue;
                var clipName = storedSFXs[i].name;
                allClips.Add(clipName, storedSFXs[i]);
                prepools.Add(clipName, sfxPrePoolAmounts[i]);
                baseVolumes.Add(clipName, sfxBaseVolumes[i]);
                volumeVariations.Add(clipName, sfxVolumeVariations[i]);
                pitchVariations.Add(clipName, sfxPitchVariations[i]);
            }
#if !UNITY_EDITOR
		storedSFXs.Clear();
#endif

            if (clipToGroupKeys.Count != clipToGroupValues.Count)
                //this should never be the case, but in case they are out of sync, sync them.
            {
                if (clipToGroupKeys.Count > clipToGroupValues.Count)
                    clipToGroupKeys.RemoveRange(clipToGroupValues.Count,
                        clipToGroupKeys.Count - clipToGroupValues.Count);
                else if (clipToGroupValues.Count > clipToGroupKeys.Count)
                    clipToGroupValues.RemoveRange(clipToGroupKeys.Count,
                        clipToGroupValues.Count - clipToGroupKeys.Count);
            }

            clipsInGroups.Clear();
            groups.Clear();

            for (var i = 0; i < clipToGroupValues.Count; i++)
            {
                if (!ClipNameIsValid(clipToGroupKeys[i]))
                    continue;

                // Set up clipsInGroups, which maps clip names to group names if they are in a group
                clipsInGroups.Add(clipToGroupKeys[i], clipToGroupValues[i]);

                // Set up groups, which maps group names to SFXGroups and populates the clip lists
                if (!groups.ContainsKey(clipToGroupValues[i]))
                {
                    groups.Add(clipToGroupValues[i],
                        new SFXGroup(clipToGroupValues[i], Load(clipToGroupKeys[i])));
                }
                else
                {
                    if (groups[clipToGroupValues[i]] == null)
                        groups[clipToGroupValues[i]] = new SFXGroup(clipToGroupValues[i], Load(clipToGroupKeys[i]));
                    else
                        groups[clipToGroupValues[i]].clips.Add(Load(clipToGroupKeys[i]));
                }
            }

            foreach (var sfxGroup in sfxGroups)
                if (sfxGroup != null && groups.ContainsKey(sfxGroup.groupName))
                    groups[sfxGroup.groupName].specificCapAmount = sfxGroup.specificCapAmount;
#if !UNITY_EDITOR
		sfxGroups.Clear();
#endif
        }

        private void AddClipToGroup( string clipName, string groupName )
        {
            // if the clips in a group, set the clip instead.
            if (clipsInGroups.ContainsKey(clipName))
            {
                Debug.LogWarning("This AudioClip(" + clipName + ") is already assigned to a group: " +
                                 GetClipToGroup(clipName) + ". It will be moved to the new group.");
                SetClipToGroup(clipName, groupName);
                return;
            }

            // if group doesn't exist, create one and add the clip.  Otherwise, add it to the group's clip list.
            var grp = GetGroupByGroupName(groupName);
            if (grp == null)
                groups.Add(groupName, new SFXGroup(groupName, Load(clipName)));
            else
                grp.clips.Add(Load(clipName));
            clipsInGroups.Add(clipName, groupName);

#if UNITY_EDITOR
            clipToGroupKeys.Add(clipName);
            clipToGroupValues.Add(groupName);
            if (grp == null)
                sfxGroups.Add(groups[groupName]);
#endif
        }

        private void SetClipToGroup( string clipName, string groupName )
        {
            // if in a group, remove it from the group before adding it.
            var grp = GetGroupForClipName(clipName);
            if (grp != null)
                RemoveClipFromGroup(clipName);
            AddClipToGroup(clipName, groupName);
        }

        private void RemoveClipFromGroup( string clipName )
        {
            // if not in a group, do nothing
            var grp = GetGroupForClipName(clipName);
            if (grp == null) return;
            // if in a group, remove it
            grp.clips.Remove(Load(clipName));
            clipsInGroups.Remove(clipName);

#if UNITY_EDITOR
            var index = clipToGroupKeys.IndexOf(clipName);
            clipToGroupKeys.RemoveAt(index);
            clipToGroupValues.RemoveAt(index);
#endif
        }

        private string GetClipToGroup( string clipName )
        {
            return clipsInGroups[clipName];
        }

        /* end of editor necessary functions */

        private void PSFX( bool pause )
        {
            foreach (var pair in ownedPools)
            foreach (var ownedSFXObject in pair.Value.ownedAudioClipPool)
            {
#if UNITY_3_4 || UNITY_3_5
				if(ownedSFXObject != null && ownedSFXObject.active)
#else
                if (ownedSFXObject != null && ownedSFXObject.activeSelf)
#endif
                    if (ownedSFXObject.GetComponent<AudioSource>() != null)
                        if (pause)
                            ownedSFXObject.GetComponent<AudioSource>().Pause();
                        else
                            ownedSFXObject.GetComponent<AudioSource>().Play();
            }
            foreach (var unOwnedSFXObject in unOwnedSFXObjects)
            {
#if UNITY_3_4 || UNITY_3_5
			if(unOwnedSFXObject != null && unOwnedSFXObject.active)
#else
                if (unOwnedSFXObject != null && unOwnedSFXObject.activeSelf)
#endif
                    if (unOwnedSFXObject.GetComponent<AudioSource>() != null)
                        if (pause)
                            unOwnedSFXObject.GetComponent<AudioSource>().Pause();
                        else
                            unOwnedSFXObject.GetComponent<AudioSource>().Play();
            }
        }

        private void HandleSFX()
        {
            if (isPaused)
                return;

            // Deactivate objects
            foreach (var pair in ownedPools)
                for (var i = 0; i < pair.Value.ownedAudioClipPool.Count; ++i)
                {
#if UNITY_3_4 || UNITY_3_5
	            if (pair.Value.ownedAudioClipPool[i].active)
#else
                    if (pair.Value.ownedAudioClipPool[i].activeSelf)
#endif
                    {
                        var thisAudio = pair.Value.ownedAudioClipPool[i].GetComponent<AudioSource>();
                        if (!thisAudio.isPlaying) // if not playing
                        {
                            if (delayedAudioSources.ContainsKey(thisAudio)) // skip if delayed still
                                continue;

                            if (runOnEndFunctions.ContainsKey(thisAudio) && runOnEndFunctions[thisAudio] != null)
                                // call run on end for sfx if its there
                            {
                                runOnEndFunctions[thisAudio].Invoke();
                                runOnEndFunctions.Remove(thisAudio);
                            }

                            var instanceID = pair.Value.ownedAudioClipPool[i].GetInstanceID();
                            if (cappedSFXObjects.ContainsKey(instanceID))
                                cappedSFXObjects.Remove(instanceID);
#if UNITY_3_4 || UNITY_3_5
	                    pair.Value.ownedAudioClipPool[i].SetActiveRecursively(false);
#else
                            pair.Value.ownedAudioClipPool[i].SetActive(false);
#endif
                            if (pair.Value.prepoolAmount <= i)
                                pair.Value.timesOfDeath[i] = Time.time + SFXObjectLifetime;
                        }
                        else // if it playing or muted and palying
                        {
                            if (delayedAudioSources.ContainsKey(thisAudio))
                                // if delayed but is playign now, remove from delayed list
                                delayedAudioSources.Remove(thisAudio);
                        }
                    }
                    else if (pair.Value.prepoolAmount <= i && Time.time > pair.Value.timesOfDeath[i])
                    {
                        RemoveSFXObject(pair.Value, i);
                    }
                }

            // Handle removing unowned audio sfx
            for (var i = unOwnedSFXObjects.Count - 1; i >= 0; i--)
            {
                if (unOwnedSFXObjects[i] != null)
                    if (unOwnedSFXObjects[i].GetComponent<AudioSource>() != null)
                    {
                        var thisAudio = unOwnedSFXObjects[i].GetComponent<AudioSource>();
                        if (thisAudio.isPlaying) // if playign or muted and playing
                        {
                            if (delayedAudioSources.ContainsKey(thisAudio))
                                // if delayed but is playign now, remove from delayed list
                                delayedAudioSources.Remove(thisAudio);
                            continue; // dont remove
                        }
                        if (delayedAudioSources.ContainsKey(thisAudio)) // skip if delayed still
                            continue;

                        if (runOnEndFunctions.ContainsKey(thisAudio) && runOnEndFunctions[thisAudio] != null)
                            // call run on end for sfx if its there
                        {
                            runOnEndFunctions[thisAudio].Invoke();
                            runOnEndFunctions.Remove(thisAudio);
                        }
                    }
                unOwnedSFXObjects.RemoveAt(i);
            }
        }

        private GameObject GetNextInactiveSFXObject( AudioClip clip, bool isUI = false )
        {
            if (!ownedPools.ContainsKey(clip) || ownedPools[clip].ownedAudioClipPool.Count == 0)
                return AddOwnedSFXObject(clip, isUI);
            var info = ownedPools[clip];
            for (var i = (info.currentIndexInPool + 1) % info.ownedAudioClipPool.Count;
                i != info.currentIndexInPool;
                i = (i + 1) % info.ownedAudioClipPool.Count)
            {
#if UNITY_3_4 || UNITY_3_5
            if (!info.ownedAudioClipPool[i].active)
#else
                if (!info.ownedAudioClipPool[i].activeSelf)
#endif
                {
                    ownedPools[clip].currentIndexInPool = i;
                    ResetSFXObject(info.ownedAudioClipPool[i], isUI);
                    return info.ownedAudioClipPool[i];
                }
            }
            return AddOwnedSFXObject(clip, isUI);
        }

        private GameObject AddOwnedSFXObject( AudioClip clip, bool isUI = false )
        {
            var SFXObject = new GameObject("SFX-[" + clip.name + "]", typeof(AudioSource));
            SFXObject.transform.parent = transform;
            SFXObject.name += "(" + SFXObject.GetInstanceID() + ")";
            SFXObject.GetComponent<AudioSource>().playOnAwake = false;
            //GameObject.DontDestroyOnLoad( SFXObject );

            if (ownedPools.ContainsKey(clip))
            {
                ownedPools[clip].ownedAudioClipPool.Add(SFXObject);
                ownedPools[clip].timesOfDeath.Add(0f);
            }
            else
            {
                var thisPrepoolAmount = 0;
                var thisBaseVolume = 1f;
                float thisVolumeVariation = 0f, thisPitchVariation = 0f;
                var clipName = clip.name;
                if (allClips.ContainsKey(clipName))
                {
                    thisPrepoolAmount = prepools[clipName];
                    thisBaseVolume = baseVolumes[clipName];
                    thisVolumeVariation = volumeVariations[clipName];
                    thisPitchVariation = pitchVariations[clipName];
                }
                ownedPools.Add(clip,
                    new SFXPoolInfo(0, thisPrepoolAmount, new List<float> {0f}, new List<GameObject> {SFXObject},
                        thisBaseVolume, thisVolumeVariation, thisPitchVariation));
            }
            ResetSFXObject(SFXObject, isUI);
            SFXObject.GetComponent<AudioSource>().clip = clip;
            return SFXObject;
        }

        private AudioSource PlaySFXAt( AudioClip clip, float volume, float pitch, Vector3 location = default,
            bool capped = false, string cappedID = "", bool looping = false, float delay = 0f,
            UnityEvent runOnEndFunction = null, SoundDuckingSetting duckingSetting = SoundDuckingSetting.DoNotDuck,
            float duckVolume = 0f, float duckPitch = 1f )
        {
            var tempGO = GetNextInactiveSFXObject(clip);
            if (tempGO == null)
                return null;

            var aSource = tempGO.GetComponent<AudioSource>();

            aSource.transform.position = location;
#if UNITY_3_4 || UNITY_3_5
        aSource.gameObject.SetActiveRecursively(true);
#else
            aSource.gameObject.SetActive(true);
#endif
            return PlaySFXBase(aSource, clip, volume, pitch, capped, cappedID, looping, delay, runOnEndFunction,
                duckingSetting, duckVolume, duckPitch);
        }

        private AudioSource PlaySFXUIAt( AudioClip clip, float volume, float pitch, bool capped = false,
            string cappedID = "", bool looping = false, float delay = 0f, UnityEvent runOnEndFunction = null,
            SoundDuckingSetting duckingSetting = SoundDuckingSetting.DoNotDuck, float duckVolume = 0f,
            float duckPitch = 1f )
        {
            var tempGO = GetNextInactiveSFXObject(clip, true);
            if (tempGO == null)
                return null;

            var aSource = tempGO.GetComponent<AudioSource>();

            aSource.transform.position = Vector3.zero;
#if UNITY_3_4 || UNITY_3_5
        aSource.gameObject.SetActiveRecursively(true);
#else
            aSource.gameObject.SetActive(true);
#endif
            return PlaySFXBase(aSource, clip, volume, pitch, capped, cappedID, looping, delay, runOnEndFunction,
                duckingSetting, duckVolume, duckPitch);
        }

        private AudioSource PlaySFXOn( AudioSource aSource, AudioClip clip, float volume, float pitch,
            bool capped = false, string cappedID = "", bool looping = false, float delay = 0f,
            UnityEvent runOnEndFunction = null, SoundDuckingSetting duckingSetting = SoundDuckingSetting.DoNotDuck,
            float duckVolume = 0f, float duckPitch = 1f )
        {
            aSource.clip = clip;

            return PlaySFXBase(aSource, clip, volume, pitch, capped, cappedID, looping, delay, runOnEndFunction,
                duckingSetting, duckVolume, duckPitch);
        }

        private AudioSource PlaySFXBase( AudioSource aSource, AudioClip clip, float volume, float pitch,
            bool capped = false, string cappedID = "", bool looping = false, float delay = 0f,
            UnityEvent runOnEndFunction = null, SoundDuckingSetting duckingSetting = SoundDuckingSetting.DoNotDuck,
            float duckVolume = 0f, float duckPitch = 1f )
        {
            aSource.Stop();
            var clipName = clip.name;
            if (pitchVariations.ContainsKey(clipName))
                aSource.pitch = pitch.Vary(pitchVariations[clipName]);
            else
                aSource.pitch = pitch;

            if (baseVolumes.ContainsKey(clipName))
                volume = volume * baseVolumes[clipName];
            if (volumeVariations.ContainsKey(clipName))
                aSource.volume = volume.VaryWithRestrictions(volumeVariations[clipName]);
            else
                aSource.volume = volume;

            if (!capped)
                aSource.loop = looping;
            aSource.mute = mutedSFX;

            if (delay <= 0f)
            {
                aSource.Play();
            }
            else
            {
                if (!delayedAudioSources.ContainsKey(aSource))
                    delayedAudioSources.Add(aSource, delay);
                else
                    delayedAudioSources[aSource] = delay;
#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1
			aSource.Play((ulong)(44100f*delay));
#else
                aSource.PlayDelayed(delay);
#endif
            }

            if (runOnEndFunction != null)
            {
                if (runOnEndFunctions.ContainsKey(aSource))
                    runOnEndFunctions[aSource] = runOnEndFunction;
                else
                    runOnEndFunctions.Add(aSource, runOnEndFunction);
            }

            Duck(duckingSetting, duckVolume, duckPitch, volume, pitch, aSource);

            if (capped && !string.IsNullOrEmpty(cappedID))
                cappedSFXObjects.Add(aSource.gameObject.GetInstanceID(), cappedID);

            return aSource;
        }

        private AudioSource PlaySFXLoopOn( AudioSource source, AudioClip clip, bool tillDestroy, float volume,
            float pitch, float maxDuration, UnityEvent runOnEndFunction = null,
            SoundDuckingSetting duckingSetting = SoundDuckingSetting.DoNotDuck, float duckVolume = 0f,
            float duckPitch = 1f )
        {
            source.Stop();
            source.clip = clip;
            var clipName = clip.name;
            if (pitchVariations.ContainsKey(clipName))
                source.pitch = pitch.Vary(pitchVariations[clipName]);
            else
                source.pitch = pitch;
            if (baseVolumes.ContainsKey(clipName))
                volume = volume * baseVolumes[clipName];
            if (volumeVariations.ContainsKey(clipName))
                source.volume = volume.VaryWithRestrictions(volumeVariations[clipName]);
            else
                source.volume = volume;
            source.mute = Instance.mutedSFX;
            source.loop = true;
            source.Play();

            if (runOnEndFunction != null)
            {
                if (runOnEndFunctions.ContainsKey(source))
                    runOnEndFunctions[source] = runOnEndFunction;
                else
                    runOnEndFunctions.Add(source, runOnEndFunction);
            }

            Duck(duckingSetting, duckVolume, duckPitch, volume, pitch, source);

            Instance.StartCoroutine(Instance._PlaySFXLoopTillDestroy(source.gameObject, source, tillDestroy,
                maxDuration));
            return source;
        }

        private IEnumerator _PlaySFXLoopTillDestroy( GameObject gO, AudioSource source, bool tillDestroy,
            float maxDuration )
        {
            var trackEndTime = false;
            var endTime = Time.time + maxDuration;
            if (!tillDestroy || maxDuration > 0.0f)
                trackEndTime = true;


            while (ShouldContinueLoop(gO, trackEndTime, endTime)) yield return null;

            source.Stop();
        }

        private void CheckInsertionIntoUnownedSFXObjects( AudioSource aSource )
        {
            if (!IsOwnedSFXObject(aSource) && !unOwnedSFXObjects.Contains(aSource.gameObject))
                Instance.unOwnedSFXObjects.Add(aSource.gameObject);
        }

        private void _StopSFX()
        {
            foreach (var pair in ownedPools)
            foreach (var ownedSFXObject in pair.Value.ownedAudioClipPool)
#if UNITY_3_4 || UNITY_3_5
				if(ownedSFXObject != null && ownedSFXObject.active)
#else
                if (ownedSFXObject != null && ownedSFXObject.activeSelf)
#endif
                    if (ownedSFXObject.GetComponent<AudioSource>() != null)
                        ownedSFXObject.GetComponent<AudioSource>().Stop();

            foreach (var unOwnedSFXObject in unOwnedSFXObjects)
#if UNITY_3_4 || UNITY_3_5
			if(unOwnedSFXObject != null && unOwnedSFXObject.active)
#else
                if (unOwnedSFXObject != null && unOwnedSFXObject.activeSelf)
#endif
                    if (unOwnedSFXObject.GetComponent<AudioSource>() != null)
                        unOwnedSFXObject.GetComponent<AudioSource>().Stop();

            delayedAudioSources.Clear();
        }

        private bool ShouldContinueLoop( GameObject gO, bool trackEndTime, float endTime )
        {
#if UNITY_3_4 || UNITY_3_5
		bool shouldContinue = (gO != null && gO.active);
#else
            var shouldContinue = gO != null && gO.activeSelf;
#endif
            if (trackEndTime)
                shouldContinue = shouldContinue && Time.time < endTime;
            return shouldContinue;
        }

        /// <summary>
        ///     Determines whether the specified cappedID is at capacity.
        /// </summary>
        private bool IsAtCapacity( string cappedID, string clipName )
        {
            var thisCapAmount = capAmount;

            // Check if in a group and has a specific cap amount
            var grp = GetGroupForClipName(clipName);
            if (grp != null)
            {
                if (grp.specificCapAmount == 0) // If no cap amount on this group
                    return false;
                if (grp.specificCapAmount != -1) // If it is a specific cap amount
                    thisCapAmount = grp.specificCapAmount;
            }

            // If there are no other capped objects with this cappedID, then it can't be at capacity
            if (!cappedSFXObjects.ContainsValue(cappedID))
                return false;

            // Check the count of capped objects with the same cappedID, if >= return true
            var count = 0;
            foreach (var id in cappedSFXObjects.Values)
                if (id == cappedID)
                {
                    count++;
                    if (count >= thisCapAmount)
                        return true;
                }
            return false;
        }

        private bool IsOwnedSFXObject( AudioSource aSource )
        {
            if (aSource.clip == null) //not common occurence here at ALL
            {
                if (aSource.gameObject.name.StartsWith("SFX-["))
                    foreach (var pair in ownedPools)
                    foreach (var ownedSFXObject in pair.Value.ownedAudioClipPool)
                        if (ownedSFXObject == aSource.gameObject)
                            return true;
                return false;
            }
            if (ownedPools.ContainsKey(aSource.clip))
                return ownedPools[aSource.clip].ownedAudioClipPool.Contains(aSource.gameObject);
            return false;
        }

        private bool IsOwnedSFXObject( GameObject obj )
        {
            return IsOwnedSFXObject(obj.GetComponent<AudioSource>());
        }

        private SFXGroup GetGroupForClipName( string clipName )
        {
            if (!clipsInGroups.ContainsKey(clipName))
                return null;
            return groups[clipsInGroups[clipName]];
        }

        private SFXGroup GetGroupByGroupName( string grpName )
        {
            if (!groups.ContainsKey(grpName))
                return null;
            return groups[grpName];
        }

        private IEnumerator XFade( float duration, AudioSource a1, AudioSource a2, UnityEvent runOnEndFunction )
        {
            var startTime = Time.realtimeSinceStartup;
            var endTime = startTime + duration;
            if (!a2.isPlaying) a2.Play();
            float a1StartVolume = a1.volume,
                a2StartVolume = a2.volume,
                deltaPercent = 0f,
                a1DeltaVolume = 0f,
                a2DeltaVolume = 0f,
                startMaxMusicVolume = a2.volume,
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
                    volumePercent = 1f;

                    if (endTime - Time.realtimeSinceStartup > duration)
                    {
                        startTime = Time.realtimeSinceStartup;
                        endTime = startTime + duration;
                    }
                    deltaPercent = (Time.realtimeSinceStartup - startTime) / duration;
                    a1DeltaVolume = deltaPercent * a1StartVolume;
                    a2DeltaVolume = deltaPercent * (startMaxMusicVolume - a2StartVolume);

                    a1.volume = Mathf.Clamp01((a1StartVolume - a1DeltaVolume) * volumePercent);
                    a2.volume = Mathf.Clamp01((a2DeltaVolume + a2StartVolume) * volumePercent);
                    yield return null;
                }
            a1.volume = 0f;
            a2.volume = a2StartVolume;
            a1.Stop();
            a1.timeSamples = 0;

            if (runOnEndFunction != null) runOnEndFunction.Invoke();
        }

        private IEnumerator XOut( float duration, AudioSource a1, UnityEvent runOnEndFunction )
        {
            var startTime = Time.realtimeSinceStartup;
            var endTime = startTime + duration;
            float maxVolume = a1.volume, deltaVolume = 0f, volumePercent = 1f;
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
                    volumePercent = 1f;

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

            if (runOnEndFunction != null) runOnEndFunction.Invoke();
        }

        /// <summary>
        ///     Crossin from a1 for duration.
        /// </summary>
        private IEnumerator XIn( float duration, AudioSource a1, UnityEvent runOnEndFunction )
        {
            var startTime = Time.realtimeSinceStartup;
            var endTime = startTime + duration;
            float a1StartVolume = 0f, startMaxMusicVolume = a1.volume, volumePercent = 1f;
            a1.volume = a1StartVolume;
            if (!a1.isPlaying) a1.Play();
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
                    volumePercent = 1f;

                    if (endTime - Time.realtimeSinceStartup > duration)
                    {
                        startTime = Time.realtimeSinceStartup;
                        endTime = startTime + duration;
                    }
                    deltaVolume = (Time.realtimeSinceStartup - startTime) / duration *
                                  (startMaxMusicVolume - a1StartVolume);

                    a1.volume = Mathf.Clamp01((deltaVolume + a1StartVolume) * volumePercent);
                    yield return null;
                }
            a1.volume = startMaxMusicVolume;

            if (runOnEndFunction != null) runOnEndFunction.Invoke();
        }

        private void Duck( SoundDuckingSetting duckingSetting, float duckVolume, float duckPitch, float unDuckedVolume,
            float unDuckedPitch, params AudioSource[] exceptions )
        {
            if (exceptions.Length <= 0 || duckingSetting == SoundDuckingSetting.DoNotDuck)
                return;

            if (!isDucking)
            {
                isDucking = true;
                preDuckVolume = maxVolume;
                preDuckVolumeMusic = maxMusicVolume;
                preDuckVolumeSFX = maxSFXVolume;
                preDuckPitch = GetPitch();
                preDuckPitchMusic = GetPitchMusic();
                preDuckPitchSFX = GetPitchSFX();
            }
            else
            {
                duckNumber++;
            }

            duckSource = exceptions[0];

            StartCoroutine(DuckTheTrack(duckingSetting, duckVolume, duckPitch, unDuckedVolume, unDuckedPitch,
                exceptions));
        }

        private IEnumerator DuckTheTrack( SoundDuckingSetting duckingSetting, float duckVolume, float duckPitch,
            float unDuckedVolume, float unDuckedPitch, AudioSource[] exceptions )
        {
            var thisNumber = duckNumber;

            /* crossfade in */
            var startTime = Time.realtimeSinceStartup;
            var endTime = startTime + duckStartSpeed;

            var startMaxVolume = 0f;
            var duckStartVolume = 0f;
            var startMaxPitch = 1f;
            var duckStartPitch = 1f;
            switch (duckingSetting)
            {
                case SoundDuckingSetting.DuckAll:
                    duckStartVolume = startMaxVolume = maxVolume;
                    duckStartPitch = startMaxPitch = GetPitch();
                    break;
                case SoundDuckingSetting.OnlyDuckMusic:
                    duckStartVolume = startMaxVolume = maxMusicVolume;
                    duckStartPitch = startMaxPitch = GetPitchMusic();
                    break;
                case SoundDuckingSetting.OnlyDuckSFX:
                    duckStartVolume = startMaxVolume = maxSFXVolume;
                    duckStartPitch = startMaxPitch = GetPitchSFX();
                    break;
                case SoundDuckingSetting.DoNotDuck:
                default:
                    yield break;
            }
            var unDuckStartVolume = 0f;
            var deltaPercent = 0f;
            var duckDeltaVolume = 0f;
            var unDuckDeltaVolume = 0f;
            var volumePercent = 1f;

            var unDuckStartPitch = 1f;
            var duckDeltaPitch = 0f;
            var unDuckDeltaPitch = 0f;

            while (Time.realtimeSinceStartup < endTime)
            {
                if (thisNumber != duckNumber)
                    yield break;

                var maxSoundVolume = 1f;
                switch (duckingSetting)
                {
                    case SoundDuckingSetting.DuckAll:
                        maxSoundVolume = maxVolume;
                        break;
                    case SoundDuckingSetting.OnlyDuckMusic:
                        maxSoundVolume = maxMusicVolume;
                        break;
                    case SoundDuckingSetting.OnlyDuckSFX:
                        maxSoundVolume = maxSFXVolume;
                        break;
                    case SoundDuckingSetting.DoNotDuck:
                    default:
                        yield break;
                }

                if (startMaxVolume == 0f)
                    volumePercent = 1f;
                else
                    volumePercent = maxSoundVolume / startMaxVolume;

                if (endTime - Time.realtimeSinceStartup > duckStartSpeed)
                {
                    startTime = Time.realtimeSinceStartup;
                    endTime = startTime + duckStartSpeed;
                }
                deltaPercent = (Time.realtimeSinceStartup - startTime) / duckStartSpeed;
                duckDeltaVolume = deltaPercent * (duckStartVolume - duckVolume);
                unDuckDeltaVolume = deltaPercent * (startMaxVolume - unDuckStartVolume);
                duckDeltaPitch = deltaPercent * (duckStartPitch - duckPitch);
                unDuckDeltaPitch = deltaPercent * (startMaxPitch - unDuckStartPitch);

                switch (duckingSetting)
                {
                    case SoundDuckingSetting.DuckAll:
                        SetVolume(Mathf.Clamp01((duckStartVolume - duckDeltaVolume) * volumePercent));
                        SetPitch(Mathf.Clamp01((duckStartPitch - duckDeltaPitch) * volumePercent));
                        foreach (var exception in exceptions)
                        {
                            SetVolumeSFX(Mathf.Clamp01((unDuckDeltaVolume + unDuckStartVolume) * volumePercent), true,
                                exception);
                            SetPitchSFX(Mathf.Clamp01((unDuckDeltaPitch + unDuckStartPitch) * volumePercent),
                                exception);
                        }
                        break;
                    case SoundDuckingSetting.OnlyDuckMusic:
                        SetVolumeMusic(Mathf.Clamp01((duckStartVolume - duckDeltaVolume) * volumePercent));
                        SetPitchMusic(Mathf.Clamp01((duckStartPitch - duckDeltaPitch) * volumePercent));
                        foreach (var exception in exceptions)
                        {
                            SetVolumeSFX(Mathf.Clamp01((unDuckDeltaVolume + unDuckStartVolume) * volumePercent), true,
                                exception);
                            SetPitchSFX(Mathf.Clamp01((unDuckDeltaPitch + unDuckStartPitch) * volumePercent),
                                exception);
                        }
                        break;
                    case SoundDuckingSetting.OnlyDuckSFX:
                        SetVolumeSFX(Mathf.Clamp01((duckStartVolume - duckDeltaVolume) * volumePercent));
                        SetPitchSFX(Mathf.Clamp01((duckStartPitch - duckDeltaPitch) * volumePercent));
                        foreach (var exception in exceptions)
                        {
                            SetVolumeSFX(Mathf.Clamp01((unDuckDeltaVolume + unDuckStartVolume) * volumePercent), true,
                                exception);
                            SetPitchSFX(Mathf.Clamp01((unDuckDeltaPitch + unDuckStartPitch) * volumePercent),
                                exception);
                        }
                        break;
                    case SoundDuckingSetting.DoNotDuck:
                    default:
                        yield break;
                }
                yield return null;
            }

            switch (duckingSetting)
            {
                case SoundDuckingSetting.DuckAll:
                    SetVolume(duckVolume);
                    SetPitch(duckPitch);
                    foreach (var exception in exceptions)
                    {
                        SetVolumeSFX(unDuckedVolume, true, exception);
                        SetPitchSFX(unDuckedPitch, exception);
                    }
                    break;
                case SoundDuckingSetting.OnlyDuckMusic:
                    SetVolumeMusic(duckVolume);
                    SetPitchMusic(duckPitch);
                    foreach (var exception in exceptions)
                    {
                        SetVolumeSFX(unDuckedVolume, true, exception);
                        SetPitchSFX(unDuckedPitch, exception);
                    }
                    break;
                case SoundDuckingSetting.OnlyDuckSFX:
                    SetVolumeSFX(duckVolume);
                    SetPitchSFX(duckPitch);
                    foreach (var exception in exceptions)
                    {
                        SetVolumeSFX(unDuckedVolume, true, exception);
                        SetPitchSFX(unDuckedPitch, exception);
                    }
                    break;
                case SoundDuckingSetting.DoNotDuck:
                default:
                    yield break;
            }
            /* end crossfade in */

            /* wait for clip to stop playing */
            while (exceptions[0].isPlaying)
            {
                if (thisNumber != duckNumber)
                    yield break;

                yield return null;
            }
            /* cross rest back in and call run on end */
            startTime = Time.realtimeSinceStartup;
            endTime = startTime + duckEndSpeed;

            switch (duckingSetting)
            {
                case SoundDuckingSetting.DuckAll:
                    duckStartVolume = maxVolume;
                    startMaxVolume = preDuckVolume;
                    duckStartPitch = GetPitch();
                    startMaxPitch = preDuckPitch;
                    break;
                case SoundDuckingSetting.OnlyDuckMusic:
                    duckStartVolume = maxMusicVolume;
                    startMaxVolume = preDuckVolumeMusic;
                    duckStartPitch = GetPitchMusic();
                    startMaxPitch = preDuckPitchMusic;
                    break;
                case SoundDuckingSetting.OnlyDuckSFX:
                    duckStartVolume = maxSFXVolume;
                    startMaxVolume = preDuckVolumeSFX;
                    duckStartPitch = GetPitchSFX();
                    startMaxPitch = preDuckPitchSFX;
                    break;
                case SoundDuckingSetting.DoNotDuck:
                default:
                    yield break;
            }

            volumePercent = 1f;
            deltaPercent = 0f;

            while (Time.realtimeSinceStartup < endTime)
            {
                if (thisNumber != duckNumber)
                    yield break;

                var maxSoundVolume = 1f;
                switch (duckingSetting)
                {
                    case SoundDuckingSetting.DuckAll:
                        maxSoundVolume = preDuckVolume;
                        break;
                    case SoundDuckingSetting.OnlyDuckMusic:
                        maxSoundVolume = preDuckVolumeMusic;
                        break;
                    case SoundDuckingSetting.OnlyDuckSFX:
                        maxSoundVolume = preDuckVolumeSFX;
                        break;
                    case SoundDuckingSetting.DoNotDuck:
                    default:
                        yield break;
                }

                if (startMaxVolume == 0f)
                    volumePercent = 1f;
                else
                    volumePercent = maxSoundVolume / startMaxVolume;

                if (endTime - Time.realtimeSinceStartup > duckEndSpeed)
                {
                    startTime = Time.realtimeSinceStartup;
                    endTime = startTime + duckEndSpeed;
                }
                deltaPercent = (Time.realtimeSinceStartup - startTime) / duckEndSpeed *
                               (startMaxVolume - duckStartVolume);

                switch (duckingSetting)
                {
                    case SoundDuckingSetting.DuckAll:
                        SetVolume(Mathf.Clamp01((deltaPercent + duckStartVolume) * volumePercent));
                        SetPitch(Mathf.Clamp01((deltaPercent + duckStartPitch) * volumePercent));
                        break;
                    case SoundDuckingSetting.OnlyDuckMusic:
                        SetVolumeMusic(Mathf.Clamp01((deltaPercent + duckStartVolume) * volumePercent));
                        SetPitchMusic(Mathf.Clamp01((deltaPercent + duckStartPitch) * volumePercent));
                        break;
                    case SoundDuckingSetting.OnlyDuckSFX:
                        SetVolumeSFX(Mathf.Clamp01((deltaPercent + duckStartVolume) * volumePercent));
                        SetPitchSFX(Mathf.Clamp01((deltaPercent + duckStartPitch) * volumePercent));
                        break;
                    case SoundDuckingSetting.DoNotDuck:
                    default:
                        yield break;
                }
                yield return null;
            }

            switch (duckingSetting)
            {
                case SoundDuckingSetting.DuckAll:
                    SetVolume(preDuckVolume);
                    SetPitch(preDuckPitch);
                    break;
                case SoundDuckingSetting.OnlyDuckMusic:
                    SetVolumeMusic(preDuckVolumeMusic);
                    SetPitchMusic(preDuckPitchMusic);
                    break;
                case SoundDuckingSetting.OnlyDuckSFX:
                    SetVolumeSFX(preDuckVolumeSFX);
                    SetPitchSFX(preDuckPitchSFX);
                    break;
                case SoundDuckingSetting.DoNotDuck:
                default:
                    yield break;
            }

            if (thisNumber != duckNumber)
                yield break;

            duckNumber = 0;

            if (isDucking)
                isDucking = false;
        }
    }
}