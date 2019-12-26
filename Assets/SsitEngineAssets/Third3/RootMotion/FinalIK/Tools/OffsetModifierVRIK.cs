using System.Collections;
using UnityEngine;

namespace RootMotion.FinalIK
{
    /// <summary>
    /// Base class for all FBBIK effector positionOffset modifiers. Works with animatePhysics, safe delegates, offset limits.
    /// </summary>
    public abstract class OffsetModifierVRIK : MonoBehaviour
    {
        [Tooltip("Reference to the VRIK component")]
        public VRIK ik;

        private float lastTime;

        [Tooltip("The master weight")] public float weight = 1f;

        // not using Time.deltaTime or Time.fixedDeltaTime here, because we don't know if animatePhysics is true or not on the character, so we have to keep track of time ourselves.
        protected float deltaTime => Time.time - lastTime;
        protected abstract void OnModifyOffset();

        protected virtual void Start()
        {
            StartCoroutine(Initiate());
        }

        private IEnumerator Initiate()
        {
            while (ik == null) yield return null;

            // You can use just LateUpdate, but note that it doesn't work when you have animatePhysics turned on for the character.
            ik.solver.OnPreUpdate += ModifyOffset;
            lastTime = Time.time;
        }

        // The main function that checks for all conditions and calls OnModifyOffset if they are met
        private void ModifyOffset()
        {
            if (!enabled) return;
            if (weight <= 0f) return;
            if (deltaTime <= 0f) return;
            if (ik == null) return;
            weight = Mathf.Clamp(weight, 0f, 1f);

            OnModifyOffset();

            lastTime = Time.time;
        }

        // Remove the delegate when destroyed
        protected virtual void OnDestroy()
        {
            if (ik != null) ik.solver.OnPreUpdate -= ModifyOffset;
        }
    }
}