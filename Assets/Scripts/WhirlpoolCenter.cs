using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class WhirlpoolCenter : MonoBehaviour
{
    [SerializeField] private int scorePenalty = 10;

    private bool penaltyApplied;
    private CircleCollider2D centerCollider;

    private void Awake()
    {
        centerCollider = GetComponent<CircleCollider2D>();
        centerCollider.isTrigger = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (penaltyApplied)
        {
            return;
        }

        Rigidbody2D playerBody = other.attachedRigidbody;

        if (playerBody == null ||
            !playerBody.gameObject.CompareTag("Player"))
        {
            return;
        }

        penaltyApplied = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SubtractScore(scorePenalty);

            Debug.Log(
                $"Duck reached whirlpool center: -{scorePenalty} points"
            );
        }
        else
        {
            Debug.LogError(
                "No GameManager was found. Score could not be changed."
            );
        }

        // Prevent this particular whirlpool from penalizing repeatedly.
        centerCollider.enabled = false;
    }
}