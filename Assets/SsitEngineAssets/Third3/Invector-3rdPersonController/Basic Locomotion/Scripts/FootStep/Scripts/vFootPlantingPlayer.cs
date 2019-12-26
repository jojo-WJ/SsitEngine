using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class vFootPlantingPlayer : MonoBehaviour
{
    public List<vAudioSurface> customSurfaces;

    // The different surfaces and their sounds.
    public vAudioSurface defaultSurface;

    public void PlayFootFallSound( FootStepObject footStepObject )
    {
        for (var i = 0; i < customSurfaces.Count; i++)
            if (customSurfaces[i] != null && ContainsTexture(footStepObject.name, customSurfaces[i]))
            {
                customSurfaces[i].PlayRandomClip(footStepObject);
                return;
            }
        if (defaultSurface != null)
            defaultSurface.PlayRandomClip(footStepObject);
    }

    // check if AudioSurface Contains texture in TextureName List
    private bool ContainsTexture( string name, vAudioSurface surface )
    {
        for (var i = 0; i < surface.TextureOrMaterialNames.Count; i++)
            if (name.Contains(surface.TextureOrMaterialNames[i]))
                return true;

        return false;
    }
}