using UnityEngine;

public class EnemySpell : MonoBehaviour
{
    public int fireballDamage = 20; // Damage value applied to enemies on hit

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with an enemy
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // Apply damage to the enemy
            playerHealth.TakeDamage(fireballDamage);
        }

        // Destroy the fireball on collision
        Destroy(gameObject);
    }
}
