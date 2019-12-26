using UnityEngine;

public class vRotateObject : MonoBehaviour
{
    public Vector3 rotationSpeed;

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
    }
}