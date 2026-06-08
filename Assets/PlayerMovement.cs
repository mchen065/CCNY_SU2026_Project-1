using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float verticalMultiplier = 0.4f;

    [Header("River Limits")]
    [SerializeField] private float riverBottomY = -34f;
    [SerializeField] private float riverTopY = -32f;

    [Header("Whirlpool Pull")]
    [SerializeField] private float maximumPullSpeed = 5f;
    [SerializeField] private float pullDecay = 5f;

    [Header("Sprite Direction")]
    [SerializeField] private bool spriteFacesLeftByDefault = true;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Vector2 movementInput;
    private Vector2 whirlpoolVelocity;

    private float normalMoveSpeed;
    private Coroutine speedBoostCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Save the duck's original speed.
        normalMoveSpeed = moveSpeed;
    }

    private void Update()
    {
        movementInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        UpdateFacingDirection();
    }

    private void FixedUpdate()
    {
        Vector2 normalMovement = new Vector2(
            movementInput.x * moveSpeed,
            movementInput.y * moveSpeed * verticalMultiplier
        );

        // Player movement and whirlpool pull work together.
        rb.linearVelocity = normalMovement + whirlpoolVelocity;

        // Gradually remove the pull after leaving the whirlpool.
        whirlpoolVelocity = Vector2.MoveTowards(
            whirlpoolVelocity,
            Vector2.zero,
            pullDecay * Time.fixedDeltaTime
        );

        // Keep the duck inside the river.
        Vector2 limitedPosition = rb.position;

        limitedPosition.y = Mathf.Clamp(
            limitedPosition.y,
            riverBottomY,
            riverTopY
        );

        rb.position = limitedPosition;
    }

    public void AddWhirlpoolPull(Vector2 pullAmount)
    {
        whirlpoolVelocity += pullAmount;

        whirlpoolVelocity = Vector2.ClampMagnitude(
            whirlpoolVelocity,
            maximumPullSpeed
        );
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if (speedBoostCoroutine != null)
        {
            StopCoroutine(speedBoostCoroutine);
        }

        moveSpeed = normalMoveSpeed * multiplier;

        speedBoostCoroutine = StartCoroutine(
            ResetSpeedAfterDelay(duration)
        );
    }

    private IEnumerator ResetSpeedAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);

        moveSpeed = normalMoveSpeed;
        speedBoostCoroutine = null;
    }

    private void UpdateFacingDirection()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (movementInput.x < -0.01f)
        {
            // Face left.
            spriteRenderer.flipX = !spriteFacesLeftByDefault;
        }
        else if (movementInput.x > 0.01f)
        {
            // Face right.
            spriteRenderer.flipX = spriteFacesLeftByDefault;
        }
    }
}