using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class vHandler
{
    public List<Transform> customHandlers;
    public Transform defaultHandler;

    public vHandler()
    {
        customHandlers = new List<Transform>();
    }
}