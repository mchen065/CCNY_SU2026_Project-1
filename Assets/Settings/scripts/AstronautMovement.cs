using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class AstronautMovement : MonoBehaviour
{
    public enum AstronautState
    {
        Idle,
        Walking,
        Shifting,
        Hit
    }

    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 2f;
    [SerializeField] private float verticalSpeed = 6f;
    [SerializeField] private float bottomLimit = -4f;
    [SerializeField] private float topLimit = 4f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    // Drag the three-heart UI Image into this field.
    [SerializeField] private Image healthBar;

    // How long the astronaut cannot take damage after a hit.
    [SerializeField] private float damageCooldown = 1f;

    // How long the astronaut remains in the Hit state.
    [SerializeField] private float hitDuration = 0.3f;

    [Header("Shifting")]
    [SerializeField] private Transform visual;

    // Half size by default.
    [Range(0.2f, 1f)]
    [SerializeField] private float shiftedSize = 0.5f;

    [Header("Animation Names")]
    [SerializeField] private string idleAnimation = "Idle";
    [SerializeField] private string walkingAnimation = "Walking";
    [SerializeField] private string shiftingAnimation = "Shifting";

    private Rigidbody2D rb;
    private BoxCollider2D hitbox;
    private Animator animator;

    private int currentHealth;
    private float verticalInput;

    private Vector3 normalVisualScale;
    private Vector2 normalHitboxSize;
    private Vector2 normalHitboxOffset;

    private bool isShifting;
    private bool isHit;
    private bool canTakeDamage = true;

    private AstronautState currentState =
        (AstronautState)(-1);

    // Other scripts can check whether the astronaut is shifting.
    public bool IsShifting => isShifting;

    // Other scripts can read the astronaut's health.
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();
        animator = GetComponentInChildren<Animator>();

        // Space physics.
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;

        // Allow movement on X and Y.
        // Only prevent unwanted rotation.
        rb.constraints =
            RigidbodyConstraints2D.FreezeRotation;

        if (visual == null && animator != null)
        {
            visual = animator.transform;
        }

        if (visual != null)
        {
            normalVisualScale = visual.localScale;
        }

        normalHitboxSize = hitbox.size;
        normalHitboxOffset = hitbox.offset;
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            // Required for fillAmount to change visually.
            healthBar.type = Image.Type.Filled;
            healthBar.fillMethod =
                Image.FillMethod.Horizontal;

            // Fill begins from the left side.
            healthBar.fillOrigin = 0;
            healthBar.fillAmount = 1f;
        }
        else
        {
            Debug.LogError(
                "Health Bar is not assigned on AstronautMovement.",
                this
            );
        }

        UpdateHealthBar();
        ChangeState(AstronautState.Idle);
    }

    private void Update()
    {
        if (SpaceGameManager.Instance != null &&
            SpaceGameManager.Instance.GameEnded)
        {
            verticalInput = 0f;
            return;
        }

        // W/S or Up/Down.
        // A and D are ignored.
        verticalInput =
            Input.GetAxisRaw("Vertical");

        bool wantsToShift =
            Input.GetKey(KeyCode.Space) &&
            !isHit;

        if (wantsToShift != isShifting)
        {
            SetShifting(wantsToShift);
        }

        if (isHit)
        {
            ChangeState(AstronautState.Hit);
        }
        else if (isShifting)
        {
            ChangeState(AstronautState.Shifting);
        }
        else if (Mathf.Abs(verticalInput) > 0.01f)
        {
            ChangeState(AstronautState.Walking);
        }
        else
        {
            ChangeState(AstronautState.Idle);
        }
    }

    private void FixedUpdate()
    {
        if (SpaceGameManager.Instance != null &&
            SpaceGameManager.Instance.GameEnded)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isHit)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Constantly travel right.
        // The player controls only the vertical movement.
        rb.linearVelocity = new Vector2(
            forwardSpeed,
            verticalInput * verticalSpeed
        );

        // Keep the astronaut inside the vertical play area.
        Vector2 position = rb.position;

        position.y = Mathf.Clamp(
            position.y,
            bottomLimit,
            topLimit
        );

        rb.position = position;
    }

    private void SetShifting(bool shifting)
    {
        isShifting = shifting;

        float sizeMultiplier =
            isShifting ? shiftedSize : 1f;

        // Change the visual size.
        if (visual != null)
        {
            visual.localScale =
                normalVisualScale * sizeMultiplier;
        }

        // Change the actual collision box size.
        hitbox.size =
            normalHitboxSize * sizeMultiplier;

        hitbox.offset =
            normalHitboxOffset * sizeMultiplier;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleContact(other.transform);
    }

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        HandleContact(collision.transform);
    }

    private void HandleContact(Transform objectHit)
    {
        // The spaceship is checked first so it never causes damage.
        SpaceshipGoal spaceship =
            objectHit.GetComponentInParent<SpaceshipGoal>();

        if (spaceship != null)
        {
            if (SpaceGameManager.Instance != null)
            {
                SpaceGameManager.Instance.WinGame();
            }

            return;
        }

        // Enemy power bullet.
        PowerBullet bullet =
            objectHit.GetComponentInParent<PowerBullet>();

        if (bullet != null)
        {
            TakeDamage(1);
            Destroy(bullet.gameObject);
            return;
        }

        // Golem enemy.
        GolemEnemy golem =
            objectHit.GetComponentInParent<GolemEnemy>();

        if (golem != null)
        {
            TakeDamage(1);
            return;
        }

        // Loose bouncing meteorite.
        LooseMeteorite looseMeteorite =
            objectHit.GetComponentInParent<LooseMeteorite>();

        if (looseMeteorite != null)
        {
            TakeDamage(1);
            return;
        }

        // Supports any remaining Meteorite.cs objects.
        Meterorite meteorite =
            objectHit.GetComponentInParent<Meterorite>();

        if (meteorite != null)
        {
            TakeDamage(1);
        }
    }

    // Public so LooseMeteorite.cs can call:
    // astronaut.TakeDamage(damage);
    public void TakeDamage(int amount = 1)
    {
        if (!canTakeDamage)
        {
            return;
        }

        if (SpaceGameManager.Instance != null &&
            SpaceGameManager.Instance.GameEnded)
        {
            return;
        }

        // Lock damage immediately so one collision
        // cannot remove several hearts.
        canTakeDamage = false;

        currentHealth -= amount;

        currentHealth = Mathf.Clamp(
            currentHealth,
            0,
            maxHealth
        );

        UpdateHealthBar();

        Debug.Log(
            "Astronaut health: " +
            currentHealth +
            "/" +
            maxHealth
        );

        if (currentHealth <= 0)
        {
            rb.linearVelocity = Vector2.zero;

            if (SpaceGameManager.Instance != null)
            {
                SpaceGameManager.Instance.LoseGame();
            }

            return;
        }

        StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        isHit = true;

        // Return to normal size after being hit.
        SetShifting(false);

        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(
            hitDuration
        );

        isHit = false;

        yield return new WaitForSeconds(
            damageCooldown
        );

        canTakeDamage = true;
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null)
        {
            return;
        }

        // 3/3 = 1.00
        // 2/3 = 0.67
        // 1/3 = 0.33
        // 0/3 = 0.00
        healthBar.fillAmount =
            (float)currentHealth / maxHealth;
    }

    private void ChangeState(
        AstronautState newState
    )
    {
        if (currentState == newState)
        {
            return;
        }

        currentState = newState;

        if (animator == null)
        {
            return;
        }

        switch (currentState)
        {
            case AstronautState.Idle:
                animator.Play(idleAnimation);
                break;

            case AstronautState.Walking:
                animator.Play(walkingAnimation);
                break;

            case AstronautState.Shifting:
                animator.Play(shiftingAnimation);
                break;

            case AstronautState.Hit:
                // Uses Idle because there is no Hit animation.
                animator.Play(idleAnimation);
                break;
        }
    }
}