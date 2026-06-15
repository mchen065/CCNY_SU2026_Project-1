using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PowerBullet : MonoBehaviour
{
    public float speed = 6f;
    public float lifetime = 5f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            Vector2 direction =
                (player.transform.position - transform.position).normalized;

            rb.linearVelocity = direction * speed;
        }

        Destroy(gameObject, lifetime);
    }
}