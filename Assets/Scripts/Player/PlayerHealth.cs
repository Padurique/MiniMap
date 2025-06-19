using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public HealthBar healthBar;

    public int CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player died! Game Over.");
        Application.Quit();
    }
    private float healthRegenTimer = 0f;

    public void RegenerateHealth()
    {
        if (currentHealth < maxHealth)
        {
            healthRegenTimer += Time.deltaTime;
            if (healthRegenTimer >= 1f)
            {
                currentHealth += 1;
                if (currentHealth > maxHealth)
                    currentHealth = maxHealth;
                healthBar.SetHealth(currentHealth);
                healthRegenTimer = 0f;
            }
        }
        else
        {
            healthRegenTimer = 0f;
        }
    }
}
