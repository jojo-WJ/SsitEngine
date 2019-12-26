using UnityEngine;
using UnityEngine.Events;

namespace Invector.CharacterController
{
    public class vOnDeadTrigger : MonoBehaviour
    {
        public UnityEvent OnDead;

        private void Start()
        {
            var character = GetComponent<vCharacter>();
            if (character)
                character.onDead.AddListener(OnDeadHandle);
        }

        public void OnDeadHandle( GameObject target )
        {
            OnDead.Invoke();
        }
    }
}