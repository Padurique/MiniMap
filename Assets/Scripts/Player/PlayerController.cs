using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sprintSpeedMultiplier = 2f;
    public Camera mainCamera;

    public int maxStamina = 100;
    public float staminaRegenRate = 5f;
    public float staminaDrainRate = 20f;
    public StaminaBar staminaBar;

    public int maxMana = 100;
    public float manaRegenRate = 2f;
    public int fireballManaCost = 20;
    public ManaBar manaBar;

    private Rigidbody2D rb;
    private bool isSprinting = false;
    private float currentStamina;
    public float currentMana;

    public PlayerHealth playerHealth;

    private float moveHorizontal;
    private float moveVertical;

    private bool isCrouching = false;

    private SpriteRenderer spriteRenderer;

    private bool isVaulting = false;
    private Coroutine vaultCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = false;
        rb.gravityScale = 0f;

        if (mainCamera == null)
            mainCamera = Camera.main;

        currentStamina = maxStamina;
        staminaBar.SetMaxStamina(maxStamina);

        currentMana = maxMana;
        manaBar.SetMaxMana(maxMana);

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal");
        moveVertical = Input.GetAxisRaw("Vertical");

        HandleRotation();
        HandleSprint();
        HandleCrouch();
        RegenerateStamina();
        RegenerateMana();
        playerHealth.RegenerateHealth();
        HandleVault();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        if (isVaulting)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        Vector2 movement = new Vector2(moveHorizontal, moveVertical).normalized;
        float currentMoveSpeed = isSprinting ? moveSpeed * sprintSpeedMultiplier : moveSpeed;
        rb.velocity = movement * currentMoveSpeed;
    }

    void HandleRotation()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        Vector2 direction = (mousePosition - transform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
    }

    void HandleSprint()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && currentStamina > 0)
        {
            isSprinting = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) || currentStamina <= 0)
        {
            isSprinting = false;
        }

        if (isSprinting)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina < 0)
            {
                currentStamina = 0;
                isSprinting = false;
            }
            staminaBar.SetStamina((int)currentStamina);
        }
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
            isCrouching = true;
        if (Input.GetKeyUp(KeyCode.LeftControl))
            isCrouching = false;

        // Visual feedback
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isCrouching ? Color.gray : Color.white;
        }
    }

    void RegenerateStamina()
    {
        if (!isSprinting && currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }
            staminaBar.SetStamina((int)currentStamina);
        }
    }

    void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            if (currentMana > maxMana)
            {
                currentMana = maxMana;
            }
            manaBar.SetMana((int)currentMana);
        }
    }

    public bool IsCrouching()
    {
        return isCrouching;
    }

    void HandleVault()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isVaulting)
        {
            isVaulting = true;
            Vector2 inputDir = new Vector2(moveHorizontal, moveVertical).normalized;
            if (inputDir == Vector2.zero)
            {
                isVaulting = false;
                return; // No direction pressed
            }

            Vector2 playerGridPos = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
            float checkRadius = 0.4f;

            // Position directly in the movement direction
            Vector2 obstaclePos = playerGridPos + inputDir;
            Vector2 vaultTargetPos = obstaclePos; // Land immediately after the obstacle

            // Check for a collider directly in the movement direction
            Collider2D obstacle = Physics2D.OverlapCircle(obstaclePos, checkRadius, LayerMask.GetMask("Collider"));

            if (obstacle != null && !isVaulting)
            {
                Debug.Log("Vaulting to: " + vaultTargetPos);
                if (vaultCoroutine != null) StopCoroutine(vaultCoroutine);
                vaultCoroutine = StartCoroutine(VaultOverObstacle(transform.position, vaultTargetPos, 0.25f)); // 0.25s vault duration
            }
            else
            {
                Debug.Log("Vault failed: must have a collider in movement direction.");
            }

            isVaulting = false;
        }
    }

    IEnumerator VaultOverObstacle(Vector3 start, Vector3 end, float duration)
    {
        isVaulting = true;
        float elapsed = 0f;
        rb.velocity = Vector2.zero; // Stop movement during vault

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
        isVaulting = false;
    }
}
