using System.Collections.Generic;
using UnityEngine;

public class vSpikeControl : MonoBehaviour
{
    [HideInInspector] public List<Transform> attachColliders;

    private void Start()
    {
        attachColliders = new List<Transform>();
        var objs = GetComponentsInChildren<vSpike>();
        foreach (var obj in objs)
            obj.control = this;
    }
}