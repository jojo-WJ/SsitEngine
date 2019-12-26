using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector
{
    public class vCullingFadeControl : MonoBehaviour
    {
        public float distanceToEndFade = 0.4f;

        public float distanceToStartFade = 0.55f;

        // [HideInInspector]
        public List<FadeMaterials> fadeMeshRenderers;

        // [HideInInspector]
        public List<FadeMaterials> fadeSkinnedMeshRenderes;

        public Vector3 offset = new Vector3(0, 1.3f, 0);

        // [HideInInspector]
        public bool usingTransp;

        public Transform targetObject
        {
            get
            {
                var cT = transform;
                return cT;
            }
        }

        public Transform cameraTransform
        {
            get
            {
                var cT = transform;
                if (Camera.main != null)
                    cT = Camera.main.transform;
                if (cT == transform)
                {
                    Debug.LogWarning("Invector : Missing MainCamera");
                    enabled = false;
                }
                return cT;
            }
        }

        private void Start()
        {
            Init();
        }

        public void Init()
        {
            foreach (var fd in fadeMeshRenderers)
            {
                fd.originalAlpha = new float[fd.originalMaterials.Length];
                for (var i = 0; i < fd.originalMaterials.Length; i++)
                    if (fd.fadeMaterials[i] == null)
                        try
                        {
                            fd.originalAlpha[i] = fd.originalMaterials[i].color.a;
                            fd.fadeMaterials[i] = fd.originalMaterials[i];
                        }
                        catch
                        {
                        }
                    else
                        try
                        {
                            fd.originalAlpha[i] = fd.fadeMaterials[i].color.a;
                        }
                        catch
                        {
                        }
            }

            foreach (var fd in fadeSkinnedMeshRenderes)
            {
                fd.originalAlpha = new float[fd.originalMaterials.Length];
                for (var i = 0; i < fd.originalMaterials.Length; i++)
                    if (fd.fadeMaterials[i] == null)
                        try
                        {
                            fd.originalAlpha[i] = fd.originalMaterials[i].color.a;
                            fd.fadeMaterials[i] = fd.originalMaterials[i];
                        }
                        catch
                        {
                        }
                    else
                        try
                        {
                            fd.originalAlpha[i] = fd.fadeMaterials[i].color.a;
                        }
                        catch
                        {
                        }
            }
        }

        private void LateUpdate()
        {
            UpdateEffect();

            if (usingTransp)
                ChangeAlphaFromDistance();
        }

        /// <summary>
        /// Update the effect to check if use fade or not
        /// </summary>
        private void UpdateEffect()
        {
            var currentDist = Vector3.Distance(cameraTransform.position, targetObject.position + offset);
            if (currentDist < distanceToStartFade && !usingTransp)
            {
                usingTransp = true;
                ChangeMaterialsToFade();
            }
            else if (usingTransp && currentDist > distanceToStartFade)
            {
                usingTransp = false;
                ChangeMaterialsToOriginal();
            }
        }

        /// <summary>
        /// Change the Renderer Materials to Original Materials
        /// </summary>
        private void ChangeMaterialsToOriginal()
        {
            foreach (var fd in fadeMeshRenderers)
                try
                {
                    fd.renderer.sharedMaterials = fd.originalMaterials;
                }
                catch
                {
                }


            foreach (var fd in fadeSkinnedMeshRenderes)
                try
                {
                    fd.renderer.sharedMaterials = fd.originalMaterials;
                }
                catch
                {
                }
        }

        /// <summary>
        /// Chenge the Renderer Materials to Fade Material
        /// </summary>
        private void ChangeMaterialsToFade()
        {
            foreach (var fd in fadeMeshRenderers)
                try
                {
                    fd.renderer.sharedMaterials = fd.fadeMaterials;
                }
                catch
                {
                }


            foreach (var fd in fadeSkinnedMeshRenderes)
                try
                {
                    fd.renderer.sharedMaterials = fd.fadeMaterials;
                }
                catch
                {
                }
        }

        /// <summary>
        /// Change the Alpha Color material From distance
        /// </summary>
        public void ChangeAlphaFromDistance()
        {
            var currentDist = Vector3.Distance(cameraTransform.position, targetObject.position + offset);
            // Mesh Renderer
            for (var i = 0; i < fadeMeshRenderers.Count; i++)
            for (var m = 0; m < fadeMeshRenderers[i].fadeMaterials.Length; m++)
                try
                {
                    var multpler = fadeMeshRenderers[i].originalAlpha[m] / (distanceToStartFade - distanceToEndFade);
                    var color = fadeMeshRenderers[i].renderer.sharedMaterials[m].color;
                    var factor = distanceToStartFade - distanceToEndFade - (distanceToStartFade - currentDist);
                    color.a = multpler * factor;
                    color.a = Mathf.Clamp(color.a, 0f, fadeMeshRenderers[i].originalAlpha[m]);
                    fadeMeshRenderers[i].renderer.materials[m].color = color;
                }
                catch
                {
                }
            //Skinned Mesh Renderer
            for (var i = 0; i < fadeSkinnedMeshRenderes.Count; i++)
            for (var m = 0; m < fadeSkinnedMeshRenderes[i].fadeMaterials.Length; m++)
                try
                {
                    var multpler = fadeSkinnedMeshRenderes[i].originalAlpha[m] /
                                   (distanceToStartFade - distanceToEndFade);
                    var color = fadeSkinnedMeshRenderes[i].renderer.sharedMaterials[m].color;
                    var factor = distanceToStartFade - distanceToEndFade - (distanceToStartFade - currentDist);
                    color.a = multpler * factor;
                    color.a = Mathf.Clamp(color.a, 0f, fadeSkinnedMeshRenderes[i].originalAlpha[m]);
                    fadeSkinnedMeshRenderes[i].renderer.materials[m].color = color;
                }
                catch
                {
                }
        }
    }

    [Serializable]
    public class FadeMaterials
    {
        public Material[] fadeMaterials;
        public float[] originalAlpha;
        public Material[] originalMaterials;
        public Renderer renderer;
    }
}