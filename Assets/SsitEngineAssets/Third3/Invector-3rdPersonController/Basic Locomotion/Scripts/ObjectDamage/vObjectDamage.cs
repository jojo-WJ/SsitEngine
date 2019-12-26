using System;
using System.Collections.Generic;
using Invector.EventSystems;
using UnityEngine;

public class vObjectDamage : MonoBehaviour
{
    [HideInInspector] [Tooltip("Check to use the damage Frequence")]
    public bool continuousDamage;

    private float currentTime;
    public vDamage damage;

    [HideInInspector] [Tooltip("Apply damage to each end of the frequency in seconds ")]
    public float damageFrequency = 0.5f;

    private List<Collider> disabledTarget;

    [Tooltip("List of tags that can be hit")]
    public List<string> tags;

    private List<Collider> targets;

    [HideInInspector] public bool useCollision;

    protected virtual void Start()
    {
        targets = new List<Collider>();
        disabledTarget = new List<Collider>();
    }

    protected virtual void Update()
    {
        if (continuousDamage && targets != null && targets.Count > 0)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
            }
            else
            {
                currentTime = damageFrequency;
                foreach (var collider in targets)
                    if (collider != null)
                    {
                        if (collider.enabled)
                            ApplyDamage(collider.transform, transform.position); // apply damage to enabled collider
                        else
                            disabledTarget.Add(collider); // add disabled collider to list of disabled
                    }
                //remove all disabled colliders of target list
                if (disabledTarget.Count > 0)
                    for (var i = disabledTarget.Count; i >= 0; i--)
                    {
                        if (disabledTarget.Count == 0) break;
                        try
                        {
                            if (targets.Contains(disabledTarget[i]))
                                targets.Remove(disabledTarget[i]);
                        }
                        catch
                        {
                            break;
                        }
                    }

                if (disabledTarget.Count > 0) disabledTarget.Clear();
            }
        }
    }

    protected virtual void OnCollisionEnter( Collision hit )
    {
        if (!useCollision || continuousDamage) return;

        if (tags.Contains(hit.gameObject.tag))
            ApplyDamage(hit.transform, hit.contacts[0].point);
    }

    protected virtual void OnTriggerEnter( Collider hit )
    {
        if (useCollision) return;
        if (continuousDamage && tags.Contains(hit.transform.tag) && !targets.Contains(hit))
            targets.Add(hit);

        else if (tags.Contains(hit.gameObject.tag))
            ApplyDamage(hit.transform, transform.position);
    }

    protected virtual void OnTriggerExit( Collider hit )
    {
        if (useCollision && !continuousDamage) return;

        if (tags.Contains(hit.gameObject.tag) && targets.Contains(hit))
            targets.Remove(hit);
    }

    protected virtual void ApplyDamage( Transform target, Vector3 hitPoint )
    {
        damage.sender = transform;
        damage.hitPosition = hitPoint;
        target.gameObject.ApplyDamage(damage);
    }
}


[Serializable]
public class vDamage
{
    [Tooltip("Activated Ragdoll when hit the Character")]
    public bool activeRagdoll;

    public string attackName;

    [Tooltip("Apply damage to the Character Health")]
    public int damageValue = 15;

    [HideInInspector] public Vector3 hitPosition;

    [Tooltip("Apply damage even if the Character is blocking")]
    public bool ignoreDefense;

    [HideInInspector] public int reaction_id;

    [HideInInspector] public Transform receiver;

    [HideInInspector] public int recoil_id;

    [HideInInspector] public Transform sender;

    [Tooltip("How much stamina the target will lost when blocking this attack")]
    public float staminaBlockCost = 5;

    [Tooltip("How much time the stamina of the target will wait to recovery")]
    public float staminaRecoveryDelay = 1;

    public vDamage( int value )
    {
        damageValue = value;
    }

    public vDamage( vDamage damage )
    {
        damageValue = damage.damageValue;
        staminaBlockCost = damage.staminaBlockCost;
        staminaRecoveryDelay = damage.staminaRecoveryDelay;
        ignoreDefense = damage.ignoreDefense;
        activeRagdoll = damage.activeRagdoll;
        sender = damage.sender;
        receiver = damage.receiver;
        recoil_id = damage.recoil_id;
        reaction_id = damage.reaction_id;
        attackName = damage.attackName;
        hitPosition = damage.hitPosition;
    }

    /// <summary>
    /// Calc damage Resuction percentage
    /// </summary>
    /// <param name="damageReduction"></param>
    public void ReduceDamage( float damageReduction )
    {
        var result = (int) (damageValue - damageValue * damageReduction / 100);
        damageValue = result;
    }
}