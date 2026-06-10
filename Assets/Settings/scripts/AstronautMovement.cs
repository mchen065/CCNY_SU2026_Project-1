using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AstronautMovement : MonoBehaviour
{
    public enum AstronautState
    {
        Idle,
        Walking,
        Shifting
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float bottomLimit = -4f;
    [SerializeField] private float topLimit = 4f;

    [Header("Shifting")]
    [SerializeField] private Transform visual;
    [SerializeField] private float shiftedSize = 0.5f;

    private Rigidbody2D rb;
    private Animator animator;

    private float verticalInput;
    private Vector3 normalSize;
    private AstronautState currentState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        // No gravity because the astronaut is in space.
        rb.gravityScale = 0f;

        // Prevent left/right movement and rotation.
        rb.constraints =
            RigidbodyConstraints2D.FreezePositionX |
            RigidbodyConstraints2D.FreezeRotation;

        // Automatically use the animated child as the visual.
        if (visual == null && animator != null)
        {
            visual = animator.transform;
        }

        if (visual != null)
        {
            normalSize = visual.localScale;
        }

        ChangeState(AstronautState.Idle);
    }

    private void Update()
    {
        // W/S or Up/Down Arrow.
        verticalInput = Input.GetAxisRaw("Vertical");

        // Hold Space to become smaller.
        if (Input.GetKey(KeyCode.Space))
        {
            if (visual != null)
            {
                visual.localScale =
                    normalSize * shiftedSize;
            }

            ChangeState(AstronautState.Shifting);
        }
        else
        {
            if (visual != null)
            {
                visual.localScale = normalSize;
            }

            // Movement key held = Walking.
            if (Mathf.Abs(verticalInput) > 0.01f)
            {
                ChangeState(AstronautState.Walking);
            }
            else
            {
                // No key held = Idle.
                ChangeState(AstronautState.Idle);
            }
        }
    }

    private void FixedUpdate()
    {
        // Move only up and down.
        rb.linearVelocity = new Vector2(
            0f,
            verticalInput * moveSpeed
        );

        // Keep the astronaut inside the screen.
        Vector2 position = rb.position;

        position.y = Mathf.Clamp(
            position.y,
            bottomLimit,
            topLimit
        );

        rb.position = position;
    }

    private void ChangeState(AstronautState newState)
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

        // Animator states must use these exact names.
        switch (currentState)
        {
            case AstronautState.Idle:
                animator.Play("Idle");
                break;

            case AstronautState.Walking:
                animator.Play("Walking");
                break;

            case AstronautState.Shifting:
                animator.Play("Shifting");
                break;
        }
    }
}