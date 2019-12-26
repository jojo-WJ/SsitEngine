using System.Collections;
using UnityEngine;

public class vDestroyGameObject : MonoBehaviour
{
    public float delay;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}