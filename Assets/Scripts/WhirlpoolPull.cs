using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class WhirlpoolPull : MonoBehaviour
{
    [Header("Pull Strength")]
    [SerializeField] private float pullAcceleration = 16f;

    [Header("Pull Distance")]
    [SerializeField] private float pullRadius = 2.5f;

    private void Reset()
    {
        CircleCollider2D pullCollider =
            GetComponent<CircleCollider2D>();

        pullCollider.isTrigger = true;
        pullCollider.radius = pullRadius;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Rigidbody2D playerBody = other.attachedRigidbody;

        if (playerBody == null ||
            !playerBody.gameObject.CompareTag("Player"))
        {
            return;
        }

        PlayerMovement playerMovement =
            playerBody.GetComponent<PlayerMovement>();

        if (playerMovement == null)
        {
            return;
        }

        Vector2 directionToCenter =
            (Vector2)transform.position - playerBody.position;

        float distance = directionToCenter.magnitude;

        if (distance < 0.01f)
        {
            return;
        }

        float closeness = Mathf.Clamp01(
            1f - distance / pullRadius
        );

        // There is still some pull near the outside edge,
        // and the pull becomes stronger near the middle.
        float strengthMultiplier = Mathf.Lerp(
            0.35f,
            1f,
            closeness
        );

        Vector2 pullAmount =
            directionToCenter.normalized *
            pullAcceleration *
            strengthMultiplier *
            Time.fixedDeltaTime;

        playerMovement.AddWhirlpoolPull(pullAmount);
    }
}