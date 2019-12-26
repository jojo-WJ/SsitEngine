using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class vAudioSurfaceControl : MonoBehaviour
{
    private bool isWorking;
    private AudioSource source;

    public AudioMixerGroup outputAudioMixerGroup
    {
        set
        {
            if (!source) source = GetComponent<AudioSource>();
            source.outputAudioMixerGroup = value;
        }
    }

    /// <summary>
    /// Play One Shot in Audio Source Component
    /// </summary>
    /// <param name="clip"></param>
    public void PlayOneShot( AudioClip clip )
    {
        if (!source) source = GetComponent<AudioSource>();
        source.PlayOneShot(clip);
        isWorking = true;
    }

    private void Update()
    {
        if (isWorking && !source.isPlaying) Destroy(gameObject);
    }
}