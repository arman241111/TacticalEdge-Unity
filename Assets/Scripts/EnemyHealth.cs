using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // Also tell EnemyAI about damage
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.TakeDamage(0); // AI tracks its own health
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
        }
    }
}
