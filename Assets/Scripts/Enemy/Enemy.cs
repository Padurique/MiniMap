using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public GameObject healthBarPrefab;
    private GameObject healthBarInstance;
    private Slider healthBarSlider;

    public WaveManager waveManager;

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform.position + new Vector3(0, 1, 0), Quaternion.identity, transform);
            healthBarInstance.transform.SetParent(transform, false);
            healthBarSlider = healthBarInstance.GetComponentInChildren<Slider>();
        }
    }

    void Update()
    {
        if (healthBarInstance != null)
        {
            healthBarInstance.transform.position = transform.position + new Vector3(0, 1, 0);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (healthBarSlider != null)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            healthBarSlider.value = healthPercent;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (waveManager != null)
        {
            waveManager.EnemyDefeated();
        }
        else
        {
            Debug.LogError("WaveManager reference missing in Enemy!");
        }

        if (healthBarInstance != null)
            Destroy(healthBarInstance);

        Destroy(gameObject);
    }
}
