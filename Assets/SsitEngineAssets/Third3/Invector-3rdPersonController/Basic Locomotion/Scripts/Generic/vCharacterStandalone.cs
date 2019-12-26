using Invector.CharacterController;
using UnityEngine;

[vClassHeader("Character Standalone")]
public class vCharacterStandalone : vCharacter
{
    /// <summary>
    /// 
    /// vCharacter Example - You can assign this script into non-Invector Third Person Characters to still use the AI and apply damage
    /// 
    /// </summary>
    [HideInInspector] public v_SpriteHealth healthSlider;

    protected virtual void Start()
    {
        // health info
        isDead = false;
        currentHealth = maxHealth;
        currentHealthRecoveryDelay = healthRecoveryDelay;
        currentStamina = maxStamina;
        // health slider hud - prefab located into Prefabs/AI/enemyHealthUI
        healthSlider = GetComponentInChildren<v_SpriteHealth>();
        Init();
    }


    /// <summary>
    /// TAKE DAMAGE - you can override the take damage method from the vCharacter and add your own calls 
    /// </summary>
    /// <param name="damage"> damage to apply </param>
    public override void TakeDamage( vDamage damage, bool hitReaction )
    {
        // don't apply damage if the character is rolling, you can add more conditions here
        if (isDead)
            return;

        // reduce the current health by the damage amount.
        currentHealth -= damage.damageValue;
        currentHealthRecoveryDelay = healthRecoveryDelay;
        // update the HUD display
        if (healthSlider != null) healthSlider.Damage(damage.damageValue);
        // apply vibration on the gamepad                    
        // vInput.instance.GamepadVibration(0.25f);
        // call OnReceiveDamage Event
        onReceiveDamage.Invoke(damage);
    }
}