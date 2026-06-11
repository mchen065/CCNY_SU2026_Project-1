using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GolemEnemy : MonoBehaviour
{
    // The golem's gameplay states.
    public enum GolemState
    {
        Idle,
        Run,
        Attack,
        Hit,
        Death
    }

    [Header("Astronaut")]
    [SerializeField] private Transform astronaut;

    [Header("Chasing")]
    [SerializeField] private float chaseSpeed = 3f;

    // How far behind the astronaut the golem stays.
    [SerializeField] private float followDistance = 4f;

    [Header("Attack")]
    [SerializeField] private GameObject powerBulletPrefab;
    [SerializeField] private Transform firePoint;

    [SerializeField] private float bulletSpeed = 7f;
    [SerializeField] private float minimumAttackDelay = 3f;
    [SerializeField] private float maximumAttackDelay = 6f;
    [SerializeField] private float attackDuration = 0.6f;

    [Header("Hit By Pebble")]
    [SerializeField] private float hitAnimationDuration = 0.25f;

    // Positive speed makes the golem fly right.
    [SerializeField] private float flyAwaySpeed = 9f;

    // Spins the golem while it flies away.
    [SerializeField] private float spinSpeed = 720f;

    [Header("Meteorite Death")]
    [SerializeField] private float deathAnimationDuration = 0.4f;
    [SerializeField] private GameObject poofEffectPrefab;

    [Header("Exact Animator State Names")]
    [SerializeField] private string idleAnimation = "enemyidle";
    [SerializeField] private string runAnimation = "enemyrun";
    [SerializeField] private string attackAnimation = "enemyattack";
    [SerializeField] private string hitAnimation = "golemhit";
    [SerializeField] private string deathAnimation = "enemydeath";

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D[] golemColliders;

    private float attackTimer;
    private bool stateLocked;
    private bool destroyed;

    private Coroutine activeRoutine;

    // Invalid starting value makes the first animation play.
    private GolemState currentState = (GolemState)(-1);

    public GolemState CurrentState => currentState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        golemColliders =
            GetComponentsInChildren<Collider2D>();

        // The golem floats in space.
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // Automatically find the astronaut.
        if (astronaut == null)
        {
            GameObject player =
                GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                astronaut = player.transform;
            }
        }

        ResetAttackTimer();
        ChangeState(GolemState.Idle);
    }

    private void Update()
    {
        if (destroyed)
        {
            return;
        }

        // Stop when the game ends.
        if (SpaceGameManager.Instance != null &&
            SpaceGameManager.Instance.GameEnded)
        {
            rb.linearVelocity = Vector2.zero;
            ChangeState(GolemState.Idle);
            return;
        }

        if (astronaut == null)
        {
            ChangeState(GolemState.Idle);
            return;
        }

        // Attack, Hit, and Death control their own states.
        if (stateLocked)
        {
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            activeRoutine =
                StartCoroutine(AttackRoutine());

            return;
        }

        ChangeState(GolemState.Run);
    }

    private void FixedUpdate()
    {
        if (destroyed ||
            stateLocked ||
            astronaut == null ||
            currentState != GolemState.Run)
        {
            return;
        }

        // The golem follows behind the astronaut.
        float desiredX =
            astronaut.position.x - followDistance;

        // Prevent the golem from moving backward to the left.
        desiredX = Mathf.Max(
            rb.position.x,
            desiredX
        );

        Vector2 targetPosition = new Vector2(
            desiredX,
            astronaut.position.y
        );

        Vector2 nextPosition = Vector2.MoveTowards(
            rb.position,
            targetPosition,
            chaseSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(nextPosition);
    }

    private IEnumerator AttackRoutine()
    {
        stateLocked = true;

        rb.linearVelocity = Vector2.zero;
        ChangeState(GolemState.Attack);

        // Wait until the middle of the attack animation.
        yield return new WaitForSeconds(
            attackDuration * 0.5f
        );

        ShootPowerBullet();

        yield return new WaitForSeconds(
            attackDuration * 0.5f
        );

        stateLocked = false;
        activeRoutine = null;

        ResetAttackTimer();
        ChangeState(GolemState.Run);
    }

    private void ShootPowerBullet()
    {
        if (powerBulletPrefab == null ||
            firePoint == null ||
            astronaut == null)
        {
            return;
        }

        GameObject bullet = Instantiate(
            powerBulletPrefab,
            firePoint.position,
            Quaternion.identity
        );

        Rigidbody2D bulletBody =
            bullet.GetComponent<Rigidbody2D>();

        if (bulletBody != null)
        {
            bulletBody.gravityScale = 0f;

            Vector2 direction =
                ((Vector2)astronaut.position -
                 bulletBody.position).normalized;

            bulletBody.linearVelocity =
                direction * bulletSpeed;
        }

        Destroy(bullet, 5f);
    }

    // Called when the astronaut's pebble hits the golem.
    public void TakePebbleHit()
    {
        if (destroyed)
        {
            return;
        }

        StopActiveRoutine();

        activeRoutine =
            StartCoroutine(HitFlyAwayRoutine());
    }

    private IEnumerator HitFlyAwayRoutine()
    {
        destroyed = true;
        stateLocked = true;

        rb.linearVelocity = Vector2.zero;
        ChangeState(GolemState.Hit);

        DisableColliders();

        yield return new WaitForSeconds(
            hitAnimationDuration
        );

        // Spin and fly toward the right side of the screen.
        rb.freezeRotation = false;
        rb.angularVelocity = spinSpeed;

        rb.linearVelocity = new Vector2(
            flyAwaySpeed,
            Random.Range(-2f, 2f)
        );

        Destroy(gameObject, 2.5f);
    }

    // Called when the golem touches a meteorite.
    public void DieFromMeteorite()
    {
        if (destroyed)
        {
            return;
        }

        StopActiveRoutine();

        activeRoutine =
            StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        destroyed = true;
        stateLocked = true;

        rb.linearVelocity = Vector2.zero;
        ChangeState(GolemState.Death);

        DisableColliders();

        yield return new WaitForSeconds(
            deathAnimationDuration
        );

        if (poofEffectPrefab != null)
        {
            Instantiate(
                poofEffectPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }

    private void ResetAttackTimer()
    {
        attackTimer = Random.Range(
            minimumAttackDelay,
            maximumAttackDelay
        );
    }

    private void StopActiveRoutine()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
    }

    private void DisableColliders()
    {
        foreach (Collider2D golemCollider in golemColliders)
        {
            golemCollider.enabled = false;
        }
    }

    private void ChangeState(GolemState newState)
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
            case GolemState.Idle:
                animator.Play(idleAnimation);
                break;

            case GolemState.Run:
                animator.Play(runAnimation);
                break;

            case GolemState.Attack:
                animator.Play(attackAnimation);
                break;

            case GolemState.Hit:
                animator.Play(hitAnimation);
                break;

            case GolemState.Death:
                animator.Play(deathAnimation);
                break;
        }

        Debug.Log($"Golem state: {currentState}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Pebble makes the golem spin and fly right.
        if (other.CompareTag("StoneProjectile"))
        {
            Destroy(other.gameObject);
            TakePebbleHit();
            return;
        }

        // Meteorite makes the golem play Death and go poof.
        if (other.CompareTag("Meteorite"))
        {
            DieFromMeteorite();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Meteorite"))
        {
            DieFromMeteorite();
        }
    }
}