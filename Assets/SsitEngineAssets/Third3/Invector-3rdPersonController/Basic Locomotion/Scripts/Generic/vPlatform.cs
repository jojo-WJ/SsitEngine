using Invector.CharacterController;
using UnityEngine;

public class vPlatform : MonoBehaviour
{
    [HideInInspector] public bool canMove;

    private float currentTime;
    private float dist, currentDist;
    private int index;
    private bool invert;

    private Vector3 oldEuler;
    public Transform[] points;
    public float speed = 1f;
    public int startIndex;
    private Transform targetTransform;
    public float timeToStayInPoints = 2f;

    private void OnDrawGizmos()
    {
        if (points == null || points.Length == 0 || startIndex >= points.Length) return;
        var oldT = points[0];
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        if (!Application.isPlaying)
        {
            transform.position = points[startIndex].position;
            transform.eulerAngles = points[startIndex].eulerAngles;
        }

        foreach (var t in points)
            if (t != null && t != oldT)
            {
                Gizmos.DrawLine(oldT.position, t.position);
                oldT = t;
            }

        foreach (var t in points)
        {
            var rotationMatrix = Matrix4x4.TRS(t.position, t.rotation, transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }
    }

    private void Start()
    {
        if (points.Length == 0 || startIndex >= points.Length) return;
        if (points.Length < 2) return;
        transform.position = points[startIndex].position;
        transform.eulerAngles = points[startIndex].eulerAngles;
        oldEuler = transform.eulerAngles;
        var targetIndex = startIndex;

        if (startIndex + 1 < points.Length)
        {
            targetIndex++;
        }
        else if (startIndex - 1 > 0)
        {
            targetIndex--;
            invert = true;
        }

        dist = Vector3.Distance(transform.position, points[targetIndex].position);
        targetTransform = points[targetIndex];
        index = targetIndex;
        canMove = true;
    }


    private void FixedUpdate()
    {
        if (points.Length == 0 && !canMove) return;

        currentDist = Vector3.Distance(transform.position, targetTransform.position);
        if (currentTime <= 0)
        {
            var distFactor = Mathf.Clamp((100f - 100f * currentDist / dist) * 0.01f, 0, 1f);
            transform.position =
                Vector3.MoveTowards(transform.position, targetTransform.position, speed * Time.deltaTime);
            if (oldEuler != transform.eulerAngles)
                transform.eulerAngles = oldEuler + (targetTransform.eulerAngles - oldEuler) * distFactor;
        }
        else
        {
            currentTime -= Time.fixedDeltaTime;
        }

        if (currentDist < 0.02f)
        {
            if (!invert)
            {
                if (index + 1 < points.Length) index++;
                else invert = true;
            }
            else
            {
                if (index - 1 >= 0) index--;
                else invert = false;
            }
            dist = Vector3.Distance(targetTransform.position, points[index].position);
            targetTransform = points[index];
            oldEuler = transform.eulerAngles;
            currentTime = timeToStayInPoints;
        }
    }

    private void OnTriggerEnter( Collider other )
    {
        if (other.transform.parent != transform && other.transform.tag == "Player" &&
            other.GetComponent<vCharacter>() != null) other.transform.parent = transform;
    }

    private void OnTriggerExit( Collider other )
    {
        if (other.transform.parent == transform && other.transform.tag == "Player") other.transform.parent = null;
    }
}