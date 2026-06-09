using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class WhirlpoolCenter : MonoBehaviour
{
    // Number of points removed when the duck reaches the center.
    [SerializeField] private int scorePenalty = 10;

    // Prevents the same whirlpool from repeatedly removing points.
    private bool penaltyApplied;

    // The small trigger collider in the middle.
    private CircleCollider2D centerCollider;

    private void Awake()
    {
        centerCollider = GetComponent<CircleCollider2D>();

        // Make sure this collider detects the duck
        // without physically blocking it.
        centerCollider.isTrigger = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Stop if this whirlpool already gave its penalty.
        if (penaltyApplied)
        {
            return;
        }

        Rigidbody2D playerBody = other.attachedRigidbody;

        if (playerBody == null)
        {
            return;
        }

        // Only react to the duck.
        if (!playerBody.gameObject.CompareTag("Player"))
        {
            return;
        }

        penaltyApplied = true;

        // Remove ten points.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SubtractScore(scorePenalty);

            Debug.Log(
                $"Duck entered whirlpool center: -{scorePenalty} points"
            );
        }

        // Disable this trigger so it cannot apply another penalty.
        centerCollider.enabled = false;
    }
}