using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class LooseMeteorite : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    // Small vertical variation while still moving mainly left.
    [SerializeField] private float verticalVariation = 1.5f;

    [Header("Spinning")]
    // Negative value spins clockwise.
    [SerializeField] private float spinSpeed = -220f;

    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Breaking")]
    [SerializeField] private GameObject breakEffect;

    [Header("Cleanup")]
    [SerializeField] private float lifetime = 30f;

    private Rigidbody2D rb;
    private Collider2D meteoriteCollider;

    private bool broken;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        meteoriteCollider = GetComponent<Collider2D>();

        // Space physics.
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

        rb.interpolation =
            RigidbodyInterpolation2D.Interpolate;

        // Allow movement on both X and Y.
        // Allow the meteorite to spin.
        rb.constraints = RigidbodyConstraints2D.None;

        meteoriteCollider.isTrigger = false;

        // Make the meteorite bounce.
        PhysicsMaterial2D bounceMaterial =
            new PhysicsMaterial2D("MeteoriteBounce");

        bounceMaterial.friction = 0f;
        bounceMaterial.bounciness = 1f;

        meteoriteCollider.sharedMaterial =
            bounceMaterial;
    }

    private void Start()
    {
        // Begin travelling mainly toward the left.
        Vector2 startingDirection = new Vector2(
            -1f,
            Random.Range(
                -verticalVariation,
                verticalVariation
            ) * 0.15f
        ).normalized;

        rb.linearVelocity =
            startingDirection * moveSpeed;

        rb.angularVelocity = spinSpeed;

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (broken)
        {
            return;
        }

        // Force gravity to remain disabled.
        rb.gravityScale = 0f;

        // Keep the meteorite moving at a steady speed
        // without forcing it downward.
        if (rb.linearVelocity.sqrMagnitude < 0.1f)
        {
            rb.linearVelocity =
                Vector2.left * moveSpeed;
        }
        else
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized *
                moveSpeed;
        }

        rb.angularVelocity = spinSpeed;
    }

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        if (broken)
        {
            return;
        }

        // Damage the astronaut.
        AstronautMovement astronaut =
            collision.collider
                .GetComponentInParent<AstronautMovement>();

        if (astronaut != null)
        {
            astronaut.TakeDamage(damage);
            return;
        }

        // Two loose meteorites break when they collide.
        LooseMeteorite otherMeteorite =
            collision.collider
                .GetComponentInParent<LooseMeteorite>();

        if (otherMeteorite != null &&
            otherMeteorite != this)
        {
            otherMeteorite.BreakMeteorite();
            BreakMeteorite();
        }

        // Wall collisions are handled automatically
        // by the bouncy Physics Material.
    }

    public void BreakMeteorite()
    {
        if (broken)
        {
            return;
        }

        broken = true;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        meteoriteCollider.enabled = false;

        if (breakEffect != null)
        {
            Instantiate(
                breakEffect,
                transform.position,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }
}