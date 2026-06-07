using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    float horizontalInput;
    float verticalInput;

    public float moveSpeed = 7f;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (horizontalInput < 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (horizontalInput > 0)
        {
            spriteRenderer.flipX = true;
        }

    }


    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            horizontalInput * moveSpeed,
            verticalInput * moveSpeed * 0.4f
        );
    }
    void LateUpdate()
    {
        Vector3 pos = transform.position;

        pos.y = Mathf.Clamp(pos.y, -34f, -24f);

        transform.position = pos;
    }
}