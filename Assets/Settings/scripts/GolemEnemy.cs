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

    [Header("Target")]
    public Transform astronaut;

    [Header("Movement")]
    public float chaseSpeed = 3f;
    public float followDistance = 4f;

    [Header("Shooting")]
    public GameObject powerBulletPrefab;
    public Transform firePoint;
    public float minAttackDelay = 3f;
    public float maxAttackDelay = 6f;
    public float attackDuration = 0.6f;

    [Header("Death / Hit")]
    public GameObject poofEffectPrefab;
    public float hitDuration = 0.3f;
    public float deathDuration = 0.4f;
    public float flyAwaySpeed = 8f;
    public float spinSpeed = 720f;

    [Header("Animation Names")]
    public string idleAnimation = "enemyidle";
    public string runAnimation = "enemyrun";
    public string attackAnimation = "enemyattack";
    public string hitAnimation = "golemhit";
    public string deathAnimation = "enemydeath";

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D[] colliders;

    private float attackTimer;
    private bool busy;
    private bool dead;

    private GolemState currentState = (GolemState)(-1);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        colliders = GetComponentsInChildren<Collider2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (astronaut == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

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
        if (dead)
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

        if (astronaut == null || busy)
        {
            return;
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
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
        if (dead || busy || astronaut == null)
        {
            return;
        }

        Vector2 targetPosition = new Vector2(
            astronaut.position.x - followDistance,
            astronaut.position.y
        );

        Vector2 newPosition = Vector2.MoveTowards(
            rb.position,
            targetPosition,
            chaseSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);
    }

    private IEnumerator AttackRoutine()
    {
        busy = true;
        rb.linearVelocity = Vector2.zero;

        ChangeState(GolemState.Attack);

        yield return new WaitForSeconds(attackDuration * 0.5f);

        if (powerBulletPrefab != null && firePoint != null)
        {
            Instantiate(
                powerBulletPrefab,
                firePoint.position,
                Quaternion.identity
            );
        }

        yield return new WaitForSeconds(attackDuration * 0.5f);

        ResetAttackTimer();
        busy = false;

        ChangeState(GolemState.Run);
    }

    public void HitByMeteorite()
    {
        if (dead)
        {
            return;
        }

        StartCoroutine(DeathRoutine());
    }

    public void HitByPlayer()
    {
        if (dead)
        {
            return;
        }

        StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        dead = true;
        busy = true;

        DisableColliders();
        ChangeState(GolemState.Hit);

        yield return new WaitForSeconds(hitDuration);

        rb.freezeRotation = false;
        rb.angularVelocity = spinSpeed;

        rb.linearVelocity = new Vector2(
            flyAwaySpeed,
            Random.Range(-2f, 2f)
        );

        Destroy(gameObject, 2.5f);
    }

    private IEnumerator DeathRoutine()
    {
        dead = true;
        busy = true;

        rb.linearVelocity = Vector2.zero;

        DisableColliders();
        ChangeState(GolemState.Death);

        yield return new WaitForSeconds(deathDuration);

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
        attackTimer = Random.Range(minAttackDelay, maxAttackDelay);
    }

    private void DisableColliders()
    {
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
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
            HitByMeteorite();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.GetComponentInParent<Meterorite>() != null)
        {
            HitByMeteorite();
        }
    }
}