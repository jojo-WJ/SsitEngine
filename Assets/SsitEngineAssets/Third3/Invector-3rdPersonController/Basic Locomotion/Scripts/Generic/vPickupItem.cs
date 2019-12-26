using UnityEngine;

public class vPickupItem : MonoBehaviour
{
    public AudioClip _audioClip;
    private AudioSource _audioSource;
    public GameObject _particle;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter( Collider other )
    {
        if (other.tag.Equals("Player") && !_audioSource.isPlaying)
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
                r.enabled = false;

            _audioSource.PlayOneShot(_audioClip);
            Destroy(gameObject, _audioClip.length);
        }
    }
}