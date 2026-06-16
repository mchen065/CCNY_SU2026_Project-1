
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GolemEnemy : MonoBehaviour
{
    public enum GolemState
    {
        Idle,
        Run,
        Attack,
        Hit,
        Death
    }

    [Header("Detection")]
    [SerializeField] private Transform astronaut;
    [SerializeField] private float detectionRadius = 8f;

    [Header("Pack Chasing")]
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float followDistance = 3.5f;
    [SerializeField] private float packSpread = 1.5f;

    [Header("Attack")]
    [SerializeField] private GameObject powerBulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float attackRange = 7f;
    [SerializeField] private float minimumAttackDelay = 3f;
    [SerializeField] private float maximumAttackDelay = 6f;
    [SerializeField] private float attackDuration = 0.6f;

    [Header("Death")]
    [SerializeField] private GameObject poofEffectPrefab;
    [SerializeField] private float deathDuration = 0.4f;

    [Header("Animation Names")]
    [SerializeField] private string idleAnimation = "enemyidle";
    [SerializeField] private string runAnimation = "enemyrun";
    [SerializeField] private string attackAnimation = "enemyattack";
    [SerializeField] private string hitAnimation = "golemhit";
    [SerializeField] private string deathAnimation = "enemydeath";

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D[] enemyColliders;

    private Vector2 packOffset;

    private float attackTimer;

    private bool detectedPlayer;
    private bool busy;
    private bool dead;

    private GolemState currentState =
        (GolemState)(-1);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        enemyColliders =
            GetComponentsInChildren<Collider2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        packOffset = new Vector2(
            Random.Range(-packSpread, packSpread),
            Random.Range(-packSpread, packSpread)
        );

        if (astronaut == null)
        {
            AstronautMovement player =
                FindObjectOfType<AstronautMovement>();

            if (player != null)
            {
                astronaut = player.transform;
            }
        }

        ResetAttackTimer();
        ChangeState(GolemState.Idle);
    }

    public void SetTarget(Transform newTarget)
    {
        astronaut = newTarget;
    }

    private void Update()
    {
        if (dead || astronaut == null)
        {
            return;
        }

        if (SpaceGameManager.Instance != null &&
            SpaceGameManager.Instance.GameEnded)
        {
            rb.linearVelocity = Vector2.zero;
            ChangeState(GolemState.Idle);
            return;
        }

        float distanceToPlayer =
            Vector2.Distance(
                transform.position,
                astronaut.position
            );

        // Once detected, the golem permanently joins the pack.
        if (!detectedPlayer &&
            distanceToPlayer <= detectionRadius)
        {
            detectedPlayer = true;
        }

        if (!detectedPlayer)
        {
            ChangeState(GolemState.Idle);
            return;
        }

        if (busy)
        {
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f &&
            distanceToPlayer <= attackRange)
        {
            StartCoroutine(AttackRoutine());
        }
        else
        {
            ChangeState(GolemState.Run);
        }
    }

    private void FixedUpdate()
    {
        if (dead ||
            busy ||
            !detectedPlayer ||
            astronaut == null)
        {
            return;
        }

        Vector2 targetPosition = new Vector2(
            astronaut.position.x -
                followDistance +
                packOffset.x,

            astronaut.position.y +
                packOffset.y
        );

        rb.MovePosition(
            Vector2.MoveTowards(
                rb.position,
                targetPosition,
                chaseSpeed * Time.fixedDeltaTime
            )
        );
    }

    private IEnumerator AttackRoutine()
    {
        busy = true;
        rb.linearVelocity = Vector2.zero;

        ChangeState(GolemState.Attack);

        yield return new WaitForSeconds(
            attackDuration * 0.5f
        );

        if (powerBulletPrefab != null &&
            firePoint != null)
        {
            Instantiate(
                powerBulletPrefab,
                firePoint.position,
                Quaternion.identity
            );
        }

        yield return new WaitForSeconds(
            attackDuration * 0.5f
        );

        busy = false;
        ResetAttackTimer();

        ChangeState(GolemState.Run);
    }

    private void DieFromMeteorite()
    {
        if (dead)
        {
            return;
        }

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        dead = true;
        busy = true;

        rb.linearVelocity = Vector2.zero;

        ChangeState(GolemState.Death);
        DisableColliders();

        yield return new WaitForSeconds(
            deathDuration
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

    private void DisableColliders()
    {
        foreach (Collider2D enemyCollider in enemyColliders)
        {
            enemyCollider.enabled = false;
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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Meterorite>() != null)
        {
            DieFromMeteorite();
        }
    }

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        if (collision.collider
            .GetComponentInParent<Meterorite>() != null)
        {
            DieFromMeteorite();
        }
    }
}