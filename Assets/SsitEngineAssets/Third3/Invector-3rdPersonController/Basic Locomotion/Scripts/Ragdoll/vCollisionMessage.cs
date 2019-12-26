using Invector.CharacterController;
using Invector.EventSystems;
using UnityEngine;

public class vCollisionMessage : MonoBehaviour, vIDamageReceiver
{
    private bool inAddDamage;

    [HideInInspector] public vRagdoll ragdoll;

    public void TakeDamage( vDamage damage, bool hitReaction = true )
    {
        if (!ragdoll) return;
        if (!ragdoll.iChar.isDead)
        {
            inAddDamage = true;
            ragdoll.ApplyDamage(damage);
            Invoke("ResetAddDamage", 0.1f);
        }
    }

    private void Start()
    {
        ragdoll = GetComponentInParent<vRagdoll>();
    }

    private void OnCollisionEnter( Collision collision )
    {
        if (collision != null)
            if (ragdoll)
            {
                ragdoll.OnRagdollCollisionEnter(new vRagdollCollision(gameObject, collision));
                if (!inAddDamage)
                {
                    var impactforce = collision.relativeVelocity.x + collision.relativeVelocity.y +
                                      collision.relativeVelocity.z;
                    if (impactforce > 10 || impactforce < -10)
                    {
                        inAddDamage = true;
                        var damage = new vDamage((int) Mathf.Abs(impactforce) - 10);
                        damage.ignoreDefense = true;
                        damage.sender = collision.transform;
                        damage.hitPosition = collision.contacts[0].point;
                        ragdoll.ApplyDamage(damage);
                        Invoke("ResetAddDamage", 0.1f);
                    }
                }
            }
    }

    private void ResetAddDamage()
    {
        inAddDamage = false;
    }
}