using UnityEngine;

namespace Invector.CharacterController.ClickToMove
{
    public class vClickToMoveCursor : MonoBehaviour
    {
        private Vector3 _scale, currentScale;
        public GameObject cursorObject;
        private bool enableCursor;
        public float scale, speed;
        private float time;
        public vClickToMoveInput tpInput;

        private void Start()
        {
            if (!tpInput) Destroy(gameObject);
            tpInput.onEnableCursor.AddListener(Enable);
            tpInput.onDisableCursor.AddListener(Disable);
            _scale = cursorObject.transform.localScale;
        }

        private void Update()
        {
            if (enableCursor)
            {
                time += speed * Time.deltaTime;
                currentScale.x = Mathf.PingPong(time, _scale.x + scale);
                currentScale.x = Mathf.Clamp(currentScale.x, _scale.x, _scale.x + scale);
                currentScale.y = Mathf.PingPong(time, _scale.y + scale);
                currentScale.y = Mathf.Clamp(currentScale.y, _scale.y, _scale.y + scale);
                currentScale.z = Mathf.PingPong(time, _scale.z + scale);
                currentScale.z = Mathf.Clamp(currentScale.z, _scale.z, _scale.z + scale);
                cursorObject.transform.localScale = currentScale;
            }
        }

        public bool Near( Vector3 pos, float dst )
        {
            var a = new Vector3(pos.x, 0, pos.z);
            var b = new Vector3(transform.position.x, 0, transform.position.z);
            return Vector3.Distance(a, b) < dst;
        }

        public void Enable( Vector3 position )
        {
            transform.position = position;
            cursorObject.SetActive(true);
            enableCursor = true;
        }

        public void Disable()
        {
            cursorObject.SetActive(false);
            enableCursor = false;
        }
    }
}