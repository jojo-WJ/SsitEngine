using Invector.CharacterController;
using UnityEngine;

public class vPunchingBag : MonoBehaviour
{
    public Rigidbody _rigidbody;
    public vCharacter character;
    public float forceMultipler = 0.5f;
    public SpringJoint joint;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        character = GetComponent<vCharacter>();
        character.onReceiveDamage.AddListener(TakeDamage);
    }

    public void TakeDamage( vDamage damage )
    {
        var point = damage.hitPosition;
        var relativePoint = transform.position;
        relativePoint.y = point.y;
        var forceForward = relativePoint - point;

        if (character != null && joint != null && character.currentHealth < 0)
        {
            joint.connectedBody = null;
            foreach (var mono in character.gameObject.GetComponentsInChildren<MonoBehaviour>())
                if (mono != this)
                    Destroy(mono);
        }

        if (_rigidbody != null)
            _rigidbody.AddForce(forceForward * (damage.damageValue * forceMultipler), ForceMode.Impulse);
    }
}