using UnityEngine;

namespace Invector.CharacterController.v2_5D
{
    [vClassHeader("2.5D Controller")]
    public class v2_5DController : vThirdPersonController
    {
        private float inputHorizontal;
        private float inputVertical;

        protected override void StrafeLimitSpeed( float value )
        {
            var limitInput = isSprinting ? 1.5f : 1f;
            var _input = transform.InverseTransformDirection(Camera.main.transform.right * input.x * limitInput);
            speed = Mathf.Clamp(_input.z, -limitInput, limitInput);
            direction = 0;
            var newInput = new Vector2(speed, direction);
            strafeMagnitude = Mathf.Clamp(newInput.magnitude, 0, value * limitInput);
        }

        protected override void StrafeVelocity( float velocity )
        {
            var v = transform.forward * speed * velocity;
            v.y = _rigidbody.velocity.y;
            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, v, 20f * Time.deltaTime);
        }
    }
}