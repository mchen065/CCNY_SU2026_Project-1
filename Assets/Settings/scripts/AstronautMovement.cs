using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
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
    public float forwardSpeed = 3f;
    public float verticalSpeed = 5f;
    public float bottomLimit = -4f;
    public float topLimit = 4f;

    [Header("Health")]
    public int maxHealth = 3;
    public Image healthBar;
    public float damageCooldown = 1f;

    [Header("Shifting")]
    public Transform visual;
    public float shiftedSize = 0.5f;

    [Header("Animations")]
    public string idleAnimation = "Idle";
    public string walkingAnimation = "Walking";
    public string shiftingAnimation = "Shifting";

    private Rigidbody2D rb;
    private Animator animator;

    private int currentHealth;
    private float verticalInput;
    private Vector3 normalScale;

    private bool canTakeDamage = true;
    private bool isShifting;
    private bool isHit;

    private AstronautState currentState = AstronautState.Idle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (visual == null && animator != null)
        {
            visual = animator.transform;
        }

        if (visual != null)
        {
            normalScale = visual.localScale;
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.type = Image.Type.Filled;
            healthBar.fillMethod = Image.FillMethod.Horizontal;
            healthBar.fillOrigin = 0;
            healthBar.fillAmount = 1f;
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

        verticalInput = Input.GetAxisRaw("Vertical");

        isShifting = Input.GetKey(KeyCode.Space) && !isHit;

        if (visual != null)
        {
            visual.localScale = isShifting
                ? normalScale * shiftedSize
                : normalScale;
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

        rb.linearVelocity = new Vector2(
            forwardSpeed,
            verticalInput * verticalSpeed
        );

        Vector2 position = rb.position;
        position.y = Mathf.Clamp(position.y, bottomLimit, topLimit);
        rb.position = position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Meterorite>() != null)
        {
            TakeDamage(1);
            return;
        }

        if (other.GetComponentInParent<GolemEnemy>() != null)
        {
            TakeDamage(1);
            return;
        }

        if (other.GetComponentInParent<PowerBullet>() != null)
        {
            TakeDamage(1);
            Destroy(other.GetComponentInParent<PowerBullet>().gameObject);
            return;
        }

        if (other.CompareTag("Spaceship"))
        {
            if (SpaceGameManager.Instance != null)
            {
                SpaceGameManager.Instance.WinGame();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.GetComponentInParent<Meterorite>() != null)
        {
            TakeDamage(1);
            return;
        }

        if (collision.transform.GetComponentInParent<GolemEnemy>() != null)
        {
            TakeDamage(1);
        }
    }

    private void TakeDamage(int amount)
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

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log("Astronaut health: " + currentHealth + "/" + maxHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
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
        canTakeDamage = false;
        isHit = true;

        yield return new WaitForSeconds(0.4f);

        isHit = false;

        yield return new WaitForSeconds(damageCooldown);

        canTakeDamage = true;
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    private void ChangeState(AstronautState newState)
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

        if (currentState == AstronautState.Idle)
        {
            animator.Play(idleAnimation);
        }
        else if (currentState == AstronautState.Walking)
        {
            animator.Play(walkingAnimation);
        }
        else if (currentState == AstronautState.Shifting)
        {
            animator.Play(shiftingAnimation);
        }
        else if (currentState == AstronautState.Hit)
        {
            animator.Play(idleAnimation);
        }
    }
}