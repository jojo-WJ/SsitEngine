using System;
using System.Collections;
using System.Collections.Generic;
using Invector.CharacterController;
using UnityEngine;

namespace Invector
{
    [vClassHeader("HitDamage Particle",
        "Default hit Particle to instantiate every time you receive damage and Custom hit Particle to instantiate based on a custom AttackName from a Attack Animation State")]
    public class vHitDamageParticle : vMonoBehaviour
    {
        public List<vHitEffect> customHitEffects = new List<vHitEffect>();
        public GameObject defaultHitEffect;

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            var character = GetComponent<vCharacter>();
            if (character != null) character.onReceiveDamage.AddListener(OnReceiveDamage);
        }

        public void OnReceiveDamage( vDamage damage )
        {
            // instantiate the hitDamage particle - check if your character has a HitDamageParticle component
            var damageDirection = damage.hitPosition -
                                  new Vector3(transform.position.x, damage.hitPosition.y, transform.position.z);
            var hitrotation = damageDirection != Vector3.zero
                ? Quaternion.LookRotation(damageDirection)
                : transform.rotation;

            if (damage.damageValue > 0)
                TriggerHitParticle(new vHittEffectInfo(
                    new Vector3(transform.position.x, damage.hitPosition.y, transform.position.z), hitrotation,
                    damage.attackName, damage.receiver));
        }

        /// <summary>
        /// Raises the hit event.
        /// </summary>
        /// <param name="hitEffectInfo">Hit effect info.</param>
        private void TriggerHitParticle( vHittEffectInfo hitEffectInfo )
        {
            var hitEffect = customHitEffects.Find(effect => effect.hitName.Equals(hitEffectInfo.hitName));

            if (hitEffect != null)
            {
                if (hitEffect.hitPrefab != null)
                {
                    var prefab = Instantiate(hitEffect.hitPrefab, hitEffectInfo.position,
                        hitEffect.rotateToHitDirection
                            ? hitEffectInfo.rotation
                            : hitEffect.hitPrefab.transform.rotation);
                    if (hitEffect.attachInReceiver && hitEffectInfo.receiver)
                        prefab.transform.SetParent(hitEffectInfo.receiver);
                }
            }
            else if (defaultHitEffect != null)
            {
                Instantiate(defaultHitEffect, hitEffectInfo.position, hitEffectInfo.rotation);
            }
        }
    }

    public class vHittEffectInfo
    {
        public string hitName;
        public Vector3 position;
        public Transform receiver;
        public Quaternion rotation;

        public vHittEffectInfo( Vector3 position, Quaternion rotation, string hitName = "", Transform receiver = null )
        {
            this.receiver = receiver;
            this.position = position;
            this.rotation = rotation;
            this.hitName = hitName;
        }
    }

    [Serializable]
    public class vHitEffect
    {
        [Tooltip("Attach prefab in Damage Receiver transform")]
        public bool attachInReceiver;

        public string hitName = "";
        public GameObject hitPrefab;
        public bool rotateToHitDirection = true;
    }
}