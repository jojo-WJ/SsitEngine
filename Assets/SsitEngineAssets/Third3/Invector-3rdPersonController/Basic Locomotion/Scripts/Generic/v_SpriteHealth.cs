using System.Collections;
using Invector.CharacterController;
using UnityEngine;
using UnityEngine.UI;

public class v_SpriteHealth : MonoBehaviour
{
    private float currentSmoothDamage;
    private float damage;
    public Text damageCounter;
    public float damageCounterTimer = 1.5f;
    public Slider damageDelay;
    public Slider healthSlider;
    public vCharacter iChar;

    private bool inDelay;
    public float smoothDamageDelay;

    private void Start()
    {
        iChar = transform.GetComponentInParent<vCharacter>();
        if (iChar == null)
        {
            Debug.LogWarning("The character must have a ICharacter Interface");
            Destroy(gameObject);
        }
        healthSlider.maxValue = iChar.maxHealth;
        healthSlider.value = healthSlider.maxValue;
        damageDelay.maxValue = iChar.maxHealth;
        damageDelay.value = healthSlider.maxValue;
        damageCounter.text = string.Empty;
    }

    private void Update()
    {
        SpriteBehaviour();
    }

    private void SpriteBehaviour()
    {
        if (Camera.main != null) transform.LookAt(Camera.main.transform.position, Vector3.up);

        if (iChar == null || iChar.currentHealth <= 0)
            Destroy(gameObject);

        healthSlider.value = iChar.currentHealth;
    }

    public void Damage( float value )
    {
        try
        {
            healthSlider.value -= value;

            damage += value;
            damageCounter.text = damage.ToString("00");
            if (!inDelay)
                StartCoroutine(DamageDelay());
        }
        catch
        {
            Destroy(this);
        }
    }

    private IEnumerator DamageDelay()
    {
        inDelay = true;

        while (damageDelay.value > healthSlider.value)
        {
            damageDelay.value -= smoothDamageDelay;
            yield return null;
        }
        inDelay = false;
        damage = 0;
        yield return new WaitForSeconds(damageCounterTimer);
        damageCounter.text = string.Empty;
    }
}