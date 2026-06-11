using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PebbleBullet : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 9f;
    [SerializeField] private float lifetime = 4f;

    [Header("Animation")]
    [SerializeField] private string pebbleAnimationName = "Pebble";

    private Rigidbody2D rb;
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        rb.gravityScale = 0f;

        // Pebble travels toward the right.
        rb.linearVelocity = Vector2.right * speed;

        if (animator != null)
        {
            animator.Play(pebbleAnimationName);
        }

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") ||
            other.CompareTag("Meteorite"))
        {
            Destroy(gameObject);
        }
    }
}