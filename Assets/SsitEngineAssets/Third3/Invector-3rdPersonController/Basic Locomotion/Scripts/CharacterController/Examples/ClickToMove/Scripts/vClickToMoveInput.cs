using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.CharacterController.ClickToMove
{
    [vClassHeader("Click To Move Input")]
    public class vClickToMoveInput : vThirdPersonInput
    {
        [Header("Click To Move Properties")] public LayerMask clickMoveLayer = 1 << 0;

        [HideInInspector] public Vector3 cursorPoint;

        public UnityEvent onDisableCursor;
        public vOnEnableCursor onEnableCursor = new vOnEnableCursor();

        protected override IEnumerator CharacterInit()
        {
            yield return StartCoroutine(base.CharacterInit());
            cursorPoint = transform.position;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            MoveToPoint();
        }

        protected override void MoveCharacter()
        {
            cc.rotateByWorld = true;
            ClickAndMove();
        }

        protected virtual void ClickAndMove()
        {
            RaycastHit hit;

            if (Input.GetMouseButton(0))
                if (Physics.Raycast(tpCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out hit,
                    Mathf.Infinity, clickMoveLayer))
                {
                    if (onEnableCursor != null) onEnableCursor.Invoke(hit.point);
                    cursorPoint = hit.point;
                }
        }

        protected void MoveToPoint()
        {
            if (!NearPoint(cursorPoint, transform.position))
            {
                MoveCharacter(cursorPoint);
            }
            else
            {
                if (onDisableCursor != null)
                    onDisableCursor.Invoke();

                cc.input = Vector2.Lerp(cc.input, Vector3.zero, 20 * Time.deltaTime);
            }
        }

        public void SetTargetPosition( Vector3 value )
        {
            cursorPoint = value;
            var dir = (value - transform.position).normalized;
            cc.input = new Vector2(dir.x, dir.z);
        }

        public void ClearTarget()
        {
            cc.input = Vector2.zero;
        }

        protected virtual bool NearPoint( Vector3 a, Vector3 b )
        {
            var _a = new Vector3(a.x, transform.position.y, a.z);
            var _b = new Vector3(b.x, transform.position.y, b.z);
            return Vector3.Distance(_a, _b) <= 0.5f;
        }

        // isometric cursor
        [Serializable]
        public class vOnEnableCursor : UnityEvent<Vector3>
        {
        }
    }
}