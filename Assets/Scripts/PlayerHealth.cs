using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public int armor = 0;
    public bool hasHelmet = false;

    [Header("UI")]
    public Text healthText;
    public Text armorText;
    public Image damageOverlay;

    private float damageAlpha = 0f;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        // Damage overlay fade
        if (damageAlpha > 0)
        {
            damageAlpha -= Time.deltaTime * 2f;
            if (damageOverlay != null)
                damageOverlay.color = new Color(1, 0, 0, damageAlpha * 0.3f);
        }

        // Update UI
        if (healthText != null)
            healthText.text = "HP: " + currentHealth;
        if (armorText != null)
            armorText.text = armor > 0 ? "Armor: " + armor : "";
    }

    public void TakeDamage(float amount)
    {
        // Armor absorbs some damage
        if (armor > 0)
        {
            float absorbed = amount * 0.5f;
            float armorDmg = Mathf.Min(armor, absorbed);
            armor -= (int)armorDmg;
            amount -= armorDmg;
        }

        currentHealth -= (int)amount;
        damageAlpha = 1f;

        // Damage flash + sound
        if (GamePolish.Instance != null) GamePolish.Instance.ShowDamageFlash();
        if (SoundManager.Instance != null) SoundManager.Instance.PlayHitBody();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        // Respawn
        currentHealth = maxHealth;
        armor = 0;
        transform.position = new Vector3(0, 2, 0);
    }
}
