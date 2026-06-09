using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class WhirlpoolPull : MonoBehaviour
{
    [Header("Pull Settings")]

    // Controls how strongly the whirlpool pulls the duck.
    [SerializeField] private float pullAcceleration = 16f;

    // Controls the weakest pull near the outside edge.
    [Range(0f, 1f)]
    [SerializeField] private float minimumStrength = 0.35f;

    // The large trigger collider around the whirlpool.
    private CircleCollider2D pullCollider;

    private void Awake()
    {
        // Find the collider attached to this PullArea.
        pullCollider = GetComponent<CircleCollider2D>();

        // Make sure it detects objects without physically blocking them.
        pullCollider.isTrigger = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Find the Rigidbody2D connected to the entering collider.
        Rigidbody2D playerBody = other.attachedRigidbody;

        if (playerBody == null)
        {
            return;
        }

        // Ignore anything that is not the duck.
        if (!playerBody.gameObject.CompareTag("Player"))
        {
            return;
        }

        // Find the duck's movement script.
        PlayerMovement playerMovement =
            playerBody.GetComponent<PlayerMovement>();

        if (playerMovement == null)
        {
            return;
        }

        // Find the true center of this collider.
        Vector2 centerPosition =
            transform.TransformPoint(pullCollider.offset);

        // Calculate the direction from the duck to the center.
        Vector2 directionToCenter =
            centerPosition - playerBody.position;

        float distance = directionToCenter.magnitude;

        // Avoid dividing by zero when exactly in the center.
        if (distance < 0.01f)
        {
            return;
        }

        // Read the collider's visible world-space radius.
        float pullRadius = pullCollider.bounds.extents.x;

        // Calculate how close the duck is to the center.
        // 0 means near the edge, and 1 means near the center.
        float closeness = Mathf.Clamp01(
            1f - distance / pullRadius
        );

        // Make the pulling effect stronger near the center.
        float strengthMultiplier = Mathf.Lerp(
            minimumStrength,
            1f,
            closeness
        );

        Vector2 pullAmount =
            directionToCenter.normalized *
            pullAcceleration *
            strengthMultiplier *
            Time.fixedDeltaTime;

        // Send the pull movement to PlayerMovement.
        playerMovement.AddWhirlpoolPull(pullAmount);
    }
}