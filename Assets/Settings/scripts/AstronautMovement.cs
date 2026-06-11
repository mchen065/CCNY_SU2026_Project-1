using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AstronautMovement : MonoBehaviour
{
    public enum AstronautState
    {
        Idle,
        Walking,
        Shifting,
        Shooting,
        Hit
    }

    [Header("Movement")]
    [SerializeField] private float forwardSpeed = 3f;
    [SerializeField] private float verticalSpeed = 5f;

    [Header("Shifting")]
    [SerializeField] private Transform visual;
    [SerializeField] private float shiftedSize = 0.5f;

    [Header("Shooting")]
    [SerializeField] private GameObject pebblePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private KeyCode shootKey = KeyCode.F;

    [Header("Animation Names")]
    [SerializeField] private string idleAnimation = "Idle";
    [SerializeField] private string walkingAnimation = "Walking";
    [SerializeField] private string shiftingAnimation = "Shifting";

    private Rigidbody2D rb;
    private Animator animator;

    private float verticalInput;
    private Vector3 normalScale;

    private bool shifting;
    private bool hit;

    private int pebbleAmmo;

    private AstronautState currentState;

    public AstronautState CurrentState => currentState;
    public int PebbleAmmo => pebbleAmmo;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        // No gravity in space.
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

        // Only W/S or Up/Down control vertical movement.
        verticalInput = Input.GetAxisRaw("Vertical");

        // Hold Space to shrink through narrow gaps.
        shifting = Input.GetKey(KeyCode.Space) && !hit;

        if (visual != null)
        {
            visual.localScale = shifting
                ? normalScale * shiftedSize
                : normalScale;
        }

        // Fire one pebble.
        if (Input.GetKeyDown(shootKey) &&
            pebbleAmmo > 0 &&
            !hit)
        {
            ShootPebble();
        }

        // Choose the state and animation.
        if (hit)
        {
            ChangeState(AstronautState.Hit);
        }
        else if (shifting)
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

        if (hit)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Constantly travel right while the player controls Y.
        rb.linearVelocity = new Vector2(
            forwardSpeed,
            verticalInput * verticalSpeed
        );
    }

    public void CollectPebble(int amount = 1)
    {
        pebbleAmmo += Mathf.Max(0, amount);

        Debug.Log($"Pebble ammo: {pebbleAmmo}");
    }

    private void ShootPebble()
    {
        if (pebblePrefab == null || firePoint == null)
        {
            return;
        }

        pebbleAmmo--;

        ChangeState(AstronautState.Shooting);

        Instantiate(
            pebblePrefab,
            firePoint.position,
            Quaternion.identity
        );
    }

    public void TakeHit()
    {
        if (!hit)
        {
            StartCoroutine(HitRoutine());
        }
    }

    private IEnumerator HitRoutine()
    {
        hit = true;

        if (SpaceGameManager.Instance != null)
        {
            SpaceGameManager.Instance.LoseLife();
        }

        yield return new WaitForSeconds(0.5f);

        hit = false;
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

            case AstronautState.Shooting:
                animator.Play(idleAnimation);
                break;

            case AstronautState.Hit:
                animator.Play(idleAnimation);
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Meteorite") ||
            other.CompareTag("PowerBullet"))
        {
            TakeHit();

            if (other.CompareTag("PowerBullet"))
            {
                Destroy(other.gameObject);
            }

            return;
        }

        if (other.CompareTag("StonePickup"))
        {
            CollectPebble();
            Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag("Spaceship") &&
            SpaceGameManager.Instance != null)
        {
            SpaceGameManager.Instance.WinGame();
        }
    }
}