using UnityEngine;

public class vChangeCameraAngleTrigger : MonoBehaviour
{
    public Vector2 angle;
    public bool applyY, applyX;
    public vThirdPersonCamera tpCamera;

    private void Start()
    {
        tpCamera = FindObjectOfType<vThirdPersonCamera>();
    }

    private void OnTriggerEnter( Collider other )
    {
        if (other.gameObject.CompareTag("Player") && tpCamera)
        {
            if (applyX)
                tpCamera.lerpState.fixedAngle.x = angle.x;
            if (applyY)
                tpCamera.lerpState.fixedAngle.y = angle.y;
        }
    }
}