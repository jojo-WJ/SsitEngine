using System;
using System.Collections.Generic;
using Invector;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
[vClassHeader("vSimpleTrigger")]
public class vSimpleTrigger : vMonoBehaviour
{
    [HideInInspector] public bool inCollision;

    public LayerMask layerToDetect = 0 << 1;
    public vTriggerEvent onTriggerEnter;
    public vTriggerEvent onTriggerExit;
    private Collider other;
    public List<string> tagsToDetect = new List<string> {"Player"};
    private bool triggerStay;
    public bool useFilter = true;

    private void OnDrawGizmos()
    {
        var red = new Color(1, 0, 0, 0.2f);
        var green = new Color(0, 1, 0, 0.2f);
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.color = inCollision && Application.isPlaying ? red : green;
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
    }

    private void Start()
    {
        inCollision = false;
        gameObject.GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter( Collider other )
    {
        if (!useFilter || tagsToDetect.Contains(other.gameObject.tag) &&
            IsInLayerMask(other.gameObject, layerToDetect) && this.other == null)
        {
            inCollision = true;
            this.other = other;
            onTriggerEnter.Invoke(other);
        }
    }

    private void OnTriggerExit( Collider other )
    {
        if (!useFilter || tagsToDetect.Contains(other.gameObject.tag) &&
            IsInLayerMask(other.gameObject, layerToDetect) &&
            (this.other == null || this.other.gameObject == other.gameObject))
        {
            inCollision = false;
            onTriggerExit.Invoke(other);
            this.other = null;
        }
    }

    private bool IsInLayerMask( GameObject obj, LayerMask mask )
    {
        return (mask.value & (1 << obj.layer)) > 0;
    }

    [Serializable]
    public class vTriggerEvent : UnityEvent<Collider>
    {
    }
}