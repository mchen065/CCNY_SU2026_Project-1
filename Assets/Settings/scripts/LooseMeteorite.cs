using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class LooseMeteorite : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float verticalVariation = 0.3f;

    [Header("Spinning")]
    [SerializeField] private float spinSpeed = -220f;

    [Header("Dramatic Breaking")]
    [SerializeField] private GameObject breakParticlePrefab;

    // Slightly enlarges the effect.
    [SerializeField] private float particleScale = 1.5f;

    [Header("Cleanup")]
    [SerializeField] private float lifetime = 30f;

    private Rigidbody2D rb;
    private Collider2D meteoriteCollider;

    private bool isBreaking;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        meteoriteCollider = GetComponent<Collider2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;

        // Zero-gravity space physics.
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        rb.constraints = RigidbodyConstraints2D.None;

        rb.collisionDetectionMode =
            CollisionDetectionMode2D.Continuous;

        rb.interpolation =
            RigidbodyInterpolation2D.Interpolate;

        meteoriteCollider.isTrigger = false;

        PhysicsMaterial2D bounceMaterial =
            new PhysicsMaterial2D("MeteoriteBounce");

        bounceMaterial.friction = 0f;
        bounceMaterial.bounciness = 1f;

        meteoriteCollider.sharedMaterial =
            bounceMaterial;
    }

    private void Start()
    {
        // Start by flying mainly toward the left.
        Vector2 direction = new Vector2(
            -1f,
            Random.Range(
                -verticalVariation,
                verticalVariation
            )
        ).normalized;

        rb.linearVelocity =
            direction * moveSpeed;

        rb.angularVelocity =
            spinSpeed;

        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (isBreaking)
        {
            return;
        }

        // Keep space gravity disabled.
        rb.gravityScale = 0f;

        // Maintain a steady speed after bouncing.
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

        rb.angularVelocity =
            spinSpeed;
    }

    private void OnCollisionEnter2D(
        Collision2D collision
    )
    {
        if (isBreaking)
        {
            return;
        }

        // The astronaut handles its own percentage roll.
        if (collision.collider
            .GetComponentInParent<AstronautMovement>() != null)
        {
            return;
        }

        LooseMeteorite otherMeteorite =
            collision.collider
                .GetComponentInParent<LooseMeteorite>();

        if (otherMeteorite == null ||
            otherMeteorite == this ||
            otherMeteorite.isBreaking)
        {
            // Hitting a wall only causes a bounce.
            return;
        }

        /*
         * Both meteorites receive the collision callback.
         * Only the meteorite with the smaller instance ID
         * creates the shared explosion.
         */
        if (GetInstanceID() >
            otherMeteorite.GetInstanceID())
        {
            return;
        }

        Vector3 explosionPosition =
            GetCollisionPosition(
                collision,
                otherMeteorite
            );

        // Mark both immediately to prevent duplicate effects.
        isBreaking = true;
        otherMeteorite.isBreaking = true;

        SpawnDramaticParticles(
            explosionPosition
        );

        otherMeteorite.DestroyMeteorite();
        DestroyMeteorite();
    }

    private Vector3 GetCollisionPosition(
        Collision2D collision,
        LooseMeteorite otherMeteorite
    )
    {
        if (collision.contactCount > 0)
        {
            return collision.GetContact(0).point;
        }

        return (
            transform.position +
            otherMeteorite.transform.position
        ) * 0.5f;
    }

    /*
     * AstronautMovement calls this when the
     * normal-size percentage roll succeeds.
     */
    public void BreakMeteorite()
    {
        if (isBreaking)
        {
            return;
        }

        isBreaking = true;

        SpawnDramaticParticles(
            transform.position
        );

        DestroyMeteorite();
    }

    private void SpawnDramaticParticles(Vector3 position)
    {
        if (breakParticlePrefab == null)
        {
            Debug.LogError(
                "Break Particle Prefab is not assigned!",
                this
            );

            return;
        }

        GameObject effect = Instantiate(
            breakParticlePrefab,
            position,
            Quaternion.identity
        );

        effect.SetActive(true);

        // Make the explosion clearly visible.
        effect.transform.localScale =
            Vector3.one * particleScale;

        ParticleSystem[] systems =
            effect.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particles in systems)
        {
            particles.gameObject.SetActive(true);

            ParticleSystem.MainModule main =
                particles.main;

            // Keep fragments in place after the meteorite disappears.
            main.simulationSpace =
                ParticleSystemSimulationSpace.World;

            main.loop = false;

            // Destroy the particle object after all particles finish.
            main.stopAction =
                ParticleSystemStopAction.Destroy;

            ParticleSystemRenderer particleRenderer =
                particles.GetComponent<ParticleSystemRenderer>();

            if (particleRenderer != null)
            {
                // Force particles in front of the background.
                particleRenderer.sortingLayerName = "Effects";
                particleRenderer.sortingOrder = 50;
            }

            particles.Clear(true);
            particles.Play(true);
        }

        Debug.Log(
            "Meteorite explosion spawned at: " + position,
            effect
        );
    }

    private void DestroyMeteorite()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        meteoriteCollider.enabled = false;

        // Hide immediately so the break feels responsive.
        SpriteRenderer[] renderers =
            GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sprite in renderers)
        {
            sprite.enabled = false;
        }

        Destroy(gameObject);
    }
}