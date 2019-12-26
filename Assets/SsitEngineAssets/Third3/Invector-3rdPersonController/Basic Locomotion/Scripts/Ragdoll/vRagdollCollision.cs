using UnityEngine;

public class vRagdollCollision
{
    /// <summary>
    /// Create a New collision info seu trouxa
    /// </summary>
    /// <param name="sender">current gameobjet</param>
    /// <param name="collision">current collision info</param>
    public vRagdollCollision( GameObject sender, Collision collision )
    {
        Sender = sender;
        Collision = collision;
        ImpactForce = collision.relativeVelocity.magnitude;
    }

    /// <summary>
    /// Gameobjet receiver of collision info
    /// </summary>
    public GameObject Sender { get; }

    /// <summary>
    /// Collision info 
    /// </summary>
    public Collision Collision { get; }

    /// <summary>
    /// Magnitude of relative linear velocity of the two colliding objects
    /// </summary>
    public float ImpactForce { get; }
}