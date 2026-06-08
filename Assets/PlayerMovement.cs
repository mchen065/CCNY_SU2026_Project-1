using UnityEngine;
using System.Collections;
public class PlayerMovement : MonoBehaviour
{
    float horizontalInput;
    float verticalInput;

    public float moveSpeed = 7f;
    private float normalMoveSpeed;
    private Coroutine speedBoostCoroutine;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        normalMoveSpeed = moveSpeed;
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
    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if (speedBoostCoroutine != null)
        {
            StopCoroutine(speedBoostCoroutine);
        }

        moveSpeed = normalMoveSpeed * multiplier;
        speedBoostCoroutine = StartCoroutine(ResetSpeedAfterDelay(duration));
    }

    private IEnumerator ResetSpeedAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);

        moveSpeed = normalMoveSpeed;
        speedBoostCoroutine = null;
    }
}