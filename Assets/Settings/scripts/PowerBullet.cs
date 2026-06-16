using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PowerBullet : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private float lifetime = 5f;

    private void Start()
    {
        Rigidbody2D rb =
            GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;

        AstronautMovement astronaut =
            FindObjectOfType<AstronautMovement>();

        if (astronaut != null)
        {
            Vector2 direction =
                ((Vector2)astronaut.transform.position -
                 rb.position).normalized;

            rb.linearVelocity =
                direction * speed;
        }

        Destroy(gameObject, lifetime);
    }
}