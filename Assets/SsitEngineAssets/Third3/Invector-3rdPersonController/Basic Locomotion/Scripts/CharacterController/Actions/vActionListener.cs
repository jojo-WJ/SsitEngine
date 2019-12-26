using System;
using Invector;
using UnityEngine;
using UnityEngine.Events;

public abstract class vActionListener : vMonoBehaviour
{
    public bool actionEnter;
    public bool actionExit;
    public bool actionStay;
    public vOnActionHandle OnDoAction = new vOnActionHandle();

    public virtual void OnActionEnter( Collider other )
    {
    }

    public virtual void OnActionStay( Collider other )
    {
    }

    public virtual void OnActionExit( Collider other )
    {
    }

    [Serializable]
    public class vOnActionHandle : UnityEvent<vTriggerGenericAction>
    {
    }
}