using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Fireball Attack")]
    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireballSpeed = 0.01f;
    public float fireballLifetime = 2f;
    public int fireballManaCost = 20;
    public int fireballDamage = 10;
    public AudioClip shootSound;

    [Header("Melee Attack")]
    public GameObject swingPrefab;
    public Transform attackPoint;
    public float attackDuration = 0.3f;
    public int meleeDamage = 50;
    public float attackRange = 1f;
    public LayerMask enemyLayer;

    private bool isAttacking = false;
    private PlayerController playerController;
    private AudioSource audioSource;

    public float stealthKillRange = 1.2f; // Adjust as needed
    public KeyCode stealthKillKey = KeyCode.E;

    private bool fireballSelected = true; // true = fireball, false = slash

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 15f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    void Update()
    {
        // Fireball (ranged) attack
        if (Input.GetButtonDown("Fire1"))
        {
            if (fireballSelected)
            {
                if (playerController.currentMana >= fireballManaCost)
                    ShootFireball();
            }
            else
            {
                if (!isAttacking)
                    StartCoroutine(SwingAttack());
            }
        }

        // Melee attack
        if (Input.GetButtonDown("Fire2") && !isAttacking)
        {
            StartCoroutine(SwingAttack());
        }

        // Stealth kill
        if (Input.GetKeyDown(stealthKillKey))
        {
            AttemptStealthKill();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            fireballSelected = true;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            fireballSelected = false;
    }

    void ShootFireball()
    {
        playerController.currentMana -= fireballManaCost;
        if (shootSound != null)
            audioSource.PlayOneShot(shootSound);

        GameObject newFireball = Instantiate(
            fireballPrefab,
            firePoint.position,
            firePoint.rotation * Quaternion.Euler(0, 0, 90)
        );
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        Vector2 fireballDirection = ((Vector2)mousePosition - (Vector2)firePoint.position).normalized;
        if (fireballDirection == Vector2.zero)
            fireballDirection = firePoint.up;

        Rigidbody2D rb = newFireball.GetComponent<Rigidbody2D>();
        rb.velocity = fireballDirection * fireballSpeed; // fireballSpeed should now work!

        Fireball fireballScript = newFireball.GetComponent<Fireball>();
        if (fireballScript != null)
            fireballScript.damage = fireballDamage;

        Destroy(newFireball, fireballLifetime);
    }

    IEnumerator SwingAttack()
    {
        isAttacking = true;

        // Find the "playerarrow" transform in the player hierarchy
        Transform playerArrow = transform.Find("playerarrow");

        // Instantiate the swing and parent it to playerarrow if found, otherwise to player
        GameObject swing = Instantiate(
            swingPrefab,
            attackPoint.position,
            attackPoint.rotation * Quaternion.Euler(0, 0, 90),
            playerArrow != null ? playerArrow : transform
        );

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>()?.TakeDamage(meleeDamage);
        }

        Destroy(swing, attackDuration);

        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
    }

    void AttemptStealthKill()
    {
        // Find all enemies in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, stealthKillRange, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            // Try EnemyAI
            EnemyAI ai = hit.GetComponent<EnemyAI>();
            if (ai != null && !ai.IsPlayerDetected())
            {
                ai.GetComponent<Enemy>()?.TakeDamage(9999);
                Debug.Log("Stealth kill (EnemyAI)!");
                break;
            }

            // Try EnemyShootingAI
            EnemyShootingAI shooter = hit.GetComponent<EnemyShootingAI>();
            if (shooter != null && !shooter.IsPlayerDetected())
            {
                shooter.GetComponent<Enemy>()?.TakeDamage(9999);
                Debug.Log("Stealth kill (EnemyShootingAI)!");
                break;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}