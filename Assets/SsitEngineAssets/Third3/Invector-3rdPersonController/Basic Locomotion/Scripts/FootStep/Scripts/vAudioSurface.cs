using System.Collections.Generic;
using Invector;
using UnityEngine;
using UnityEngine.Audio;

public class vAudioSurface : ScriptableObject
{
    public List<AudioClip> audioClips; // The different clips that can be played on this surface.    
    public AudioMixerGroup audioMixerGroup; // The AudioSource that will play the clips.   
    public AudioSource audioSource;
    public GameObject particleObject;

    private vFisherYatesRandom randomSource = new vFisherYatesRandom(); // For randomly reordering clips.   

    [vHideInInspector("useStepMark")] public LayerMask stepLayer;

    [vHideInInspector("useStepMark")] public GameObject stepMark;

    public List<string> TextureOrMaterialNames; // The tag on the surfaces that play these sounds.

    [vHideInInspector("useStepMark")] public float timeToDestroy = 5f;

    public bool useStepMark;

    public vAudioSurface()
    {
        audioClips = new List<AudioClip>();
        TextureOrMaterialNames = new List<string>();
    }

    public void PlayRandomClip( FootStepObject footStepObject )
    {
        // if there are no clips to play return.
        if (audioClips == null || audioClips.Count == 0)
            return;

        // initialize variable if not already started
        if (randomSource == null)
            randomSource = new vFisherYatesRandom();

        // find a random clip and play it.
        GameObject audioObject = null;
        if (audioSource != null)
        {
            audioObject = Instantiate(audioSource.gameObject, footStepObject.sender.position, Quaternion.identity);
        }
        else
        {
            audioObject = new GameObject("audioObject");
            audioObject.transform.position = footStepObject.sender.position;
        }

        var source = audioObject.AddComponent<vAudioSurfaceControl>();
        if (audioMixerGroup != null) source.outputAudioMixerGroup = audioMixerGroup;
        var index = randomSource.Next(audioClips.Count);
        if (particleObject && footStepObject.ground && stepLayer.ContainsLayer(footStepObject.ground.gameObject.layer))
            Instantiate(particleObject, footStepObject.sender.position, footStepObject.sender.rotation);
        if (useStepMark)
            StepMark(footStepObject);

        source.PlayOneShot(audioClips[index]);
    }

    private void StepMark( FootStepObject footStep )
    {
        RaycastHit hit;
        if (Physics.Raycast(footStep.sender.transform.position + new Vector3(0, 0.1f, 0), -footStep.sender.up, out hit,
            1f, stepLayer))
            if (stepMark)
            {
                var angle = Quaternion.FromToRotation(footStep.sender.up, hit.normal);
                var step = Instantiate(stepMark, hit.point, angle * footStep.sender.rotation);
                step.transform.SetParent(footStep.ground.transform);
                Destroy(step, timeToDestroy);
            }
    }
}