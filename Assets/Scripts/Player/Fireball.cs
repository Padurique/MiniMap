using UnityEngine;

public class Fireball : MonoBehaviour
{
    public int damage = 20; // Set this from PlayerAttack if needed

    void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject); // Destroy fireball on hit
        }
        // Optionally: destroy on hitting walls, etc.
    }
}
