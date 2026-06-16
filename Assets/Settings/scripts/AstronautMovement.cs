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

    // The astronaut moves faster while Space is held.
    // Example: 1.5 = 50% faster.
    [SerializeField] private float shiftingSpeedMultiplier = 1.5f;

    [SerializeField] private float bottomLimit = -4f;
    [SerializeField] private float topLimit = 4f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private Image healthBar;

    [SerializeField] private float hitDuration = 0.3f;
    [SerializeField] private float damageCooldown = 1f;

    [Header("Meteorite Breaking")]

    // This chance is used only while the astronaut is normal-sized.
    // 0.35 means a 35% chance.
    [Range(0f, 1f)]
    [SerializeField] private float normalBreakChance = 0.35f;

    [Header("Shifting")]
    [SerializeField] private Transform visual;

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

    public bool IsShifting => isShifting;
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

        // Allow X and Y movement, but stop rotation.
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
            healthBar.type = Image.Type.Filled;
            healthBar.fillMethod =
                Image.FillMethod.Horizontal;

            healthBar.fillOrigin = 0;
            healthBar.fillAmount = 1f;
        }
        else
        {
            Debug.LogError(
                "Assign the Health Bar UI Image.",
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

        float speedMultiplier =
            isShifting
                ? shiftingSpeedMultiplier
                : 1f;

        // Shifting increases both forward and vertical speed.
        rb.linearVelocity = new Vector2(
            forwardSpeed * speedMultiplier,
            verticalInput *
                verticalSpeed *
                speedMultiplier
        );

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

        if (visual != null)
        {
            visual.localScale =
                normalVisualScale * sizeMultiplier;
        }

        // Shrink the physical collider too.
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
        // Spaceship always wins and never causes damage.
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

        // Golem collision.
        GolemEnemy golem =
            objectHit.GetComponentInParent<GolemEnemy>();

        if (golem != null)
        {
            TakeDamage(1);
            return;
        }

        // Loose meteorite collision.
        LooseMeteorite meteorite =
            objectHit.GetComponentInParent<LooseMeteorite>();

        if (meteorite != null)
        {
            HandleMeteoriteCollision(meteorite);
        }
    }

    private void HandleMeteoriteCollision(
        LooseMeteorite meteorite
    )
    {
        if (meteorite == null)
        {
            return;
        }

        // A normal-sized astronaut has a percentage
        // chance to break the meteorite.
        if (!isShifting)
        {
            float breakRoll = Random.value;

            if (breakRoll <= normalBreakChance)
            {
                Debug.Log(
                    "Meteorite broken! Roll: " +
                    breakRoll
                );

                meteorite.BreakMeteorite();
                return;
            }
        }

        // Shifting does not receive the normal-size
        // breaking chance. A failed roll also causes damage.
        TakeDamage(1);
    }

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
                animator.Play(idleAnimation);
                break;
        }
    }
}