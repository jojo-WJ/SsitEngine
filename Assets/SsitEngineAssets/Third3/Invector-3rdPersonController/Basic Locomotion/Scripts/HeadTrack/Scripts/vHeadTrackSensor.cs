using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class vHeadTrackSensor : MonoBehaviour
{
    [HideInInspector] public vHeadTrack headTrack;

    public SphereCollider sphere;

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && sphere && headTrack) sphere.radius = headTrack.distanceToDetect;
    }

    private void Start()
    {
        var _rigidB = GetComponent<Rigidbody>();
        sphere = GetComponent<SphereCollider>();
        sphere.isTrigger = true;
        _rigidB.useGravity = false;
        _rigidB.isKinematic = true;
        _rigidB.constraints = RigidbodyConstraints.FreezeAll;
        if (headTrack) sphere.radius = headTrack.distanceToDetect;
    }

    private void OnTriggerEnter( Collider other )
    {
        if (headTrack != null) headTrack.OnDetect(other);
    }

    private void OnTriggerExit( Collider other )
    {
        if (headTrack != null) headTrack.OnLost(other);
    }
}