using UnityEngine;

public class EnemyShootingAI : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float detectionAngle = 60f;
    public int coneSegments = 20;
    public float auditoryRange = 0.7f;
    public float preferredDistance = 7f;
    public float tolerance = 1f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    private float nextFireTime = 0f;
    public AudioClip shootSound;
    private AudioSource audioSource;
    private Rigidbody2D rb;

    // Patrol state
    private enum State { Patrol, Engage, Memory }
    private State currentState = State.Patrol;

    private float patrolTimer = 0f;
    private float patrolMoveDuration = 2f;
    private float patrolWaitDuration = 1.5f;
    private float patrolLookDuration = 1f;
    private float patrolStateTimer = 0f;
    private Vector2 patrolDirection = Vector2.right;
    private int patrolPhase = 0; // 0=move, 1=wait, 2=look

    // Memory
    private float memoryTimer = 0f;
    private float memoryDuration = 5f;

    // Detection visuals
    private LineRenderer coneRenderer;
    private LineRenderer smallCircleRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        preferredDistance = detectionRange - 0.5f; // Keep player at edge of detection

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = shootSound;
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 5f;
        audioSource.maxDistance = 20f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;

        // Setup LineRenderer for the cone
        coneRenderer = gameObject.AddComponent<LineRenderer>();
        coneRenderer.positionCount = coneSegments + 2;
        coneRenderer.widthMultiplier = 0.05f;
        coneRenderer.material = new Material(Shader.Find("Sprites/Default"));
        coneRenderer.startColor = new Color(1, 0.5f, 0, 0.3f);
        coneRenderer.endColor = new Color(1, 0.5f, 0, 0.3f);
        coneRenderer.loop = true;
        coneRenderer.useWorldSpace = true;
        coneRenderer.sortingLayerName = "Default";
        coneRenderer.sortingOrder = 10;

        // Small detection circle
        smallCircleRenderer = new GameObject("SmallCircle").AddComponent<LineRenderer>();
        smallCircleRenderer.transform.parent = transform;
        smallCircleRenderer.positionCount = 32;
        smallCircleRenderer.widthMultiplier = 0.05f;
        smallCircleRenderer.material = new Material(Shader.Find("Sprites/Default"));
        smallCircleRenderer.startColor = Color.green;
        smallCircleRenderer.endColor = Color.green;
        smallCircleRenderer.loop = true;
        smallCircleRenderer.sortingLayerName = "Default";
        smallCircleRenderer.sortingOrder = 11;

        // Start facing a random direction
        patrolDirection = Random.insideUnitCircle.normalized;
        transform.up = patrolDirection;
    }

    void Update()
    {
        Vector2 toPlayer = player.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;
        Vector2 forward = transform.up;
        float angleToPlayer = Vector2.Angle(forward, toPlayer);

        // Vision cone detection
        bool playerInCone = (distanceToPlayer <= detectionRange) && (angleToPlayer <= detectionAngle / 2f);

        // Auditory circle detection (ignore if player is crouching)
        PlayerController playerController = player.GetComponent<PlayerController>();
        bool playerInCircle = false;
        if (playerController != null && !playerController.IsCrouching())
        {
            playerInCircle = (distanceToPlayer <= auditoryRange);
        }

        // State transitions
        switch (currentState)
        {
            case State.Patrol:
                if (playerInCone || playerInCircle)
                {
                    currentState = State.Engage;
                }
                else
                {
                    PatrolRoutine();
                }
                break;

            case State.Engage:
                if (playerInCone || playerInCircle)
                {
                    MaintainDistance(distanceToPlayer, toPlayer);

                    if (Time.time >= nextFireTime)
                    {
                        ShootProjectile();
                        nextFireTime = Time.time + 1f / fireRate;
                    }
                    memoryTimer = 0f; // Reset memory timer while player is detected
                }
                else
                {
                    currentState = State.Memory;
                    memoryTimer = 0f;
                }
                break;

            case State.Memory:
                if (playerInCone || playerInCircle)
                {
                    currentState = State.Engage;
                }
                else
                {
                    memoryTimer += Time.deltaTime;
                    MaintainDistance(distanceToPlayer, toPlayer); // Keep last known behavior
                    if (memoryTimer >= memoryDuration)
                    {
                        currentState = State.Patrol;
                        patrolPhase = 1; // Start with wait after engage
                        patrolStateTimer = 0f;
                    }
                }
                break;
        }

        DrawDetectionCone();
        DrawSmallCircle(auditoryRange);
    }

    void PatrolRoutine()
    {
        patrolStateTimer += Time.deltaTime;

        if (patrolPhase == 0) // Move
        {
            rb.velocity = patrolDirection * (moveSpeed * 0.5f);
            float targetAngle = Mathf.Atan2(patrolDirection.y, patrolDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, targetAngle), 90f * Time.deltaTime);

            if (patrolStateTimer >= patrolMoveDuration)
            {
                patrolPhase = 1;
                patrolStateTimer = 0f;
                rb.velocity = Vector2.zero;
            }
        }
        else if (patrolPhase == 1) // Wait
        {
            rb.velocity = Vector2.zero;
            if (patrolStateTimer >= patrolWaitDuration)
            {
                patrolPhase = 2;
                patrolStateTimer = 0f;
            }
        }
        else if (patrolPhase == 2) // Look around
        {
            rb.velocity = Vector2.zero;
            // Slowly rotate to simulate looking around
            transform.Rotate(0, 0, 60f * Time.deltaTime);
            if (patrolStateTimer >= patrolLookDuration)
            {
                patrolPhase = 0;
                patrolStateTimer = 0f;
                // Pick a new random direction
                patrolDirection = Random.insideUnitCircle.normalized;
            }
        }
    }

    void MaintainDistance(float distanceToPlayer, Vector2 toPlayer)
    {
        Vector2 direction = toPlayer.normalized;

        if (distanceToPlayer < preferredDistance - tolerance)
        {
            // Too close, move away
            rb.velocity = -direction * moveSpeed;
        }
        else if (distanceToPlayer > preferredDistance + tolerance)
        {
            // Too far, move closer
            rb.velocity = direction * moveSpeed;
        }
        else
        {
            // If within preferred range, don't move
            rb.velocity = Vector2.zero;
        }

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, targetAngle), 180f * Time.deltaTime);
    }

    void ShootProjectile()
    {
        Vector3 direction = (player.position - firePoint.position).normalized;
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.velocity = direction * 10f;

        audioSource.PlayOneShot(shootSound);
    }

    void DrawDetectionCone()
    {
        Vector3 origin = transform.position;
        Vector3 forward = transform.up;
        float halfAngle = detectionAngle / 2f;

        coneRenderer.positionCount = coneSegments + 2;
        coneRenderer.loop = true;

        coneRenderer.SetPosition(0, origin);

        for (int i = 0; i <= coneSegments; i++)
        {
            float angle = -halfAngle + (detectionAngle * i / coneSegments);
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Vector3 dir = rot * forward;
            coneRenderer.SetPosition(i + 1, origin + dir * detectionRange);
        }
    }

    void DrawSmallCircle(float radius)
    {
        int segments = 32;
        smallCircleRenderer.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = 2 * Mathf.PI * i / segments;
            Vector3 pos = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            smallCircleRenderer.SetPosition(i, pos);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw cone lines
        Vector3 forward = transform.up;
        float halfAngle = detectionAngle / 2f;
        Quaternion leftRayRotation = Quaternion.Euler(0, 0, -halfAngle);
        Quaternion rightRayRotation = Quaternion.Euler(0, 0, halfAngle);

        Vector3 leftRay = leftRayRotation * forward * detectionRange;
        Vector3 rightRay = rightRayRotation * forward * detectionRange;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftRay);
        Gizmos.DrawLine(transform.position, transform.position + rightRay);

        // Draw small detection circle
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, auditoryRange);
    }

    public bool IsPlayerDetected()
    {
        // Return true if the enemy is in Engage or Memory state
        return currentState == State.Engage || currentState == State.Memory;
    }
}