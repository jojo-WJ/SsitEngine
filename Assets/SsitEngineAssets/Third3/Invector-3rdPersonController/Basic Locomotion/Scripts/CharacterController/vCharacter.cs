using System;
using Invector.EventSystems;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.CharacterController
{
    [Serializable]
    public class OnDead : UnityEvent<GameObject>
    {
    }

    [Serializable]
    public class OnReceiveDamage : UnityEvent<vDamage>
    {
    }

    [Serializable]
    public class OnActiveRagdoll : UnityEvent
    {
    }

    [Serializable]
    public class OnActionHandle : UnityEvent<Collider>
    {
    }

    [Serializable]
    [vClassHeader("vCharacter")]
    public abstract class vCharacter : vMonoBehaviour, vIDamageReceiver
    {
        protected bool isInit;

        public Transform GetTransform => transform;

        public virtual void TakeDamage( vDamage damage, bool hitReaction = true )
        {
            if (damage != null)
            {
                currentHealth -= damage.damageValue;
                if (damage.activeRagdoll) EnableRagdoll();
            }
        }

        public virtual void Init()
        {
            animator = GetComponent<Animator>();
            var actionListeners = GetComponents<vActionListener>();
            for (var i = 0; i < actionListeners.Length; i++)
            {
                if (actionListeners[i].actionEnter)
                    onActionEnter.AddListener(actionListeners[i].OnActionEnter);
                if (actionListeners[i].actionStay)
                    onActionStay.AddListener(actionListeners[i].OnActionStay);
                if (actionListeners[i].actionExit)
                    onActionExit.AddListener(actionListeners[i].OnActionExit);
            }
        }

        /// <summary>
        /// Change the currentHealth of Character
        /// </summary>
        /// <param name="value"></param>
        public virtual void ChangeHealth( int value )
        {
            currentHealth += value;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        /// <summary>
        /// Change the MaxHealth of Character
        /// </summary>
        /// <param name="value"></param>
        public virtual void ChangeMaxHealth( int value )
        {
            maxHealth += value;
            if (maxHealth < 0)
                maxHealth = 0;
        }

        /// <summary>
        /// Change the currentStamina of Character
        /// </summary>
        /// <param name="value"></param>
        public virtual void ChangeStamina( int value )
        {
            currentStamina += value;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        }

        /// <summary>
        /// Change the MaxStamina of Character
        /// </summary>
        /// <param name="value"></param>
        public virtual void ChangeMaxStamina( int value )
        {
            maxStamina += value;
            if (maxStamina < 0)
                maxStamina = 0;
        }

        public virtual void ResetRagdoll()
        {
        }

        public virtual void EnableRagdoll()
        {
        }

        protected virtual void OnTriggerEnter( Collider other )
        {
            onActionEnter.Invoke(other);
        }

        protected virtual void OnTriggerStay( Collider other )
        {
            onActionStay.Invoke(other);
        }

        protected virtual void OnTriggerExit( Collider other )
        {
            onActionExit.Invoke(other);
        }

        #region Character Variables

        [Header("Health & Stamina")] public float maxHealth = 100f;

        public float healthRecovery;
        public float maxStamina = 200f;
        public float staminaRecovery = 1.2f;

        [HideInInspector] public float currentStaminaRecoveryDelay;

        // [HideInInspector]
        public float healthRecoveryDelay;

        [HideInInspector] public float currentHealthRecoveryDelay;

        [HideInInspector] public float currentStamina;

        // [HideInInspector]
        public float currentHealth;

        protected bool recoveringStamina;
        protected bool canRecovery;

        [HideInInspector] public bool isDead { get; protected set; }

        public enum DeathBy
        {
            Animation,
            AnimationWithRagdoll,
            Ragdoll
        }

        public DeathBy deathBy = DeathBy.Animation;

        public bool removeComponentsAfterDie;

        // get the animator component of character
        [HideInInspector] public Animator animator { get; private set; }

        // know if the character is ragdolled or not
        // [HideInInspector]
        public bool ragdolled { get; set; }

        [Header("--- Character Events ---")] public OnReceiveDamage onReceiveDamage = new OnReceiveDamage();

        public OnDead onDead = new OnDead();
        public OnActiveRagdoll onActiveRagdoll = new OnActiveRagdoll();

        [Header("Check if Character is in Trigger with tag Action")] [HideInInspector]
        public OnActionHandle onActionEnter = new OnActionHandle();

        [HideInInspector] public OnActionHandle onActionStay = new OnActionHandle();

        [HideInInspector] public OnActionHandle onActionExit = new OnActionHandle();

        #endregion
    }
}