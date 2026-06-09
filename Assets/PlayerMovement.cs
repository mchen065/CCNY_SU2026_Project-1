using System.Collections;
using UnityEngine;

// Controls the duck's normal movement, sprite direction,
// river boundaries, temporary speed boosts, and whirlpool pulling.
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]

    // The duck's normal horizontal movement speed.
    // You can change this value in the Inspector.
    [SerializeField] private float moveSpeed = 7f;

    // Controls how quickly the duck moves vertically.
    // 0.4 means vertical movement is 40% of horizontal movement.
    [SerializeField] private float verticalMultiplier = 0.4f;


    [Header("River Limits")]

    // The lowest Y position the duck is allowed to reach.
    [SerializeField] private float riverBottomY = -34f;

    // The highest Y position the duck is allowed to reach.
    [SerializeField] private float riverTopY = -32f;


    [Header("Whirlpool Pull")]

    // Limits the maximum speed caused by a whirlpool.
    // This prevents the duck from being pulled too quickly.
    [SerializeField] private float maximumPullSpeed = 5f;

    // Controls how quickly the whirlpool pull disappears
    // after the duck leaves the whirlpool.
    [SerializeField] private float pullDecay = 5f;


    [Header("Sprite Direction")]

    // Keep this checked if the original duck sprite faces left.
    // Uncheck it if the original sprite faces right.
    [SerializeField] private bool spriteFacesLeftByDefault = true;


    // Reference to the Rigidbody2D attached to the duck.
    // This is used to move the duck with Unity physics.
    private Rigidbody2D rb;

    // Reference to the SpriteRenderer attached to the duck.
    // This is used to flip the duck left and right.
    private SpriteRenderer spriteRenderer;


    // Stores the player's horizontal and vertical input.
    private Vector2 movementInput;

    // Stores the extra movement created by whirlpools.
    private Vector2 whirlpoolVelocity;


    // Saves the duck's original movement speed.
    // This allows the normal speed to return after a speed boost.
    private float normalMoveSpeed;

    // Stores the currently active speed-boost coroutine.
    private Coroutine speedBoostCoroutine;


    private void Awake()
    {
        // Find the Rigidbody2D attached to this GameObject.
        rb = GetComponent<Rigidbody2D>();

        // Find the SpriteRenderer attached to this GameObject.
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Save the original movement speed.
        normalMoveSpeed = moveSpeed;
    }


    private void Update()
    {
        // Read left/right input:
        // A or Left Arrow = negative value.
        // D or Right Arrow = positive value.
        float horizontalInput =
            Input.GetAxisRaw("Horizontal");

        // Read up/down input:
        // W or Up Arrow = positive value.
        // S or Down Arrow = negative value.
        float verticalInput =
            Input.GetAxisRaw("Vertical");

        // Store horizontal and vertical input together.
        movementInput = new Vector2(
            horizontalInput,
            verticalInput
        ).normalized;

        // Normalizing prevents diagonal movement
        // from being faster than straight movement.

        // Make the duck face left or right.
        UpdateFacingDirection();
    }


    private void FixedUpdate()
    {
        // Calculate the duck's normal movement.
        Vector2 normalMovement = new Vector2(
            movementInput.x * moveSpeed,

            // Vertical movement is multiplied by verticalMultiplier
            // so the duck does not move too far outside the river.
            movementInput.y * moveSpeed * verticalMultiplier
        );

        // Combine player-controlled movement
        // with movement caused by a whirlpool.
        rb.linearVelocity =
            normalMovement + whirlpoolVelocity;


        // Slowly reduce the whirlpool pull toward zero.
        // This makes the pull disappear smoothly
        // after the duck leaves the whirlpool.
        whirlpoolVelocity = Vector2.MoveTowards(
            whirlpoolVelocity,
            Vector2.zero,
            pullDecay * Time.fixedDeltaTime
        );


        // Read the duck's current Rigidbody2D position.
        Vector2 limitedPosition = rb.position;

        // Keep the duck between the bottom and top
        // boundaries of the river.
        limitedPosition.y = Mathf.Clamp(
            limitedPosition.y,
            riverBottomY,
            riverTopY
        );

        // Apply the restricted position.
        rb.position = limitedPosition;
    }


    // The WhirlpoolPull script calls this method
    // while the duck is inside a whirlpool's pulling area.
    public void AddWhirlpoolPull(Vector2 pullAmount)
    {
        // Add the whirlpool's pull to the current pull velocity.
        whirlpoolVelocity += pullAmount;

        // Prevent the pulling velocity from becoming too strong.
        whirlpoolVelocity = Vector2.ClampMagnitude(
            whirlpoolVelocity,
            maximumPullSpeed
        );
    }


    // The special speed coin calls this method.
    //
    // multiplier:
    // 2 means double speed.
    //
    // duration:
    // Number of seconds the speed boost lasts.
    public void ApplySpeedBoost(
        float multiplier,
        float duration
    )
    {
        // If another speed boost is already active,
        // stop its timer before starting the new one.
        if (speedBoostCoroutine != null)
        {
            StopCoroutine(speedBoostCoroutine);
        }

        // Increase the speed using the original speed.
        moveSpeed = normalMoveSpeed * multiplier;

        // Begin the timer that will restore normal speed.
        speedBoostCoroutine = StartCoroutine(
            ResetSpeedAfterDelay(duration)
        );
    }


    // Coroutine used to wait before restoring normal speed.
    private IEnumerator ResetSpeedAfterDelay(float duration)
    {
        // Wait for the chosen boost duration.
        yield return new WaitForSeconds(duration);

        // Return the duck to its original speed.
        moveSpeed = normalMoveSpeed;

        // Clear the coroutine reference because
        // the speed boost has finished.
        speedBoostCoroutine = null;
    }


    private void UpdateFacingDirection()
    {
        // Stop if the duck does not have a SpriteRenderer.
        if (spriteRenderer == null)
        {
            return;
        }

        // The player is moving left.
        if (movementInput.x < -0.01f)
        {
            // If the original sprite already faces left,
            // it does not need to be flipped.
            spriteRenderer.flipX =
                !spriteFacesLeftByDefault;
        }

        // The player is moving right.
        else if (movementInput.x > 0.01f)
        {
            // If the original sprite faces left,
            // flip it so it faces right.
            spriteRenderer.flipX =
                spriteFacesLeftByDefault;
        }

        // When moving only up or down,
        // the duck keeps its previous facing direction.
    }
}